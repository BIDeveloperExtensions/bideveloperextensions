using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using System.Data;
using BIDSHelper.Core;

namespace BIDSHelper.SSAS
{
    public class UnusedColumnsPlugin : BIDSHelperPluginBase
    {
        public UnusedColumnsPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.UnusedColumnsReportId);
        }

        public override string ShortName
        {
            get { return "UnusedColumns"; }
        }

        //public override int Bitmap
        //{
        //    get { return 543; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Unused Columns Report..."; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
        //}

        public override string FeatureName
        {
            get { return "Unused Columns Report"; }
        }

        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        public override string HelpUrl
        {
            get { return this.GetCodePlexHelpUrl("Column Usage Reports"); }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASMulti; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "This report lists all columns in the DSV which are not used in dimensions, cubes, or mining structures."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ShouldDisplayCommand()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                if (sFileName.EndsWith(".bim")) return true; //show the menu if this is the .bim file of a Tabular model, but don't show the Used Columns Report for Tabular since the data source view isn't really something a Tabular developer manages

                ProjectItem pi = (ProjectItem)hierItem.Object;
                if (!(pi.Object is DataSourceView)) return false;
                Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)pi.ContainingProject;

                return (projExt.Kind == BIDSProjectKinds.SSAS); //only show in an SSAS project, not in a report model or SSIS project (which also can have a DSV)
            }
            catch
            {
                return false;
            }
        }


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                DsvColumnResult results = null;
                string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();

                if (sFileName.EndsWith(".bim"))
                {
                    Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = TabularHelpers.GetTabularSandboxFromBimFile(this, true);
#if DENALI || SQL2014
                    DataSourceViewCollection dsvs = sandbox.Database.DataSourceViews;
#else
                    DataSourceViewCollection dsvs = sandbox.AMOServer.Databases[0].DataSourceViews;
#endif
                    foreach (DataSourceView o in dsvs)
                    {
                        results = DsvHelpers.IterateDsvColumns(o);
                    }
                }
                else
                {
                    DataSourceView dsv = (DataSourceView)projItem.Object;
                    results = DsvHelpers.IterateDsvColumns(dsv);
                }

                if (results.UnusedColumns.Count == 0)
                {
                    MessageBox.Show("There are no unused columns.", "BIDS Helper - Unused Columns Report");
                    return;
                }

                ReportViewerForm frm = new ReportViewerForm();
                frm.ReportBindingSource.DataSource = results.UnusedColumns.Values;
                frm.Report = "SSAS.UnusedColumns.rdlc";
                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "BIDSHelper_UnusedColumn";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = "SSAS.UnusedColumns.rdlc";

                frm.Caption = "Unused Columns Report";
                frm.WindowState = FormWindowState.Maximized;
                frm.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //protected Dictionary<string, UnusedColumn> unusedColumns = new Dictionary<string, UnusedColumn>();
        //protected List<UsedColumn> usedColumns = new List<UsedColumn>();
        //private DataSourceView m_dsv;

        


        public class UnusedColumn
        {
            private DataColumn m_column;
            private DataSourceView m_dsv;
            public UnusedColumn(DataColumn column, DataSourceView dsv)
            {
                m_column = column;
                m_dsv = dsv;
            }

            public string TableName
            {
                get
                {
                    if (m_column.Table.ExtendedProperties.ContainsKey("FriendlyName"))
                    {
                        return m_column.Table.ExtendedProperties["FriendlyName"].ToString();
                    }
                    else
                    {
                        return m_column.Table.TableName;
                    }
                }
            }

            public string ColumnName
            {
                get { return m_column.ColumnName; }
            }

            public string DataTypeName
            {
                get { return m_column.DataType.Name; }
            }

            public string dsvName
            {
                get { return m_dsv.Name; }
            }

            public string DatabaseName
            {
                get { return m_dsv.Parent.Name; }
            }
        }

        public class UsedColumn
        {
            private DataItem m_dataItem;
            private ColumnBinding m_column;
            private DataSourceView m_dsv;
            private string m_usageType;
            private string m_usageObjectName;
            public UsedColumn(DataItem di, ColumnBinding column, DataSourceView dsv, string usageType, string usageObjectName)
            {
                m_dataItem = di;
                m_column = column;
                m_dsv = dsv;
                m_usageType = usageType;
                m_usageObjectName = usageObjectName;
            }

            public string TableName
            {
                get
                {
                    if (m_dsv.Schema.Tables[m_column.TableID] != null)
                    {
                        if (m_dsv.Schema.Tables[m_column.TableID].ExtendedProperties.ContainsKey("FriendlyName"))
                        {
                            return m_dsv.Schema.Tables[m_column.TableID].ExtendedProperties["FriendlyName"].ToString();
                        }
                        else
                        {
                            return m_dsv.Schema.Tables[m_column.TableID].TableName;
                        }
                    }
                    else
                    {
                        return m_column.TableID;
                    }
                }
            }

            public bool IsInvalidBinding
            {
                get
                {
                    return (m_dsv.Schema.Tables[m_column.TableID] == null || m_dsv.Schema.Tables[m_column.TableID].Columns[m_column.ColumnID] == null);
                }
            }

            public string ColumnName
            {
                get {
                    if (IsInvalidBinding)
                        return m_column.ColumnID;
                    else
                        return m_dsv.Schema.Tables[m_column.TableID].Columns[m_column.ColumnID].ColumnName;
                }
            }

            public string DataTypeName
            {
                get
                {
                    if (IsInvalidBinding)
                        return null;
                    else
                        return m_dsv.Schema.Tables[m_column.TableID].Columns[m_column.ColumnID].DataType.Name;
                }
            }

            public string BindingDataTypeName
            {
                get { return m_dataItem.DataType.ToString(); }
            }

            public string dsvName
            {
                get { return m_dsv.Name; }
            }

            public string DatabaseName
            {
                get { return m_dsv.Parent.Name; }
            }

            public string UsageType
            {
                get { return m_usageType; }
            }

            public string UsageObjectName
            {
                get { return m_usageObjectName; }
            }
        }
    }

    public class UsedColumnsPlugin : BIDSHelperPluginBase
    {
        public UsedColumnsPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.UsedColumnsReportId);
        }

        public override string ShortName
        {
            get { return "UsedColumns"; }
        }

        //public override int Bitmap
        //{
        //    get { return 723; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Used Columns Report..."; }
        //}

        public override string FeatureName
        {
            get { return "Used Columns Report"; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "This report lists all columns in the DSV which are used in dimensions, cubes, or mining structures. This report can be used for proof reading the setup of your cube or for documentation."; }
        }

        public override BIDSFeatureCategories FeatureCategory
        {
            get
            {
                return BIDSFeatureCategories.SSASMulti;
            }
        }

        public override string ToolTip {  get { return string.Empty; } }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ShouldDisplayCommand()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                if (sFileName.EndsWith(".bim")) return true; //show the menu if this is the .bim file of a Tabular model, but don't show the Used Columns Report for Tabular since the data source view isn't really something a Tabular developer manages

                ProjectItem pi = (ProjectItem)hierItem.Object;
                if (!(pi.Object is DataSourceView)) return false;
                Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)pi.ContainingProject;

                return (projExt.Kind == BIDSProjectKinds.SSAS); //only show in an SSAS project, not in a report model or SSIS project (which also can have a DSV)
            }
            catch
            {
                return false;
            }
        }


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                DataSourceView dsv = (DataSourceView)projItem.Object;
                DsvColumnResult results = DsvHelpers.IterateDsvColumns(dsv);

                ReportViewerForm frm = new ReportViewerForm();
                frm.ReportBindingSource.DataSource = results.UsedColumns;
                frm.Report = "SSAS.UsedColumns.rdlc";
                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "BIDSHelper_UsedColumn";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = "SSAS.UsedColumns.rdlc";

                frm.Caption = "Used Columns Report";
                frm.WindowState = FormWindowState.Maximized;
                frm.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
