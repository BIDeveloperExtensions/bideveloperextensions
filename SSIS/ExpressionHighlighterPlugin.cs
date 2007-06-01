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

namespace BIDSHelper
{
    public class ExpressionHighlighterPlugin : BIDSHelperPluginBase
    {
        private WindowEvents windowEvents;
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.List<string> windowHandlesFixedForExpressionHighlighter = new System.Collections.Generic.List<string>();

        EditorWindow win = null;
        System.ComponentModel.BackgroundWorker processPackage = null;

        public ExpressionHighlighterPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
            windowEvents = appObject.Events.get_WindowEvents(null);
            windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);

            processPackage = new System.ComponentModel.BackgroundWorker();
            processPackage.DoWork += new System.ComponentModel.DoWorkEventHandler(processPackage_DoWork);
            processPackage.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(processPackage_ProgressChanged);
            processPackage.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(processPackage_RunWorkerCompleted);
        }

        void processPackage_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void processPackage_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void processPackage_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void windowEvents_WindowCreated(Window Window)
        {
            windowEvents_WindowActivated(Window, null);
        }

        //TODO: need to find a way to pick up changes to the package more quickly than just the WindowActivated event
        //The DtsPackageView object seems to have the appropriate methods, but it's internal to the Microsoft.DataTransformationServices.Design assembly.
        void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            IDTSSequence container = null;

            try
            {
                if (GotFocus == null) return;
                IDesignerHost designer = (IDesignerHost)GotFocus.Object;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if (!(pi.Name.ToLower().EndsWith(".dtsx"))) return;
                //EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
                DdsDiagramHostControl diagram = null;
                ListView lvwConnMgrs = null;

                IntPtr ptr = win.Handle;
                string sHandle = ptr.ToInt64().ToString();

                if (!windowHandlesFixedForExpressionHighlighter.Contains(sHandle))
                {
                    windowHandlesFixedForExpressionHighlighter.Add(sHandle);
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                }


                if (win.SelectedIndex == 0) //Control Flow
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];

                    lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];

                }
                else if (win.SelectedIndex == 1)
                {
                    ////data flow designer

                    //diagram = (DdsDiagramHostControl)viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls["PipelineTaskView"];
                    //MainPipe pipe = (MainPipe)((TaskHost)diagram.ComponentDiagram.RootComponent).InnerObject;
                    //foreach (MSDDS.IDdsDiagramObject o in diagram.DDS.Objects)
                    //{
                    //    if (o.Type == DdsLayoutObjectType.dlotShape)
                    //    {
                    //        //bool bHasExpression = false;
                    //        MSDDS.IDdsExtendedProperty prop = o.IDdsExtendedProperties.Item("LogicalObject");
                    //        if (prop == null) continue;
                    //        string sObjectGuid = prop.Value.ToString();
                    //        IDTSComponentMetaData90 transform = pipe.ComponentMetaDataCollection.GetObjectByID(int.Parse(sObjectGuid.Substring(sObjectGuid.LastIndexOf("/") + 1)));
                    //        return;
                    //    }
                    //}
                    lvwConnMgrs = (ListView)viewControl.Controls["dataFlowsTrayTabControl"].Controls["dataFlowConnectionsTabPage"].Controls["dataFlowConnectionsListView"];
                }
                else if (win.SelectedIndex == 2) //Event Handlers
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
                    lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                }
                else
                {
                    return;
                }


                container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;

                Type managedshapebasetype = GetPrivateType(typeof(Microsoft.DataTransformationServices.Design.ColumnInfo), "Microsoft.DataTransformationServices.Design.ManagedShapeBase");
                if (managedshapebasetype == null) return;

                foreach (MSDDS.IDdsDiagramObject o in diagram.DDS.Objects)
                {
                    if (o.Type == DdsLayoutObjectType.dlotShape)
                    {
                        //TODO: any way of looking at the task metadata and determining that it hasn't changed since the last time we searched it for expressions?
                        bool bHasExpression = false;
                        MSDDS.IDdsExtendedProperty prop = o.IDdsExtendedProperties.Item("LogicalObject");
                        if (prop == null) continue;
                        string sObjectGuid = prop.Value.ToString();
                        try
                        {
                            Executable executable = FindExecutable(container, sObjectGuid);

                            if (executable is IDTSPropertiesProvider)
                            {
                                bHasExpression = HasExpression(executable);
                            }
                        }
                        catch
                        {
                            continue;
                        }

                        object managedShape = managedshapebasetype.InvokeMember("GetManagedShape", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static, null, null, new object[] { o });
                        if (managedShape != null)
                        {
                            System.Drawing.Bitmap icon = (System.Drawing.Bitmap)managedshapebasetype.InvokeMember("Icon", getflags | System.Reflection.BindingFlags.Public, null, managedShape, null);
                            if (!bHasExpression && icon.Tag != null)
                            {
                                //reset the icon because this one doesn't have an expression anymore
                                System.Reflection.BindingFlags setflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                                managedshapebasetype.InvokeMember("Icon", setflags, null, managedShape, new object[] { icon.Tag });
                            }
                            else if (bHasExpression && icon.Tag == null)
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
                foreach (ListViewItem lviConn in lvwConnMgrs.Items)
                {
                    ConnectionManager conn = FindConnectionManager(GetPackageFromContainer((DtsContainer)container), lviConn.Text);
    
                    bool bHasExpression = HasExpression(conn);

                    System.Drawing.Bitmap icon = (System.Drawing.Bitmap)lviConn.ImageList.Images[lviConn.ImageIndex];

                    if (!bHasExpression && icon.Tag != null)
                    {

                        lviConn.ImageIndex = (int)icon.Tag;

                    }

                    else if (bHasExpression && icon.Tag == null)
                    {

                        System.Drawing.Bitmap newicon = new System.Drawing.Bitmap(lviConn.ImageList.Images[lviConn.ImageIndex]);

                        newicon.Tag = lviConn.ImageIndex; //save the old index

                        ModifyIcon(newicon, System.Drawing.Color.Magenta);

                        lviConn.ImageList.Images.Add(newicon);

                        lviConn.ImageIndex = lviConn.ImageList.Images.Count - 1;

                    }



                }

                return;

                //TODO: does the above code run too slow? should it be put in a BackgroundWorker thread? will that cause threads to step on each other?

            }
            catch { }
        }

        private Package GetPackageFromContainer(DtsContainer container)
        {
            while (! (container is Package))
            {
                container = container.Parent;
            }
            return (Package)container;
        }

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
        private bool HasExpression(Executable executable)
        {
            IDTSPropertiesProvider task = (IDTSPropertiesProvider)executable;
            bool returnValue = false;

            foreach (DtsProperty p in task.Properties)
            {
                try
                {
                    if (task.GetExpression(p.Name) != null)
                    {
                        returnValue = true;
                        break;
                    }
                }
                catch { }
            }
            return returnValue;
        }

        private bool HasExpression(ConnectionManager connectionManager)
        {
            IDTSPropertiesProvider dtsObject = (IDTSPropertiesProvider)connectionManager;
            bool returnValue = false;

            foreach (DtsProperty p in dtsObject.Properties)
            {
                try
                {
                    if (dtsObject.GetExpression(p.Name) != null)
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
        Executable FindExecutable(IDTSSequence parentExecutable, string sObjectGuid)
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
                    matchingExecutable = FindExecutable((IDTSSequence)e, sObjectGuid);
                }
            }

            return matchingExecutable;
        }

        ConnectionManager FindConnectionManager(Package package, string connectionManagerName)
        {
            ConnectionManager matchingConnectionManager = null;

            if (package.Connections.Contains(connectionManagerName))
            {
                matchingConnectionManager = package.Connections[connectionManagerName];
            }

            return matchingConnectionManager;
        }


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

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            windowEvents_WindowActivated(this.ApplicationObject.ActiveWindow, null);
        }

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
            return false; //TODO: decide whether to have a menu option where you can turn on/off this feature like the ShowExtraProperties feature
        }


        public override void Exec()
        {
        }

    }
}