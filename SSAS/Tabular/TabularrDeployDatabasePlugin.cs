using System;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.AnalysisServices.BackEnd;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using BIDSHelper.Core;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASTabular)]
    public class TabularDeployDatabasePlugin : BIDSHelperPluginBase
    {

        public TabularDeployDatabasePlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.TabularDeployDatabaseId, ".bim");
        }

        public override string ShortName
        {
            get { return "TabularDeployDatabase"; }
        }

        public override string FeatureName
        {
            get
            {
                return "Deploy a Tabular Database";
            }
        }

        //public override int Bitmap
        //{
        //    get { return 2605; }
        //}

        public override string ToolTip
        {
            get { return "Deploys the Tabular Database for Non-Server Admins"; }
        }

        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        public override string HelpUrl
        {
            get { return this.GetCodePlexHelpUrl("Deploy Tabular Database"); }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASTabular; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Allows you to right click on a database in a Tabular solution and deploy it when you are just a database admin"; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        //public override bool DisplayCommand(UIHierarchyItem item)
        //{
        //    try
        //    {
        //        UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
        //        if (((System.Array)solExplorer.SelectedItems).Length != 1)
        //            return false;

        //        UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
        //        if (!(hierItem.Object is ProjectItem)) return false;
        //        string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
        //        return (sFileName.EndsWith(".bim"));
        //    }
        //    catch
        //    {
        //    }
        //    return false;
        //}


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));

                var sandbox = TabularHelpers.GetTabularSandboxFromBimFile(this, true);
                if (sandbox == null) throw new Exception("Can't get Sandbox!");

                ProjectItem projItem = (ProjectItem)hierItem.Object;

                ExecInternal(projItem, sandbox);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        private void ExecInternal(ProjectItem projItem, DataModelingSandbox sandbox)
        {
#if DENALI || SQL2014
            var db = sandbox.Database;
#else
            var db = ((DataModelingSandboxAmo)sandbox.Impl).Database;
#endif
            // extract deployment information
            DeploymentSettings deploySet = new DeploymentSettings(projItem);

            ApplicationObject.StatusBar.Progress(true, "Deploying Tabular Database", 3, 5);
            // Connect to Analysis Services
            Microsoft.AnalysisServices.Server svr = new Microsoft.AnalysisServices.Server();
            svr.Connect(deploySet.TargetServer);
            ApplicationObject.StatusBar.Progress(true, "Deploying Tabular Database", 4, 5);
            // execute the xmla
            try
            {
                Microsoft.AnalysisServices.Scripter scr = new Microsoft.AnalysisServices.Scripter();

                Database targetDB = svr.Databases.FindByName(deploySet.TargetDatabase);
                if (targetDB == null)
                {
                    throw new System.Exception(
                        string.Format("A database called {0} could not be found on the {1} server",
                                      deploySet.TargetDatabase, deploySet.TargetServer));
                }
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                xws.ConformanceLevel = ConformanceLevel.Fragment;
                XmlWriter xwrtr = XmlWriter.Create(sb, xws);
                // TODO - do we need different code for JSON based models??
                scr.ScriptAlter(new Microsoft.AnalysisServices.MajorObject[] {db}, xwrtr, true);

                // update the MDX Script
                XmlaResultCollection xmlaRC = svr.Execute(sb.ToString());
                if (xmlaRC.Count == 1 && xmlaRC[0].Messages.Count == 0)
                {
                    // all OK - 1 result - no messages    
                }
                else
                {
                    StringBuilder sbErr = new StringBuilder();
                    for (int iRC = 0; iRC < xmlaRC.Count; iRC++)
                    {
                        for (int iMsg = 0; iMsg < xmlaRC[iRC].Messages.Count; iMsg++)
                        {
                            sbErr.AppendLine(xmlaRC[iRC].Messages[iMsg].Description);
                        }
                    }
                    MessageBox.Show(sbErr.ToString(), "BIDSHelper - Deploy Tabular Database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "BIDSHelper - Deploy Tabular Database - Exception");
                package.Log.Exception("Deploy Tabular Database Failed", ex);
            }
        }

    }
}