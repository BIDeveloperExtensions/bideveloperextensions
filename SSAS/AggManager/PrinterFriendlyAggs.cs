using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AnalysisServices;

namespace AggManager
{
    public class PrinterFriendlyAggs
    {
        public static void ShowAggsReport(AggregationDesign aggDesign, string sCorrectAggregationDesignName)
        {
            List<Agg> aggs = ListAggs(aggDesign, sCorrectAggregationDesignName, aggDesign.ParentCube.Name + " - " + aggDesign.Parent.Name + " measure group");
            ShowWarningsReport(aggs);
        }

        public static void ShowAggsReport(Cube cube)
        {
            List<Agg> aggs = new List<Agg>();
            foreach (MeasureGroup mg in cube.MeasureGroups)
            {
                if (mg.IsLinked) continue;
                foreach (AggregationDesign aggDesign in mg.AggregationDesigns)
                {
                    aggs.AddRange(ListAggs(aggDesign, aggDesign.Name, null));
                }
            }
            ShowWarningsReport(aggs);
        }

        private static void ShowWarningsReport(List<Agg> aggs)
        {
            if (aggs.Count > 0)
            {
                BIDSHelper.ReportViewerForm frm = new BIDSHelper.ReportViewerForm();
                frm.ReportBindingSource.DataSource = aggs;
                frm.Report = "SSAS.AggManager.PrinterFriendlyAggs.rdlc";
                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "AggManager_Agg";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = frm.Report;

                frm.Caption = "Aggregations Report";
                frm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                frm.Show();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("No aggregations found.");
            }
        }

        private static List<Agg> ListAggs(AggregationDesign aggDesign, string sCorrectAggregationDesignName, string sReportTitle)
        {
            List<Agg> aggs = new List<Agg>();
            foreach (Aggregation agg in aggDesign.Aggregations)
            {
                aggs.Add(new Agg(agg, sCorrectAggregationDesignName, sReportTitle));
            }
            return aggs;
        }


        public class Agg
        {
            public Agg(Aggregation agg, string sCorrectAggregationDesignName, string sReportTitle)
            {
                mAggName = agg.Name;
                mAggDesignName = sCorrectAggregationDesignName;
                mMeasureGroupName = agg.ParentMeasureGroup.Name;
                mCubeNameOrReportTitle = (sReportTitle == null ? agg.ParentCube.Name : sReportTitle);
                mDatabaseName = agg.ParentDatabase.Name;
                mAttributes = string.Empty;
                foreach (AggregationDimension ad in agg.Dimensions)
                {
                    foreach (AggregationAttribute aa in ad.Attributes)
                    {
                        if (!string.IsNullOrEmpty(mAttributes)) mAttributes += "\r\n";
                        mAttributes += "[" + ad.CubeDimension.Name + "].[" + aa.Attribute.Name + "]";
                    }
                }
            }

            private string mAttributes;
            private string mDatabaseName;
            private string mCubeNameOrReportTitle;
            private string mMeasureGroupName;
            private string mAggDesignName;
            private string mAggName;

            public string Attributes
            {
                get { return mAttributes; }
            }

            public string AggName
            {
                get { return mAggName; }
            }

            public string AggDesignName
            {
                get { return mAggDesignName; }
            }

            public string CubeNameOrReportTitle
            {
                get { return mCubeNameOrReportTitle; }
            }

            public string MeasureGroupName
            {
                get { return mMeasureGroupName; }
            }

            public string DatabaseName
            {
                get { return mDatabaseName; }
            }
        }
    }
}
