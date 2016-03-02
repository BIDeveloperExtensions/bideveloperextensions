using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using Microsoft.AnalysisServices;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace PCDimNaturalizer
{
    public abstract class PCDimNaturalizer
    {
        public const int WM_USER1 = 0x0400;
        public const int WM_USER2 = 0x0401;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static string LogFile;
        public IntPtr SourceWindowHandle;
        public int MinimumLevelCount;

        protected string txtNewView;
        protected OleDbConnection Conn;

        public static void LogText(string txt)
        {
            try
            { if (LogFile != null) File.AppendAllText(LogFile, DateTime.Now.ToUniversalTime() + ":  " + txt + "\r\n"); }
            catch (Exception) { }
        }

        protected void UpdateStatus(string NewText)
        {
            if (SourceWindowHandle != null)
            {
                IntPtr lparam = Marshal.StringToHGlobalAnsi(NewText);
                SendMessage(SourceWindowHandle, WM_USER1, IntPtr.Zero, lparam);
            }
            LogText(NewText);
        }

        protected void UpdateStatus(Bitmap NewImage)
        {
            if (SourceWindowHandle != null)
                SendMessage(SourceWindowHandle, WM_USER2, IntPtr.Zero, NewImage.GetHbitmap());
        }

        protected int GetLevelCountFromPCTable(string Table, string ID, string PID) { return GetLevelCountFromPCTable(Table, ID, PID, true); }

        protected int GetLevelCountFromPCTable(string Table, string ID, string PID, bool IsIDNumeric)
        {
            OleDbCommand cmd = new OleDbCommand(";WITH PCStructure(" + PID + ", " + ID + ", Level) AS (" +
                "SELECT " + PID + ", " + ID + ", 1 AS Level FROM " + Table +
                " WHERE " + PID + " IS NULL OR " + PID +
                " = " + (IsIDNumeric ? "0" : "''") + " OR " + PID + " = " + ID + " " +
                "UNION ALL  " +
                "SELECT e." + PID + ", e." + ID + ", Level + 1  " +
                "FROM " + Table + " e " +
                "INNER JOIN PCStructure d ON e." + PID + " = d." + ID + " AND e." + PID + " != e." + ID + ")  " +
                "SELECT Max(Level) FROM PCStructure", Conn);
            object res = cmd.ExecuteScalar();
            return (int)res;
        }

        public void Naturalize()
        {
            Naturalize(0);
        }

        public virtual void Naturalize(object iLevels) { }
    }

    public class SQLPCDimNaturalizer : PCDimNaturalizer
    {
        public List<string> SQLColsASPCAttributes = new List<string>(), SQLColsAsNonPCAttributes = new List<string>();

        private string table, id, pid;

        public SQLPCDimNaturalizer(OleDbConnection SQLConn, string SQLTableName, string SQLID, string SQLPID, int MinLevels)
        {
            try
            {
                int iDotLoc = SQLTableName.Trim().IndexOf('.');
                table = SQLTableName;
                id = SQLID;
                pid = SQLPID;
                txtNewView = SQLTableName.Substring(0, iDotLoc) + ".[DimNaturalized_" + SQLTableName.Substring(iDotLoc + 2);
                Conn = SQLConn;
            }
            catch (Exception e)
            {
                UpdateStatus(BIDSHelper.Resources.Common.ProcessError);
                UpdateStatus("Error initializing naturalizer:\r\n" + e.ToString());
                throw e;
            }
        }

        private void CreateNaturalizedView()
        {
            // Drop old view if it is there
            OleDbCommand cmd = new OleDbCommand("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'" + txtNewView + "') AND type in (N'V')) drop view " + txtNewView, Conn);
            cmd.ExecuteNonQuery();

            // Build naturalized CTE view
            string CTESel = "CREATE VIEW " + txtNewView + " AS\r\n" +
                "WITH PCStructure(Level, " + pid + ", CurrentMemberID, Level2, ";
            string CTEQry = "AS (SELECT 3 Level, " + pid + ", " + id + ", " + id + " as Level2,\r\n";
            string strCTELevelEnumeration = "FROM " + table + " WHERE " + pid + " IS NULL " +
                "UNION ALL SELECT Level + 1, e." + pid + ", e." + id + ", ";
            string strSel = "select CurrentMemberSubselect.*,\r\n";
            string strQry = "from PCStructure a\r\nleft outer join (select " + id + " CurrentMemberID, ";

            foreach (string attr in SQLColsAsNonPCAttributes)
                strQry += attr + ", ";
            strQry = strQry.Remove(strQry.Length - 2) + "\r\nfrom " + table + ") CurrentMemberSubselect on CurrentMemberSubselect.CurrentMemberID = a.CurrentMemberID\r\n";

            for (int i = 2; i <= MinimumLevelCount + 1; i++)
            {
                string LevelName = "Level" + i.ToString();

                if (i != 2)
                {
                    CTESel += LevelName + ", ";
                    CTEQry += "null as " + LevelName + ",\r\n"; // Level1 already included in CTE so skip that one
                }
                strCTELevelEnumeration += "CASE Level WHEN " + i.ToString() + " THEN e." + id + " ELSE " + LevelName + " END AS " + LevelName + ",\r\n";
                strSel += LevelName + "Subselect.*, ";
                strQry += "left outer join (select " + id + " " + LevelName + ",\r\n";
                foreach (string attr in SQLColsASPCAttributes)
                    strQry += "[" + attr + "] [" + LevelName + "_" + attr + "],\r\n"; 
                strQry = strQry.Remove(strQry.Length - 3) + "\r\nfrom " + table + ") " + LevelName + "Subselect on " + LevelName + "Subselect." + LevelName + " = a." + LevelName + "\r\n";
            }

            CTESel = CTESel.Remove(CTESel.Length - 2) + ")\r\n";
            CTEQry = CTEQry.Remove(CTEQry.Length - 3) + "\r\n";
            strCTELevelEnumeration = strCTELevelEnumeration.Remove(strCTELevelEnumeration.Length - 3) + " FROM " + table + " e " +
                    "INNER JOIN PCStructure d ON e." + pid + " = d.CurrentMemberID)\r\n";
            strQry = CTESel + CTEQry + strCTELevelEnumeration + strSel.Remove(strSel.Length - 2) + "\r\n" + strQry;
            cmd.CommandText = strQry;
            cmd.ExecuteNonQuery();

            DataSourceView dsv = Program.SQLFlattener.dsv;

            string strSchema = txtNewView.Substring(1, txtNewView.IndexOf(".") - 2);
            txtNewView = txtNewView.Substring(txtNewView.IndexOf(".") + 2); // Once it is in the DSV, the naturalized view no longer requires the schema name so strip it out...
            txtNewView = txtNewView.Substring(0, txtNewView.Length - 1);

            string sTableName = strSchema + "_" + txtNewView;

            //remove old version of this table
            if (dsv.Schema.Tables[sTableName] != null)
            {
                for (int i = dsv.Schema.Relations.Count - 1; i >= 0; i--)
                    if (dsv.Schema.Relations[i].RelationName.Contains(sTableName))
                        dsv.Schema.Relations.Remove(dsv.Schema.Relations[i]);
                for (int i = 0; i < dsv.Schema.Tables.Count; i++)
                    for (int j = 0; j < dsv.Schema.Tables[i].Constraints.Count; j++)
                        if (dsv.Schema.Tables[i].Constraints[j].ConstraintName.Contains(sTableName))
                            dsv.Schema.Tables[i].Constraints.Remove(dsv.Schema.Tables[i].Constraints[j]);
                dsv.Schema.Tables[sTableName].Constraints.Clear();
                dsv.Schema.Tables.Remove(sTableName);
            }

            DataTable tbl = Program.SQLFlattener.DataSourceConnection.FillDataSet(dsv.Schema, strSchema, txtNewView, "View");

            IContainer container = dsv.Schema.Site.Container;
            container.Add(tbl);
            foreach (DataColumn column in tbl.Columns)
            {
                container.Add(column);
            }
        }

        public override void Naturalize(object MinLevels)
        {
            try
            {
                // Initialize
                UpdateStatus("Initializing connections and calculating level depth...");
                if (Conn.State != ConnectionState.Open)
                    Conn.Open();
                MinimumLevelCount = GetLevelCountFromPCTable(table, "[" + id + "]", "[" + pid + "]");
                int iMinLevels = Convert.ToInt32(MinLevels);
                if (MinimumLevelCount < iMinLevels) MinimumLevelCount = iMinLevels;

                // Build natural DSV view
                UpdateStatus("Creating naturalized view for dimension...");
                CreateNaturalizedView();
                UpdateStatus("Operation complete.");
                UpdateStatus(BIDSHelper.Resources.Common.ProgressComplete);
            }
            catch (Exception e)
            {
                if (SourceWindowHandle != null && SourceWindowHandle.ToInt32() != 0)
                {
                    UpdateStatus("Error during: [" + Program.Progress.txtStatus.Text + "]\r\n" + e.ToString());
                    UpdateStatus(BIDSHelper.Resources.Common.ProcessError);
                }
                else
                {
                    if (LogFile != null)
                        File.WriteAllText(Program.LogFile, e.ToString());
                    throw e;
                }
            }
        }


    }

    public class ASPCDimNaturalizer : PCDimNaturalizer
    {
        public List<string> PCAttributesToInclude = new List<string>(), NonPCHierarchiesToInclude = new List<string>();
        public int ASNaturalizationActionLevel;

        private Server srv;
        private Database db;
        private Dimension dim;
        private DataTable tbl;
        private EnhancedColumnBinding id, pid;



        public ASPCDimNaturalizer(Server ASServer, Database ASDatabase, Dimension PCDimension, int ActionLevel)
        {
            try
            {
                srv = ASServer;
                db = ASDatabase;
                dim = PCDimension;
                txtNewView = ((EnhancedColumnBinding)dim.KeyAttribute.KeyColumns[0].Source).Table.ExtendedProperties["DbSchemaName"] + ".[DimNaturalized_" +
                        ((EnhancedColumnBinding)dim.KeyAttribute.KeyColumns[0].Source).Table.ExtendedProperties["DbTableName"] + "]";
                ASNaturalizationActionLevel = ActionLevel;
                InitIDCols();
            }
            catch (Exception e)
            {
                UpdateStatus(BIDSHelper.Resources.Common.ProcessError);
                UpdateStatus("Error initializing naturalizer:\r\n" + e.ToString());
                throw e;
            }
        }

        public ASPCDimNaturalizer(string ASServer, string ASDatabase, string PCDimension, int ActionLevel)
        {
            try
            {
                srv = new Server();
                srv.Connect("Integrated Security=SSPI;Persist Security Info=False;Data Source=" + ASServer);
                db = srv.Databases.GetByName(ASDatabase);
                dim = db.Dimensions.GetByName(PCDimension);
                if (((EnhancedColumnBinding)dim.KeyAttribute.KeyColumns[0].Source).Table.ExtendedProperties["DbSchemaName"] != null)
                    txtNewView = ((EnhancedColumnBinding)dim.KeyAttribute.KeyColumns[0].Source).Table.ExtendedProperties["DbSchemaName"] + ".[DimNaturalized_" +
                        ((EnhancedColumnBinding)dim.KeyAttribute.KeyColumns[0].Source).Table.ExtendedProperties["DbTableName"] + "]";
                else
                    txtNewView = "[DimNaturalized_" +
                        ((EnhancedColumnBinding)dim.KeyAttribute.KeyColumns[0].Source).Table.TableName + "]";
                ASNaturalizationActionLevel = ActionLevel;
                InitIDCols();
            }
            catch (Exception e)
            {
                UpdateStatus(BIDSHelper.Resources.Common.ProcessError);
                UpdateStatus("Error initializing naturalizer:\r\n" + e.ToString());
                throw e;
            }
        }

        private void InitIDCols()
        {
            id = ((EnhancedColumnBinding)dim.KeyAttribute.KeyColumns[0].Source);
            for (int i = 0; i < dim.Attributes.Count; i++)
                if (dim.Attributes[i].Usage == AttributeUsage.Parent)
                {
                    pid = (EnhancedColumnBinding)dim.Attributes[i].KeyColumns[0].Source;
                    break;
                }
            if (pid == null) throw new Exception("Dimension is not parent child.");
            if (id.Table.ExtendedProperties.ContainsKey("QueryDefinition"))
                throw new Exception("DSV named queries are not supported currently.  The PC Dimension Naturalizer only supports SQL tables or views for the dimension source.");
            tbl = id.Table;
        }

        void AddDimToCubes(Dimension Dim)
        {
            foreach (Cube cub in db.Cubes)
            {
                if (cub.Dimensions.Contains(dim.ID))
                    cub.Dimensions.Add(Dim.ID);

                foreach (MeasureGroup mg in cub.MeasureGroups)
                    if (mg.Dimensions.Contains(dim.ID))
                    {
                        RegularMeasureGroupDimension rmgd = mg.Dimensions.Add(Dim.ID);
                        MeasureGroupAttribute mga = rmgd.Attributes.Add(Dim.KeyAttribute.ID);
                        mga.Type = MeasureGroupAttributeType.Granularity;
                        foreach (DataItem keyCol in ((RegularMeasureGroupDimension)mg.Dimensions[dim.ID]).Attributes[dim.KeyAttribute.ID].KeyColumns)
                            mga.KeyColumns.Add(keyCol.Clone());
                    }
            }
        }



        string GetNaturalizedLevelName(int iLevel)
        {
            // Convoluted, but doing this to match the automatic PC naming behavior found in existing implementation of AS

            string[] NamingTemplate = null;
            if (PCParentAttribute().NamingTemplate != null)
                NamingTemplate = PCParentAttribute().NamingTemplate.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            else
                NamingTemplate = new string[] { "" };
            if (NamingTemplate[0] == "")
                return "Level " + iLevel.ToString("00");
            else
            {
                if (NamingTemplate.Length > iLevel - 1)
                    return NamingTemplate[iLevel - 2];
                else
                    return NamingTemplate[NamingTemplate.Length - 1].Contains("*")
                        ? NamingTemplate[NamingTemplate.Length - 1].Replace("*", (iLevel - 1).ToString("00"))
                        : NamingTemplate[NamingTemplate.Length - 1] == ""
                            ? NamingTemplate[0].Replace("*", (iLevel - 1).ToString())
                            : NamingTemplate.Length < iLevel - 1
                                ? NamingTemplate[NamingTemplate.Length - 1] + " " + (iLevel - 1).ToString("00")
                                : NamingTemplate[NamingTemplate.Length - 1];
            }
        }

        private Dimension MaterializeHierarchies(Dimension Dim, DataTable tblNew)
        {
            Dimension dimNew = Dim;
            Hierarchy hierNew = null;
            DimensionAttribute atParent = null;

            // Build naturalized PC hierarchy
            foreach (DimensionAttribute da in dim.Attributes)
                if (da.Usage == AttributeUsage.Parent)
                {
                    atParent = da;
                    break;
                }

            hierNew = dimNew.Hierarchies.Add(atParent.Name);
            hierNew.MemberKeysUnique = MemberKeysUnique.Unique;
            for (int i = 2; i <= MinimumLevelCount + 1; i++)
            {
                Level lvlNew = new Level(GetNaturalizedLevelName(i));
                lvlNew.SourceAttribute = dimNew.Attributes[lvlNew.Name];
                lvlNew.HideMemberIf = HideIfValue.ParentName;
                hierNew.Levels.Add(lvlNew);
            }


            // Migrate existing user hierarchies from PC dimension...
            foreach (Hierarchy hier in dim.Hierarchies)
            {
                if (NonPCHierarchiesToInclude.Contains(hier.Name))
                {
                    if (dimNew.Attributes.ContainsName(hier.Name))
                        hierNew = dimNew.Hierarchies.Add(hier.Name + "Hierarchy");
                    else
                        hierNew = dimNew.Hierarchies.Add(hier.Name);
                    for (int j = 0; j < hier.Levels.Count; j++)
                    {
                        hierNew.Levels.Add(new Level(hier.Levels[j].Name));
                        if (hier.Levels[j].SourceAttribute.Usage == AttributeUsage.Regular)
                            hierNew.Levels[j].SourceAttribute = dimNew.Attributes[hier.Levels[j].SourceAttribute.Name];
                        else
                            hierNew.Levels[j].SourceAttribute = dimNew.Attributes[dim.KeyAttribute.Name];
                        hierNew.Levels[j].HideMemberIf = hier.Levels[j].HideMemberIf;
                    }
                }
            }

            return dimNew;
        }

        public static bool IsAttributeRelated(DimensionAttribute attr, DimensionAttribute key)
        {
            foreach (AttributeRelationship atrel in key.AttributeRelationships)
                if (key.AttributeRelationships.Contains(attr.ID) || IsAttributeRelated(attr, atrel.Attribute))
                    return true;
            return false;
        }

        public static List<List<DimensionAttribute>> GetAttrRelOwnerChainToKey(CheckedListBox.CheckedItemCollection Attrs)
        {
            List<List<DimensionAttribute>> lists = new List<List<DimensionAttribute>>();
            foreach (object obj in Attrs)
            {
                DimensionAttribute attr = Program.ASFlattener.dim.Attributes.GetByName(obj.ToString());
                lists.Add(GetAttrRelOwnerChainToKey(attr));
            }
            return lists;
        }

        public static List<DimensionAttribute> GetAttrRelOwnerChainToKey(DimensionAttribute Attr)
        {
            List<DimensionAttribute> atList = new List<DimensionAttribute>();
            atList.Add(Attr);

            foreach (DimensionAttribute atRelOwner in Program.ASFlattener.dim.Attributes)
                if (atRelOwner.AttributeRelationships.ContainsName(Attr.Name))
                {
                    if (atRelOwner.Usage == AttributeUsage.Regular)
                        atList.AddRange(GetAttrRelOwnerChainToKey(atRelOwner));
                    atList.Add(atRelOwner);
                    return atList;
                }
            return atList;
        }

        private Dimension MaterializeNaturalizedAttributeRelationships(Dimension Dim, DataTable tblNew)
        {
            Dimension dimNew = Dim;
            // Prepare for headache...
            try
            {
                // For each attribute in the original dimension
                foreach (DimensionAttribute attRelOwner in dim.Attributes)
                    // Skip it if it is the special parent attribute of PC dimension since that is not in the naturalized dimension
                    if (attRelOwner.Usage != AttributeUsage.Parent)
                        // Then loop through each attribute in the original dimension again to see if it is related to the first attribute
                        foreach (DimensionAttribute attRelated in dim.Attributes)
                            if (attRelOwner.AttributeRelationships.Contains(attRelated.ID) && attRelated.Usage != AttributeUsage.Parent)
                            {
                                // If the source and destination are both attributes selected for inclusion in the dimension and both are related to the dimension key directly or indirectly
                                if ((PCAttributesToInclude.Contains(attRelOwner.Name) || attRelOwner.Usage == AttributeUsage.Key)
                                    && PCAttributesToInclude.Contains(attRelated.Name)
                                    && IsAttributeRelated(attRelated, dim.KeyAttribute))
                                    // Then loop through the levels to add a relationship for each level member corresponding to the original PC attribute
                                    for (int k = 2; k <= MinimumLevelCount + 1; k++)
                                        // If it was related to the PC key, relate directly to level name in naturalized version, otherwise relate to the corresponding related attribute for the level...
                                        if (attRelOwner.Usage == AttributeUsage.Key)
                                            dimNew.Attributes[GetNaturalizedLevelName(k)].AttributeRelationships.Add(
                                                new AttributeRelationship(GetNaturalizedLevelName(k) + "_" + attRelated.Name));
                                        else
                                            dimNew.Attributes[GetNaturalizedLevelName(k) + "_" + attRelOwner.Name].AttributeRelationships.Add(
                                                    new AttributeRelationship(GetNaturalizedLevelName(k) + "_" + attRelated.Name));
                                // Finally if attribute exists as part of non-PC hierarchy, it will have a name without "LevelX" in front, so we need to add relationship for that
                                if (dimNew.Attributes.Contains(attRelated.Name) && attRelOwner.Usage != AttributeUsage.Key)
                                    dimNew.Attributes[attRelOwner.Name].AttributeRelationships.Add(
                                                new AttributeRelationship(attRelated.Name));
                            }

                // Relate each level of the naturalized hierarchy to the one below it to optimize hierarchy navigation
                for (int i = 3; i <= MinimumLevelCount + 1; i++)
                    dimNew.Attributes[GetNaturalizedLevelName(i)].AttributeRelationships.Add(new AttributeRelationship(GetNaturalizedLevelName(i - 1)));

                //relate the lowest level to the key
                dimNew.KeyAttribute.AttributeRelationships.Add(new AttributeRelationship(GetNaturalizedLevelName(MinimumLevelCount + 1)));

                //any attributes which aren't related directly or indirectly to the key, add that attribute relationship
                foreach (DimensionAttribute attr in dimNew.Attributes)
                {
                    if (attr.ID != dimNew.KeyAttribute.ID && !IsAttributeRelated(attr, dimNew.KeyAttribute))
                    {
                        dimNew.KeyAttribute.AttributeRelationships.Add(new AttributeRelationship(attr.ID));
                    }
                }

                return dimNew;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private DimensionAttribute CloneAttributeSkeleton(DimensionAttribute OriginalAttribute, string NewIDAndName)
        {
            DimensionAttribute attr = OriginalAttribute.Clone();
            attr.Name = NewIDAndName;
            attr.ID = NewIDAndName;
            attr.AttributeRelationships.Clear();
            attr.KeyColumns.Clear();
            attr.CustomRollupColumn = null;
            attr.UnaryOperatorColumn = null;
            attr.NameColumn = null;
            attr.ValueColumn = null;
            attr.OrderBy = OrderBy.Key;
            attr.OrderByAttributeID = null;

            // Do not know why but when this was copied from PC settings it caused failure in new attributes, possibly because of breakdown into levels?
            attr.DiscretizationMethod = DiscretizationMethod.None;
            return attr;
        }

        private Dimension MaterializeSingleNonPCUserHierarchyAttribute(Dimension Dim, DataTable tblNew, DimensionAttribute attr)
        {
            Dimension dimNew = Dim;
            if (attr.Usage == AttributeUsage.Regular)
            {
                DimensionAttribute attrNew = CloneAttributeSkeleton(attr, attr.Name);
                for (int k = 0; k < attr.KeyColumns.Count; k++)
                {
                    string ColName = "CurrentMember_" + ((EnhancedColumnBinding)attr.KeyColumns[k].Source).ColumnName.Trim('[').Trim(']');
                    attrNew.KeyColumns.Add(new DataItem(txtNewView, ColName,
                        OleDbTypeConverter.GetRestrictedOleDbType(tblNew.Columns[ColName].DataType),
                        tblNew.Columns[ColName].MaxLength));
                }
                if (attr.NameColumn != null)
                {
                    string ColName = "CurrentMember_" + ((EnhancedColumnBinding)attr.NameColumn.Source).ColumnName.Trim('[').Trim(']');
                    attrNew.NameColumn = new DataItem(txtNewView, ColName,
                        OleDbType.WChar,  // Name must always be string even though source column has in some customer cases not been...
                        tblNew.Columns[ColName].MaxLength);
                }
                if (attr.ValueColumn != null)
                {
                    string ColName = "CurrentMember_" + ((EnhancedColumnBinding)attr.ValueColumn.Source).ColumnName.Trim('[').Trim(']');
                    attrNew.ValueColumn = new DataItem(txtNewView, ColName,
                        OleDbTypeConverter.GetRestrictedOleDbType(tblNew.Columns[ColName].DataType),
                        tblNew.Columns[ColName].MaxLength);
                }


                dimNew.Attributes.Add(attrNew);
            }
            return dimNew;
        }

        private Dimension MaterializeNonPCUserHierarchyAttributes(Dimension Dim, DataTable tblNew)
        {
            Dimension dimNew = Dim;

            foreach (DimensionAttribute attr in dim.Attributes)
                if (NonPCHierarchiesToInclude.Contains(attr.Name))
                    dimNew = MaterializeSingleNonPCUserHierarchyAttribute(dimNew, tblNew, attr);

            return dimNew;
        }

        private string ColNameFromDataItem(DataItem item)
        {
            return ((EnhancedColumnBinding)item.Source).ColumnName.Trim('[').Trim(']'); ;
        }

        private DimensionAttribute MaterializeAttributeColumns(DimensionAttribute attrOrig, DataTable tblNew, string MemberName)
        {
            DimensionAttribute attr = null;
            attr = CloneAttributeSkeleton(attrOrig, MemberName);


            attr.KeyColumns.Add(
                new DataItem(txtNewView, MemberName + "_KeyColumn", OleDbTypeConverter.GetRestrictedOleDbType(tblNew.Columns[MemberName + "_KeyColumn"].DataType), tblNew.Columns[MemberName + "_KeyColumn"].MaxLength));
            if (attrOrig.NameColumn != null)
                if (attrOrig.KeyColumns.Count > 1 || ColNameFromDataItem(attrOrig.KeyColumns[0]) != ColNameFromDataItem(attrOrig.NameColumn))
                    attr.NameColumn = new DataItem(txtNewView, MemberName + "_NameColumn", OleDbTypeConverter.GetRestrictedOleDbType(tblNew.Columns[MemberName + "_NameColumn"].DataType), tblNew.Columns[MemberName + "_NameColumn"].MaxLength);
                else
                    attr.NameColumn = new DataItem(txtNewView, MemberName + "_KeyColumn", OleDbType.WChar, tblNew.Columns[MemberName + "_KeyColumn"].MaxLength);  // Name column must always be string type.  In at least one case, data used an integer column, but name column still had to be string or error occurs...
            if (attrOrig.ValueColumn != null)
                if (attrOrig.KeyColumns.Count > 1 || ColNameFromDataItem(attrOrig.KeyColumns[0]) != ColNameFromDataItem(attrOrig.ValueColumn) && ColNameFromDataItem(attrOrig.NameColumn) != ColNameFromDataItem(attrOrig.ValueColumn))
                    attr.ValueColumn = new DataItem(txtNewView, MemberName + "_ValueColumn", OleDbTypeConverter.GetRestrictedOleDbType(tblNew.Columns[MemberName + "_ValueColumn"].DataType), tblNew.Columns[MemberName + "_ValueColumn"].MaxLength);
                else
                    attr.ValueColumn = new DataItem(txtNewView, MemberName + "_KeyColumn", OleDbTypeConverter.GetRestrictedOleDbType(tblNew.Columns[MemberName + "_KeyColumn"].DataType), tblNew.Columns[MemberName + "_KeyColumn"].MaxLength);
            if (MemberName != dim.KeyAttribute.Name)
            {
                if (attrOrig.UnaryOperatorColumn != null || (attrOrig.Usage == AttributeUsage.Key && PCParentAttribute().UnaryOperatorColumn != null))
                    attr.UnaryOperatorColumn = new DataItem(txtNewView, MemberName + "_UnaryOperatorColumn", OleDbTypeConverter.GetRestrictedOleDbType(tblNew.Columns[MemberName + "_UnaryOperatorColumn"].DataType), tblNew.Columns[MemberName + "_UnaryOperatorColumn"].MaxLength);
                if (attrOrig.CustomRollupColumn != null || (attrOrig.Usage == AttributeUsage.Key && PCParentAttribute().CustomRollupColumn != null))
                    attr.CustomRollupColumn = new DataItem(txtNewView, MemberName + "_CustomRollupColumn", OleDbTypeConverter.GetRestrictedOleDbType(tblNew.Columns[MemberName + "_CustomRollupColumn"].DataType), tblNew.Columns[MemberName + "_CustomRollupColumn"].MaxLength);
                if (attrOrig.OrderByAttribute != null)
                    attr.OrderByAttributeID = MemberName + "_" + attrOrig.OrderByAttribute.Name;
            }
            return attr;
        }

        private Dimension MaterializeNaturalizedDimensionAttributes(Dimension Dim, DataTable tblNew)
        {
            Dimension dimNew = Dim;
            foreach (DimensionAttribute attrOrig in dim.Attributes)
            {
                DimensionAttribute attr = null;
                string strNewAttribName = null;
                for (int j = 2; j <= MinimumLevelCount + 1; j++)
                {
                    if (PCAttributesToInclude.Contains(attrOrig.Name) || attrOrig.Usage == AttributeUsage.Key)
                    {
                        if (attrOrig.Usage != AttributeUsage.Parent)
                        {
                            if (attrOrig.Usage == AttributeUsage.Key)
                            {
                                if (j == 2)
                                {
                                    attr = MaterializeAttributeColumns(attrOrig, tblNew, dim.KeyAttribute.Name);
                                    attr.Usage = AttributeUsage.Key;
                                    attr.AttributeHierarchyVisible = false;
                                    dimNew.Attributes.Add(attr);
                                }
                                strNewAttribName = GetNaturalizedLevelName(j);
                            }
                            else
                                strNewAttribName = GetNaturalizedLevelName(j) + "_" + attrOrig.Name;
                            attr = MaterializeAttributeColumns(attrOrig, tblNew, strNewAttribName);

                            attr.Usage = AttributeUsage.Regular;

                            // Do not create PC attribute hierarchy unless another attribute requires it to relate to key.
                            // Non PC attribute hierarchies can be (and are by default) created side-by-side with PC hierarchy by selecting them in options dialog.
                            if (attrOrig.Usage == AttributeUsage.Regular)
                            {
                                bool AttrRelatesOtherAttrToKey = false;
                                foreach (DimensionAttribute attrTestRelated in dim.Attributes)
                                    if (IsAttributeRelated(attrTestRelated, attrOrig))
                                    {
                                        AttrRelatesOtherAttrToKey = true;
                                        break;
                                    }
                                if (!AttrRelatesOtherAttrToKey)
                                {
                                    attr.AttributeHierarchyEnabled = false;
                                    attr.AttributeHierarchyVisible = false;
                                }
                            }

                            dimNew.Attributes.Add(attr);
                        }
                    }
                }
            }
            return dimNew;
        }

        private Dimension MaterializeNaturalizedDimension()
        {
            try
            {

                Dimension dimNew = dim.Parent.Dimensions.GetByName(dim.Name + "_Naturalized");
                DataTable tblNew = dim.DataSourceView.Schema.Tables[txtNewView];
                //foreach (Cube cub in db.Cubes)
                //{
                //    foreach (MeasureGroup mg in cub.MeasureGroups)
                //        if (mg.Dimensions.Contains(dim.Name + "_Naturalized")) mg.Dimensions.Remove(dim.Name + "_Naturalized", true);
                //    if (cub.Dimensions.Contains(dim.Name + "_Naturalized")) cub.Dimensions.Remove(dim.Name + "_Naturalized", true);


                //}
                //db.Dimensions.Remove(dim.Name + "_Naturalized");
                //dimNew.Name = dim.Name + "_Naturalized";
                //dimNew.ID = dimNew.Name;
                dimNew.Annotations.Clear();
                dimNew.Attributes.Clear();
                dimNew.Hierarchies.Clear();

                dimNew = MaterializeNaturalizedDimensionAttributes(dimNew, tblNew);
                dimNew = MaterializeNonPCUserHierarchyAttributes(dimNew, tblNew);
                dimNew = MaterializeNaturalizedAttributeRelationships(dimNew, tblNew);
                dimNew = MaterializeHierarchies(dimNew, tblNew);

                //db.Dimensions.Add(dimNew);
                return dimNew;
            }
            catch (Exception e)
            {
                throw e;
            }
        }




        private string GetNonPCColNameOrExpressionFromDSVColumn(DataItem DSVColumn)
        {
            EnhancedColumnBinding col = (EnhancedColumnBinding)DSVColumn.Source;
            if (col.NamedCalculationExpression != "")
                return col.NamedCalculationExpression + " [CurrentMember_" + col.ColumnName.Trim('[');
            else
                return col.ColumnName + " [CurrentMember_" + col.ColumnName.Trim('[');
        }

        private void CreateRelationshipsForNamedQueryInDSV()
        {
            dim.DataSourceView.Schema.Tables[txtNewView].PrimaryKey = new DataColumn[] { dim.DataSourceView.Schema.Tables[txtNewView].Columns[dim.KeyAttribute.Name + "_KeyColumn"] };

            // Add child relationships (primary key constraints against this table) for new named query in view
            for (int i = 0; i < tbl.ChildRelations.Count; i++)
            {
                if (tbl.ChildRelations[i].ParentTable != tbl.ChildRelations[i].ChildTable)
                {
                    DataColumn[] NewRelationColumns = tbl.ChildRelations[i].ParentColumns;
                    for (int j = 0; j < NewRelationColumns.Length; j++)
                        NewRelationColumns[j] = dim.DataSourceView.Schema.Tables[txtNewView].Columns[dim.KeyAttribute.Name + "_KeyColumn"];

                    DataRelation dataRelation = new DataRelation(tbl.ChildRelations[i].RelationName.Replace((string)tbl.ChildRelations[i].ParentTable.ExtendedProperties["FriendlyName"], txtNewView),
                        NewRelationColumns,
                        tbl.ChildRelations[i].ChildColumns);
                    dim.DataSourceView.Schema.Relations.Add(dataRelation);
                }
            }
            // Add parent relationships (foriegn key constraints against this table) for new named query in view...
            for (int i = 0; i < tbl.ParentRelations.Count; i++)
            {
                for (int j = 2; j <= MinimumLevelCount + 1; j++) // Must add a FK relationship for each level in flattened hierarchy!  argh!
                {
                    if (tbl.ParentRelations[i].ParentTable != tbl.ParentRelations[i].ChildTable)
                    {
                        bool MissingRelationColumnInImportedDimension = false;
                        DataColumn[] NewRelationColumns = tbl.ParentRelations[i].ChildColumns;
                        for (int k = 0; k < NewRelationColumns.Length; k++)
                        {
                            string strFKCol = "";
                            for (int l = 0; l < dim.Attributes.Count; l++)  // OK, find the current FK column in the attribute columns list to figure out its name in the named query...
                            {
                                if (dim.Attributes[l].KeyColumns.Count == 1 && ColNameFromDataItem(dim.Attributes[l].KeyColumns[0]) == NewRelationColumns[k].ColumnName)
                                    strFKCol = GetNaturalizedLevelName(j) + "_" + dim.Attributes[l].Name + "_KeyColumn";
                                else if (dim.Attributes[l].NameColumn != null && ColNameFromDataItem(dim.Attributes[l].NameColumn) == NewRelationColumns[k].ColumnName)
                                    strFKCol = GetNaturalizedLevelName(j) + "_" + dim.Attributes[l].Name + "_NameColumn";
                                else if (dim.Attributes[l].ValueColumn != null && ColNameFromDataItem(dim.Attributes[l].ValueColumn) == NewRelationColumns[k].ColumnName)
                                    strFKCol = GetNaturalizedLevelName(j) + "_" + dim.Attributes[l].Name + "_ValueColumn";
                                if (strFKCol != "") break;
                            }
                            NewRelationColumns[k] = dim.DataSourceView.Schema.Tables[txtNewView].Columns[strFKCol];
                            if (NewRelationColumns[k] == null)
                            {
                                MissingRelationColumnInImportedDimension = true;
                                break;
                            }
                        }
                        if (!MissingRelationColumnInImportedDimension)
                        {
                            DataRelation dataRelation = new DataRelation("[" + tbl.ParentRelations[i].RelationName.Replace(tbl.ExtendedProperties["DbTableName"].ToString(), txtNewView.Replace("[", "").Replace("]", "") + j.ToString()) + "]",
                                tbl.ParentRelations[i].ParentColumns,
                                NewRelationColumns);

                            //dedup relation name
                            if (dim.DataSourceView.Schema.Relations.Contains(dataRelation.RelationName))
                            {
                                if (tbl.ExtendedProperties.ContainsKey("FriendlyName"))
                                {
                                    dataRelation.RelationName = dataRelation.RelationName.Replace(tbl.ExtendedProperties["FriendlyName"].ToString(), txtNewView.Replace("[", "").Replace("]", "") + j.ToString());
                                }
                                string sStartingName = dataRelation.RelationName;
                                int iUniqueCounter = 1;
                                while (dim.DataSourceView.Schema.Relations.Contains(dataRelation.RelationName))
                                {
                                    dataRelation.RelationName = sStartingName.Substring(0, sStartingName.Length-1) + (iUniqueCounter++) + "]";
                                }
                            }

                            dim.DataSourceView.Schema.Relations.Add(dataRelation);
                        }
                    }
                }
            }
        }

        private void ClearDSVOfOldVersionOfNamedQueryAndConstraints()
        {
            try
            {
                if (dim.DataSourceView.Schema.Tables[txtNewView] != null)
                {
                    for (int i = dim.DataSourceView.Schema.Relations.Count - 1; i >= 0; i--)
                        if (dim.DataSourceView.Schema.Relations[i].RelationName.Contains(txtNewView))
                            dim.DataSourceView.Schema.Relations.Remove(dim.DataSourceView.Schema.Relations[i]);
                    for (int i = 0; i < dim.DataSourceView.Schema.Tables.Count; i++)
                        for (int j = 0; j < dim.DataSourceView.Schema.Tables[i].Constraints.Count; j++)
                            if (dim.DataSourceView.Schema.Tables[i].Constraints[j].ConstraintName.Contains(txtNewView))
                                dim.DataSourceView.Schema.Tables[i].Constraints.Remove(dim.DataSourceView.Schema.Tables[i].Constraints[j]);
                    dim.DataSourceView.Schema.Tables[txtNewView].Constraints.Clear();
                    dim.DataSourceView.Schema.Tables.Remove(txtNewView);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void AddNaturalizedViewToDSV()
        {
            string strQry = "select top 1 * from " + txtNewView;
            string strSchema = "";
            if (txtNewView.IndexOf(".") != -1) strSchema = txtNewView.Substring(0, txtNewView.IndexOf("."));
            txtNewView = txtNewView.Substring(txtNewView.IndexOf(".") + 1).Replace("[", "").Replace("]", ""); // Once it is in the DSV, the naturalized view no longer requires the schema name so strip it out...

            if (txtNewView.StartsWith("[") && txtNewView.EndsWith("]"))
                txtNewView = txtNewView.Substring(1, txtNewView.Length - 2);

            string sTableName = txtNewView;
            txtNewView = strSchema + "_" + txtNewView;

            ClearDSVOfOldVersionOfNamedQueryAndConstraints();

            DataSourceView dsv = dim.DataSourceView;

            DataTable tbl = Program.ASFlattener.DataSourceConnection.FillDataSet(dsv.Schema, strSchema, sTableName, "View");

            IContainer container = dsv.Schema.Site.Container;
            container.Add(tbl);
            foreach (DataColumn column in tbl.Columns)
            {
                container.Add(column);
            }

            CreateRelationshipsForNamedQueryInDSV();
        }

        private string GetPartialQueryForSingleAttributeColumn(DimensionAttribute Attr, DataItem Col, string ColType, int iLevel)
        {
            string qryView = "";
            string AttribName = "";
            string LevelName = GetNaturalizedLevelName(iLevel);
            EnhancedColumnBinding cb = (EnhancedColumnBinding)Col.Source;
            if (Attr.Usage != AttributeUsage.Key)
                AttribName = "_" + Attr.Name;
            if (cb.NamedCalculationExpression != "")
                qryView += cb.NamedCalculationExpression + " [" + LevelName + AttribName + "_" + ColType + "]";
            else
                qryView += cb.ColumnName + " [" + LevelName + AttribName + "_" + ColType + "]";
            return qryView;
        }

        private string GetPartialQueryForAttributeColumns(DimensionAttribute Attr, DataItemCollection Cols, string ColType, int iLevel)
        {
            try
            {
                string qryView = "";
                if (Cols.Count > 1)
                {
                    EnhancedColumnBinding cb = null;
                    for (int k = 0; k < Cols.Count; k++)
                    {
                        cb = (EnhancedColumnBinding)Cols[k].Source;
                        if (cb.NamedCalculationExpression != "")
                            qryView += "CONVERT(VARCHAR(MAX), " + cb.NamedCalculationExpression + ") + ";
                        else
                            qryView += "CONVERT(VARCHAR(MAX), " + cb.ColumnName + ") + ";
                    }
                    qryView = qryView.Trim().Trim('+');
                    qryView += " [" + GetNaturalizedLevelName(iLevel) + "_" + Attr.Name + "_" + ColType + "]";
                }
                else
                    qryView += GetPartialQueryForSingleAttributeColumn(Attr, Cols[0], ColType, iLevel);
                return qryView;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetPartialQueryForAttributeColumns(DimensionAttribute Attr, DataItem Col, string ColType, int iLevel)
        {
            return GetPartialQueryForSingleAttributeColumn(Attr, Col, ColType, iLevel); ;
        }

        private DimensionAttribute PCParentAttribute()
        {
            foreach (DimensionAttribute da in dim.Attributes)
                if (da.Usage == AttributeUsage.Parent)
                    return da;
            return null;
        }

        private string GetSingleNonPCUserHierarchyColumnNamesFromOriginalDimension(DimensionAttribute attr)
        {
            string strSubselect = "";
            bool NameColumnCoveredInKey = false;
            bool ValueColumnCoveredInKey = false;
            foreach (DataItem di in attr.KeyColumns)
            {
                strSubselect += GetNonPCColNameOrExpressionFromDSVColumn(di) + ", -- End of column definition\r\n";
                if (attr.NameColumn != null && ColNameFromDataItem(di) == ColNameFromDataItem(attr.NameColumn))
                    NameColumnCoveredInKey = true;
                if (attr.ValueColumn != null && ColNameFromDataItem(di) == ColNameFromDataItem(attr.ValueColumn))
                    ValueColumnCoveredInKey = true;
            }
            if (!NameColumnCoveredInKey && attr.NameColumn != null)
                strSubselect += GetNonPCColNameOrExpressionFromDSVColumn(attr.NameColumn) + ", -- End of column definition\r\n";
            if (!ValueColumnCoveredInKey && attr.ValueColumn != null)
                strSubselect += GetNonPCColNameOrExpressionFromDSVColumn(attr.ValueColumn) + ", -- End of column definition\r\n";
            return strSubselect;
        }

        private string GetSingleColumnNameAndExpressionForView(DataItem attr, string ColNameForView)
        {
            string strSubselect = "";
            EnhancedColumnBinding col = (EnhancedColumnBinding)attr.Source;
            if (col.NamedCalculationExpression != "")
                strSubselect += col.NamedCalculationExpression + " " + ColNameForView + ",\r\n";
            else
                strSubselect += col.ColumnName + " " + ColNameForView + ",\r\n";
            return strSubselect;
        }

        private string GetNonPCUserHierarchyColumnNamesFromOriginalDimension()
        {
            string strSubselect = " CurrentMemberSubselect.* from " + id.TableName + " b,\r\n(select " + id.ColumnName + " [" + dim.KeyAttribute.Name + "_KeyColumn], ";
            string strColAlias = "";

            if (dim.KeyAttribute.NameColumn != null && ColNameFromDataItem(dim.KeyAttribute.KeyColumns[0]) != ColNameFromDataItem(dim.KeyAttribute.NameColumn))
            {
                strColAlias = GetSingleColumnNameAndExpressionForView(dim.KeyAttribute.NameColumn, "[" + dim.KeyAttribute.Name + "_NameColumn]");
                if (!strSubselect.Contains(strColAlias)) strSubselect += strColAlias;
            }

            if (dim.KeyAttribute.ValueColumn != null && ColNameFromDataItem(dim.KeyAttribute.KeyColumns[0]) != ColNameFromDataItem(dim.KeyAttribute.ValueColumn))
            {
                strColAlias = GetSingleColumnNameAndExpressionForView(dim.KeyAttribute.ValueColumn, "[" + dim.KeyAttribute.Name + "_ValueColumn]");
                if (!strSubselect.Contains(strColAlias)) strSubselect += strColAlias;
            }

            foreach (DimensionAttribute attr in dim.Attributes)
                if (NonPCHierarchiesToInclude.Contains(attr.Name))
                {
                    strColAlias = GetSingleNonPCUserHierarchyColumnNamesFromOriginalDimension(attr);
                    string[] strAllCols = strColAlias.Split(new string[] { ", -- End of column definition\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string strCurCol in strAllCols)
                        if (!strSubselect.Contains(strCurCol))
                            strSubselect += strCurCol.Replace(", -- End of column definition\r\n", "") + ",\r\n";
                }

            return strSubselect.Remove(strSubselect.Length - 3) + "\r\nfrom " + id.TableName + " b)\r\nCurrentMemberSubselect\r\n";
        }

        private void CreateNaturalizedView()
        {
            try
            {
                // Drop old view if it is there
                OleDbCommand cmd = new OleDbCommand("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'" + txtNewView + "') AND type in (N'V')) drop view " + txtNewView, Conn);
                cmd.ExecuteNonQuery();

                // Build naturalized CTE view.  
                string CTESel = "";
                string CTEQry = "";
                string strCTELevelEnumeration = "";
                string strLevelsEnumerations = "";
                string strSel = "select Level" + (MinimumLevelCount + 1).ToString() + "Subselect.*\r\n";
                string strQry = "from PCStructure a, \r\n";
                string strSelEnd = "";
                string strWhere = "where\r\n";

                for (int i = MinimumLevelCount + 1; i > 1; i--)
                {
                    string LevelName = GetNaturalizedLevelName(i);

                    if (i != 2)
                    {
                        CTESel = "[" + LevelName + "_KeyColumn], " + CTESel;
                        CTEQry = id.ColumnName + " as [" + LevelName + "_KeyColumn],\r\n" + CTEQry;
                    }
                    strCTELevelEnumeration = "CASE Level\r\n";
                    for (int j = 2; j < i; j++)
                    {
                        strCTELevelEnumeration += "WHEN " + j.ToString() + " THEN e." + id.ColumnName + "\r\n";
                    }
                    strCTELevelEnumeration += "WHEN " + i.ToString() + " THEN e." + id.ColumnName + " ELSE [" + LevelName + "_KeyColumn]\r\nEND\r\nAS [" + LevelName + "_KeyColumn],\r\n";
                    strQry += "(select " + id.ColumnName + " [" + LevelName + "_KeyColumn], ";


                    if (PCParentAttribute().UnaryOperatorColumn != null)
                    {
                        if (i > 2)
                            strQry += "CASE WHEN " + id.ColumnName + " = [" + GetNaturalizedLevelName(i - 1) + "_KeyColumn] THEN null ELSE ";
                        EnhancedColumnBinding col = (EnhancedColumnBinding)PCParentAttribute().UnaryOperatorColumn.Source;
                        if (col.NamedCalculationExpression != "")
                            strQry += col.NamedCalculationExpression + " " + (i > 2 ? " END [" : "[") + LevelName + "_UnaryOperatorColumn],\r\n";
                        else
                            strQry += col.ColumnName + " " + (i > 2 ? " END [" : "[") + LevelName + "_UnaryOperatorColumn],\r\n";
                    }

                    if (PCParentAttribute().CustomRollupColumn != null)
                    {
                        if (i > 2)
                            strQry += "CASE WHEN " + id.ColumnName + " = [" + GetNaturalizedLevelName(i - 1) + "_KeyColumn] THEN null ELSE ";
                        EnhancedColumnBinding col = (EnhancedColumnBinding)PCParentAttribute().CustomRollupColumn.Source;
                        if (col.NamedCalculationExpression != "")
                            strQry += col.NamedCalculationExpression
                                + (i > 2 ? " END [" : " [")
                                + LevelName + "_CustomRollupColumn],\r\n";
                        else
                            strQry += col.ColumnName
                                + (i > 2 ? " END [" : " [")
                                + LevelName + "_CustomRollupColumn],\r\n";
                    }

                    List<DimensionAttribute> das = new List<DimensionAttribute>();
                    foreach (DimensionAttribute datr in dim.Attributes)
                    {
                        if (PCAttributesToInclude.Contains(datr.Name) || datr.Usage == AttributeUsage.Key)
                        {
                            while (true)
                            {
                                DimensionAttribute da = datr;
                                if (da.Usage != AttributeUsage.Key)
                                    strQry += GetPartialQueryForAttributeColumns(da, da.KeyColumns, "KeyColumn", i) + ",\r\n";
                                if (da.NameColumn != null && (da.KeyColumns.Count > 1 || ColNameFromDataItem(da.KeyColumns[0]) != ColNameFromDataItem(da.NameColumn)))
                                    strQry += GetPartialQueryForAttributeColumns(da, da.NameColumn, "NameColumn", i) + ",\r\n";
                                if (da.ValueColumn != null && (da.KeyColumns.Count > 1 || ColNameFromDataItem(da.KeyColumns[0]) != ColNameFromDataItem(da.ValueColumn) && ColNameFromDataItem(da.NameColumn) != ColNameFromDataItem(da.ValueColumn)))
                                    strQry += GetPartialQueryForAttributeColumns(da, da.ValueColumn, "ValueColumn", i) + ",\r\n";
                                if (da.OrderByAttribute != null && !PCAttributesToInclude.Contains(da.OrderByAttribute.Name))
                                {
                                    da = da.OrderByAttribute;
                                    PCAttributesToInclude.Add(da.Name);
                                }
                                else
                                    break;
                            }

                        }
                    }

                    if (i > 2)
                        strQry = strQry.Remove(strQry.Length - 3) + "\r\n, Level" + (i - 1).ToString() + "Subselect.*\r\nfrom " + id.TableName + " b,\r\n";
                    strSelEnd = ") Level" + i.ToString() + "Subselect\r\n" + strSelEnd;
                    strWhere += "Level" + (MinimumLevelCount + 1).ToString() + "Subselect.[" + LevelName + "_KeyColumn] = a.[" + LevelName + "_KeyColumn] and\r\n";
                    strLevelsEnumerations = strCTELevelEnumeration + strLevelsEnumerations;
                }

                CTESel = "CREATE VIEW " + txtNewView + " AS\r\n" +
                    "WITH PCStructure(Level, " + pid.ColumnName + ", [" + dim.KeyAttribute.Name + "_KeyColumn], [" + GetNaturalizedLevelName(2) + "_KeyColumn]" + ((CTESel.Length > 0) ? ", " + CTESel.Remove(CTESel.Length - 2) : "") + ")\r\n";
                CTEQry = "AS (SELECT 3 Level, " + pid.ColumnName + ", " + id.ColumnName + ",\r\n" + id.ColumnName + " as [" + GetNaturalizedLevelName(2) + "_KeyColumn]" + (CTEQry.Length > 0 ? ", \r\n" + CTEQry.Remove(CTEQry.Length - 3) : "") + "\r\n";
                strWhere += "Level" + (MinimumLevelCount + 1).ToString() + "Subselect.[" + dim.KeyAttribute.Name + "_KeyColumn] = a.[" + dim.KeyAttribute.Name + "_KeyColumn]";
                strLevelsEnumerations = "FROM " + id.TableName + " WHERE " + pid.ColumnName + " IS NULL OR " + pid.ColumnName + " = " + id.ColumnName + " " +
                    "UNION ALL SELECT Level + 1, e." + pid.ColumnName + ", e." + id.ColumnName + ",\r\n" + strLevelsEnumerations.Remove(strLevelsEnumerations.Length - 3) + " FROM " + id.TableName + " e " +
                    "INNER JOIN PCStructure d ON e." + pid.ColumnName + " = d.[" + dim.KeyAttribute.Name + "_KeyColumn] AND e." + pid.ColumnName + " != e." + id.ColumnName + ")\r\n";
                strQry = CTESel + CTEQry + strLevelsEnumerations + strSel.Remove(strSel.Length - 2) + "\r\n" + strQry +
                    GetNonPCUserHierarchyColumnNamesFromOriginalDimension() + strSelEnd + strWhere;
                cmd.CommandText = strQry;

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        private OleDbConnection GetNonLocalizedConnection(string ConnStr)
        {
            OleDbConnection connTmp = new OleDbConnection(ConnStr);
            if (!connTmp.Provider.ToUpper().Contains("SQLNCLI") && !connTmp.Provider.ToUpper().Contains("SQLOLEDB"))
                throw new Exception("The dimension source does not use SQLOLEDB or SQL Native Client for its OLE DB provider.  Naturalization is not supported curently for this case.");
            if (connTmp.DataSource.ToLower().Contains("localhost") || connTmp.DataSource.Contains("."))
            {
                string strSrvOnly = srv.Name;
                if (strSrvOnly.Contains("\\"))
                    strSrvOnly = strSrvOnly.Substring(0, strSrvOnly.IndexOf("\\"));
                connTmp.ConnectionString = connTmp.ConnectionString.ToLower().Replace("localhost", strSrvOnly).Replace("=.;", "=" + strSrvOnly + ";").Replace("=.\\", "=" + strSrvOnly + "\\");
            }
            return connTmp;
        }

        private void EnsureAllIncludedPCAttributesRelateToKey()
        {
            for (int i = 0; i < PCAttributesToInclude.Count; i++)
            {
                if (!dim.KeyAttribute.AttributeRelationships.ContainsName(PCAttributesToInclude[i]) && dim.KeyAttribute.Name != PCAttributesToInclude[i])
                {
                    List<DimensionAttribute> atList = ASPCDimNaturalizer.GetAttrRelOwnerChainToKey(dim.Attributes.GetByName(PCAttributesToInclude[i]));
                    foreach (DimensionAttribute attr in atList)
                        if (!PCAttributesToInclude.Contains(attr.Name))
                            PCAttributesToInclude.Add(attr.Name);
                    if (atList == null)
                        throw new Exception("Not all attributes specified for the dimension are related directly or indirectly to the dimension key.");
                }
            }
        }

        private bool IsOLEDBTypeNumeric(OleDbType typ)
        {
            if (typ == OleDbType.Integer ||
                typ == OleDbType.BigInt ||
                typ == OleDbType.Binary ||
                typ == OleDbType.Boolean ||
                typ == OleDbType.Currency ||
                typ == OleDbType.Decimal ||
                typ == OleDbType.Double ||
                typ == OleDbType.LongVarBinary ||
                typ == OleDbType.Numeric ||
                typ == OleDbType.Single ||
                typ == OleDbType.SmallInt ||
                typ == OleDbType.TinyInt ||
                typ == OleDbType.UnsignedBigInt ||
                typ == OleDbType.UnsignedInt ||
                typ == OleDbType.UnsignedSmallInt ||
                typ == OleDbType.UnsignedTinyInt ||
                typ == OleDbType.VarBinary ||
                typ == OleDbType.VarNumeric)
                return true;
            else
                return false;
        }

        public override void Naturalize(object MinLevels)
        {
            try
            {
                // Initialize
                UpdateStatus("Initializing connections and calculating level depth...");
                EnsureAllIncludedPCAttributesRelateToKey();
                Conn = (OleDbConnection)Program.ASFlattener.DataSourceConnection.ConnectionObject;
                MinimumLevelCount = GetLevelCountFromPCTable(id.TableName, id.ColumnName, pid.ColumnName, IsOLEDBTypeNumeric(PCParentAttribute().KeyColumns[0].DataType));
                int iMinLevels = Convert.ToInt32(MinLevels);
                if (MinimumLevelCount < iMinLevels) MinimumLevelCount = iMinLevels;

                // Build natural DSV view
                UpdateStatus("Creating naturalized view for dimension...");
                CreateNaturalizedView();

                if (ASNaturalizationActionLevel > 1)
                {
                    Dimension dimNew = null;

                    UpdateStatus("Adding naturalized view to DSV...");
                    AddNaturalizedViewToDSV();
                    // Build dimension and add to cube
                    if (ASNaturalizationActionLevel > 2)
                    {
                        UpdateStatus("Materializing dimension structure in database...");
                        dimNew = MaterializeNaturalizedDimension();
                        if (ASNaturalizationActionLevel > 3)
                        {
                            UpdateStatus("Adding dimension to cubes...");
                            AddDimToCubes(dimNew);
                        }
                    }
                    UpdateStatus("Saving modified objects to server...");

                    if (dim.ParentServer != null)
                    {
                        db.Update(UpdateOptions.ExpandFull | UpdateOptions.AlterDependents);
                        if (ASNaturalizationActionLevel > 4)
                        {
                            UpdateStatus("Processing dimension: " + dimNew.Name);
                            dimNew.Process(ProcessType.ProcessFull);
                            foreach (Cube cub in db.Cubes)
                            {
                                if (cub.Dimensions.Contains(dim.ID))
                                {
                                    UpdateStatus("Processing cube: " + cub.Name);
                                    cub.Process(ProcessType.ProcessFull);
                                }
                            }
                        }
                    }
                }
                UpdateStatus("Operation complete.");
                UpdateStatus(BIDSHelper.Resources.Common.ProgressComplete);
            }
            catch (Exception e)
            {
                if (SourceWindowHandle != null && SourceWindowHandle.ToInt32() != 0)
                {
                    UpdateStatus("Error during: [" + Program.Progress.txtStatus.Text + "]\r\n" + e.ToString());
                    UpdateStatus(BIDSHelper.Resources.Common.ProcessError);
                }
                else
                {
                    if (LogFile != null)
                        File.WriteAllText(Program.LogFile, e.ToString());
                    throw e;
                }
            }
        }

    }

    class EnhancedColumnBinding
    {
        private ColumnBinding col = null;

        EnhancedColumnBinding(ColumnBinding Column)
        {
            col = Column;
        }

        public static explicit operator EnhancedColumnBinding(ColumnBinding Column)
        {
            return new EnhancedColumnBinding(Column);
        }

        public string NamedCalculationExpression
        {
            get
            {
                try
                {
                    string strColExp = (string)DSV.Schema.Tables[col.TableID].Columns[col.ColumnID].ExtendedProperties["ComputedColumnExpression"];
                    if (strColExp == null)
                        return string.Empty;
                    else
                        return strColExp.ToString().Trim();
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        public ColumnBinding Column
        {
            get { return col; }
            set { col = Column; }
        }

        public DataSourceView DSV
        {
            get { return ((Dimension)((DimensionAttribute)((DataItem)col.Parent).Parent).Parent).DataSourceView; }
        }

        public DataTable Table
        {
            get { return DSV.Schema.Tables[col.TableID]; }
        }

        public string TableName
        {
            get
            {
                if (DSV.Schema.Tables[col.TableID].ExtendedProperties.Contains("DbSchemaName") && DSV.Schema.Tables[col.TableID].ExtendedProperties.Contains("DbTableName"))
                    return "[" + DSV.Schema.Tables[col.TableID].ExtendedProperties["DbSchemaName"].ToString().Trim() + "].[" + DSV.Schema.Tables[col.TableID].ExtendedProperties["DbTableName"].ToString().Trim() + "]";
                else
                    return DSV.Schema.Tables[col.TableID].TableName;
            }
        }

        public string ColumnName
        {
            get { return "[" + DSV.Schema.Tables[col.TableID].Columns[col.ColumnID].ColumnName.Trim() + "]"; } //.ExtendedProperties["DbColumnName"].ToString().Trim() + "]"; }
        }
    }
}