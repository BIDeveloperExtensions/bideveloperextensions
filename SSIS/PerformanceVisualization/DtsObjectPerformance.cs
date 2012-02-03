using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

#region Conditional compile for Yukon vs Katmai
#if KATMAI || DENALI
using IDTSComponentMetaDataXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData100;
using IDTSPathXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPath100;
using IDTSOutputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn100;
#else
using IDTSComponentMetaDataXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData90;
using IDTSPathXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPath90;
using IDTSOutputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn90;
#endif
#endregion

namespace BIDSHelper.SSIS.PerformanceVisualization
{

    public class DtsObjectPerformance : IDtsGridRowData
    {
        public DtsObjectPerformance(string ID, string Name, Type Type, int Indent)
        {
            this.ID = ID;
            this.Name = Name;
            this.Type = Type;
            this.Indent = Indent;
        }

        public string ID;

        public string UniqueId
        {
            get { return ID; }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private Type _Type;
        public virtual Type Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        private int _Indent;
        public int Indent
        {
            get { return _Indent; }
            set { _Indent = value; }
        }

        private bool _IsError;
        public bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; }
        }

        private List<DateRange> _DateRanges = new List<DateRange>();
        public List<DateRange> DateRanges
        {
            get { return _DateRanges; }
        }

        public virtual bool HasChildren
        {
            get { return (Children.Count > 0); }
        }

        public List<DtsObjectPerformance> Children = new List<DtsObjectPerformance>();

        public virtual List<IDtsGridRowData> GetAllDtsGanttGridRowDatas()
        {
            List<IDtsGridRowData> list = new List<IDtsGridRowData>();
            list.Add(this);
            foreach (DtsObjectPerformance p in Children)
            {
                list.AddRange(p.GetAllDtsGanttGridRowDatas());
            }
            return list;
        }

        public int? TotalSeconds
        {
            get
            {
                bool bHasRange = false;
                TimeSpan ts = new TimeSpan();
                foreach (DateRange range in _DateRanges)
                {
                    if (range.EndDate > DateTime.MinValue)
                    {
                        bHasRange = true;
                        ts = ts.Add(range.EndDate.Subtract(range.StartDate));
                    }
                }
                if (!bHasRange) return null;
                return (int?)ts.TotalSeconds;
            }
        }

        public virtual double? InboundRowsSec
        {
            get { return null; }
        }

        public virtual double? OutboundRowsSec
        {
            get { return null; }
        }

        public int? BufferRowCount
        {
            get { return null; }
        }

        public int? BufferEstimatedBytesPerRow
        {
            get { return null; }
        }

        public virtual double? InboundKbSec
        {
            get { return null; }
        }

