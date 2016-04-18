using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Data;
using static BIDSHelper.SSAS.UnusedColumnsPlugin;

namespace BIDSHelper.SSAS
{
    public class DsvColumnResult
    {
        //protected Dictionary<string, UnusedColumn> unusedColumns = new Dictionary<string, UnusedColumn>();
        //protected List<UsedColumn> usedColumns = new List<UsedColumn>();
        //private DataSourceView m_dsv;

        public DsvColumnResult(DataSourceView dsv) { DSV = dsv; }

        public DataSourceView DSV { get; internal set; }
        public Dictionary<string, UnusedColumn> UnusedColumns { get; set; }
        public List<UsedColumn> UsedColumns { get; set; }
    }
    public static class DsvHelpers
    {
        public static DsvColumnResult IterateDsvColumns(DataSourceView dsv)
        {
            //DataSourceView m_dsv = dsv;

            DsvColumnResult results = new DsvColumnResult(dsv);
            //add all DSV columns to a list
            //unusedColumns.Clear();
            //usedColumns.Clear();
            foreach (DataTable t in dsv.Schema.Tables)
            {
                foreach (DataColumn c in t.Columns)
                {
                    results.UnusedColumns.Add("[" + t.TableName + "].[" + c.ColumnName + "]", new UnusedColumn(c, dsv));
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
                            ProcessDataItemInLists(results, di, "Dimension Attribute Key");
                        }
                        ProcessDataItemInLists(results, attr.NameColumn, "Dimension Attribute Name");
                        ProcessDataItemInLists(results, attr.ValueColumn, "Dimension Attribute Value");
                        ProcessDataItemInLists(results, attr.UnaryOperatorColumn, "Dimension Attribute Unary Operator");
                        ProcessDataItemInLists(results, attr.SkippedLevelsColumn, "Dimension Attribute Skipped Levels");
                        ProcessDataItemInLists(results, attr.CustomRollupColumn, "Dimension Attribute Custom Rollup");
                        ProcessDataItemInLists(results, attr.CustomRollupPropertiesColumn, "Dimension Attribute Custom Rollup Properties");
                        foreach (AttributeTranslation tran in attr.Translations)
                        {
                            ProcessDataItemInLists(results, tran.CaptionColumn, "Dimension Attribute Translation");

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
                            ProcessDataItemInLists(results, m.Source, "Measure");
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
                                            ProcessDataItemInLists(results, di3, "Fact Table Dimension Key");
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
                    RecurseMiningStructureColumnsAndProcessDataItemInLists(results, structure.Columns);
            }

            return results;
        }


        private static void ProcessDataItemInLists(DsvColumnResult result, DataItem di, string usageType)
        {
            if (di == null) return;
            ColumnBinding cb = di.Source as ColumnBinding;
            if (cb == null) return;
            string sColUniqueName = "[" + cb.TableID + "].[" + cb.ColumnID + "]";
            if (result.UnusedColumns.ContainsKey(sColUniqueName))
                result.UnusedColumns.Remove(sColUniqueName);
            result.UsedColumns.Add(new UsedColumn(di, cb, result.DSV, usageType, di.Parent.FriendlyPath));
        }

        private static void RecurseMiningStructureColumnsAndProcessDataItemInLists(DsvColumnResult result, MiningStructureColumnCollection cols)
        {
            foreach (MiningStructureColumn col in cols)
            {
                if (col is ScalarMiningStructureColumn)
                {
                    ScalarMiningStructureColumn scalar = (ScalarMiningStructureColumn)col;
                    foreach (DataItem di in scalar.KeyColumns)
                    {
                        ProcessDataItemInLists(result, di, "Mining Structure Column Key");
                    }
                    ProcessDataItemInLists(result, scalar.NameColumn, "Mining Structure Column Name");
                }
                else if (col is TableMiningStructureColumn)
                {
                    TableMiningStructureColumn tblCol = (TableMiningStructureColumn)col;
                    RecurseMiningStructureColumnsAndProcessDataItemInLists( result, tblCol.Columns);
                }
            }
        }

    }
}
