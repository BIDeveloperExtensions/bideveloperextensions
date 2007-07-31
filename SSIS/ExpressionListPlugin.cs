using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using System;
using Microsoft.Win32;
using MSDDS;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.ComponentModel;

namespace BIDSHelper
{
    public class ExpressionListPlugin : BIDSHelperPluginBase
    {
        private WindowEvents windowEvents;
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.List<string> windowHandlesFixedForExpressionHighlighter = new System.Collections.Generic.List<string>();
        private System.Collections.Generic.List<string> windowHandlesInProgressStatus = new System.Collections.Generic.List<string>();
        private ExpressionListControl expressionListWindow = null;
        private DTE2 appObject = null;

        EditorWindow win = null;
        System.ComponentModel.BackgroundWorker processPackage = null;

        public ExpressionListPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
            //all commented - don't want it to do anything
            windowEvents = appObject.Events.get_WindowEvents(null);
            windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
            windowEvents.WindowClosing += new _dispWindowEvents_WindowClosingEventHandler(windowEvents_WindowClosing);

            processPackage = new System.ComponentModel.BackgroundWorker();
            processPackage.WorkerReportsProgress = true;
            processPackage.WorkerSupportsCancellation = true;
            processPackage.DoWork += new System.ComponentModel.DoWorkEventHandler(processPackage_DoWork);
            processPackage.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(processPackage_ProgressChanged);
            processPackage.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(processPackage_RunWorkerCompleted);

            object programmableObject = null;

            //This guid must be unique for each different tool window,
            // but you may use the same guid for the same tool window.
            //This guid can be used for indexing the windows collection,
            // for example: applicationObject.Windows.Item(guidstr)
            Window toolWindow = null;
            String guidstr = "{6679390F-A712-40EA-8729-E2184A1436BF}";
            EnvDTE80.Windows2 windows2 = (EnvDTE80.Windows2)appObject.Windows;
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            toolWindow = windows2.CreateToolWindow2(addinInstance, asm.Location, "BIDSHelper.ExpressionListControl", "Expressions", guidstr, ref programmableObject);
            expressionListWindow = (ExpressionListControl)programmableObject;
            expressionListWindow.RefreshExpressions += new EventHandler(expressionListWindow_RefreshExpressions);

            //Set the picture displayed when the window is tab docked
            //expressionListWindow.SetTabPicture(BIDSHelper.Resources.Resource.ExpressionList.ToBitmap().GetHbitmap());


            //When using the hosting control, you must set visible to true before calling HostUserControl,
            // otherwise the UserControl cannot be hosted properly.
            toolWindow.Visible = true;


        }

