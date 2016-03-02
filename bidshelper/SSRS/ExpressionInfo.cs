namespace BIDSHelper.SSRS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    internal class ExpressionInfo
    {
        object m_SSRSExpressionInfo;
        public ExpressionInfo(object SSRSExpressionInfo)
        {
            m_SSRSExpressionInfo = SSRSExpressionInfo;
        }

        internal DataAggregateInfoList Aggregates
        {
            get
            {
                IList arr = (IList)m_SSRSExpressionInfo.GetType().InvokeMember("Aggregates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, m_SSRSExpressionInfo, null);
                if (arr == null) return null;
                DataAggregateInfoList l = new DataAggregateInfoList();
                foreach (object o in arr)
                {
                    l.Add(new DataAggregateInfo(o));
                }
                return l; 
            }
        }

        internal bool DynamicFieldReferences
        {
            get
            {
                return (bool)m_SSRSExpressionInfo.GetType().InvokeMember("DynamicFieldReferences", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, m_SSRSExpressionInfo, null);
            }
        }

        internal Hashtable ReferencedFieldProperties
        {
            get
            {
                return (Hashtable)m_SSRSExpressionInfo.GetType().InvokeMember("ReferencedFieldProperties", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, m_SSRSExpressionInfo, null);
            }
        }

#if YUKON
        internal ArrayList ReferencedDataSets
        {
            get
            {
                return (ArrayList)m_SSRSExpressionInfo.GetType().InvokeMember("m_referencedDataSets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic, null, m_SSRSExpressionInfo, null);
            }
        }
#else
        internal List<string> ReferencedDataSets
        {
            get
            {
                return (List<string>)m_SSRSExpressionInfo.GetType().InvokeMember("m_referencedDataSets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic, null, m_SSRSExpressionInfo, null);
            }
        }
#endif

        internal RunningValueInfoList RunningValues
        {
            get
            {
                IList arr = (IList)m_SSRSExpressionInfo.GetType().InvokeMember("RunningValues", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, m_SSRSExpressionInfo, null);
                if (arr == null) return null;
                RunningValueInfoList l = new RunningValueInfoList();
                foreach (object o in arr)
                {
                    l.Add(new RunningValueInfo(o));
                }
                return l; 
            }
        }

        internal string[] DataSetsFromLookups
        {
            get
            {
#if !YUKON
                bool bHasLookupsProperty = false;
                foreach (System.Reflection.PropertyInfo propInfo in m_SSRSExpressionInfo.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic))
                {
                    if (propInfo.Name == "Lookups")
                    {
                        bHasLookupsProperty = true;
                        break;
                    }
                }
                if (!bHasLookupsProperty) return null;

                IList arr = (IList)m_SSRSExpressionInfo.GetType().InvokeMember("Lookups", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, m_SSRSExpressionInfo, null);
                if (arr == null) return null;

                List<string> listDataSets = new List<string>();
                foreach (object lookup in arr)
                {
                    object oDestinationExpressionInfo = lookup.GetType().InvokeMember("DestinationInfo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, lookup, null);
                    DataAggregateInfo agg = new DataAggregateInfo(oDestinationExpressionInfo); //not really the right type for this wrapper, but good enough to get the Scope out
                    listDataSets.Add(agg.Scope);
                }
                return listDataSets.ToArray();
#else
                return null;
#endif
            }
        }

        internal Types Type
        {
            get
            {
                return (Types)m_SSRSExpressionInfo.GetType().InvokeMember("Type", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, m_SSRSExpressionInfo, null);
            }

        }

        internal enum Types
        {
            Expression,
            Field,
            Aggregate,
            Constant,
            Token
        }

        [Flags]
        internal enum LocationFlags
        {
            InDataRegion = 4,
            InDataSet = 2,
            InDetail = 16,
            InGrouping = 8,
            InMatrixCell = 32,
            InMatrixCellTopLevelItem = 256,
            InMatrixGroupHeader = 1024,
            InMatrixOrTable = 512,
            InMatrixSubtotal = 128,
            InPageSection = 64,
            None = 1
        }

        public enum ObjectType
        {
            Report,
            PageHeader,
            PageFooter,
            Line,
            Rectangle,
            Checkbox,
            Textbox,
            Image,
            Subreport,
            ActiveXControl,
            List,
            Matrix,
            Table,
            OWCChart,
            Chart,
            Grouping,
            ReportParameter,
            DataSource,
            DataSet,
            Field,
            Query,
            QueryParameter,
            EmbeddedImage,
            ReportItem,
            Subtotal,
            CodeClass,
            CustomReportItem
        }

        internal enum ExpressionType
        {
            General,
            ReportParameter,
            ReportLanguage,
            QueryParameter,
            GroupExpression,
            SortExpression,
            DataSetFilters,
            DataRegionFilters,
            GroupingFilters,
            FieldValue
        }
    }

    [Serializable]
    internal sealed class DataAggregateInfoList : ArrayList
    {
        internal DataAggregateInfoList()
        {
        }

        internal DataAggregateInfoList(int capacity)
            : base(capacity)
        {
        }

        internal new DataAggregateInfo this[int index]
        {
            get
            {
                return (DataAggregateInfo)base[index];
            }
        }
    }

    [Serializable]
    internal class DataAggregateInfo
    {
        private object m_SSRSObject;
        public DataAggregateInfo(object SSRSObject)
        {
            m_SSRSObject = SSRSObject;
        }

        internal string Scope
        {
            get
            {
                try
                {
                    //RunningValueInfo has a Scope property, but DataAggregateInfo does not... use the Scope property when available, and fallback to GetScope otherwise since it's not accurate in some situations
                    return (string)m_SSRSObject.GetType().InvokeMember("Scope", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, m_SSRSObject, null);
                }
                catch
                {
                    object[] p = new object[] { null };
                    bool b = (bool)m_SSRSObject.GetType().InvokeMember("GetScope", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic, null, m_SSRSObject, p);
                    return (string)p[0];
                }
            }
        }

        internal ExpressionInfo[] Expressions
        {
            get
            {
                object[] arr = (object[])m_SSRSObject.GetType().InvokeMember("Expressions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, m_SSRSObject, null);
                List<ExpressionInfo> list = new List<ExpressionInfo>();
                foreach (object o in arr)
                {
                    ExpressionInfo i = new ExpressionInfo(o);
                    list.Add(i);
                }
                return list.ToArray();
            }
        }

        internal enum AggregateTypes
        {
            First,
            Last,
            Sum,
            Avg,
            Max,
            Min,
            CountDistinct,
            CountRows,
            Count,
            StDev,
            Var,
            StDevP,
            VarP,
            Aggregate,
            Previous
        }
    }

    [Serializable]
    internal sealed class StringList : ArrayList
    {
        internal StringList()
        {
        }

        internal StringList(int capacity)
            : base(capacity)
        {
        }

        internal new string this[int index]
        {
            get
            {
                return (string)base[index];
            }
            set
            {
                base[index] = value;
            }
        }
    }

    [Serializable]
    internal sealed class IntList : ArrayList
    {
        internal IntList()
        {
        }

        internal IntList(int capacity)
            : base(capacity)
        {
        }

        internal void CopyTo(IntList target)
        {
            if (target != null)
            {
                target.Clear();
                for (int i = 0; i < this.Count; i++)
                {
                    target.Add(this[i]);
                }
            }
        }

        internal new int this[int index]
        {
            get
            {
                return (int)base[index];
            }
            set
            {
                base[index] = value;
            }
        }
    }

    [Serializable]
    internal sealed class RunningValueInfoList : ArrayList
    {
        internal RunningValueInfoList()
        {
        }

        internal RunningValueInfoList(int capacity)
            : base(capacity)
        {
        }

        internal new RunningValueInfo this[int index]
        {
            get
            {
                return (RunningValueInfo)base[index];
            }
        }
    }

    [Serializable]
    internal sealed class RunningValueInfo : DataAggregateInfo
    {
        public RunningValueInfo(object SSRSObject)
            : base(SSRSObject)
        {
        }
    }

}
