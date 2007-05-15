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

                //\\ extract deployment information
                DeploymentSettings deploySet = new DeploymentSettings(projItem);

                //\\ use xlst to create xmla alter command
                XslCompiledTransform xslt = new XslCompiledTransform();
                XmlReader xsltRdr;
                XmlReader xrdr;

                //\\ read xslt from embedded resource
                xsltRdr = XmlReader.Create(new StringReader(Properties.Resources.DeployMdxScript));
                using ((xsltRdr))
                {
                    //\\ read content from .cube file
                    xrdr = XmlReader.Create(projItem.get_FileNames(1));
                    using (xrdr)
                    {
                        //\\ Build up the Alter MdxScript command using XSLT against the .cube file
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
                        //\\ Connect to Analysis Services
                        Microsoft.AnalysisServices.Server svr = new Microsoft.AnalysisServices.Server();
                        svr.Connect(deploySet.TargetServer);
                        ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 4, 5);
                        //\\ execute the xmla
                        svr.Execute(sb.ToString());
                        ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 5, 5);
                        //\\ report any results back (status bar?)
                        svr.Disconnect();

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