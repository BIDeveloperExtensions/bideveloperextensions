#if SQL2017
extern alias localAdomdClient;
using localAdomdClient.Microsoft.AnalysisServices.AdomdClient;
#else
using Microsoft.AnalysisServices.AdomdClient;
#endif

using EnvDTE;
using EnvDTE80;
using System.Xml;
using System.Xml.Xsl;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;

using Microsoft.AnalysisServices;
using BIDSHelper.Core;
using System;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class DeployAggDesignsPlugin : BIDSHelperPluginBase
    {

        public DeployAggDesignsPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.DeployAggDesignsId, ".cube");
        }

        public override string ShortName
        {
            get { return "DeployAggDesigns"; }
        }

        //public override int Bitmap
        //{
        //    get { return 2605; }
        //}

        public override string ToolTip
        {
            get { return "Deploys just the Aggregation Designs for this cube"; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASMulti; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Deploy just the aggregation designs of a cube."; }
        }

        public override string FeatureName
        {
            get
            {
                return "Deploy Aggregation Designs";
            }
        }


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                foreach (UIHierarchyItem hierItem in ((System.Array)solExplorer.SelectedItems))
                {
                    if (hierItem.Name.ToLower().EndsWith(".cube") && hierItem.Object is ProjectItem) //checking the file extension is adequate because this feature is not needed for in online mode (when live connected to the server)
                    {
                        ProjectItem projItem = (ProjectItem)hierItem.Object;
                        DeployAggDesigns(projItem, this.ApplicationObject);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void DeployAggDesigns(ProjectItem projItem, DTE2 ApplicationObject)
        {
            Microsoft.AnalysisServices.Cube oCube = (Microsoft.AnalysisServices.Cube)projItem.Object;

            bool bFoundAggDesign = false;
            foreach (MeasureGroup mg in oCube.MeasureGroups)
            {
                if (mg.AggregationDesigns.Count > 0)
                {
                    bFoundAggDesign = true;
                    break;
                }
            }
            if (!bFoundAggDesign)
            {
                MessageBox.Show("There are no aggregation designs defined in this cube yet.");
                return;
            }

            if (MessageBox.Show("This command deploys just the aggregation designs in this cube. It does not change which aggregation design is assigned to each partition.\r\n\r\nYou should run a ProcessIndex command from Management Studio on this cube after aggregation designs have been deployed.\r\n\r\nDo you wish to continue?", "BIDS Helper - Deploy Aggregation Designs", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(true, "Deploying Aggregation Designs", 1, 5);

                string sPartitionsFileName = projItem.get_FileNames(1);
                sPartitionsFileName = sPartitionsFileName.Substring(0, sPartitionsFileName.Length - 5) + ".partitions";
                
                // Check if the file is read-only (and probably checked in to a source control system)
                // before attempting to save. (issue: 10327 )
                FileAttributes fa = System.IO.File.GetAttributes(sPartitionsFileName);
                if ((fa & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
                {
                    //TODO - prompt before saving?
                    //Save the cube
                    projItem.Save("");
                }

                ApplicationObject.StatusBar.Progress(true, "Deploying Aggregation Designs", 2, 5);

                // extract deployment information
                DeploymentSettings deploySet = new DeploymentSettings(projItem);

                // use xlst to create xmla alter command
                XslCompiledTransform xslt = new XslCompiledTransform();
                XmlReader xsltRdr;
                XmlReader xrdr;

                // read xslt from embedded resource
                xsltRdr = XmlReader.Create(new StringReader(BIDSHelper.Resources.Common.DeployAggDesigns));
                using ((xsltRdr))
                {
                    // read content from .partitions file
                    xrdr = XmlReader.Create(sPartitionsFileName);
                    using (xrdr)
                    {
                        ApplicationObject.StatusBar.Progress(true, "Deploying Aggregation Designs", 3, 5);
                        // Connect to Analysis Services
                        Microsoft.AnalysisServices.Server svr = new Microsoft.AnalysisServices.Server();
                        svr.Connect(deploySet.TargetServer);
                        ApplicationObject.StatusBar.Progress(true, "Deploying Aggregation Designs", 4, 5);
                        // execute the xmla
                        try
                        {
                            // Build up the Alter MdxScript command using XSLT against the .partitions file
                            XslCompiledTransform xslta = new XslCompiledTransform();
                            StringBuilder sb = new StringBuilder();
                            XmlWriterSettings xws = new XmlWriterSettings();
                            xws.OmitXmlDeclaration = true;
                            xws.ConformanceLevel = ConformanceLevel.Fragment;
                            XmlWriter xwrtr = XmlWriter.Create(sb, xws);

                            xslta.Load(xsltRdr);
                            XsltArgumentList xslarg = new XsltArgumentList();

                            Database targetDB = svr.Databases.FindByName(deploySet.TargetDatabase);
                            if (targetDB == null)
                            {
                                throw new System.Exception(string.Format("A database called {0} could not be found on the {1} server", deploySet.TargetDatabase, deploySet.TargetServer));
                            }
                            xslarg.AddParam("TargetDatabase", "", targetDB.ID);
                            xslarg.AddParam("TargetCubeID", "", oCube.ID);
                            xslta.Transform(xrdr, xslarg, xwrtr);

                            Cube oServerCube = targetDB.Cubes.Find(oCube.ID);
                            if (oServerCube == null)
                            {
                                throw new System.Exception(string.Format("The {0} cube is not yet deployed to the {1} server.", oCube.Name, deploySet.TargetServer));
                            }

                            // update the agg designs
                            XmlaResultCollection xmlaRC = svr.Execute(sb.ToString());
                            StringBuilder sbErr = new StringBuilder();
                            for (int iRC = 0; iRC < xmlaRC.Count; iRC++)
                            {
                                for (int iMsg = 0; iMsg < xmlaRC[iRC].Messages.Count; iMsg++)
                                {
                                    if (!string.IsNullOrEmpty(xmlaRC[iRC].Messages[iMsg].Description))
                                        sbErr.AppendLine(xmlaRC[iRC].Messages[iMsg].Description);
                                }
                            }
                            if (sbErr.Length > 0)
                                MessageBox.Show(sbErr.ToString(), "BIDSHelper - Deploy Aggregation Designs");

                            projItem.DTE.Solution.SolutionBuild.BuildProject(projItem.DTE.Solution.SolutionBuild.ActiveConfiguration.Name, projItem.ContainingProject.UniqueName, false);
                        }
                        catch (System.Exception ex)
                        {
                            if (MessageBox.Show("The following error occured while trying to deploy the aggregation designs\r\n"
                                                + ex.Message
                                                + "\r\n\r\nDo you want to see a stack trace?"
                                            , "BIDSHelper - Deploy Aggregation Designs"
                                            , MessageBoxButtons.YesNo
                                            , MessageBoxIcon.Error
                                            , MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            {
                                MessageBox.Show(ex.StackTrace);
                            }
                        }
                        finally
                        {
                            ApplicationObject.StatusBar.Progress(true, "Deploying Aggregation Designs", 5, 5);
                            svr.Disconnect();
                        }
                    }
                }
            }
            finally
            {
                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(false, "Deploying Aggregation Designs", 5, 5);
            }
        }

    }
}