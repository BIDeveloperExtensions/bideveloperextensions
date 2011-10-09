using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Xml.Xsl;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;

using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.AdomdClient;


namespace BIDSHelper
{
    public class DeployMDXScriptPlugin : BIDSHelperPluginBase
    {

        public DeployMDXScriptPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "DeployMdxScript"; }
        }

        public override string FeatureName
        {
            get
            {
                return "Deploy the MDX Script for a cube";
            }
        }

        public override int Bitmap
        {
            get { return 2605; }
        }

        public override string ButtonText
        {
            get { return "Deploy MDX Script"; }
        }

        public override string ToolTip
        {
            get { return "Deploys just the MDX Script for this cube"; }
        }

        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        public override string HelpUrl
        {
            get { return this.GetCodePlexHelpUrl("Deploy MDX Script"); }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSAS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Allows you to right click on a cube in an Analysis Services solution and deploy just the calculation script."; }
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
                bool bFoundRightItem = false;
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                foreach (UIHierarchyItem hierItem in ((System.Array)solExplorer.SelectedItems))
                {
                    if (hierItem.Name.ToLower().EndsWith(".cube")) //checking the file extension is adequate because this feature is not needed for in online mode (when live connected to the server)
                        bFoundRightItem = true;
                    else
                        return false;
                }
                return bFoundRightItem;
            }
            catch
            {
                return false;
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
                        DeployScript(projItem, this.ApplicationObject);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void DeployScript(ProjectItem projItem, DTE2 ApplicationObject)
        {
            Microsoft.AnalysisServices.Cube oCube = (Microsoft.AnalysisServices.Cube)projItem.Object;
            try
            {
                //validate the script because deploying an invalid script makes cube unusable
                Microsoft.AnalysisServices.Design.Scripts script = new Microsoft.AnalysisServices.Design.Scripts(oCube);               

            }
            catch (Microsoft.AnalysisServices.Design.ScriptParsingFailed ex)
            {
                string throwaway = ex.Message;
                MessageBox.Show("MDX Script in " + oCube.Name + " is not valid.", "Problem Deploying MDX Script");
                return;
            }

            if (oCube.MdxScripts.Count == 0)
            {
                MessageBox.Show("There is no MDX script defined in this cube yet.");
                return;
            }

            try
            {
                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 1, 5);

                
                // Check if the file is read-only (and probably checked in to a source control system)
                // before attempting to save. (issue: 10327 )
                FileAttributes fa = System.IO.File.GetAttributes(projItem.get_FileNames(1));
                if ((fa & FileAttributes.ReadOnly) != FileAttributes.ReadOnly )
                {
                    //TODO - can I check and maybe prompt before saving?
                    //Save the cube
                    projItem.Save("");
                }

                ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 2, 5);

                // extract deployment information
                DeploymentSettings deploySet = new DeploymentSettings(projItem);

                // use xlst to create xmla alter command
                XslCompiledTransform xslt = new XslCompiledTransform();
                XmlReader xsltRdr;
                XmlReader xrdr;

                // read xslt from embedded resource
                xsltRdr = XmlReader.Create(new StringReader(BIDSHelper.Resources.Common.DeployMdxScript));
                using ((xsltRdr))
                {
                    // read content from .cube file
                    xrdr = XmlReader.Create(projItem.get_FileNames(1));
                    using (xrdr)
                    {


                        
                        ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 3, 5);
                        // Connect to Analysis Services
                        Microsoft.AnalysisServices.Server svr = new Microsoft.AnalysisServices.Server();
                        svr.Connect(deploySet.TargetServer);
                        ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 4, 5);
                        // execute the xmla
                        try
                        {
                            Microsoft.AnalysisServices.Scripter scr = new Microsoft.AnalysisServices.Scripter();
                            
                            // Build up the Alter MdxScript command using XSLT against the .cube file
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
                            string targetDatabaseID = targetDB.ID;
                            xslarg.AddParam("TargetDatabase", "", targetDatabaseID);
                            xslta.Transform(xrdr, xslarg, xwrtr);

                            // Extract the current script from the server and keep a temporary backup copy of it
                            StringBuilder sbBackup = new StringBuilder();
                            XmlWriterSettings xwSet = new XmlWriterSettings();
                            xwSet.ConformanceLevel = ConformanceLevel.Fragment;
                            xwSet.OmitXmlDeclaration = true;
                            xwSet.Indent = true;
                            XmlWriter xwScript = XmlWriter.Create(sbBackup,xwSet);

                            Cube oServerCube = targetDB.Cubes.Find(oCube.ID);
                            if (oServerCube == null)
                            {
                                throw new System.Exception(string.Format("The {0} cube is not yet deployed to the {1} server.", oCube.Name, deploySet.TargetServer));
                            }
                            else if (oServerCube.State == AnalysisState.Unprocessed)
                            {
                                throw new System.Exception(string.Format("The {0} cube is not processed the {1} server.", oCube.Name, deploySet.TargetServer));
                            }
                            if (oServerCube.MdxScripts.Count == 0)
                            {
                                scr.ScriptAlter(new Microsoft.AnalysisServices.MajorObject[] { oServerCube }, xwScript, true);
                            }
                            else
                            {
                                MdxScript mdxScr = oServerCube.MdxScripts[0];
                                scr.ScriptAlter(new Microsoft.AnalysisServices.MajorObject[] { mdxScr }, xwScript, true);
                            }
                            xwScript.Close();

                            // update the MDX Script
                            XmlaResultCollection xmlaRC = svr.Execute(sb.ToString());
                            if (xmlaRC.Count == 1 && xmlaRC[0].Messages.Count == 0)
                            {
                                // all OK - 1 result - no messages    
                            }
                            else
                            {
                                    StringBuilder sbErr = new StringBuilder();
                                for (int iRC = 0; iRC < xmlaRC.Count;iRC ++)
                                {
                                    for (int iMsg = 0; iMsg < xmlaRC[iRC].Messages.Count; iMsg++)
                                    {
                                        sbErr.AppendLine(xmlaRC[iRC].Messages[iMsg].Description);
                                    }
                                }
                                MessageBox.Show(sbErr.ToString(),"BIDSHelper - Deploy MDX Script" );
                            }

                            
                            // Test the MDX Script
                            AdomdConnection cn = new AdomdConnection("Data Source=" + deploySet.TargetServer + ";Initial Catalog=" + deploySet.TargetDatabase);
                            cn.Open();
                            AdomdCommand cmd = cn.CreateCommand();
                            string qry = "SELECT {} ON 0 FROM [" + oCube.Name +"];";
                            cmd.CommandText = qry;
                            try
                            {
                                // test that we can query the cube without errors
                                cmd.Execute();

                                // Building the project means that the .asdatabase file gets re-built so that
                                // we do not break the Deployment Wizard.
                                // --
                                // This line is included in this try block so that it is only executed if we can
                                // successfully query the cube without errors.
                                projItem.DTE.Solution.SolutionBuild.BuildProject(projItem.DTE.Solution.SolutionBuild.ActiveConfiguration.Name, projItem.ContainingProject.UniqueName, false);

                            }
                            catch (System.Exception ex)
                            {
                                // undo the deployment if we caught an exception during the deployment
                                svr.Execute(sbBackup.ToString());
                                MessageBox.Show(ex.Message);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            if (MessageBox.Show("The following error occured while trying to deploy the MDX Script\r\n" 
                                                + ex.Message 
                                                + "\r\n\r\nDo you want to see a stack trace?"
                                            ,"BIDSHelper - Deploy MDX Script"
                                            , MessageBoxButtons.YesNo 
                                            , MessageBoxIcon.Error
                                            , MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            {
                                MessageBox.Show(ex.StackTrace);
                            }
                        }
                        finally
                        {
                            ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 5, 5);
                            // report any results back (status bar?)
                            svr.Disconnect();
                        }
                    }
                }
            }
            finally
            {
                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(false, "Deploying MdxScript", 5, 5);
            }
        }

    }
}