        public virtual double? OutboundKbSec
        {
            get { return null; }
        }
    }

    public class DtsPipelinePerformance : DtsObjectPerformance
    {
        private Dictionary<int, DtsObjectPerformance> listTransformsPerformanceLookup = new Dictionary<int, DtsObjectPerformance>();
        private Dictionary<int, int> listBufferSizes = new Dictionary<int, int>();
        private int DefaultBufferMaxRows;

        public List<ExecutionTree> ExecutionTrees = new List<ExecutionTree>();
        public Dictionary<int, PipelinePath> InputOutputLookup = new Dictionary<int, PipelinePath>();

        public DtsPipelinePerformance(string ID, string Name, MainPipe pipeline, int Indent)
            : base(ID, Name, typeof(MainPipe), Indent)
        {
            DefaultBufferMaxRows = pipeline.DefaultBufferMaxRows;
            foreach (IDTSComponentMetaDataXX component in pipeline.ComponentMetaDataCollection)
            {
                DtsObjectPerformance perf = new DtsObjectPerformance(component.ID.ToString(), component.Name, component.GetType(), Indent + 1);
                Children.Add(perf);
                listTransformsPerformanceLookup.Add(component.ID, perf);
            }
            foreach (IDTSPathXX path in pipeline.PathCollection)
            {
                PipelinePath p = new PipelinePath();
                p.ID = path.ID;
                p.UniqueId = ID + "Path" + path.ID;
                p.OutputID = path.StartPoint.ID;
                p.OutputName = path.StartPoint.Name;
                p.OutputTransformID = path.StartPoint.Component.ID;
                p.OutputTransformName = path.StartPoint.Component.Name;
                p.OutputTransformType = path.StartPoint.Component.ObjectType;
                p.InputID = path.EndPoint.ID;
                p.InputName = path.EndPoint.Name;
                p.InputTransformID = path.EndPoint.Component.ID;
                p.InputTransformName = path.EndPoint.Component.Name;
                p.InputTransformType = path.EndPoint.Component.ObjectType;
                p.Indent = this.Indent + 2; //TODO: indent the paths recursively? probably good enough for now
                InputOutputLookup.Add(p.InputID, p);
                InputOutputLookup.Add(p.OutputID, p);

                foreach (IDTSOutputColumnXX col in path.StartPoint.OutputColumnCollection)
                {
                    if (!p.dictLineageIDColumnSizes.ContainsKey(col.LineageID))
                        p.dictLineageIDColumnSizes.Add(col.LineageID, GetColumnSizeInBytes(col));
                }
            }

            //hook up PreviousPath
            foreach (IDTSPathXX path in pipeline.PathCollection)
            {
                if (path.StartPoint.SynchronousInputID != 0)
                {
                    PipelinePath p = InputOutputLookup[path.StartPoint.ID];
                    p.PreviousPath = InputOutputLookup[path.StartPoint.SynchronousInputID];
                }
            }
        }

        public static int GetColumnSizeInBytes(IDTSOutputColumnXX col)
        {
            //this is based on a quick interpretation of http://msdn2.microsoft.com/en-us/library/ms141036.aspx and is not guaranteed to be exactly accurate
            switch (col.DataType)
            {
                case DataType.DT_BOOL:
                case DataType.DT_I1:
                case DataType.DT_UI1:
                    return 1;
                case DataType.DT_GUID:
                case DataType.DT_I2:
                case DataType.DT_UI2:
                    return 2;
                case DataType.DT_I4:
                case DataType.DT_UI4:
                case DataType.DT_R4:
                    return 4;
                case DataType.DT_CY:
                case DataType.DT_DATE:
                case DataType.DT_DBDATE:
                case DataType.DT_DBTIME:
                case DataType.DT_DBTIMESTAMP:
                case DataType.DT_FILETIME:
                case DataType.DT_I8:
                case DataType.DT_UI8:
                case DataType.DT_R8:
                    return 8;
                case DataType.DT_DECIMAL:
                    return 12;
                case DataType.DT_NUMERIC:
                    return 16;
                case DataType.DT_BYTES:
                case DataType.DT_STR:
                    return col.Length + 1; //null terminated
                case DataType.DT_WSTR:
                    return col.Length * 2 + 1; //null terminated
                case DataType.DT_IMAGE:
                case DataType.DT_TEXT:
                case DataType.DT_NTEXT:
                    return 24; //don't know length based on metadata so we guess... SSIS buffer tuning may use the same guess???
#if DENALI || KATMAI
                case DataType.DT_DBTIME2:
                case DataType.DT_DBTIMESTAMP2:
                    return 8;
                case DataType.DT_DBTIMESTAMPOFFSET:
                    return 10;
#endif
                default:
                    return 4;
            }
        }

        public void SetBufferRowCount(int BufferID, int BufferRowCount)
        {
            if (listBufferSizes.ContainsKey(BufferID))
                listBufferSizes[BufferID] = BufferRowCount;
            else
                listBufferSizes.Add(BufferID, BufferRowCount);
        }

        public int GetBufferRowCount(int BufferID)
        {
            if (listBufferSizes.ContainsKey(BufferID))
                return listBufferSizes[BufferID];
            else
                return DefaultBufferMaxRows;
        }

        public override double? InboundRowsSec
        {
            get
            {
                long lRows = 0;
                long? lTotalSeconds = null;
                foreach (ExecutionTree tree in this.ExecutionTrees)
                {
                    if (tree.TotalSeconds != null)
                    {
                        if (lTotalSeconds == null) lTotalSeconds = 0;
                        lTotalSeconds += (long)tree.TotalSeconds;
                        foreach (PipelinePath path in tree.Paths)
                        {
                            if ((path.OutputTransformType & DTSObjectType.OT_SOURCEADAPTER) == DTSObjectType.OT_SOURCEADAPTER && path.RowCount != null)
                            {
                                lRows += (long)path.RowCount;
                            }
                        }
                    }
                }
                if (lTotalSeconds == null) return null;
                if (lTotalSeconds == 0) return lRows;
                return ((double)lRows) / lTotalSeconds;
            }
        }

        public override double? OutboundRowsSec
        {
            get
            {
                long lRows = 0;
                long? lTotalSeconds = null;
                foreach (ExecutionTree tree in this.ExecutionTrees)
                {
                    if (tree.TotalSeconds != null)
                    {
                        if (lTotalSeconds == null) lTotalSeconds = 0;
                        lTotalSeconds += (long)tree.TotalSeconds;
                        foreach (PipelinePath path in tree.Paths)
                        {
                            if ((path.InputTransformType & DTSObjectType.OT_DESTINATIONADAPTER) == DTSObjectType.OT_DESTINATIONADAPTER && path.RowCount != null)
                            {
                                lRows += (long)path.RowCount;
                            }
                        }
                    }
                }
                if (lTotalSeconds == null) return null;
                if (lTotalSeconds == 0) return lRows;
                return ((double)lRows) / lTotalSeconds;
            }
        }

        public override double? InboundKbSec
        {
            get
            {
                int iCnt = 0;
                double dblTotal = 0;
                foreach (ExecutionTree tree in this.ExecutionTrees)
                {
                    double? dblInboundKbSec = tree.InboundKbSec;
                    if (dblInboundKbSec != null)
                    {
                        dblTotal += (double)dblInboundKbSec;
                        iCnt++;
                    }
                }
                if (iCnt > 0)
                    return dblTotal;
                else
                    return null;
            }
        }

        public override double? OutboundKbSec
        {
            get
            {
                int iCnt = 0;
                double dblTotal = 0;
                foreach (ExecutionTree tree in this.ExecutionTrees)
                {
                    double? dblOutboundKbSec = tree.OutboundRowsSec;
                    if (dblOutboundKbSec != null)
                    {
                        dblTotal += (double)dblOutboundKbSec;
                        iCnt++;
                    }
                }
                if (iCnt > 0)
                    return dblTotal;
                else
                    return null;
            }
        }

        public override bool HasChildren
        {
            get { return (ExecutionTrees.Count > 0); }
        }

        public override Type Type
        {
            get { return typeof(DtsPipelinePerformance); }
        }

        public override List<IDtsGridRowData> GetAllDtsGanttGridRowDatas()
        {
            List<IDtsGridRowData> list = new List<IDtsGridRowData>();
            list.Add(this);
            foreach (ExecutionTree tree in ExecutionTrees)
            {
                tree.Indent = this.Indent + 1;
                list.Add(tree);
                foreach (PipelinePath path in tree.Paths)
                {
                    list.Add(path);
                }
            }
            return list;
        }
    }

    public class ExecutionTree : IDtsGridRowData
    {
        public int ID;

        private string _UniqueId;
        public string UniqueId
        {
            get { return _UniqueId; }
            set { _UniqueId = value; }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public Type Type
        {
            get { return this.GetType(); }
        }

        private int _Indent;
        public int Indent
        {
            get { return _Indent; }
            set { _Indent = value; }
        }

        public bool IsError
        {
            get { return false; }
        }

        private List<DateRange> _DateRanges = new List<DateRange>();
        public List<DateRange> DateRanges
        {
            get
            {
                return _DateRanges;
            }
        }

        public bool HasChildren
        {
            get { return (Paths.Count > 0); }
        }

        public double? InboundRowsSec
        {
            get
            {
                long lRows = 0;
                foreach (PipelinePath path in Paths)
                {
                    if (path.PreviousPath == null && path.RowCount != null)
                    {
                        lRows += (long)path.RowCount;
                    }
                }
                if (this.TotalSeconds == 0) return lRows;
                return ((double)lRows) / this.TotalSeconds;
            }
        }

        public double? OutboundRowsSec
        {
            get
            {
                long lRows = 0;
                foreach (PipelinePath path in Paths)
                {
                    bool bIsEndOfExecutionTree = true;
                    foreach (PipelinePath path2 in Paths)
                    {
                        if (path2.PreviousPath == path)
                        {
                            bIsEndOfExecutionTree = false;
                            break;
                        }
                    }
                    if (bIsEndOfExecutionTree && path.RowCount != null)
                    {
                        lRows += (long)path.RowCount;
                    }
                }
                if (this.TotalSeconds == 0) return lRows;
                return ((double)lRows) / this.TotalSeconds;
            }
        }

        private int? _BufferRowCount;
        public int? BufferRowCount
        {
            get { return _BufferRowCount; }
            set { _BufferRowCount = value; }
        }

        public int? BufferEstimatedBytesPerRow
        {
            get
            {
                //sum up the byte count of each distinct LineageID... may overstate the buffer row's byte count in some scenarios, but should be pretty close
                long lBytes = 4; //is there overhead per row?
                List<int> listLineageIDs = new List<int>();
                foreach (PipelinePath path in Paths)
                {
                    foreach (int iLineageID in path.dictLineageIDColumnSizes.Keys)
                    {
                        if (!listLineageIDs.Contains(iLineageID))
                        {
                            listLineageIDs.Add(iLineageID);
                            lBytes += path.dictLineageIDColumnSizes[iLineageID] + 2; //extra bytes per column notes whether it's nullable???
                        }
                    }
                }
                if (lBytes < int.MaxValue) //just be sure we don't cause an overflow error
                    return (int)lBytes;
                else
                    return int.MaxValue;
            }
        }

        public double? InboundKbSec
        {
            get
            {
                double? dblInboundRowsSec = InboundRowsSec;
                if (dblInboundRowsSec != null)
                    return dblInboundRowsSec * BufferEstimatedBytesPerRow / 1024;
                else
                    return null;
            }
        }

        public double? OutboundKbSec
        {
            get
            {
                double? dblOutboundRowsSec = OutboundRowsSec;
                if (dblOutboundRowsSec != null)
                    return dblOutboundRowsSec * BufferEstimatedBytesPerRow / 1024;
                else
                    return null;
            }
        }

        public List<PipelinePath> Paths = new List<PipelinePath>();

        public int? TotalSeconds
        {
            get
            {
                bool bHasRange = false;
                TimeSpan ts = new TimeSpan();
                foreach (DateRange range in _DateRanges)
                {
                    if (range.EndDate > DateTime.MinValue)
                    {
                        bHasRange = true;
                        ts = ts.Add(range.EndDate.Subtract(range.StartDate));
                    }
                }
                if (!bHasRange) return null;
                return (int?)ts.TotalSeconds;
            }
        }
    }

    public class PipelinePath : IDtsGridRowData
    {
        public int ID;
        public int OutputID;
        public string OutputName;
        public int OutputTransformID;
        public string OutputTransformName;
        public int InputID;
        public string InputName;
        public int InputTransformID;
        public string InputTransformName;
        public PipelinePath PreviousPath;
        public DTSObjectType OutputTransformType;
        public DTSObjectType InputTransformType;
        internal Dictionary<int, int> dictLineageIDColumnSizes = new Dictionary<int, int>();


        private string _UniqueId;
        public string UniqueId
        {
            get { return _UniqueId; }
            set { _UniqueId = value; }
        }

        public string Name
        {
            get { return OutputName + " --> " + InputName; }
        }

        private long _RowCount;
        public long? RowCount
        {
            get { return _RowCount; }
            set { _RowCount = (long)value; }
        }

        private long _BufferCount;
        public long? BufferCount
        {
            get { return _BufferCount; }
            set { _BufferCount = (long)value; }
        }

        public bool IsError
        {
            get { return false; }
        }

        private List<DateRange> _DateRanges = new List<DateRange>();
        public List<DateRange> DateRanges
        {
            get { return _DateRanges; }
        }

        public int? TotalSeconds
        {
            get { return null; }
        }

        public int? BufferRowCount
        {
            get { return null; }
        }

        public int? BufferEstimatedBytesPerRow
        {
            get { return null; }
        }

        public double? InboundRowsSec
        {
            get { return null; }
        }

        public double? OutboundRowsSec
        {
            get { return null; }
        }

        public double? InboundKbSec
        {
            get { return null; }
        }

        public double? OutboundKbSec
        {
            get { return null; }
        }

        public Type Type
        {
            get { return this.GetType(); }
        }

        private int _Indent;
        public int Indent
        {
            get { return _Indent; }
            set { _Indent = value; }
        }

        public bool HasChildren
        {
            get { return false; }
        }
    }

    public interface IDtsGridRowData
    {
        string UniqueId { get; }
        string Name { get; }
        int Indent { get; }
        bool HasChildren { get; }
        List<DateRange> DateRanges { get; }
        int? TotalSeconds { get; }
        int? BufferRowCount { get; }
        int? BufferEstimatedBytesPerRow { get; }
        double? InboundRowsSec { get; }
        double? OutboundRowsSec { get; }
        double? InboundKbSec { get; }
        double? OutboundKbSec { get; }
        Type Type { get; }
        bool IsError { get; }
    }

    public class DateRange
    {
        public DateRange(DateTime StartDate)
        {
            this.StartDate = StartDate;
        }
        public DateRange(DateTime StartDate, DateTime EndDate)
        {
            this.StartDate = StartDate;
            this.EndDate = EndDate;
        }
        public DateTime StartDate;
        public DateTime EndDate;
    }
}
