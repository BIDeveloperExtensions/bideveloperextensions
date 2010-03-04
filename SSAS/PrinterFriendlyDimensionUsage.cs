using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.AnalysisServices;

namespace BIDSHelper
{
    public class PrinterFriendlyDimensionUsage
    {


        public static List<DimensionUsage> GetDimensionUsage(Cube c)
        {
            List<DimensionUsage> dimUsage = new List<DimensionUsage>();
            foreach (MeasureGroup mg in c.MeasureGroups)
            {
                System.Diagnostics.Trace.Write("mg " + mg.Name);
                foreach (MeasureGroupDimension mgdim in mg.Dimensions)
                {
                    System.Diagnostics.Trace.Write(",mgdim " + mgdim.Dimension.Name);
                    if (mgdim is ReferenceMeasureGroupDimension)
                    {
                        ReferenceMeasureGroupDimension refMgDim = (ReferenceMeasureGroupDimension)mgdim;
                        getReferencedMeasureGroupAttributeUsage(dimUsage, mg, refMgDim);
                    }
                    else if (mgdim is DegenerateMeasureGroupDimension)
                    {
                        DegenerateMeasureGroupDimension degMgDim = (DegenerateMeasureGroupDimension)mgdim;
                        getFactMeasureGroupAttributeUsage(dimUsage, mg, degMgDim);
                    }
                    else if (mgdim is ManyToManyMeasureGroupDimension)
                    {
                        ManyToManyMeasureGroupDimension m2mMgDim = (ManyToManyMeasureGroupDimension)mgdim;
                        getManyToManyMeasureGroupAttributeUsage(dimUsage, mg, m2mMgDim);
                    }
                    else if (mgdim is DataMiningMeasureGroupDimension)
                    {
                        DataMiningMeasureGroupDimension dmMgDim = (DataMiningMeasureGroupDimension)mgdim;
                        getDataMiningMeasureGroupAttributeUsage(dimUsage, mg, dmMgDim);
                    }
                    else if (mgdim is RegularMeasureGroupDimension)
                    {
                        RegularMeasureGroupDimension regMDdim = (RegularMeasureGroupDimension)mgdim;
                        getRegularMeasureGroupAttributeUsage(dimUsage, mg, regMDdim);
                    }
                }
            
            }

            return dimUsage;
        }


        /*
        * All relationships - Relationship Type
        *                   - Measure Group Name
        *                   - Dim Name
        */

        /*
        * Regular Col1 - Ganularity Attribute Name
        *         Col2 - Dimension Column(s) Name
        *         Col3 - Measure Group Columns(s) Name
        */
        private static void getRegularMeasureGroupAttributeUsage(List<DimensionUsage> dimUsage, MeasureGroup mg, RegularMeasureGroupDimension regMDdim)
        {
            string tableId = string.Empty;
            DimensionUsage usage = new DimensionUsage("Regular", mg, regMDdim.CubeDimension, regMDdim.Dimension);//, cb.TableID, cb.ColumnID);
            usage.Column1Name = "Granularity Attribute";
            
            usage.Column2Name = "Dimension Column";
            usage.Column3Name = "Measure Group Columns";

            foreach (MeasureGroupAttribute mga in regMDdim.Attributes)
            {
                
                                   
                if (mga.Type == MeasureGroupAttributeType.Granularity)
                {
                    tableId = string.Empty;
                    usage.Column1Value = mga.Attribute.Name;
                    System.Diagnostics.Trace.Write(",mga " + mga.CubeAttribute.Attribute.Name);

                    //foreach (DataItem di in mga.KeyColumns)
                    //{    
                        //ColumnBinding cb = (ColumnBinding)di.Source;
                        // TODO - get the key columns for the attribute, not just its name
                        //System.Diagnostics.Trace.WriteLine(",di " + di.Source.ToString());
                        
                        foreach (DataItem di2 in mga.Attribute.KeyColumns)
                        {
                            if (di2.Source is ColumnBinding)
                            {
                                tableId = ((ColumnBinding)di2.Source).TableID;
                                DataSourceView dsv = mga.Parent.Dimension.DataSourceView;
                                if (dsv.Schema.Tables.Contains(tableId))
                                {
                                    DataTable oTable = dsv.Schema.Tables[dsv.Schema.Tables.IndexOf(tableId)];
                                    if (oTable.ExtendedProperties.ContainsKey("FriendlyName"))
                                    {
                                        usage.Column2Value += oTable.ExtendedProperties["FriendlyName"] + ".";
                                    }
                                }
                                else
                                {
                                    usage.Column2Value += ((ColumnBinding)di2.Source).TableID + ".";
                                }
                                usage.Column2Value += ((ColumnBinding)di2.Source).ColumnID + "\n";
                            }
                        }
                
                        foreach (DataItem di3 in mga.KeyColumns)
                        {
                            if (di3.Source is ColumnBinding)
                            {
                                tableId = ((ColumnBinding)di3.Source).TableID;
                                DataSourceView dsv = mga.ParentCube.DataSourceView;
                                if (dsv.Schema.Tables.Contains(tableId))
                                {
                                    DataTable oTable = dsv.Schema.Tables[dsv.Schema.Tables.IndexOf(tableId)];
                                    if (oTable.ExtendedProperties.ContainsKey("FriendlyName"))
                                    {
                                        usage.Column3Value += oTable.ExtendedProperties["FriendlyName"] + ".";
                                    }
                                }
                                else
                                {
                                    usage.Column3Value += ((ColumnBinding)di3.Source).TableID + ".";
                                }

                                usage.Column3Value += ((ColumnBinding)di3.Source).ColumnID;
                                usage.Column3Value += "  (" + di3.NullProcessing.ToString().Substring(0, 1) + ")";
                                usage.Column3Value += "\n";
                            }
                        }

                        
                    //}
                }
                
            }
            dimUsage.Add(usage);
        }

