using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.Text.RegularExpressions;

namespace BIDSHelper.SSIS.PerformanceVisualization
{
    public class DtsPerformanceLogEventParser : IDTSLogging
    {
        public bool PackageEndReceived = false;
        public List<string> Errors = new List<string>();

        private Package _Package;
        private DtsObjectPerformance Performance;
        private Dictionary<string, DtsObjectPerformance> listComponentsPerformanceLookup = new Dictionary<string, DtsObjectPerformance>(StringComparer.CurrentCultureIgnoreCase);

        private Regex regexBufferSizeTuning = new Regex(@"buffer type (\d+) .+ (\d+) rows in buffers of this type", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Regex regexCreateBuffer = new Regex(@"CreatePrimeBuffer of type (\d+) for output ID (\d+)");

#if DENALI
        private Regex regexExecutionTreeOutput = new Regex(@"\s+(.+?)\.Outputs\[(.+?)\]\;");
#else
        private Regex regexExecutionTreeOutput = new Regex(@"output \"".+?\"" \((\d+)\)");
#endif

#if KATMAI || DENALI
        private const string EXECUTION_TREE_START_PHRASE = "Begin Path ";
        private const string EXECUTION_TREE_END_PHRASE = "End Path ";
#else
        private const string EXECUTION_TREE_START_PHRASE = "begin execution tree ";
        private const string EXECUTION_TREE_END_PHRASE = "end execution tree ";
#endif

        public DtsPerformanceLogEventParser(Package package)
        {
            _Package = package;

            Performance = new DtsObjectPerformance(package.ID, package.Name, package.GetType(), 0);
            listComponentsPerformanceLookup.Add(package.ID, Performance);
            RecurseComponentsAndBuildPerformanceObjects(_Package, this.Performance, 0);
        }

        private void RecurseComponentsAndBuildPerformanceObjects(IDTSSequence container, DtsObjectPerformance performance, int indent)
        {
            foreach (Executable exe in container.Executables)
            {
                if (exe is DtsContainer)
                {
                    DtsContainer child = (DtsContainer)exe;
                    DtsObjectPerformance childPerformance;
                    if (exe is TaskHost)
                    {
                        TaskHost task = (TaskHost)exe;
                        MainPipe pipeline = task.InnerObject as MainPipe;
                        if (pipeline != null)
                        {
                            childPerformance = new DtsPipelinePerformance(child.ID, child.Name, pipeline, indent + 1);
                        }
                        else
                        {
                            childPerformance = new DtsObjectPerformance(child.ID, child.Name, child.GetType(), indent + 1);
                        }
                        listComponentsPerformanceLookup.Add(child.ID, childPerformance);
                    }
                    else
                    {
                        childPerformance = new DtsObjectPerformance(child.ID, child.Name, child.GetType(), indent + 1);
                        listComponentsPerformanceLookup.Add(child.ID, childPerformance);
                    }
                    performance.Children.Add(childPerformance);
                    if (exe is IDTSSequence)
                    {
                        RecurseComponentsAndBuildPerformanceObjects((IDTSSequence)exe, childPerformance, indent + 1);
                    }
                }
            }
        }

        public void LoadEvent(DtsLogEvent e)
        {
            if (!listComponentsPerformanceLookup.ContainsKey(e.SourceId)) return;
            DtsObjectPerformance perf = listComponentsPerformanceLookup[e.SourceId];
            if (e.Event == BidsHelperCapturedDtsLogEvent.OnPipelineRowsSent)
            {
                //<ignore> : <ignore> : PathID : PathName : TransformID : TransformName : InputID : InputName : RowsSent
                //Denali includes the following at the end... DestinationTransformName : Paths[SourceName.SourceOutputName] : DestinationTransformName.Inputs[InputName]
                DtsPipelinePerformance pipePerf = (DtsPipelinePerformance)perf;
                string[] parts = e.Message.Split(new string[] { " : " }, StringSplitOptions.None);
                int iPathID = int.Parse(parts[2]);
                int iInputID = int.Parse(parts[6]);
                if (pipePerf.InputOutputLookup.ContainsKey(iInputID))
                {
                    PipelinePath path = pipePerf.InputOutputLookup[iInputID];
                    path.DateRanges.Add(new DateRange(e.StartTime, e.EndTime));
                    path.RowCount += int.Parse(parts[8]);
                    path.BufferCount++;
                    
                }
            }
            else if (e.Event == BidsHelperCapturedDtsLogEvent.OnPipelinePrePrimeOutput)
            {
                //PrimeOutput will be called on a component. : 28490 : Flat File Source 1
                //<ignore> : TransformID : TransformName
                DtsPipelinePerformance pipePerf = (DtsPipelinePerformance)perf;
                string[] parts = e.Message.Split(new string[] { " : " }, StringSplitOptions.None);
                int iTransformID = int.Parse(parts[1]);
                foreach (ExecutionTree tree in pipePerf.ExecutionTrees)
                {
                    if (tree.Paths.Count > 0)
                    {
                        PipelinePath path = tree.Paths[0]; //only look for the component on the output of the first path
                        if (path.OutputTransformID == iTransformID)
                        {
                            tree.DateRanges.Add(new DateRange(e.StartTime));
                            //break; //there could be multiple execution trees started by the same component?
                        }
                    }
                }
            }
            else if (e.Event == BidsHelperCapturedDtsLogEvent.OnPipelinePostEndOfRowset)
            {
                //A component has finished processing all of its rows. : 30341 : Multicast : 30342 : Multicast Input 1
                //<ignore> : TransformID : TransformName : InputID : InputName
                DtsPipelinePerformance pipePerf = (DtsPipelinePerformance)perf;
                string[] parts = e.Message.Split(new string[] { " : " }, StringSplitOptions.None);
                int iInputID = int.Parse(parts[3]);

                foreach (ExecutionTree tree in pipePerf.ExecutionTrees)
                {
                    foreach (PipelinePath path in tree.Paths)
                    {
                        if (path.InputID == iInputID)
                        {
                            if (tree.DateRanges.Count > 0) //this is to avoid an error on issue 31275 where OnPipelinePrePrimeOutput wasn't ever called so we didn't start a date range
                            {
                                DateRange lastDateRange = tree.DateRanges[tree.DateRanges.Count - 1];
                                if (lastDateRange.EndDate == DateTime.MinValue)
                                {
                                    lastDateRange.EndDate = e.EndTime;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (e.Event == BidsHelperCapturedDtsLogEvent.OnPreExecute)
            {
                perf.DateRanges.Add(new DateRange(e.StartTime));
            }
            else if (e.Event == BidsHelperCapturedDtsLogEvent.OnPostExecute)
            {
                if (perf.DateRanges.Count > 0)
                {
                    DateRange lastDateRange = perf.DateRanges[perf.DateRanges.Count - 1];
                    if (lastDateRange.EndDate == DateTime.MinValue)
                    {
                        lastDateRange.EndDate = e.EndTime;
                    }
                }
            }
            else if (e.Event == BidsHelperCapturedDtsLogEvent.PipelineExecutionTrees)
            {
                DtsPipelinePerformance pipePerf = (DtsPipelinePerformance)perf;
                ExecutionTree tree = null;
                foreach (string line in e.Message.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.StartsWith(EXECUTION_TREE_START_PHRASE))
                    {
                        int iTreeID = int.Parse(line.Substring(EXECUTION_TREE_START_PHRASE.Length));
                        tree = new ExecutionTree();
                        tree.ID = iTreeID;
                        tree.UniqueId = pipePerf.ID + "ExecutionTree" + iTreeID;
                        tree.Name = "Execution Tree " + iTreeID;

                        bool bFoundTree = false;
                        foreach (ExecutionTree t in pipePerf.ExecutionTrees)
                        {
                            if (t.ID == tree.ID)
                            {
                                bFoundTree = true;
                                break;
                            }
                        }
                        if (!bFoundTree)
                            pipePerf.ExecutionTrees.Add(tree);
                    }
                    else if (line.StartsWith(EXECUTION_TREE_END_PHRASE))
                    {
                        //skip line
                    }
                    else
                    {
                        Match match = regexExecutionTreeOutput.Match(line);

#if DENALI
                        if (match.Groups.Count == 3)
                        {
                            string sComponent = match.Groups[1].Value;
                            string sOutput = match.Groups[2].Value;
                            foreach (PipelinePath path in pipePerf.InputOutputLookup.Values)
                            {
                                if (path.OutputName == sOutput && path.OutputTransformName == sComponent && !tree.Paths.Contains(path))
                                {
                                    tree.Paths.Add(path);
                                }
                            }
                        }
#else
                        if (match.Groups.Count == 2)
                        {
                            int iOutputID = int.Parse(match.Groups[1].Value);
                            if (pipePerf.InputOutputLookup.ContainsKey(iOutputID) && !tree.Paths.Contains(pipePerf.InputOutputLookup[iOutputID]))
                                tree.Paths.Add(pipePerf.InputOutputLookup[iOutputID]);
                        }
#endif
                    }
                }
            }
            else if (e.Event == BidsHelperCapturedDtsLogEvent.PipelineExecutionPlan)
            {
                DtsPipelinePerformance pipePerf = (DtsPipelinePerformance)perf;
                foreach (string line in e.Message.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Match match = regexCreateBuffer.Match(line);
                    if (match.Groups.Count == 3)
                    {
                        int iBufferID = int.Parse(match.Groups[1].Value) - 1;
                        int iBufferRowCount = pipePerf.GetBufferRowCount(iBufferID);
                        int iOutputID = int.Parse(match.Groups[2].Value);
                        foreach (ExecutionTree tree in pipePerf.ExecutionTrees)
                        {
                            foreach (PipelinePath path in tree.Paths)
                            {
                                if (path.OutputID == iOutputID)
                                {
                                    tree.BufferRowCount = iBufferRowCount;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (e.Event == BidsHelperCapturedDtsLogEvent.BufferSizeTuning)
            {
                DtsPipelinePerformance pipePerf = (DtsPipelinePerformance)perf;
                Match match = regexBufferSizeTuning.Match(e.Message);
                if (match.Groups.Count == 3)
                {
                    int iBufferID = int.Parse(match.Groups[1].Value);
                    int iBufferRowCount = int.Parse(match.Groups[2].Value);
                    pipePerf.SetBufferRowCount(iBufferID, iBufferRowCount);
                }
            }
            else if (e.Event == BidsHelperCapturedDtsLogEvent.OnError)
            {
                perf.IsError = true;
                this.Errors.Add(e.Message);
            }
            else if (e.Event == BidsHelperCapturedDtsLogEvent.PackageEnd)
            {
                this.PackageEndReceived = true;
            }
        }

        public List<IDtsGridRowData> GetAllDtsGanttGridRowDatas()
        {
            return Performance.GetAllDtsGanttGridRowDatas();
        }

        #region IDTSLogging Members
        //this code is currently not in use as SSIS logging goes to a text file
        //but this code is left in here in case we need to execute a package in process and handle log events in the future
        //we don't want to run the package in process since we want to have the ability to run as 64-bit
        //these methods allow capturing events through custom SSIS logging handler class
        //the log events are then passed to the rest of the class for being parsed

        public bool Enabled
        {
            get { return true; }
        }

        public bool[] GetFilterStatus(ref string[] eventNames)
        {
            throw new NotImplementedException();
        }

        private List<string> listMonitoredEventNames = new List<string>(System.Enum.GetNames(typeof(BidsHelperCapturedDtsLogEvent)));

        public void Log(string eventName, string computerName, string operatorName, string sourceName, string sourceGuid, string executionGuid, string messageText, DateTime startTime, DateTime endTime, int dataCode, ref byte[] dataBytes)
        {
            if (listMonitoredEventNames.Contains(eventName))
            {
                DtsLogEvent e = new DtsLogEvent();
                e.Event = (BidsHelperCapturedDtsLogEvent)System.Enum.Parse(typeof(BidsHelperCapturedDtsLogEvent), eventName);
                e.SourceName = sourceName;
                e.SourceId = sourceGuid;
                e.StartTime = startTime;
                e.EndTime = endTime;
                e.Message = messageText;
                this.LoadEvent(e);
            }
        }

        #endregion

    }

    public class DtsLogEvent
    {
        public BidsHelperCapturedDtsLogEvent Event;
        public string SourceName;
        public string SourceId;
        public DateTime StartTime;
        public DateTime EndTime;
        public string Message;
    }

    public enum BidsHelperCapturedDtsLogEvent
    {
        PackageStart,
        PackageEnd,
        OnPreExecute,
        OnPostExecute,
        OnError,
        OnPipelinePrePrimeOutput,
        OnPipelinePostEndOfRowset,
        BufferSizeTuning,
        OnPipelineRowsSent,
        PipelineExecutionTrees,
        PipelineExecutionPlan
    }
}
