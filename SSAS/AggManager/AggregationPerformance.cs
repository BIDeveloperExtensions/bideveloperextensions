using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.AdomdClient;
using System.Data;

namespace AggManager
{
    public class AggregationPerformanceTester
    {

        private string _errors = "";
        private AggregationDesign _currentAggD;
        private string _currentMGPath;
        private Dictionary<Aggregation, int> _dictHitAggs = new Dictionary<Aggregation, int>();
        private bool _queryEnded; //marks whether the session traces have caught up
        private long _queryDuration;
        private Trace _trc;
        private string _sessionID;
        private bool _cancelled = false;
        private List<AggregationPerformance> _listAggPerf = new List<AggregationPerformance>();
        private List<MissingAggregationPerformance> _listMissingAggPerf = new List<MissingAggregationPerformance>();
        private static Dictionary<MeasureGroupAttribute, string> _dictDefaultMembers = new Dictionary<MeasureGroupAttribute, string>();
        private AggregationDesign _emptyAggregationDesign = new AggregationDesign(); //a dictionary key can't be null, so we use this instead
        private bool _testAgg;
        private bool _testNoAggs;
        private bool _testWithoutSomeAggs;
        private int _totalIterations;

        public AggregationPerformanceTester(AggregationDesign aggD, bool TestAgg, bool TestNoAggs, bool TestWithoutSomeAggs)
        {
            _currentAggD = aggD;
            _currentMGPath = aggD.ParentServer.ID + "." + aggD.ParentDatabase.ID + "." + aggD.ParentCube.ID + "." + aggD.Parent.ID + ".";
            _testAgg = TestAgg;
            _testNoAggs = TestNoAggs;
            _testWithoutSomeAggs = TestWithoutSomeAggs;
            _totalIterations = (TestAgg ? 1 : 0) + (TestNoAggs ? 1 : 0) + (TestWithoutSomeAggs ? 1 : 0);
        }

        public void Cancel()
        {
            try
            {
                if (!_cancelled)
                {
                    _cancelled = true;
                    RaiseProgressEvent(99, "Starting cancel...");
                    Server s = new Server();
                    s.Connect("Data Source=" + _currentAggD.ParentServer.Name);
                    s.CancelSession(_sessionID);
                    RaiseProgressEvent(100, "Successfully cancelled...");
                }
            }
            catch (Exception ex)
            {
                RaiseProgressEvent(99, "Trying to cancel...\r\n" + ex.Message + " " + ex.StackTrace);
            }
        }

        public AggregationPerformance[] Results
        {
            get { return _listAggPerf.ToArray(); }
        }

        public MissingAggregationPerformance[] MissingResults
        {
            get { return _listMissingAggPerf.ToArray(); }
        }

        public void StartTest()
        {
            try
            {
                Dictionary<Aggregation, long> dictAggRowCount = new Dictionary<Aggregation, long>();
                Dictionary<AggregationDesign, long> dictAggDesignRowCount = new Dictionary<AggregationDesign, long>();

                AdomdConnection conn = new AdomdConnection("Data Source=" + _currentAggD.ParentServer.Name + ";Initial Catalog=" + _currentAggD.ParentDatabase.Name);
                conn.Open();
                _sessionID = conn.SessionID;

                if (_cancelled) return;

                foreach (Partition p in _currentAggD.Parent.Partitions)
                {
                    if (p.AggregationDesignID != _currentAggD.ID) continue;

                    RaiseProgressEvent(0, "Retrieving list of processed aggs in partition " + p.Name + "...");

                    AdomdRestrictionCollection coll = new AdomdRestrictionCollection();
                    coll.Add("DATABASE_NAME", _currentAggD.ParentDatabase.Name);
                    coll.Add("CUBE_NAME", _currentAggD.ParentCube.Name);
                    coll.Add("MEASURE_GROUP_NAME", p.Parent.Name);
                    coll.Add("PARTITION_NAME", p.Name);
                    DataSet aggDS = conn.GetSchemaDataSet("DISCOVER_PARTITION_STAT", coll);
                    foreach (DataRow row in aggDS.Tables[0].Rows)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(row["AGGREGATION_NAME"])))
                        {
                            Aggregation a = p.AggregationDesign.Aggregations.FindByName(Convert.ToString(row["AGGREGATION_NAME"]));
                            if (a == null) throw new Exception("Couldn't find aggregation [" + row["AGGREGATION_NAME"] + "]");
                            long lngAggRowCount = Convert.ToInt64(row["AGGREGATION_SIZE"]);
                            if (lngAggRowCount > 0)
                            {
                                if (!dictAggRowCount.ContainsKey(a))
                                    dictAggRowCount.Add(a, lngAggRowCount);
                                else
                                    dictAggRowCount[a] += lngAggRowCount;
                            }
                        }
                        else
                        {
                            long lngPartitionRowCount = Convert.ToInt64(row["AGGREGATION_SIZE"]);
                            if (!dictAggDesignRowCount.ContainsKey(p.AggregationDesign ?? _emptyAggregationDesign))
                                dictAggDesignRowCount.Add(p.AggregationDesign ?? _emptyAggregationDesign, lngPartitionRowCount);
                            else
                                dictAggDesignRowCount[p.AggregationDesign ?? _emptyAggregationDesign] += lngPartitionRowCount;
                        }
                        if (_cancelled) return;
                    }
                }

                if (dictAggRowCount.Count == 0) return;

