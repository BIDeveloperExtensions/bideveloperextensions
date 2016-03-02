using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AnalysisServices;

namespace AggManager
{
    public class ValidateAggs
    {
        public static void Validate(AggregationDesign aggDesign, string sCorrectAggregationDesignName)
        {
            List<AggValidationWarning> masterWarnings = CheckAggDesign(aggDesign, sCorrectAggregationDesignName, aggDesign.ParentCube.Name + " - " + aggDesign.Parent.Name + " measure group");
            ShowWarningsReport(masterWarnings);
        }

        public static void Validate(Cube cube)
        {
            List<AggValidationWarning> masterWarnings = new List<AggValidationWarning>();
            foreach (MeasureGroup mg in cube.MeasureGroups)
            {
                if (mg.IsLinked) continue;
                foreach (AggregationDesign aggDesign in mg.AggregationDesigns)
                {
                    masterWarnings.AddRange(CheckAggDesign(aggDesign, aggDesign.Name, null));
                }
            }
            ShowWarningsReport(masterWarnings);
        }

        private static void ShowWarningsReport(List<AggValidationWarning> masterWarnings)
        {
            if (masterWarnings.Count > 0)
            {
                BIDSHelper.ReportViewerForm frm = new BIDSHelper.ReportViewerForm();
                frm.ReportBindingSource.DataSource = masterWarnings;
                frm.Report = "SSAS.AggManager.ValidateAggs.rdlc";
                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "AggManager_AggValidationWarning";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = frm.Report;

                frm.Caption = "Aggregation Validation Warnings";
                frm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                frm.Show();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("No aggregations warnings found.");
            }
        }

