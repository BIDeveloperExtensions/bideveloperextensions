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

        public override string FeatureName
        {
            get { return "Parent-Child Dimension Naturalizer"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
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
            get { return BIDSFeatureCategories.SSAS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Parent-Child Dimension Naturalizer which aids in converting parent-child dimensions into natural hierarchies."; }
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
                if (pi.Object is DataSourceView)
                {
                    Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)pi.ContainingProject;
                    return (projExt.Kind == BIDSProjectKinds.SSAS); //only show in an SSAS project, not in a report model or SSIS project (which also can have a DSV)
                }
                else if (pi.Object is Dimension)
                {
                    Dimension dim = pi.Object as Dimension;
                    if (dim != null && dim.IsParentChild)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                ProjectItem pi = (ProjectItem)hierItem.Object;
                if (pi.Object is DataSourceView)
                    ExecDSV(pi);
                else if (pi.Object is Dimension)
                    ExecDimension(pi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void ExecDimension(ProjectItem pi)
        {
            try
            {
                Dimension dim = pi.Object as Dimension;
                Program.ASFlattener = new frmASFlattener();
                Program.ASFlattener.dim = dim;
                frmASFlattenerOptions Options = new frmASFlattenerOptions();
                Options.lbHierarchies.Items.Clear();
                Options.lbAttributes.Items.Clear();
                if (dim != null)
                {
                    foreach (Hierarchy hier in dim.Hierarchies)
                        Options.lbHierarchies.Items.Add(new ctlFancyCheckedListBoxItem(hier.Name, true));
                    foreach (DimensionAttribute attr in dim.Attributes)
                    {
                        if (attr.Usage == AttributeUsage.Regular)
                        {
                            if (ASPCDimNaturalizer.IsAttributeRelated(attr, dim.KeyAttribute))
                                Options.lbAttributes.Items.Add(attr.Name);
                            Options.lbHierarchies.Items.Add(new ctlFancyCheckedListBoxItem(attr.Name, false));
                        }
                    }
                }

                for (int i = 0; i < Options.lbAttributes.Items.Count; i++)
                    Options.lbAttributes.SetItemChecked(i, true);
                for (int i = 0; i < Options.lbHierarchies.Items.Count; i++)
                    Options.lbHierarchies.SetItemChecked(i, true);
                Options.tabControl1.SelectedIndex = 0;
                Options.numMinLevels.Value = 0;
                Options.trkActionLevel.Value = 4;

                if (Options.ShowDialog() == DialogResult.OK)
                {
                    ProjectItem piDsv = null;
                    foreach (ProjectItem piTemp in pi.ContainingProject.ProjectItems)
                    {
                        if (piTemp.Object == dim.DataSourceView)
                        {
                            piDsv = piTemp;
                            break;
                        }
                    }

                    //close all project windows
                    foreach (ProjectItem piTemp in pi.ContainingProject.ProjectItems)
                    {
                        bool bIsOpen = piTemp.get_IsOpen(BIDSViewKinds.Designer);
                        if (bIsOpen)
                        {
                            Window win = piTemp.Open(BIDSViewKinds.Designer);
                            win.Close(vsSaveChanges.vsSaveChangesYes);
                        }
                    }

                    Microsoft.DataWarehouse.Design.DataSourceConnection openedDataSourceConnection = GetOpenedDataSourceConnection(dim.DataSource);
                    Program.ASFlattener.db = Program.ASFlattener.dim.Parent;
                    Program.ASFlattener.DataSourceConnection = openedDataSourceConnection;

                    Program.SQLFlattener = null;

                    Microsoft.DataWarehouse.VsIntegration.Shell.Project.IFileProjectHierarchy projectService = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.IFileProjectHierarchy)((System.IServiceProvider)pi.ContainingProject).GetService(typeof(Microsoft.DataWarehouse.VsIntegration.Shell.Project.IFileProjectHierarchy));
                    Microsoft.DataWarehouse.Interfaces.IConfigurationSettings settings = (Microsoft.DataWarehouse.Interfaces.IConfigurationSettings)((System.IServiceProvider)pi.ContainingProject).GetService(typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings));
                    Microsoft.DataWarehouse.Project.DataWarehouseProjectManager projectManager = (Microsoft.DataWarehouse.Project.DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);

                    Dimension dimNew = dim.Clone();
                    dimNew.Name = dim.Name + "_Naturalized";
                    dimNew.ID = dimNew.Name;

                    string sNewDimProjectItemName = dimNew.Name + ".dim";
                    if (dim.ParentServer != null)
                    {
                        sNewDimProjectItemName = dimNew.Name;
                    }
                    if (dim.Parent.Dimensions.ContainsName(dimNew.Name))
                    {
                        projectManager.GetProjectItemFromName(sNewDimProjectItemName).Delete(); //deletes the project item and the dimension from the AMO dimensions collection
                    }

                    if (dim.ParentServer == null)
                    {
                        string sFullPath = pi.get_FileNames(0);
                        sFullPath = sFullPath.Substring(0, sFullPath.Length - System.IO.Path.GetFileName(sFullPath).Length) + sNewDimProjectItemName;
                        XmlWriter writer = new System.Xml.XmlTextWriter(sFullPath, Encoding.UTF8);
                        Microsoft.AnalysisServices.Utils.Serialize(writer, dimNew, false);
                        writer.Close();

                        Microsoft.DataWarehouse.VsIntegration.Shell.Project.IFileProjectNode parentNode = null;
                        Microsoft.DataWarehouse.VsIntegration.Shell.Project.IFileProjectNode projNode = projectManager.CreateFileProjectNode(ref parentNode, 1, sNewDimProjectItemName, sFullPath, 0, 0);

                        projectService.Add(projNode, parentNode);
                        ((Microsoft.DataWarehouse.VsIntegration.Shell.Project.ComponentModel.IFileProjectComponentManager)projectManager).UpdateComponentModel(Microsoft.DataWarehouse.VsIntegration.Shell.Project.ComponentModel.UpdateOperationType.AddObject, projNode);
                    }
                    else
                    {
                        dim.Parent.Dimensions.Add(dimNew);
                    }

                    Program.Progress = new frmProgress();
                    Program.Progress.ShowDialog(); // The Progress form actually launches the naturalizer...

                    if (piDsv != null && dim.ParentServer == null)
                    {
                        Window winDesigner = piDsv.Open(BIDSViewKinds.Designer);
                        winDesigner.Activate();
                        System.ComponentModel.Design.IDesignerHost host = (System.ComponentModel.Design.IDesignerHost)(winDesigner.Object);
                        Microsoft.AnalysisServices.Design.DataSourceDesigner designer = (Microsoft.AnalysisServices.Design.DataSourceDesigner)host.GetDesigner(dim.DataSourceView);
                        designer.MakeDesignerDirty();
                    }

                    ProjectItem piNew = projectManager.GetProjectItemFromName(sNewDimProjectItemName);
                    if (dim.ParentServer == null)
                    {
                        piNew.Save(null);
                        //piNew.ContainingProject.Save(null); //didn't work
                    }
                    else
                    {
                        //already processed inside the ASPCDimNaturalizer code
                    }

                    Window winNew = piNew.Open(BIDSViewKinds.Designer);
                    winNew.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void ExecDSV(ProjectItem pi)
        {
            try
            {
                //close all project windows
                foreach (ProjectItem piTemp in pi.ContainingProject.ProjectItems)
                {
                    bool bIsOpen = piTemp.get_IsOpen(BIDSViewKinds.Designer);
                    if (bIsOpen)
                    {
                        Window win = piTemp.Open(BIDSViewKinds.Designer);
                        win.Close(vsSaveChanges.vsSaveChangesYes);
                    }
                }

                DataSourceView dsv = pi.Object as DataSourceView;

                Microsoft.DataWarehouse.Design.DataSourceConnection openedDataSourceConnection = GetOpenedDataSourceConnection(dsv.DataSource);

                Program.SQLFlattener = new frmSQLFlattener();
                Program.SQLFlattener.txtServer.Text = openedDataSourceConnection.DataSource;
                Program.SQLFlattener.cmbDatabase.Items[0] = openedDataSourceConnection.Database;
                Program.SQLFlattener.Conn = (System.Data.OleDb.OleDbConnection)openedDataSourceConnection.ConnectionObject;
                Program.SQLFlattener.cmbTable.Items[0] = string.Empty;
                Program.SQLFlattener.cmbID.Items[0] = string.Empty;
                Program.SQLFlattener.cmbPID.Items[0] = string.Empty;
                Program.SQLFlattener.dsv = dsv;
                Program.SQLFlattener.DataSourceConnection = openedDataSourceConnection;

                Program.ASFlattener = null;

                if (Program.SQLFlattener.ShowDialog() == DialogResult.OK)
                {
                    Window winDesigner = pi.Open(BIDSViewKinds.Designer);
                    winDesigner.Activate();
                    System.ComponentModel.Design.IDesignerHost host = (System.ComponentModel.Design.IDesignerHost)(winDesigner.Object);
                    Microsoft.AnalysisServices.Design.DataSourceDesigner designer = (Microsoft.AnalysisServices.Design.DataSourceDesigner)host.GetDesigner(dsv);
                    designer.MakeDesignerDirty();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private Microsoft.DataWarehouse.Design.DataSourceConnection GetOpenedDataSourceConnection(DataSource dataSource)
        {
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
                return null;
            }
            else if (!(openedDataSourceConnection.ConnectionObject is System.Data.OleDb.OleDbConnection))
            {
                MessageBox.Show("Data source [" + dataSource.Name + "] is not an OLEDB connection. Only OLEDB connections are supported currently.");
                return null;
            }

            return openedDataSourceConnection;
        }
    }
}