                //figure out any DefaultMember that aren't the all member
                string sDefaultMembersCalcs = "";
                string sDefaultMembersCols = "";
                foreach (MeasureGroupDimension mgd in _currentAggD.Parent.Dimensions)
                {
                    RegularMeasureGroupDimension rmgd = mgd as RegularMeasureGroupDimension;
                    if (rmgd == null) continue;
                    foreach (MeasureGroupAttribute mga in rmgd.Attributes)
                    {
                        if (mga.CubeAttribute.AttributeHierarchyEnabled && mga.Attribute.AttributeHierarchyEnabled)
                        {
                            sDefaultMembersCalcs += "MEMBER [Measures].[|" + mga.CubeAttribute.Parent.Name + " | " + mga.CubeAttribute.Attribute.Name + "|] as iif([" + mga.CubeAttribute.Parent.Name + "].[" + mga.CubeAttribute.Attribute.Name + "].DefaultMember.Level.Name = \"(All)\", null, [" + mga.CubeAttribute.Parent.Name + "].[" + mga.CubeAttribute.Attribute.Name + "].DefaultMember.UniqueName)\r\n";
                            if (sDefaultMembersCols.Length > 0) sDefaultMembersCols += ",";
                            sDefaultMembersCols += "[Measures].[|" + mga.CubeAttribute.Parent.Name + " | " + mga.CubeAttribute.Attribute.Name + "|]\r\n";
                        }
                    }
                }

                RaiseProgressEvent(1, "Detecting DefaultMember on each dimension attribute...");

                AdomdCommand cmd = new AdomdCommand();
                cmd.Connection = conn;
                cmd.CommandText = "with\r\n"
                    + sDefaultMembersCalcs
                    + "select {\r\n"
                    + sDefaultMembersCols
                    + "} on 0\r\n"
                    + "from [" + _currentAggD.ParentCube.Name.Replace("]", "]]") + "]";
                CellSet cs = cmd.ExecuteCellSet();

                int iCol = 0;
                _dictDefaultMembers.Clear();
                foreach (MeasureGroupDimension mgd in _currentAggD.Parent.Dimensions)
                {
                    RegularMeasureGroupDimension rmgd = mgd as RegularMeasureGroupDimension;
                    if (rmgd == null) continue;
                    foreach (MeasureGroupAttribute mga in rmgd.Attributes)
                    {
                        if (mga.CubeAttribute.AttributeHierarchyEnabled && mga.Attribute.AttributeHierarchyEnabled)
                        {
                            string sValue = Convert.ToString(cs.Cells[iCol++].Value);
                            if (!string.IsNullOrEmpty(sValue))
                            {
                                _dictDefaultMembers.Add(mga, sValue);
                            }
                        }
                    }
                }

                conn.Close(false);

                if (_cancelled) return;

                RaiseProgressEvent(2, "Starting trace...");

                Server s = new Server();
                s.Connect("Data Source=" + _currentAggD.ParentServer.Name, _sessionID);

                Server sAlt = new Server();
                sAlt.Connect("Data Source=" + _currentAggD.ParentServer.Name);
                MeasureGroup mgAlt = sAlt.Databases.GetByName(_currentAggD.ParentDatabase.Name).Cubes.GetByName(_currentAggD.ParentCube.Name).MeasureGroups.GetByName(_currentAggD.Parent.Name);