        void expressionListWindow_RefreshExpressions(object sender, EventArgs e)
        {
            IDTSSequence container = null;
            TaskHost taskHost = null;

            DataGridView dgv = (DataGridView)expressionListWindow.Controls["DataGridView1"];
            dgv.Rows.Clear();

            if (win == null) return;

            try
            {
                Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
                DdsDiagramHostControl diagram = null;

                //IntPtr ptr = editorWin.Handle;
                //sHandle = ptr.ToInt64().ToString();

                //if (!windowHandlesFixedForExpressionHighlighter.Contains(sHandle))
                //{
                //    windowHandlesFixedForExpressionHighlighter.Add(sHandle);
                //    editorWin.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                //}

                //if (windowHandlesInProgressStatus.Contains(sHandle))
                //{
                //    return;
                //}
                //windowHandlesInProgressStatus.Add(sHandle);

                if (win.SelectedIndex == 0) //Control Flow
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];
                    container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                }
                else if (win.SelectedIndex == 1) //data flow
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls["PipelineTaskView"];
                    taskHost = (TaskHost)diagram.ComponentDiagram.RootComponent;
                    container = (IDTSSequence)taskHost.Parent;
                }
                else if (win.SelectedIndex == 2) //Event Handlers
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
                    container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                }
                else
                {
                    return;
                }

                processPackage.RunWorkerAsync(container);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Window Events

        void windowEvents_WindowClosing(Window Window)
        {
            processPackage.CancelAsync();
            win = null;
        }

        void windowEvents_WindowCreated(Window Window)
        {
            windowEvents_WindowActivated(Window, null);
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            windowEvents_WindowActivated(this.ApplicationObject.ActiveWindow, null);
        }

        //TODO: need to find a way to pick up changes to the package more quickly than just the WindowActivated event
        //The DtsPackageView object seems to have the appropriate methods, but it's internal to the Microsoft.DataTransformationServices.Design assembly.
        void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {

            try
            {
                if (GotFocus == null) return;
                if (GotFocus.DTE.Mode == vsIDEMode.vsIDEModeDebug) return;
                IDesignerHost designer = (IDesignerHost)GotFocus.Object;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if (!(pi.Name.ToLower().EndsWith(".dtsx"))) return;
                //EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                //Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
                //DdsDiagramHostControl diagram = null;
                ////ListView lvwConnMgrs = null;

                //IntPtr ptr = win.Handle;
                //sHandle = ptr.ToInt64().ToString();

                //if (!windowHandlesFixedForExpressionHighlighter.Contains(sHandle))
                //{
                //    windowHandlesFixedForExpressionHighlighter.Add(sHandle);
                //    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                //}

                //if (windowHandlesInProgressStatus.Contains(sHandle))
                //{
                //    return;
                //}
                //windowHandlesInProgressStatus.Add(sHandle);

                //if (win.SelectedIndex == 0) //Control Flow
                //{
                //    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];
                //    //lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                //    container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                //}
                //else if (win.SelectedIndex == 1) //data flow
                //{
                //    diagram = (DdsDiagramHostControl)viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls["PipelineTaskView"];
                //    taskHost = (TaskHost)diagram.ComponentDiagram.RootComponent;
                //    //pipe = (mainpipe)taskhost.innerobject;
                //    container = (IDTSSequence)taskHost.Parent;
                //    //lvwconnmgrs = (listview)viewcontrol.controls["dataflowstraytabcontrol"].controls["dataflowconnectionstabpage"].controls["dataflowconnectionslistview"];
                //}
                //else if (win.SelectedIndex == 2) //Event Handlers
                //{
                //    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
                //    //lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                //    container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                //}
                //else
                //{
                //    return;
                //}

                //processPackage.RunWorkerAsync(container);

                //Type managedshapebasetype = GetPrivateType(typeof(Microsoft.DataTransformationServices.Design.ColumnInfo), "Microsoft.DataTransformationServices.Design.ManagedShapeBase");
                //if (managedshapebasetype == null) return;

                //foreach (MSDDS.IDdsDiagramObject o in diagram.DDS.Objects)
                //{
                //    if (o.Type == DdsLayoutObjectType.dlotShape)
                //    {
                //        //TODO: any way of looking at the task metadata and determining that it hasn't changed since the last time we searched it for expressions?
                //        bool bHasExpression = false;
                //        MSDDS.IDdsExtendedProperty prop = o.IDdsExtendedProperties.Item("LogicalObject");
                //        if (prop == null) continue;
                //        string sObjectGuid = prop.Value.ToString();


                //        if (pipe == null) //Not a data flow
                //        {
                //            try
                //            {
                //                Executable executable = FindExecutable(container, sObjectGuid);

                //                if (executable is IDTSPropertiesProvider)
                //                {
                //                    bHasExpression = HasExpression(executable);
                //                }
                //            }
                //            catch
                //            {
                //                continue;
                //            }

                //        }
                //        else
                //        {
                //            IDTSComponentMetaData90 transform = pipe.ComponentMetaDataCollection.GetObjectByID(int.Parse(sObjectGuid.Substring(sObjectGuid.LastIndexOf("/") + 1)));
                //            bHasExpression = HasExpression(taskHost, transform.Name);
                //        }

                //        object managedShape = managedshapebasetype.InvokeMember("GetManagedShape", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static, null, null, new object[] { o });
                //        if (managedShape != null)
                //        {
                //            System.Drawing.Bitmap icon = (System.Drawing.Bitmap)managedshapebasetype.InvokeMember("Icon", getflags | System.Reflection.BindingFlags.Public, null, managedShape, null);
                //            if (!bHasExpression && icon.Tag != null)
                //            {
                //                //reset the icon because this one doesn't have an expression anymore
                //                System.Reflection.BindingFlags setflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                //                managedshapebasetype.InvokeMember("Icon", setflags, null, managedShape, new object[] { icon.Tag });
                //            }
                //            else if (bHasExpression && icon.Tag == null)
                //            {
                //                //save what the icon looked like originally so we can go back if they remove the expression
                //                icon.Tag = icon.Clone();

                //                //now update the icon to note this one has an expression
                //                ModifyIcon(icon, System.Drawing.Color.Magenta);

                //                //TODO: change tooltip and put listing of all properties and their expressions?
                //            }
                //        }
                //    }
                //}
                //HighlighConnectionManagers(container, lvwConnMgrs);

                return;

                //TODO: does the above code run too slow? should it be put in a BackgroundWorker thread? will that cause threads to step on each other?

            }
            catch { }
            finally
            {
                //if (windowHandlesInProgressStatus.Contains(sHandle))
                //{
                //    windowHandlesInProgressStatus.Remove(sHandle);
                //}
            }
        }

        #endregion

        #region BackgroundWorker Events

        void processPackage_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
            expressionListWindow.StopProgressBar();
        }

        void processPackage_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            ExpressionInfo info = (ExpressionInfo)e.UserState;

            if (info.HasExpression)
            {
                string[] newRow = { info.ObjectType, info.ObjectPath, info.ObjectName, info.PropertyName, info.Expression };

                DataGridView dgv = (DataGridView)expressionListWindow.Controls["DataGridView1"];
                dgv.Rows.Add(newRow);
            }



            //if (info.ObjectType == "Microsoft.SqlServer.Dts.Runtime.ConnectionManager")
            //{
            //    HighlightConnectionManagers(info.ObjectName, info.HasExpression);
            //}
            //else if (info.ObjectType == "Microsoft.SqlServer.Dts.Runtime.Variable")
            //{
            //    return;
            //}
            //else
            //{
            //    HighlightDiagram(info.ObjectID, info.PropertyName, info.HasExpression);
            //}
        }

        void processPackage_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker = (System.ComponentModel.BackgroundWorker)sender;

            DtsContainer sequence = (DtsContainer)e.Argument;

            IterateContainer(sequence, worker, string.Empty);
        }

        #endregion

        #region Package Scanning

        private void IterateContainer(DtsContainer container, System.ComponentModel.BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending) return;

            if (container is Package)
            {
                path = "Package.";
                CheckConnectionManagers((Package)container, worker, path);
            }

            if (container is IDTSPropertiesProvider)
            {
                CheckProperties((IDTSPropertiesProvider)container, worker, path);
            }

            IDTSSequence sequence = (IDTSSequence)container;

            foreach (Executable exec in sequence.Executables)
            {
                if (exec is IDTSSequence)
                {
                    IterateContainer((DtsContainer)exec, worker, path);
                }
                else if (exec is IDTSPropertiesProvider)
                {
                    CheckProperties((IDTSPropertiesProvider)exec, worker, path);
                }
            }
        }

        private void CheckConnectionManagers(Package package, BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending) return;

            foreach (ConnectionManager cm in package.Connections)
            {
                DtsContainer container = (DtsContainer)package;
                ScanProperties(worker, path + "Connections[" + cm.Name + "].", cm.GetType().ToString(), cm.ID, cm.Name, (IDTSPropertiesProvider)cm);
            }
        }

        private void CheckProperties(IDTSPropertiesProvider propProvider, BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending) return;

            if (propProvider is DtsContainer)
            {
                DtsContainer container = (DtsContainer)propProvider;
                ScanProperties(worker, path, container.GetType().ToString(), container.ID, container.Name, propProvider);
                ScanVariables(worker, path, container.GetType().ToString(), container.ID, container.Name, container.Variables);
            }
        }

        private void ScanVariables(BackgroundWorker worker, string objectPath, string objectType, string objectID, string objectName, Variables variables)
        {
            if (worker.CancellationPending) return;

            foreach (Variable v in variables)
            {
                try
                {
                    if (!v.EvaluateAsExpression) continue;
                    ExpressionInfo info = new ExpressionInfo();
                    info.ObjectID = objectID;
                    info.ObjectName = objectName;
                    info.ObjectPath = objectPath;
                    info.ObjectType = v.GetType().ToString();
                    info.PropertyName = v.Name;
                    info.Expression = v.Expression;
                    info.HasExpression = v.EvaluateAsExpression;
                    worker.ReportProgress(0, info);
                }
                catch { }
            }
        }

        private void ScanProperties(System.ComponentModel.BackgroundWorker worker, string objectPath, string objectType, string objectID, string objectName, IDTSPropertiesProvider provider)
        {
            if (worker.CancellationPending) return;

            foreach (DtsProperty p in provider.Properties)
            {
                try
                {
                    string expression = provider.GetExpression(p.Name);
                    if (expression == null)
                    {
                        continue;
                    }

                    ExpressionInfo info = new ExpressionInfo();
                    info.ObjectID = objectID;
                    info.ObjectName = objectName;
                    info.ObjectPath = objectPath + "Properties["+p.Name+"]";
                    info.ObjectType = objectType;
                    info.PropertyName = p.Name;
                    info.Expression = expression;
                    info.HasExpression = (info.Expression != null);
                    worker.ReportProgress(0, info);
                }
                catch { }
            }
        }

        #endregion


        private void HighlightDiagram(string objectGuid, string objectName, bool hasExpression)
        {
            Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
            DdsDiagramHostControl diagram = null;
            MainPipe pipe = null;
            TaskHost taskHost = null;

            if (win.SelectedIndex == 0) //controlFlow
            {
                diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];
            }
            else if (win.SelectedIndex == 1) //data flow
            {
                diagram = (DdsDiagramHostControl)viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls["PipelineTaskView"];
                taskHost = (TaskHost)diagram.ComponentDiagram.RootComponent;
                pipe = (MainPipe)taskHost.InnerObject;
            }
            else if (win.SelectedIndex == 2) //Event Handlers
            {
                diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
            }
            else
            {
                return;
            }

            Type managedshapebasetype = GetPrivateType(typeof(Microsoft.DataTransformationServices.Design.ColumnInfo), "Microsoft.DataTransformationServices.Design.ManagedShapeBase");
            if (managedshapebasetype == null) return;

            foreach (MSDDS.IDdsDiagramObject o in diagram.DDS.Objects)
            {
                if (o.Type == DdsLayoutObjectType.dlotShape)
                {
                    //TODO: any way of looking at the task metadata and determining that it hasn't changed since the last time we searched it for expressions?
                    MSDDS.IDdsExtendedProperty prop = o.IDdsExtendedProperties.Item("LogicalObject");
                    if (prop == null) continue;
                    string designerGuid = prop.Value.ToString();

                    if (pipe == null) //not a data flow
                    {
                        if (objectGuid != designerGuid) continue;
                    }
                    else
                    {
                        IDTSComponentMetaData90 transform = pipe.ComponentMetaDataCollection.GetObjectByID(int.Parse(designerGuid.Substring(designerGuid.LastIndexOf("/") + 1)));
                        if (objectName.StartsWith("[" + transform.Name + "]")) continue;
                    }

                    object managedShape = managedshapebasetype.InvokeMember("GetManagedShape", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static, null, null, new object[] { o });
                    if (managedShape != null)
                    {
                        System.Drawing.Bitmap icon = (System.Drawing.Bitmap)managedshapebasetype.InvokeMember("Icon", getflags | System.Reflection.BindingFlags.Public, null, managedShape, null);
                        if (!hasExpression && icon.Tag != null)
                        {
                            //reset the icon because this one doesn't have an expression anymore
                            System.Reflection.BindingFlags setflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                            managedshapebasetype.InvokeMember("Icon", setflags, null, managedShape, new object[] { icon.Tag });
                        }
                        else if (hasExpression && icon.Tag == null)
                        {
                            //save what the icon looked like originally so we can go back if they remove the expression
                            icon.Tag = icon.Clone();

                            //now update the icon to note this one has an expression
                            ModifyIcon(icon, System.Drawing.Color.Magenta);

                            //TODO: change tooltip and put listing of all properties and their expressions?
                        }
                    }
                }
            }

        }

        private void HighlightConnectionManagers(string objectName, bool hasExpression)
        {
            ListView lvwConnMgrs = null;
            Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);

            if (win.SelectedIndex == 0) //Control Flow
            {
                lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
            }
            else if (win.SelectedIndex == 1) //Data Flow
            {
                lvwConnMgrs = (ListView)viewControl.Controls["dataFlowsTrayTabControl"].Controls["dataFlowConnectionsTabPage"].Controls["dataFlowConnectionsListView"];
            }
            else if (win.SelectedIndex == 2) //Event Handlers
            {
                lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
            }

            foreach (ListViewItem lviConn in lvwConnMgrs.Items)
            {
                if (lviConn.Text == objectName)
                {
                    System.Drawing.Bitmap icon = (System.Drawing.Bitmap)lviConn.ImageList.Images[lviConn.ImageIndex];

                    if (!hasExpression && icon.Tag != null)
                    {
                        lviConn.ImageIndex = (int)icon.Tag;
                    }
                    else if (hasExpression && icon.Tag == null)
                    {
                        System.Drawing.Bitmap newicon = new System.Drawing.Bitmap(lviConn.ImageList.Images[lviConn.ImageIndex]);
                        newicon.Tag = lviConn.ImageIndex; //save the old index
                        ModifyIcon(newicon, System.Drawing.Color.Magenta);
                        lviConn.ImageList.Images.Add(newicon);
                        lviConn.ImageIndex = lviConn.ImageList.Images.Count - 1;
                    }
                }
            }

        }

        //private void HighlighConnectionManagers(IDTSSequence container, ListView lvwConnMgrs)
        //{
        //    foreach (ListViewItem lviConn in lvwConnMgrs.Items)
        //    {
        //        ConnectionManager conn = FindConnectionManager(GetPackageFromContainer((DtsContainer)container), lviConn.Text);

        //        bool bHasExpression = HasExpression(conn);

        //        System.Drawing.Bitmap icon = (System.Drawing.Bitmap)lviConn.ImageList.Images[lviConn.ImageIndex];

        //        if (!bHasExpression && icon.Tag != null)
        //        {

        //            lviConn.ImageIndex = (int)icon.Tag;

        //        }

        //        else if (bHasExpression && icon.Tag == null)
        //        {

        //            System.Drawing.Bitmap newicon = new System.Drawing.Bitmap(lviConn.ImageList.Images[lviConn.ImageIndex]);

        //            newicon.Tag = lviConn.ImageIndex; //save the old index

        //            ModifyIcon(newicon, System.Drawing.Color.Magenta);

        //            lviConn.ImageList.Images.Add(newicon);

        //            lviConn.ImageIndex = lviConn.ImageList.Images.Count - 1;

        //        }



        //    }
        //}

        //private Package GetPackageFromContainer(DtsContainer container)
        //{
        //    while (!(container is Package))
        //    {
        //        container = container.Parent;
        //    }
        //    return (Package)container;
        //}

        private static void ModifyIcon(System.Drawing.Bitmap icon, System.Drawing.Color color)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 8 - i; j > -1; j--)
                {
                    icon.SetPixel(i, j, color);
                }
            }
        }

        //Determine if the task has an expression
        //private bool HasExpression(Executable executable)
        //{
        //    IDTSPropertiesProvider task = (IDTSPropertiesProvider)executable;
        //    bool returnValue = false;

        //    foreach (DtsProperty p in task.Properties)
        //    {
        //        try
        //        {
        //            if (task.GetExpression(p.Name) != null)
        //            {
        //                returnValue = true;
        //                break;
        //            }
        //        }
        //        catch { }
        //    }
        //    return returnValue;
        //}

        //private bool HasExpression(ConnectionManager connectionManager)
        //{
        //    IDTSPropertiesProvider dtsObject = (IDTSPropertiesProvider)connectionManager;
        //    bool returnValue = false;

        //    foreach (DtsProperty p in dtsObject.Properties)
        //    {
        //        try
        //        {
        //            if (dtsObject.GetExpression(p.Name) != null)
        //            {
        //                returnValue = true;
        //                break;
        //            }
        //        }
        //        catch { }
        //    }
        //    return returnValue;
        //}

        private bool HasExpression(TaskHost taskHost, string transformName)
        {
            IDTSPropertiesProvider dtsObject = (IDTSPropertiesProvider)taskHost;
            bool returnValue = false;
            transformName = "[" + transformName + "]";

            foreach (DtsProperty p in dtsObject.Properties)
            {
                try
                {
                    if (p.Name.StartsWith(transformName) && dtsObject.GetExpression(p.Name) != null)
                    {
                        returnValue = true;
                        break;
                    }
                }
                catch { }
            }
            return returnValue;
        }

        //recursively looks in executables to find executable with the specified GUID
        //Executable FindExecutable(IDTSSequence parentExecutable, string sObjectGuid)
        //{
        //    Executable matchingExecutable = null;

        //    if (parentExecutable.Executables.Contains(sObjectGuid))
        //    {
        //        matchingExecutable = parentExecutable.Executables[sObjectGuid];
        //    }
        //    else
        //    {
        //        foreach (Executable e in parentExecutable.Executables)
        //        {
        //            matchingExecutable = FindExecutable((IDTSSequence)e, sObjectGuid);
        //        }
        //    }

        //    return matchingExecutable;
        //}

        //ConnectionManager FindConnectionManager(Package package, string connectionManagerName)
        //{
        //    ConnectionManager matchingConnectionManager = null;

        //    if (package.Connections.Contains(connectionManagerName))
        //    {
        //        matchingConnectionManager = package.Connections[connectionManagerName];
        //    }

        //    return matchingConnectionManager;
        //}


        Type GetPrivateType(Type publicTypeInSameAssembly, string FullName)
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

        public override string ShortName
        {
            get { return "ExpressionListPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "Expression List"; }
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
            return false; //TODO: decide whether to have a menu option where you can turn on/off this feature like the ShowExtraProperties feature
        }

        private struct ExpressionInfo
        {
            public string ObjectType;
            public string ObjectName;
            public string ObjectID;
            public string ObjectPath;
            public string PropertyName;
            public string Expression;
            public bool HasExpression;

        }


        public override void Exec()
        {
        }

    }
}