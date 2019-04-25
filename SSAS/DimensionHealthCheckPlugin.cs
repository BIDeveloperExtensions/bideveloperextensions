using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.Data;
using System.ComponentModel.Design;
using BIDSHelper.Core;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class DimensionHealthCheckPlugin : BIDSHelperPluginBase
    {
        private Dimension oLastDimension;
        private IComponentChangeService changesvc;

#if YUKON || KATMAI
        private static EnvDTE.Window toolWin;
#endif

        public DimensionHealthCheckPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.DimensionHealthCheckId, typeof(Dimension));
        }

        public override string ShortName
        {
            get { return "DimensionHealthCheck"; }
        }

        //public override int Bitmap
        //{
        //    get { return 4380; }
        //}

        public override string FeatureName
        {
            get { return "Dimension Health Check"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; /*doesn't show anywhere*/ }
        }

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
        //}

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASMulti; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Allows you check various indications of dimension health, analyzing data and attributes to ensure they are valid."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        //public override bool DisplayCommand(UIHierarchyItem item)
        //{
        //    try
        //    {
        //        UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
        //        if (((System.Array)solExplorer.SelectedItems).Length != 1)
        //            return false;

        //        UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
        //        return (((ProjectItem)hierItem.Object).Object is Dimension);
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                Dimension d = (Dimension)projItem.Object;

                if (d.DataSource == null)
                {
                    if (d.Source is TimeBinding)
                    {
                        MessageBox.Show("Dimension Health Check is not supported on a Server Time dimension.");
                    }
                    else
                    {
                        MessageBox.Show("The data source for this dimension is not set. Dimension Health Check cannot be run.");
                    }
                    return;
                }
                else if (d.Source is DimensionBinding)
                {
                    MessageBox.Show("Dimension Health Check is not supported on a linked dimension.");
                    return;
                }

                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(true, "Checking Dimension Health...", 0, d.Attributes.Count * 2);

                DimensionError[] errors = Check(d);
                if (errors == null) return;

                this.oLastDimension = d;
                this.changesvc = (IComponentChangeService)d.Site.GetService(typeof(IComponentChangeService));

                int iErrorCnt = 0;
                string sCaption = d.Name + ": Dimension Health Check";

#if YUKON || KATMAI
                EnvDTE80.Windows2 toolWins;
                object objTemp = null;
                toolWins = (Windows2)ApplicationObject.Windows;
                if (toolWin == null)
                {
                    toolWin = toolWins.CreateToolWindow2(AddInInstance, typeof(WebBrowser).Assembly.Location, typeof(WebBrowser).FullName, sCaption, "{" + typeof(WebBrowser).GUID.ToString() + "}", ref objTemp);
                }
                else
                {
                    objTemp = toolWin.Object;
                    toolWin.Caption = sCaption;
                }

                WebBrowser browser = (WebBrowser)objTemp;
#else
                //appear to be having some problems with .NET controls inside tool windows, even though this issue says fixed: http://connect.microsoft.com/VisualStudio/feedback/details/512181/vsip-vs-2010-beta2-width-of-add-in-toolwindow-not-changed-with-activex-hosted-control#tabs
                //so just create this inside a regular WinForm
                WebBrowser browser = new WebBrowser();
