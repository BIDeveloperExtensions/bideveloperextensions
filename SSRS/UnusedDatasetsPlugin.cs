using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;

namespace BIDSHelper.SSRS
{
    public class UnusedDatasetsPlugin : BIDSHelperPluginBase
    {
        public UnusedDatasetsPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "UnusedDatasets"; }
        }

        public override int Bitmap
        {
            get { return 543; }
        }

        public override string ButtonText
        {
            get { return "Unused Report Datasets..."; }
        }

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        public override string MenuName
        {
            get { return "Project,Solution"; }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the name of the friendly name of the plug-in.
        /// </summary>
        /// <value>The friendly name.</value>
        public override string FeatureName
        {
            get { return "Unused Report Datasets"; }
        }

        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        public override string  HelpUrl
        {
	        get { return this.GetCodePlexHelpUrl("Dataset Usage Reports"); }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSRS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Provides a report of unused datasets. You can then delete the unused datasets, thus speeding report performance and scalability."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;
                if (hierItem.Object is Project)
                {
                    Project p = (Project)hierItem.Object;
                    if (GetRdlFilesInProjectItems(p.ProjectItems, true).Length > 0)
                        return true;
                }
                else if (solution != null)
                {
                    foreach (Project p in solution.Projects)
                    {
                        if (GetRdlFilesInProjectItems(p.ProjectItems, true).Length > 0)
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

        public static string[] GetRdlFilesInProjectItems(ProjectItems pis, bool bGetRDLC)
        {
            if (pis == null) return new string[] {};

            List<string> lst = new List<string>();
            foreach (ProjectItem pi in pis)
            {
                if (pi.Name.ToLower().EndsWith(".rdl") || (bGetRDLC && pi.Name.ToLower().EndsWith(".rdlc")))
                {
                    lst.Add(pi.get_FileNames(1));
                }
                lst.AddRange(GetRdlFilesInProjectItems(pi.ProjectItems, bGetRDLC));
            }
            return lst.ToArray();
        }

        public override void Exec()
        {
            ScanReports(true);
        }

        protected void ScanReports(bool LookForUnusedDatasets)
        {
            string sCurrentFile = string.Empty;
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;

                List<string> lstRdls = new List<string>();
                if (hierItem.Object is Project)
                {
                    Project p = (Project)hierItem.Object;
                    lstRdls.AddRange(GetRdlFilesInProjectItems(p.ProjectItems, true));
                }
                else if (solution != null)
                {
                    foreach (Project p in solution.Projects)
                    {
                        lstRdls.AddRange(GetRdlFilesInProjectItems(p.ProjectItems, true));
                    }
                }

                List<UsedRsDataSets.RsDataSetUsage> lstDataSets = new List<UsedRsDataSets.RsDataSetUsage>();
                foreach (string file in lstRdls)
                {
                    sCurrentFile = file;
                    UsedRsDataSets urds = new UsedRsDataSets(file);
                    foreach (UsedRsDataSets.RsDataSet ds in urds.DataSets)
                    {
                        if (LookForUnusedDatasets && ds.Usages.Count == 0)
                        {
                            UsedRsDataSets.RsDataSetUsage u = new UsedRsDataSets.RsDataSetUsage();
                            u.ReportName = ds.ReportName;
                            u.DataSetName = ds.DataSetName;
                            lstDataSets.Add(u);
                        }
                        else if (!LookForUnusedDatasets && ds.Usages.Count > 0)
                        {
                            foreach (string usage in ds.Usages)
                            {
                                UsedRsDataSets.RsDataSetUsage u = new UsedRsDataSets.RsDataSetUsage();
                                u.ReportName = ds.ReportName;
                                u.DataSetName = ds.DataSetName;
                                u.Usage = usage;
                                lstDataSets.Add(u);
                            }
                        }
                    }
                }

                if (lstDataSets.Count == 0)
                {
                    if (LookForUnusedDatasets)
                        MessageBox.Show("All datasets are in use.", "BIDS Helper Unused Datasets Report");
                    else
                        MessageBox.Show("No datasets found.", "BIDS Helper Used Datasets Report");
                }
                else
                {
                    ReportViewerForm frm = new ReportViewerForm();
                    frm.ReportBindingSource.DataSource = lstDataSets;
                    if (LookForUnusedDatasets)
                    {
                        frm.Report = "SSRS.UnusedDatasets.rdlc";
                        frm.Caption = "Unused Datasets Report";
                    }
                    else
                    {
                        frm.Report = "SSRS.UsedDatasets.rdlc";
                        frm.Caption = "Used Datasets Report";
                    }
                    Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                    reportDataSource1.Name = "BIDSHelper_SSRS_RsDataSetUsage";
                    reportDataSource1.Value = frm.ReportBindingSource;
                    frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);

                    frm.WindowState = FormWindowState.Maximized;
                    frm.Show();
                }
            }
            catch (System.Exception ex)
            {
                string sError = string.Empty;
                if (!string.IsNullOrEmpty(sCurrentFile)) sError += "Error while scanning report: " + sCurrentFile + "\r\n";
                sError += ex.Message + "\r\n" + ex.StackTrace;
                MessageBox.Show(sError);
            }
        }

    }

    public class UsedDatasetsPlugin : UnusedDatasetsPlugin
    {
        public UsedDatasetsPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "UsedDatasets"; }
        }

        public override int Bitmap
        {
            get { return 723; }
        }

        public override string ButtonText
        {
            get { return "Used Report Datasets..."; }
        }

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

        public override void Exec()
        {
            ScanReports(false);
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
                //see if the surrounding table, for example, has already said it uses a particular DataSet. If so, we won't report it again
                string sDataSetUsedByParent = string.Empty;
                XmlNode parentNode = nodeExpression.SelectSingleNode("ancestor-or-self::*/rs:DataSetName", nsmgr);
                if (parentNode != null) sDataSetUsedByParent = parentNode.InnerText;

                //parse the expression
                VBExpressionParser parser = new VBExpressionParser();
                ExpressionInfo info = parser.ParseExpression(nodeExpression.InnerText, new ExpressionParser.ExpressionContext(ExpressionParser.ExpressionType.General, ExpressionParser.ConstantType.String, ExpressionParser.LocationFlags.None, ExpressionParser.ObjectType.Field, string.Empty, "Text", "DSN", true));
                RecurseExpressionInfo(info, nodeExpression, sDataSetUsedByParent);
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