        /*
         * Many    Col1 - Intermediate Measure Group Name
         * 
         */
        private static void getManyToManyMeasureGroupAttributeUsage(List<DimensionUsage> dimUsage, MeasureGroup mg, ManyToManyMeasureGroupDimension m2mMDdim)
        {
            DimensionUsage usage = new DimensionUsage("Many to Many", mg, m2mMDdim.CubeDimension, m2mMDdim.Dimension);
            usage.Column1Name = "Intermediate Measure Group";
            usage.Column1Value = m2mMDdim.MeasureGroup.Name;
            dimUsage.Add(usage);
        }

        /*
         * DataMining    Col1 - Source Dimension Name
         * 
         */
        private static void getDataMiningMeasureGroupAttributeUsage(List<DimensionUsage> dimUsage, MeasureGroup mg, DataMiningMeasureGroupDimension dmMDdim)
        {
            DimensionUsage usage = new DimensionUsage("Data Mining", mg, dmMDdim.CubeDimension, dmMDdim.Dimension);
            usage.Column1Name = "Source Dimension";
            usage.Column1Value = dmMDdim.Dimension.Name;
            dimUsage.Add(usage);
        }

        /* 
        * Referenced Col1 - Ref Dim Attrib Name
        *            Col2 - Intermediate Dim Name
        *            Col3 - Intermediate Dim Attib Name
        *            Col4 - Path 
         *  ?? Materialized
        */
        private static void getReferencedMeasureGroupAttributeUsage(List<DimensionUsage> dimUsage, MeasureGroup mg, ReferenceMeasureGroupDimension refMgDim)
        {
            DimensionUsage usage = new DimensionUsage("Referenced",mg, refMgDim.CubeDimension, refMgDim.Dimension);
            string tableId = string.Empty;

            usage.Column1Name = "Reference Dimension Attribute";
            foreach (CubeAttribute a in refMgDim.CubeDimension.Attributes)
            {
                if (a.Attribute.Usage == AttributeUsage.Key)
                {
                    usage.Column1Value = a.Attribute.Name;
                    break;
                }
            }
            usage.Column2Name = "Intermediate Dimension";
            usage.Column2Value = refMgDim.IntermediateDimension.Name;

            usage.Column3Name = "Intermediate Dimension Attribute";
            usage.Column3Value = refMgDim.IntermediateGranularityAttribute.Attribute.Name;

            // not currently exposed on the report due to space limitation
            // the string (Materialized) is added after the dim name instead.
            usage.Column4Name = "Materialized";
            usage.Column4Value = refMgDim.Materialization.ToString();
            usage.Materialized = (refMgDim.Materialization == ReferenceDimensionMaterialization.Regular);

            dimUsage.Add(usage);
        }

