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

        public DeployMDXScriptPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "DeployMdxScript"; }
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
                xsltRdr = XmlReader.Create(new StringReader(Properties.Resources.DeployMdxScript));
                using ((xsltRdr))
                {
                    // read content from .cube file
                    xrdr = XmlReader.Create(projItem.get_FileNames(1));
                    using (xrdr)
                    {
                        // Build up the Alter MdxScript command using XSLT against the .cube file
                        XslCompiledTransform xslta = new XslCompiledTransform();
                        StringBuilder sb = new StringBuilder();
                        XmlWriterSettings xws = new XmlWriterSettings();
                        xws.OmitXmlDeclaration = true;
                        XmlWriter xwrtr = XmlWriter.Create(sb, xws);
                        
                        xslta.Load(xsltRdr);
                        XsltArgumentList xslarg = new XsltArgumentList();
                        xslarg.AddParam("TargetDatabase", "", deploySet.TargetDatabase);
                        xslta.Transform(xrdr, xslarg, xwrtr);
                        System.Diagnostics.Debug.Print(sb.ToString());
                        
                        ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 3, 5);
                        // Connect to Analysis Services
                        Microsoft.AnalysisServices.Server svr = new Microsoft.AnalysisServices.Server();
                        svr.Connect(deploySet.TargetServer);
                        ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 4, 5);
                        // execute the xmla
                        try
                        {
                            Microsoft.AnalysisServices.Scripter scr = new Microsoft.AnalysisServices.Scripter();
                            
                            StringBuilder sbBackup = new StringBuilder();
                            XmlWriterSettings xwSet = new XmlWriterSettings();
                            xwSet.ConformanceLevel = ConformanceLevel.Fragment;
                            xwSet.Indent = true;
                            XmlWriter xwScript = XmlWriter.Create(sbBackup,xwSet);

                            MdxScript mdxScr = svr.Databases.GetByName(deploySet.TargetDatabase).Cubes.Find(oCube.ID).MdxScripts[0];
                            scr.ScriptAlter(new Microsoft.AnalysisServices.MajorObject[]{mdxScr},xwScript,true);
                            xwScript.Close();
                            // update the MDX Script
                            svr.Execute(sb.ToString());
                            // Test the MDX Script
                            AdomdConnection cn = new AdomdConnection("Data Source=" + deploySet.TargetServer + ";Initial Catalog=" + deploySet.TargetDatabase);
                            cn.Open();
                            AdomdCommand cmd = cn.CreateCommand();
                            string qry = "SELECT {measures.Members.Item(0)} ON 0 FROM [" + oCube.Name +"];";
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
                            if (MessageBox.Show(ex.Message + "\r\nDo you want to see a stack trace?", "", MessageBoxButtons.YesNo , MessageBoxIcon.Error, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            {
                                MessageBox.Show(ex.StackTrace);
                            }
                            svr.RollbackTransaction();
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