#if SQL2019
extern alias asAlias;
using asAlias.Microsoft.DataWarehouse.Controls;
using asAlias.Microsoft.DataWarehouse.Design;
using asAlias::Microsoft.DataWarehouse.ComponentModel;
#else
using Microsoft.DataWarehouse.Controls;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.ComponentModel;
#endif
using EnvDTE;
using EnvDTE80;
using System.Xml;
using System.Xml.Xsl;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.AnalysisServices;
using System.ComponentModel.Design;
//using Microsoft.DataWarehouse.Design;
//using Microsoft.DataWarehouse.Controls;
using System;
using BIDSHelper.SSAS;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class DeployPerspectivesPlugin : BIDSHelperWindowActivatedPluginBase
    {
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private ToolBarButton newDeployPerspectivesButton = null;
        private ToolBarButton newSeparatorButton = null;
        private System.Collections.Generic.Dictionary<string, EditorWindow> windowHandlesFixedDeployPerspectives = new System.Collections.Generic.Dictionary<string, EditorWindow>();

        public DeployPerspectivesPlugin(BIDSHelperPackage package)
            : base(package)
        {
        }

        public override string ShortName
        {
            get { return "DeployPerspectives"; }
        }

        public override string FeatureName
        {
            get
            {
                return "Deploy the perspectives for a cube";
            }
        }

        public override string ToolTip
        {
            get { return "Deploys just the perspectives for this cube"; }
        }

        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        public override string HelpUrl
        {
            get { return this.GetCodePlexHelpUrl("Deploy Perspectives"); }
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
            get { return "Allows you to deploy just the perspectives."; }
        }




        public override void OnDisable()
        {
            base.OnDisable();

            foreach (EditorWindow win in windowHandlesFixedDeployPerspectives.Values)
            {
                win.ActiveViewChanged -= win_ActiveViewChanged;

                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);
                if (toolbar != null)
                {
                    toolbar.ButtonClick -= toolbar_ButtonClick;

                    if (newDeployPerspectivesButton != null)
                    {
                        if (toolbar.Buttons.ContainsKey(newDeployPerspectivesButton.Name))
                        { toolbar.Buttons.RemoveByKey(newDeployPerspectivesButton.Name); }

                        if (newSeparatorButton != null)
                        {
                            if (toolbar.Buttons.ContainsKey(newSeparatorButton.Name))
                            { toolbar.Buttons.RemoveByKey(newSeparatorButton.Name); }
                        }
                    }
                }
            }
        }

        public override bool ShouldHookWindowCreated
        {
            get
            { return true; }
        }

        void windowEvents_WindowCreated(Window Window)
        {
            OnWindowActivated(Window, null);
        }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus == null) return;
                IDesignerHost designer = GotFocus.Object as IDesignerHost;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if ((pi == null) || (!(pi.Object is Cube))) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(IComponentNavigator));
                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);

                IntPtr ptr = win.Handle;
                string sHandle = ptr.ToInt64().ToString();

                if (!windowHandlesFixedDeployPerspectives.ContainsKey(sHandle))
                {
                    windowHandlesFixedDeployPerspectives.Add(sHandle, win);
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                }

                if (pi.Name.ToLower().EndsWith(".cube") && win.SelectedView.MenuItemCommandID.ID == 12904) //language neutral way of saying win.SelectedView.Caption == "Perspectives"
                {
                    if (!toolbar.Buttons.ContainsKey(this.FullName + ".DeployPerspectives"))
                    {
                        newSeparatorButton = new ToolBarButton();
                        newSeparatorButton.Name = this.FullName + ".Separator";
                        newSeparatorButton.Style = ToolBarButtonStyle.Separator;

                        if (BIDSHelperPackage.Plugins[DeployMdxScriptPlugin.BaseName + typeof(DeployPerspectivesPlugin).Name].Enabled)
                        {
                            toolbar.ImageList.Images.Add(BIDSHelper.Resources.Common.DeployPerspectivesIcon);
                            newDeployPerspectivesButton = new ToolBarButton();
                            newDeployPerspectivesButton.ToolTipText = "Deploy Perspectives (BIDS Helper)";
                            newDeployPerspectivesButton.Name = this.FullName + ".DeployPerspectives";
                            newDeployPerspectivesButton.Tag = newDeployPerspectivesButton.Name;
                            newDeployPerspectivesButton.ImageIndex = toolbar.ImageList.Images.Count - 1;
                            newDeployPerspectivesButton.Enabled = true;
                            newDeployPerspectivesButton.Style = ToolBarButtonStyle.PushButton;

                            toolbar.Buttons.Add(newSeparatorButton);
                            toolbar.Buttons.Add(newDeployPerspectivesButton);

                            //catch the button clicks of the new buttons we just added
                            toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);
                        }
                    }
                }
            }
            catch { }
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
        }


        void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button.Tag != null)
                {
                    string sButtonTag = e.Button.Tag.ToString();
                    if (sButtonTag == this.FullName + ".DeployPerspectives")
                    {
                        IDesignerHost designer = (IDesignerHost)ApplicationObject.ActiveWindow.Object;
                        if (designer == null) return;
                        ProjectItem pi = ApplicationObject.ActiveWindow.ProjectItem;
                        DeployPerspectives(pi, this.ApplicationObject);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public override void Exec()
        {
        }

        public static void DeployPerspectives(ProjectItem projItem, DTE2 ApplicationObject)
        {
            Microsoft.AnalysisServices.Cube oCube = (Microsoft.AnalysisServices.Cube)projItem.Object;

            if (oCube.Perspectives.Count == 0)
            {
                MessageBox.Show("There are no perspectives defined in this cube yet.");
                return;
            }

            try
            {
                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(true, "Deploying perspectives", 1, 5);

                FileAttributes fa = System.IO.File.GetAttributes(projItem.get_FileNames(1));
                if ((fa & FileAttributes.ReadOnly) != FileAttributes.ReadOnly )
                {
                    //Save the cube
                    projItem.Save("");
                }

                ApplicationObject.StatusBar.Progress(true, "Deploying perspectives", 2, 5);

                // extract deployment information
                DeploymentSettings deploySet = new DeploymentSettings(projItem);

                // use xlst to create xmla alter command
                XslCompiledTransform xslt = new XslCompiledTransform();
                XmlReader xsltRdr;
                XmlReader xrdr;

                // read xslt from embedded resource
                xsltRdr = XmlReader.Create(new StringReader(BIDSHelper.Resources.Common.DeployPerspectives));
                using ((xsltRdr))
                {
                    // read content from .cube file
                    xrdr = XmlReader.Create(projItem.get_FileNames(1));
                    using (xrdr)
                    {


                        
                        ApplicationObject.StatusBar.Progress(true, "Deploying perspectives", 3, 5);
                        // Connect to Analysis Services
                        Microsoft.AnalysisServices.Server svr = new Microsoft.AnalysisServices.Server();
                        svr.Connect(deploySet.TargetServer);
                        ApplicationObject.StatusBar.Progress(true, "Deploying perspectives", 4, 5);
                        // execute the xmla
                        try
                        {
                            // Build up the Alter perspectives command using XSLT against the .cube file
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

                            Cube oServerCube = targetDB.Cubes.Find(oCube.ID);
                            if (oServerCube == null)
                            {
                                throw new System.Exception(string.Format("The {0} cube is not yet deployed to the {1} server.", oCube.Name, deploySet.TargetServer));
                            }

                            //drop any perspectives which don't exist
                            svr.CaptureXml = true;
                            for (int i = 0; i < oServerCube.Perspectives.Count; i++)
                            {
                                Perspective p = oServerCube.Perspectives[i];
                                if (!oCube.Perspectives.Contains(p.ID))
                                {
                                    p.Drop();
                                    i--;
                                }
                            }
                            svr.CaptureXml = false;
                            try
                            {
                                if (svr.CaptureLog.Count > 0)
                                    svr.ExecuteCaptureLog(true, false);
                            }
                            catch (System.Exception ex)
                            {
                                throw new System.Exception("Error dropping perspective that were deleted in the source code. " + ex.Message);
                            }


                            // update the perspectives
                            XmlaResultCollection xmlaRC = svr.Execute(sb.ToString());

                            StringBuilder sbErr = new StringBuilder();
                            for (int iRC = 0; iRC < xmlaRC.Count; iRC++)
                            {
                                for (int iMsg = 0; iMsg < xmlaRC[iRC].Messages.Count; iMsg++)
                                {
                                    sbErr.AppendLine(xmlaRC[iRC].Messages[iMsg].Description);
                                }
                            }
                            if (sbErr.Length > 0)
                            {
                                MessageBox.Show(sbErr.ToString(), "BIDSHelper - Deploy Perspectives");
                            }



                            try
                            {
                                // Building the project means that the .asdatabase file gets re-built so that
                                // we do not break the Deployment Wizard.
                                projItem.DTE.Solution.SolutionBuild.BuildProject(projItem.DTE.Solution.SolutionBuild.ActiveConfiguration.Name, projItem.ContainingProject.UniqueName, false);

                            }
                            catch (System.Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            if (MessageBox.Show("The following error occured while trying to deploy the perspectives\r\n"
                                                + ex.Message
                                                + "\r\n\r\nDo you want to see a stack trace?"
                                            , "BIDS Helper - Deploy Perspectives"
                                            , MessageBoxButtons.YesNo
                                            , MessageBoxIcon.Error
                                            , MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            {
                                MessageBox.Show(ex.StackTrace);
                            }
                        }
                        finally
                        {
                            ApplicationObject.StatusBar.Progress(true, "Deploying perspectives", 5, 5);
                            // report any results back (status bar?)
                            svr.Disconnect();
                        }
                    }
                }
            }
            finally
            {
                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(false, "Deploying perspectives", 5, 5);
            }
        }

    }
}