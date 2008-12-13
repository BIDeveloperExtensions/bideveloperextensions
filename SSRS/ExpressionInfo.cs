namespace BIDSHelper.SSRS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    internal class ExpressionInfo
    {
        [NonSerialized]
        private DataAggregateInfoList m_aggregates;
        private bool m_boolValue;
        [NonSerialized]
        private int m_compileTimeID;
        [NonSerialized]
        private bool m_dynamicFieldReferences;
        private int m_exprHostID;
        private int m_intValue;
        private string m_originalText;
        [NonSerialized]
        private StringList m_referencedDataSets;
        [NonSerialized]
        private StringList m_referencedDataSources;
        [NonSerialized]
        private Hashtable m_referencedFieldProperties;
        [NonSerialized]
        private StringList m_referencedFields;
        [NonSerialized]
        private StringList m_referencedParameters;
        [NonSerialized]
        private StringList m_referencedReportItems;
        [NonSerialized]
        private RunningValueInfoList m_runningValues;
        private string m_stringValue;
        [NonSerialized]
        private string m_transformedExpression;
        [NonSerialized]
        private StringList m_transformedExpressionAggregateIDs;
        [NonSerialized]
        private IntList m_transformedExpressionAggregatePositions;
        private Types m_type;

        internal ExpressionInfo()
        {
            this.m_exprHostID = -1;
            this.m_compileTimeID = -1;
        }

        internal ExpressionInfo(Types type)
        {
            this.m_exprHostID = -1;
            this.m_compileTimeID = -1;
            this.m_type = Types.Expression;
        }

        internal void AddAggregate(DataAggregateInfo aggregate)
        {
            if (this.m_aggregates == null)
            {
                this.m_aggregates = new DataAggregateInfoList();
            }
            this.m_aggregates.Add(aggregate);
        }

        internal void AddDynamicPropertyReference(string fieldName)
        {
            System.Diagnostics.Debug.Assert(null != fieldName);
            if (this.m_referencedFieldProperties == null)
            {
                this.m_referencedFieldProperties = new Hashtable();
            }
            else if (this.m_referencedFieldProperties.ContainsKey(fieldName))
            {
                this.m_referencedFieldProperties.Remove(fieldName);
            }
            this.m_referencedFieldProperties.Add(fieldName, null);
        }

        internal void AddReferencedDataSet(string dataSetName)
        {
            if (this.m_referencedDataSets == null)
            {
                this.m_referencedDataSets = new StringList();
            }
            this.m_referencedDataSets.Add(dataSetName);
        }

        internal void AddReferencedDataSource(string dataSourceName)
        {
            if (this.m_referencedDataSources == null)
            {
                this.m_referencedDataSources = new StringList();
            }
            this.m_referencedDataSources.Add(dataSourceName);
        }

        internal void AddReferencedField(string fieldName)
        {
            if (this.m_referencedFields == null)
            {
                this.m_referencedFields = new StringList();
            }
            this.m_referencedFields.Add(fieldName);
        }

        internal void AddReferencedParameter(string parameterName)
        {
            if (this.m_referencedParameters == null)
            {
                this.m_referencedParameters = new StringList();
            }
            this.m_referencedParameters.Add(parameterName);
        }

        internal void AddReferencedReportItem(string reportItemName)
        {
            if (this.m_referencedReportItems == null)
            {
                this.m_referencedReportItems = new StringList();
            }
            this.m_referencedReportItems.Add(reportItemName);
        }

        internal void AddRunningValue(RunningValueInfo runningValue)
        {
            if (this.m_runningValues == null)
            {
                this.m_runningValues = new RunningValueInfoList();
            }
            this.m_runningValues.Add(runningValue);
        }

        internal void AddStaticPropertyReference(string fieldName, string propertyName)
        {
            System.Diagnostics.Debug.Assert((fieldName != null) && (null != propertyName));
            if (this.m_referencedFieldProperties == null)
            {
                this.m_referencedFieldProperties = new Hashtable();
            }
            if (this.m_referencedFieldProperties.ContainsKey(fieldName))
            {
                Hashtable hashtable = this.m_referencedFieldProperties[fieldName] as Hashtable;
                if (hashtable != null)
                {
                    hashtable[propertyName] = null;
                }
            }
            else
            {
                Hashtable hashtable2 = new Hashtable();
                hashtable2.Add(propertyName, null);
                this.m_referencedFieldProperties.Add(fieldName, hashtable2);
            }
        }

        internal DataAggregateInfo GetSumAggregateWithoutScope()
        {
            if ((Types.Aggregate == this.m_type) && (this.m_aggregates != null))
            {
                string str;
                System.Diagnostics.Debug.Assert(1 == this.m_aggregates.Count);
                DataAggregateInfo info = this.m_aggregates[0];
                if ((DataAggregateInfo.AggregateTypes.Sum == info.AggregateType) && !info.GetScope(out str))
                {
                    return info;
                }
            }
            return null;
        }

        internal bool HasRecursiveAggregates()
        {
            if (this.m_aggregates != null)
            {
                for (int i = 0; i < this.m_aggregates.Count; i++)
                {
                    if (this.m_aggregates[i].Recursive)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal DataAggregateInfoList Aggregates
        {
            get
            {
                return this.m_aggregates;
            }
        }

        internal bool BoolValue
        {
            get
            {
                return this.m_boolValue;
            }
            set
            {
                this.m_boolValue = value;
            }
        }

        internal int CompileTimeID
        {
            get
            {
                return this.m_compileTimeID;
            }
            set
            {
                this.m_compileTimeID = value;
            }
        }

        internal bool DynamicFieldReferences
        {
            get
            {
                return this.m_dynamicFieldReferences;
            }
            set
            {
                this.m_dynamicFieldReferences = value;
            }
        }

        internal int ExprHostID
        {
            get
            {
                return this.m_exprHostID;
            }
            set
            {
                this.m_exprHostID = value;
            }
        }

        internal int IntValue
        {
            get
            {
                return this.m_intValue;
            }
            set
            {
                this.m_intValue = value;
            }
        }

        internal string OriginalText
        {
            get
            {
                return this.m_originalText;
            }
            set
            {
                this.m_originalText = value;
            }
        }

        internal Hashtable ReferencedFieldProperties
        {
            get
            {
                return this.m_referencedFieldProperties;
            }
        }

        internal StringList ReferencedDataSets
        {
            get
            {
                return this.m_referencedDataSets;
            }
        }

        internal RunningValueInfoList RunningValues
        {
            get
            {
                return this.m_runningValues;
            }
        }

        internal string TransformedExpression
        {
            get
            {
                return this.m_transformedExpression;
            }
            set
            {
                this.m_transformedExpression = value;
            }
        }

        internal StringList TransformedExpressionAggregateIDs
        {
            get
            {
                return this.m_transformedExpressionAggregateIDs;
            }
            set
            {
                this.m_transformedExpressionAggregateIDs = value;
            }
        }

        internal IntList TransformedExpressionAggregatePositions
        {
            get
            {
                return this.m_transformedExpressionAggregatePositions;
            }
            set
            {
                this.m_transformedExpressionAggregatePositions = value;
            }
        }

        internal Types Type
        {
            get
            {
                return this.m_type;
            }
            set
            {
                this.m_type = value;
            }
        }

        internal string Value
        {
            get
            {
                return this.m_stringValue;
            }
            set
            {
                this.m_stringValue = value;
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
        private AggregateTypes m_aggregateType;
        private StringList m_duplicateNames;
        private ExpressionInfo[] m_expressions;
        [NonSerialized]
        private bool m_exprHostInitialized;
        [NonSerialized]
        private List<string> m_fieldsUsedInValueExpression;
        [NonSerialized]
        private bool m_hasScope;
        [NonSerialized]
        private bool m_isCopied;
        private string m_name;
        [NonSerialized]
        private bool m_recursive;
        [NonSerialized]
        private string m_scope;

        internal string Scope
        {
            get { return m_scope; }
            set { m_scope = value; }
        }

        internal bool GetScope(out string scope)
        {
            scope = this.m_scope;
            return this.m_hasScope;
        }

        internal bool IsPostSortAggregate()
        {
            if (((this.m_aggregateType != AggregateTypes.First) && (AggregateTypes.Last != this.m_aggregateType)) && (AggregateTypes.Previous != this.m_aggregateType))
            {
                return false;
            }
            return true;
        }

        internal void SetScope(string scope)
        {
            this.m_hasScope = true;
            this.m_scope = scope;
        }

        internal AggregateTypes AggregateType
        {
            get
            {
                return this.m_aggregateType;
            }
            set
            {
                this.m_aggregateType = value;
            }
        }

        internal StringList DuplicateNames
        {
            get
            {
                return this.m_duplicateNames;
            }
            set
            {
                this.m_duplicateNames = value;
            }
        }

        internal ExpressionInfo[] Expressions
        {
            get
            {
                return this.m_expressions;
            }
            set
            {
                this.m_expressions = value;
            }
        }

        internal string ExpressionText
        {
            get
            {
                if ((this.m_expressions != null) && (1 == this.m_expressions.Length))
                {
                    return this.m_expressions[0].OriginalText;
                }
                return string.Empty;
            }
        }

        internal bool ExprHostInitialized
        {
            get
            {
                return this.m_exprHostInitialized;
            }
            set
            {
                this.m_exprHostInitialized = value;
            }
        }

        internal List<string> FieldsUsedInValueExpression
        {
            get
            {
                return this.m_fieldsUsedInValueExpression;
            }
            set
            {
                this.m_fieldsUsedInValueExpression = value;
            }
        }

        internal bool IsCopied
        {
            get
            {
                return this.m_isCopied;
            }
            set
            {
                this.m_isCopied = value;
            }
        }

        internal string Name
        {
            get
            {
                return this.m_name;
            }
            set
            {
                this.m_name = value;
            }
        }

        internal bool Recursive
        {
            get
            {
                return this.m_recursive;
            }
            set
            {
                this.m_recursive = value;
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

}
