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
    public class DimensionOptimizationReportPlugin : BIDSHelperPluginBase
    {
        public DimensionOptimizationReportPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "DimensionOptimizationReportPlugin"; }
        }

        public override string FriendlyName
        {
            get { return "Dimension Optimization Report"; }
        }
        
        public override int Bitmap
        {
            get { return 44; }
        }

        public override string ButtonText
        {
            get { return "Dimension Optimization Report..."; }
        }

        public override string ToolTip
        {
            get { return ""; } //not used anywhere
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        public override string MenuName
        {
            get { return "Folder Node"; }
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
                return (hierItem.Name == "Dimensions" && ((ProjectItem)hierItem.Object).Object == null);
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
                Database db = projItem.ContainingProject.Object as Database;
                List<DimensionOptimizations> list = GetDimensionOptimizations(db);

                ReportViewerForm frm = new ReportViewerForm();
                frm.ReportBindingSource.DataSource = list;
                frm.Report = "SSAS.DimensionOptimization.rdlc";
                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "BIDSHelper_DimensionOptimizations";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = frm.Report;

                frm.Caption = "Dimension Optimizations Report";
                frm.WindowState = FormWindowState.Maximized;
                frm.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<DimensionOptimizations> GetDimensionOptimizations(Database db)
        {
            List<DimensionOptimizations> optimizations = new List<DimensionOptimizations>();
            List<string> listCubeDimensions = new List<string>();
            foreach (Cube c in db.Cubes)
            {
                foreach (CubeDimension cd in c.Dimensions)
                {
                    foreach (CubeAttribute ca in cd.Attributes)
                    {
                        if (!listCubeDimensions.Contains(cd.Name))
                        {
                            optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ca.Attribute.Name, " Dimension Properties ", "Estimated Count", ca.Attribute.EstimatedCount.ToString("n0")));
                            optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ca.Attribute.Name, " Dimension Properties ", "Enabled", ca.Attribute.AttributeHierarchyEnabled.ToString()));
                            optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ca.Attribute.Name, " Dimension Properties ", "Optimized State", ca.Attribute.AttributeHierarchyOptimizedState.ToString()));
                            optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ca.Attribute.Name, " Dimension Properties ", "Ordered", ca.Attribute.AttributeHierarchyOrdered.ToString()));
                            optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ca.Attribute.Name, " Dimension Properties ", "Visible", ca.Attribute.AttributeHierarchyVisible.ToString()));
                        }
                        
                        optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ca.Attribute.Name, c.Name, "Aggregation Usage", ca.AggregationUsage.ToString()));
                        optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ca.Attribute.Name, c.Name, "(Effective) Enabled", (ca.AttributeHierarchyEnabled && ca.Attribute.AttributeHierarchyEnabled).ToString()));
                        
                        OptimizationType effectiveOptimization = ca.AttributeHierarchyOptimizedState;
                        if (ca.Attribute.AttributeHierarchyOptimizedState == OptimizationType.NotOptimized) effectiveOptimization = OptimizationType.NotOptimized; //the cube attribute setting can only override if the dimension attribute setting is FullyOptimized
                        optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ca.Attribute.Name, c.Name, "(Effective) Optimized State", effectiveOptimization.ToString()));

                        optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ca.Attribute.Name, c.Name, "(Effective) Visible", (ca.AttributeHierarchyVisible && ca.Attribute.AttributeHierarchyVisible).ToString()));
                    }
                    foreach (CubeHierarchy ch in cd.Hierarchies)
                    {
                        optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ch.Hierarchy.Name, c.Name, "(Effective) Enabled", ch.Enabled.ToString()));
                        optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ch.Hierarchy.Name, c.Name, "(Effective) Optimized State", ch.OptimizedState.ToString()));
                        optimizations.Add(new DimensionOptimizations(db.Name, cd.Name, ch.Hierarchy.Name, c.Name, "(Effective) Visible", ch.Visible.ToString()));
                    }
                    listCubeDimensions.Add(cd.Name);
                }
            }
            return optimizations;
        }

        public class DimensionOptimizations
        {
            public DimensionOptimizations(string DatabaseName, string DimensionName, string AttributeName, string CubeName, string PropertyName, string Value)
            {
                mDatabaseName = DatabaseName;
                mDimensionName = DimensionName;
                mAttributeName = AttributeName;
                mCubeName = CubeName;
                mPropertyName = PropertyName;
                mValue = Value;
            }

            private string mDatabaseName;
            public string DatabaseName
            {
                get { return mDatabaseName; }
            }

            private string mDimensionName;
            public string DimensionName
            {
                get { return mDimensionName; }
            }

            private string mAttributeName;
            public string AttributeName
            {
                get { return mAttributeName; }
            }

            private string mCubeName;
            public string CubeName
            {
                get { return mCubeName; }
            }

            private string mPropertyName;
            public string PropertyName
            {
                get { return mPropertyName; }
            }

            private string mValue;
            public string Value
            {
                get { return mValue; }
            }
        }
    }
}