        private static List<AggValidationWarning> CheckAggDesign(AggregationDesign aggDesign, string sCorrectAggregationDesignName, string sReportTitle)
        {
            List<AggValidationWarning> masterWarnings = new List<AggValidationWarning>();

            //check for m2m agg problems
            foreach (Aggregation agg in aggDesign.Aggregations)
            {
                foreach (AggregationDimension aggDim in agg.Dimensions)
                {
                    if (aggDim.Attributes.Count > 0 && aggDim.MeasureGroupDimension is ManyToManyMeasureGroupDimension)
                    {
                        ManyToManyMeasureGroupDimension m2mDim = (ManyToManyMeasureGroupDimension)aggDim.MeasureGroupDimension;
                        MeasureGroup intermediateMG = m2mDim.MeasureGroup;
                        List<MeasureGroupAttribute> missing = new List<MeasureGroupAttribute>();
                        foreach (MeasureGroupDimension commonDim in intermediateMG.Dimensions)
                        {
                            RegularMeasureGroupDimension regCommonDim = commonDim as RegularMeasureGroupDimension;
                            if (commonDim.CubeDimensionID != aggDim.CubeDimensionID || regCommonDim == null)
                            {
                                if (!aggDim.ParentMeasureGroup.Dimensions.Contains(commonDim.CubeDimensionID)) continue; //this isn't a shared dimension
                                MeasureGroupDimension dataMeasureGroupDim = aggDim.ParentMeasureGroup.Dimensions[commonDim.CubeDimensionID];
                                if (dataMeasureGroupDim is ManyToManyMeasureGroupDimension) continue; //this shared dimension is m2m on the data measure group so don't include it

                                //this is a common dimension and the granularity attribute on the intermediate measure group needs to be in the agg
                                bool bFoundGranularityAgg = false;
                                MeasureGroupAttribute mga = GetGranularityAttribute(regCommonDim);
                                AggregationDimension aggCommonDim = agg.Dimensions.Find(commonDim.CubeDimensionID);
                                if (aggCommonDim != null)
                                {
                                    if (aggCommonDim.Attributes.Find(mga.AttributeID) != null)
                                    {
                                        bFoundGranularityAgg = true;
                                    }
                                }
                                if (!bFoundGranularityAgg && mga != null)
                                {
                                    missing.Add(mga);
                                }
                            }
                        }
                        string sWarning = "This aggregation contains many-to-many dimension [" + m2mDim.CubeDimension.Name + "]. It will not be used unless it also contains ";
                        for (int i = 0; i < missing.Count; i++)
                        {
                            MeasureGroupAttribute mga = missing[i];
                            if (i > 0) sWarning += " and ";
                            sWarning += "[" + mga.Parent.CubeDimension.Name + "].[" + mga.Attribute.Name + "]";
                        }

                        if (missing.Count == 0)
                            sWarning = "";
                        else
                            sWarning += ". ";
                        sWarning += "The many-to-many dimension [" + m2mDim.CubeDimension.Name + "] itself should not be included in the aggregation to workaround a bug.";

                        masterWarnings.Add(new AggValidationWarning(agg, sCorrectAggregationDesignName, sWarning, sReportTitle));
                    }
                }
            }

            //check for non-materialized reference dimensions
            foreach (Aggregation agg in aggDesign.Aggregations)
            {
                foreach (AggregationDimension aggDim in agg.Dimensions)
                {
                    if (aggDim.Attributes.Count > 0 && aggDim.MeasureGroupDimension is ReferenceMeasureGroupDimension)
                    {
                        ReferenceMeasureGroupDimension refDim = (ReferenceMeasureGroupDimension)aggDim.MeasureGroupDimension;
                        if (refDim.Materialization == ReferenceDimensionMaterialization.Indirect)
                        {
                            string sWarning = "This aggregation contains a non-materialized reference dimension [" + refDim.CubeDimension.Name + "] which is not supported.";
                            masterWarnings.Add(new AggValidationWarning(agg, sCorrectAggregationDesignName, sWarning, sReportTitle));
                        }
                    }
                }
            }

            //check whether all measures are semi-additive
            bool bAllMeasuresAreSemiAdditive = true;
            foreach (Measure m in aggDesign.Parent.Measures)
            {
                if (m.AggregateFunction == AggregationFunction.Count || m.AggregateFunction == AggregationFunction.DistinctCount || m.AggregateFunction == AggregationFunction.Sum || m.AggregateFunction == AggregationFunction.Min || m.AggregateFunction == AggregationFunction.Max || m.AggregateFunction == AggregationFunction.None)
                {
                    bAllMeasuresAreSemiAdditive = false;
                    break;
                }
                else if (m.AggregateFunction == AggregationFunction.ByAccount)
                {
                    //if it's a ByAccount measure, we need to check the list of AggregationFunctions on each account
                    foreach (Account acct in aggDesign.ParentDatabase.Accounts)
                    {
                        if (acct.AggregationFunction == AggregationFunction.Sum) //Sum is the only additive AggregationFunction allowed in account intelligence
                        {
                            bAllMeasuresAreSemiAdditive = false;
                            break;
                        }
                    }
                }
            }

            //if all measures are semi-additive, find the Time dimension the semi-additive behavior operates on (which we think is the first Time dimension)
            if (bAllMeasuresAreSemiAdditive)
            {
                CubeDimension semiAdditiveDim = null;
                MeasureGroupDimension semiAdditiveMgDim = null;
                foreach (CubeDimension cd in aggDesign.ParentCube.Dimensions)
                {
                    MeasureGroupDimension mgd = aggDesign.Parent.Dimensions.Find(cd.ID);
                    if (mgd != null && mgd.Dimension.Type == DimensionType.Time)
                    {
                        semiAdditiveDim = mgd.CubeDimension;
                        semiAdditiveMgDim = mgd;
                        break;
                    }
                }

                if (semiAdditiveDim == null || semiAdditiveMgDim == null || !(semiAdditiveMgDim is RegularMeasureGroupDimension))
                {
                    //TODO: should we warn about this?
                }
                else
                {
                    foreach (Aggregation agg in aggDesign.Aggregations)
                    {
                        AggregationDimension semiAdditiveAggDim = agg.Dimensions.Find(semiAdditiveDim.ID);
                        MeasureGroupAttribute granularity = GetGranularityAttribute((RegularMeasureGroupDimension)semiAdditiveMgDim);
                        if (semiAdditiveAggDim == null || semiAdditiveAggDim.Attributes.Find(granularity.AttributeID) == null)
                        {
                            string sWarning = "This measure group contains only semi-additive measures. This aggregation will not be used when semi-additive measure values are retrieved because it does not include the granularity attribute of the semi-additive dimension ([" + semiAdditiveDim.Name + "].[" + granularity.Attribute.Name + "]). (The Exists-with-a-measure-group function can still run off this aggregation, though.)";
                            masterWarnings.Add(new AggValidationWarning(agg, sCorrectAggregationDesignName, sWarning, sReportTitle));
                        }
                    }
                }
            }

            //check for aggs on parent-child attributes
            foreach (Aggregation agg in aggDesign.Aggregations)
            {
                foreach (AggregationDimension aggDim in agg.Dimensions)
                {
                    foreach (AggregationAttribute attr in aggDim.Attributes)
                    {
                        if (attr.Attribute.Usage == AttributeUsage.Parent)
                        {
                            string sWarning = "This aggregation contains [" + aggDim.CubeDimension.Name + "].[" + attr.Attribute.Name + "] which is a parent-child attribute. This is not allowed. The aggregation should include [" + aggDim.CubeDimension.Name + "].[" + aggDim.Dimension.KeyAttribute.Name + "] instead.";
                            masterWarnings.Add(new AggValidationWarning(agg, sCorrectAggregationDesignName, sWarning, sReportTitle));
                        }
                    }
                }
            }

            //check for aggs on AttributeHierarchyEnabled=false attributes
            foreach (Aggregation agg in aggDesign.Aggregations)
            {
                foreach (AggregationDimension aggDim in agg.Dimensions)
                {
                    foreach (AggregationAttribute attr in aggDim.Attributes)
                    {
                        if (!attr.CubeAttribute.AttributeHierarchyEnabled)
                        {
                            string sWarning = "This aggregation contains [" + aggDim.CubeDimension.Name + "].[" + attr.Attribute.Name + "] which is not enabled as an attribute hierarchy. This is not allowed.";
                            masterWarnings.Add(new AggValidationWarning(agg, sCorrectAggregationDesignName, sWarning, sReportTitle));
                        }
                    }
                }
            }

            //find a list of ALTER statements that alter the DEFAULT_MEMBER property in the calc script
            List<string> attributesWithDefaultMemberAlteredInCalcScript = new List<string>();
            System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex(@"ALTER\s+CUBE\s+(CURRENTCUBE|\[?" + aggDesign.ParentCube.Name + @"\]?)\s+UPDATE\s+DIMENSION\s+(.+?)\s*\,\s*DEFAULT_MEMBER\s+", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
            foreach (MdxScript script in aggDesign.ParentCube.MdxScripts)
            {
                if (script.DefaultScript)
                {
                    StringBuilder sCommands = new StringBuilder();
                    foreach (Command cmd in script.Commands)
                    {
                        sCommands.AppendLine(cmd.Text);
                    }
                    foreach (System.Text.RegularExpressions.Match match in regEx.Matches(sCommands.ToString()))
                    {
                        try
                        {
                            attributesWithDefaultMemberAlteredInCalcScript.Add(match.Groups[2].Captures[0].Value.ToLower());
                        }
                        catch { }
                    }
                    break;
                }
            }

            //build list of cube dimension attributes that have default members, are not aggregatable, or are marked as AggregationUsage=Full
            foreach (MeasureGroupDimension mgDim in aggDesign.Parent.Dimensions)
            {
                if (mgDim is ManyToManyMeasureGroupDimension) continue; //don't suggest adding any m2m dimensions
                CubeDimension cd = mgDim.CubeDimension;
                foreach (CubeAttribute ca in cd.Attributes)
                {
                    if (!ca.AttributeHierarchyEnabled) continue;
                    if (!IsAtOrAboveGranularity(ca.Attribute, mgDim)) continue;

                    foreach (Aggregation agg in aggDesign.Aggregations)
                    {
                        AggregationDimension aggDim = agg.Dimensions.Find(cd.ID);
                        if (ca.Attribute.Usage == AttributeUsage.Parent)
                        {
                            if (!(mgDim is RegularMeasureGroupDimension)) continue;
                            if (!IsAtOrAboveGranularity(cd.Dimension.KeyAttribute, mgDim)) continue;

                            //if this is a parent-child attribute and the key isn't in the agg, then check whether the parent-child attribute has a DefaultMember or is aggregatable
                            if (aggDim == null || aggDim.Attributes.Find(cd.Dimension.KeyAttribute.ID) == null)
                            {
                                string sWarning = "";
                                if (!string.IsNullOrEmpty(ca.Attribute.DefaultMember) || attributesWithDefaultMemberAlteredInCalcScript.Contains(((string)("[" + cd.Name + "].[" + ca.Attribute.Name + "]")).ToLower()))
                                {
                                    sWarning += "has a DefaultMember";
                                }

                                if (!ca.Attribute.IsAggregatable)
                                {
                                    if (!string.IsNullOrEmpty(sWarning)) sWarning += " and ";
                                    sWarning += "is not aggregatable";
                                }

                                if (!string.IsNullOrEmpty(sWarning))
                                {
                                    sWarning = "This aggregation should probably contain [" + cd.Name + "].[" + cd.Dimension.KeyAttribute.Name + "] because [" + cd.Name + "].[" + ca.Attribute.Name + "] is a parent-child attribute which " + sWarning + ".";
                                    masterWarnings.Add(new AggValidationWarning(agg, sCorrectAggregationDesignName, sWarning, sReportTitle));
                                }
                            }
                        }
                        //for non-parent-child attributes...
                        else if (aggDim == null || aggDim.Attributes.Find(ca.AttributeID) == null)
                        {
                            string sWarning = "";
                            if (!string.IsNullOrEmpty(ca.Attribute.DefaultMember) || attributesWithDefaultMemberAlteredInCalcScript.Contains(((string)("[" + cd.Name + "].[" + ca.Attribute.Name + "]")).ToLower()))
                            {
                                sWarning += "has a DefaultMember";
                            }

                            if (!ca.Attribute.IsAggregatable)
                            {
                                if (!string.IsNullOrEmpty(sWarning)) sWarning += " and ";
                                sWarning += "is not aggregatable";
                            }

                            if (ca.AggregationUsage == AggregationUsage.Full)
                            {
                                if (!string.IsNullOrEmpty(sWarning)) sWarning += " and ";
                                sWarning += "is marked as AggregationUsage=Full";
                            }

                            if (aggDim != null && AggContainsChild(aggDim, ca.Attribute)) continue; //if this attribute is redundant, then no need to warn about it

                            if (!string.IsNullOrEmpty(sWarning))
                            {
                                sWarning = "This aggregation should probably contain [" + cd.Name + "].[" + ca.Attribute.Name + "] which " + sWarning + ".";
                                masterWarnings.Add(new AggValidationWarning(agg, sCorrectAggregationDesignName, sWarning, sReportTitle));
                            }
                        }
                    }
                }
            }

            //look for aggs with redundant attributes
            foreach (Aggregation agg in aggDesign.Aggregations)
            {
                bool bHasRedundancy = false;
                foreach (AggregationDimension aggDim in agg.Dimensions)
                {
                    foreach (AggregationAttribute attr in aggDim.Attributes)
                    {
                        if (AggContainsParent(aggDim, attr.Attribute))
                        {
                            bHasRedundancy = true;
                            break;
                        }
                    }
                    if (bHasRedundancy) break;
                }
                if (bHasRedundancy)
                {
                    string sWarning = "This aggregation contains redundant attributes which unnecessarily bloat the size of the aggregation.";
                    masterWarnings.Add(new AggValidationWarning(agg, sCorrectAggregationDesignName, sWarning, sReportTitle));
                }
            }

            //check for aggs on below granularity attributes
            foreach (Aggregation agg in aggDesign.Aggregations)
            {
                foreach (AggregationDimension aggDim in agg.Dimensions)
                {
                    foreach (AggregationAttribute attr in aggDim.Attributes)
                    {
                        if (!IsAtOrAboveGranularity(attr.Attribute, aggDim.MeasureGroupDimension))
                        {
                            string sWarning = "This aggregation contains [" + aggDim.CubeDimension.Name + "].[" + attr.Attribute.Name + "] which is below granularity. This is not allowed.";
                            masterWarnings.Add(new AggValidationWarning(agg, sCorrectAggregationDesignName, sWarning, sReportTitle));
                        }
                    }
                }
            }

            return masterWarnings;
        }

        private static bool AggContainsParent(AggregationDimension aggDim, DimensionAttribute attr)
        {
            foreach (AttributeRelationship rel in attr.AttributeRelationships)
            {
                if (aggDim.Attributes.Find(rel.AttributeID) != null)
                    return true;
                else if (AggContainsParent(aggDim, rel.Attribute))
                    return true;
            }
            return false;
        }

        public static MeasureGroupAttribute GetGranularityAttribute(RegularMeasureGroupDimension mgDim)
        {
            if (mgDim != null)
            {
                foreach (MeasureGroupAttribute mga in mgDim.Attributes)
                {
                    if (mga.Type == MeasureGroupAttributeType.Granularity)
                    {
                        return mga;
                    }
                }
            }
            return null;
        }

        public static bool IsAtOrAboveGranularity(DimensionAttribute attribute, MeasureGroupDimension mgDim)
        {
            if (mgDim is RegularMeasureGroupDimension)
            {
                MeasureGroupAttribute granularity = GetGranularityAttribute((RegularMeasureGroupDimension)mgDim);
                if (granularity.AttributeID == attribute.ID)
                    return true;
                return IsParentOf(attribute, granularity.Attribute);
            }
            else if (mgDim is ManyToManyMeasureGroupDimension)
            {
                //this depends on the granularity of the m2m dimension as it appears on the intermediate measure group
                ManyToManyMeasureGroupDimension m2mDim = (ManyToManyMeasureGroupDimension)mgDim;
                return IsAtOrAboveGranularity(attribute, m2mDim.MeasureGroup.Dimensions.Find(mgDim.CubeDimensionID));
            }
            else
            {
                return true;
            }
        }

        private static bool IsParentOf(DimensionAttribute parent, DimensionAttribute child)
        {
            foreach (AttributeRelationship rel in child.AttributeRelationships)
            {
                if (rel.AttributeID == parent.ID)
                    return true;
                else if (IsParentOf(parent, rel.Attribute))
                    return true;
            }
            return false;
        }

        private static bool AggContainsChild(AggregationDimension aggDim, DimensionAttribute attr)
        {
            foreach (AggregationAttribute aggAttr in aggDim.Attributes)
            {
                if (IsParentOf(attr, aggAttr.Attribute))
                    return true;
            }
            return false;
        }

        public class AggValidationWarning
        {
            public AggValidationWarning(Aggregation agg, string sCorrectAggregationDesignName, string sWarning, string sReportTitle)
            {
                mAggName = agg.Name;
                mAggDesignName = sCorrectAggregationDesignName;
                mMeasureGroupName = agg.ParentMeasureGroup.Name;
                mCubeNameOrReportTitle = (sReportTitle == null ? agg.ParentCube.Name : sReportTitle);
                mDatabaseName = agg.ParentDatabase.Name;
                mWarning = sWarning;
            }

            private string mWarning;
            private string mDatabaseName;
            private string mCubeNameOrReportTitle;
            private string mMeasureGroupName;
            private string mAggDesignName;
            private string mAggName;

            public string Warning
            {
                get { return mWarning; }
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
