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
using System.Data;

namespace BIDSHelper
{
    public class UnusedColumnsPlugin : BIDSHelperPluginBase
    {
        public UnusedColumnsPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "UnusedColumns"; }
        }

        public override int Bitmap
        {
            get { return 543; }
        }

        public override string ButtonText
        {
            get { return "Unused Columns Report..."; }
        }

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

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
            get { return BIDSFeatureCategories.SSAS; }
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
        }


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                DataSourceView dsv = (DataSourceView)projItem.Object;
                IterateDsvColumns(dsv);

                ReportViewerForm frm = new ReportViewerForm();
                frm.ReportBindingSource.DataSource = unusedColumns.Values;
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

        protected Dictionary<string, UnusedColumn> unusedColumns = new Dictionary<string, UnusedColumn>();
        protected List<UsedColumn> usedColumns = new List<UsedColumn>();
        private DataSourceView m_dsv;

        protected void IterateDsvColumns(DataSourceView dsv)
        {
            m_dsv = dsv;

            //add all DSV columns to a list
            unusedColumns.Clear();
            usedColumns.Clear();
            foreach (DataTable t in dsv.Schema.Tables)
            {
                foreach (DataColumn c in t.Columns)
                {
                    unusedColumns.Add("[" + t.TableName + "].[" + c.ColumnName + "]", new UnusedColumn(c, dsv));
                }
            }

            //remove columns that are used in dimensions
            foreach (Dimension dim in dsv.Parent.Dimensions)
            {
                if (dim.DataSourceView != null && dim.DataSourceView.ID == dsv.ID)
                {
                    foreach (DimensionAttribute attr in dim.Attributes)
                    {
                        foreach (DataItem di in attr.KeyColumns)
                        {
                            ProcessDataItemInLists(di, "Dimension Attribute Key");
                        }
                        ProcessDataItemInLists(attr.NameColumn, "Dimension Attribute Name");
                        ProcessDataItemInLists(attr.ValueColumn, "Dimension Attribute Value");
                        ProcessDataItemInLists(attr.UnaryOperatorColumn, "Dimension Attribute Unary Operator");
                        ProcessDataItemInLists(attr.SkippedLevelsColumn, "Dimension Attribute Skipped Levels");
                        ProcessDataItemInLists(attr.CustomRollupColumn, "Dimension Attribute Custom Rollup");
                        ProcessDataItemInLists(attr.CustomRollupPropertiesColumn, "Dimension Attribute Custom Rollup Properties");
                        foreach (AttributeTranslation tran in attr.Translations)
                        {
                            ProcessDataItemInLists(tran.CaptionColumn, "Dimension Attribute Translation");

                        }
                    }
                }
            }

            foreach (Cube cube in dsv.Parent.Cubes)
            {
                if (cube.DataSourceView != null && cube.DataSourceView.ID == dsv.ID)
                {
                    foreach (MeasureGroup mg in cube.MeasureGroups)
                    {
                        //remove columns that are used in measures
                        foreach (Measure m in mg.Measures)
                        {
                            ProcessDataItemInLists(m.Source, "Measure");
                        }

                        //remove columns that are used in dimension relationships
                        foreach (MeasureGroupDimension mgdim in mg.Dimensions)
                        {
                            if (mgdim is ManyToManyMeasureGroupDimension)
                            {
                                //no columns to remove
                            }
                            else if (mgdim is DataMiningMeasureGroupDimension)
                            {
                                //no columns to remove
                            }
                            else if (mgdim is RegularMeasureGroupDimension)
                            {
                                //Degenerate dimensions and Reference dimensions
                                RegularMeasureGroupDimension regMDdim = (RegularMeasureGroupDimension)mgdim;
                                foreach (MeasureGroupAttribute mga in regMDdim.Attributes)
                                {
                                    if (mga.Type == MeasureGroupAttributeType.Granularity)
                                    {
                                        foreach (DataItem di3 in mga.KeyColumns)
                                        {
                                            ProcessDataItemInLists(di3, "Fact Table Dimension Key");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //remove mining structure columns
            foreach (MiningStructure structure in dsv.Parent.MiningStructures)
            {
                if (structure.DataSourceView != null && structure.DataSourceView.ID == dsv.ID)
                    RecurseMiningStructureColumnsAndProcessDataItemInLists(structure.Columns);
            }
        }

        private void ProcessDataItemInLists(DataItem di, string usageType)
        {
            if (di == null) return;
            ColumnBinding cb = di.Source as ColumnBinding;
            if (cb == null) return;
            string sColUniqueName = "[" + cb.TableID + "].[" + cb.ColumnID + "]";
            if (unusedColumns.ContainsKey(sColUniqueName))
                unusedColumns.Remove(sColUniqueName);
            usedColumns.Add(new UsedColumn(di, cb, m_dsv, usageType, di.Parent.FriendlyPath));
        }

        private void RecurseMiningStructureColumnsAndProcessDataItemInLists(MiningStructureColumnCollection cols)
        {
            foreach (MiningStructureColumn col in cols)
            {
                if (col is ScalarMiningStructureColumn)
                {
                    ScalarMiningStructureColumn scalar = (ScalarMiningStructureColumn)col;
                    foreach (DataItem di in scalar.KeyColumns)
                    {
                        ProcessDataItemInLists(di, "Mining Structure Column Key");
                    }
                    ProcessDataItemInLists(scalar.NameColumn, "Mining Structure Column Name");
                }
                else if (col is TableMiningStructureColumn)
                {
                    TableMiningStructureColumn tblCol = (TableMiningStructureColumn)col;
                    RecurseMiningStructureColumnsAndProcessDataItemInLists(tblCol.Columns);
                }
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

    public class UsedColumnsPlugin : UnusedColumnsPlugin
    {
        public UsedColumnsPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "UsedColumns"; }
        }

        public override int Bitmap
        {
            get { return 723; }
        }

        public override string ButtonText
        {
            get { return "Used Columns Report..."; }
        }

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

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                DataSourceView dsv = (DataSourceView)projItem.Object;
                IterateDsvColumns(dsv);

                ReportViewerForm frm = new ReportViewerForm();
                frm.ReportBindingSource.DataSource = usedColumns;
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
