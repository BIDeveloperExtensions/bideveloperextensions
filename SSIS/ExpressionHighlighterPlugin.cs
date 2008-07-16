using EnvDTE;
using EnvDTE80;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using System;
using MSDDS;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.Collections.Generic;
using System.Threading;

#if KATMAI
using IDTSComponentMetaDataXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData100;
#else
using IDTSComponentMetaDataXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData90;
#endif


namespace BIDSHelper
{
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

        public ExpressionHighlighterPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
            workerToDos.WorkerReportsProgress = false;
            workerToDos.WorkerSupportsCancellation = true;
            workerToDos.DoWork += new System.ComponentModel.DoWorkEventHandler(workerToDos_DoWork);
        }


        #region Window Events Overrides and Disable Event
        public override bool ShouldHookWindowCreated
        {
            get { return false; }
        }
        public override bool ShouldHookWindowClosing
        {
            get { return true; }
        }

        public override void OnWindowActivated(Window GotFocus, Window lostFocus)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("OnWindowActivated: GotFocus=" + (GotFocus == null ? "Null" : "NotNull") + "  LostFocus=" + (lostFocus == null ? "Null" : "NotNull"));
                BuildToDos(GotFocus, null);
            }
            catch { }
        }
        public override void OnWindowClosing(Window ClosingWindow)
        {
            try
            {
                if (ClosingWindow == null) return;
                IDesignerHost designer = ClosingWindow.Object as IDesignerHost;
                if (designer == null) return;
                ProjectItem pi = ClosingWindow.ProjectItem;
                if (!(pi.Name.ToLower().EndsWith(".dtsx"))) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                Package package = (Package)win.PropertiesLinkComponent;

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

        private void BuildToDos(Window GotFocus, DtsObject oIncrementalObject, int? iIncrementalTransformID)
        {
            try
            {
                DateTime dtToDoBuildingStartTime = DateTime.Now;
                DateTime dtSynchronousHighlightingCutoff = DateTime.Now.AddSeconds(MAX_SYNCHRONOUS_HIGHLIGHTING_TIME_SECONDS);
                if (!this.Enabled) return;
                if (ExpressionListPlugin.bShouldSkipExpressionHighlighting) return;

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
                if (!(pi.Name.ToLower().EndsWith(".dtsx"))) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                Package package = (Package)win.PropertiesLinkComponent;

                //check whether we should abort because highlighting has been disabled for this window
                if (disableHighlighting.ContainsKey(win) && disableHighlighting[win]) return;

                try
                {
                    ExpressionListPlugin.bShouldSkipExpressionHighlighting = true; //don't come into this design time properties code until the prior one finished
                    
                    //refresh DDS objects as all their properties aren't updated until you save the DTSX file
                    //this code is to workaround a problem such that a newly copied/pasted TaskHost isn't linked in via the DDS objects correctly until this refresh
                    System.Collections.Hashtable designTimeProperties = new System.Collections.Hashtable();
                    System.Reflection.BindingFlags publicstaticflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static;
                    TYPE_DTS_SERIALIZATION.InvokeMember("CollectDesignTimeProperties", publicstaticflags, null, null, new object[] { package, designTimeProperties });
                    TYPE_DTS_SERIALIZATION.InvokeMember("SaveDesignTimePropertiesToPackage", publicstaticflags, null, null, new object[] { package, designTimeProperties });
                }
                finally
                {
                    ExpressionListPlugin.bShouldSkipExpressionHighlighting = false;
                }

                if (win.Tag == null)
                {
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                    IComponentChangeService configurationsChangeService = (IComponentChangeService)designer;
                    configurationsChangeService.ComponentChanged += new ComponentChangedEventHandler(configurationsChangeService_ComponentChanged);
                    configurationsChangeService.ComponentAdded += new ComponentEventHandler(configurationsChangeService_ComponentAdded);
                    win.Tag = true;
                    bRescan = true;
                }

                for (int iViewIndex = 0; iViewIndex < 3; iViewIndex++)
                {
                    EditorWindow.EditorView view = win.Views[iViewIndex];
                    Control viewControl = (Control)view.GetType().InvokeMember("ViewControl", getflags, null, view, null);
                    if (viewControl == null) continue;

                    if (iViewIndex == 0) //Control Flow
                    {
                        ListView lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                        BuildConnectionManagerToDos(package, lvwConnMgrs, bIncremental, bRescan, oIncrementalConnectionManager);

                        DdsDiagramHostControl diagram = viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"] as DdsDiagramHostControl;
                        if (diagram == null) continue;

                        IDTSSequence container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;

                        foreach (MSDDS.IDdsDiagramObject o in diagram.DDS.Objects)
                        {
                            if (o.Type != DdsLayoutObjectType.dlotShape) continue;
                            MSDDS.IDdsExtendedProperty prop = o.IDdsExtendedProperties.Item("LogicalObject");
                            if (prop == null) continue;
                            string sObjectGuid = prop.Value.ToString();

                            Executable executable = null;
                            bool bRescanThisTask = bRescan;
                            if (oIncrementalContainer != null && oIncrementalContainer.ID == sObjectGuid)
                            {
                                executable = oIncrementalContainer;
                                bRescanThisTask = true;
                            }
                            else
                            {
                                executable = FindExecutable(container, sObjectGuid);
                            }
                            if (executable != null)
                            {
                                TaskHighlightingToDo todo;
                                lock (highlightingToDos)
                                {
                                    if (highlightingToDos.ContainsKey(executable))
                                        todo = (TaskHighlightingToDo)highlightingToDos[executable];
                                    else
                                        highlightingToDos.Add(executable, todo = new TaskHighlightingToDo());
                                    todo.package = package;
                                    todo.executable = executable;
                                    todo.controlFlowDesigner = designer;
                                    todo.controlFlowDiagram = diagram;
                                    todo.controlFlowDiagramTask = o;
                                    todo.BackgroundOnly = !(!todo.BackgroundOnly || (view.Selected && diagram.Visible));
                                    todo.Rescan = bRescanThisTask;
                                }
                            }
                        }
                    }
                    else if (iViewIndex == 2) //Event Handlers
                    {
                        Microsoft.DataTransformationServices.Design.Controls.EventHandlersComboBox eventHandlersCombo = ((Microsoft.DataTransformationServices.Design.Controls.EventHandlersComboBox)(viewControl.Controls["panel1"].Controls["panel2"].Controls["comboBoxEventHandler"]));
                        if (eventHandlersCombo.Tag == null)
                        {
                            eventHandlersCombo.SelectedIndexChanged += new EventHandler(comboBox_SelectedIndexChanged);
                            eventHandlersCombo.Tag = true;
                            //don't need to monitor the Base Control (leftmost) combo box because changing it will trigger a change to the event handler combo: ((Microsoft.DataWarehouse.Controls.BaseControlComboBox)(viewControl.Controls["panel1"].Controls["panel2"].Controls["Custom ComboBox"]))
                        }

                        ListView lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                        BuildConnectionManagerToDos(package, lvwConnMgrs, bIncremental, bRescan, oIncrementalConnectionManager);
                        
                        foreach (Control c in viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls)
                        {
                            DdsDiagramHostControl diagram = c as DdsDiagramHostControl;
                            if (diagram == null) continue;

                            IDTSSequence container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;

                            foreach (MSDDS.IDdsDiagramObject o in diagram.DDS.Objects)
                            {
                                if (o.Type != DdsLayoutObjectType.dlotShape) continue;
                                MSDDS.IDdsExtendedProperty prop = o.IDdsExtendedProperties.Item("LogicalObject");
                                if (prop == null) continue;
                                string sObjectGuid = prop.Value.ToString();

                                Executable executable = null;
                                bool bRescanThisTask = bRescan;
                                if (oIncrementalContainer != null && oIncrementalContainer.ID == sObjectGuid)
                                {
                                    executable = oIncrementalContainer;
                                    bRescanThisTask = true;
                                }
                                else
                                {
                                    executable = FindExecutable(container, sObjectGuid);
                                }
                                if (executable != null)
                                {
                                    TaskHighlightingToDo todo;
                                    lock (highlightingToDos)
                                    {
                                        if (highlightingToDos.ContainsKey(executable))
                                            todo = (TaskHighlightingToDo)highlightingToDos[executable];
                                        else
                                            highlightingToDos.Add(executable, todo = new TaskHighlightingToDo());
                                        todo.package = package;
                                        todo.executable = executable;
                                        todo.controlFlowDesigner = designer;
                                        todo.controlFlowDiagram = diagram;
                                        todo.controlFlowDiagramTask = o;
                                        todo.BackgroundOnly = !(!todo.BackgroundOnly || (view.Selected && diagram.Visible));
                                        todo.Rescan = bRescanThisTask;
                                    }
                                }
                            }
                        }
                    }
                    else if (iViewIndex == 1) //Data Flow
                    {
                        ListView lvwConnMgrs = (ListView)viewControl.Controls["dataFlowsTrayTabControl"].Controls["dataFlowConnectionsTabPage"].Controls["dataFlowConnectionsListView"];
                        BuildConnectionManagerToDos(package, lvwConnMgrs, bIncremental, bRescan, oIncrementalConnectionManager);

                        Microsoft.DataTransformationServices.Design.Controls.PipelineComboBox pipelineComboBox = (Microsoft.DataTransformationServices.Design.Controls.PipelineComboBox)(viewControl.Controls["panel1"].Controls["pipelineComboBox"]);
                        if (pipelineComboBox.Tag == null)
                        {
                            pipelineComboBox.SelectedIndexChanged += new EventHandler(comboBox_SelectedIndexChanged);
                            pipelineComboBox.Tag = true;
                        }

                        foreach (Control c in viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls)
                        {
                            DdsDiagramHostControl diagram = c as DdsDiagramHostControl;
                            if (diagram == null) continue;

                            TaskHost taskHost = (TaskHost)diagram.ComponentDiagram.RootComponent;
                            Executable executable = (Executable)taskHost;
                            MainPipe pipe = (MainPipe)taskHost.InnerObject;
                            IDTSSequence container = (IDTSSequence)taskHost.Parent;
                            if (bIncremental && !bRescan && oIncrementalContainer != taskHost) continue;

                            List<string> transforms = new List<string>();
                            foreach (MSDDS.IDdsDiagramObject o in diagram.DDS.Objects)
                            {
                                if (o.Type == DdsLayoutObjectType.dlotShape)
                                {
                                    MSDDS.IDdsExtendedProperty prop = o.IDdsExtendedProperties.Item("LogicalObject");
                                    if (prop == null) continue;
                                    string sObjectGuid = prop.Value.ToString();
                                    int id = int.Parse(sObjectGuid.Substring(sObjectGuid.LastIndexOf("/") + 1));
                                    sObjectGuid = pi.Name + "/" + sObjectGuid; //this is the todo key... (trying to use the IDTSComponentMetaDataXX as the key caused problems with COM object references and threading)
                                    IDTSComponentMetaDataXX transform = pipe.ComponentMetaDataCollection.GetObjectByID(id);
                                    transforms.Add(sObjectGuid);

                                    TransformHighlightingToDo transformTodo;
                                    lock (highlightingToDos)
                                    {
                                        if (highlightingToDos.ContainsKey(sObjectGuid))
                                            transformTodo = (TransformHighlightingToDo)highlightingToDos[sObjectGuid];
                                        else
                                            highlightingToDos.Add(sObjectGuid, transformTodo = new TransformHighlightingToDo());
                                        transformTodo.package = package;
                                        transformTodo.dataFlowDesigner = designer;
                                        transformTodo.dataFlowDiagram = diagram;
                                        transformTodo.dataFlowDiagramTask = o;
                                        transformTodo.taskHost = taskHost;
                                        transformTodo.transformName = transform.Name;
                                        transformTodo.transformUniqueID = sObjectGuid;
                                        transformTodo.BackgroundOnly = !(!transformTodo.BackgroundOnly || (view.Selected && diagram.Visible));
                                    }
                                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(transform);
                                }
                            }

                            TaskHighlightingToDo todo;
                            lock (highlightingToDos)
                            {
                                if (highlightingToDos.ContainsKey(executable))
                                    todo = (TaskHighlightingToDo)highlightingToDos[executable];
                                else
                                    highlightingToDos.Add(executable, todo = new TaskHighlightingToDo());
                                todo.package = package;
                                todo.executable = executable;
                                todo.BackgroundOnly = !(!todo.BackgroundOnly || (view.Selected && diagram.Visible));
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

                if (dtToDoBuildingStartTime.AddSeconds(MAX_SECONDS_BUILDING_TO_DOS_BEFORE_OFFER_DISABLE) < DateTime.Now && !disableHighlighting.ContainsKey(win))
                {
                    //it took more than 10 seconds to build the to do's list... offer that we disable highlighting for this package
                    DialogResult result = MessageBox.Show("BIDS Helper Expression and Configuration Highlighting is taking a very long time to complete.\r\nThis can occur sometimes with complex packages.\r\n\r\nWould you like to disable highlighting on this package until you reopen it?\r\n\r\n(You may also completely disable the feature on all packages via Tools... Options... BIDS Helper.)", "BIDS Helper Expression and Configuration Highlighter Performance", MessageBoxButtons.YesNo);
                    disableHighlighting.Add(win, (result == DialogResult.Yes));
                    if (result == DialogResult.Yes) return;
                }

                if (!bIncremental && bRescan)
                    HighlightingToDo.CachePackageConfigurations(package, pi);

                StartToDosThread(dtSynchronousHighlightingCutoff);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + " " + ex.StackTrace);
            }
        }

        private void BuildConnectionManagerToDos(Package package, ListView lvwConnMgrs, bool bIncremental, bool bRescan, ConnectionManager oIncrementalConnectionManager)
        {
            foreach (ListViewItem lviConn in lvwConnMgrs.Items)
            {
                ConnectionManager conn = lviConn.GetType().InvokeMember("Component", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, lviConn, null) as ConnectionManager;
                if (conn == null) continue;
                ConnectionManagerHighlightingToDo todo;
                lock (highlightingToDos)
                {
                    if (highlightingToDos.ContainsKey(conn))
                        todo = (ConnectionManagerHighlightingToDo)highlightingToDos[conn];
                    else
                        highlightingToDos.Add(conn, todo = new ConnectionManagerHighlightingToDo());
                    todo.package = package;
                    todo.connection = conn;
                    todo.listConnectionLVIs.Add(lviConn);
                    todo.BackgroundOnly = false;
                    todo.Rescan = (conn == oIncrementalConnectionManager) || bRescan;
                }
            }
            if (lvwConnMgrs.Tag == null)
            {
                lvwConnMgrs.DrawItem += new DrawListViewItemEventHandler(lvwConnMgrs_DrawItem);
                lvwConnMgrs.OwnerDraw = true; //forces the DrawItem event to fire so that we can detect when we need to fix the icon
                lvwConnMgrs.Tag = true;
            }
        }
        #endregion

        #region Worker Thread
        private void StartToDosThread(DateTime dtSynchronousHighlightingCutoff)
        {
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
                            if (!todo.BackgroundOnly) break;
                        }
                    }

                    //if there are no foreground todos, the last todo from the loop above is executed
                    if (todo.BackgroundOnly && !bWorkerThreadEventSet)
                    {
                        System.Diagnostics.Debug.WriteLine("finished foreground highlighting tasks");
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
                            ApplicationObject.StatusBar.Progress(false, STATUS_BAR_PROGRESS_CAPTION, 100, 100);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine((int)(((double)iToDosExecuted / (iToDosExecuted + iForegroundTasks)) * 100) + " percent complete");
                            ApplicationObject.StatusBar.Progress(true, STATUS_BAR_PROGRESS_CAPTION, ((int)((double)iToDosExecuted / (iToDosExecuted + iForegroundTasks) * 100)), 100);
                        }
                    }
                }
                bWorkerThreadDoneWithWork = true;
                if (highlightingToDos.Count > 0 && workerToDos.CancellationPending)
                    System.Diagnostics.Debug.WriteLine("cancelled background worker successfully!!!!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error inside expression highlighter DoWork: " + ex.Message + " " + ex.StackTrace);
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
        private void lvwConnMgrs_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            try
            {
                if (this.Enabled)
                    ConnectionManagerHighlightingToDo.HighlightConnectionManagerLVI(e.Item);
                e.DrawDefault = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("problem in lvwConnMgrs_DrawItem: " + ex.Message + " " + ex.StackTrace);
            }
        }

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
                System.Diagnostics.Debug.WriteLine(e.Component.GetType().FullName + " added");
                if (e.Component is DtsContainer)
                {
                    IDesignerHost designer = (IDesignerHost)sender;
                    foreach (Window win in this.ApplicationObject.Windows)
                    {
                        if (win.Object == designer)
                        {
                            HighlightingToDo.ClearCache((Executable)e.Component);
                            BuildToDos(win, (DtsContainer)e.Component);
                            return;
                        }
                    }
                }
                else if (e.Component is Microsoft.DataTransformationServices.PipelineDesignTimeObject)
                {
                    Microsoft.DataTransformationServices.PipelineDesignTimeObject pipelineMetadata = e.Component as Microsoft.DataTransformationServices.PipelineDesignTimeObject;
                    System.Diagnostics.Debug.WriteLine("added transform: " + pipelineMetadata.Name);
                    IDesignerHost designer = (IDesignerHost)sender;
                    foreach (Window win in this.ApplicationObject.Windows)
                    {
                        if (win.Object == designer)
                        {
                            string sUniqueID = win.ProjectItem.Name + "/" + pipelineMetadata.PipelineTask.ID + "/components/" + pipelineMetadata.ID;
                            System.Diagnostics.Debug.WriteLine("clearing cache for: " + sUniqueID);
                            HighlightingToDo.ClearCache(sUniqueID);
                            BuildToDos(win, pipelineMetadata.PipelineTask, pipelineMetadata.ID);
                            return;
                        }
                    }
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
                System.Diagnostics.Debug.WriteLine("enter " + e.Component.GetType().FullName + " ComponentChanged");
                if (e.Member != null)
                    System.Diagnostics.Debug.WriteLine("member descriptor type: " + e.Member.GetType().FullName);

                if (e.Component is Package)
                {
                    if (e.Member == null) //capture when the package configuration editor window is closed
                    {
                        IDesignerHost designer = (IDesignerHost)sender;
                        foreach (Window win in this.ApplicationObject.Windows)
                        {
                            if (win.Object == designer)
                            {
                                HighlightingToDo.ClearCache((Package)e.Component);
                                BuildToDos(win, (DtsContainer)e.Component);
                                bHighlightCalled = true;
                                return;
                            }
                        }
                    }
                }
                else if (e.Component is DtsObject && e.Member != null)
                {
                    if (e.Member.Name == "Expressions" || e.Member.Name == "Name")
                    {
                        IDesignerHost designer = (IDesignerHost)sender;
                        foreach (Window win in this.ApplicationObject.Windows)
                        {
                            if (win.Object == designer)
                            {
                                if (e.Component is Executable)
                                    HighlightingToDo.ClearCache((Executable)e.Component);
                                else if (e.Component is ConnectionManager)
                                    HighlightingToDo.ClearCache((ConnectionManager)e.Component);
                                BuildToDos(win, (DtsObject)(e.Component));
                                bHighlightCalled = true;
                                return;
                            }
                        }
                    }
                }
                else if (e.Component is Microsoft.DataTransformationServices.PipelineDesignTimeObject)
                {
                    Microsoft.DataTransformationServices.PipelineDesignTimeObject pipelineMetadata = e.Component as Microsoft.DataTransformationServices.PipelineDesignTimeObject;
                    System.Diagnostics.Debug.WriteLine("edited transform: " + pipelineMetadata.Name);
                    IDesignerHost designer = (IDesignerHost)sender;
                    foreach (Window win in this.ApplicationObject.Windows)
                    {
                        if (win.Object == designer)
                        {
                            string sUniqueID = win.ProjectItem.Name + "/" + pipelineMetadata.PipelineTask.ID + "/components/" + pipelineMetadata.ID;
                            System.Diagnostics.Debug.WriteLine("clearing cache for: " + sUniqueID);
                            HighlightingToDo.ClearCache(sUniqueID);
                            BuildToDos(win, pipelineMetadata.PipelineTask, pipelineMetadata.ID);
                            bHighlightCalled = true;
                            return;
                        }
                    }
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

        public static Type GetPrivateType(Type publicTypeInSameAssembly, string FullName)
        {
            foreach (Type t in System.Reflection.Assembly.GetAssembly(publicTypeInSameAssembly).GetTypes())
            {
                if (t.FullName == FullName)
                {
                    return t;
                }
            }
            return null;
        }
        #endregion

        #region Standard Plugin Overrides
        public override string ShortName
        {
            get { return "ExpressionHighlighterPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "Expression Highlighter"; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }

        public override string MenuName
        {
            get { return ""; } //no need to have a menu command
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            return false; //no menu item
        }

        public override void Exec()
        {
        }
        #endregion
    }
}