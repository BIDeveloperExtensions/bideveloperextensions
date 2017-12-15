using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.BackEnd;
using BIDSHelper.SSAS;

namespace BIDSHelper
{
    public class PrinterFriendlyDimensionUsage
    {


        public static List<DimensionUsage> GetDimensionUsage(Cube c)
        {
            List<CubeDimension> listCubeDimensions = new List<CubeDimension>();
            foreach (CubeDimension cd in c.Dimensions)
            {
                listCubeDimensions.Add(cd);
            }

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
                    if (listCubeDimensions.Contains(mgdim.CubeDimension))
                    {
                        listCubeDimensions.Remove(mgdim.CubeDimension);
                    }
                }
            
            }

            //add any cube dimensions which aren't related to any measure groups
            foreach (CubeDimension cd in listCubeDimensions)
            {
                DimensionUsage du = new DimensionUsage(string.Empty, null, cd, cd.Dimension);
                dimUsage.Add(du);
            }

            return dimUsage;
        }

#if DENALI || SQL2014
        public static List<DimensionUsage> GetTabularDimensionUsage(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox, bool bIsBusMatrix)
        {

            Cube c = sandbox.Cube;

            List<CubeDimension> listCubeDimensions = new List<CubeDimension>();
            List<DimensionUsage> dimUsage = new List<DimensionUsage>();
            foreach (CubeDimension cd in c.Dimensions)
            {
                bool bFoundVisibleAttribute = false;
                foreach (DimensionAttribute da in cd.Dimension.Attributes)
                {
                    if (da.AttributeHierarchyVisible)
                    {
                        bFoundVisibleAttribute = true;
                        break;
                    }
                }
                if (bFoundVisibleAttribute)
                    listCubeDimensions.Add(cd);

                bool bFoundVisibleMeasure = false;
                foreach (Microsoft.AnalysisServices.BackEnd.DataModelingMeasure m in sandbox.Measures)
                {
                    if (m.Table == cd.Dimension.Name && !m.IsPrivate)
                    {
                        bFoundVisibleMeasure = true;
                        break;
                    }
                }
                if (!bFoundVisibleMeasure && bIsBusMatrix) continue;

                MeasureGroup mg = c.MeasureGroups[cd.DimensionID];
                List<DimensionUsage> tmp = RecurseTabularRelationships(cd.Dimension, mg, bIsBusMatrix);
                dimUsage.AddRange(tmp);

                if (bFoundVisibleAttribute && bFoundVisibleMeasure) //if this table had a measure but no dimension relationships (except to itself)
                {
                    DimensionUsage du = new DimensionUsage("Fact", mg, cd, cd.Dimension);
                    dimUsage.Add(du);
                }
                else if (tmp.Count == 0 && bIsBusMatrix && bFoundVisibleMeasure) //if this table with a measure had no dimension relationships, add it as such...
                {
                    DimensionUsage du = new DimensionUsage(string.Empty, mg, null, null);
                    dimUsage.Add(du);
                }
            }

            //remove dimensions in relationships
            foreach (DimensionUsage du in dimUsage)
            {
                for (int i = 0; i < listCubeDimensions.Count; i++)
                {
                    if (listCubeDimensions[i].Name == du.DimensionName)
                    {
                        listCubeDimensions.RemoveAt(i);
                        i--;
                    }
                }
            }

            //add any cube dimensions which aren't related to any measure groups
            foreach (CubeDimension cd in listCubeDimensions)
            {
                DimensionUsage du = new DimensionUsage(string.Empty, null, cd, cd.Dimension);
                dimUsage.Add(du);
            }

            return dimUsage;
        }
