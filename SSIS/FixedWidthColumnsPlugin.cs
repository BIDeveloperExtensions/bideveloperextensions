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

        public override string FeatureName
        {
            get { return "Create Fixed Width Columns"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
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
            get { return "Allows you to easily define Flat File Connections with fixed width columns, designed to allow you to easy copy and paste column name and with information from Excel or similar source."; }
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
                    if (conn == null || conn.CreationName != "FLATFILE" ||
                        (conn.Properties["Format"].GetValue(conn).ToString() != "FixedWidth" &&
                            conn.Properties["Format"].GetValue(conn).ToString() != "RaggedRight")) return false;
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

            System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
            EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
            Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);

#if DENALI
            Control lvwConnMgrs = null;
            package = (Package)win.PropertiesLinkComponent;

            if (win.SelectedIndex == (int)SSISHelpers.SsisDesignerTabIndex.ControlFlow)
            {
                //it's now a Microsoft.DataTransformationServices.Design.Controls.DtsConnectionsListView object which doesn't inherit from ListView and which is internal
                lvwConnMgrs = (Control)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
            }
            else if (win.SelectedIndex == (int)SSISHelpers.SsisDesignerTabIndex.DataFlow)
            {
                lvwConnMgrs = (Control)viewControl.Controls["dataFlowsTrayTabControl"].Controls["dataFlowConnectionsTabPage"].Controls["dataFlowConnectionsListView"];
            }
            else if (win.SelectedIndex == (int)SSISHelpers.SsisDesignerTabIndex.EventHandlers)
            {
                lvwConnMgrs = (Control)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
            }
            else
            {
                return null;
            }

            Microsoft.SqlServer.IntegrationServices.Designer.ConnectionManagers.ConnectionManagerUserControl cmControl = (Microsoft.SqlServer.IntegrationServices.Designer.ConnectionManagers.ConnectionManagerUserControl)lvwConnMgrs.GetType().InvokeMember("m_connectionManagerUserControl", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, lvwConnMgrs, null);
            Microsoft.SqlServer.IntegrationServices.Designer.ConnectionManagers.ConnectionManagerModelElement connModelEl = cmControl.SelectedItem as Microsoft.SqlServer.IntegrationServices.Designer.ConnectionManagers.ConnectionManagerModelElement;
            if (connModelEl == null) return null;
            ConnectionManager conn = connModelEl.ConnectionManager;
#else
            ListView lvwConnMgrs = null;
            IDTSSequence container = null;
            TaskHost taskHost = null;
            DdsDiagramHostControl diagram = null;
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
            package = GetPackageFromContainer((DtsContainer)container);

            ListViewItem lviConn = lvwConnMgrs.SelectedItems[0];
            ConnectionManager conn = FindConnectionManager(package, lviConn.Text);
#endif

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
                if (conn != null && conn.Properties["Format"].GetValue(conn).ToString() == "FixedWidth")
                {
                    //hiding properties for ragged right
                    form.dataGridView1.Height = form.dataGridView1.Height + 50 + 26;
                    form.dataGridView1.Top -= 50;
                    form.label1.Height -= 50;
                    form.cboRaggedRightDelimiter.Visible = false;
                    form.lblRaggedRight.Visible = false;
                }

                DialogResult dialogResult = form.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
#if KATMAI || DENALI
                    wrap.IDTSConnectionManagerFlatFile100 ff = conn.InnerObject as wrap.IDTSConnectionManagerFlatFile100;
                    DtsConvert.GetExtendedInterface(conn);
#else
                    wrap.IDTSConnectionManagerFlatFile90 ff = conn.InnerObject as wrap.IDTSConnectionManagerFlatFile90;
                    DtsConvert.ToConnectionManager90(conn);
#endif

                    while (ff.Columns.Count > 0)
                        ff.Columns.Remove(0);

                    List<string> listUsedNames = new List<string>();

                    //JCW - Added counter to identify the last column
                    int columnCount = 1;

                    foreach (DataGridViewRow row in form.dataGridView1.Rows)
                    {
                        string sName = row.Cells[0].Value.ToString().Trim();
                        string sOriginalName = sName;
                        int i = 1;
                        while (listUsedNames.Contains(sName)) //find a unique name for the column
                        {
                            sName = sOriginalName + (++i);
                        }
                        listUsedNames.Add(sName);

#if KATMAI || DENALI
                        wrap.IDTSConnectionManagerFlatFileColumn100 col = ff.Columns.Add();
                        wrap.IDTSName100 name = col as wrap.IDTSName100;
#else
                        wrap.IDTSConnectionManagerFlatFileColumn90 col = ff.Columns.Add();
                        wrap.IDTSName90 name = col as wrap.IDTSName90;
#endif

                        name.Name = sName;
                        col.MaximumWidth = int.Parse(row.Cells[1].Value.ToString());
                        col.DataType = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_STR;

                        if (columnCount == form.dataGridView1.Rows.Count && form.cboRaggedRightDelimiter.Text != "[None]")
                        {
                            col.ColumnWidth = 0;
                            col.ColumnType = "Delimited";
                            col.ColumnDelimiter = DecodeDelimiter(form.cboRaggedRightDelimiter.Text);
                        }
                        else
                        {
                            col.ColumnWidth = int.Parse(row.Cells[1].Value.ToString());
                            col.ColumnType = "FixedWidth";
                        }


                        columnCount++;
                    }

                    //mark package object as dirty
                    IComponentChangeService changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));
                    changesvc.OnComponentChanging(package, null);
                    changesvc.OnComponentChanged(package, null, null, null); //marks the package designer as dirty
                    SSISHelpers.MarkPackageDirty(package);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string DecodeDelimiter(string delimiterLabel)
        {
            if (delimiterLabel == "[None]") return string.Empty;
            if (delimiterLabel == "{CR}{LF}") return "\r\n";
            if (delimiterLabel == "{CR}") return "\r";
            if (delimiterLabel == "{LF}") return "\n";
            if (delimiterLabel == "Semicolon {;}") return ";";
            if (delimiterLabel == "Comma {,}") return ",";
            if (delimiterLabel == "Tab") return "\t";
            if (delimiterLabel == "Vertical Bar {|}") return "|";

            return delimiterLabel;
        }

    }

}