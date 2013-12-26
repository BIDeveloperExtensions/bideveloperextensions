using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AnalysisServices;
using System.Data.SqlClient;
using System.ComponentModel.Design;


namespace BIDSHelper
{
    public partial class DeleteUnusedIndexesForm : Form
    {
        static Server liveServer;
        Database liveDB;
        Cube liveCube;

        Database cloneDB;
        Cube cloneCube;

        AdomdConnection adomdConnection;

        public List<Microsoft.AnalysisServices.Dimension> dirtiedDimensions = new List<Microsoft.AnalysisServices.Dimension>();

        string sCubePath;
        Trace trc = null;
        Dictionary<CubeAttribute, int> dictHitIndexes;
        string ConnectionString;
        string Table;

        private int iQueries = 0;
        private int iQuerySubcubeVerboseEvents = 0;
        private DateTime dtTraceStarted;
        private bool bStopped = false;
        private bool bExecuting = false;
        private int iWindowFullHeight;
        private int iWindowShortHeight;
        private int iWindowFullWidth;
        private int iWindowNarrowWidth;
        private static string sSessionIDColumnName;
        List<string> listDrillthroughQueries;
        private string sExport;
        private EnvDTE.ProjectItem projItem;

        public DeleteUnusedIndexesForm()
        {
            InitializeComponent();
        }

        public void Init(
           EnvDTE.ProjectItem projItem,
           Database cloneDB,
           Cube cloneCube)
        {
            this.projItem = projItem;
            Cube selectedCube = projItem.Object as Cube;
            if ((selectedCube != null) && (selectedCube.ParentServer != null))
            {
                // if we are in Online mode there will be a parent server
                liveServer = selectedCube.ParentServer;
                liveDB = selectedCube.Parent;
                liveCube = selectedCube;

                try
                {
                    adomdConnection = new AdomdConnection(liveServer.ConnectionString + ";Initial Catalog=" + liveDB.Name);
                    adomdConnection.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error connecting ADOMD.NET to server with connection string: " + liveServer.ConnectionString, ex);
                }
            }
            else
            {
                // if we are in Project mode we will use the server name from 
                // the deployment settings
                DeploymentSettings deploySet = new DeploymentSettings(projItem);
                liveServer = new Server();

                try
                {
                    liveServer.Connect(deploySet.TargetServer);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error connecting AMO to server from deployment properties: " + deploySet.TargetServer, ex);
                }
                
                liveDB = liveServer.Databases.GetByName(deploySet.TargetDatabase);
                liveCube = liveDB.Cubes.GetByName(selectedCube.Name);

                try
                {
                    adomdConnection = new AdomdConnection("Data Source=" + deploySet.TargetServer + ";Initial Catalog=" + deploySet.TargetDatabase);
                    adomdConnection.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error connecting ADOMD.NET to server from deployment properties: " + deploySet.TargetServer, ex);
                }
            }
            sCubePath = liveServer.ID + "." + liveDB.ID + "." + liveCube.ID + ".";

            this.cloneDB = cloneDB;
            this.cloneCube = cloneDB.Cubes[liveCube.ID];

            this.lblServer.Text = liveServer.Name;
            this.lblDatabaseName.Text = this.liveDB.Name;
            this.iWindowFullHeight = this.Height;
            this.iWindowShortHeight = this.btnExecute.Bottom + 40;
            this.Height = iWindowShortHeight;
            this.iWindowFullWidth = this.Width;
            this.iWindowNarrowWidth = this.grpProgress.Left - 30;
            this.Width = iWindowNarrowWidth;
            this.buttonCancel.Visible = false;
            this.buttonOK.Visible = false;
            this.treeViewAggregation.Visible = false;
            this.lblUnusedAggregationsToDelete.Visible = false;
        }


