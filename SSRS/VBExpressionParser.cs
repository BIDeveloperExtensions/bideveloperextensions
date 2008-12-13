namespace BIDSHelper.SSRS
{
    using System;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class VBExpressionParser : ExpressionParser
    {
        private bool m_bodyRefersToReportItems;
        private ExpressionParser.ExpressionContext m_context;
        private int m_numberOfAggregates;
        private int m_numberOfRunningValues;
        private bool m_pageSectionRefersToReportItems;
        private ReportRegularExpressions m_regexes;
        private const string Previous = "Previous";
        private const string RowNumber = "RowNumber";
        private const string RunningValue = "RunningValue";
        private const string Star = "*";

        internal VBExpressionParser()
        {
            this.m_regexes = ReportRegularExpressions.Value;
            this.m_numberOfAggregates = 0;
            this.m_numberOfRunningValues = 0;
            this.m_bodyRefersToReportItems = false;
            this.m_pageSectionRefersToReportItems = false;
        }

        internal override void ConvertField2ComplexExpr(ref ExpressionInfo info)
        {
            System.Diagnostics.Debug.Assert(info.Type == ExpressionInfo.Types.Field);
            info.Type = ExpressionInfo.Types.Expression;
            info.TransformedExpression = "Fields!" + info.Value + ".Value";
        }

        private string CreateAggregateID()
        {
            return ("Aggregate" + (this.m_numberOfAggregates + this.m_numberOfRunningValues));
        }

        private bool Detected(string expression, Regex detectionRegex)
        {
            return (this.NumberOfTimesDetected(expression, detectionRegex) != 0);
        }

        private bool DetectUserReference(string expression)
        {
            return this.Detected(expression, this.m_regexes.UserDetection);
        }

        private void EnforceRestrictions(ref string expression, bool isParameter, ExpressionParser.GrammarFlags grammarFlags)
        {
            int num = this.NumberOfTimesDetected(expression, this.m_regexes.ReportItemsDetection);
            if (0 < num)
            {
                if ((this.m_context.Location & LocationFlags.InPageSection) != 0)
                {
                    this.m_pageSectionRefersToReportItems = true;
                }
                else
                {
                    this.m_bodyRefersToReportItems = true;
                }
            }
            this.RemoveLineTerminators(ref expression, this.m_regexes.LineTerminatorDetection);
        }

        private void GetAggregate(int currentPos, string functionName, string expression, bool isParameter, ExpressionParser.GrammarFlags grammarFlags, out int newPos, out DataAggregateInfo aggregate)
        {
            StringList list;
            this.GetArguments(currentPos, expression, out newPos, out list);
            aggregate = new DataAggregateInfo();
            aggregate.AggregateType = (DataAggregateInfo.AggregateTypes)Enum.Parse(typeof(DataAggregateInfo.AggregateTypes), functionName, true);
            if (DataAggregateInfo.AggregateTypes.CountRows == aggregate.AggregateType)
            {
                aggregate.AggregateType = DataAggregateInfo.AggregateTypes.CountRows;
                aggregate.Expressions = new ExpressionInfo[0];
                if (1 == list.Count)
                {
                    aggregate.SetScope(this.GetScope(list[0], false));
                }
                else if (2 == list.Count)
                {
                    aggregate.Recursive = this.IsRecursive(list[1]);
                }
            }
            else
            {
                if (1 <= list.Count)
                {
                    if ((DataAggregateInfo.AggregateTypes.Count == aggregate.AggregateType) && ("*" == list[0].Trim()))
                    {
                    }
                    else
                    {
                        aggregate.Expressions = new ExpressionInfo[] { this.GetParameterExpression(list[0], grammarFlags) };
                    }
                }
                if (2 <= list.Count)
                {
                    aggregate.SetScope(this.GetScope(list[1], false));
                }
                if (3 <= list.Count)
                {
                    if (aggregate.IsPostSortAggregate() || (DataAggregateInfo.AggregateTypes.Aggregate == aggregate.AggregateType))
                    {
                    }
                    else
                    {
                        aggregate.Recursive = this.IsRecursive(list[2]);
                    }
                }
            }
            this.m_numberOfAggregates++;
        }

        private void GetArguments(int currentPos, string expression, out int newPos, out StringList arguments)
        {
            int num = 1;
            arguments = new StringList();
            string str = string.Empty;
            while ((0 < num) && (currentPos < expression.Length))
            {
                Match match = this.m_regexes.Arguments.Match(expression, currentPos);
                if (!match.Success)
                {
                    str = str + expression.Substring(currentPos);
                    currentPos = expression.Length;
                }
                else
                {
                    string str2 = match.Result("${openParen}");
                    string str3 = match.Result("${closeParen}");
                    string str4 = match.Result("${comma}");
                    if ((str2 != null) && (str2.Length != 0))
                    {
                        num++;
                        str = str + expression.Substring(currentPos, (match.Index - currentPos) + match.Length);
                    }
                    else if ((str3 != null) && (str3.Length != 0))
                    {
                        num--;
                        if (num == 0)
                        {
                            str = str + expression.Substring(currentPos, match.Index - currentPos);
                            if (str.Trim().Length != 0)
                            {
                                arguments.Add(str);
                                str = string.Empty;
                            }
                        }
                        else
                        {
                            str = str + expression.Substring(currentPos, (match.Index - currentPos) + match.Length);
                        }
                    }
                    else if ((str4 != null) && (str4.Length != 0))
                    {
                        if (1 == num)
                        {
                            str = str + expression.Substring(currentPos, match.Index - currentPos);
                            arguments.Add(str);
                            str = string.Empty;
                        }
                        else
                        {
                            str = str + expression.Substring(currentPos, (match.Index - currentPos) + match.Length);
                        }
                    }
                    else
                    {
                        str = str + expression.Substring(currentPos, (match.Index - currentPos) + match.Length);
                    }
                    currentPos = match.Index + match.Length;
                }
            }
            if (num > 0)
            {
                if (str.Trim().Length != 0)
                {
                    arguments.Add(str);
                    str = string.Empty;
                }
            }
            newPos = currentPos;
        }

        private ExpressionInfo GetParameterExpression(string parameterExpression, ExpressionParser.GrammarFlags grammarFlags)
        {
            ExpressionInfo expressionInfo = m_context.ParseExtended ? new ExpressionInfoExtended() : new ExpressionInfo(); //BUG FIX OVER SSRS CODE
            expressionInfo.OriginalText = parameterExpression;
            if ((this.m_context.Location & LocationFlags.InPageSection) != 0)
            {
                grammarFlags |= ExpressionParser.GrammarFlags.DenyPrevious | ExpressionParser.GrammarFlags.DenyRowNumber | ExpressionParser.GrammarFlags.DenyRunningValue | ExpressionParser.GrammarFlags.DenyAggregates;
            }
            else
            {
                grammarFlags |= ExpressionParser.GrammarFlags.DenyPrevious | ExpressionParser.GrammarFlags.DenyReportItems | ExpressionParser.GrammarFlags.DenyRowNumber | ExpressionParser.GrammarFlags.DenyRunningValue | ExpressionParser.GrammarFlags.DenyAggregates;
            }
            this.VBLex(parameterExpression, true, grammarFlags, expressionInfo);
            return expressionInfo;
        }

        private void GetPreviousAggregate(int currentPos, string functionName, string expression, bool isParameter, ExpressionParser.GrammarFlags grammarFlags, out int newPos, out RunningValueInfo runningValue)
        {
            StringList list;
            this.GetArguments(currentPos, expression, out newPos, out list);
            runningValue = new RunningValueInfo();
            runningValue.AggregateType = DataAggregateInfo.AggregateTypes.Previous;
            if (1 <= list.Count)
            {
                runningValue.Expressions = new ExpressionInfo[] { this.GetParameterExpression(list[0], grammarFlags) };
            }
            this.m_numberOfRunningValues++;
        }

        private void GetReferencedDataSetNames(string expression, ExpressionInfo expressionInfo)
        {
            MatchCollection matchs = this.m_regexes.DataSetName.Matches(expression);
            for (int i = 0; i < matchs.Count; i++)
            {
                string dataSetName = matchs[i].Result("${datasetname}");
                if ((dataSetName != null) && (dataSetName.Length != 0))
                {
                    expressionInfo.AddReferencedDataSet(dataSetName);
                }
            }
        }

        private void GetReferencedDataSourceNames(string expression, ExpressionInfo expressionInfo)
        {
            MatchCollection matchs = this.m_regexes.DataSourceName.Matches(expression);
            for (int i = 0; i < matchs.Count; i++)
            {
                string dataSourceName = matchs[i].Result("${datasourcename}");
                if ((dataSourceName != null) && (dataSourceName.Length != 0))
                {
                    expressionInfo.AddReferencedDataSource(dataSourceName);
                }
            }
        }

        private void GetReferencedFieldNames(string expression, ExpressionInfo expressionInfo)
        {
            if (this.Detected(expression, this.m_regexes.DynamicFieldReference))
            {
                expressionInfo.DynamicFieldReferences = true;
            }
            else
            {
                MatchCollection matchs = this.m_regexes.DynamicFieldPropertyReference.Matches(expression);
                for (int j = 0; j < matchs.Count; j++)
                {
                    string fieldName = matchs[j].Result("${fieldname}");
                    if ((fieldName != null) && (fieldName.Length != 0))
                    {
                        expressionInfo.AddDynamicPropertyReference(fieldName);
                    }
                }
                matchs = this.m_regexes.StaticFieldPropertyReference.Matches(expression);
                for (int k = 0; k < matchs.Count; k++)
                {
                    string str2 = matchs[k].Result("${fieldname}");
                    string propertyName = matchs[k].Result("${propertyname}");
                    if (((str2 != null) && (str2.Length != 0)) && ((propertyName != null) && (propertyName.Length != 0)))
                    {
                        expressionInfo.AddStaticPropertyReference(str2, propertyName);
                    }
                }
            }
            MatchCollection matchs2 = this.m_regexes.FieldName.Matches(expression);
            for (int i = 0; i < matchs2.Count; i++)
            {
                string str4 = matchs2[i].Result("${fieldname}");
                if ((str4 != null) && (str4.Length != 0))
                {
                    expressionInfo.AddReferencedField(str4);
                }
            }
        }

        private void GetReferencedParameterNames(string expression, ExpressionInfo expressionInfo)
        {
            MatchCollection matchs = this.m_regexes.ParameterName.Matches(expression);
            for (int i = 0; i < matchs.Count; i++)
            {
                string parameterName = matchs[i].Result("${parametername}");
                if ((parameterName != null) && (parameterName.Length != 0))
                {
                    expressionInfo.AddReferencedParameter(parameterName);
                }
            }
        }

        private void GetReferencedReportItemNames(string expression, ExpressionInfo expressionInfo)
        {
            MatchCollection matchs = this.m_regexes.ReportItemName.Matches(expression);
            for (int i = 0; i < matchs.Count; i++)
            {
                string reportItemName = matchs[i].Result("${reportitemname}");
                if ((reportItemName != null) && (reportItemName.Length != 0))
                {
                    expressionInfo.AddReferencedReportItem(reportItemName);
                }
            }
        }

        private string GetReferencedReportParameters(string expression)
        {
            string str = null;
            Match match = this.m_regexes.ParameterOnly.Match(expression);
            if (match.Success)
            {
                str = match.Result("${parametername}");
            }
            return str;
        }

        private void GetRowNumber(int currentPos, string functionName, string expression, bool isParameter, ExpressionParser.GrammarFlags grammarFlags, out int newPos, out RunningValueInfo rowNumber)
        {
            StringList list;
            this.GetArguments(currentPos, expression, out newPos, out list);
            rowNumber = new RunningValueInfo();
            rowNumber.AggregateType = DataAggregateInfo.AggregateTypes.CountRows;
            rowNumber.Expressions = new ExpressionInfo[0];
            if (1 <= list.Count)
            {
                rowNumber.Scope = this.GetScope(list[0], true);
            }
            this.m_numberOfRunningValues++;
        }

        private void GetRunningValue(int currentPos, string functionName, string expression, bool isParameter, ExpressionParser.GrammarFlags grammarFlags, out int newPos, out RunningValueInfo runningValue)
        {
            StringList list;
            this.GetArguments(currentPos, expression, out newPos, out list);
            runningValue = new RunningValueInfo();
            if (2 <= list.Count)
            {
                bool flag;
                try
                {
                    runningValue.AggregateType = (DataAggregateInfo.AggregateTypes)Enum.Parse(typeof(DataAggregateInfo.AggregateTypes), list[1], true);
                    flag = ((DataAggregateInfo.AggregateTypes.Aggregate != runningValue.AggregateType) && (DataAggregateInfo.AggregateTypes.Previous != runningValue.AggregateType)) && (DataAggregateInfo.AggregateTypes.CountRows != runningValue.AggregateType);
                }
                catch (ArgumentException)
                {
                    flag = false;
                }
            }
            if (1 <= list.Count)
            {
                if ((DataAggregateInfo.AggregateTypes.Count == runningValue.AggregateType) && ("*" == list[0].Trim()))
                {
                }
                else
                {
                    runningValue.Expressions = new ExpressionInfo[] { this.GetParameterExpression(list[0], grammarFlags) };
                }
            }
            if (3 <= list.Count)
            {
                runningValue.Scope = this.GetScope(list[2], true);
            }
            this.m_numberOfRunningValues++;
        }

        private string GetScope(string expression, bool allowNothing)
        {
            if (this.m_regexes.NothingOnly.Match(expression).Success)
            {
                if (allowNothing)
                {
                    return null;
                }
            }
            else
            {
                Match match2 = this.m_regexes.StringLiteralOnly.Match(expression);
                if (match2.Success)
                {
                    return match2.Result("${string}");
                }
            }
            return null;
        }

        private bool IsRecursive(string flag)
        {
            ExpressionParser.RecursiveFlags simple = ExpressionParser.RecursiveFlags.Simple;
            try
            {
                simple = (ExpressionParser.RecursiveFlags)Enum.Parse(typeof(ExpressionParser.RecursiveFlags), flag, true);
            }
            catch { }
            return (ExpressionParser.RecursiveFlags.Recursive == simple);
        }

        private ExpressionInfo Lex(string expression, ExpressionParser.ExpressionContext context, out string vbExpression)
        {
            ExpressionParser.GrammarFlags flags;
            vbExpression = null;
            this.m_context = context;
            ExpressionInfo expressionInfo = context.ParseExtended ? new ExpressionInfoExtended() : new ExpressionInfo();
            expressionInfo.OriginalText = expression;
            Match match = this.m_regexes.NonConstant.Match(expression);
            if (!match.Success)
            {
                expressionInfo.Type = ExpressionInfo.Types.Constant;
                switch (context.ConstantType)
                {
                    case ExpressionParser.ConstantType.String:
                        expressionInfo.Value = expression;
                        return expressionInfo;

                    case ExpressionParser.ConstantType.Boolean:
                        bool flag;
                        try
                        {
                            flag = XmlConvert.ToBoolean(expression);
                        }
                        catch
                        {
                            flag = false;
                        }
                        expressionInfo.BoolValue = flag;
                        return expressionInfo;

                    case ExpressionParser.ConstantType.Integer:
                        int num;
                        try
                        {
                            num = XmlConvert.ToInt32(expression);
                        }
                        catch
                        {
                            num = 0;
                        }
                        expressionInfo.IntValue = num;
                        return expressionInfo;
                }
                System.Diagnostics.Debug.Assert(false);
                throw new InvalidOperationException();
            }
            if ((this.m_context.Location & LocationFlags.InPageSection) != 0)
            {
                flags = ((ExpressionParser.GrammarFlags)ExpressionParser.ExpressionType2Restrictions(this.m_context.ExpressionType)) | (ExpressionParser.GrammarFlags.DenyDataSources | ExpressionParser.GrammarFlags.DenyDataSets | ExpressionParser.GrammarFlags.DenyPrevious | ExpressionParser.GrammarFlags.DenyFields | ExpressionParser.GrammarFlags.DenyRowNumber | ExpressionParser.GrammarFlags.DenyRunningValue);
            }
            else
            {
                flags = ((ExpressionParser.GrammarFlags)ExpressionParser.ExpressionType2Restrictions(this.m_context.ExpressionType)) | ExpressionParser.GrammarFlags.DenyPageGlobals;
            }
            vbExpression = expression.Substring(match.Length);
            this.VBLex(vbExpression, false, flags, expressionInfo);
            return expressionInfo;
        }

        private int NumberOfTimesDetected(string expression, Regex detectionRegex)
        {
            int num = 0;
            MatchCollection matchs = detectionRegex.Matches(expression);
            for (int i = 0; i < matchs.Count; i++)
            {
                string str = matchs[i].Result("${detected}");
                if ((str != null) && (str.Length != 0))
                {
                    num++;
                }
            }
            return num;
        }

        internal override ExpressionInfo ParseExpression(string expression, ExpressionParser.ExpressionContext context)
        {
            string str;
            System.Diagnostics.Debug.Assert(null != expression);
            return this.Lex(expression, context, out str);
        }

        internal override ExpressionInfo ParseExpression(string expression, ExpressionParser.ExpressionContext context, out bool userCollectionReferenced)
        {
            string str;
            ExpressionInfo info = this.Lex(expression, context, out str);
            userCollectionReferenced = false;
            if (info.Type == ExpressionInfo.Types.Expression)
            {
                userCollectionReferenced = this.DetectUserReference(str);
            }
            return info;
        }

        internal override ExpressionInfo ParseExpression(string expression, ExpressionParser.ExpressionContext context, ExpressionParser.DetectionFlags flag, out bool reportParameterReferenced, out string reportParameterName, out bool userCollectionReferenced)
        {
            string str;
            ExpressionInfo info = this.Lex(expression, context, out str);
            reportParameterReferenced = false;
            reportParameterName = null;
            userCollectionReferenced = false;
            if (info.Type == ExpressionInfo.Types.Expression)
            {
                if ((flag & ExpressionParser.DetectionFlags.ParameterReference) != 0)
                {
                    reportParameterReferenced = true;
                    reportParameterName = this.GetReferencedReportParameters(str);
                }
                if ((flag & ExpressionParser.DetectionFlags.UserReference) != 0)
                {
                    userCollectionReferenced = this.DetectUserReference(str);
                }
            }
            return info;
        }

        private void RemoveLineTerminators(ref string expression, Regex detectionRegex)
        {
            if (expression != null)
            {
                StringBuilder builder = new StringBuilder(expression, expression.Length);
                MatchCollection matchs = detectionRegex.Matches(expression);
                for (int i = matchs.Count - 1; i >= 0; i--)
                {
                    string str = matchs[i].Result("${detected}");
                    if ((str != null) && (str.Length != 0))
                    {
                        builder.Remove(matchs[i].Index, matchs[i].Length);
                    }
                }
                if (matchs.Count != 0)
                {
                    expression = builder.ToString();
                }
            }
        }

        private void VBLex(string expression, bool isParameter, ExpressionParser.GrammarFlags grammarFlags, ExpressionInfo expressionInfo)
        {
            if ((grammarFlags & ExpressionParser.GrammarFlags.DenyFields) == 0)
            {
                Match match = this.m_regexes.FieldOnly.Match(expression);
                if (match.Success)
                {
                    string fieldName = match.Result("${fieldname}");
                    expressionInfo.AddReferencedField(fieldName);
                    expressionInfo.Type = ExpressionInfo.Types.Field;
                    expressionInfo.Value = fieldName;
                    return;
                }
                if (this.m_context.ParseExtended && this.m_regexes.FieldWithExtendedProperty.Match(expression).Success)
                {
                    ((ExpressionInfoExtended)expressionInfo).IsExtendedSimpleFieldReference = true;
                }
            }
            if ((grammarFlags & ExpressionParser.GrammarFlags.DenyDataSets) == 0)
            {
                Match match2 = this.m_regexes.RewrittenCommandText.Match(expression);
                if (match2.Success)
                {
                    string dataSetName = match2.Result("${datasetname}");
                    expressionInfo.AddReferencedDataSet(dataSetName);
                    expressionInfo.Type = ExpressionInfo.Types.Token;
                    expressionInfo.Value = dataSetName;
                    return;
                }
            }
            this.EnforceRestrictions(ref expression, isParameter, grammarFlags);
            string str3 = string.Empty;
            int startat = 0;
            bool flag = false;
            while (startat < expression.Length)
            {
                Match match3 = this.m_regexes.SpecialFunction.Match(expression, startat);
                if (!match3.Success)
                {
                    str3 = str3 + expression.Substring(startat);
                    startat = expression.Length;
                }
                else
                {
                    str3 = str3 + expression.Substring(startat, match3.Index - startat);
                    string strA = match3.Result("${sfname}");
                    if ((strA == null) || (strA.Length == 0))
                    {
                        str3 = str3 + match3.Value;
                        startat = match3.Index + match3.Length;
                        continue;
                    }
                    str3 = str3 + match3.Result("${prefix}");
                    startat = match3.Index + match3.Length;
                    string str5 = this.CreateAggregateID();
                    if (string.Compare(strA, "Previous", true, CultureInfo.InvariantCulture) == 0)
                    {
                        RunningValueInfo info;
                        this.GetPreviousAggregate(startat, strA, expression, isParameter, grammarFlags, out startat, out info);
                        info.Name = str5;
                        expressionInfo.AddRunningValue(info);
                    }
                    else if (string.Compare(strA, "RunningValue", true, CultureInfo.InvariantCulture) == 0)
                    {
                        RunningValueInfo info2;
                        this.GetRunningValue(startat, strA, expression, isParameter, grammarFlags, out startat, out info2);
                        info2.Name = str5;
                        expressionInfo.AddRunningValue(info2);
                    }
                    else if (string.Compare(strA, "RowNumber", true, CultureInfo.InvariantCulture) == 0)
                    {
                        RunningValueInfo info3;
                        this.GetRowNumber(startat, strA, expression, isParameter, grammarFlags, out startat, out info3);
                        info3.Name = str5;
                        expressionInfo.AddRunningValue(info3);
                    }
                    else
                    {
                        DataAggregateInfo info4;
                        this.GetAggregate(startat, strA, expression, isParameter, grammarFlags, out startat, out info4);
                        info4.Name = str5;
                        expressionInfo.AddAggregate(info4);
                    }
                    if (!flag)
                    {
                        flag = true;
                        if ((str3.Trim().Length == 0) && (expression.Substring(startat).Trim().Length == 0))
                        {
                            expressionInfo.Type = ExpressionInfo.Types.Aggregate;
                            expressionInfo.Value = str5;
                            return;
                        }
                    }
                    if (expressionInfo.TransformedExpressionAggregatePositions == null)
                    {
                        expressionInfo.TransformedExpressionAggregatePositions = new IntList();
                        expressionInfo.TransformedExpressionAggregateIDs = new StringList();
                    }
                    expressionInfo.TransformedExpressionAggregatePositions.Add(str3.Length);
                    expressionInfo.TransformedExpressionAggregateIDs.Add(str5);
                    str3 = str3 + "Aggregates!" + str5;
                }
            }
            this.GetReferencedFieldNames(str3, expressionInfo);
            this.GetReferencedReportItemNames(str3, expressionInfo);
            this.GetReferencedParameterNames(str3, expressionInfo);
            this.GetReferencedDataSetNames(str3, expressionInfo);
            this.GetReferencedDataSourceNames(str3, expressionInfo);
            expressionInfo.Type = ExpressionInfo.Types.Expression;
            expressionInfo.TransformedExpression = str3;
            if ((this.m_context.ObjectType == ObjectType.Textbox) && this.Detected(expressionInfo.TransformedExpression, this.m_regexes.MeDotValueDetection))
            {
                base.SetValueReferenced();
            }
        }

        internal override bool BodyRefersToReportItems
        {
            get
            {
                return this.m_bodyRefersToReportItems;
            }
        }

        internal override int LastID
        {
            get
            {
                return (this.m_numberOfAggregates + this.m_numberOfRunningValues);
            }
        }

        internal override int NumberOfAggregates
        {
            get
            {
                return this.m_numberOfAggregates;
            }
        }

        internal override bool PageSectionRefersToReportItems
        {
            get
            {
                return this.m_pageSectionRefersToReportItems;
            }
        }

        internal sealed class ReportRegularExpressions
        {
            internal Regex AggregatesDetection;
            internal Regex Arguments;
            internal Regex DataSetName;
            internal Regex DataSetsDetection;
            internal Regex DataSourceName;
            internal Regex DataSourcesDetection;
            internal Regex DynamicFieldPropertyReference;
            internal Regex DynamicFieldReference;
            internal Regex ExtendedPropertyName;
            internal Regex FieldDetection;
            internal Regex FieldName;
            internal Regex FieldOnly;
            internal Regex FieldWithExtendedProperty;
            internal Regex IllegalCharacterDetection;
            internal Regex LineTerminatorDetection;
            internal Regex MeDotValueDetection;
            internal Regex NonConstant;
            internal Regex NothingOnly;
            internal Regex PageGlobalsDetection;
            internal Regex ParameterName;
            internal Regex ParameterOnly;
            internal Regex ParametersDetection;
            internal Regex ReportItemName;
            internal Regex ReportItemsDetection;
            internal Regex RewrittenCommandText;
            internal Regex SpecialFunction;
            internal Regex StaticFieldPropertyReference;
            internal Regex StringLiteralOnly;
            internal Regex UserDetection;
            internal static readonly VBExpressionParser.ReportRegularExpressions Value = new VBExpressionParser.ReportRegularExpressions();

            private ReportRegularExpressions()
            {
                RegexOptions options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase;
                this.NonConstant = new Regex(@"^\s*=", options);
                string str = Regex.Escape(@"-+()#,:&*/\^<=>");
                string str2 = Regex.Escape("!");
                string str3 = Regex.Escape(".");
                string str4 = "[" + str2 + str3 + "]";
                string str5 = "(^|[" + str + @"\s])";
                string str6 = "($|[" + str + str2 + str3 + @"\s])";
                this.FieldDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>Fields)" + str6, options);
                this.ReportItemsDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>ReportItems)" + str6, options);
                this.ParametersDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>Parameters)" + str6, options);
                this.PageGlobalsDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>(Globals" + str4 + "PageNumber)|(Globals" + str4 + "TotalPages))" + str6, options);
                this.AggregatesDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>Aggregates)" + str6, options);
                this.UserDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>User)" + str6, options);
                this.DataSetsDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>DataSets)" + str6, options);
                this.DataSourcesDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>DataSources)" + str6, options);
                this.MeDotValueDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>(?:Me.)?Value)" + str6, options);
                string str7 = Regex.Escape(":");
                string str8 = Regex.Escape("#");
                string str9 = "(" + str8 + "[^" + str8 + "]*" + str8 + ")";
                string str10 = Regex.Escape(":=");
                this.LineTerminatorDetection = new Regex(@"(?<detected>(\u000D\u000A)|([\u000D\u000A\u2028\u2029]))", options);
                this.IllegalCharacterDetection = new Regex("(\"((\"\")|[^\"])*\")|" + str9 + "|" + str10 + "|(?<detected>" + str7 + ")", options);
                string str11 = @"[\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Pc}][\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Pc}\p{Nd}\p{Mn}\p{Mc}\p{Cf}]*";
                string str12 = "ReportItems" + str2 + "(?<reportitemname>" + str11 + ")";
                string str13 = "Fields" + str2 + "(?<fieldname>" + str11 + ")";
                string str14 = "Parameters" + str2 + "(?<parametername>" + str11 + ")";
                string str15 = "DataSets" + str2 + "(?<datasetname>" + str11 + ")";
                string str16 = "DataSources" + str2 + "(?<datasourcename>" + str11 + ")";
                string str17 = "Fields((" + str2 + "(?<fieldname>" + str11 + "))|((" + str3 + "Item)?" + Regex.Escape("(") + "\"(?<fieldname>" + str11 + ")\"" + Regex.Escape(")") + "))";
                this.ExtendedPropertyName = new Regex("(Value|IsMissing|UniqueName|BackgroundColor|Color|FontFamily|Fontsize|FontWeight|FontStyle|TextDecoration|FormattedValue|Key|LevelNumber|ParentUniqueName)", options);
                string str18 = "(" + str3 + "Properties)?" + Regex.Escape("(") + "\"(?<propertyname>" + str11 + ")\"" + Regex.Escape(")");
                this.FieldWithExtendedProperty = new Regex(string.Concat(new object[] { @"^\s*", str17, "((", str3, this.ExtendedPropertyName, ")|(", str18, @"))\s*$" }), options);
                this.DynamicFieldReference = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + "(?<detected>(Fields(" + str3 + "Item)?" + Regex.Escape("(") + "))", options);
                this.DynamicFieldPropertyReference = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + str13 + Regex.Escape("("), options);
                this.StaticFieldPropertyReference = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + str13 + str3 + "(?<propertyname>" + str11 + ")", options);
                this.FieldOnly = new Regex(@"^\s*" + str13 + str3 + @"Value\s*$", options);
                this.RewrittenCommandText = new Regex(@"^\s*" + str15 + str3 + @"RewrittenCommandText\s*$", options);
                this.ParameterOnly = new Regex(@"^\s*" + str14 + str3 + @"Value\s*$", options);
                this.StringLiteralOnly = new Regex("^\\s*\"(?<string>((\"\")|[^\"])*)\"\\s*$", options);
                this.NothingOnly = new Regex(@"^\s*Nothing\s*$", options);
                this.ReportItemName = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + str12, options);
                this.FieldName = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + str13, options);
                this.ParameterName = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + str14, options);
                this.DataSetName = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + str15, options);
                this.DataSourceName = new Regex("(\"((\"\")|[^\"])*\")|" + str5 + str16, options);
                this.SpecialFunction = new Regex("(\"((\"\")|[^\"])*\")|(?<prefix>" + str5 + @")(?<sfname>RunningValue|RowNumber|First|Last|Previous|Sum|Avg|Max|Min|CountDistinct|Count|CountRows|StDevP|VarP|StDev|Var|Aggregate)\s*\(", options);
                string str19 = Regex.Escape("(");
                string str20 = Regex.Escape(")");
                string str21 = Regex.Escape(",");
                this.Arguments = new Regex("(\"((\"\")|[^\"])*\")|(?<openParen>" + str19 + ")|(?<closeParen>" + str20 + ")|(?<comma>" + str21 + ")", options);
            }
        }
    }

    internal abstract class ExpressionParser
    {
        private bool m_valueReferenced;
        private bool m_valueReferencedGlobal;

        internal ExpressionParser()
        {
        }

        internal abstract void ConvertField2ComplexExpr(ref ExpressionInfo expression);
        protected static Restrictions ExpressionType2Restrictions(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.General:
                    return Restrictions.None;

                case ExpressionType.ReportParameter:
                    return Restrictions.ReportParameter;

                case ExpressionType.ReportLanguage:
                    return Restrictions.ReportParameter;

                case ExpressionType.QueryParameter:
                    return Restrictions.ReportParameter;

                case ExpressionType.GroupExpression:
                    return Restrictions.GroupExpression;

                case ExpressionType.SortExpression:
                    return Restrictions.SortExpression;

                case ExpressionType.DataSetFilters:
                    return Restrictions.DataSetFilters;

                case ExpressionType.DataRegionFilters:
                    return Restrictions.DataSetFilters;

                case ExpressionType.GroupingFilters:
                    return Restrictions.SortExpression;

                case ExpressionType.FieldValue:
                    return Restrictions.FieldValue;
            }
            System.Diagnostics.Debug.Assert(false);
            return Restrictions.None;
        }

        internal abstract ExpressionInfo ParseExpression(string expression, ExpressionContext context);
        internal abstract ExpressionInfo ParseExpression(string expression, ExpressionContext context, out bool userCollectionReferenced);
        internal abstract ExpressionInfo ParseExpression(string expression, ExpressionContext context, DetectionFlags flag, out bool reportParameterReferenced, out string reportParameterName, out bool userCollectionReferenced);
        internal void ResetValueReferencedFlag()
        {
            this.m_valueReferenced = false;
        }

        protected void SetValueReferenced()
        {
            this.m_valueReferenced = true;
            this.m_valueReferencedGlobal = true;
        }

        internal abstract bool BodyRefersToReportItems { get; }

        internal abstract int LastID { get; }

        internal abstract int NumberOfAggregates { get; }

        internal abstract bool PageSectionRefersToReportItems { get; }

        internal bool ValueReferenced
        {
            get
            {
                return this.m_valueReferenced;
            }
        }

        internal bool ValueReferencedGlobal
        {
            get
            {
                return this.m_valueReferencedGlobal;
            }
        }

        internal enum ConstantType
        {
            String,
            Boolean,
            Integer
        }

        [Flags]
        internal enum DetectionFlags
        {
            ParameterReference = 1,
            UserReference = 2
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

        [StructLayout(LayoutKind.Sequential)]
        internal struct ExpressionContext
        {
            private ExpressionType m_expressionType;
            private ConstantType m_constantType;
            private LocationFlags m_location;
            private ObjectType m_objectType;
            private string m_objectName;
            private string m_propertyName;
            private string m_dataSetName;
            private bool m_parseExtended;
            internal ExpressionContext(ExpressionParser.ExpressionType expressionType, ExpressionParser.ConstantType constantType, LocationFlags location, ObjectType objectType, string objectName, string propertyName, string dataSetName, bool parseExtended)
            {
                this.m_expressionType = expressionType;
                this.m_constantType = constantType;
                this.m_location = location;
                this.m_objectType = objectType;
                this.m_objectName = objectName;
                this.m_propertyName = propertyName;
                this.m_dataSetName = dataSetName;
                this.m_parseExtended = parseExtended;
            }

            internal ExpressionType ExpressionType
            {
                get
                {
                    return this.m_expressionType;
                }
            }
            internal ExpressionParser.ConstantType ConstantType
            {
                get
                {
                    return this.m_constantType;
                }
            }
            internal LocationFlags Location
            {
                get
                {
                    return this.m_location;
                }
            }
            internal ObjectType ObjectType
            {
                get
                {
                    return this.m_objectType;
                }
            }
            internal string ObjectName
            {
                get
                {
                    return this.m_objectName;
                }
            }
            internal string PropertyName
            {
                get
                {
                    return this.m_propertyName;
                }
            }
            internal string DataSetName
            {
                get
                {
                    return this.m_dataSetName;
                }
            }
            internal bool ParseExtended
            {
                get
                {
                    return this.m_parseExtended;
                }
            }
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

        [Flags]
        protected enum GrammarFlags
        {
            DenyAggregates = 1,
            DenyDataSets = 0x100,
            DenyDataSources = 0x200,
            DenyFields = 8,
            DenyPageGlobals = 0x20,
            DenyPostSortAggregate = 0x40,
            DenyPrevious = 0x80,
            DenyReportItems = 0x10,
            DenyRowNumber = 4,
            DenyRunningValue = 2
        }

        internal enum RecursiveFlags
        {
            Simple,
            Recursive
        }

        [Flags]
        protected enum Restrictions
        {
            AggregateParameterInBody = 0x97,
            AggregateParameterInPageSection = 0x87,
            DataRegionFilters = 0x97,
            DataSetFilters = 0x97,
            FieldValue = 0xb7,
            GroupExpression = 0x93,
            GroupingFilters = 0xd6,
            InBody = 0x20,
            InPageSection = 910,
            None = 0,
            QueryParameter = 0x39f,
            ReportLanguage = 0x39f,
            ReportParameter = 0x39f,
            SortExpression = 0xd6
        }
    }

    internal sealed class ExpressionInfoExtended : ExpressionInfo
    {
        [NonSerialized]
        private bool m_isExtendedSimpleFieldReference;

        internal bool IsExtendedSimpleFieldReference
        {
            get
            {
                return this.m_isExtendedSimpleFieldReference;
            }
            set
            {
                this.m_isExtendedSimpleFieldReference = value;
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
    }

}
