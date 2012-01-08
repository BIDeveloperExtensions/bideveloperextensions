using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.Text.RegularExpressions;

#region Conditional compile for Yukon vs Katmai
#if KATMAI || DENALI
using IDTSComponentMetaDataXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData100;
using IDTSOutputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput100;
using IDTSInputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput100;
using IDTSInputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn100;
using IDTSOutputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn100;
using IDTSCustomPropertyCollectionXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomPropertyCollection100;
using IDTSCustomPropertyXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomProperty100;
#else
using IDTSComponentMetaDataXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData90;
using IDTSOutputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput90;
using IDTSInputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput90;
using IDTSInputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn90;
using IDTSOutputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn90;
using IDTSCustomPropertyCollectionXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomPropertyCollection90;
using IDTSCustomPropertyXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomProperty90;
#endif
#endregion

namespace BIDSHelper.SSIS.PerformanceVisualization
{
    /// <summary>
    /// A helper class that can be used when upstream SSIS components are replaced causing all the old LineageID values downstream to be invalid.
    /// </summary>
    public class LineageIdReplacer
    {
        private Regex regex;
        private Dictionary<int, int> lineageIDsToReplace;

        public static void ReplaceLineageIDs(MainPipe pipeline, Dictionary<int, int> lineageIDsToReplace)
        {
            LineageIdReplacer replacer = new LineageIdReplacer(pipeline, lineageIDsToReplace);
        }

        private LineageIdReplacer(MainPipe pipeline, Dictionary<int, int> lineageIDsToReplace)
        {
            this.lineageIDsToReplace = lineageIDsToReplace;

            StringBuilder sRegex = new StringBuilder();
            foreach (int i in lineageIDsToReplace.Keys)
            {
                if (sRegex.Length > 0)
                    sRegex.Append("|");
                sRegex.Append(i);
            }
            sRegex.Insert(0, @"\#\b(");
            sRegex.Append(@")\b");
            regex = new Regex(sRegex.ToString(), RegexOptions.Compiled);

            //fix all lineageIDs and other lineageID-dependent properties
            foreach (IDTSComponentMetaDataXX componentToFix in pipeline.ComponentMetaDataCollection)
            {
                ReplaceCustomPropertiesLineageIDs(componentToFix.CustomPropertyCollection);
                foreach (IDTSInputXX inputToFix in componentToFix.InputCollection)
                {
                    ReplaceCustomPropertiesLineageIDs(inputToFix.CustomPropertyCollection);
                    foreach (IDTSInputColumnXX inputCol in inputToFix.InputColumnCollection)
                    {
                        if (lineageIDsToReplace.ContainsKey(inputCol.LineageID))
                        {
                            inputCol.LineageID = lineageIDsToReplace[inputCol.LineageID];
                        }
                        ReplaceCustomPropertiesLineageIDs(inputCol.CustomPropertyCollection);
                    }
                }
                foreach (IDTSOutputXX outputToFix in componentToFix.OutputCollection)
                {
                    ReplaceCustomPropertiesLineageIDs(outputToFix.CustomPropertyCollection);
                    foreach (IDTSOutputColumnXX outputCol in outputToFix.OutputColumnCollection)
                    {
                        ReplaceCustomPropertiesLineageIDs(outputCol.CustomPropertyCollection);
                    }
                }
            }
        }

        private void ReplaceCustomPropertiesLineageIDs(IDTSCustomPropertyCollectionXX customProps)
        {
            IDTSCustomPropertyXX friendlyExpression = null;
            IDTSCustomPropertyXX expression = null;
            foreach (IDTSCustomPropertyXX prop in customProps)
            {
                if (prop.Name == "FriendlyExpression")
                {
                    friendlyExpression = prop;
                }
                else if (prop.Name == "Expression")
                {
                    prop.Value = ReplaceValueLineageIDs(prop.Value);
                    expression = prop;
                }
                else if (prop.ContainsID)
                {
                    if (prop.TypeConverter == "LCMappingType")
                    {
                        //this property shouldn't have ContainsID=true set, so we have to workaround this bug: https://connect.microsoft.com/SQLServer/feedback/ViewFeedback.aspx?FeedbackID=338029
                    }
                    else
                    {
                        prop.Value = ReplaceValueLineageIDs(prop.Value);
                    }
                }
            }
            if (friendlyExpression != null && expression != null)
            {
                friendlyExpression.Value = expression.Value;
            }
        }

        private object ReplaceValueLineageIDs(object obj)
        {
            if (obj == null)
            {
                return obj;
            }
            else if ((((obj is int) || (obj is uint)) || ((obj is long) || (obj is ulong))) || ((obj is short) || (obj is ushort)))
            {
                int iInt = (int)Convert.ChangeType(obj, typeof(int));
                if (lineageIDsToReplace.ContainsKey(iInt))
                    return Convert.ChangeType(lineageIDsToReplace[iInt], obj.GetType());
                else
                    return obj;
            }
            else if (obj.GetType() == typeof(string))
            {
                string str = obj.ToString();
                return regex.Replace(str, new MatchEvaluator(this.MatchEvaluatorDelegate));
            }
            else
            {
                return obj;
            }
        }

        private string MatchEvaluatorDelegate(Match match)
        {
            if (match.Groups.Count < 2) return match.Value;
            int iMatch = int.Parse(match.Groups[1].Value);
            return "#" + lineageIDsToReplace[iMatch].ToString();
        }
    }

}
