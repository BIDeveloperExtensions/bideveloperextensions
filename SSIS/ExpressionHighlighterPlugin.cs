namespace BIDSHelper.SSIS
{
    using EnvDTE;
    using EnvDTE80;
    using System.Windows.Forms;
    using System.ComponentModel.Design;
    using Microsoft.DataWarehouse.Design;
    using System;
    using Microsoft.SqlServer.Dts.Runtime;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.Win32;
    using Microsoft.SqlServer.IntegrationServices.Designer.Model;
    using Microsoft.SqlServer.IntegrationServices.Designer.ConnectionManagers;
    using Core;

    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class ExpressionHighlighterPlugin : BIDSHelperWindowActivatedPluginBase
    {
        private static System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private static string STATUS_BAR_PROGRESS_CAPTION = "BIDS Helper: Highlighting Expressions and Configurations";
        private static Type TYPE_DTS_SERIALIZATION = GetPrivateType(typeof(Microsoft.DataTransformationServices.Design.ColumnInfo), "Microsoft.DataTransformationServices.Design.Serialization.DtsSerialization");
        private static int MAX_SYNCHRONOUS_HIGHLIGHTING_TIME_SECONDS = 0; //TODO
        private static int MAX_SECONDS_BUILDING_TO_DOS_BEFORE_OFFER_DISABLE = 10;
        private System.ComponentModel.BackgroundWorker workerToDos = new System.ComponentModel.BackgroundWorker();
        private AutoResetEvent workerThreadEvent = new AutoResetEvent(false);
        private bool bWorkerThreadDoneWithWork = false;
        private Dictionary<object, HighlightingToDo> highlightingToDos = new Dictionary<object, HighlightingToDo>();
        private Dictionary<EditorWindow, bool> disableHighlighting = new Dictionary<EditorWindow, bool>();
        private Dictionary<EditorWindow, DateTime> mostRecentDDSRefresh = new Dictionary<EditorWindow, DateTime>();
        private DateTime mostRecentComponentEvent = DateTime.MinValue;

        public ExpressionHighlighterPlugin(BIDSHelperPackage package)
            : base(package)
        {
            this.package.Log.Debug("ExpressionHighlighterPlugin constructing");
            workerToDos.WorkerReportsProgress = false;
            workerToDos.WorkerSupportsCancellation = true;
            workerToDos.DoWork += new System.ComponentModel.DoWorkEventHandler(workerToDos_DoWork);
        }

        private static string REGISTRY_EXPRESSION_COLOR_SETTING_NAME = "ExpressionColor";
        private static string REGISTRY_CONFIGURATION_COLOR_SETTING_NAME = "ConfigurationColor";

        public static System.Drawing.Color ExpressionColorDefault = System.Drawing.Color.Magenta;
        public static System.Drawing.Color ExpressionColor
        {
            get
            {
                int iColor = ExpressionColorDefault.ToArgb();
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(BIDSHelperPackage.PluginRegistryPath(typeof(ExpressionHighlighterPlugin)));
                
                if (rk != null)
                {
                    iColor = (int)rk.GetValue(REGISTRY_EXPRESSION_COLOR_SETTING_NAME, iColor);
                    rk.Close();
                }
                return System.Drawing.Color.FromArgb(iColor);
            }
            set
            {
                string regPath = BIDSHelperPackage.PluginRegistryPath(typeof(ExpressionHighlighterPlugin));
                RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(regPath, true);
                if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(regPath);
                settingKey.SetValue(REGISTRY_EXPRESSION_COLOR_SETTING_NAME, value.ToArgb(), RegistryValueKind.DWord);
                settingKey.Close();
                HighlightingToDo.expressionColor = value;
            }
        }

        public static System.Drawing.Color ConfigurationColorDefault = System.Drawing.Color.FromArgb(17, 200, 255);
        public static System.Drawing.Color ConfigurationColor
        {
            get
            {
                int iColor = ConfigurationColorDefault.ToArgb();
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(BIDSHelperPackage.PluginRegistryPath(typeof(ExpressionHighlighterPlugin)));
                if (rk != null)
                {
                    iColor = (int)rk.GetValue(REGISTRY_CONFIGURATION_COLOR_SETTING_NAME, iColor);
                    rk.Close();
                }
                return System.Drawing.Color.FromArgb(iColor);
            }
            set
            {
                string regPath = BIDSHelperPackage.PluginRegistryPath(typeof(ExpressionHighlighterPlugin));
                RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(regPath, true);
                if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(regPath);
                settingKey.SetValue(REGISTRY_CONFIGURATION_COLOR_SETTING_NAME, value.ToArgb(), RegistryValueKind.DWord);
                settingKey.Close();
                HighlightingToDo.configurationColor = value;
            }
        }

        #region Window Events Overrides and Disable Event
        public override bool ShouldHookWindowCreated { get { return false; } }
        public override bool ShouldHookWindowClosing { get { return true; } }
        public override bool ShouldHookWindowActivated { get { return true; } }

        public override void OnWindowActivated(Window GotFocus, Window lostFocus)
        {
            try
            {
                package.Log.Debug("ExpressionHighlighterPlugin OnWindowActivated fired");
                System.Diagnostics.Debug.WriteLine("OnWindowActivated: GotFocus=" + (GotFocus == null ? "Null" : "NotNull") + "  LostFocus=" + (lostFocus == null ? "Null" : "NotNull"));
                package.Log.Debug("OnWindowActivated: GotFocus=" + (GotFocus == null ? "Null" : "NotNull") + "  LostFocus=" + (lostFocus == null ? "Null" : "NotNull"));
                BuildToDos(GotFocus, null);
            }
            catch (Exception ex) {
                package.Log.Exception("ExpressionHighlighterPlugin.OnWindowOverride", ex);
            }
        }
        public override void OnWindowClosing(Window ClosingWindow)
        {
            try
            {
                this.package.Log.Debug("ExpressionHighlighterPlugin.OnWindowClosing");
                if (ClosingWindow == null) return;
                IDesignerHost designer = ClosingWindow.Object as IDesignerHost;
                if (designer == null) return;
//#if !DENALI && !SQL2014 //apparently ProjectItem is null in Denali and above
//                ProjectItem pi = ClosingWindow.ProjectItem;
//                if (pi == null || !(pi.Name.ToLower().EndsWith(".dtsx"))) return;
//#endif

                EditorWindow win = designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator)) as EditorWindow;
                if (win == null) return;
                Package package = win.PropertiesLinkComponent as Package;
                if (package == null) return;

                //clear cache
                HighlightingToDo.ClearCache(package);
                System.Diagnostics.Debug.WriteLine("done clearing cache for closed package");

                //clear todos
                List<object> todosToRemove = new List<object>();
                foreach (object key in highlightingToDos.Keys)
                {
                    if (highlightingToDos[key].package == package)
                        todosToRemove.Add(key);
                }
                lock (highlightingToDos)
                {
                    foreach (object key in todosToRemove)
                        highlightingToDos.Remove(key);
                }
                System.Diagnostics.Debug.WriteLine("done removing todos for closed package");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                this.package.Log.Exception("ExpressionHighlighterPlugin.OnWindowClosing", ex);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (workerToDos.IsBusy)
                workerToDos.CancelAsync();

            //don't worry about unhooking events as all of them check whether it's enabled before doing anything expensive

            HighlightingToDo.ClearCache();
        }
        #endregion

        #region Build To-Dos
        private void BuildToDos(Window GotFocus, DtsObject oIncrementalObject)
        {
            BuildToDos(GotFocus, oIncrementalObject, null);
        }

        internal void BuildToDos(Window GotFocus, DtsObject oIncrementalObject, int? iIncrementalTransformID)
        {
            try
            {
                DateTime dtToDoBuildingStartTime = DateTime.Now;
                DateTime dtSynchronousHighlightingCutoff = DateTime.Now.AddSeconds(MAX_SYNCHRONOUS_HIGHLIGHTING_TIME_SECONDS);
                if (!this.Enabled) return;
                if (ExpressionListPlugin.shouldSkipExpressionHighlighting) return;

                DtsContainer oIncrementalContainer = oIncrementalObject as DtsContainer;
                ConnectionManager oIncrementalConnectionManager = oIncrementalObject as ConnectionManager;
                bool bIncremental = (oIncrementalContainer != null);
                bool bRescan = false;
                if (oIncrementalContainer is Package)
                {
                    bIncremental = false;
                    bRescan = true;
                }

                if (GotFocus == null) return;
                if (GotFocus.DTE.Mode == vsIDEMode.vsIDEModeDebug) return;
                IDesignerHost designer = GotFocus.Object as IDesignerHost;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if (pi == null || !(pi.Name.ToLower().EndsWith(".dtsx"))) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                if (win == null) // Happens with bad packages and flipping between XML and code, and on save. Not 100% clear, but caused stability issues
                    return;

                Package package = (Package)win.PropertiesLinkComponent;

                //check whether we should abort because highlighting has been disabled for this window
                if (disableHighlighting.ContainsKey(win) && disableHighlighting[win]) return;


                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                //NEW REQUEUE CODE DESIGNED TO POSTPONE WORK DONE DURING A PASTE OPERATION UNTIL AFTER IS HAS COMPLETED
                bool bRequeue = false;

                try
                {
                    IClipboardService clipboardService = (IClipboardService)package.Site.GetService(typeof(IClipboardService));
                    if (clipboardService.IsPasteActive)
                        bRequeue = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Problem checking if there is a paste underway: " + ex.Message + "\r\n" + ex.StackTrace);
                    this.package.Log.Exception("Problem checking if there is a paste underway", ex);
                }

                if (bRequeue)
                {
                    RequeueToDo todo = new RequeueToDo();
                    lock (highlightingToDos)
                    {
                        highlightingToDos.Add(todo, todo);
                        todo.editorWin = win;
                        todo.GotFocus = GotFocus;
                        todo.oIncrementalObject = oIncrementalObject;
                        todo.iIncrementalTransformID = iIncrementalTransformID;
                        todo.package = package;
                        todo.plugin = this;
                        todo.BackgroundOnly = false; //run all the requeues first as it will avoid redoing lots of work
                        todo.Rescan = false;
                    }
                    System.Diagnostics.Debug.WriteLine("requeued as todo");
                    StartToDosThread(dtSynchronousHighlightingCutoff);
                    return;
                }
                //END OF NEW REQUEUE CODE
                ///////////////////////////////////////////////////////////////////////////////////////////////////////


                if (win.SetTagExpressionHighlighterPlugin())
                {
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                    IComponentChangeService configurationsChangeService = (IComponentChangeService)designer;
                    configurationsChangeService.ComponentChanged += new ComponentChangedEventHandler(configurationsChangeService_ComponentChanged);
                    configurationsChangeService.ComponentAdded += new ComponentEventHandler(configurationsChangeService_ComponentAdded);
                    bRescan = true;
                }

                for (int iViewIndex = 0; iViewIndex <= (int)SSISHelpers.SsisDesignerTabIndex.EventHandlers; iViewIndex++)
                {
                    EditorWindow.EditorView view = win.Views[iViewIndex];
                    Control viewControl = (Control)view.GetType().InvokeMember("ViewControl", getflags, null, view, null);
                    if (viewControl == null) continue;

                    if (iViewIndex == (int)SSISHelpers.SsisDesignerTabIndex.ControlFlow)
                    {
                        //it's now a Microsoft.DataTransformationServices.Design.Controls.DtsConnectionsListView object which doesn't inherit from ListView and which is internal
                        Control lvwConnMgrs = (Control)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                        if (lvwConnMgrs != null)
                        {
                            BuildConnectionManagerToDos(package, lvwConnMgrs, bIncremental, bRescan, oIncrementalConnectionManager);
                        }

                        Microsoft.SqlServer.IntegrationServices.Designer.Model.ControlFlowGraphModelElement ctlFlowModel = (Microsoft.SqlServer.IntegrationServices.Designer.Model.ControlFlowGraphModelElement)viewControl.GetType().InvokeMember("GraphModel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, viewControl, null);
                        foreach (Microsoft.SqlServer.Graph.Model.ModelElement task in ctlFlowModel)
                        {
                            Executable executable = null;
                            Microsoft.SqlServer.Graph.Extended.IModelElementEx taskModelEl = task as Microsoft.SqlServer.Graph.Extended.IModelElementEx;
                            if (taskModelEl != null)
                            {
                                executable = taskModelEl.LogicalObject as Executable;

                                bool bRescanThisTask = bRescan;
                                if (oIncrementalContainer == executable)
                                {
                                    bRescanThisTask = true;
                                }
                                if (executable != null)
                                {
                                    TaskHighlightingToDo todo;
                                    lock (highlightingToDos)
                                    {
                                        this.package.Log.Debug("ExpressionHighlighter scanning task: " + task.Name);
                                        if (highlightingToDos.ContainsKey(executable))
                                            todo = (TaskHighlightingToDo)highlightingToDos[executable];
                                        else
                                            highlightingToDos.Add(executable, todo = new TaskHighlightingToDo());
                                        todo.package = package;
                                        todo.executable = executable;
                                        todo.controlFlowDesigner = designer;
                                        todo.controlFlowTaskModelElement = task;
                                        todo.BackgroundOnly = !(!todo.BackgroundOnly || (view.Selected && viewControl.Visible));
                                        todo.Rescan = bRescanThisTask;
                                    }
                                }
                            }
                        }
                    }
                    else if (iViewIndex == (int)SSISHelpers.SsisDesignerTabIndex.EventHandlers)
                    {
                        Microsoft.DataTransformationServices.Design.Controls.EventHandlersComboBox eventHandlersCombo = ((Microsoft.DataTransformationServices.Design.Controls.EventHandlersComboBox)(viewControl.Controls["panel1"].Controls["panel2"].Controls["comboBoxEventHandler"]));
                        if (eventHandlersCombo.Tag == null)
                        {
                            eventHandlersCombo.SelectedIndexChanged += new EventHandler(comboBox_SelectedIndexChanged);
                            eventHandlersCombo.Tag = true;
                            //don't need to monitor the Base Control (leftmost) combo box because changing it will trigger a change to the event handler combo: ((Microsoft.DataWarehouse.Controls.BaseControlComboBox)(viewControl.Controls["panel1"].Controls["panel2"].Controls["Custom ComboBox"]))
                        }

                        //it's now a Microsoft.DataTransformationServices.Design.Controls.DtsConnectionsListView object which doesn't inherit from ListView and which is internal
                        Control lvwConnMgrs = (Control)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                        if (lvwConnMgrs != null)
                        {
                            BuildConnectionManagerToDos(package, lvwConnMgrs, bIncremental, bRescan, oIncrementalConnectionManager);
                        }

                        foreach (Control c in viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls)
                        {
                            if (!(c is Microsoft.DataTransformationServices.Design.EventHandlerElementHost)) continue;
                            Microsoft.SqlServer.IntegrationServices.Designer.Model.ControlFlowGraphModelElement ctlFlowModel = (Microsoft.SqlServer.IntegrationServices.Designer.Model.ControlFlowGraphModelElement)c.GetType().InvokeMember("GraphModel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, c, null);
                            foreach (Microsoft.SqlServer.Graph.Model.ModelElement task in ctlFlowModel)
                            {
                                Executable executable = null;
                                Microsoft.SqlServer.Graph.Extended.IModelElementEx taskModelEl = task as Microsoft.SqlServer.Graph.Extended.IModelElementEx;
                                if (taskModelEl != null)
                                {
                                    executable = taskModelEl.LogicalObject as Executable;

                                    bool bRescanThisTask = bRescan;
                                    if (oIncrementalContainer == executable)
                                    {
                                        bRescanThisTask = true;
                                    }
                                    if (executable != null)
                                    {
                                        TaskHighlightingToDo todo;
                                        lock (highlightingToDos)
                                        {
                                            this.package.Log.Debug("ExpressionHightlighter scanning task: " + task.Name);
                                            if (highlightingToDos.ContainsKey(executable))
                                                todo = (TaskHighlightingToDo)highlightingToDos[executable];
                                            else
                                                highlightingToDos.Add(executable, todo = new TaskHighlightingToDo());
                                            todo.package = package;
                                            todo.executable = executable;
                                            todo.controlFlowDesigner = designer;
                                            todo.controlFlowTaskModelElement = task;
                                            todo.BackgroundOnly = !(!todo.BackgroundOnly || (view.Selected && c.Visible));
                                            todo.Rescan = bRescanThisTask;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (iViewIndex == (int)SSISHelpers.SsisDesignerTabIndex.DataFlow)
                    {
                        //it's now a Microsoft.DataTransformationServices.Design.Controls.DtsConnectionsListView object which doesn't inherit from ListView and which is internal
                        Control lvwConnMgrs = (Control)viewControl.Controls["dataFlowsTrayTabControl"].Controls["dataFlowConnectionsTabPage"].Controls["dataFlowConnectionsListView"];
                        if (lvwConnMgrs != null)
                        {
                            BuildConnectionManagerToDos(package, lvwConnMgrs, bIncremental, bRescan, oIncrementalConnectionManager);
                        }

                        Microsoft.DataTransformationServices.Design.Controls.PipelineComboBox pipelineComboBox = (Microsoft.DataTransformationServices.Design.Controls.PipelineComboBox)(viewControl.Controls["panel1"].Controls["tableLayoutPanel"].Controls["pipelineComboBox"]);
                        if (pipelineComboBox.Tag == null)
                        {
                            pipelineComboBox.SelectedIndexChanged += new EventHandler(comboBox_SelectedIndexChanged);
                            pipelineComboBox.Tag = true;
                        }

                        foreach (Control c in viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls)
                        {
                            if (c.GetType().FullName != "Microsoft.DataTransformationServices.Design.PipelineTaskView") continue;
                            object pipelineDesigner = c.GetType().InvokeMember("PipelineTaskDesigner", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, c, null);
                            if (pipelineDesigner == null) continue;
                            Microsoft.SqlServer.IntegrationServices.Designer.Model.DataFlowGraphModelElement dataFlowModel = (Microsoft.SqlServer.IntegrationServices.Designer.Model.DataFlowGraphModelElement)pipelineDesigner.GetType().InvokeMember("DataFlowGraphModel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, pipelineDesigner, null);
                            List<string> transforms = new List<string>();

                            Executable executable = null;
                            foreach (Microsoft.SqlServer.Graph.Model.ModelElement transform in dataFlowModel)
                            {
                                Microsoft.SqlServer.Graph.Extended.IModelElementEx transformModelEl = transform as Microsoft.SqlServer.Graph.Extended.IModelElementEx;
                                if (transformModelEl != null)
                                {
                                    Microsoft.DataTransformationServices.PipelineComponentMetadata metadata = transformModelEl.LogicalObject as Microsoft.DataTransformationServices.PipelineComponentMetadata;
                                    if (metadata == null) continue;
                                    executable = metadata.PipelineTask as Executable;

                                    int id = metadata.ID;
                                    string sName = metadata.Name;
                                    string sObjectGuid = pi.Name + "/" + metadata.PipelineTask.ID + "/components/" + id; //this is the todo key... (trying to use the IDTSComponentMetaDataXX as the key caused problems with COM object references and threading)
                                    transforms.Add(sObjectGuid);

                                    TransformHighlightingToDo transformTodo;
                                    lock (highlightingToDos)
                                    {
                                        this.package.Log.Debug("ExpressionHighlighter scanning transform: " + sName);
                                        if (highlightingToDos.ContainsKey(sObjectGuid))
                                            transformTodo = (TransformHighlightingToDo)highlightingToDos[sObjectGuid];
                                        else
                                            highlightingToDos.Add(sObjectGuid, transformTodo = new TransformHighlightingToDo());
                                        transformTodo.package = package;
                                        transformTodo.dataFlowTransformModelElement = transform;
                                        transformTodo.taskHost = dataFlowModel.PipelineTask;
                                        transformTodo.transformName = sName;
                                        transformTodo.transformUniqueID = sObjectGuid;
                                        transformTodo.BackgroundOnly = !(!transformTodo.BackgroundOnly || (view.Selected && c.Visible));
                                    }
                                }
                            }

                            if (executable == null) continue; //are going to get an error on ContainsKey anyway

                            TaskHighlightingToDo todo;
                            lock (highlightingToDos)
                            {
                                if (highlightingToDos.ContainsKey(executable))
                                    todo = (TaskHighlightingToDo)highlightingToDos[executable];
                                else
                                    highlightingToDos.Add(executable, todo = new TaskHighlightingToDo());
                                todo.package = package;
                                todo.executable = executable;
                                todo.BackgroundOnly = !(!todo.BackgroundOnly || (view.Selected && c.Visible));
                                if (todo.transforms == null)
                                {
                                    todo.transforms = transforms;
                                }
                                else
                                {
                                    lock (todo.transforms)
                                    {
                                        todo.transforms = transforms;
                                    }
                                }
                            }
                        }
                    }
                }

                //if taking a long time, offer to disable for this package
                if (dtToDoBuildingStartTime.AddSeconds(MAX_SECONDS_BUILDING_TO_DOS_BEFORE_OFFER_DISABLE) < DateTime.Now && !disableHighlighting.ContainsKey(win))
                {
                    //it took more than 10 seconds to build the to do's list... offer that we disable highlighting for this package
                    DialogResult result = MessageBox.Show("BIDS Helper Expression and Configuration Highlighting is taking a very long time to complete.\r\nThis can occur sometimes with complex packages.\r\n\r\nWould you like to disable highlighting on this package until you reopen it?\r\n\r\n(You may also completely disable the feature on all packages via Tools... Options... BIDS Helper.)", "BIDS Helper Expression and Configuration Highlighter Performance", MessageBoxButtons.YesNo);
                    disableHighlighting.Add(win, (result == DialogResult.Yes));
                    if (result == DialogResult.Yes) return;
                }


                //only scan configurations if...
                //1. it's a full load (not incremental)
                //2. it's a full rescan
                //even in project deployment model in Denali you can continue using configurations, so go ahead and run this code as it won't be expensive unless there are configurations
                if (!bIncremental && bRescan)
                    HighlightingToDo.CachePackageConfigurations(package, pi);

                StartToDosThread(dtSynchronousHighlightingCutoff);

            }
            catch (Exception ex)
            {
                this.package.Log.Exception("ExpressionHighlightingPlugin.BuildToDos", ex);
                System.Diagnostics.Debug.WriteLine(ex.Message + " " + ex.StackTrace);
            }
        }

        private void BuildConnectionManagerToDos(Package package, Control lvwConnMgrs, bool bIncremental, bool bRescan, ConnectionManager oIncrementalConnectionManager)
        {
            ConnectionManagerUserControl cmControl = (ConnectionManagerUserControl)lvwConnMgrs.GetType().InvokeMember("m_connectionManagerUserControl", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, lvwConnMgrs, null);
            ConnectionManagersModelElement conns = (ConnectionManagersModelElement)lvwConnMgrs.GetType().InvokeMember("m_connectionsElement", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, lvwConnMgrs, null);

            if (cmControl != null && conns != null)
            {
                foreach (ConnectionManagerModelElement lviConn in conns)
                {
                    System.Windows.FrameworkElement fe = (System.Windows.FrameworkElement)cmControl.GetViewFromViewModel(lviConn);
                    if (fe == null) continue; //guess the framework element isn't loaded yet?
                    ConnectionManager conn = lviConn.ConnectionManager;
                    if (conn == null) continue;
                    ConnectionManagerHighlightingToDo todo;
                    lock (highlightingToDos)
                    {
                        this.package.Log.Debug("ExpressionHighlighter Scanning Connection: " + conn.Name);
                        if (highlightingToDos.ContainsKey(conn))
                            todo = (ConnectionManagerHighlightingToDo)highlightingToDos[conn];
                        else
                            highlightingToDos.Add(conn, todo = new ConnectionManagerHighlightingToDo());
                        todo.package = package;
                        todo.connection = conn;
                        todo.listConnectionLVIs.Add((System.Windows.FrameworkElement)cmControl.GetViewFromViewModel(lviConn));
                        todo.BackgroundOnly = false;
                        todo.Rescan = (conn == oIncrementalConnectionManager) || bRescan;
                    }
                }
            }
        }
        #endregion

        #region Worker Thread
        private void StartToDosThread(DateTime dtSynchronousHighlightingCutoff)
        {
            this.package.Log.Debug("ExpressionHighlighter StartToDosThread enter");
            while (bWorkerThreadDoneWithWork && workerToDos.IsBusy)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(100); //wait for the worker thread to actually finish
            }

            //start worker thread if it's not already started
            lock (workerThreadEvent)
            {
                workerThreadEvent.Reset();
                if (!workerToDos.IsBusy)
                    workerToDos.RunWorkerAsync();
                //workerToDos_DoWork(null, null); //just for debugging all in one thread
            }

            //wait for the remaining portion of the allowed foreground time
            TimeSpan tsWait = dtSynchronousHighlightingCutoff.Subtract(DateTime.Now);
            if (tsWait > TimeSpan.Zero)
            {
                System.Diagnostics.Debug.WriteLine("running highlighting synchronously for " + (int)tsWait.TotalMilliseconds + " ms");
                workerThreadEvent.WaitOne(tsWait, false);
                if (workerToDos.IsBusy)
                    System.Diagnostics.Debug.WriteLine("running highlighting asynchronously");
            }
        }

        void workerToDos_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bWorkerThreadDoneWithWork = false;
            Dictionary<Package, List<string>> warnings = new Dictionary<Package, List<string>>();
            bool bWorkerThreadEventSet = false;
            try
            {
                //TODO: consider not showing the progress bar until it takes more than 1 second to complete???
                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationSync);
                ApplicationObject.StatusBar.Progress(true, STATUS_BAR_PROGRESS_CAPTION, 0, 100);

                int iToDosExecuted = 0;
                while (highlightingToDos.Count > 0 && !workerToDos.CancellationPending)
                {
                    HighlightingToDo todo = null;
                    object todoKey = null;
                    int iForegroundTasks = 0;

                    //find a foreground task as they take priority
                    lock (highlightingToDos)
                    {
                        foreach (HighlightingToDo v in highlightingToDos.Values)
                        {
                            if (!v.BackgroundOnly) iForegroundTasks++;
                        }
                        foreach (object k in highlightingToDos.Keys)
                        {
                            todoKey = k;
                            todo = highlightingToDos[todoKey];
                            if (todo is RequeueToDo) break;
                        }
                        if (todo == null)
                        {
                            foreach (object k in highlightingToDos.Keys)
                            {
                                todoKey = k;
                                todo = highlightingToDos[todoKey];
                                if (!todo.BackgroundOnly) break;
                            }
                        }
                    }

                    //if there are no foreground todos, the last todo from the loop above is executed
                    if (todo.BackgroundOnly && !bWorkerThreadEventSet)
                    {
                        System.Diagnostics.Debug.WriteLine("finished foreground highlighting tasks");
                        this.package.Log.Debug("finished foreground highlighting tasks");
                        workerThreadEvent.Set();
                        bWorkerThreadEventSet = true;
                    }
                    else if (!todo.BackgroundOnly)
                    {
                        bWorkerThreadEventSet = false;
                    }

                    if (!warnings.ContainsKey(todo.package))
                        warnings.Add(todo.package, new List<string>());

                    try
                    {
                        todo.Highlight();
                    }
                    catch (Exception ex)
                    {
                        warnings[todo.package].Add("BIDS Helper had problems highlighting expressions and configurations: " + ex.Message);
                    }
                    iToDosExecuted++;

                    lock (highlightingToDos)
                    {
                        highlightingToDos.Remove(todoKey);

                        if (bWorkerThreadEventSet)
                        {
                            System.Diagnostics.Debug.WriteLine("100% complete with foreground tasks");
                            this.package.Log.Debug("100% complete with foreground tasks");
                            ApplicationObject.StatusBar.Progress(false, STATUS_BAR_PROGRESS_CAPTION, 100, 100);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine((int)(((double)iToDosExecuted / (iToDosExecuted + iForegroundTasks)) * 100) + " percent complete");
                            ApplicationObject.StatusBar.Progress(true, STATUS_BAR_PROGRESS_CAPTION, ((int)((double)iToDosExecuted / (iToDosExecuted + iForegroundTasks) * 100)), 100);
                        }
                    }
                }
                VariablesWindowPlugin.RefreshHighlights();
                bWorkerThreadDoneWithWork = true;
                if (highlightingToDos.Count > 0 && workerToDos.CancellationPending)
                    System.Diagnostics.Debug.WriteLine("cancelled background worker successfully!!!!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error inside expression highlighter DoWork: " + ex.Message + " " + ex.StackTrace);
                this.package.Log.Exception("ExpressionHighlighterPlugin.workerToDos_DoWork",ex);
            }
            finally
            {
                bWorkerThreadDoneWithWork = true;
                try
                {
                    ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationSync);
                    ApplicationObject.StatusBar.Progress(false, STATUS_BAR_PROGRESS_CAPTION, 100, 100);
                }
                catch { }
                try
                {
                    foreach (Package p in warnings.Keys)
                    {
                        ITaskListService taskListService = p.Site.GetService(typeof(ITaskListService)) as ITaskListService;
                        List<string> templist = new List<string>();
                        if (HighlightingToDo.cacheConfigurationWarnings.ContainsKey(p))
                            templist.AddRange(HighlightingToDo.cacheConfigurationWarnings[p]);
                        templist.AddRange(warnings[p]);
                        AddWarningsToVSErrorList(taskListService, p.Name + ".dtsx", templist.ToArray());
                    }
                    System.Diagnostics.Debug.WriteLine("finished displaying warnings");
                }
                catch { }

                //at this point trimming the cache doesn't look necessary as it only takes up 10% extra memory in BIDS
                //try
                //{
                //    HighlightingToDo.TrimCache();
                //}
                //catch { }

                try
                {
                    System.Diagnostics.Debug.WriteLine("finished highlighting");
                    //even if (!bWorkerThreadEventSet), go ahead and set the worker thread in case another incremental run began in the middle of the previous run
                    workerThreadEvent.Set();
                    System.Diagnostics.Debug.WriteLine("finished setting thread event");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("problem setting thread event: " + ex.Message + " " + ex.StackTrace);
                }
            }
        }
        #endregion

        #region Event Handlers For Incremental Highlighting
//#if !DENALI && !SQL2014
//        private void lvwConnMgrs_DrawItem(object sender, DrawListViewItemEventArgs e)
//        {
//            try
//            {
//                if (this.Enabled)
//                    ConnectionManagerHighlightingToDo.HighlightConnectionManagerLVI(e.Item);
//                e.DrawDefault = true;
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine("problem in lvwConnMgrs_DrawItem: " + ex.Message + " " + ex.StackTrace);
//            }
//        }
//#endif

        void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(((Control)sender).Name + " dropdown changed");
                BuildToDos(this.ApplicationObject.ActiveWindow, null);
            }
            catch { }
        }

        void configurationsChangeService_ComponentAdded(object sender, ComponentEventArgs e)
        {
            try
            {
                mostRecentComponentEvent = DateTime.Now;
                System.Diagnostics.Debug.WriteLine(e.Component.GetType().FullName + " added");
                if (e.Component is DtsContainer)
                {
                    IDesignerHost designer = (IDesignerHost)sender;
                    Window win = FindWindowForDesigner(designer);
                    if (win == null) return;
                    HighlightingToDo.ClearCache((Executable)e.Component);
                    BuildToDos(win, (DtsContainer)e.Component);
                    return;
                }
                else if (e.Component is Microsoft.DataTransformationServices.PipelineDesignTimeObject)
                {
                    Microsoft.DataTransformationServices.PipelineDesignTimeObject pipelineMetadata = e.Component as Microsoft.DataTransformationServices.PipelineDesignTimeObject;
                    System.Diagnostics.Debug.WriteLine("added transform: " + pipelineMetadata.Name);
                    IDesignerHost designer = (IDesignerHost)sender;
                    Window win = FindWindowForDesigner(designer);
                    if (win == null) return;
                    string sUniqueID = win.ProjectItem.Name + "/" + pipelineMetadata.PipelineTask.ID + "/components/" + pipelineMetadata.ID;
                    System.Diagnostics.Debug.WriteLine("clearing cache for: " + sUniqueID);
                    HighlightingToDo.ClearCache(sUniqueID);
                    BuildToDos(win, pipelineMetadata.PipelineTask, pipelineMetadata.ID);
                    return;
                }
            }
            catch { }
        }

        void configurationsChangeService_ComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            //you can make changes while in Debug mode, so do not quit if in debug mode
            //in debug mode, this function clears the cache immediately, but no rescanning is done because BuildToDos quits in debug mode

            bool bHighlightCalled = false;
            try
            {
                mostRecentComponentEvent = DateTime.Now;
                System.Diagnostics.Debug.WriteLine("enter " + e.Component.GetType().FullName + " ComponentChanged");
                if (e.Member != null)
                    System.Diagnostics.Debug.WriteLine("member descriptor type: " + e.Member.GetType().FullName);

                if (e.Component is Package)
                {
                    if (e.Member == null) //capture when the package configuration editor window is closed
                    {
                        IDesignerHost designer = (IDesignerHost)sender;
                        Window win = FindWindowForDesigner(designer);
                        if (win == null) return;
                        HighlightingToDo.ClearCache((Package)e.Component);
                        BuildToDos(win, (DtsContainer)e.Component);
                        bHighlightCalled = true;
                        return;
                    }
                }
                else if (e.Component is DtsObject && e.Member != null)
                {
                    if (e.Member.Name == "Expressions" || e.Member.Name == "Name")
                    {
                        IDesignerHost designer = (IDesignerHost)sender;
                        Window win = FindWindowForDesigner(designer);
                        if (win == null) return;
                        if (e.Component is Executable)
                            HighlightingToDo.ClearCache((Executable)e.Component);
                        else if (e.Component is ConnectionManager)
                            HighlightingToDo.ClearCache((ConnectionManager)e.Component);
                        BuildToDos(win, (DtsObject)(e.Component));
                        bHighlightCalled = true;
                        return;
                    }
                }
                else if (e.Component is Microsoft.DataTransformationServices.PipelineDesignTimeObject)
                {
                    Microsoft.DataTransformationServices.PipelineDesignTimeObject pipelineMetadata = e.Component as Microsoft.DataTransformationServices.PipelineDesignTimeObject;
                    System.Diagnostics.Debug.WriteLine("edited transform: " + pipelineMetadata.Name);
                    IDesignerHost designer = (IDesignerHost)sender;
                    Window win = FindWindowForDesigner(designer);
                    if (win == null) return;
                    string sUniqueID = win.ProjectItem.Name + "/" + pipelineMetadata.PipelineTask.ID + "/components/" + pipelineMetadata.ID;
                    System.Diagnostics.Debug.WriteLine("clearing cache for: " + sUniqueID);
                    HighlightingToDo.ClearCache(sUniqueID);
                    BuildToDos(win, pipelineMetadata.PipelineTask, pipelineMetadata.ID);
                    bHighlightCalled = true;
                    return;
                }
                //TODO:
                //don't think this is necessary anymore
                //else if (e.Component is Microsoft.DataWarehouse.VsIntegration.Designer.NamedCustomTypeDescriptor)
                //{
                //    IDesignerHost designer = (IDesignerHost)sender;
                //    foreach (Window win in this.ApplicationObject.Windows)
                //    {
                //        if (win.Object == designer)
                //        {
                //            BuildToDos(win, null);
                //            bHighlightCalled = true;
                //            return;
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error in configurationsChangeService_ComponentChanged: " + ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                try
                {
                    if (e.Component != null)
                    {
                        if (e.Member != null)
                            System.Diagnostics.Debug.WriteLine(e.Component.GetType().FullName + " property updated: " + e.Member.Name + (bHighlightCalled ? " HIGHLIGHTED" : ""));
                        else
                            System.Diagnostics.Debug.WriteLine(e.Component.GetType().FullName + " updated" + (bHighlightCalled ? " HIGHLIGHTED" : ""));
                    }
                }
                catch { }
            }
        }

        private Window FindWindowForDesigner(IDesignerHost designer)
        {
            if (this.ApplicationObject.ActiveWindow.Object == designer) return this.ApplicationObject.ActiveWindow;

            foreach (Window win in this.ApplicationObject.Windows)
            {
                if (win.Object == designer)
                {
                    return win;
                }
            }
            return null;
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ActiveViewChanged starting");
            OnActiveViewChanged();
            System.Diagnostics.Debug.WriteLine("ActiveViewChanged finished");
        }
        #endregion

        #region Helper Functions
        private Package GetPackageFromContainer(DtsContainer container)
        {
            while (!(container is Package))
            {
                container = container.Parent;
            }
            return (Package)container;
        }


        //recursively looks in executables to find executable with the specified GUID
        public static Executable FindExecutable(IDTSSequence parentExecutable, string sObjectGuid)
        {
            Executable matchingExecutable = null;

            if (parentExecutable.Executables.Contains(sObjectGuid))
            {
                matchingExecutable = parentExecutable.Executables[sObjectGuid];
            }
            else
            {
                foreach (Executable e in parentExecutable.Executables)
                {
                    if (e is IDTSSequence)
                    {
                        matchingExecutable = FindExecutable((IDTSSequence)e, sObjectGuid);
                        if (matchingExecutable != null) return matchingExecutable;
                    }
                }
            }
            return matchingExecutable;
        }

        private void AddWarningsToVSErrorList(ITaskListService taskListService, string sFilename, string[] warnings)
        {
            ErrorList errorList = this.ApplicationObject.ToolWindows.ErrorList;
            Window2 errorWin2 = (Window2)(errorList.Parent);

            if (warnings.Length > 0)
            {
                if (!errorWin2.Visible)
                {
                    this.ApplicationObject.ExecuteCommand("View.ErrorList", " ");
                }
                //errorWin2.SetFocus(); //don't focus the error window because the Expression Highlighter pays attention to window focusing
            }

            //remove old task items from this document and BIDS Helper class
            System.Collections.Generic.List<ITaskItem> tasksToRemove = new System.Collections.Generic.List<ITaskItem>();
            foreach (ITaskItem ti in taskListService.GetTaskItems())
            {
                ICustomTaskItem task = ti as ICustomTaskItem;
                if (task != null && task.CustomInfo == this && task.Document == sFilename)
                {
                    tasksToRemove.Add(ti);
                }
            }
            foreach (ITaskItem ti in tasksToRemove)
            {
                taskListService.Remove(ti);
            }

            //add new task items
            foreach (string s in warnings)
            {
                ICustomTaskItem item = (ICustomTaskItem)taskListService.CreateTaskItem(TaskItemType.Custom, s);
                item.Category = TaskItemCategory.Misc;
                item.Appearance = TaskItemAppearance.Squiggle;
                item.Priority = TaskItemPriority.Normal;
                item.Document = sFilename;
                item.CustomInfo = this;
                taskListService.Add(item);
            }
        }


        #endregion

        #region Standard Plugin Overrides
        public override string ShortName
        {
            get { return "ExpressionHighlighterPlugin"; }
        }


        public override string ToolTip
        {
            get { return string.Empty; }
        }


        /// <summary>
        /// Gets the name of the friendly name of the plug-in.
        /// </summary>
        /// <value>The friendly name.</value>
        /// <remarks>Used for the HelpUrl as the ButtonText not match Wiki page.</remarks>
        public override string FeatureName
        {
            get { return "Expression and Configuration Highlighter"; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Highlight objects in your SSIS Packages that have configurations or expressions applied making them easy to identify."; }
        }

        

        public override void Exec()
        {
        }
        #endregion
    }
}