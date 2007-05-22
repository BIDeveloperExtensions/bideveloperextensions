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

        //TODO: may be needed if we decide to capture the ActiveViewChanged event... see TODO below on this topic
        //private System.Collections.Generic.List<string> windowHandlesFixedPartitionsView = new System.Collections.Generic.List<string>();

        public ExpressionHighlighterPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
            windowEvents = appObject.Events.get_WindowEvents(null);
            windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
        }

        void windowEvents_WindowCreated(Window Window)
        {
            windowEvents_WindowActivated(Window, null);
        }

        //TODO: need to find a way to pick up changes to the package more quickly than just the WindowActivated event
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
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
                DdsDiagramHostControl diagram = null;

                if (win.SelectedIndex == 0) //Control Flow
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];

                    //string s1 = "";
                    //foreach (Control ctrl in viewControl.Controls["panel1"].Controls)
                    //{
                    //    s1 += (ctrl.Name + ":" + ctrl.ToString() + Environment.NewLine);
                    //}
                    //System.Windows.Forms.MessageBox.Show(s1);
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
                }
                else if (win.SelectedIndex == 2) //Event Handlers
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
                }
                //TODO: put indicator on the connection in the connection managers pane?
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
                            Executable executable = FindCorrespondingExecutable(container, sObjectGuid);
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
                                //TODO: need better indicator???
                                ModifyIcon(icon, System.Drawing.Color.Magenta);

                                //TODO: change tooltip and put listing of all properties and their expressions?
                            }
                        }
                    }
                }
                return;






                //TODO: decide whether we need to monitor the ActiveViewChanged event to catch when we flip to a new tab
                //TODO: does the above code run too slow? should it be put in a BackgroundWorker thread? will that cause threads to step on each other?

                //IntPtr ptr = win.Handle;
                //string sHandle = ptr.ToInt64().ToString();

                //if (!windowHandlesFixedPartitionsView.Contains(sHandle))
                //{
                //    windowHandlesFixedPartitionsView.Add(sHandle);
                //    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                //}
            }
            catch { }
        }

        private static void ModifyIcon(System.Drawing.Bitmap icon, System.Drawing.Color color)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 8-i; j > -1; j--)
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

        //recursively looks in executables to find executable with the specified GUID
        Executable FindCorrespondingExecutable(IDTSSequence parentExecutable, string sObjectGuid)
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
                    matchingExecutable = FindCorrespondingExecutable((IDTSSequence) e, sObjectGuid);
                }
            }

            return matchingExecutable;
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