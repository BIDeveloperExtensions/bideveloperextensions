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
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
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
                //Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)
                EnvDTE.Project projExt = pi.ContainingProject;

                return (projExt.Kind == BIDSProjectKinds.SSAS); //only show in an SSAS project, not in a report model or SSIS project (which also can have a DSV)
            }
            catch (Exception ex)
            {
                package.Log.Exception("Error in UnusedColumnsPlugin.ShouldDisplayCommand", ex);
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
                    foreach (DataSourceView o in dsvs)
                    {
                        results = DsvHelpers.IterateDsvColumns(o);
                    }
#else
                    if (sandbox.IsTabularMetadata)
                    {
                        Microsoft.AnalysisServices.BackEnd.EditMappingUtility util = new Microsoft.AnalysisServices.BackEnd.EditMappingUtility(sandbox);
                        results = new DsvColumnResult(null);
                        TabularHelpers.EnsureDataSourceCredentials(sandbox);

                        foreach (Microsoft.AnalysisServices.BackEnd.DataModelingTable table in sandbox.Tables)
                        {
                            if (table.IsCalculated || table.IsPushedData) continue;
                            if (table.IsStructuredDataSource)
                            {
                                MessageBox.Show("BI Developer Extensions does not yet support modern (Power Query) data sources.", "BI Developer Extensions");
                                return;
                            }

                            //new 1200 models don't appear to have an equivalent of the DSV where the list of columns from the SQL query are cached, so we will have to get the columns from executing (schema only) the SQL query
                            var conn = ((Microsoft.AnalysisServices.BackEnd.RelationalDataStorage)((util.GetDataSourceConnection(util.GetDataSourceID(table.Id), sandbox)))).DataSourceConnection;
                            conn.Open();
                            System.Data.Common.DbCommand cmd = conn.CreateCommand();
                            cmd.CommandText = sandbox.GetSourceQueryDefinition(table.Id);
                            cmd.CommandTimeout = 0;
                            cmd.Prepare();
                            System.Data.Common.DbDataReader reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
                            DataTable tbl = reader.GetSchemaTable();
                            for (int i = 0; i < tbl.Rows.Count; i++)
                            {
                                string sColumnName = Convert.ToString(tbl.Rows[i]["ColumnName"]);
                                Type oDataType = (Type)tbl.Rows[i]["DataType"];
                                bool bFound = false;
                                foreach (Microsoft.AnalysisServices.BackEnd.DataModelingColumn col in table.Columns)
                                {
                                    if (col.IsCalculated || col.IsRowNumber) continue;
                                    if (sColumnName == col.DBColumnName)
                                    {
                                        bFound = true;
                                        break;
                                    }
                                }
                                if (!bFound)
                                {
                                    DataTable t = new DataTable(table.Name);
                                    DataColumn c = t.Columns.Add(sColumnName, oDataType);

                                    results.UnusedColumns.Add("[" + t.TableName + "].[" + sColumnName + "]", new UnusedColumn(c, null));
                                }
                            }
                        }
                    }
                    else //AMO Tabular
                    {
                        DataSourceViewCollection dsvs = sandbox.AMOServer.Databases[sandbox.DatabaseID].DataSourceViews;
                        foreach (DataSourceView o in dsvs)
                        {
                            results = DsvHelpers.IterateDsvColumns(o);
                        }
                    }
#endif
                }
                else
                {
                    DataSourceView dsv = (DataSourceView)projItem.Object;
                    results = DsvHelpers.IterateDsvColumns(dsv);
                }

                if (results == null || results.UnusedColumns == null || results.UnusedColumns.Count == 0)
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
                get {
                    if (m_dsv != null)
                        return m_dsv.Name;
                    else
                        return string.Empty;
                }
            }

            public string DatabaseName
            {
                get {
                    if (m_dsv != null)
                        return m_dsv.Parent.Name;
                    else
                        return string.Empty;
                }
            }
        }

        public class UsedColumn
        {
            private DataItem m_dataItem;
            private ColumnBinding m_column;
            private DataSourceView m_dsv;
            private string m_usageType;
            private string m_usageObjectName;
            private Microsoft.AnalysisServices.BackEnd.DataModelingColumn m_tomColumn;
            public UsedColumn(DataItem di, ColumnBinding column, DataSourceView dsv, string usageType, string usageObjectName)
            {
                m_dataItem = di;
                m_column = column;
                m_dsv = dsv;
                m_usageType = usageType;
                m_usageObjectName = usageObjectName;
            }
            public UsedColumn(Microsoft.AnalysisServices.BackEnd.DataModelingColumn column)
            {
                m_tomColumn = column;
                m_usageType = "Column";
            }

            public string TableName
            {
                get
                {
                    if (m_tomColumn != null)
                    {
                        return m_tomColumn.Table.SourceTableName;
                    }
                    else
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
            }

            public bool IsInvalidBinding
            {
                get
                {
                    if (m_tomColumn != null)
                        return false;
                    else
                        return (m_dsv.Schema.Tables[m_column.TableID] == null || m_dsv.Schema.Tables[m_column.TableID].Columns[m_column.ColumnID] == null);
                }
            }

            public string ColumnName
            {
                get {
                    if (m_tomColumn != null)
                        return m_tomColumn.DBColumnName;
                    else if (IsInvalidBinding)
                        return m_column.ColumnID;
                    else
                        return m_dsv.Schema.Tables[m_column.TableID].Columns[m_column.ColumnID].ColumnName;
                }
            }

            public string DataTypeName
            {
                get
                {
                    if (m_tomColumn != null)
                        return Microsoft.AnalysisServices.OleDbTypeConverter.Convert(m_tomColumn.DataType).Name;
                    else if (IsInvalidBinding)
                        return null;
                    else
                        return m_dsv.Schema.Tables[m_column.TableID].Columns[m_column.ColumnID].DataType.Name;
                }
            }

            public string BindingDataTypeName
            {
                get
                {
                    if (m_tomColumn != null)
                        return string.Empty;
                    else
                        return m_dataItem.DataType.ToString();
                }
            }

            public string dsvName
            {
                get
                {
                    if (m_tomColumn != null)
                        return string.Empty;
                    else
                        return m_dsv.Name;
                }
            }

            public string DatabaseName
            {
                get
                {
                    if (m_tomColumn != null)
                        return m_tomColumn.Table.Sandbox.DatabaseName;
                    else
                        return m_dsv.Parent.Name;
                }
            }

            public string UsageType
            {
                get {
                    return m_usageType;
                }
            }

            public string UsageObjectName
            {
                get
                {
                    if (m_tomColumn != null)
                    {
                        if (m_tomColumn.Table.Name.Contains(" "))
                            return "'" + m_tomColumn.Table.Name + "'[" + m_tomColumn.Name + "]";
                        else
                            return m_tomColumn.Table.Name + "[" + m_tomColumn.Name + "]";
                    }
                    else
                        return m_usageObjectName;
                }
            }
        }
    }

    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
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
                //Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)pi.ContainingProject;
                EnvDTE.Project projExt = pi.ContainingProject;

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
                    foreach (DataSourceView o in dsvs)
                    {
                        results = DsvHelpers.IterateDsvColumns(o);
                    }