#endif

                browser.AllowNavigation = true;
                if (browser.Document != null) //idea from http://geekswithblogs.net/paulwhitblog/archive/2005/12/12/62961.aspx
                    browser.Document.OpenNew(true);
                else
                    browser.Navigate("about:blank");
                Application.DoEvents();

                browser.Document.Write("<font style='font-family:Arial;font-size:10pt'>");
                browser.Document.Write("<h3>" + d.Name + ": Dimension Health Check</h3>");
                browser.Document.Write("<i>Checks whether attribute relationships hold true according to the data.<br>Also checks definition of attribute keys to determine if they are unique.<br>Also checks whether any obvious attribute relationships are missing.</i><br><br>");
                if (errors.Length > 0)
                {
                    browser.Document.Write("<b>Problems</b><br>");
                    foreach (DimensionError e in errors)
                    {
                        iErrorCnt++;
                        browser.Document.Write("<li>");
                        browser.Document.Write(e.ErrorDescription);
                        DimensionDataError de = e as DimensionDataError;
                        DimensionRelationshipWarning rw = e as DimensionRelationshipWarning;
                        if (de != null && de.ErrorTable != null)
                        {
                            browser.Document.Write(" <a href=\"javascript:void(null)\" id=expander" + iErrorCnt + " ErrorCnt=" + iErrorCnt + " style='color:blue'>Show/hide problem rows</a><br>\r\n");
                            browser.Document.Write("<table id=error" + iErrorCnt + " cellspacing=0 style='display:none;font-family:Arial;font-size:10pt'>");
                            browser.Document.Write("<tr><td></td>");
                            for (int i = 0; i < de.ErrorTable.Columns.Count; i++)
                            {
                                browser.Document.Write("<td nowrap><b>");
                                browser.Document.Write(System.Web.HttpUtility.HtmlEncode(de.ErrorTable.Columns[i].ColumnName));
                                browser.Document.Write("</b></td><td>&nbsp;&nbsp;</td>");
                            }
                            browser.Document.Write("</tr>\r\n");
                            foreach (DataRow dr in de.ErrorTable.Rows)
                            {
                                browser.Document.Write("<tr><td>&nbsp;&nbsp;&nbsp;&nbsp;</td>");
                                for (int i = 0; i < de.ErrorTable.Columns.Count; i++)
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
                        else if (rw != null)
                        {
                            browser.Document.Write(" <a href=\"javascript:void(null)\" id=expander" + iErrorCnt + " Attribute=\"" + rw.Attribute.ID + "\" RelatedAttribute=\"" + rw.RelatedAttribute.ID + "\" style='color:blue'>Change attribute relationship</a>\r\n");
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

#if YUKON || KATMAI
                //setting IsFloating and Linkable to false makes this window tabbed
                toolWin.IsFloating = false;
                toolWin.Linkable = false;
                toolWin.Visible = true;
#else
                Form descForm = new Form();
                descForm.Icon = BIDSHelper.Resources.Common.BIDSHelper;
                descForm.Text = "BIDS Helper - " + sCaption;
                descForm.MaximizeBox = true;
                descForm.MinimizeBox = false;
                descForm.Width = 600;
                descForm.Height = 500;
                descForm.SizeGripStyle = SizeGripStyle.Show;
                descForm.MinimumSize = new System.Drawing.Size(descForm.Width/2, descForm.Height/2);

                browser.Top = 10;
                browser.Left = 10;
                browser.Width = descForm.Width - 30;
                browser.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                browser.Dock = DockStyle.Fill;
                browser.Height = descForm.Height - 60;
                descForm.Controls.Add(browser);
                descForm.Show();
#endif
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
                if (!string.IsNullOrEmpty(el.GetAttribute("ErrorCnt")))
                {
                    HtmlElement error = el.Document.GetElementById("error" + el.GetAttribute("ErrorCnt"));
                    if (error != null)
                    {
                        if (error.Style.ToLower().StartsWith("display") 
                            || error.Style.ToLower().Contains("display: none") 
                            || error.Style.ToLower().Contains("display:none"))
                            error.Style = "font-family:Arial;font-size:10pt";
                        else
                            error.Style = "display:none;font-family:Arial;font-size:10pt";
                    }
                }
                else
                {
                    string sAttributeID = el.GetAttribute("Attribute");
                    string sRelatedAttributeID = el.GetAttribute("RelatedAttribute");
                    DimensionAttribute parent = this.oLastDimension.Attributes[sAttributeID];
                    DimensionAttribute child = this.oLastDimension.Attributes[sRelatedAttributeID];

                    if (MessageBox.Show("Are you sure you want to make [" + child.Name + "] related to [" + parent.Name + "]?", "BIDS Helper - Fix Obvious Attribute Relationship Oversight?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;
                    }

                    foreach (DimensionAttribute da in this.oLastDimension.Attributes)
                    {
                        if (da.AttributeRelationships.Contains(child.ID))
                        {
                            da.AttributeRelationships.Remove(child.ID);
                        }
                    }
                    parent.AttributeRelationships.Add(child.ID);

                    //mark dimension as dirty
                    this.changesvc.OnComponentChanging(this.oLastDimension, null);
                    this.changesvc.OnComponentChanged(this.oLastDimension, null, null, null);

                    MessageBox.Show("Finished making [" + child.Name + "] related to [" + parent.Name + "].\r\n\r\nPlease rerun Dimension Health Check to see if any other warnings were resolved.", "BIDS Helper - Fix Obvious Attribute Relationship Oversight");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        //making these static allows me not to have to change all the function signatures below... and it is fine as static because multiple dimension health checks can't be run in parallel
        private static string sq = "[";
        private static string fq = "]";
        private static Microsoft.DataWarehouse.Design.RDMSCartridge cartridge = null;
        private static string DBServerName = ""; //will say Oracle or Microsoft SQL Server

        private DimensionError[] Check(Dimension d)
        {
            if (d.MiningModelID != null) return new DimensionError[] { };
            List<DimensionError> problems = new List<DimensionError>();
            //TODO: need to add in code to allow you to cancel such that it will stop an executing query

            DataSource dataSource = d.DataSource;

            try
            {
                //if the key attribute points to a table with a different data source than the default data source for the DSV, use it
                ColumnBinding col = GetColumnBindingForDataItem(d.KeyAttribute.KeyColumns[0]);
                DataTable table = d.DataSourceView.Schema.Tables[col.TableID];
                if (table.ExtendedProperties.ContainsKey("DataSourceID"))
                {
                    dataSource = d.Parent.DataSources[table.ExtendedProperties["DataSourceID"].ToString()];
                }
            }
            catch { }

            Microsoft.DataWarehouse.Design.DataSourceConnection openedDataSourceConnection = Microsoft.DataWarehouse.DataWarehouseUtilities.GetOpenedDataSourceConnection((object)null, dataSource.ID, dataSource.Name, dataSource.ManagedProvider, dataSource.ConnectionString, dataSource.Site, false);
            try
            {
                if (openedDataSourceConnection != null)
                {
                    openedDataSourceConnection.QueryTimeOut = (int)dataSource.Timeout.TotalSeconds;
                }
            }
            catch { }

            if (openedDataSourceConnection == null)
            {
                DimensionError err = new DimensionError();
                err.ErrorDescription = "Unable to connect to data source [" + dataSource.Name + "] to test attribute relationships and key uniqueness.";
                problems.Add(err);
            }
            else
            {
                sq = openedDataSourceConnection.Cartridge.IdentStartQuote;
                fq = openedDataSourceConnection.Cartridge.IdentEndQuote;
                DBServerName = openedDataSourceConnection.DBServerName;
                cartridge = openedDataSourceConnection.Cartridge;

                int iProgressCount = 0;
                String sql = "";
                bool bGotSQL = false;
                foreach (DimensionAttribute da in d.Attributes)
                {
                    try
                    {
                        bGotSQL = false;
                        if (da.Usage != AttributeUsage.Parent)
                            sql = GetQueryToValidateKeyUniqueness(da);
                        else
                            sql = null;
                        if (sql != null)
                        {
                            bGotSQL = true;
                            DataSet ds = new DataSet();
                            openedDataSourceConnection.Fill(ds, sql);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                string problem = "Attribute [" + da.Name + "] has key values with multiple names.";
                                DimensionDataError err = new DimensionDataError();
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
                            if (da.Usage != AttributeUsage.Parent)
                                sql = GetQueryToValidateRelationship(r);
                            else
                                sql = null;
                            if (sql != null)
                            {
                                bGotSQL = true;
                                DataSet ds = new DataSet();
                                openedDataSourceConnection.Fill(ds, sql);
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    string problem = "Attribute relationship [" + da.Name + "] -> [" + r.Attribute.Name + "] is not valid because it results in a many-to-many relationship.";
                                    DimensionDataError err = new DimensionDataError();
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
                cartridge = null;
                openedDataSourceConnection.Close();
            }

            //check obvious attribute relationship mistakes
            foreach (DimensionAttribute da in d.Attributes)
            {
                foreach (DimensionAttribute child in d.Attributes)
                {
                    try
                    {
                        if (child.ID != da.ID && da.AttributeHierarchyEnabled && ContainsSubsetOfKeys(da, child) && !IsParentOf(child, da))
                        {
                            if (ContainsSubsetOfKeys(child, da) && (IsParentOf(da, child) || (child.Name.CompareTo(da.Name) < 0 && child.AttributeHierarchyEnabled)))
                            {
                                //if the keys for both are the same, then skip this one if the opposite attribute relationship is defined... otherwise, only return one direction based on alphabetic order
                                continue;
                            }

                            DimensionRelationshipWarning warn = new DimensionRelationshipWarning();
                            if (d.KeyAttribute.AttributeRelationships.Contains(child.ID))
                                warn.ErrorDescription = "Attribute [" + child.Name + "] has a subset of the keys of attribute [" + da.Name + "]. Therefore, those attributes can be related which is preferable to leaving [" + child.Name + "] related directly to the key.";
                            else
                                warn.ErrorDescription = "Attribute [" + child.Name + "] has a subset of the keys of attribute [" + da.Name + "]. Therefore, those attributes can be related. However, this may not be necessary since [" + child.Name + "] is already part of a set of attribute relationships.";

                            warn.Attribute = da;
                            warn.RelatedAttribute = child;
                            problems.Add(warn);
                        }
                    }
                    catch (Exception ex)
                    {
                        string problem = "Attempt to check for obvious attribute relationship oversights on [" + da.Name + "] and [" + child.Name + "] failed:" + ex.Message + ex.StackTrace;
                        DimensionError err = new DimensionError();
                        err.ErrorDescription = problem;
                        problems.Add(err);
                    }
                }
            }

            return problems.ToArray();
        }

        private static bool ContainsSubsetOfKeys(DimensionAttribute a1, DimensionAttribute a2)
        {
            //check that every a2.KeyColumns can be found in a1.KeyColumns
            if (a2.KeyColumns.Count == 0) return false;
            foreach (DataItem di2 in a2.KeyColumns)
            {
                bool bFoundKey = false;
                foreach (DataItem di1 in a1.KeyColumns)
                {
                    if (CompareDataItems(di1, di2))
                    {
                        bFoundKey = true;
                        break;
                    }
                }
                if (!bFoundKey) return false;
            }
            return true;
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
                return sq + oTable.ExtendedProperties["DbSchemaName"].ToString() + fq + "." + sq + oTable.ExtendedProperties["DbTableName"].ToString() + fq + " " + sq + oTable.ExtendedProperties["FriendlyName"].ToString() + fq;
            }
            else if (oTable.ExtendedProperties.ContainsKey("QueryDefinition"))
            {
                return "(\r\n " + oTable.ExtendedProperties["QueryDefinition"].ToString() + "\r\n) " + sq + oTable.ExtendedProperties["FriendlyName"].ToString() + fq + "\r\n";
            }
            else
            {
                throw new Exception("Unexpected query definition for table binding.");
            }
        }

        private static string GetNextUniqueColumnIdentifier()
        {
            string colAlias = System.Guid.NewGuid().ToString("N"); //no dashes

            //oracle identifiers can't be more than 30 chars... so the first 30 of 32 chars in the GUID will probably be unique
            long maxLen = 32;
            if (cartridge != null)
                maxLen = cartridge.GetLimit(Microsoft.DataWarehouse.Design.RDMSCartridge.LimitEnum.ColumnIdentifierLength);
            colAlias = colAlias.Substring(0, Math.Min(colAlias.Length,(int)maxLen));
            
            return colAlias;
        }

        private static string GetQueryToValidateUniqueness(DataSourceView dsv, DataItem[] child, DataItem[] parent)
        {
            //need to do GetColumnBindingForDataItem
            StringBuilder select = new StringBuilder();
            StringBuilder outerSelect = new StringBuilder();
            StringBuilder groupBy = new StringBuilder();
            StringBuilder orderBy = new StringBuilder();
            StringBuilder join = new StringBuilder();
            StringBuilder topLevelColumns = new StringBuilder();
            Dictionary<DataTable, JoinedTable> tables = new Dictionary<DataTable, JoinedTable>();
            List<string> previousColumns = new List<string>();
            foreach (DataItem di in child)
            {
                ColumnBinding col = GetColumnBindingForDataItem(di);
                if (!previousColumns.Contains("[" + col.TableID + "].[" + col.ColumnID + "]"))
                {
                    string colAlias = GetNextUniqueColumnIdentifier();
                    DataColumn dc = dsv.Schema.Tables[col.TableID].Columns[col.ColumnID];
                    groupBy.Append((select.Length == 0 ? "group by " : ","));
                    outerSelect.Append((select.Length == 0 ? "select " : ","));
                    select.Append((select.Length == 0 ? "select distinct " : ","));
                    string sIsNull = "";
                    string sForceDataTypeOnJoin = "";
                    if (dc.DataType == typeof(string))
                    {
                        if (DBServerName == "Oracle")
                        {
                            if (di.NullProcessing == NullProcessing.Preserve)
                                sIsNull = "'__BIDS_HELPER_DIMENSION_HEALTH_CHECK_UNIQUE_STRING__'"; //a unique value that shouldn't ever occur in the real data
                            else
                                sIsNull = "' '"; //oracle treats empty string as null
                        }
                        else
                        {
                            if (di.NullProcessing == NullProcessing.Preserve)
                                sIsNull = "'__BIDS_HELPER_DIMENSION_HEALTH_CHECK_UNIQUE_STRING__'"; //a unique value that shouldn't ever occur in the real data
                            else
                                sIsNull = "''";
                        }
                    }
                    else if (dc.DataType == typeof(DateTime))
                    {
                        if (DBServerName == "Oracle")
                        {
                            if (di.NullProcessing == NullProcessing.Preserve)
                                sIsNull = "to_date('1/1/1899 01:02:03','MM/DD/YYYY HH:MI:SS')"; //a unique value that shouldn't ever occur in the real data
                            else
                                sIsNull = "to_date('12/30/1899','MM/DD/YYYY')"; //think this is what SSAS converts null dates to
                        }
                        else if (DBServerName == "Teradata")
                        {
                            sForceDataTypeOnJoin = "timestamp";
                            if (di.NullProcessing == NullProcessing.Preserve)
                                sIsNull = "cast('1899/12/30 01:02:03.456789' as timestamp)"; //a unique value that shouldn't ever occur in the real data
                            else
                                sIsNull = "cast('1899/12/30 00:00:00' as timestamp)"; //think this is what SSAS converts null dates to
                        }
                        else
                        {
                            if (di.NullProcessing == NullProcessing.Preserve)
                                sIsNull = "'1/1/1899 01:02:03 AM'"; //a unique value that shouldn't ever occur in the real data
                            else
                                sIsNull = "'12/30/1899'"; //think this is what SSAS converts null dates to
                        }
                    }
                    else if (dc.DataType == typeof(Guid)) // Guid
                    {
                        if (di.NullProcessing == NullProcessing.Preserve)
                            sIsNull = "'" + (new Guid()).ToString() + "'"; //a unique value that shouldn't ever occur in the real data
                        else
                            sIsNull = "'" + Guid.Empty.ToString() + "'";
                    }
                    else //numeric
                    {
                        if (di.NullProcessing == NullProcessing.Preserve)
                            sIsNull = "-987654321.123456789"; //a unique value that shouldn't ever occur in the real data
                        else
                            sIsNull = "0";
                    }

                    if (!string.IsNullOrEmpty(sForceDataTypeOnJoin))
                    {
                        join.Append((join.Length == 0 ? "on " : "and ")).Append("coalesce(cast(y.").Append(sq).Append(colAlias).Append(fq).Append(" as ").Append(sForceDataTypeOnJoin).Append("),").Append(sIsNull).Append(") = coalesce(cast(z.").Append(sq).Append(colAlias).Append(fq).Append(" as ").Append(sForceDataTypeOnJoin).Append("),").Append(sIsNull).AppendLine(")");
                    }
                    else
                    {
                        join.Append((join.Length == 0 ? "on " : "and ")).Append("coalesce(y.").Append(sq).Append(colAlias).Append(fq).Append(",").Append(sIsNull).Append(") = coalesce(z.").Append(sq).Append(colAlias).Append(fq).Append(",").Append(sIsNull).AppendLine(")");
                    }

                    groupBy.Append(sq).Append(colAlias).AppendLine(fq);
                    outerSelect.Append(sq).Append(colAlias).AppendLine(fq);
                    if (topLevelColumns.Length > 0) topLevelColumns.Append(",");
                    topLevelColumns.Append("y.").Append(sq).Append(colAlias).Append(fq).Append(" as ").Append(sq).Append(dc.ColumnName).AppendLine(fq);
                    orderBy.Append((orderBy.Length == 0 ? "order by " : ","));
                    orderBy.Append("y.").Append(sq).Append(colAlias).AppendLine(fq);
                    if (!dc.ExtendedProperties.ContainsKey("ComputedColumnExpression"))
                    {
                        select.Append(sq).Append(dsv.Schema.Tables[col.TableID].ExtendedProperties["FriendlyName"].ToString()).Append(fq).Append(".").Append(sq).Append((dc.ExtendedProperties["DbColumnName"] ?? dc.ColumnName).ToString()).Append(fq).Append(" as ").Append(sq).Append(colAlias).AppendLine(fq);
                    }
                    else
                    {
                        select.AppendLine(dc.ExtendedProperties["ComputedColumnExpression"].ToString());
                        select.Append(" as ").Append(sq).Append(colAlias).AppendLine(fq);
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
                    string colAlias = GetNextUniqueColumnIdentifier();
                    DataColumn dc = dsv.Schema.Tables[col.TableID].Columns[col.ColumnID];
                    select.Append(",");
                    //use the __PARENT__ prefix in case there's a column with the same name but different table
                    if (!dc.ExtendedProperties.ContainsKey("ComputedColumnExpression"))
                    {
                        select.Append(sq).Append(dsv.Schema.Tables[col.TableID].ExtendedProperties["FriendlyName"].ToString()).Append(fq).Append(".").Append(sq).Append((dc.ExtendedProperties["DbColumnName"] ?? dc.ColumnName).ToString()).Append(fq).Append(" as ").Append(sq).Append(colAlias).AppendLine(fq);
                    }
                    else
                    {
                        select.AppendLine(dc.ExtendedProperties["ComputedColumnExpression"].ToString());
                        select.Append(" as ").Append(sq).Append(colAlias).AppendLine(fq);
                    }
                    if (topLevelColumns.Length > 0) topLevelColumns.Append(",");
                    topLevelColumns.Append("y.").Append(sq).Append(colAlias).Append(fq).Append(" as ").Append(sq).Append(dc.ColumnName).AppendLine(fq);
                    orderBy.Append((orderBy.Length == 0 ? "order by " : ","));
                    orderBy.Append("y.").Append(sq).Append(colAlias).AppendLine(fq);

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
            if (baseTable == null)
            {
                //there are joins to and from the base table, so guess at the base table based on the child parameter to this function
                ColumnBinding col = GetColumnBindingForDataItem(child[0]);
                baseTable = dsv.Schema.Tables[col.TableID];
            }

            //by now, all tables needed for joins will be in the dictionary
            select.Append("\r\nfrom ").AppendLine(GetFromClauseForTable(baseTable));
            select.Append(TraverseParentRelationshipsAndGetFromClause(tables, baseTable));

            outerSelect.AppendLine("\r\nfrom (").Append(select).AppendLine(") x").Append(groupBy).AppendLine("\r\nhaving count(*)>1");

            string invalidValuesInner = outerSelect.ToString();
            outerSelect = new StringBuilder();
            outerSelect.Append("select ").AppendLine(topLevelColumns.ToString()).AppendLine(" from (").Append(select).AppendLine(") y");
            outerSelect.AppendLine("join (").AppendLine(invalidValuesInner).AppendLine(") z").AppendLine(join.ToString()).AppendLine(orderBy.ToString());
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
            tables[t].AddedToQuery = true;
            StringBuilder joins = new StringBuilder();
            foreach (DataRelation r in t.ParentRelations)
            {
                if (r.ParentTable != r.ChildTable && tables.ContainsKey(r.ParentTable) && !tables[r.ParentTable].AddedToQuery)
                {
                    joins.Append("join ").AppendLine(GetFromClauseForTable(r.ParentTable));
                    for (int i = 0; i < r.ParentColumns.Length; i++)
                    {
                        joins.Append((i == 0 ? " on " : " and "));
                        joins.Append(sq).Append(r.ParentTable.ExtendedProperties["FriendlyName"].ToString()).Append(fq).Append(".").Append(sq).Append(r.ParentColumns[i].ColumnName).Append(fq);
                        joins.Append(" = ").Append(sq).Append(r.ChildTable.ExtendedProperties["FriendlyName"].ToString()).Append(fq).Append(".").Append(sq).Append(r.ChildColumns[i].ColumnName).AppendLine(fq);
                    }
                    joins.Append(TraverseParentRelationshipsAndGetFromClause(tables, r.ParentTable));
                }
            }
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
        }

        class DimensionDataError : DimensionError
        {
            public DataTable ErrorTable;
        }

        class DimensionRelationshipWarning : DimensionError
        {
            public DimensionAttribute Attribute;
            public DimensionAttribute RelatedAttribute;
        }
    }
}