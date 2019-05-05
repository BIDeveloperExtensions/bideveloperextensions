using System;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using System.Windows.Forms;
using System.Collections.Generic;
using BIDSHelper.Core;

namespace BIDSHelper.SSRS
{

    [FeatureCategory(BIDSFeatureCategories.SSRS)]
    public class UsedDatasetsPlugin : BIDSHelperPluginBase
    {
        public UsedDatasetsPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.UsedDatasetsId);
        }

        public override string ShortName
        {
            get { return "UsedDatasets"; }
        }

        //public override int Bitmap
        //{
        //    get { return 723; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Used Report Datasets..."; }
        //}

        /// <summary>
        /// Gets the name of the friendly name of the plug-in.
        /// </summary>
        /// <value>The friendly name.</value>
        public override string FeatureName
        {
            get { return "Used Report Datasets"; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Provides a report of datasets in use. It also lists which parts of the report use each dataset."; }
        }

        public override BIDSFeatureCategories FeatureCategory
        {
            get
            {
                return BIDSFeatureCategories.SSRS;
            }
        }

        public override string ToolTip
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ShouldDisplayCommand()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;
                Project project = hierItem.Object as Project;
                if (project == null && hierItem.Object is ProjectItem)
                {
                    ProjectItem pi = hierItem.Object as ProjectItem;
                    project = pi.SubProject;
                }
                if (project != null)
                {
                    if (DatasetScanner.GetRdlFilesInProjectItems(project.ProjectItems, true).Length > 0)
                        return true;
                }
                else if (solution != null)
                {
                    foreach (Project p in solution.Projects)
                    {
                        if (DatasetScanner.GetRdlFilesInProjectItems(p.ProjectItems, true).Length > 0)
                            return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public override void Exec()
        {
            DatasetScanner.ScanReports(this.ApplicationObject.ToolWindows.SolutionExplorer, false);
        }
    }

    public class UsedRsDataSets
    {
        private static List<string> _nodeNamesToReport;
        private Dictionary<string, RsDataSet> _dataSets = new Dictionary<string, RsDataSet>(StringComparer.CurrentCultureIgnoreCase);

        static UsedRsDataSets()
        {
            _nodeNamesToReport = new List<string>();
            _nodeNamesToReport.AddRange(new string[] { "Action", "Drillthrough", "PageHeader", "PageFooter" });
        }

        public UsedRsDataSets(string RdlPath)
        {
            //all this code is just to call VBExpressionParser.ParseExpression which is a private class
            //that method required all sorts of other objects, not many of which we know what in the world they are for
            System.Reflection.Assembly assembly = System.Reflection.Assembly.Load("Microsoft.ReportingServices.ProcessingCore");
#if !YUKON
            Type type = BIDSHelper.SSIS.ExpressionHighlighterPlugin.GetPrivateType(assembly.GetTypes()[0], "Microsoft.ReportingServices.RdlExpressions.VBExpressionParser");
#else
            Type type = BIDSHelper.SSIS.ExpressionHighlighterPlugin.GetPrivateType(assembly.GetTypes()[0], "Microsoft.ReportingServices.ReportProcessing.VBExpressionParser");
#endif
            System.Reflection.ConstructorInfo constructor = type.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)[0];

            //create PublishingErrorContext
            Type typeErrorContext = BIDSHelper.SSIS.ExpressionHighlighterPlugin.GetPrivateType(assembly.GetTypes()[0], "Microsoft.ReportingServices.ReportProcessing.PublishingErrorContext");
            System.Reflection.ConstructorInfo constructorErrorContext = typeErrorContext.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)[0];
            object oErrorContext = constructorErrorContext.Invoke(new object[] { });

            //get ExpressionContext type and constructor
            Type typeExpressionContext = BIDSHelper.SSIS.ExpressionHighlighterPlugin.GetPrivateType(assembly.GetTypes()[0], "Microsoft.ReportingServices.RdlExpressions.ExpressionParser+ExpressionContext");

            System.Reflection.ConstructorInfo constructorExpressionContext = typeExpressionContext.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)[0];

            //get InternalPublishingContext constructor
            Type typeInternalPublishingContext = BIDSHelper.SSIS.ExpressionHighlighterPlugin.GetPrivateType(assembly.GetTypes()[0], "Microsoft.ReportingServices.ReportProcessing.PublishingContext"); //used to be ReportPublishing.InternalPublishingContext which doesn't exist anymore?
            System.Reflection.ConstructorInfo constructorInternalPublishingContext = null;
            foreach (System.Reflection.ConstructorInfo c in typeInternalPublishingContext.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if (c.GetParameters().Length == 8)
                {
                    constructorInternalPublishingContext = c;
                    break;
                }
            }
            if (constructorInternalPublishingContext == null) throw new Exception("Couldn't find InternalPublishingContext constructor");

            //get PreviewItemContext
            System.Reflection.Assembly assemblyReportPreview = System.Reflection.Assembly.Load("Microsoft.ReportingServices.ReportPreview");
            Type typeItemContext = BIDSHelper.SSIS.ExpressionHighlighterPlugin.GetPrivateType(assemblyReportPreview.GetTypes()[0], "Microsoft.Reporting.PreviewItemContext");

            ////////////////////////////////////////
            //now run the main code to parse the RDL
            XmlDocument doc = new XmlDocument();
            doc.Load(RdlPath);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("rs", doc.DocumentElement.NamespaceURI);

            //build list of all DataSets in report
            foreach (XmlNode nodeDataSet in doc.SelectNodes("/rs:Report/rs:DataSets/rs:DataSet", nsmgr))
            {
                RsDataSet ds = new RsDataSet();
                ds.ReportName = System.IO.Path.GetFileNameWithoutExtension(RdlPath);
                ds.DataSetName = nodeDataSet.Attributes["Name"].Value;
                _dataSets.Add(ds.DataSetName, ds);
            }

            //build list of all objects such as tables that use the dataset
            foreach (XmlNode nodeDataSetRef in doc.SelectNodes("//rs:DataSetName", nsmgr))
            {
                if (_dataSets.ContainsKey(nodeDataSetRef.InnerText))
                {
                    if (!_dataSets[nodeDataSetRef.InnerText].Usages.Contains(GetPathForXmlNode(nodeDataSetRef)))
                    {
                        _dataSets[nodeDataSetRef.InnerText].Usages.Add(GetPathForXmlNode(nodeDataSetRef));
                    }
                }
            }

            //loop through all expressions and parse them
            foreach (XmlNode nodeExpression in doc.SelectNodes("//*[starts-with(text(),'=')]", nsmgr))
            {
                try
                {
                    //see if the surrounding table, for example, has already said it uses a particular DataSet. If so, we won't report it again
                    string sDataSetUsedByParent = string.Empty;
                    XmlNode parentNode = nodeExpression.SelectSingleNode("ancestor-or-self::*/rs:DataSetName", nsmgr);
                    if (parentNode != null) sDataSetUsedByParent = parentNode.InnerText;

                    //create VBExpressionParser
                    object vbparser = constructor.Invoke(new object[] { oErrorContext });

                    //create PreviewItemContext
                    object itemContext = typeItemContext.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)[0].Invoke(null);

                    //signature of this function call is: public InternalPublishingContext(ICatalogItemContext catalogContext, Microsoft.ReportingServices.ReportProcessing.ReportProcessing.CheckSharedDataSource checkDataSourceCallback, Microsoft.ReportingServices.ReportProcessing.ReportProcessing.ResolveTemporaryDataSource resolveTemporaryDataSourceCallback, DataSourceInfoCollection originalDataSources, IConfiguration configuration, IDataProtection dataProtection, bool traceAtomicScopes, bool isPackagedReportArchive)
                    object oInternalPublishingContext = constructorInternalPublishingContext.Invoke(new object[] { itemContext, null, null, null, null, null, null, null }); //last two params used to be: false, false

                    //create ExpressionContext
                    //signature of this function call is: internal ExpressionContext(ExpressionParser.ExpressionType expressionType, DataType constantType, LocationFlags location, ObjectType objectType, string objectName, string propertyName, string dataSetName, int maxExpressionLength, InternalPublishingContext publishingContext);
                    object expressionContext = constructorExpressionContext.Invoke(new object[] { ExpressionInfo.ExpressionType.General, 0x12, ExpressionInfo.LocationFlags.None, ExpressionInfo.ObjectType.Field, string.Empty, "Text", "DSN", int.MaxValue, oInternalPublishingContext });

                    //find and invoke ParseExpression method

                    int iParseExpressionParamCount = 3;
                    object[] arrParseExpressionParams = new object[] { nodeExpression.InnerText, expressionContext, 0 };

                    System.Reflection.MethodInfo methodParseExpression = null;
                    foreach (System.Reflection.MethodInfo method in type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                    {
                        if (method.Name == "ParseExpression" && method.GetParameters().Length == iParseExpressionParamCount)
                        {
                            methodParseExpression = method;
                            break;
                        }
                    }
                    if (methodParseExpression == null) throw new Exception("Couldn't find VBExpressionParser.ParseExpression method");
                    object expressionInfo = methodParseExpression.Invoke(vbparser, arrParseExpressionParams);

                    //create a wrapper for the SSRS object then examine it recursively
                    ExpressionInfo info = new ExpressionInfo(expressionInfo);
                    RecurseExpressionInfo(info, nodeExpression, sDataSetUsedByParent);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while scanning...\r\nReport: " + RdlPath + "\r\nExpression: " + nodeExpression.InnerText, ex);
                }
            }
        }

        private void RecurseExpressionInfo(ExpressionInfo info, XmlNode nodeExpression, string sDataSetUsedByParent)
        {
            if (_dataSets.Count == 1 && string.IsNullOrEmpty(sDataSetUsedByParent))
            {
                if (info.ReferencedFieldProperties != null //this gets you more complicated expressions that refer to fields
                || info.DynamicFieldReferences //this gets you Field("Field1").Value expressions
                || info.Type == ExpressionInfo.Types.Field) //this gets you simple "=Fields!Field.Value" expressions
                {
                    foreach (RsDataSet ds in _dataSets.Values)
                        ds.Usages.Add(GetPathForXmlNode(nodeExpression));
                }
            }
            if (info.ReferencedDataSets != null)
            {
                foreach (string sRefDataSet in info.ReferencedDataSets)
                {
                    if (_dataSets.ContainsKey(sRefDataSet) && string.Compare(sDataSetUsedByParent, sRefDataSet, true) != 0)
                    {
                        _dataSets[sRefDataSet].Usages.Add(GetPathForXmlNode(nodeExpression));
                    }
                }
            }
            RecurseAggregates(info, nodeExpression, sDataSetUsedByParent);

            if (info.DataSetsFromLookups != null)
            {
                foreach (string sRefDataSet in info.DataSetsFromLookups)
                {
                    if (_dataSets.ContainsKey(sRefDataSet) && string.Compare(sDataSetUsedByParent, sRefDataSet, true) != 0)
                    {
                        _dataSets[sRefDataSet].Usages.Add(GetPathForXmlNode(nodeExpression));
                    }
                }
            }
        }

        private void RecurseAggregates(ExpressionInfo info, XmlNode nodeExpression, string sDataSetUsedByParent)
        {
            DataAggregateInfoList lst = new DataAggregateInfoList();
            if (info.RunningValues != null)
            {
                foreach (RunningValueInfo rvi in info.RunningValues)
                {
                    lst.Add(rvi);
                }
            }

            if (info.Aggregates != null)
            {
                foreach (DataAggregateInfo agg in info.Aggregates)
                {
                    lst.Add(agg);
                }
            }

            foreach (DataAggregateInfo agg in lst)
            {
                if (agg.Scope != null && string.Compare(sDataSetUsedByParent, agg.Scope, true) != 0 && _dataSets.ContainsKey(agg.Scope))
                {
                    _dataSets[agg.Scope].Usages.Add(GetPathForXmlNode(nodeExpression));
                }
                if (agg.Expressions != null)
                {
                    foreach (ExpressionInfo aggSubExpression in agg.Expressions)
                    {
                        RecurseExpressionInfo(aggSubExpression, nodeExpression, sDataSetUsedByParent);
                    }
                }
            }
        }

        public RsDataSet[] DataSets
        {
            get
            {
                List<RsDataSet> lst = new List<RsDataSet>(_dataSets.Values.Count);
                lst.AddRange(_dataSets.Values);
                return lst.ToArray();
            }
        }

        private static string GetPathForXmlNode(XmlNode node)
        {
            XmlNode nodeOriginal = node;
            string sPath = string.Empty;
            while (node.ParentNode != null)
            {
                if (node.Attributes != null && node.Attributes["Name"] != null)
                {
                    if (sPath.Length > 0) sPath = "... " + sPath;
                    sPath = node.Name + " " + node.Attributes["Name"].Value + sPath;
                }
                else if (_nodeNamesToReport.Contains(node.Name))
                {
                    if (sPath.Length > 0) sPath = "... " + sPath;
                    sPath = node.Name + sPath;
                }
                node = node.ParentNode;
            }

            //no path constructed... run back through the nodes and get every node name
            if (string.IsNullOrEmpty(sPath))
            {
                node = nodeOriginal;
                while (node.ParentNode != null)
                {
                    if (sPath.Length > 0) sPath = "... " + sPath;
                    sPath = node.Name + sPath;
                    node = node.ParentNode;
                }
            }

            return sPath;
        }

        public class RsDataSet
        {
            private string _reportName;
            private string _dataSetName;
            private List<string> _usages = new List<string>();

            public string ReportName
            {
                get { return _reportName; }
                set { _reportName = value; }
            }

            public string DataSetName
            {
                get { return _dataSetName; }
                set { _dataSetName = value; }
            }

            public List<string> Usages
            {
                get { return _usages; }
                set { _usages = value; }
            }
        }

        public class RsDataSetUsage
        {
            private string _reportName;
            private string _dataSetName;
            private string _usage;

            public string ReportName
            {
                get { return _reportName; }
                set { _reportName = value; }
            }

            public string DataSetName
            {
                get { return _dataSetName; }
                set { _dataSetName = value; }
            }

            public string Usage
            {
                get { return _usage; }
                set { _usage = value; }
            }
        }
    }
}