#else
        public static List<DimensionUsage> GetTabularDimensionUsage(DataModelingSandboxWrapper sandbox, bool bIsBusMatrix)
        {

            //Cube c = sandbox.Cube;
            DataModelingSandbox tomSandbox = sandbox.GetSandbox();

            List<CubeDimension> listCubeDimensions = new List<CubeDimension>();
            List<DimensionUsage> dimUsage = new List<DimensionUsage>();

            List<DataModelingTable> listDimensions = new List<DataModelingTable>();

            foreach (DataModelingTable table in tomSandbox.Tables)
            {
                bool bFoundVisibleAttribute = false;
                foreach (DataModelingColumn col in table.Columns)
                {
                    if (!table.IsPrivate && !col.IsPrivate && col.IsAttributeHierarchyQueriable)
                    {
                        bFoundVisibleAttribute = true;
                        break;
                    }
                }
                if (bFoundVisibleAttribute)
                    listDimensions.Add(table);

                bool bFoundVisibleMeasure = false;
                foreach (DataModelingMeasure m in sandbox.Measures)
                {
                    if (!m.IsPrivate && m.Table == table.Name)
                    {
                        bFoundVisibleMeasure = true;
                        break;
                    }
                }
                if (!bFoundVisibleMeasure && bIsBusMatrix) continue;

                List<DimensionUsage> tmp = RecurseTabularRelationships(table, table, bIsBusMatrix, new List<Microsoft.AnalysisServices.BackEnd.Relationship>(), false);
                dimUsage.AddRange(tmp);

                if (bFoundVisibleAttribute && bFoundVisibleMeasure) //if this table had a measure but no dimension relationships (except to itself)
                {
                    DimensionUsage du = new DimensionUsage("Fact", table, table);
                    dimUsage.Add(du);
                }
                else if (tmp.Count == 0 && bIsBusMatrix && bFoundVisibleMeasure) //if this table with a measure had no dimension relationships, add it as such...
                {
                    DimensionUsage du = new DimensionUsage(string.Empty, table, null);
                    dimUsage.Add(du);
                }
                
            }

            List<DataModelingTable> listTables = new List<DataModelingTable>(sandbox.GetSandbox().Tables);

            //remove dimensions in relationships or hidden or not having any visible columns
            foreach (DimensionUsage du in dimUsage)
            {
                for (int i = 0; i < listTables.Count; i++)
                {
                    bool bFoundVisibleAttribute = false;
                    foreach (DataModelingColumn col in listTables[i].Columns)
                    {
                        if (!col.IsPrivate && col.IsAttributeHierarchyQueriable)
                        {
                            bFoundVisibleAttribute = true;
                            break;
                        }
                    }

                    if (!bFoundVisibleAttribute || listTables[i].Name == du.DimensionName || listTables[i].IsPrivate)
                    {
                        listTables.RemoveAt(i);
                        i--;
                        continue;
                    }

                }
            }

            //add any dimensions which aren't related to any fact tables
            foreach (DataModelingTable cd in listTables)
            {
                DimensionUsage du = new DimensionUsage(string.Empty, null, cd);
                dimUsage.Add(du);
            }

            return dimUsage;
        }