        private static System.Text.RegularExpressions.Regex _regexVerboseCubeDimension = new System.Text.RegularExpressions.Regex("Dimension \\d+ \\[(?<dimension>[^\\]]+?)\\] \\((?<attributes>[^\\)]+)\\)", System.Text.RegularExpressions.RegexOptions.Compiled);
        public static List<CubeAttribute> GetCubeAttributesWithSlice(string sQuerySubcubeVerbose, Microsoft.AnalysisServices.MeasureGroup mg)
        {
            List<CubeAttribute> list = new List<CubeAttribute>();
            int iDimensionIndex = 0;
            foreach (string sLine in sQuerySubcubeVerbose.Replace("\r\n", "\n").Replace("\r", "\n").Split(new char[] { '\n' }))
            {
                if (string.IsNullOrEmpty(sLine.Trim()))
                    break; //stop when we get to a blank line

                System.Text.RegularExpressions.Match match = _regexVerboseCubeDimension.Match(sLine);
                string sCubeDimensionName = match.Groups["dimension"].Value;
                CubeDimension cd = mg.Parent.Dimensions.FindByName(sCubeDimensionName);
                if (cd == null) continue;
                string sCubeDimensionID = cd.ID;

                string sAttributes = match.Groups["attributes"].Value;
                string[] arrAttributes = sAttributes.Split(new char[] { ' ' });
                for (int iAttribute = 0; iAttribute < arrAttributes.Length && iAttribute < cd.Attributes.Count; iAttribute++)
                {
                    string sAttributeSlice = arrAttributes[iAttribute];
                    if (sAttributeSlice == "0" || sAttributeSlice == "1" || sAttributeSlice == "*")
                    {
                        continue;
                    }
                    CubeAttribute ca = cd.Attributes[iAttribute];
                    list.Add(ca);
                }

                iDimensionIndex++;
            }
            return list;
        }

        private void radioTraceTypeSQL_CheckedChanged(object sender, EventArgs e)
        {
            this.btnSQLConnection.Visible = true;
        }

        private void radioTraceTypeLive_CheckedChanged(object sender, EventArgs e)
        {
            this.btnSQLConnection.Visible = false;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                //important to show the notification on the main thread of BIDS
                this.BeginInvoke(new MethodInvoker(delegate() { btnExecute_Click(sender, e); }));
            }
            else
            {
                if (bExecuting)
                {
                    bStopped = true;
                    timer1_Tick(null, null);
                }
                else
                {
                    if (radioTraceTypeSQL.Checked)
                    {
                        if (string.IsNullOrEmpty(ConnectionString) || string.IsNullOrEmpty(Table))
                            btnSQLConnection_Click(null, null);
                        if (string.IsNullOrEmpty(ConnectionString) || string.IsNullOrEmpty(Table))
                            return; //if they still haven't entered it, then just cancel out of the execute
                    }
                    bStopped = false;
                    Execute();
                }
            }
        }

