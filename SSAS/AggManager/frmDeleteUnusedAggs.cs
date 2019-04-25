/*============================================================================
  File:    frmDeleteUnusedAggs.cs

  Summary: Contains the form to examine a trace to find unused aggs that can be deleted
============================================================================*/
/*
 * This file created new for BIDSHelper. 
 *    http://www.codeplex.com/BIDSHelper
 * 
 * It is not part of the official Agg Manager version: 
 * http://www.codeplex.com/MSFTASProdSamples                                   
 *                                                                             
 ============================================================================*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AnalysisServices;
using System.Data.SqlClient;

namespace AggManager
{
    public partial class DeleteUnusedAggs : Form
    {
        Server liveServer;
        Database liveDB;
        Cube liveCube;

        Database cloneDB;
        Cube cloneCube;
        MeasureGroup cloneMG;

        string sCubePath;
        Trace trc = null;
        Dictionary<Aggregation, int> dictHitAggs;
        string ConnectionString;
        string Table;

        private int iQueries = 0;
        private int iAggHits = 0;
        private DateTime dtTraceStarted;
        private bool bStopped = false;
        private bool bExecuting = false;
        private int iWindowFullHeight;
        private int iWindowShortHeight;
        private int iWindowFullWidth;
        private int iWindowNarrowWidth;

        public DeleteUnusedAggs()
        {
            InitializeComponent();
        }

        public void Init(
           EnvDTE.ProjectItem projItem,
           Database cloneDB,
           MeasureGroup cloneMG)
        {
            Cube selectedCube = projItem.Object as Cube;
            if ((selectedCube != null) && (selectedCube.ParentServer != null))
            {
                // if we are in Online mode there will be a parent server
                this.liveServer = selectedCube.ParentServer;
                this.liveDB = selectedCube.Parent;
                this.liveCube = selectedCube;
            }
            else
            {
                // if we are in Project mode we will use the server name from 
                // the deployment settings
                DeploymentSettings deploySet = new DeploymentSettings(projItem);
                this.liveServer = new Server();
                this.liveServer.Connect(deploySet.TargetServer);
                this.liveDB = this.liveServer.Databases.GetByName(deploySet.TargetDatabase);
                this.liveCube = this.liveDB.Cubes.GetByName(selectedCube.Name);
            }
            sCubePath = this.liveServer.ID + "." + this.liveDB.ID + "." + this.liveCube.ID + ".";

            this.cloneMG = cloneMG;
            this.cloneDB = cloneDB;
            this.cloneCube = cloneDB.Cubes[liveCube.ID];

            this.lblServer.Text = this.liveServer.Name;
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

        private void Execute()
        {
            dictHitAggs = new Dictionary<Aggregation, int>();
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
            this.iAggHits = 0;
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

                    string sTraceID = "BIDS Helper Delete Unused Aggs Trace " + System.Guid.NewGuid().ToString();
                    trc = liveServer.Traces.Add(sTraceID, sTraceID);
                    trc.OnEvent += new TraceEventHandler(trc_OnEvent);
                    trc.Stopped += new TraceStoppedEventHandler(trc_Stopped);
                    trc.AutoRestart = true;

                    TraceEvent te;
                    te = trc.Events.Add(TraceEventClass.QueryEnd);
                    te.Columns.Add(TraceColumn.DatabaseName);
                    te.Columns.Add(TraceColumn.EventSubclass);

                    te = trc.Events.Add(TraceEventClass.GetDataFromAggregation);
                    te.Columns.Add(TraceColumn.DatabaseName);
                    te.Columns.Add(TraceColumn.TextData);
                    te.Columns.Add(TraceColumn.ObjectPath);

                    trc.Update();
                    trc.Start();

                    this.btnExecute.Text = "Stop Trace";
                    this.btnExecute.Enabled = true;
                }
                else
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "select * from " + Table; //just select everything and filter in .NET (which allows us to throw better error messages if a column is missing
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        if (!ReaderContainsColumn(reader, "EventClass"))
                            MessageBox.Show("Table " + Table + " must contain EventClass column");
                        else if (!ReaderContainsColumn(reader, "EventSubclass"))
                            MessageBox.Show("Table " + Table + " must contain EventSubclass column");
                        else if (!ReaderContainsColumn(reader, "TextData"))
                            MessageBox.Show("Table " + Table + " must contain TextData column");
                        else if (!ReaderContainsColumn(reader, "ObjectPath"))
                            MessageBox.Show("Table " + Table + " must contain ObjectPath column");
                        else
                        {
                            this.dtTraceStarted = DateTime.Now;
                            string sDateColumnName = "";
                            if (ReaderContainsColumn(reader, "StartTime"))
                                sDateColumnName = "StartTime";
                            else if (ReaderContainsColumn(reader, "CurrentTime"))
                                sDateColumnName = "CurrentTime";
                            DateTime dtMin = DateTime.MaxValue;
                            DateTime dtMax = DateTime.MinValue;
                            while (reader.Read())
                            {
                                MyTraceEventArgs arg = new MyTraceEventArgs(reader);
                                HandleTraceEvent(arg);
                                if (!string.IsNullOrEmpty(sDateColumnName) && !Convert.IsDBNull(reader[sDateColumnName]))
                                {
                                    DateTime dt = Convert.ToDateTime(reader[sDateColumnName]);
                                    if (dtMin > dt) dtMin = dt;
                                    if (dtMax < dt) dtMax = dt;
                                    long iSecondsDiff = (long)(dtMax - dtMin).TotalSeconds;// Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, dtMin, dtMax, Microsoft.VisualBasic.FirstDayOfWeek.System, Microsoft.VisualBasic.FirstWeekOfYear.Jan1);
                                    this.dtTraceStarted = DateTime.Now.AddSeconds(-iSecondsDiff); // Microsoft.VisualBasic.DateAndTime.DateAdd(Microsoft.VisualBasic.DateInterval.Second, -iSecondsDiff, DateTime.Now);
                                }
                                timer1_Tick(null, null);
                                Application.DoEvents();
                            }
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
            public TraceEventSubclass EventSubclass;
            public string DatabaseName = null;
            public string ObjectPath = null;
            public string TextData = null;

            public MyTraceEventArgs(TraceEventArgs args)
            {
                EventClass = args.EventClass;
                EventSubclass = args.EventSubclass;
                DatabaseName = args.DatabaseName;
                ObjectPath = args.ObjectPath;
                TextData = args.TextData;
            }

            public MyTraceEventArgs(SqlDataReader reader)
            {
                EventClass = (TraceEventClass)Enum.ToObject(typeof(TraceEventClass), Convert.ToInt64(reader["EventClass"]));
                
                //can't parse EventSubclass like EventClass
                //following code based on http://msdn2.microsoft.com/en-us/library/ms174472.aspx
                if (!Convert.IsDBNull(reader["EventSubclass"]))
                {
                    if (EventClass == TraceEventClass.QueryEnd)
                    {
                        if (Convert.ToInt64(reader["EventSubclass"]) == 0)
                            EventSubclass = TraceEventSubclass.MdxQuery;
                        else if (Convert.ToInt64(reader["EventSubclass"]) == 1)
                            EventSubclass = TraceEventSubclass.DmxQuery;
                        else if (Convert.ToInt64(reader["EventSubclass"]) == 2)
                            EventSubclass = TraceEventSubclass.SqlQuery;
                        else if (Convert.ToInt64(reader["EventSubclass"]) == 3)
                            EventSubclass = TraceEventSubclass.DAXQuery;
                        else
                            EventSubclass = TraceEventSubclass.NotAvailable;
                    }
                }

                if (!Convert.IsDBNull(reader["ObjectPath"]))
                {
                    string[] pathParts = reader["ObjectPath"].ToString().Split('.');
                    DatabaseName = pathParts[1];
                    //TODO: path contains ID... won't match name?
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
            }
        }

        private void FinishExecute(bool bShowResults)
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
                    if (cloneMG == null)
                    {
                        foreach (MeasureGroup mg in cloneCube.MeasureGroups)
                        {
                            FillTree(mg);
                        }
                    }
                    else
                    {
                        FillTree(cloneMG);
                    }
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

        private void FillTree(MeasureGroup mg)
        {
            TreeNode parentNode = treeViewAggregation.Nodes.Add(mg.Name);
            parentNode.Tag = mg;
            parentNode.ImageIndex = parentNode.SelectedImageIndex = 0;
            bool bAllAggDesignsChecked = true;
            foreach (AggregationDesign aggDesign in mg.AggregationDesigns)
            {
                if (aggDesign.Aggregations.Count == 0) continue;
                bool bAggDesignUsed = false;
                foreach (Partition p in mg.Partitions)
                {
                    if (p.AggregationDesignID == aggDesign.ID)
                    {
                        bAggDesignUsed = true;
                        break;
                    }
                }
                if (!bAggDesignUsed) continue;

                TreeNode aggDesignNode = parentNode.Nodes.Add(aggDesign.Name);
                aggDesignNode.Tag = aggDesign;
                aggDesignNode.ImageIndex = aggDesignNode.SelectedImageIndex = 1;
                bool bAllAggsChecked = true;
                foreach (Aggregation agg in aggDesign.Aggregations)
                {
                    int iAggHits = (dictHitAggs.ContainsKey(agg) ? dictHitAggs[agg] : 0);
                    TreeNode aggNode = aggDesignNode.Nodes.Add(agg.Name + (iAggHits > 0 ? " (" + iAggHits.ToString("d") + " agg hits)" : ""));
                    aggNode.Tag = agg;
                    aggNode.ImageIndex = aggNode.SelectedImageIndex = 2;
                    aggNode.Checked = !dictHitAggs.ContainsKey(agg);
                    bAllAggsChecked = bAllAggsChecked && aggNode.Checked;
                }
                aggDesignNode.Checked = bAllAggsChecked;
                aggDesignNode.StateImageIndex = 2;
                bAllAggDesignsChecked = bAllAggDesignsChecked && bAllAggsChecked;
            }
            parentNode.Checked = bAllAggDesignsChecked;
            if (parentNode.Nodes.Count == 0) treeViewAggregation.Nodes.Remove(parentNode);
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
                MyTraceEventArgs args = new MyTraceEventArgs(e);
                HandleTraceEvent(args);
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
            if (e.EventClass == TraceEventClass.QueryEnd 
                && (e.EventSubclass == TraceEventSubclass.MdxQuery 
                 || e.EventSubclass == TraceEventSubclass.DAXQuery) 
                && e.DatabaseName == liveDB.Name)
                iQueries++;
            else if (e.EventClass == TraceEventClass.GetDataFromAggregation && e.DatabaseName == liveDB.Name && e.ObjectPath.StartsWith(sCubePath))
            {
                string sMeasureGroupID = e.ObjectPath.Substring(sCubePath.Length);
                string sPartitionID = sMeasureGroupID.Substring(sMeasureGroupID.IndexOf('.') + 1);
                sMeasureGroupID = sMeasureGroupID.Substring(0, sMeasureGroupID.IndexOf('.'));
                MeasureGroup mg = cloneCube.MeasureGroups.Find(sMeasureGroupID);
                if (cloneMG == null) //if checking for hit aggs on entire cube
                {
                    if (mg == null)
                        return;
                }
                else
                {
                    if (mg == null || sMeasureGroupID != cloneMG.ID)
                        return;
                }
                MeasureGroup liveMG = this.liveCube.MeasureGroups.Find(sMeasureGroupID);
                if (liveMG == null) return;

                Partition livePartition = liveMG.Partitions.Find(sPartitionID);
                if (livePartition == null) return;

                string sAggID = e.TextData.Split(new char[] { '\r', '\n' })[0];
                if (livePartition.AggregationDesign == null) return;
                Aggregation liveAgg = livePartition.AggregationDesign.Aggregations.Find(sAggID);
                if (liveAgg == null) return;

                //found this aggregation on the live cube... now find the equivalent agg in the cloned (local) cube
                AggregationDesign cloneAggDesign = mg.AggregationDesigns.Find(liveAgg.Parent.ID);
                if (cloneAggDesign == null) return;

                Aggregation cloneAgg = cloneAggDesign.Aggregations.Find(sAggID);
                if (cloneAgg == null) return;

                lock (dictHitAggs)
                {
                    if (dictHitAggs.ContainsKey(cloneAgg))
                    {
                        dictHitAggs[cloneAgg]++;
                    }
                    else
                    {
                        dictHitAggs.Add(cloneAgg, 1);
                    }
                }
                iAggHits++;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.lblAggHits.Text = this.iAggHits.ToString("g");
            this.lblQueries.Text = this.iQueries.ToString("g");
            this.lblTraceDuration.Text = FormatSeconds((long)(DateTime.Now - dtTraceStarted).TotalSeconds);// Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, this.dtTraceStarted, DateTime.Now, Microsoft.VisualBasic.FirstDayOfWeek.System, Microsoft.VisualBasic.FirstWeekOfYear.Jan1));
            this.lblAggHits.Refresh();
            this.lblQueries.Refresh();
            this.lblTraceDuration.Refresh();
            if (bStopped) FinishExecute(true);
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
            try
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message)) MessageBox.Show("Error saving: " + ex.Message);
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

        private void SetCheckOnChildrenAndRecurse(TreeNode node, bool bChecked)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = bChecked;
                SetCheckOnChildrenAndRecurse(child, bChecked);
            }
        }

        private void DeleteUnusedAggs_FormClosed(object sender, FormClosedEventArgs e)
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

    }

}