                try
                {
                    Database db = s.Databases.GetByName(_currentAggD.ParentDatabase.Name);

                    string sTraceID = "BIDS Helper Aggs Performance Trace " + System.Guid.NewGuid().ToString();
                    _trc = s.Traces.Add(sTraceID, sTraceID);
                    _trc.OnEvent += new TraceEventHandler(trace_OnEvent);
                    _trc.Stopped += new TraceStoppedEventHandler(trace_Stopped);
                    _trc.AutoRestart = false;

                    TraceEvent te;
                    te = _trc.Events.Add(TraceEventClass.QueryEnd);
                    te.Columns.Add(TraceColumn.Duration);
                    te.Columns.Add(TraceColumn.SessionID);

                    te = _trc.Events.Add(TraceEventClass.GetDataFromAggregation);
                    te.Columns.Add(TraceColumn.ObjectPath);
                    te.Columns.Add(TraceColumn.TextData);
                    te.Columns.Add(TraceColumn.SessionID);
                    te.Columns.Add(TraceColumn.ConnectionID);

                    _trc.Update();
                    _trc.Start();

                    if (_cancelled) return;

                    s.BeginTransaction();
                    UnprocessOtherPartitions(s);

                    int i = 0;
                    if (_testAgg)
                    {
                        foreach (Aggregation a in dictAggRowCount.Keys)
                        {
                            RaiseProgressEvent(3 + (int)(87.0 * i++ / dictAggRowCount.Count / _totalIterations), "Testing performance with agg " + i + " of " + dictAggRowCount.Count + " (" + a.Name + ")...");

                            AggregationPerformance aggP = new AggregationPerformance(a);
                            aggP.AggregationRowCount = dictAggRowCount[a];
                            aggP.PartitionRowCount = dictAggDesignRowCount[a.Parent];
                            aggP.MeasureGroupRowCount = aggP.PartitionRowCount; //if there are multiple aggregation designs, outside code will fix that

                            ServerExecute(s, "<ClearCache xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">" + "\r\n"
                            + "    <Object>" + "\r\n"
                            + "      <DatabaseID>" + _currentAggD.ParentDatabase.ID + "</DatabaseID>" + "\r\n"
                            + "      <CubeID>" + _currentAggD.ParentCube.ID + "</CubeID>" + "\r\n"
                            + "    </Object>" + "\r\n"
                            + "  </ClearCache>");

                            _queryEnded = false;

                            //initialize the MDX script with a no-op query
                            ServerExecuteMDX(db, "with member [Measures].[_Exec MDX Script_] as null select [Measures].[_Exec MDX Script_] on 0 from [" + _currentAggD.ParentCube.Name.Replace("]", "]]") + "]", _sessionID);

                            while (!this._queryEnded) //wait for session trace query end event
                            {
                                if (_cancelled) return;
                                System.Threading.Thread.Sleep(100);
                            }

                            aggP.ScriptPerformanceWithAgg = _queryDuration;

                            _queryEnded = false;
                            //don't clear dictHitAggs because if an agg got hit during the ExecuteMDXScript event, then it will be cached for the query

                            ServerExecuteMDX(db, aggP.PerformanceTestMDX, _sessionID);

                            while (!this._queryEnded) //wait for session trace query end event
                            {
                                if (_cancelled) return;
                                System.Threading.Thread.Sleep(100);
                            }

                            aggP.QueryPerformanceWithAgg = _queryDuration;

                            if (_dictHitAggs.ContainsKey(a))
                                aggP.HitAggregation = true;

                            aggP.AggHits = _dictHitAggs;
                            _dictHitAggs = new Dictionary<Aggregation, int>();

                            _listAggPerf.Add(aggP);

                            if (_cancelled) return;
                        }
                    }


                    if (_testWithoutSomeAggs && _listAggPerf.Count > 0)
                    {
                        RaiseProgressEvent(4 + (int)(87.0 * i / dictAggRowCount.Count / _totalIterations), "Dropping some aggs inside a transaction...");

                        //build list of all aggs which were hit, and which are contained within another agg
                        List<AggregationPerformance> allAggs = new List<AggregationPerformance>();
                        foreach (AggregationPerformance ap in _listAggPerf)
                        {
                            if (!ap.HitAggregation) continue;
                            foreach (AggregationPerformance ap2 in _listAggPerf)
                            {
                                if (ap.Aggregation != ap2.Aggregation && ap.Aggregation.Parent == ap2.Aggregation.Parent && SearchSimilarAggs.IsAggregationIncluded(ap.Aggregation, ap2.Aggregation, false))
                                {
                                    allAggs.Add(ap);
                                    break;
                                }
                            }
                        }
                        allAggs.Sort(delegate(AggregationPerformance a, AggregationPerformance b)
                            {
                                int iCompare = 0;
                                try
                                {
                                    if (a == b || a.Aggregation == b.Aggregation) return 0;
                                    iCompare = a.AggregationRowCount.CompareTo(b.AggregationRowCount);
                                    if (iCompare == 0)
                                    {
                                        //if the aggs are the same rowcount, then sort by whether one is contained in the other
                                        if (SearchSimilarAggs.IsAggregationIncluded(a.Aggregation, b.Aggregation, false))
                                            return -1;
                                        else if (SearchSimilarAggs.IsAggregationIncluded(b.Aggregation, a.Aggregation, false))
                                            return 1;
                                        else
                                            return 0;
                                    }
                                }
                                catch { }
                                return iCompare;
                            });

                        List<AggregationPerformance> deletedAggregationPerfs = new List<AggregationPerformance>();
                        List<AggregationPerformance> nextAggs = new List<AggregationPerformance>();
                        List<AggregationPerformance> aggsToSkipTesting = new List<AggregationPerformance>();

                        System.Diagnostics.Stopwatch timerProcessIndexes = new System.Diagnostics.Stopwatch();
                        long lngLastProcessIndexesTime = 0;
                        AggregationPerformance lastDeletedAggregationPerf = null;

                        while (allAggs.Count > 0)
                        {
                            AggregationPerformance aggP = null;
                            if (nextAggs.Count == 0)
                            {
                                aggP = allAggs[0];
                                allAggs.RemoveAt(0);
                            }
                            else
                            {
                                aggP = nextAggs[0];
                                nextAggs.RemoveAt(0);
                                allAggs.Remove(aggP);
                            }
                            deletedAggregationPerfs.Add(aggP);

                            //capture XMLA for deleting aggs
                            AggregationDesign aggD = mgAlt.AggregationDesigns.GetByName(aggP.Aggregation.Parent.Name);
                            aggD.ParentServer.CaptureXml = true;
                            foreach (AggregationPerformance ap in deletedAggregationPerfs)
                            {
                                if (aggD.Aggregations.ContainsName(ap.Aggregation.Name))
                                {
                                    aggD.Aggregations.RemoveAt(aggD.Aggregations.IndexOfName(ap.Aggregation.Name));
                                }
                            }
                            aggD.Update(UpdateOptions.ExpandFull);
                            string sAlterXMLA = aggD.ParentServer.CaptureLog[0];
                            aggD.ParentServer.CaptureLog.Clear();
                            aggD.ParentServer.CaptureXml = false;
                            aggD.Refresh(true); //get the deleted aggs back

                            ServerExecute(s, sAlterXMLA);

                            if (_cancelled) return;

                            RaiseProgressEvent(5 + (int)(87.0 * i++ / dictAggRowCount.Count / _totalIterations), "Processing aggs without some aggs " + ((i - 1) % dictAggRowCount.Count + 1) + " of " + dictAggRowCount.Count + " (" + aggP.AggregationName + ")...");

                            timerProcessIndexes.Reset();
                            timerProcessIndexes.Start();

                            //process aggs to delete existing aggs
                            ServerExecute(s, "<Batch xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">" + "\r\n"
                                + "  <Parallel>" + "\r\n"
                                + "    <Process xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ddl2=\"http://schemas.microsoft.com/analysisservices/2003/engine/2\" xmlns:ddl2_2=\"http://schemas.microsoft.com/analysisservices/2003/engine/2/2\" xmlns:ddl100_100=\"http://schemas.microsoft.com/analysisservices/2008/engine/100/100\">" + "\r\n"
                                + "      <Object>" + "\r\n"
                                + "       <DatabaseID>" + _currentAggD.ParentDatabase.ID + "</DatabaseID>" + "\r\n"
                                + "       <CubeID>" + _currentAggD.ParentCube.ID + "</CubeID>" + "\r\n"
                                + "       <MeasureGroupID>" + _currentAggD.Parent.ID + "</MeasureGroupID>" + "\r\n"
                                + "      </Object>" + "\r\n"
                                + "      <Type>ProcessIndexes</Type>" + "\r\n"
                                + "      <WriteBackTableCreation>UseExisting</WriteBackTableCreation>" + "\r\n"
                                + "    </Process>" + "\r\n"
                                + "  </Parallel>" + "\r\n"
                                + "</Batch>" + "\r\n");
                            if (!string.IsNullOrEmpty(_errors)) throw new Exception(_errors);

                            timerProcessIndexes.Stop();

                            //record time it took to process aggs... compare how long the prior one took, then you can determine how much incremental time was spent on the newly deleted agg
                            if (lastDeletedAggregationPerf != null)
                                lastDeletedAggregationPerf.ProcessIndexesDuration = lngLastProcessIndexesTime - timerProcessIndexes.ElapsedMilliseconds;

                            lngLastProcessIndexesTime = timerProcessIndexes.ElapsedMilliseconds;
                            lastDeletedAggregationPerf = aggP;

                            if (_cancelled) return;

                            int j = 0;
                            foreach (AggregationPerformance deleteAP in deletedAggregationPerfs)
                            {
                                RaiseProgressEvent(6 + (int)(87.0 * i / dictAggRowCount.Count / _totalIterations), "Testing performance without some aggs " + ((i - 1) % dictAggRowCount.Count + 1) + " of " + dictAggRowCount.Count + "\r\nTesting agg " + (++j) + " of " + deletedAggregationPerfs.Count + " (" + deleteAP.AggregationName + ")...");

                                if (aggsToSkipTesting.Contains(deleteAP)) continue; //skip this agg if we've already determined it won't hit another agg

                                ServerExecute(s, "<ClearCache xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">" + "\r\n"
                                + "    <Object>" + "\r\n"
                                + "      <DatabaseID>" + _currentAggD.ParentDatabase.ID + "</DatabaseID>" + "\r\n"
                                + "      <CubeID>" + _currentAggD.ParentCube.ID + "</CubeID>" + "\r\n"
                                + "    </Object>" + "\r\n"
                                + "  </ClearCache>");

                                _queryEnded = false;

                                //initialize the MDX script with a no-op query
                                ServerExecuteMDX(db, "with member [Measures].[_Exec MDX Script_] as null select [Measures].[_Exec MDX Script_] on 0 from [" + _currentAggD.ParentCube.Name.Replace("]", "]]") + "]", _sessionID);

                                while (!this._queryEnded) //wait for session trace query end event
                                {
                                    if (_cancelled) return;
                                    System.Threading.Thread.Sleep(100);
                                }

                                long lngScriptDuration = _queryDuration;

                                _queryEnded = false;
                                //don't clear dictHitAggs because if an agg got hit during the ExecuteMDXScript event, then it will be cached for the query

                                ServerExecuteMDX(db, deleteAP.PerformanceTestMDX, _sessionID);

                                while (!this._queryEnded) //wait for session trace query end event
                                {
                                    if (_cancelled) return;
                                    System.Threading.Thread.Sleep(100);
                                }

                                long lngQueryDuration = _queryDuration;

                                List<Aggregation> deletedAggregations = new List<Aggregation>();
                                foreach (AggregationPerformance a in deletedAggregationPerfs)
                                {
                                    deletedAggregations.Add(a.Aggregation);
                                }

                                MissingAggregationPerformance missingAggPerf = new MissingAggregationPerformance(deleteAP, deletedAggregations.ToArray(), _dictHitAggs);
                                _listMissingAggPerf.Add(missingAggPerf);
                                missingAggPerf.QueryPerformance = lngQueryDuration;
                                missingAggPerf.ScriptPerformance = lngScriptDuration;

                                foreach (Aggregation a in missingAggPerf.AggHitsDiff)
                                {
                                    foreach (AggregationPerformance ap in allAggs)
                                    {
                                        if (ap.Aggregation == a && !nextAggs.Contains(ap))
                                            nextAggs.Add(ap);
                                    }
                                }

                                if (missingAggPerf.AggHitsDiff.Length == 0)
                                {
                                    aggsToSkipTesting.Add(deleteAP);
                                }
                                else
                                {
                                    bool bThisAggContainedInRemainingAgg = false;
                                    foreach (AggregationPerformance ap2 in allAggs)
                                    {
                                        if (deleteAP.Aggregation != ap2.Aggregation && deleteAP.Aggregation.Parent == ap2.Aggregation.Parent && SearchSimilarAggs.IsAggregationIncluded(deleteAP.Aggregation, ap2.Aggregation, false))
                                        {
                                            bThisAggContainedInRemainingAgg = true;
                                            break;
                                        }
                                    }
                                    if (!bThisAggContainedInRemainingAgg)
                                        aggsToSkipTesting.Add(deleteAP); //stop testing this agg when it's not contained in any remaining aggs that need to be tested
                                }

                                _dictHitAggs = new Dictionary<Aggregation, int>();
                            }

                            if (_cancelled) return;

                            s.RollbackTransaction();
                            s.BeginTransaction();

                            UnprocessOtherPartitions(s);
                        }
                    }


                    if (_testNoAggs)
                    {
                        //ensure the counter is where it's supposed to be since the "test with some aggs" test may not have done iterations for every agg
                        i = Math.Max(i, dictAggRowCount.Count * (_totalIterations - 1));

                        RaiseProgressEvent(4 + (int)(87.0 * i / dictAggRowCount.Count / _totalIterations), "Dropping all aggs inside a transaction...");

                        //delete all aggs in all aggregation designs
                        string sXMLA = "<Batch xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\" xmlns:as=\"http://schemas.microsoft.com/analysisservices/2003/engine\" xmlns:dwd=\"http://schemas.microsoft.com/DataWarehouse/Designer/1.0\">" + "\r\n"
                            + "  <Alter AllowCreate=\"true\" ObjectExpansion=\"ExpandFull\">" + "\r\n"
                            + "    <Object>" + "\r\n"
                            + "      <DatabaseID>" + _currentAggD.Parent.ParentDatabase.ID + "</DatabaseID>" + "\r\n"
                            + "      <CubeID>" + _currentAggD.Parent.Parent.ID + "</CubeID>" + "\r\n"
                            + "      <MeasureGroupID>" + _currentAggD.Parent.ID + "</MeasureGroupID>" + "\r\n"
                            + "      <AggregationDesignID>" + _currentAggD.ID + "</AggregationDesignID>" + "\r\n"
                            + "    </Object>" + "\r\n"
                            + "    <ObjectDefinition>" + "\r\n"
                            + "      <AggregationDesign>" + "\r\n"
                            + "        <ID>" + _currentAggD.ID + "</ID>" + "\r\n"
                            + "        <Name>" + _currentAggD.Name + "</Name>" + "\r\n"
                            + "        <Aggregations>" + "\r\n"
                            + "        </Aggregations>" + "\r\n"
                            + "      </AggregationDesign>" + "\r\n"
                            + "    </ObjectDefinition>" + "\r\n"
                            + "  </Alter>" + "\r\n"
                            + "</Batch>" + "\r\n";
                        ServerExecute(s, sXMLA);

                        RaiseProgressEvent(5 + (int)(87.0 * i / dictAggRowCount.Count / _totalIterations), "Processing empty aggregation design...");

                        if (_cancelled) return;

                        //process aggs to delete existing aggs
                        ServerExecute(s, "<Batch xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">" + "\r\n"
                            + "  <Parallel>" + "\r\n"
                            + "    <Process xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ddl2=\"http://schemas.microsoft.com/analysisservices/2003/engine/2\" xmlns:ddl2_2=\"http://schemas.microsoft.com/analysisservices/2003/engine/2/2\" xmlns:ddl100_100=\"http://schemas.microsoft.com/analysisservices/2008/engine/100/100\">" + "\r\n"
                            + "      <Object>" + "\r\n"
                            + "       <DatabaseID>" + _currentAggD.ParentDatabase.ID + "</DatabaseID>" + "\r\n"
                            + "       <CubeID>" + _currentAggD.ParentCube.ID + "</CubeID>" + "\r\n"
                            + "       <MeasureGroupID>" + _currentAggD.Parent.ID + "</MeasureGroupID>" + "\r\n"
                            + "      </Object>" + "\r\n"
                            + "      <Type>ProcessIndexes</Type>" + "\r\n"
                            + "      <WriteBackTableCreation>UseExisting</WriteBackTableCreation>" + "\r\n"
                            + "    </Process>" + "\r\n"
                            + "  </Parallel>" + "\r\n"
                            + "</Batch>" + "\r\n");
                        if (!string.IsNullOrEmpty(_errors)) throw new Exception(_errors);

                        if (_cancelled) return;

                        foreach (AggregationPerformance aggP in _listAggPerf)
                        {
                            RaiseProgressEvent(10 + (int)(87.0 * i++ / dictAggRowCount.Count / _totalIterations), "Testing performance with no aggs " + ((i - 1) % dictAggRowCount.Count + 1) + " of " + dictAggRowCount.Count + " (" + aggP.AggregationName + ")...");

                            ServerExecute(s, "<ClearCache xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\">" + "\r\n"
                            + "    <Object>" + "\r\n"
                            + "      <DatabaseID>" + _currentAggD.ParentDatabase.ID + "</DatabaseID>" + "\r\n"
                            + "      <CubeID>" + _currentAggD.ParentCube.ID + "</CubeID>" + "\r\n"
                            + "    </Object>" + "\r\n"
                            + "  </ClearCache>");

                            if (_cancelled) return;

                            _queryEnded = false;

                            //initialize the MDX script with a no-op query
                            ServerExecuteMDX(db, "with member [Measures].[_Exec MDX Script_] as null select [Measures].[_Exec MDX Script_] on 0 from [" + _currentAggD.ParentCube.Name.Replace("]", "]]") + "]", _sessionID);

                            while (!this._queryEnded) //wait for session trace query end event
                            {
                                if (_cancelled) return;
                                System.Threading.Thread.Sleep(100);
                            }

                            aggP.ScriptPerformanceWithoutAggs = _queryDuration;

                            _queryEnded = false;

                            ServerExecuteMDX(db, aggP.PerformanceTestMDX, _sessionID);

                            while (!this._queryEnded) //wait for session trace query end event
                            {
                                if (_cancelled) return;
                                System.Threading.Thread.Sleep(100);
                            }

                            aggP.QueryPerformanceWithoutAggs = _queryDuration;
                        }
                    } //end of testing with no aggs



                    RaiseProgressEvent(100, "Finished measure group " + _currentAggD.Parent.Name);

                    if (!string.IsNullOrEmpty(_errors)) throw new Exception(_errors);
                }
                finally
                {
                    try
                    {
                        if (!s.Connected)
                        {
                            s.Connect("Data Source=" + _currentAggD.ParentServer.Name, _sessionID);
                        }
                    }
                    catch
                    {
                        try
                        {
                            if (!s.Connected)
                            {
                                s.Connect("Data Source=" + _currentAggD.ParentServer.Name); //can't connect to that session, so just reconnect
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        s.RollbackTransaction();
                    }
                    catch { }

                    try
                    {
                        _trc.Drop();
                    }
                    catch { }

                    try
                    {
                        s.Disconnect();
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                if (!_cancelled)
                {
                    _errors += ex.Message + "\r\n" + ex.StackTrace + "\r\n";
                    System.Windows.Forms.MessageBox.Show(_errors);
                }
            }
        }

        //unprocess partitions that use another (or no) agg design
        private void UnprocessOtherPartitions(Server s)
        {
            string sUnprocessOtherPartitionsXMLA = "";
            foreach (Partition p in _currentAggD.Parent.Partitions)
            {
                if (p.AggregationDesignID != _currentAggD.ID)
                {
                    sUnprocessOtherPartitionsXMLA += ""
                        + "    <Process xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ddl2=\"http://schemas.microsoft.com/analysisservices/2003/engine/2\" xmlns:ddl2_2=\"http://schemas.microsoft.com/analysisservices/2003/engine/2/2\" xmlns:ddl100_100=\"http://schemas.microsoft.com/analysisservices/2008/engine/100/100\">" + "\r\n"
                        + "      <Object>" + "\r\n"
                        + "       <DatabaseID>" + _currentAggD.ParentDatabase.ID + "</DatabaseID>" + "\r\n"
                        + "       <CubeID>" + _currentAggD.ParentCube.ID + "</CubeID>" + "\r\n"
                        + "       <MeasureGroupID>" + _currentAggD.Parent.ID + "</MeasureGroupID>" + "\r\n"
                        + "       <PartitionID>" + p.ID + "</PartitionID>" + "\r\n"
                        + "      </Object>" + "\r\n"
                        + "      <Type>ProcessClear</Type>" + "\r\n"
                        + "      <WriteBackTableCreation>UseExisting</WriteBackTableCreation>" + "\r\n"
                        + "    </Process>" + "\r\n";
                }
            }
            if (sUnprocessOtherPartitionsXMLA.Length > 0)
            {
                s.Execute("<Batch xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\" xmlns:as=\"http://schemas.microsoft.com/analysisservices/2003/engine\" xmlns:dwd=\"http://schemas.microsoft.com/DataWarehouse/Designer/1.0\">" + "\r\n"
                    + " <Parallel>"
                    + sUnprocessOtherPartitionsXMLA
                    + " </Parallel>"
                    + "</Batch>");
            }
        }

        public event System.ComponentModel.ProgressChangedEventHandler OnProgress;

        private void RaiseProgressEvent(int iProgress, string sStatus)
        {
            OnProgress(this, new System.ComponentModel.ProgressChangedEventArgs(iProgress, sStatus));
        }

        public string Errors
        {
            get { return _errors; }
        }

        public bool Cancelled
        {
            get { return _cancelled; }
        }

        private void ServerExecuteMDX(Database db, string MDX, string sSessionID)
        {
            System.Xml.XmlReader reader = db.Parent.SendXmlaRequest(XmlaRequestType.Execute, new System.IO.StringReader(
                "<Envelope xmlns=\"http://schemas.xmlsoap.org/soap/envelope/\">" + "\r\n"
                + "  <Header>" + "\r\n"
                + "    <Session xmlns=\"urn:schemas-microsoft-com:xml-analysis\" SessionId=\"" + sSessionID + "\" />" + "\r\n"
                + "  </Header>" + "\r\n"
                + "  <Body>" + "\r\n"
                + "    <Execute xmlns=\"urn:schemas-microsoft-com:xml-analysis\">" + "\r\n"
                + "      <Command>" + "\r\n"
                + "        <Statement>" + "\r\n"
                + "          " + XMLEncode(MDX) + "\r\n"
                + "        </Statement>" + "\r\n"
                + "      </Command>" + "\r\n"
                + "      <Properties>" + "\r\n"
                + "        <PropertyList>" + "\r\n"
                + "          <Catalog>" + db.Name + "</Catalog>" + "\r\n"
                + "          <Format>Tabular</Format>" + "\r\n"
                + "        </PropertyList>" + "\r\n"
                + "      </Properties>" + "\r\n"
                + "    </Execute>" + "\r\n"
                + "  </Body>" + "\r\n"
                + "</Envelope>"));

            if (reader.ReadToFollowing("faultstring"))
            {
                string sFault = reader.ReadElementContentAsString();
                _errors += sFault + "\r\n";
            }

            reader.Close();
        }

        private void ServerExecute(Server server, string command)
        {
            XmlaResultCollection results = server.Execute(command);
            if (results != null)
            {
                foreach (XmlaResult result in results)
                {
                    foreach (XmlaMessage message in result.Messages)
                    {
                        if (message is XmlaError)
                        {
                            _errors += message.Description + "\r\n";
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(_errors)) throw new Exception(_errors);
        }

        private static string XMLEncode(string sString)
        {
            System.Text.StringBuilder sTmp = new System.Text.StringBuilder(sString);
            sTmp.Replace("&", "&amp;");
            sTmp.Replace("\"", "&quot;");
            sTmp.Replace("'", "&apos;");
            sTmp.Replace("<", "&lt;");
            sTmp.Replace(">", "&gt;");
            return sTmp.ToString();
        }

        private static bool IsAttributeRelated(DimensionAttribute attr, DimensionAttribute key)
        {
            foreach (AttributeRelationship atrel in key.AttributeRelationships)
                if (key.AttributeRelationships.Contains(attr.ID) || IsAttributeRelated(attr, atrel.Attribute))
                    return true;
            return false;
        }
        
        private void trace_Stopped(ITrace sender, TraceStoppedEventArgs e)
        {
            if (e.Exception != null && !string.IsNullOrEmpty(e.Exception.Message))
            {
                _errors += e.StopCause.ToString() + " - " + e.Exception.Message + " - " + e.Exception.StackTrace + "\r\n";
            }
        }

        private void trace_OnEvent(object sender, TraceEventArgs e)
        {
            try
            {
                if (e.EventClass == TraceEventClass.GetDataFromAggregation)
                {
                    if (!e.ObjectPath.StartsWith(_currentMGPath)) return;
                    if (e.SessionID != _sessionID && e.ConnectionID != "0") return; //has to either be this session or the admin session (under which ExecuteMdxScript runs)
                    string sPartitionID = e.ObjectPath.Substring(_currentMGPath.Length);
                    Partition partition = _currentAggD.Parent.Partitions.Find(sPartitionID);
                    if (partition == null) return;

                    string sAggID = e.TextData.Split(new char[] { '\r', '\n' })[0];
                    if (partition.AggregationDesign == null) return;
                    Aggregation agg = partition.AggregationDesign.Aggregations.Find(sAggID);
                    if (agg == null) return;

                    lock (_dictHitAggs)
                    {
                        if (_dictHitAggs.ContainsKey(agg))
                        {
                            _dictHitAggs[agg]++;
                        }
                        else
                        {
                            _dictHitAggs.Add(agg, 1);
                        }
                    }
                }
                else if (e.EventClass == TraceEventClass.QueryEnd)
                {
                    if (e.SessionID != _sessionID) return;
                    _queryEnded = true;
                    _queryDuration = e.Duration;
                }
            }
            catch { } //ignore errors
        }

        public class AggregationPerformance
        {
            private Aggregation _agg;
            private long _aggRowCount;
            private long _partitionRowCount;
            private long _queryPerformanceWithAgg;
            private long? _queryPerformanceWithoutAggs;
            private long _scriptPerformanceWithAgg;
            private long? _scriptPerformanceWithoutAggs;
            private bool _hitAggregation = false;
            private long _measureGroupRowCount;
            private Dictionary<Aggregation, int> _dictAggHits = new Dictionary<Aggregation, int>();
            private string _cachedPerformanceTestMDX;
            private long? _processIndexesDuration;

            public AggregationPerformance(Aggregation a)
            {
                _agg = a;
            }

            public Aggregation Aggregation
            {
                get { return _agg; }
            }

            public long AggregationRowCount
            {
                get { return _aggRowCount; }
                set { _aggRowCount = value; }
            }

            /// <summary>
            /// The rowcount of the fact-level data for all partitions in this agg design
            /// </summary>
            public long PartitionRowCount
            {
                get { return _partitionRowCount; }
                set { _partitionRowCount = value; }
            }

            public long MeasureGroupRowCount
            {
                get { return _measureGroupRowCount; }
                set { _measureGroupRowCount = value; }
            }

            public long QueryPerformanceWithAgg
            {
                get { return _queryPerformanceWithAgg; }
                set { _queryPerformanceWithAgg = value; }
            }

            public long? QueryPerformanceWithoutAggs
            {
                get { return _queryPerformanceWithoutAggs; }
                set { _queryPerformanceWithoutAggs = value; }
            }

            public long ScriptPerformanceWithAgg
            {
                get { return _scriptPerformanceWithAgg; }
                set { _scriptPerformanceWithAgg = value; }
            }

            public long? ScriptPerformanceWithoutAggs
            {
                get { return _scriptPerformanceWithoutAggs; }
                set { _scriptPerformanceWithoutAggs = value; }
            }

            public long? ProcessIndexesDuration
            {
                get { return _processIndexesDuration; }
                set { _processIndexesDuration = value; }
            }

            public Dictionary<Aggregation, int> AggHits
            {
                get { return _dictAggHits; }
                set { _dictAggHits = value; }
            }

            public string DatabaseName
            {
                get { return _agg.ParentDatabase.Name; }
            }

            public string CubeName
            {
                get { return _agg.ParentCube.Name; }
            }

            public string MeasureGroupName
            {
                get { return _agg.ParentMeasureGroup.Name; }
            }

            public string AggregationDesignName
            {
                get { return _agg.Parent.Name; }
            }

            public string AggregationName
            {
                get { return _agg.Name; }
            }

            public double AggregationApproximateSize
            {
                get
                {
                    long iMeasureBytes = 0;
                    foreach (Microsoft.AnalysisServices.Measure m in _agg.ParentMeasureGroup.Measures)
                    {
                        if (m.DataType == MeasureDataType.Inherited)
                        {
                            if (m.Source.DataSize > 0)
                                iMeasureBytes += m.Source.DataSize;
                            else if (m.Source.DataType == System.Data.OleDb.OleDbType.Integer)
                                iMeasureBytes += 4;
                            else if (m.Source.DataType == System.Data.OleDb.OleDbType.SmallInt)
                                iMeasureBytes += 2;
                            else if (m.Source.DataType == System.Data.OleDb.OleDbType.TinyInt)
                                iMeasureBytes += 1;
                            else
                                iMeasureBytes += 8;
                        }
                        else
                        {
                            if (m.DataType == MeasureDataType.Integer)
                                iMeasureBytes += 4;
                            else if (m.DataType == MeasureDataType.SmallInt)
                                iMeasureBytes += 2;
                            else if (m.DataType == MeasureDataType.TinyInt)
                                iMeasureBytes += 1;
                            else
                                iMeasureBytes += 8;
                        }
                    }

                    int iNumSurrogateKeysInAgg = 0;
                    foreach (AggregationDimension aggDim in _agg.Dimensions)
                    {
                        foreach (AggregationAttribute aggAttr in aggDim.Attributes)
                        {
                            iNumSurrogateKeysInAgg++;
                        }
                    }

                    //the size of each row is 4 bytes for each surrogate key plus the size of measures
                    long lngFactTableRowSize = (_agg.ParentMeasureGroup.Dimensions.Count * 4 + iMeasureBytes);
                    long lngAggRowSize = (iNumSurrogateKeysInAgg * 4 + iMeasureBytes);

                    //multiply the estimated rows by the size of each row
                    return ((double)(_aggRowCount * lngAggRowSize)) / ((double)(_partitionRowCount * lngFactTableRowSize));
                }
            }

            public bool HitAggregation
            {
                get { return _hitAggregation; }
                set { _hitAggregation = value; }
            }

            public string PerformanceTestMDX
            {
                get
                {
                    if (_cachedPerformanceTestMDX != null) return _cachedPerformanceTestMDX;

                    string MDX = "";
                    foreach (AggregationDimension aggDim in _agg.Dimensions)
                    {
                        foreach (AggregationAttribute aggAttr in aggDim.Attributes)
                        {
                            if (!string.IsNullOrEmpty(MDX)) MDX += "  *";
                            MDX += "  [" + aggAttr.CubeAttribute.Parent.Name + "].[" + aggAttr.CubeAttribute.Attribute.Name + "].[" + aggAttr.CubeAttribute.Attribute.Name + "].Members\r\n";
                        }

                        //look through all DefaultMembers and add any to the query that will change the results
                        foreach (MeasureGroupAttribute mga in AggregationPerformanceTester._dictDefaultMembers.Keys)
                        {
                            if (mga.CubeAttribute.Parent.ID != aggDim.CubeDimensionID) continue; //only add attributes that are in this dimension... this keeps dimensions together so that crossjoins are less expensive

                            bool bInclude = true;
                            foreach (AggregationAttribute aggAttr in aggDim.Attributes)
                            {
                                if (aggAttr.CubeAttribute.Parent.ID == mga.CubeAttribute.Parent.ID)
                                {
                                    if (aggAttr.AttributeID == mga.AttributeID)
                                        bInclude = false;
                                    else if (AggregationPerformanceTester.IsAttributeRelated(mga.Attribute, aggAttr.Attribute))
                                        bInclude = false;
                                }
                            }
                            if (bInclude && mga.Attribute.IsAggregatable)
                            {
                                if (!string.IsNullOrEmpty(MDX)) MDX += "  *";
                                MDX += "  {" + AggregationPerformanceTester._dictDefaultMembers[mga] + ".Parent}\r\n";
                            }
                        }
                    }

                    //pick up the rest of the DefaultMembers that we didn't catch above
                    foreach (MeasureGroupAttribute mga in AggregationPerformanceTester._dictDefaultMembers.Keys)
                    {
                        if (!_agg.Dimensions.Contains(mga.CubeAttribute.Parent.ID) && mga.Attribute.IsAggregatable)
                        {
                            if (!string.IsNullOrEmpty(MDX)) MDX += "  *";
                            MDX += "  {" + AggregationPerformanceTester._dictDefaultMembers[mga] + ".Parent}\r\n";
                        }
                    }

                    bool bVisiblePhysicalMeasures = false;
                    foreach (Microsoft.AnalysisServices.Measure m in _agg.ParentMeasureGroup.Measures)
                    {
                        if (m.Visible)
                        {
                            bVisiblePhysicalMeasures = true;
                            break;
                        }
                    }

                    //deal with aggregations at the all level
                    if (string.IsNullOrEmpty(MDX))
                    {
                        MDX = "Root()";
                    }

                    if (bVisiblePhysicalMeasures)
                    {
                        //use Exists-with-a-measure-group to avoid the calc script and to hit above date granularity aggs in a semi-additive measure group
                        _cachedPerformanceTestMDX = "select {} on columns,\r\n"
                            + "Head(\r\n"
                            + " Exists(\r\n"
                            + MDX
                            + "  ,\r\n"
                            + "  ,\"" + _agg.ParentMeasureGroup.Name.Replace("\"", "\"\"") + "\"\r\n"
                            + " )\r\n"
                            + " ,1\r\n"
                            + ")\r\n"
                            + "on rows\r\n"
                            + "from [" + _agg.ParentCube.Name.Replace("]", "]]") + "]";
                    }
                    else
                    {
                        //if there are no visible physical measures, then Exists-with-a-measure-group won't work
                        _cachedPerformanceTestMDX = "select {} on columns,\r\n"
                            + "Head(\r\n"
                            + " NonEmpty(\r\n"
                            + MDX
                            + "  ,MeasureGroupMeasures(\"" + _agg.ParentMeasureGroup.Name.Replace("\"", "\"\"") + "\")\r\n"
                            + " )\r\n"
                            + " ,1\r\n"
                            + ")\r\n"
                            + "on rows\r\n"
                            + "from [" + _agg.ParentCube.Name.Replace("]", "]]") + "]";
                    }
                    return _cachedPerformanceTestMDX;
                }
            }
        }

        public class MissingAggregationPerformance
        {
            private AggregationPerformance _aggP;
            private Aggregation[] _missingAggregation;
            private long _scriptPerformance;
            private long _queryPerformance;
            private Dictionary<Aggregation, int> _aggHits;

            public MissingAggregationPerformance(AggregationPerformance aggP, Aggregation[] missingAggregations, Dictionary<Aggregation, int> aggHits)
            {
                _aggP = aggP;
                _missingAggregation = missingAggregations;
                _aggHits = aggHits;
            }

            public string MeasureGroupName
            {
                get { return _aggP.Aggregation.ParentMeasureGroup.Name; }
            }

            public string AggregationDesignName
            {
                get { return _aggP.Aggregation.Parent.Name; }
            }

            public string AggregationName
            {
                get { return _aggP.Aggregation.Name; }
            }

            public long ScriptPerformance
            {
                get { return _scriptPerformance; }
                set { _scriptPerformance = value; }
            }

            public long QueryPerformance
            {
                get { return _queryPerformance; }
                set { _queryPerformance = value; }
            }

            public long ScriptPerformanceDiff
            {
                get { return _scriptPerformance - _aggP.ScriptPerformanceWithAgg; }
            }

            public long QueryPerformanceDiff
            {
                get { return _queryPerformance - _aggP.QueryPerformanceWithAgg; }
            }

            public long? ProcessIndexesDuration
            {
                get { return _aggP.ProcessIndexesDuration; }
            }

            public string[] MissingAggregations
            {
                get
                {
                    List<string> _aggs = new List<string>();
                    foreach (Aggregation a in _missingAggregation)
                    {
                        _aggs.Add(a.Name);
                    }
                    return _aggs.ToArray();
                }
            }

            public Aggregation[] AggHitsDiff
            {
                get
                {
                    List<Aggregation> _diff = new List<Aggregation>();
                    foreach (Aggregation a in _aggHits.Keys)
                    {
                        if (!_aggP.AggHits.ContainsKey(a))
                        {
                            _diff.Add(a);
                        }
                    }
                    return _diff.ToArray();
                }
            }

            public string[] AggHitsDiffNames
            {
                get
                {
                    List<string> _diff = new List<string>();
                    foreach (Aggregation a in AggHitsDiff)
                    {
                        _diff.Add(a.Name);
                    }
                    return _diff.ToArray();
                }
            }
        }
    }
}