        private void Execute()
        {
            dictHitIndexes = new Dictionary<CubeAttribute, int>();
            listDrillthroughQueries = new List<string>();
            bExecuting = true;
            this.radioTraceTypeLive.Enabled = false;
            this.radioTraceTypeSQL.Enabled = false;
            this.btnSQLConnection.Enabled = false;
            this.dtTraceStarted = DateTime.Now;
            this.Width = this.iWindowFullWidth;
            this.Height = this.iWindowShortHeight;
            this.buttonCancel.Visible = false;
            this.buttonOK.Visible = false;
            this.grpProgress.Visible = true;
            this.treeViewAggregation.Visible = false;
            this.lblUnusedAggregationsToDelete.Visible = false;
            this.iQueries = 0;
            this.dtTraceStarted = DateTime.Now;

            timer1_Tick(null, null);
            this.btnExecute.Enabled = false;
            Application.DoEvents();

            try
            {
                if (this.radioTraceTypeLive.Checked)
                {
                    timer1.Enabled = true;

                    string sTraceID = "BIDS Helper Delete Unused Indexes Trace " + System.Guid.NewGuid().ToString();
                    trc = liveServer.Traces.Add(sTraceID, sTraceID);
                    trc.OnEvent += new TraceEventHandler(trc_OnEvent);
                    trc.Stopped += new TraceStoppedEventHandler(trc_Stopped);
                    trc.AutoRestart = true;

                    TraceEvent te;
                    te = trc.Events.Add(TraceEventClass.QueryBegin);
                    te.Columns.Add(TraceColumn.DatabaseName);
                    te.Columns.Add(TraceColumn.TextData);
                    te.Columns.Add(TraceColumn.SessionID);

                    te = trc.Events.Add(TraceEventClass.QueryEnd);
                    te.Columns.Add(TraceColumn.DatabaseName);
                    te.Columns.Add(TraceColumn.SessionID);

                    te = trc.Events.Add(TraceEventClass.QuerySubcubeVerbose);
                    te.Columns.Add(TraceColumn.DatabaseName);
                    te.Columns.Add(TraceColumn.TextData);
                    te.Columns.Add(TraceColumn.ObjectPath);
                    te.Columns.Add(TraceColumn.SessionID);

                    trc.Update();
                    trc.Start();

                    this.btnExecute.Text = "Stop Trace";
                    this.btnExecute.Enabled = true;
                }
                else
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);
                    conn.Open();

                    bool bHasRowNumberColumn = false;
                    try
                    {
                        //test that this table has the RowNumber column
                        SqlCommand cmdCheckRowNumber = new SqlCommand();
                        cmdCheckRowNumber.Connection = conn;
                        cmdCheckRowNumber.CommandText = "select top 1 RowNumber from " + Table + " (nolock)";
                        cmdCheckRowNumber.CommandTimeout = 0;
                        cmdCheckRowNumber.ExecuteNonQuery();
                        bHasRowNumberColumn = true;
                    }
                    catch { }

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "select * from " + Table + " (nolock) order by CurrentTime" + (bHasRowNumberColumn ? ", RowNumber" : ""); //just select everything and filter in .NET (which allows us to throw better error messages if a column is missing... must be ordered so that we can recognize drillthrough queries and skip the query subcube verbose events until query end... ordering by CurrentTime then RowNumber allows this to work in ASTrace archive tables which have overlapping RowNumber ranges
                    cmd.CommandTimeout = 0;
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        if (!ReaderContainsColumn(reader, "EventClass"))
                            MessageBox.Show("Table " + Table + " must contain EventClass column");
                        else if (!ReaderContainsColumn(reader, "TextData"))
                            MessageBox.Show("Table " + Table + " must contain TextData column");
                        else if (!ReaderContainsColumn(reader, "ObjectPath"))
                            MessageBox.Show("Table " + Table + " must contain ObjectPath column");
                        else if (!ReaderContainsColumn(reader, "SessionID") && !ReaderContainsColumn(reader, "SPID"))
                            MessageBox.Show("Table " + Table + " must contain SessionID or SPID column");
                        else
                        {
                            if (ReaderContainsColumn(reader, "SessionID"))
                                sSessionIDColumnName = "SessionID";
                            else
                                sSessionIDColumnName = "SPID";


                            this.dtTraceStarted = DateTime.Now;
                            string sDateColumnName = "CurrentTime";
                            DateTime dtMin = DateTime.MaxValue;
                            DateTime dtMax = DateTime.MinValue;
                            Int64 iRowCount = 0;
                            while (reader.Read())
                            {
                                MyTraceEventArgs arg = new MyTraceEventArgs(reader);
                                HandleTraceEvent(arg);
                                if (!string.IsNullOrEmpty(sDateColumnName) && !Convert.IsDBNull(reader[sDateColumnName]))
                                {
                                    DateTime dt = Convert.ToDateTime(reader[sDateColumnName]);
                                    if (dtMin > dt) dtMin = dt;
                                    if (dtMax < dt) dtMax = dt;
                                    long iSecondsDiff = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, dtMin, dtMax, Microsoft.VisualBasic.FirstDayOfWeek.System, Microsoft.VisualBasic.FirstWeekOfYear.Jan1);
                                    this.dtTraceStarted = Microsoft.VisualBasic.DateAndTime.DateAdd(Microsoft.VisualBasic.DateInterval.Second, -iSecondsDiff, DateTime.Now);
                                }
                                if (iRowCount++ % 500 == 0)
                                {
                                    timer1_Tick(null, null);
                                    Application.DoEvents();
                                }
                            }
                            timer1_Tick(null, null);
                            Application.DoEvents();

                            FinishExecute(true);
                            return;
                        }
                        FinishExecute(false);
                    }
                    finally
                    {
                        try
                        {
                            reader.Close();
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                FinishExecute(false);
                MessageBox.Show(ex.Message);
            }
        }

        public static bool ReaderContainsColumn(SqlDataReader reader, string col)
        {
            DataTable schema = reader.GetSchemaTable();
            foreach (DataRow row in schema.Rows)
            {
                if (string.Compare(row["ColumnName"].ToString(), col, true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private class MyTraceEventArgs
        {
            public TraceEventClass EventClass;
            public string DatabaseName = null;
            public string ObjectPath = null;
            public string TextData = null;
            public string SessionIdOrSpid = null;

            public MyTraceEventArgs(TraceEventArgs args)
            {
                EventClass = args.EventClass;
                DatabaseName = args.DatabaseName;
                ObjectPath = args.ObjectPath;
                TextData = args.TextData;
                SessionIdOrSpid = args.SessionID;
            }

            public MyTraceEventArgs(SqlDataReader reader)
            {
                EventClass = (TraceEventClass)Enum.ToObject(typeof(TraceEventClass), Convert.ToInt64(reader["EventClass"]));

                if (!Convert.IsDBNull(reader["ObjectPath"]))
                {
                    string[] pathParts = reader["ObjectPath"].ToString().Split('.');
                    Database db = liveServer.Databases.Find(pathParts[1]);
                    if (db != null)
                    {
                        DatabaseName = db.Name;
                    }
                }
                else if (ReaderContainsColumn(reader, "DatabaseName") && !Convert.IsDBNull(reader["DatabaseName"]))
                {
                    DatabaseName = reader["DatabaseName"].ToString();
                }
                else
                {
                    DatabaseName = null;
                }
                if (!Convert.IsDBNull(reader["ObjectPath"]))
                    ObjectPath = reader["ObjectPath"].ToString();
                if (!Convert.IsDBNull(reader["TextData"]))
                    TextData = reader["TextData"].ToString();
                if (!Convert.IsDBNull(reader[sSessionIDColumnName]))
                    SessionIdOrSpid = reader[sSessionIDColumnName].ToString();
            }
        }

        private void FinishExecute(bool bShowResults)
        {
            if (this.InvokeRequired)
            {
                //important to show the notification on the main thread of BIDS
                this.BeginInvoke(new MethodInvoker(delegate() { FinishExecute(bShowResults); }));
            }
            else
            {
                try
                {
                    if (trc != null)
                    {
                        try
                        {
                            trc.Drop();
                        }
                        catch { }
                        trc = null;
                    }

                    bExecuting = false;
                    this.radioTraceTypeLive.Enabled = true;
                    this.radioTraceTypeSQL.Enabled = true;
                    this.btnSQLConnection.Enabled = true;
                    this.btnExecute.Text = "Execute";
                    this.btnExecute.Enabled = true;
                    timer1.Enabled = false;
                    if (bShowResults)
                    {
                        this.treeViewAggregation.Visible = true;
                        this.lblUnusedAggregationsToDelete.Visible = true;
                        this.Height = this.iWindowFullHeight;
                        this.buttonCancel.Visible = true;
                        this.buttonOK.Visible = true;

                        treeViewAggregation.Nodes.Clear();
                        this.treeViewAggregation.AfterCheck -= new System.Windows.Forms.TreeViewEventHandler(this.treeViewAggregation_AfterCheck);
                        FillTree();
                        treeViewAggregation.ExpandAll();
                        if (treeViewAggregation.Nodes.Count > 0) treeViewAggregation.Nodes[0].EnsureVisible();
                        this.treeViewAggregation.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAggregation_AfterCheck);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void FillTree()
        {
            Dictionary<string, long> dictHierarchyCardinality = new Dictionary<string, long>();
            try
            {
                AdomdRestrictionCollection restrictions = new AdomdRestrictionCollection();
                restrictions.Add(new AdomdRestriction("CATALOG_NAME", this.liveDB.Name));
                restrictions.Add(new AdomdRestriction("CUBE_NAME", this.liveCube.Name));
                restrictions.Add(new AdomdRestriction("HIERARCHY_VISIBILITY", 3)); //visible and non-visible hierarchies
                foreach (System.Data.DataRow r in adomdConnection.GetSchemaDataSet("MDSCHEMA_HIERARCHIES", restrictions).Tables[0].Rows)
                {
                    dictHierarchyCardinality.Add(Convert.ToString(r["HIERARCHY_UNIQUE_NAME"]), Convert.ToInt64(r["HIERARCHY_CARDINALITY"]));
                }
            }
            catch { }

            StringBuilder sbExport = new StringBuilder();
            sbExport.Append("Cube Dimension Name").Append("\t").Append("Attribute Name").Append("\t").Append("Number of Slices on this Attribute").Append("\t").Append("Attribute Cardinality").Append("\t").Append("Related Partition Count").Append("\t").Append("Currently Effective Optimized State").Append("\t").Append("Recommendation").AppendLine();
            foreach (CubeDimension cd in cloneCube.Dimensions)
            {
                TreeNode parentNode = treeViewAggregation.Nodes.Add(cd.Name);
                parentNode.Tag = cd;
                parentNode.ImageIndex = parentNode.SelectedImageIndex = 0;
                bool bAllIndexesChecked = true;

                foreach (CubeAttribute ca in cd.Attributes)
                {
                    if (ca.AttributeHierarchyEnabled && ca.Attribute.AttributeHierarchyEnabled)
                    {
                        CubeAttribute caSliced = null;
                        foreach (CubeAttribute sliced in dictHitIndexes.Keys)
                        {
                            if (sliced.Parent.ID == ca.Parent.ID && sliced.AttributeID == ca.AttributeID)
                            {
                                caSliced = sliced;
                                break;
                            }
                        }

                        string sNodeName = ca.Attribute.Name;
                        int iIndexHits = 0;
                        if (caSliced != null)
                        {
                            iIndexHits = dictHitIndexes[caSliced];
                            sNodeName += " (Index hits: " + iIndexHits.ToString("g") + ")";
                        }
                        TreeNode attributeNode = parentNode.Nodes.Add(sNodeName);
                        attributeNode.Tag = ca;
                        attributeNode.ImageIndex = attributeNode.SelectedImageIndex = 1;
                        attributeNode.Checked = (caSliced == null);
                        bAllIndexesChecked = bAllIndexesChecked && attributeNode.Checked;

                        bool bInEnabledHierarchy = false;
                        foreach (CubeHierarchy ch in ca.Parent.Hierarchies)
                        {
                            foreach (Microsoft.AnalysisServices.Level l in ch.Hierarchy.Levels)
                            {
                                if (l.SourceAttributeID == ca.AttributeID && ch.Enabled && ch.OptimizedState == OptimizationType.FullyOptimized)
                                {
                                    bInEnabledHierarchy = true;
                                }
                            }
                        }

                        OptimizationType optimized = ca.Attribute.AttributeHierarchyOptimizedState;
                        if (optimized == OptimizationType.FullyOptimized) optimized = ca.AttributeHierarchyOptimizedState;
                        if (optimized == OptimizationType.NotOptimized && bInEnabledHierarchy) optimized = OptimizationType.FullyOptimized;

                        string sRecommendation = "";
                        if (optimized == OptimizationType.FullyOptimized && iIndexHits == 0)
                        {
                            attributeNode.ForeColor = Color.Black;
                            sRecommendation = "Disable";
                            attributeNode.BackColor = Color.Green;
                            attributeNode.ToolTipText = "Currently indexed but the index was not used during the profiler trace. If left checked, BIDS Helper will disable the indexes when you click OK.";
                        }
                        else if (optimized == OptimizationType.NotOptimized)
                        {
                            attributeNode.ForeColor = Color.DarkGray;
                            attributeNode.ToolTipText = "Indexes not currently being built.";
                            if (iIndexHits > 0)
                            {
                                sRecommendation = "Enable";
                                attributeNode.ForeColor = Color.Black;
                                attributeNode.BackColor = Color.Red;
                                attributeNode.ToolTipText = "Currently not indexed but the queries observed during the profiler trace would have used the index if it had been built. If left unchecked, BIDS Helper will re-enable indexing when you click OK.";
                            }
                        }
                        else
                        {
                            attributeNode.ForeColor = Color.Black;
                            attributeNode.ToolTipText = "Indexes are being built and are used during the queries observed during the profiler trace.";
                        }

                        long? iCardinality = null;
                        string sAttributeUniqueName = "[" + ca.Parent.Name + "].[" + ca.Attribute.Name + "]";
                        if (dictHierarchyCardinality.ContainsKey(sAttributeUniqueName))
                        {
                            iCardinality = dictHierarchyCardinality[sAttributeUniqueName];
                        }

                        int iPartitions = 0;
                        try
                        {
                            foreach (MeasureGroup mg in liveCube.MeasureGroups)
                            {
                                if (mg.Dimensions.Contains(cd.ID))
                                {
                                    try
                                    {
                                        RegularMeasureGroupDimension rmgd = mg.Dimensions[cd.ID] as RegularMeasureGroupDimension;
                                        if (rmgd == null) continue;
                                        if (!AggManager.ValidateAggs.IsAtOrAboveGranularity(ca.Attribute, rmgd)) continue;
                                        iPartitions += mg.Partitions.Count;
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }

                        sbExport.Append(cd.Name).Append("\t").Append(ca.Attribute.Name).Append("\t").Append(iIndexHits).Append("\t").Append(iCardinality).Append("\t").Append(iPartitions).Append("\t").Append(optimized.ToString()).Append("\t").Append(sRecommendation).AppendLine();
                    }
                }
                parentNode.Checked = bAllIndexesChecked;
            }
            treeViewAggregation.Sort();
            sExport = sbExport.ToString();
        }

        void trc_Stopped(ITrace sender, TraceStoppedEventArgs e)
        {
            if (!bStopped)
                MessageBox.Show("The trace was stopped for the following cause: " + e.StopCause.ToString());
            FinishExecute(false);
        }

        void trc_OnEvent(object sender, TraceEventArgs e)
        {
            try
            {
                lock (this) //will this help ensure we run the code in serial, not in parallel?? it's important for this code to load the trace events in serial and in order
                {
                    MyTraceEventArgs args = new MyTraceEventArgs(e);
                    HandleTraceEvent(args);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    bStopped = true;
                    FinishExecute(false);
                }
                catch { }
                MessageBox.Show("There was a problem receiving a trace event: " + ex.Message);
            }
        }

        void HandleTraceEvent(MyTraceEventArgs e)
        {
            if (e.EventClass == TraceEventClass.QueryBegin && e.DatabaseName == liveDB.Name)
            {
                if (MDXHelper.IsDrillthroughQuery(e.TextData))
                {
                    lock (listDrillthroughQueries)
                    {
                        if (!listDrillthroughQueries.Contains(e.SessionIdOrSpid))
                            listDrillthroughQueries.Add(e.SessionIdOrSpid);
                    }
                }
                else
                {
                    iQueries++;
                }
            }
            else if (e.EventClass == TraceEventClass.QueryEnd && e.DatabaseName == liveDB.Name)
            {
                lock (listDrillthroughQueries)
                {
                    if (listDrillthroughQueries.Contains(e.SessionIdOrSpid))
                        listDrillthroughQueries.Remove(e.SessionIdOrSpid);
                }
            }
            else if (e.EventClass == TraceEventClass.QuerySubcubeVerbose && e.ObjectPath.StartsWith(sCubePath) && !listDrillthroughQueries.Contains(e.SessionIdOrSpid))
            {
                string sMeasureGroupID = e.ObjectPath.Substring(sCubePath.Length);
                MeasureGroup mg = cloneCube.MeasureGroups.Find(sMeasureGroupID);
                MeasureGroup liveMG = this.liveCube.MeasureGroups.Find(sMeasureGroupID);
                if (liveMG == null) return;

                List<CubeAttribute> attributesSliced = GetCubeAttributesWithSlice(e.TextData, liveMG);
                lock (dictHitIndexes)
                {
                    foreach (CubeAttribute ca in attributesSliced)
                    {
                        if (!dictHitIndexes.ContainsKey(ca))
                            dictHitIndexes.Add(ca, 1);
                        else
                            dictHitIndexes[ca]++;
                    }
                }

                iQuerySubcubeVerboseEvents++;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                //important to show the notification on the main thread of BIDS
                this.BeginInvoke(new MethodInvoker(delegate() { timer1_Tick(sender, e); }));
            }
            else
            {
                this.lblAggHits.Text = this.iQuerySubcubeVerboseEvents.ToString("g");
                this.lblQueries.Text = this.iQueries.ToString("g");
                this.lblTraceDuration.Text = FormatSeconds(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, this.dtTraceStarted, DateTime.Now, Microsoft.VisualBasic.FirstDayOfWeek.System, Microsoft.VisualBasic.FirstWeekOfYear.Jan1));
                this.lblAggHits.Refresh();
                this.lblQueries.Refresh();
                this.lblTraceDuration.Refresh();
                if (bStopped) FinishExecute(true);
            }
        }

        //format seconds as hours:minutes:seconds
        private string FormatSeconds(long iSeconds)
        {
            long iHours = iSeconds / 3600;
            long iMinutes = (iSeconds / 60) % 60;
            iSeconds = iSeconds % 60;
            string sReturn = "";
            if (iHours > 0)
                sReturn = iHours + ":" + iMinutes.ToString("00") + ":";
            else
                sReturn = iMinutes + ":";
            sReturn += iSeconds.ToString("00");
            return sReturn;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                //important to show the notification on the main thread of BIDS
                this.BeginInvoke(new MethodInvoker(delegate() { buttonOK_Click(sender, e); }));
            }
            else
            {
                try
                {
                    EnvDTE.Window w = projItem.Open(BIDSViewKinds.Designer); //opens the designer
                    IDesignerHost designer = (IDesignerHost)w.Object;
                    IComponentChangeService changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));
                    changesvc.OnComponentChanging(this.cloneCube, null);

                    dirtiedDimensions = new List<Microsoft.AnalysisServices.Dimension>();

                    //enable attributes
                    foreach (TreeNode nodeDimension in treeViewAggregation.Nodes)
                    {
                        foreach (TreeNode nodeAttribute in nodeDimension.Nodes)
                        {
                            if (!nodeAttribute.Checked)
                            {
                                CubeAttribute ca = (CubeAttribute)nodeAttribute.Tag;
                                ca.AttributeHierarchyOptimizedState = OptimizationType.FullyOptimized;

                                //if you're trying to enable indexes, make sure the dimension hierarchy is optimized, since you can't make the cube attribute optimized if the dimension attribute isn't
                                if (ca.Attribute.AttributeHierarchyOptimizedState == OptimizationType.NotOptimized)
                                {
                                    if (!dirtiedDimensions.Contains(ca.Attribute.Parent))
                                    {
                                        foreach (EnvDTE.ProjectItem pi in projItem.ContainingProject.ProjectItems)
                                        {
                                            if (!(pi.Object is Microsoft.AnalysisServices.Dimension)) continue;
                                            if ((Microsoft.AnalysisServices.Dimension)pi.Object != ca.Attribute.Parent) continue;
                                            bool bIsOpen = pi.get_IsOpen(EnvDTE.Constants.vsViewKindDesigner);
                                            EnvDTE.Window win = null;
                                            if (bIsOpen)
                                            {
                                                foreach (EnvDTE.Window w2 in projItem.DTE.Windows)
                                                {
                                                    if (w2.ProjectItem != null && w2.ProjectItem.Document != null && w2.ProjectItem.Document.FullName == pi.Document.FullName)
                                                    {
                                                        win = w2;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (win == null)
                                            {
                                                win = pi.Open(EnvDTE.Constants.vsViewKindDesigner);
                                                if (!bIsOpen) win.Visible = false;
                                            }

                                            IDesignerHost dimdesigner = (IDesignerHost)win.Object;
                                            IComponentChangeService dimchangesvc = (IComponentChangeService)dimdesigner.GetService(typeof(IComponentChangeService));
                                            dimchangesvc.OnComponentChanging(ca.Attribute.Parent, null);

                                            //perform the update
                                            ca.Attribute.AttributeHierarchyOptimizedState = OptimizationType.FullyOptimized;

                                            dimchangesvc.OnComponentChanged(ca.Attribute.Parent, null, null, null); //marks the dimension designer as dirty
                                        }

                                        dirtiedDimensions.Add(ca.Attribute.Parent);
                                    }
                                    else
                                    {
                                        ca.Attribute.AttributeHierarchyOptimizedState = OptimizationType.FullyOptimized;
                                    }
                                }
                            }
                            else
                            {
                                CubeAttribute ca = (CubeAttribute)nodeAttribute.Tag;
                                ca.AttributeHierarchyOptimizedState = OptimizationType.NotOptimized;

                                //if you're trying to disable indexes, make sure the hierarchy isn't set to optimized
                                foreach (CubeHierarchy ch in ca.Parent.Hierarchies)
                                {
                                    foreach (Microsoft.AnalysisServices.Level l in ch.Hierarchy.Levels)
                                    {
                                        if (l.SourceAttributeID == ca.AttributeID && ch.Enabled)
                                        {
                                            ch.OptimizedState = OptimizationType.NotOptimized;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    changesvc.OnComponentChanged(this.cloneCube, null, null, null); //marks the cube designer as dirty

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(ex.Message)) MessageBox.Show("Error saving: " + ex.Message);
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool bCheckClickInProgress = false;
        private void treeViewAggregation_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (this.InvokeRequired)
            {
                //important to show the notification on the main thread of BIDS
                this.BeginInvoke(new MethodInvoker(delegate() { treeViewAggregation_AfterCheck(sender, e); }));
            }
            else
            {
                if (bCheckClickInProgress) return;
                try
                {
                    bCheckClickInProgress = true;
                    TreeNode node = e.Node;
                    SetCheckOnChildrenAndRecurse(node, node.Checked);
                    while (node.Parent != null)
                    {
                        node = node.Parent;
                        bool bAllChildrenChecked = true;
                        foreach (TreeNode child in node.Nodes)
                        {
                            if (!child.Checked)
                            {
                                bAllChildrenChecked = false;
                                break;
                            }
                        }
                        node.Checked = bAllChildrenChecked;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    bCheckClickInProgress = false;
                }
            }
        }

        private void SetCheckOnChildrenAndRecurse(TreeNode node, bool bChecked)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = bChecked;
                SetCheckOnChildrenAndRecurse(child, bChecked);
            }
        }

        private void DeleteUnusedIndexes_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (trc != null)
                {
                    try
                    {
                        trc.Drop();
                    }
                    catch { }
                    trc = null;
                }
            }
            catch { }
        }

        private void btnSQLConnection_Click(object sender, EventArgs e)
        {
            AggManager.TraceTable form = new AggManager.TraceTable();
            DialogResult res = form.ShowDialog(this);
            if (res != DialogResult.OK) return;
            this.ConnectionString = form.ConnectionString;
            this.Table = form.Table;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(sExport);
            MessageBox.Show("The BIDS Helper recommendations have been saved to the clipboard.\r\n\r\nPaste these results into Excel.\r\n\r\nThese results do not reflect any checkboxes you have changed.");
        }

    }

}