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
                return (((ProjectItem)hierItem.Object).Object is Dimension);
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

                DimensionError[] errors = Check(d);

                int iErrorCnt = 0;
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
                    foreach (DimensionError e in errors)
                    {
                        iErrorCnt++;
                        browser.Document.Write("<li>");
                        browser.Document.Write(e.ErrorDescription);
                        if (e.ErrorTable != null)
                        {
                            browser.Document.Write(" <a href=\"javascript:void(null)\" id=expander" + iErrorCnt + " ErrorCnt=" + iErrorCnt + " style='color:blue'>Show/hide problem rows</a><br>\r\n");
                            browser.Document.Write("<table id=error" + iErrorCnt + " cellspacing=0 style='display:none;font-family:Arial;font-size:10pt'>");
                            browser.Document.Write("<tr><td></td>");
                            for (int i = 0; i < e.ErrorTable.Columns.Count; i++)
                            {
                                browser.Document.Write("<td nowrap><b>");
                                browser.Document.Write(System.Web.HttpUtility.HtmlEncode(e.ErrorTable.Columns[i].ColumnName));
                                browser.Document.Write("</b></td><td>&nbsp;&nbsp;</td>");
                            }
                            browser.Document.Write("</tr>\r\n");
                            foreach (DataRow dr in e.ErrorTable.Rows)
                            {
                                browser.Document.Write("<tr><td>&nbsp;&nbsp;&nbsp;&nbsp;</td>");
                                for (int i = 0; i < e.ErrorTable.Columns.Count; i++)
                                {
                                    browser.Document.Write("<td nowrap>");
                                    if (!Convert.IsDBNull(dr[i]))
                                        browser.Document.Write(System.Web.HttpUtility.HtmlEncode(dr[i].ToString()));
                                    else
                                        browser.Document.Write("<font color=lightgrey>(null)</font>");
                                    browser.Document.Write("</td><td>&nbsp;&nbsp;</td>");
                                }
                                browser.Document.Write("</tr>");
                            }
                            browser.Document.Write("</table>");
                        }
                    }
                }
                else
                {
                    browser.Document.Write("<b>No problems found</b>");
                }
                browser.Document.Write("</font>");
                browser.IsWebBrowserContextMenuEnabled = false;
                browser.AllowWebBrowserDrop = false;

                Application.DoEvents();

                for (int i = 1; i <= iErrorCnt; i++)
                {
                    //in some of the newer versions of Internet Explorer, javascript is not enabled
                    //so do the dynamic stuff with C# events and code
                    try
                    {
                        browser.Document.GetElementById("expander" + i).Click += new HtmlElementEventHandler(Expander_Click);
                    }
                    catch { }
                }

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

        void Expander_Click(object sender, HtmlElementEventArgs e)
        {
            try
            {
                HtmlElement el = (HtmlElement)sender;
                HtmlElement error = el.Document.GetElementById("error" + el.GetAttribute("ErrorCnt"));
                if (error != null)
                {
                    if (error.Style.ToLower().StartsWith("display"))
                        error.Style = "font-family:Arial;font-size:10pt";
                    else
                        error.Style = "display:none;font-family:Arial;font-size:10pt";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }



        private DimensionError[] Check(Dimension d)
        {
            if (d.MiningModelID != null) return new DimensionError[] { };
            List<DimensionError> problems = new List<DimensionError>();
            //TODO: need to add in code to allow you to cancel such that it will stop an executing query

            Microsoft.DataWarehouse.Design.DataSourceConnection openedDataSourceConnection = Microsoft.AnalysisServices.Design.DSVUtilities.GetOpenedDataSourceConnection(d.DataSource);

            int iProgressCount = 0;
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
                        DataSet ds = new DataSet();
                        openedDataSourceConnection.Fill(ds, sql);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string problem = "Attribute [" + da.Name + "] has key values with multiple names.";
                            DimensionError err = new DimensionError();
                            err.ErrorDescription = problem;
                            err.ErrorTable = ds.Tables[0];
                            problems.Add(err);
                        }
                    }
                    ApplicationObject.StatusBar.Progress(true, "Checking Attribute Key Uniqueness...", ++iProgressCount, d.Attributes.Count * 2);
                }
                catch (Exception ex)
                {
                    string problem = "Attempt to validate key and name relationship for attribute [" + da.Name + "] failed:" + ex.Message + ex.StackTrace + (bGotSQL ? "\r\nSQL query was: " + sql : "");
                    DimensionError err = new DimensionError();
                    err.ErrorDescription = problem;
                    problems.Add(err);
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
                            DataSet ds = new DataSet();
                            openedDataSourceConnection.Fill(ds, sql);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                string problem = "Attribute relationship [" + da.Name + "] -> [" + r.Attribute.Name + "] is not valid because it results in a many-to-many relationship.";
                                DimensionError err = new DimensionError();
                                err.ErrorDescription = problem;
                                err.ErrorTable = ds.Tables[0];
                                problems.Add(err);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string problem = "Attempt to validate attribute relationship [" + da.Name + "] -> [" + r.Attribute.Name + "] failed:" + ex.Message + ex.StackTrace + (bGotSQL ? "\r\nSQL query was: " + sql : "");
                        DimensionError err = new DimensionError();
                        err.ErrorDescription = problem;
                        problems.Add(err);
                    }
                }
                ApplicationObject.StatusBar.Progress(true, "Checking Attribute Relationships...", ++iProgressCount, d.Attributes.Count * 2);
            }
            openedDataSourceConnection.Close();
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
            DataItem[] keyCols;
            DataItem[] nameAndValueCols;
            if (da.KeyColumns.Count == 0)
            {
                keyCols = new DataItem[] { da.NameColumn };
                nameAndValueCols = new DataItem[] { da.ValueColumn }; //value column will be there because of previous check
            }
            else if (da.NameColumn == null)
            {
                if (da.KeyColumns.Count > 1)
                {
                    throw new Exception("If no name column is defined, you may not have more than one key column.");
                }
                keyCols = new DataItem[] { da.KeyColumns[0] };

                //use value as "name" column when checking uniqueness
                nameAndValueCols = new DataItem[] { da.ValueColumn }; //value column will be there because of previous check
            }
            else
            {
                keyCols = new DataItem[da.KeyColumns.Count];
                for (int i = 0; i < da.KeyColumns.Count; i++)
                {
                    keyCols[i] = da.KeyColumns[i];
                }
                if (da.ValueColumn == null)
                {
                    nameAndValueCols = new DataItem[] { da.NameColumn };
                }
                else
                {
                    nameAndValueCols = new DataItem[] { da.NameColumn, da.ValueColumn };
                }
            }
            return GetQueryToValidateUniqueness(oDSV, keyCols, nameAndValueCols);
        }

        private static string GetQueryToValidateRelationship(AttributeRelationship r)
        {
            DataSourceView oDSV = r.ParentDimension.DataSourceView;
            DataItem[] cols1 = new DataItem[r.Parent.KeyColumns.Count];
            DataItem[] cols2 = new DataItem[r.Attribute.KeyColumns.Count];
            for (int i = 0; i < r.Parent.KeyColumns.Count; i++)
            {
                cols1[i] = r.Parent.KeyColumns[i];
            }
            for (int i = 0; i < r.Attribute.KeyColumns.Count; i++)
            {
                cols2[i] = r.Attribute.KeyColumns[i];
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

        private static string GetQueryToValidateUniqueness(DataSourceView dsv, DataItem[] child, DataItem[] parent)
        {
            //need to do GetColumnBindingForDataItem
            StringBuilder select = new StringBuilder();
            StringBuilder outerSelect = new StringBuilder();
            StringBuilder groupBy = new StringBuilder();
            StringBuilder join = new StringBuilder();
            StringBuilder topLevelColumns = new StringBuilder();
            Dictionary<DataTable, JoinedTable> tables = new Dictionary<DataTable, JoinedTable>();
            List<string> previousColumns = new List<string>();
            foreach (DataItem di in child)
            {
                ColumnBinding col = GetColumnBindingForDataItem(di);
                if (!previousColumns.Contains("[" + col.TableID + "].[" + col.ColumnID + "]"))
                {
                    string colAlias = System.Guid.NewGuid().ToString();
                    DataColumn dc = dsv.Schema.Tables[col.TableID].Columns[col.ColumnID];
                    groupBy.Append((select.Length == 0 ? "group by " : ","));
                    outerSelect.Append((select.Length == 0 ? "select " : ","));
                    select.Append((select.Length == 0 ? "select distinct " : ","));
                    string sIsNull = "";
                    //select.Append(" /*" + dc.DataType.FullName + "*/ "); //for troubleshooting data types
                    if (dc.DataType == typeof(string))
                    {
                        if (di.NullProcessing == NullProcessing.Preserve)
                            sIsNull = "'__BIDS_HELPER_DIMENSION_HEALTH_CHECK_UNIQUE_STRING__'"; //a unique value that shouldn't ever occur in the real data
                        else
                            sIsNull = "''";
                    }
                    else //numeric
                    {
                        if (di.NullProcessing == NullProcessing.Preserve)
                            sIsNull = "-987654321.123456789"; //a unique value that shouldn't ever occur in the real data
                        else
                            sIsNull = "0";
                    }
                    join.Append((join.Length == 0 ? "on " : "and ")).Append("isnull(y.[").Append(colAlias).Append("],").Append(sIsNull).Append(") = isnull(z.[").Append(colAlias).Append("],").Append(sIsNull).AppendLine(")");
                    groupBy.Append("[").Append(colAlias).AppendLine("]");
                    outerSelect.Append("[").Append(colAlias).AppendLine("]");
                    if (topLevelColumns.Length > 0) topLevelColumns.Append(",");
                    topLevelColumns.Append("y.[").Append(colAlias).Append("] as [").Append(dc.ColumnName).AppendLine("]");
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

            foreach (DataItem di in parent)
            {
                ColumnBinding col = GetColumnBindingForDataItem(di);
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
                    if (topLevelColumns.Length > 0) topLevelColumns.Append(",");
                    topLevelColumns.Append("y.[").Append(colAlias).Append("] as [").Append(dc.ColumnName).AppendLine("]");

                    if (!tables.ContainsKey(dsv.Schema.Tables[col.TableID]))
                    {
                        tables.Add(dsv.Schema.Tables[col.TableID], new JoinedTable(dsv.Schema.Tables[col.TableID]));
                    }
                    previousColumns.Add("[" + col.TableID + "].[" + col.ColumnID + "]");
                }
            }

            int iLastTableCount = 0;
            while (iLastTableCount != tables.Values.Count)
            {
                iLastTableCount = tables.Values.Count;
                JoinedTable[] arrJt = new JoinedTable[iLastTableCount];
                tables.Values.CopyTo(arrJt, 0); //because you can't iterate the dictionary keys while they are changing
                foreach (JoinedTable jt in arrJt)
                {
                    TraverseParentRelationshipsAndAddNewTables(tables, jt.table);
                }
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

            outerSelect.AppendLine("\r\nfrom (").Append(select).AppendLine(") x").Append(groupBy).AppendLine("\r\nhaving count(*)>1");

            string invalidValuesInner = outerSelect.ToString();
            outerSelect = new StringBuilder();
            outerSelect.Append("select ").AppendLine(topLevelColumns.ToString()).AppendLine(" from (").Append(select).AppendLine(") as y");
            outerSelect.AppendLine("join (").AppendLine(invalidValuesInner).AppendLine(") z").AppendLine(join.ToString());
            return outerSelect.ToString();
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
                        tables.Add(r.ParentTable, new JoinedTable(r.ParentTable));
                        tables[r.ParentTable].Joined = true;
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

        class DimensionError
        {
            public string ErrorDescription;
            public DataTable ErrorTable;
        }
    }
}