#endif
        private static List<DimensionUsage> RecurseTabularRelationships(Dimension dMG, MeasureGroup mgOuter, bool bIsBusMatrix)
        {
            List<DimensionUsage> list = new List<DimensionUsage>();
            foreach (Microsoft.AnalysisServices.Relationship relOuter in dMG.Relationships)
            {
                bool bFound = false;
                MeasureGroup mgFrom = dMG.Parent.Cubes[0].MeasureGroups[relOuter.FromRelationshipEnd.DimensionID];
                DimensionAttribute daFrom = dMG.Attributes[relOuter.FromRelationshipEnd.Attributes[0].AttributeID];
                Dimension dTo = dMG.Parent.Dimensions[relOuter.ToRelationshipEnd.DimensionID];
                DimensionAttribute daTo = dTo.Attributes[relOuter.ToRelationshipEnd.Attributes[0].AttributeID];
                CubeDimension dToCube = dMG.Parent.Cubes[0].Dimensions[relOuter.ToRelationshipEnd.DimensionID];
                foreach (MeasureGroupDimension mgdOuter in mgFrom.Dimensions)
                {
                    ReferenceMeasureGroupDimension rmgdOuter = mgdOuter as ReferenceMeasureGroupDimension;
                    if (rmgdOuter != null && rmgdOuter.Materialization == ReferenceDimensionMaterialization.Regular && rmgdOuter.RelationshipID == relOuter.ID)
                    {
                        //active relationships have a materialized reference relationship
                        bFound = true;
                        break;
                    }
                }
                string sActiveFlag = "Active";
                if (!bFound)
                {
                    sActiveFlag = "Inactive";
                    if (bIsBusMatrix) continue; //don't show inactive relationships in bus matrix view
                }

                DimensionUsage usage = new DimensionUsage(sActiveFlag, mgOuter, dToCube, dTo);
                usage.Column1Name = "Foreign Key Column";
                usage.Column1Value = daFrom.Name;
                usage.Column2Name = "Primary Key Column";
                usage.Column2Value = daTo.Name;

                bool bFoundVisibleAttribute = false;
                foreach (DimensionAttribute da in dTo.Attributes)
                {
                    if (da.AttributeHierarchyVisible)
                    {
                        bFoundVisibleAttribute = true;
                        break;
                    }
                }
                if (bFoundVisibleAttribute) //only if the To end has visible attributes should we show it as a dimension
                    list.Add(usage);

                if (bIsBusMatrix)
                {
                    //recurse if it's the bus matrix view
                    list.AddRange(RecurseTabularRelationships(dTo, mgOuter, bIsBusMatrix));
                }
            }

            return list;
        }

        private static List<DimensionUsage> RecurseTabularRelationships(DataModelingTable dimensionTable, DataModelingTable outerFactTable, bool bIsBusMatrix, List<Microsoft.AnalysisServices.BackEnd.Relationship> listRelationshipsTraversed, bool bManyToMany)
        {
            List<DimensionUsage> list = new List<DimensionUsage>();
            foreach (Microsoft.AnalysisServices.BackEnd.Relationship relOuter in dimensionTable.Sandbox.Relationships.RelationshipCollection)
            {
                if (listRelationshipsTraversed.Contains(relOuter)) continue; //don't double back on path

                DataModelingColumn reportedDimensionColumn = null;
                DimensionUsage usage = null;
                bool bThisRelationshipManyToMany = bManyToMany;
                string sRelationshipType = "Active";
                if (!relOuter.Active)
                {
                    sRelationshipType = "Inactive";
                    if (bIsBusMatrix) continue; //don't show inactive relationships in bus matrix view
                }

                if (bThisRelationshipManyToMany)
                    sRelationshipType = "Many to Many";

                if (relOuter.ToColumn.Table.Name == dimensionTable.Name 
                    && relOuter.CrossFilterDirection == Microsoft.AnalysisServices.BackEnd.CrossFilterDirection.Both
                    && relOuter.Active)
                {
                    sRelationshipType = "Many to Many";
                    reportedDimensionColumn = relOuter.FromColumn;
                    bThisRelationshipManyToMany = true;

                    usage = new DimensionUsage(sRelationshipType, outerFactTable, reportedDimensionColumn.Table);
                    usage.Column1Name = "Foreign Key Column";
                    usage.Column1Value = relOuter.ToColumn.Name;
                    usage.Column2Name = "Primary Key Column";
                    usage.Column2Value = relOuter.FromColumn.Name;

                }
                else if (relOuter.FromColumn.Table.Name != dimensionTable.Name)
                {
                    continue; //find any relationships that start from the "dimensionTable" table
                }
                else
                {
                    reportedDimensionColumn = relOuter.ToColumn;

                    usage = new DimensionUsage(sRelationshipType, outerFactTable, reportedDimensionColumn.Table);
                    usage.Column1Name = "Foreign Key Column";
                    usage.Column1Value = relOuter.FromColumn.Name;
                    usage.Column2Name = "Primary Key Column";
                    usage.Column2Value = relOuter.ToColumn.Name;
                }


                bool bFoundVisibleAttribute = false;
                foreach (DataModelingColumn col in reportedDimensionColumn.Table.Columns)
                {
                    if (!col.Table.IsPrivate && !col.IsPrivate && col.IsAttributeHierarchyQueriable)
                    {
                        bFoundVisibleAttribute = true;
                        break;
                    }
                }
                if (bFoundVisibleAttribute) //only if the To end has visible attributes should we show it as a dimension
                    list.Add(usage);

                if (bIsBusMatrix)
                {
                    List<Microsoft.AnalysisServices.BackEnd.Relationship> listLatestRelationshipsTraversed = new List<Microsoft.AnalysisServices.BackEnd.Relationship>();
                    listLatestRelationshipsTraversed.AddRange(listRelationshipsTraversed);
                    listLatestRelationshipsTraversed.Add(relOuter);

                    //recurse if it's the bus matrix view
                    list.AddRange(RecurseTabularRelationships(reportedDimensionColumn.Table, outerFactTable, bIsBusMatrix, listLatestRelationshipsTraversed, bThisRelationshipManyToMany));
                }
            }

            return list;
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

                            if (dsv.Schema.Tables.Contains(usage.Column2Value))
                            {
                                DataTable oTable = dsv.Schema.Tables[dsv.Schema.Tables.IndexOf(usage.Column2Value)];
                                if (oTable.ExtendedProperties.ContainsKey("FriendlyName"))
                                {
                                    usage.Column2Value = oTable.ExtendedProperties["FriendlyName"].ToString();
                                }
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
            if (mg != null)
            {
                mCubeName = mg.Parent.Name;
                mDatabaseName = mg.Parent.Parent.Name;
                mMeasureGroup = mg.Name;
            }
            else if (dimCube != null)
            {
                mCubeName = dimCube.Parent.Name;
                mDatabaseName = dimCube.Parent.Parent.Name;
            }
            if (dimCube != null)
            {
                mDimensionName = dimCube.Name;
                if (dimCube.Name != dim.Name) mDimensionName += " (" + dim.Name + ")";
            }
            mRelationshipType = relationshipType;
            //mFactTableColumnName = factTableColumn;
            //mAttributeColumnName = attributeColumn;
        }

        public DimensionUsage(string relationshipType, DataModelingTable factTable, DataModelingTable dimensionTable)
        {
            if (factTable != null)
            {
                mCubeName = factTable.Sandbox.ModelName;
                mDatabaseName = factTable.Sandbox.DatabaseName;
                mMeasureGroup = factTable.Name;
            }
            if (dimensionTable != null)
            {
                mCubeName = dimensionTable.Sandbox.ModelName;
                mDatabaseName = dimensionTable.Sandbox.DatabaseName;
                mDimensionName = dimensionTable.Name;
            }
            mRelationshipType = relationshipType;
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
            set { mCubeName = value; }
        }

        public string DatabaseName
        {
            get { return mDatabaseName; }
            set { mDatabaseName = value; }
        }

        public string ImageName
        {
            get
            {
                if (RelationshipType == "Active" || RelationshipType == "Inactive") return "relationshipRegular"; //tabular
                return "relationship" + RelationshipType.Replace(" ", string.Empty);
            }
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
