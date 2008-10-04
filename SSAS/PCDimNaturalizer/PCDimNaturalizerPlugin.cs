using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using BIDSHelper;

namespace PCDimNaturalizer
{
    public class PCDimNaturalizerPlugin : BIDSHelperPluginBase
    {
        #region Standard Plugin Overrides
        public PCDimNaturalizerPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "PCDimNaturalizer"; }
        }

        public override int Bitmap
        {
            get { return 688; } //or 2010
        }

        public override string ButtonText
        {
            get { return "Naturalize Parent-Child Dimension..."; }
        }

        public override string FriendlyName
        {
            get { return "Parent-Child Dimension Naturalizer"; }
        }

        /*public override string MenuName
        {
            get { return "DSV Background context menu"; } //could also use DSV Table context menu
        }*/

        public override string ToolTip
        {
            get { return ""; } //not used anywhere
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
                ProjectItem pi = (ProjectItem)hierItem.Object;
                if (!(pi.Object is DataSourceView)) return false;
                Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)pi.ContainingProject;
                return (projExt.Kind == BIDSProjectKinds.SSAS); //only show in an SSAS project, not in a report model or SSIS project (which also can have a DSV)
            }
            catch
            {
                return false;
            }
            //try
            //{
            //    System.ComponentModel.Design.IDesignerHost host = (System.ComponentModel.Design.IDesignerHost)(ApplicationObject.ActiveWindow.Object);
            //    DataSourceView dsv = (DataSourceView)host.RootComponent;
            //    Microsoft.AnalysisServices.Design.DataSourceDesigner designer = (Microsoft.AnalysisServices.Design.DataSourceDesigner)host.GetDesigner(dsv);

            //    System.Collections.ArrayList arrSelectedTables = designer.DataSourceDiagram.GetSelectedTableNames();
            //    if (arrSelectedTables.Count > 1)
            //        return false;

            //    return true;
            //}
            //catch
            //{
            //    return false;
            //}
        }
        #endregion

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                ProjectItem pi = (ProjectItem)hierItem.Object;

                bool bIsOpen = pi.get_IsOpen(BIDSViewKinds.Designer);
                if (bIsOpen)
                {
                    //if (MessageBox.Show("Save the DataSourceView?", "BIDS Helper - Naturalize Parent-Child Dimension", MessageBoxButtons.OKCancel) != DialogResult.OK)
                    //{
                    //    return;
                    //}
                    Window win = pi.Open(BIDSViewKinds.Designer);
                    win.Close(vsSaveChanges.vsSaveChangesPrompt);
                }

                DataSourceView dsv = pi.Object as DataSourceView;

                //System.ComponentModel.Design.IDesignerHost host = (System.ComponentModel.Design.IDesignerHost)(ApplicationObject.ActiveWindow.Object);
                //DataSourceView dsv = (DataSourceView)host.RootComponent;

                //ApplicationObject.ActiveWindow.Close(vsSaveChanges.vsSaveChangesYes);


                //Microsoft.AnalysisServices.Design.DataSourceDesigner designer = (Microsoft.AnalysisServices.Design.DataSourceDesigner)host.GetDesigner(dsv);
                DataSource dataSource = dsv.DataSource;

                //System.Collections.ArrayList arrSelectedTables = designer.DataSourceDiagram.GetSelectedTableNames();
                //if (arrSelectedTables.Count == 1)
                //{
                //    string sDataSourceID = dsv.Schema.Tables[arrSelectedTables[0]].ExtendedProperties["DataSourceID"];
                //    if (!string.IsNullOrEmpty(sDataSourceID) && sDataSourceID != dataSource.ID)
                //    {
                //        dataSource = dsv.Parent.DataSources[sDataSourceID];
                //    }
                //}

                Microsoft.DataWarehouse.Design.DataSourceConnection openedDataSourceConnection = Microsoft.DataWarehouse.DataWarehouseUtilities.GetOpenedDataSourceConnection((object)null, dataSource.ID, dataSource.Name, dataSource.ManagedProvider, dataSource.ConnectionString, dataSource.Site, false);
                try
                {
                    if (openedDataSourceConnection != null)
                    {
                        openedDataSourceConnection.QueryTimeOut = (int)dataSource.Timeout.TotalSeconds;
                    }
                }
                catch { }

                if (openedDataSourceConnection == null)
                {
                    MessageBox.Show("Unable to connect to data source [" + dataSource.Name + "]");
                    return;
                }
                else if (!(openedDataSourceConnection.ConnectionObject is System.Data.OleDb.OleDbConnection))
                {
                    MessageBox.Show("Data source [" + dataSource.Name + "] is not an OLEDB connection. Only OLEDB connections are supported currently.");
                    return;
                }

                Program.SQLFlattener = new frmSQLFlattener();
                Program.SQLFlattener.txtServer.Text = openedDataSourceConnection.DataSource;
                Program.SQLFlattener.cmbDatabase.Items[0] = openedDataSourceConnection.Database;
                Program.SQLFlattener.Conn = (System.Data.OleDb.OleDbConnection)openedDataSourceConnection.ConnectionObject;
                Program.SQLFlattener.cmbTable.Items[0] = string.Empty;
                Program.SQLFlattener.cmbID.Items[0] = string.Empty;
                Program.SQLFlattener.cmbPID.Items[0] = string.Empty;
                Program.SQLFlattener.dsv = dsv;
                Program.SQLFlattener.DataSourceConnection = openedDataSourceConnection;
                
                Program.SQLFlattener.ShowDialog();

                Window winDesigner = pi.Open(BIDSViewKinds.Designer);
                winDesigner.Activate();
                System.ComponentModel.Design.IDesignerHost host = (System.ComponentModel.Design.IDesignerHost)(winDesigner.Object);
                Microsoft.AnalysisServices.Design.DataSourceDesigner designer = (Microsoft.AnalysisServices.Design.DataSourceDesigner)host.GetDesigner(dsv);

                //Application.DoEvents();

                //System.Collections.ArrayList listTables = new System.Collections.ArrayList();
                //listTables.Add(Program.SQLFlattener.TableName);
                //designer.DataSourceDiagram.RearrangeTables(listTables, true);
                //System.Collections.ArrayList shapesByTableNames = designer.DataSourceDiagram.GetShapesByTableNames(listTables);
                //designer.DataSourceDiagram.RearrangeTables(shapesByTableNames, true);
                designer.MakeDesignerDirty();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
