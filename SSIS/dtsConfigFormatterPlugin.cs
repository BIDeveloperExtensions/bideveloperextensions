using System;
using System.Collections.Generic;
using System.Text;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;

namespace BIDSHelper.SSIS
{
    class dtsConfigFormatterPlugin:BIDSHelperPluginBase
    {
        private WindowEvents windowEvents;
        private DocumentEvents docEvents;
        private Timer t = new Timer();
        private int mRetryCnt = 0;
        private const int MAX_RETRY = 5;

        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        
        //TODO: may be needed if we decide to capture the ActiveViewChanged event... see TODO below on this topic
        //private System.Collections.Generic.List<string> windowHandlesFixedPartitionsView = new System.Collections.Generic.List<string>();

        public dtsConfigFormatterPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
            windowEvents = appObject.Events.get_WindowEvents(null);
            windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
            docEvents = appObject.Events.get_DocumentEvents(null);
            docEvents.DocumentOpened += new _dispDocumentEvents_DocumentOpenedEventHandler(docEvents_DocumentOpened);
            t.Tick += new EventHandler(t_Tick);
            t.Interval = 500;
        }

        void docEvents_DocumentOpened(Document doc)
        {
            doc.Activate();
            
            //windowEvents_WindowActivated(doc.ActiveWindow, null);
        }

        void windowEvents_WindowCreated(Window activeWindow)
        {
            if (activeWindow.ProjectItem.Document.Path.ToLower().EndsWith(".dtsconfig"))
            {
                activeWindow.Activate();
            }
            
            //windowEvents_WindowActivated(activeWindow, null);
        }

        //TODO: need to find a way to pick up changes to the package more quickly than just the WindowActivated event
        void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus == null) return;
                //IDesignerHost designer = (IDesignerHost)GotFocus.Object;
                //if (designer == null) return;
                ProjectItem pi = null;
                try
                {
                    pi = GotFocus.ProjectItem;
                }
                catch
                {
                    return;
                }
                
                if ((pi == null) || (!(pi.Name.ToLower().EndsWith(".dtsconfig")))) return;

                //pi.DTE.ExecuteCommand("Edit.FormatDocument" ,"");

                if ((t !=null) && (t.Enabled)) return;

                t.Enabled = true;
                t.Tag = pi;

                //EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));

                //Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
                //DdsDiagramHostControl diagram = null;
                //if (win.SelectedIndex == 0)
                //{
                //    //control flow designer
                //    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];
                //}
                //    //TODO: add in code to do this for the event handlers tab
                //    //TODO: put indicator on the connection in the connection managers pane?
                //else
                //{
                //    return;
                //}

                
                //return;
      
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        void t_Tick(object sender, EventArgs e)
        {
            t.Enabled = false;
            try
            {
                FormatActiveDocument((ProjectItem)t.Tag);
                mRetryCnt = 0;
            }
            catch
            {
                mRetryCnt++;
                if (mRetryCnt <= MAX_RETRY)
                {
                    System.Media.SystemSounds.Beep.Play();
                    t.Enabled = true;
                }
                else 
                {
                    mRetryCnt = 0; // reset the retry count                
                }
            }
        }

       
        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            windowEvents_WindowActivated(this.ApplicationObject.ActiveWindow, null);
        }

        public override string ShortName
        {
            get { return "dtsConfigFormatterPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "dtsConfig Formatter"; }
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

        void FormatActiveDocument(ProjectItem pi)
        {
            try
            {
                pi.DTE.ExecuteCommand("Edit.FormatDocument", "");
            }
            catch { }
        }
    }
}