        /*
         * Fact    Col1 - Granularity Attribute
         *         Col2 - Source Table name
         */
        private static void getFactMeasureGroupAttributeUsage(List<DimensionUsage> dimUsage, MeasureGroup mg, DegenerateMeasureGroupDimension factMGdim)
        {
            DimensionUsage usage = null;
            usage = new DimensionUsage("Fact", mg, factMGdim.CubeDimension, factMGdim.Dimension);
            usage.Column1Name = "Granularity Attribute";
            usage.Column2Name = "Source Table";
            
            foreach (MeasureGroupAttribute mga in factMGdim.Attributes)
            {
                //mga.
                if (mga.Type == MeasureGroupAttributeType.Granularity)
                {
                    
                    usage.Column1Value = mga.Attribute.Name;
                    foreach (DataItem di in mga.KeyColumns)
                    {
                        if (di.Source is ColumnBinding)
                        {
                            usage.Column2Value = ((ColumnBinding)di.Source).TableID;
                            DataSourceView dsv = mga.ParentCube.DataSourceView;
                            DataTable oTable = dsv.Schema.Tables[dsv.Schema.Tables.IndexOf(usage.Column2Value)];
                            if (oTable.ExtendedProperties.ContainsKey("FriendlyName"))
                            {
                                usage.Column2Value = oTable.ExtendedProperties["FriendlyName"].ToString();
                            }
                        }
                    }
                }
            }
            dimUsage.Add(usage);
        }

    } // end Class PrinterFriendlyDimensionUsage

    public class DimensionUsage
    { 
#region Constructor
        public DimensionUsage(string relationshipType, MeasureGroup mg, CubeDimension dimCube, Dimension dim) //, string factTableColumn, string attributeColumn)
        {
            mCubeName = mg.Parent.Name;
            mDatabaseName = mg.Parent.Parent.Name;
            mDimensionName = dimCube.Name;
            if (dimCube.Name != dim.Name)  mDimensionName += " (" + dim.Name + ")";
            mMeasureGroup = mg.Name;
            mRelationshipType = relationshipType;
            //mFactTableColumnName = factTableColumn;
            //mAttributeColumnName = attributeColumn;
        }
#endregion

#region Public Fields
        private string mDimensionName;
        //private string mGranularity;
        private string mMeasureGroup;
        private string mRelationshipType;
        //private string mFactTableColumnName;
        //private string mAttributeColumnName;
        private string mColumn1Name;
        private string mColumn1Value;
        private string mColumn2Name;
        private string mColumn2Value;
        private string mColumn3Name;
        private string mColumn3Value;
        private string mColumn4Name;
        private string mColumn4Value;
        private string mCubeName;
        private string mDatabaseName;
        private NullProcessing mNullProcessing;
        private bool mMaterialized = false;

        public string CubeName
        {
            get { return mCubeName; }
        }

        public string DatabaseName
        {
            get { return mDatabaseName; }
        }

        public string ImageName
        {
            get { return "relationship" + RelationshipType.Replace(" ", string.Empty); }
        }

        public string RelationshipType
        {
            get { return mRelationshipType; }
        }
        public string Column1Name
        {
            get { return mColumn1Name; }
            set { mColumn1Name = value;}
        }

        public string Column1Value
        {
            get { return mColumn1Value; }
            set { mColumn1Value = value; }
        }
        public string Column2Name
        {
            get { return mColumn2Name; }
            set { mColumn2Name = value; }
        }

        public string Column2Value
        {
            get { return mColumn2Value; }
            set { mColumn2Value = value; }
        }
        public string Column3Name
        {
            get { return mColumn3Name; }
            set { mColumn3Name = value; }
        }

        public string Column3Value
        {
            get { return mColumn3Value; }
            set { mColumn3Value = value; }
        }
        public string Column4Name
        {
            get { return mColumn4Name; }
            set { mColumn4Name = value; }
        }

        public string Column4Value
        {
            get { return mColumn4Value; }
            set { mColumn4Value = value; }
        }
        public string DimensionName
        {
            get {
                if (Materialized)
                {
                    return mDimensionName + " (Materialized)";
                }
                return mDimensionName;
            }
        }
        //public string Granularity
        //{
        //    get { return mGranularity; }
        //}
        public string MeasureGroup
        {
            get { return mMeasureGroup; }
        }
        //public string FactTableColumnName
        //{
        //    get { return mFactTableColumnName; }
        //}
        //public string AttributeColumnName
        //{
        //    get {return mAttributeColumnName; }
        //}

        public NullProcessing NullProcessingOption
        {
            get { return mNullProcessing; }
            set { mNullProcessing = value; }
        }

        public string NullProcessingIndicator
        {
            get 
            { 
                return mNullProcessing.ToString().Substring(0, 1); 
            }
        }

        public bool Materialized
        {
            get { return mMaterialized; }
            set { mMaterialized = value; }
        }
#endregion
    }
}
