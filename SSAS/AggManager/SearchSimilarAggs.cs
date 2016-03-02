using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AnalysisServices;

//SearchSimilarAggs code thanks to Leandro Tubia

namespace AggManager
{
    public class SearchSimilarAggs
    {

        public static void ShowAggsSimilaritiesReport(MeasureGroup mg, string sCorrectAggregationDesignName, Boolean bCountMembers)
        {
            AggregationDesign aggDesign = mg.AggregationDesigns.GetByName(sCorrectAggregationDesignName); 
            List<SimilarAgg> aggs = ListSimilarAggs(aggDesign, sCorrectAggregationDesignName, aggDesign.ParentCube.Name + " - " + aggDesign.Parent.Name + " measure group", bCountMembers);
            ShowReport(aggs);
        }

        public static void ShowAggsSimilaritiesReport(Cube cube, Boolean bCountMembers)
        {
            List<SimilarAgg> aggs = new List<SimilarAgg>();
            foreach (MeasureGroup mg in cube.MeasureGroups)
            {
                if (mg.IsLinked) continue;
                foreach (AggregationDesign aggDesign in mg.AggregationDesigns)
                {
                    aggs.AddRange(ListSimilarAggs(aggDesign, aggDesign.Name, null, bCountMembers));
                }
            }
            ShowReport(aggs);
        }

        private static void ShowReport(List<SimilarAgg> aggs)
        {
            if (aggs.Count > 0)
            {
                BIDSHelper.ReportViewerForm frm = new BIDSHelper.ReportViewerForm();
                frm.ReportBindingSource.DataSource = aggs;
                frm.Report = "SSAS.AggManager.SearchSimilarAggs.rdlc";
                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "AggManager_SimilarAgg";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = frm.Report;

                frm.Caption = "Similar Aggregations Report";
                frm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                frm.Show();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("No aggregations found.");
            }
        }

        private static List<SimilarAgg> ListSimilarAggs(AggregationDesign aggDesign, string sCorrectAggregationDesignName, string sReportTitle, Boolean bCountMembers)
        {
            Boolean bIncluded;
            List<SimilarAgg> aggs = new List<SimilarAgg>();
            foreach (Aggregation agg in aggDesign.Aggregations)
            {
                bIncluded = false;
                foreach (Aggregation agg2 in aggDesign.Aggregations)
                {
                    if (agg.Name != agg2.Name)
                    {
                        if (IsAggregationIncluded(agg, agg2, bCountMembers))
                        {
                            aggs.Add(new SimilarAgg(agg, agg2, sCorrectAggregationDesignName, sReportTitle, bCountMembers));
                            bIncluded = true;
                        }
                    }
                }
                if (!bIncluded)
                    aggs.Add(new SimilarAgg(agg, null, sCorrectAggregationDesignName, sReportTitle, bCountMembers));
            }
            return aggs;
        }

        internal static Boolean IsAggregationIncluded(Aggregation agg1, Aggregation agg2, Boolean bCountMembers)
        {
            Boolean bIsAttribute1Included = false;

            foreach (MeasureGroupDimension mgDim in agg1.ParentMeasureGroup.Dimensions)
            {
                AggregationDimension dim1 = agg1.Dimensions.Find(mgDim.CubeDimensionID);
                AggregationDimension dim2 = agg2.Dimensions.Find(mgDim.CubeDimensionID);

                if ((dim1 == null || dim1.Attributes.Count == 0) && (dim2 == null || dim2.Attributes.Count == 0))
                    // both at the All level... continue
                    continue;

                if ((dim1 != null && dim1.Attributes.Count > 0) && (dim2 == null || dim2.Attributes.Count == 0))
                    // dim2 aggregates at All level so it's not possible it being more granular than dim1, 
                    // then agg2 cannot contain agg1
                    return false;

                if ((dim1 == null || dim1.Attributes.Count == 0) && (dim2 != null && dim2.Attributes.Count > 0))
                    // dim1 aggregates at All level so it's probable that all aggregation being included in dim2, 
                    // but still have to evaluate the rest of attributes
                    continue;

                if ((dim1 != null && dim1.Attributes.Count > 0) && (dim2 != null && dim2.Attributes.Count > 0))
                    // both dim1 and dim2 have aggregations at lower level than All, so they need to be evaluated
                {

                    // For both Dim1 and Dim2 attributes, purge those attributes that are redundant
                    AggregationDimension dim1Purged = RemoveRedundantAttributes(dim1);
                    AggregationDimension dim2Purged = RemoveRedundantAttributes(dim2);


                    foreach (AggregationAttribute att1 in dim1Purged.Attributes)
                    {
                        if (dim2Purged.Attributes.Contains(att1.AttributeID))
                            continue;

                        Boolean bExistsAttributeInSameTree = false;
                        foreach (AggregationAttribute att2 in dim2Purged.Attributes)
                        {

                            bIsAttribute1Included = IsRedundantAttribute(agg1.ParentMeasureGroup, dim1.CubeDimensionID, att1.AttributeID, att2.AttributeID, false, -1);
                            //bIsAttribute2Included = IsRedundantAttribute(agg1.ParentMeasureGroup, dim1.CubeDimensionID, att2.AttributeID, att1.AttributeID, false, -1);

                            if (bIsAttribute1Included)
                            // Attribute att1 is included in att2, then if countmembers is turned on
                            // ponderated ratio will be calculated
                            // else go out for another attribute  
                            {
                                if (bCountMembers)
                                {
                                    if (!IsRedundantAttribute(agg1.ParentMeasureGroup, dim1.CubeDimensionID, att1.AttributeID, att2.AttributeID, true, -1))
                                        // If included but member count differ vastly, then report that agg1 is not included in agg2
                                        return false;
                                }
                                else
                                {
                                    bExistsAttributeInSameTree = true;
                                    break;
                                }
                            }

                        }
                        if (!bExistsAttributeInSameTree)
                            // if dim1 does not have attributes in same tree as dim2, then agg1 is not included.
                            return false;

                    }
                }
            }

            // Finally if all dim1 are equal or included in dim2 then aggregation1 is included in aggregation2
            return true;
        }