#else
                    if (sandbox.IsTabularMetadata)
                    {
                        Microsoft.AnalysisServices.BackEnd.EditMappingUtility util = new Microsoft.AnalysisServices.BackEnd.EditMappingUtility(sandbox);
                        results = new DsvColumnResult(null);

                        foreach (Microsoft.AnalysisServices.BackEnd.DataModelingTable table in sandbox.Tables)
                        {
                            if (table.IsCalculated || table.IsPushedData) continue;
                            if (table.IsStructuredDataSource)
                            {
                                MessageBox.Show("BI Developer Extensions does not yet support modern (Power Query) data sources.", "BI Developer Extensions");
                                return;
                            }

                            foreach (Microsoft.AnalysisServices.BackEnd.DataModelingColumn col in table.Columns)
                            {
                                if (col.IsRowNumber || col.IsCalculated) continue;
                                results.UsedColumns.Add(new UnusedColumnsPlugin.UsedColumn(col));
                            }
                        }
                    }
                    else //AMO Tabular
                    {
                        DataSourceViewCollection dsvs = sandbox.AMOServer.Databases[sandbox.DatabaseID].DataSourceViews;
                        foreach (DataSourceView o in dsvs)
                        {
                            results = DsvHelpers.IterateDsvColumns(o);
                        }
                    }
#endif
                }
                else
                {
                    DataSourceView dsv = (DataSourceView)projItem.Object;
                    results = DsvHelpers.IterateDsvColumns(dsv);
                }


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
