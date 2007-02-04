using System;
using System.Collections.Generic;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.Data;

namespace BIDSHelper
{
    public class DimensionHealthCheckPlugin : BIDSHelperPluginBase
    {

        public DimensionHealthCheckPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "DimensionHealthCheck"; }
        }

        public override int Bitmap
        {
            get { return 4380; }
        }

        public override string ButtonText
        {
            get { return "Dimension Health Check"; }
        }

        public override string ToolTip
        {
            get { return ""; /*doesn't show anywhere*/ }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
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
                if (hierItem.Name.ToLower().EndsWith(".dim"))
                    return true;
                else
                    return false;
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
                Dimension d = (Dimension)projItem.Object;

                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(true, "Checking Dimension Health...", 0, d.Attributes.Count * 2);

                string[] errors = Check(d);

                EnvDTE80.Windows2 toolWins;
                EnvDTE.Window toolWin;
                object objTemp = null;
                toolWins = (Windows2)ApplicationObject.Windows;
                toolWin = toolWins.CreateToolWindow2(AddInInstance, typeof(WebBrowser).Assembly.Location, typeof(WebBrowser).FullName, d.Name + ": Dimension Health Check", "{" + typeof(WebBrowser).GUID.ToString() + "}", ref objTemp);
                WebBrowser browser = (WebBrowser)objTemp;
                browser.Navigate("about:blank");
                browser.Document.Write("<font style='font-family:Arial;font-size:10pt'>");
                browser.Document.Write("<h3>" + d.Name + ": Dimension Health Check</h3>");
                browser.Document.Write("<i>Checks whether attribute relationships hold true according to the data.<br>Also checks definition of attribute keys to determine if they are unique.</i><br><br>");
                if (errors.Length > 0)
                {
                    browser.Document.Write("<b>Problems</b><br>");
                    foreach (string s in errors)
                    {
                        browser.Document.Write("<li>");
                        browser.Document.Write(s);
                    }
                }
                else
                {
                    browser.Document.Write("<b>No problems found</b>");
                }
                browser.Document.Write("</font>");
                browser.IsWebBrowserContextMenuEnabled = false;

                //setting IsFloating and Linkable to false makes this window tabbed
                toolWin.IsFloating = false;
                toolWin.Linkable = false;
                toolWin.Visible = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(false, "Checking Dimension Health...", 2, 2);
            }
        }
        
        
        
        private string[] Check(Dimension d)
        {
            if (d.MiningModelID != null) return new string[] { };
            List<string> problems = new List<string>();
            //TODO: need to add in code to allow you to cancel such that it will stop an executing query
            System.Data.Common.DbCommand cmd;
            System.Data.Common.DbConnection conn;
            System.Data.Common.DbDataReader dr = null;
            if (d.DataSource.ConnectionString.Contains("Provider=SQLNCLI.1;"))
            {
                cmd = new System.Data.SqlClient.SqlCommand();
                conn = new System.Data.SqlClient.SqlConnection(d.DataSource.ConnectionString.Replace("Provider=SQLNCLI.1;", ""));
            }
            else
            {
                cmd = new System.Data.OleDb.OleDbCommand();
                conn = new System.Data.OleDb.OleDbConnection(d.DataSource.ConnectionString);
            }
            //TODO: try other datasource types like ODBC???

            //don't know how to retrieve password for SQL security, so change this to integrated security
            conn.ConnectionString = "Data Source=\"" + conn.DataSource + "\";Initial Catalog=\"" + conn.Database + "\";Integrated Security=SSPI;";


            int iProgressCount = 0;
            conn.Open();
            cmd.Connection = conn;
            String sql = "";
            bool bGotSQL = false;
            foreach (DimensionAttribute da in d.Attributes)
            {
                try
                {
                    bGotSQL = false;
                    sql = GetQueryToValidateKeyUniqueness(da);
                    if (sql != null)
                    {
                        bGotSQL = true;
                        cmd.CommandText = sql;
                        dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            problems.Add("Attribute [" + da.Name + "] has key values with multiple names.");
                        }
                        dr.Close();
                    }
                    ApplicationObject.StatusBar.Progress(true, "Checking Attribute Key Uniqueness...", ++iProgressCount, d.Attributes.Count * 2);
                }
                catch (Exception ex)
                {
                    problems.Add("Attempt to validate key and name relationship for attribute [" + da.Name + "] failed:" + ex.Message + ex.StackTrace + (bGotSQL ? "\r\nSQL query was: " + sql : ""));
                }
                finally
                {
                    if (dr != null && !dr.IsClosed) dr.Close();
                }
            }
            foreach (DimensionAttribute da in d.Attributes)
            {
                foreach (AttributeRelationship r in da.AttributeRelationships)
                {
                    try
                    {
                        bGotSQL = false;
                        sql = GetQueryToValidateRelationship(r);
                        if (sql != null)
                        {
                            bGotSQL = true;
                            cmd.CommandText = sql;
                            dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                problems.Add("Attribute relationship [" + da.Name + "] -> [" + r.Attribute.Name + "] is not valid because it results in a many-to-many relationship.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        problems.Add("Attempt to validate attribute relationship [" + da.Name + "] -> [" + r.Attribute.Name + "] failed:" + ex.Message + ex.StackTrace + (bGotSQL ? "\r\nSQL query was: " + sql : ""));
                    }
                    finally
                    {
                        if (dr != null && !dr.IsClosed) dr.Close();
                    }
                }
                ApplicationObject.StatusBar.Progress(true, "Checking Attribute Relationships...", ++iProgressCount, d.Attributes.Count * 2);
            }
            conn.Close();
            return problems.ToArray();
        }

        private static bool CompareDataItems(DataItem a, DataItem b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            else if (a != null && b != null)
            {
                if (a.Source.GetType().FullName == "Microsoft.AnalysisServices.ColumnBinding" && b.Source.GetType().FullName == "Microsoft.AnalysisServices.ColumnBinding")
                {
                    ColumnBinding colA = (ColumnBinding)a.Source;
                    ColumnBinding colB = (ColumnBinding)b.Source;
                    if ("[" + colA.TableID + "].[" + colA.ColumnID + "]" == "[" + colB.TableID + "].[" + colB.ColumnID + "]")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static string GetQueryToValidateKeyUniqueness(DimensionAttribute da)
        {
            if (da.KeyColumns.Count == 0 && (da.NameColumn == null || da.ValueColumn == null)) return null; // no need
            if (da.NameColumn == null && da.ValueColumn == null) return null; // no need
            if (da.KeyColumns.Count == 1 && CompareDataItems(da.KeyColumns[0], da.NameColumn) && da.ValueColumn == null) return null; // no need
            if (da.KeyColumns.Count == 1 && CompareDataItems(da.KeyColumns[0], da.ValueColumn) && da.NameColumn == null) return null; // no need

            DataSourceView oDSV = da.Parent.DataSourceView;
            ColumnBinding[] keyCols;
            ColumnBinding[] nameAndValueCols;
            if (da.KeyColumns.Count == 0)
            {
                keyCols = new ColumnBinding[] { GetColumnBindingForDataItem(da.NameColumn) };
                nameAndValueCols = new ColumnBinding[] { GetColumnBindingForDataItem(da.ValueColumn) }; //value column will be there because of previous check
            }
            else if (da.NameColumn == null)
            {
                if (da.KeyColumns.Count > 1)
                {
                    throw new Exception("If no name column is defined, you may not have more than one key column.");
                }
                keyCols = new ColumnBinding[] { GetColumnBindingForDataItem(da.KeyColumns[0]) };

                //use value as "name" column when checking uniqueness
                nameAndValueCols = new ColumnBinding[] { GetColumnBindingForDataItem(da.ValueColumn) }; //value column will be there because of previous check
            }
            else
            {
                keyCols = new ColumnBinding[da.KeyColumns.Count];
                for (int i = 0; i < da.KeyColumns.Count; i++)
                {
                    keyCols[i] = GetColumnBindingForDataItem(da.KeyColumns[i]);
                }
                if (da.ValueColumn == null)
                {
                    nameAndValueCols = new ColumnBinding[] { GetColumnBindingForDataItem(da.NameColumn) };
                }
                else
                {
                    nameAndValueCols = new ColumnBinding[] { GetColumnBindingForDataItem(da.NameColumn), GetColumnBindingForDataItem(da.ValueColumn) };
                }
            }
            return GetQueryToValidateUniqueness(oDSV, keyCols, nameAndValueCols);
        }

        private static string GetQueryToValidateRelationship(AttributeRelationship r)
        {
            DataSourceView oDSV = r.ParentDimension.DataSourceView;
            ColumnBinding[] cols1 = new ColumnBinding[r.Parent.KeyColumns.Count];
            ColumnBinding[] cols2 = new ColumnBinding[r.Attribute.KeyColumns.Count];
            for (int i = 0; i < r.Parent.KeyColumns.Count; i++)
            {
                cols1[i] = GetColumnBindingForDataItem(r.Parent.KeyColumns[i]);
            }
            for (int i = 0; i < r.Attribute.KeyColumns.Count; i++)
            {
                cols2[i] = GetColumnBindingForDataItem(r.Attribute.KeyColumns[i]);
            }
            return GetQueryToValidateUniqueness(oDSV, cols1, cols2);
        }

        private static string GetFromClauseForTable(DataTable oTable)
        {
            if (!oTable.ExtendedProperties.ContainsKey("QueryDefinition") && oTable.ExtendedProperties.ContainsKey("DbTableName") && oTable.ExtendedProperties.ContainsKey("DbSchemaName"))
            {
                return "[" + oTable.ExtendedProperties["DbSchemaName"].ToString() + "].[" + oTable.ExtendedProperties["DbTableName"].ToString() + "] as [" + oTable.ExtendedProperties["FriendlyName"].ToString() + "]";
            }
            else if (oTable.ExtendedProperties.ContainsKey("QueryDefinition"))
            {
                return "(\r\n " + oTable.ExtendedProperties["QueryDefinition"].ToString() + "\r\n) as [" + oTable.ExtendedProperties["FriendlyName"].ToString() + "]\r\n";
            }
            else
            {
                throw new Exception("Unexpected query definition for table binding.");
            }
        }

        private static string GetQueryToValidateUniqueness(DataSourceView dsv, ColumnBinding[] child, ColumnBinding[] parent)
        {
            StringBuilder select = new StringBuilder();
            StringBuilder outerSelect = new StringBuilder();
            StringBuilder groupBy = new StringBuilder();
            Dictionary<DataTable, JoinedTable> tables = new Dictionary<DataTable, JoinedTable>();
            List<string> previousColumns = new List<string>();
            foreach (ColumnBinding col in child)
            {
                if (!previousColumns.Contains("[" + col.TableID + "].[" + col.ColumnID + "]"))
                {
                    string colAlias = System.Guid.NewGuid().ToString();
                    DataColumn dc = dsv.Schema.Tables[col.TableID].Columns[col.ColumnID];
                    groupBy.Append((select.Length == 0 ? "group by " : ","));
                    outerSelect.Append((select.Length == 0 ? "select " : ","));
                    select.Append((select.Length == 0 ? "select distinct " : ","));
                    groupBy.Append("[").Append(colAlias).AppendLine("]");
                    outerSelect.Append("[").Append(colAlias).AppendLine("]");
                    if (!dc.ExtendedProperties.ContainsKey("ComputedColumnExpression"))
                    {
                        select.Append("[").Append(colAlias).Append("] = [").Append(dsv.Schema.Tables[col.TableID].ExtendedProperties["FriendlyName"].ToString()).Append("].[").Append((dc.ExtendedProperties["DbColumnName"] ?? dc.ColumnName).ToString()).AppendLine("]");
                    }
                    else
                    {
                        select.Append("[").Append(colAlias).Append("]");
                        select.Append(" = ").AppendLine(dc.ExtendedProperties["ComputedColumnExpression"].ToString());
                    }

                    if (!tables.ContainsKey(dsv.Schema.Tables[col.TableID]))
                    {
                        tables.Add(dsv.Schema.Tables[col.TableID], new JoinedTable(dsv.Schema.Tables[col.TableID]));
                    }
                    previousColumns.Add("[" + col.TableID + "].[" + col.ColumnID + "]");
                }
            }

            foreach (ColumnBinding col in parent)
            {
                if (!previousColumns.Contains("[" + col.TableID + "].[" + col.ColumnID + "]"))
                {
                    string colAlias = System.Guid.NewGuid().ToString();
                    DataColumn dc = dsv.Schema.Tables[col.TableID].Columns[col.ColumnID];
                    select.Append(",");
                    //use the __PARENT__ prefix in case there's a column with the same name but different table
                    if (!dc.ExtendedProperties.ContainsKey("ComputedColumnExpression"))
                    {
                        select.Append("[").Append(colAlias).Append("] = [").Append(dsv.Schema.Tables[col.TableID].ExtendedProperties["FriendlyName"].ToString()).Append("].[").Append((dc.ExtendedProperties["DbColumnName"] ?? dc.ColumnName).ToString()).AppendLine("]");
                    }
                    else
                    {
                        select.Append("[").Append(colAlias).Append("]");
                        select.Append(" = ").AppendLine(dc.ExtendedProperties["ComputedColumnExpression"].ToString());
                    }

                    if (!tables.ContainsKey(dsv.Schema.Tables[col.TableID]))
                    {
                        tables.Add(dsv.Schema.Tables[col.TableID], new JoinedTable(dsv.Schema.Tables[col.TableID]));
                    }
                    previousColumns.Add("[" + col.TableID + "].[" + col.ColumnID + "]");
                }
            }
            foreach (JoinedTable t in tables.Values)
            {
                TraverseParentRelationshipsAndAddNewTables(tables, t.table);
            }

            //check that all but one table have a valid join path to them
            DataTable baseTable = null;
            foreach (JoinedTable t in tables.Values)
            {
                if (!t.Joined)
                {
                    if (baseTable == null)
                    {
                        baseTable = t.table;
                    }
                    else
                    {
                        throw new Exception("Cannot find join path for table " + t.table.TableName + " or " + baseTable.TableName + ". Only one table can be the starting table for the joins.");
                    }
                }
            }

            //by now, all tables needed for joins will be in the dictionary
            select.Append("\r\nfrom ").AppendLine(GetFromClauseForTable(baseTable));
            select.Append(TraverseParentRelationshipsAndGetFromClause(tables, baseTable));

            return outerSelect.AppendLine("\r\nfrom (").Append(select).AppendLine(") x").Append(groupBy).Append("\r\nhaving count(*)>1").ToString();
        }

        //traverse all the parent relationships looking for connections to other tables we need
        //if a connection is found, then be sure to unwind and add all the missing intermediate tables along the way
        private static bool TraverseParentRelationshipsAndAddNewTables(Dictionary<DataTable, JoinedTable> tables, DataTable t)
        {
            bool bReturn = false;
            foreach (DataRelation r in t.ParentRelations)
            {
                if (r.ParentTable != r.ChildTable)
                {
                    if (!tables.ContainsKey(r.ParentTable) && TraverseParentRelationshipsAndAddNewTables(tables, r.ParentTable))
                    {
                        tables[r.ParentTable].Joined = true;
                        tables.Add(r.ParentTable, new JoinedTable(r.ParentTable));
                        bReturn = true;
                    }
                    else if (tables.ContainsKey(r.ParentTable))
                    {
                        tables[r.ParentTable].Joined = true;
                        bReturn = true;
                    }
                }
            }
            return bReturn;
        }

        private static string TraverseParentRelationshipsAndGetFromClause(Dictionary<DataTable, JoinedTable> tables, DataTable t)
        {
            StringBuilder joins = new StringBuilder();
            foreach (DataRelation r in t.ParentRelations)
            {
                if (r.ParentTable != r.ChildTable && tables.ContainsKey(r.ParentTable) && !tables[r.ParentTable].AddedToQuery)
                {
                    joins.Append("join ").AppendLine(GetFromClauseForTable(r.ParentTable));
                    for (int i = 0; i < r.ParentColumns.Length; i++)
                    {
                        joins.Append((i == 0 ? " on " : " and "));
                        joins.Append("[").Append(r.ParentTable.ExtendedProperties["FriendlyName"].ToString()).Append("].[").Append(r.ParentColumns[i].ColumnName).Append("]");
                        joins.Append(" = [").Append(r.ChildTable.ExtendedProperties["FriendlyName"].ToString()).Append("].[").Append(r.ChildColumns[i].ColumnName).AppendLine("]");
                    }
                    joins.Append(TraverseParentRelationshipsAndGetFromClause(tables, r.ParentTable));
                }
            }
            tables[t].AddedToQuery = true;
            return joins.ToString();
        }


        private static ColumnBinding GetColumnBindingForDataItem(DataItem di)
        {
            if (di.Source is ColumnBinding)
            {
                return (ColumnBinding)di.Source;
            }
            else
            {
                throw new Exception("Binding for column was unexpected type.");
            }
        }


        class JoinedTable
        {
            public DataTable table;
            public bool Joined = false;
            public bool AddedToQuery = false;
            public JoinedTable(DataTable t)
            {
                table = t;
            }
        }
    }
}