        private static AggregationDimension RemoveRedundantAttributes(AggregationDimension dim)
        {
            AggregationDimension dimPurged = (AggregationDimension)dim.Clone();

            foreach (AggregationAttribute att1 in dimPurged.Attributes)
            {
                foreach (AggregationAttribute att2 in dimPurged.Attributes)
                {
                    if (att1.AttributeID == att2.AttributeID) break;
                    if (IsRedundantAttribute(dimPurged.ParentMeasureGroup, dimPurged.CubeDimensionID, att1.AttributeID, att2.AttributeID, false, -1))
                    {
                        dimPurged.Attributes.Remove(att1);
                        break;
                    }

                }
            }
            return dimPurged;
        }


        // Compare if attributeA is included in AttributeB
        private static Boolean IsRedundantAttribute(MeasureGroup mg1, string DimId, string AttributeA, string AttributeB, Boolean CompareEstimatedCount, long OriginalEstimatedCount)
        {
            if (mg1 == null) return false;

            long lngMembersDelta;
            float flDeltaRatio;
            float flPonderatedDeltaRatio;
            long lngEstimatedCountToSendRecursively;

            MeasureGroupDimension dimension = mg1.Dimensions.Find(DimId);
            if (dimension == null) return false;

            CubeAttribute cubeattribute = dimension.CubeDimension.Attributes.Find(AttributeB);
            if (cubeattribute == null) return false;

            // Check if AttributeA is included in AttributeB by
            // standing on AttributeB and recursively searching for AttributeA in all its child relationships
            foreach (AttributeRelationship attrRel in cubeattribute.Attribute.AttributeRelationships)
            {
                CubeAttribute childAttr = cubeattribute.Parent.Attributes.Find(attrRel.AttributeID);
                if (attrRel.AttributeID == AttributeA)
                {
                    // if CompareEstimatedCount is turn off, attribute is definitely included
                    if (!CompareEstimatedCount)
                    { return true; }
                    else
                    {
                        // Else calculate the delta between Estimated Counts
                        if (OriginalEstimatedCount > 0)
                        { lngMembersDelta = (OriginalEstimatedCount - childAttr.Attribute.EstimatedCount); }
                        else
                        { lngMembersDelta = (cubeattribute.Attribute.EstimatedCount - childAttr.Attribute.EstimatedCount); }
                        // Calculate the ratio between the delta and the child estimated count.
                        flDeltaRatio = (float)lngMembersDelta / (float)childAttr.Attribute.EstimatedCount;
                        // Ponderate delta ratio by multiplying it by intMembersDelta
                        flPonderatedDeltaRatio = flDeltaRatio * (float)lngMembersDelta;
                        // Testings on different scenarios demostrated that if flPonderatedDeltaRatio > 1000 both attributes have much different cardinality.
                        if (flPonderatedDeltaRatio < 1000)
                        {
                            // if AttributeA is included in AttributeB and besides their cardinality are similar 
                            // then A is definitely included in B.
                            return true;
                        }
                    }
                }
                if (childAttr.Attribute.AttributeRelationships.Count > 0)
                {
                    // If in first iteration, OriginalEstimatedCount is null and base Estimated Count es get from AttributeB
                    // if second or later iteration, it pushes the OriginalEstimatedCount
                    if (OriginalEstimatedCount == -1)
                    { lngEstimatedCountToSendRecursively = cubeattribute.Attribute.EstimatedCount; }
                    else
                    { lngEstimatedCountToSendRecursively = OriginalEstimatedCount; }

                    if (IsRedundantAttribute(mg1, DimId, AttributeA, childAttr.AttributeID, CompareEstimatedCount, lngEstimatedCountToSendRecursively))
                        return true;

                }
            }

            return false;
        }


        public class SimilarAgg
        {
            public SimilarAgg(Aggregation agg, Aggregation similaragg, string sCorrectAggregationDesignName, string sReportTitle, Boolean bCountMembers)
            {
                mAggName = agg.Name;
                string sAggEstimate = AggManager.EditAggs.GetEstimatedAggSizeRange(AggManager.EditAggs.GetEstimatedSize(agg));
                if (sAggEstimate != null) mAggName += " (" + sAggEstimate + ")";

                mAggDesignName = sCorrectAggregationDesignName;
                mMeasureGroupName = agg.ParentMeasureGroup.Name;
                mCubeNameOrReportTitle = (sReportTitle == null ? agg.ParentCube.Name : sReportTitle) + ((bCountMembers ? " (Counting attribute members) " : ""));
                mDatabaseName = agg.ParentDatabase.Name;
                if (similaragg == null)
                {
                    mSimilarAggName = "";
                }
                else
                {
                    mSimilarAggName = similaragg.Name;
                    sAggEstimate = AggManager.EditAggs.GetEstimatedAggSizeRange(AggManager.EditAggs.GetEstimatedSize(similaragg));
                    if (sAggEstimate != null) mSimilarAggName += " (" + sAggEstimate + ")";
                }
            }

            private string mSimilarAggName;
            private string mDatabaseName;
            private string mCubeNameOrReportTitle;
            private string mMeasureGroupName;
            private string mAggDesignName;
            private string mAggName;

            public string SimilarAggName
            {
                get { return mSimilarAggName; }
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
