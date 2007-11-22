using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using System;
using Microsoft.Win32;
using MSDDS;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using wrap = Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace BIDSHelper
{
    public class FixedWidthColumnsPlugin : BIDSHelperPluginBase
    {

        public FixedWidthColumnsPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }


        public override string ShortName
        {
            get { return "FixedWidthColumnsPlugin"; }
        }

        public override int Bitmap
        {
            get { return 636; }
        }

        public override string ButtonText
        {
            get { return "Create Fixed Width Columns..."; }
        }

        public override string FriendlyName
        {
            get { return "Create Fixed Width Columns"; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }

        public override string MenuName
        {
            get { return "Connection"; }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                if (hierItem.Name.ToLower().EndsWith(".dtsx"))
                {
                    ProjectItem pi = (ProjectItem)hierItem.Object;
                    if (pi == null) return false;
                    Window win = pi.Document.ActiveWindow;
                    if (win == null) return false;
                    if (pi.Document.ActiveWindow.DTE.Mode == vsIDEMode.vsIDEModeDebug) return false;
                    IDesignerHost designer = (IDesignerHost)pi.Document.ActiveWindow.Object;
                    Package package = null;
                    ConnectionManager conn = GetSelectedConnectionManager(designer, out package);
                    if (conn == null || conn.CreationName != "FLATFILE" || conn.Properties["Format"].GetValue(conn).ToString() != "FixedWidth") return false;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private ConnectionManager GetSelectedConnectionManager(IDesignerHost designer, out Package package)
        {
            package = null;
            IDTSSequence container = null;
            TaskHost taskHost = null;

            System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
            EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
            Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
            DdsDiagramHostControl diagram = null;
            ListView lvwConnMgrs = null;

            if (win.SelectedIndex == 0) //Control Flow
            {
                diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];
                lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
            }
            else if (win.SelectedIndex == 1) //Data Flow
            {
                diagram = (DdsDiagramHostControl)viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls["PipelineTaskView"];
                taskHost = (TaskHost)diagram.ComponentDiagram.RootComponent;
                container = (IDTSSequence)taskHost.Parent;
                lvwConnMgrs = (ListView)viewControl.Controls["dataFlowsTrayTabControl"].Controls["dataFlowConnectionsTabPage"].Controls["dataFlowConnectionsListView"];
            }
            else if (win.SelectedIndex == 2) //Event Handlers
            {
                diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
                lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
            }
            else
            {
                return null;
            }

            if (lvwConnMgrs.SelectedItems.Count != 1) return null;
            ListViewItem lviConn = lvwConnMgrs.SelectedItems[0];

            package = GetPackageFromContainer((DtsContainer)container);
            ConnectionManager conn = FindConnectionManager(package, lviConn.Text);
            return conn;
        }

        private Package GetPackageFromContainer(DtsContainer container)
        {
            while (!(container is Package))
            {
                container = container.Parent;
            }
            return (Package)container;
        }
        
        private ConnectionManager FindConnectionManager(Package package, string connectionManagerName)
        {
            ConnectionManager matchingConnectionManager = null;

            if (package.Connections.Contains(connectionManagerName))
            {
                matchingConnectionManager = package.Connections[connectionManagerName];
            }

            return matchingConnectionManager;
        }
        
        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                ProjectItem pi = (ProjectItem)hierItem.Object;
                Window win = pi.Document.ActiveWindow;
                IDesignerHost designer = (IDesignerHost)pi.Document.ActiveWindow.Object;
                Package package = null;
                ConnectionManager conn = GetSelectedConnectionManager(designer, out package);
                
                BIDSHelper.SSIS.FixedWidthColumnsForm form = new BIDSHelper.SSIS.FixedWidthColumnsForm();
                DialogResult dialogResult = form.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
#if KATMAI
                    wrap.IDTSConnectionManagerFlatFile100 ff = conn.InnerObject as wrap.IDTSConnectionManagerFlatFile100;
                    DtsConvert.GetExtendedInterface(conn);
#else
                    wrap.IDTSConnectionManagerFlatFile90 ff = conn.InnerObject as wrap.IDTSConnectionManagerFlatFile90;
                    DtsConvert.ToConnectionManager90(conn);
#endif

                    while (ff.Columns.Count > 0)
                        ff.Columns.Remove(0);

                    List<string> listUsedNames = new List<string>();
                    foreach (DataGridViewRow row in form.dataGridView1.Rows)
                    {
                        string sName = row.Cells[0].Value.ToString();
                        string sOriginalName = sName;
                        int i = 1;
                        while (listUsedNames.Contains(sName)) //find a unique name for the column
                        {
                            sName = sOriginalName + (++i);
                        }
                        listUsedNames.Add(sName);

#if KATMAI
                        wrap.IDTSConnectionManagerFlatFileColumn100 col = ff.Columns.Add();
                        wrap.IDTSName100 name = col as wrap.IDTSName100;
#else
                        wrap.IDTSConnectionManagerFlatFileColumn90 col = ff.Columns.Add();
                        wrap.IDTSName90 name = col as wrap.IDTSName90;
#endif

                        name.Name = sName;
                        col.ColumnWidth = int.Parse(row.Cells[1].Value.ToString());
                        col.MaximumWidth = col.ColumnWidth;
                        col.DataType = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_STR;
                        col.ColumnType = "FixedWidth";
                    }

                    //mark package object as dirty
                    IComponentChangeService changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));
                    changesvc.OnComponentChanging(package, null);
                    changesvc.OnComponentChanged(package, null, null, null); //marks the package designer as dirty
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }

}