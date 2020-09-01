extern alias sharedDataWarehouseInterfaces;
extern alias asDataWarehouseInterfaces;
extern alias asAlias;
using EnvDTE;
using EnvDTE80;
using System.Text;
using Microsoft.DataWarehouse.Design;
using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.DataTransformationServices.Project;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project.Configuration;
using Microsoft.DataWarehouse.Project;
using Microsoft.DataWarehouse.VsIntegration.Shell;
using System.Windows.Forms;
using Microsoft.VisualStudio.CommandBars;
using BIDSHelper.Core;
using BIDSHelper.SSAS;

namespace BIDSHelper.SSIS
{
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class DeployPackagesPlugin : BIDSHelperPluginBase
    {
        private CommandBarButton cmdButtonProperties = null;
        private Guid guidForCustomPropertyFrame;

        public DeployPackagesPlugin(BIDSHelperPackage package)
            : base(package)
        {
            RegisterClassesForCOM();
            CaptureClickEventForProjectPropertiesMenu();
            CreateContextMenu(CommandList.DeploySSISPackageId); //don't mark as SSIS project type menu so ShouldDisplayCommand called
        }

        #region Standard Property Overrides
        public override string ShortName
        {
            get { return "DeployPackagesPlugin"; }
        }

        //public override int Bitmap
        //{
        //    get { return 1812; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        //public override string MenuName
        //{
        //    get { return "Item,Project,Solution"; }
        //}

        public override string FeatureName
        {
            get { return "Deploy SSIS Packages"; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Deploy SSIS packages directly from BIDS without the overhead of a deployment manifest or using the Package Installation Wizard."; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }
        #endregion


        public override bool ShouldDisplayCommand()
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            if (((System.Array)solExplorer.SelectedItems).Length == 1)
            {
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                //Project proj = hierItem.Object as Project;
                Project proj = this.GetSelectedProjectReference(true); //only return if project node selected
                SolutionClass solution = hierItem.Object as SolutionClass;
                if (proj != null)
                {
                    return (proj.Kind == BIDSProjectKinds.SSIS && IsLegacyDeploymentMode(proj));
                }
                else if (solution != null)
                {
                    foreach (Project p in solution.Projects)
                    {
                        if (p.Kind != BIDSProjectKinds.SSIS || !IsLegacyDeploymentMode(proj)) return false;
                    }
                    return (solution.Projects.Count > 0);
                }
                else
                {
                    if (!(hierItem.Object is ProjectItem)) return false;
                    proj = ((ProjectItem)hierItem.Object).ContainingProject;
                    string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                    return (sFileName.EndsWith(".dtsx") && IsLegacyDeploymentMode(proj));
                }
            }
            else
            {
                foreach (object selected in ((System.Array)solExplorer.SelectedItems))
                {
                    UIHierarchyItem hierItem = (UIHierarchyItem)selected;
                    if (!(hierItem.Object is ProjectItem)) return false;
                    Project proj = ((ProjectItem)hierItem.Object).ContainingProject;
                    string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                    if (!sFileName.EndsWith(".dtsx") || !IsLegacyDeploymentMode(proj)) return false;
                }
                return (((System.Array)solExplorer.SelectedItems).Length > 0);
            }
        }

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                Project pr = GetSelectedProjectReference(true); //only return if project node selected

                bool bJustDeploySelectedPackages = false;
                System.Collections.Generic.List<Project> projects = new System.Collections.Generic.List<Project>();
                if (pr != null)
                {
                    projects.Add(pr);
                }
                else if (hierItem.Object is Project)
                {
                    projects.Add((Project)hierItem.Object);
                }
                else if (hierItem.Object is SolutionClass)
                {
                    foreach (Project p in ((SolutionClass)hierItem.Object).Projects)
                    {
                        projects.Add(p);
                    }
                }
                else
                {
                    ProjectItem pi = (ProjectItem)hierItem.Object;
                    projects.Add(pi.ContainingProject);
                    bJustDeploySelectedPackages = true;
                }

                object settings = projects[0].GetIConfigurationSettings();
                if (settings == null)
                {
                    MessageBox.Show("Could not get IConfigurationSettings");
                    return;
                }
                DataWarehouseProjectManager projectManager = (DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);

                this.ApplicationObject.ToolWindows.OutputWindow.Parent.SetFocus();
                IOutputWindowFactory service = ((System.IServiceProvider)projects[0]).GetService(typeof(IOutputWindowFactory)) as IOutputWindowFactory;
                IOutputWindow outputWindow = service.GetStandardOutputWindow(StandardOutputWindow.Deploy);
                outputWindow.Activate();
                outputWindow.Clear();
                outputWindow.ReportStatusMessage("BIDS Helper is deploying packages...");
                this.ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                this.ApplicationObject.StatusBar.Text = "Deploying package(s)...";

                foreach (Project proj in projects)
                {
                    System.Array selectedItems; //holds packages to deploy

                    if (bJustDeploySelectedPackages)
                    {
                        selectedItems = ((System.Array)solExplorer.SelectedItems);
                    }
                    else
                    {
                        System.Collections.Generic.List<ProjectItem> list = new System.Collections.Generic.List<ProjectItem>(proj.ProjectItems.Count);
                        foreach (ProjectItem item in proj.ProjectItems)
                        {
                            list.Add(item);
                        }
                        selectedItems = list.ToArray();
                    }

                    PackageHelper.SetTargetServerVersion(proj); //cache the target server version before you use it
                    DeployProject(proj, outputWindow, selectedItems, !bJustDeploySelectedPackages);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                this.ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
                this.ApplicationObject.StatusBar.Text = string.Empty;
            }
        }

        private void DeployProject(Project proj, IOutputWindow outputWindow, System.Array selectedItems, bool bCreateBat)
        {
            object settings = proj.GetIConfigurationSettings();
            if (settings == null)
            {
                MessageBox.Show("Could not get IConfigurationSettings");
                return;
            }
            DataWarehouseProjectManager projectManager = (DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);

            StringBuilder sBatFileContents = new StringBuilder();
            try
            {
                string sConfigFileName = ((Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)proj).FullName + ".bidsHelper.user";
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                if (System.IO.File.Exists(sConfigFileName))
                {
                    doc.Load(sConfigFileName);
                }

#if !(YUKON || KATMAI || DENALI)
                //refreshes the cached target version which is needed in GetPathToDtsExecutable below
                SsisTargetServerVersion? projectTargetVersion = SSISHelpers.GetTargetServerVersion(proj);
#endif
                IProjectConfiguration config = projectManager.ConfigurationManager.CurrentConfiguration;
                DtsProjectExtendedConfigurationOptions newOptions = new DtsProjectExtendedConfigurationOptions();
                LoadFromBidsHelperConfiguration(doc, config.DisplayName, newOptions);

                //print out header in output window
                if (newOptions.DeploymentType == DtsProjectExtendedConfigurationOptions.DeploymentTypes.FilePathDestination)
                {
                    if (string.IsNullOrEmpty(newOptions.FilePath))
                    {
                        outputWindow.ReportStatusError(OutputWindowErrorSeverity.Error, "Deployment FilePath is not set. Right click on the project node and set the FilePath property.");
                        return;
                    }
                    outputWindow.ReportStatusMessage("Deploying to file path " + newOptions.FilePath + "\r\n");
                }
                else
                {
                    if (newOptions.DeploymentType == DtsProjectExtendedConfigurationOptions.DeploymentTypes.SsisPackageStoreMsdbDestination)
                    {
                        outputWindow.ReportStatusMessage("Deploying to SSIS Package Store MSDB on server " + newOptions.DestinationServer);
                    }
                    else if (newOptions.DeploymentType == DtsProjectExtendedConfigurationOptions.DeploymentTypes.SsisPackageStoreFileSystemDestination)
                    {
                        outputWindow.ReportStatusMessage("Deploying to SSIS Package Store File System on server " + newOptions.DestinationServer);
                    }
                    else if (newOptions.DeploymentType == DtsProjectExtendedConfigurationOptions.DeploymentTypes.SqlServerDestination)
                    {
                        outputWindow.ReportStatusMessage("Deploying to SQL Server MSDB on server: " + newOptions.DestinationServer);
                    }
                    if (!string.IsNullOrEmpty(newOptions.DestinationFolder))
                        outputWindow.ReportStatusMessage("Deploying to folder: " + newOptions.DestinationFolder);
                    outputWindow.ReportStatusMessage(string.Empty);
                }
                System.Windows.Forms.Application.DoEvents();

                //determine destination types and folders
                string sDestFolder = newOptions.DestinationFolder;
                sDestFolder = sDestFolder.Replace("/", "\\");
                if (!string.IsNullOrEmpty(sDestFolder) && !sDestFolder.EndsWith("\\")) sDestFolder += "\\";
                while (sDestFolder.StartsWith("\\"))
                    sDestFolder = sDestFolder.Substring(1);

                string sDestType = "SQL";
                if (newOptions.DeploymentType == DtsProjectExtendedConfigurationOptions.DeploymentTypes.SsisPackageStoreFileSystemDestination)
                {
                    sDestFolder = "File System\\" + sDestFolder;
                    sDestType = "DTS";
                }
                else if (newOptions.DeploymentType == DtsProjectExtendedConfigurationOptions.DeploymentTypes.SsisPackageStoreMsdbDestination)
                {
                    sDestFolder = "MSDB\\" + sDestFolder;
                    sDestType = "DTS";
                }
                else if (newOptions.DeploymentType == DtsProjectExtendedConfigurationOptions.DeploymentTypes.FilePathDestination)
                {
                    string sDestinationPath = newOptions.FilePath;
                    if (!sDestinationPath.EndsWith("\\")) sDestinationPath += "\\";
                    if (!System.IO.Directory.Exists(sDestinationPath))
                        System.IO.Directory.CreateDirectory(sDestinationPath);
                    sDestFolder = sDestinationPath;
                    sBatFileContents.Append("mkdir \"").Append(sDestFolder).AppendLine("\"");
                }

                //setup Process object to call the dtutil EXE
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = SSIS.PerformanceVisualization.PerformanceTab.GetPathToDtsExecutable("dtutil.exe", false); //makes the bat file less portable, but does workaround the problem if SSIS2005 and SSIS2008 are both installed... issue 21074

                if (string.IsNullOrEmpty(process.StartInfo.FileName))
                    throw new Exception("Can't find path to dtutil in registry! Please make sure you have the SSIS service installed from the " + PackageHelper.TargetServerVersion.ToString() + " install media");


                if (newOptions.DeploymentType != DtsProjectExtendedConfigurationOptions.DeploymentTypes.FilePathDestination)
                {
                    //create the directories
                    string sAccumulatingDir = "";
                    try
                    {
                        foreach (string dir in sDestFolder.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!(string.IsNullOrEmpty(sAccumulatingDir) && sDestType == "DTS"))
                            {
                                process.StartInfo.Arguments = string.Format("/FCreate {0};{1};\"{2}\" /SourceServer {3} ", sDestType, (sAccumulatingDir == "" ? "\\" : "\"" + sAccumulatingDir + "\""), dir, newOptions.DestinationServer);
                                sBatFileContents.Append("\"").Append(process.StartInfo.FileName).Append("\" ").AppendLine(process.StartInfo.Arguments);
                                process.Start();
                                process.WaitForExit();
                            }
                            if (!string.IsNullOrEmpty(sAccumulatingDir))
                                sAccumulatingDir += "\\";
                            sAccumulatingDir += dir;
                        }
                    }
                    catch { }
                }

                //loop through each package to deploy
                foreach (object selected in selectedItems)
                {
                    ProjectItem pi;
                    string sFileName;
                    string sFilePath;
                    if (selected is ProjectItem)
                    {
                        pi = (ProjectItem)selected;
                    }
                    else if (selected is UIHierarchyItem && ((UIHierarchyItem)selected).Object is ProjectItem)
                    {
                        pi = ((ProjectItem)((UIHierarchyItem)selected).Object);
                    }
                    else
                    {
                        continue;
                    }
                    sFileName = pi.Name;
                    sFilePath = pi.get_FileNames(0);
                    if (!sFileName.ToLower().EndsWith(".dtsx")) continue;

                    if (pi.Document != null && !pi.Document.Saved)
                    {
                        pi.Save("");
                    }

                    if (newOptions.DeploymentType == DtsProjectExtendedConfigurationOptions.DeploymentTypes.FilePathDestination)
                    {
                        string sDestinationPath = sDestFolder + sFileName;
                        if (System.IO.File.Exists(sDestinationPath))
                        {
                            System.IO.File.SetAttributes(sDestinationPath, System.IO.FileAttributes.Normal);
                        }
                        sBatFileContents.Append("xcopy \"").Append(sFilePath).Append("\" \"").Append(sDestFolder).AppendLine("\" /Y /R");
                        System.IO.File.Copy(sFilePath, sDestinationPath, true);
                        outputWindow.ReportStatusMessage("Deployed " + sFileName);
                    }
                    else
                    {
                        process.Refresh();
                        process.StartInfo.Arguments = string.Format("/FILE \"{0}\" /DestServer {1} /COPY {2};\"{3}\" /Q", sFilePath, newOptions.DestinationServer, sDestType, sDestFolder + sFileName.Substring(0, sFileName.Length - ".dtsx".Length));
                        sBatFileContents.Append("\"").Append(process.StartInfo.FileName).Append("\" ").AppendLine(process.StartInfo.Arguments);
                        process.Start();
                        string sError = process.StandardError.ReadToEnd();
                        string sStandardOutput = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        if (process.ExitCode > 0)
                        {
                            outputWindow.ReportStatusError(OutputWindowErrorSeverity.Error, "BIDS Helper encountered an error when deploying package " + sFileName + "!\r\n\"" + process.StartInfo.FileName + "\" " + process.StartInfo.Arguments + "\r\nexit code = " + process.ExitCode + "\r\n" + sStandardOutput);
                            this.ApplicationObject.ToolWindows.OutputWindow.Parent.AutoHides = false; //pin the window open so you can see the problem
                            return;
                        }
                        outputWindow.ReportStatusMessage("Deployed " + sFileName);
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                outputWindow.ReportStatusMessage("BIDS Helper completed deploying packages successfully.");

                if (bCreateBat)
                {
                    string sBatFilename = System.IO.Path.GetDirectoryName(((Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)proj).FullName);
                    sBatFilename += "\\" + newOptions.OutputPath;
                    if (!System.IO.Directory.Exists(sBatFilename))
                        System.IO.Directory.CreateDirectory(sBatFilename);
                    sBatFilename += "\\bidsHelperDeployPackages.bat";
                    if (System.IO.File.Exists(sBatFilename))
                    {
                        System.IO.File.SetAttributes(sBatFilename, System.IO.FileAttributes.Normal);
                    }
                    System.IO.File.WriteAllText(sBatFilename, sBatFileContents.ToString());
                    outputWindow.ReportStatusMessage("Deployment commands saved to: " + sBatFilename + "\r\n\r\n");
                }
            }
            catch (Exception ex)
            {
                outputWindow.ReportStatusError(OutputWindowErrorSeverity.Error, "BIDS Helper encountered an error when deploying packages:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                this.ApplicationObject.ToolWindows.OutputWindow.Parent.AutoHides = false; //pin the window open so you can see the problem
            }
        }

        #region Override Project Properties Dialog
        /// <summary>
        /// register DtsProjectExtendedDeployPropertyPage for COM. Must be registered every time Vis Studio starts up
        /// </summary>
        private void RegisterClassesForCOM()
        {
            try
            {
                RegistrationServices regSvc = new RegistrationServices();
                object[] attributes = typeof(DtsProjectExtendedDeployPropertyPage).GetCustomAttributes(typeof(GuidAttribute), false);
                if (attributes.Length == 0)
                    throw new Exception("Couldn't finding GuidAttribute on DtsProjectExtendedDeployPropertyPage");
                guidForCustomPropertyFrame = new Guid(((GuidAttribute)attributes[0]).Value);
                regSvc.RegisterTypeForComClients(typeof(DtsProjectExtendedDeployPropertyPage), ref guidForCustomPropertyFrame);
            }
            catch (Exception ex)
            {
                throw new Exception("Problem registering DtsProjectExtendedDeployPropertyPage for COM", ex);
            }
        }

        private void CaptureClickEventForProjectPropertiesMenu()
        {
            CommandBars cmdBars = (CommandBars)this.ApplicationObject.CommandBars;
            CommandBar pluginCmdBar = cmdBars["Project"];

            bool bSuccess = false;
            foreach (CommandBarControl cmd in pluginCmdBar.Controls)
            {
                int iID = 0;
                string sGuid = "";
                this.ApplicationObject.Commands.CommandInfo(cmd, out sGuid, out iID);
                Command cmd2 = this.ApplicationObject.Commands.Item(sGuid, iID);
                if (cmd2.Name == "ClassViewContextMenus.ClassViewProject.Properties"
                    || cmd2.Name == "ClassViewContextMenus.ClassViewMultiselectProjectreferencesItems.Properties"
                    || cmd.Id == (int)BIDSToolbarButtonID.ProjectProperties
                    || cmd.Id == (int)BIDSToolbarButtonID.ProjectPropertiesAlternate)
                {
                    cmdButtonProperties = cmd as CommandBarButton; //must save to a member variable of the class or the event won't fire later
                    cmdButtonProperties.Click += new _CommandBarButtonEvents_ClickEventHandler(cmdButtonProperties_Click);
                    bSuccess = true;
                    break;
                }
            }

            if (!bSuccess)
            {
                foreach (CommandBarControl cmd in pluginCmdBar.Controls)
                {
                    int iID = 0;
                    string sGuid = "";
                    this.ApplicationObject.Commands.CommandInfo(cmd, out sGuid, out iID);
                    Command cmd2 = this.ApplicationObject.Commands.Item(sGuid, iID);
                    if (cmd2.Name.EndsWith(".Properties"))
                    {
                        cmdButtonProperties = cmd as CommandBarButton; //must save to a member variable of the class or the event won't fire later
                        cmdButtonProperties.Click += new _CommandBarButtonEvents_ClickEventHandler(cmdButtonProperties_Click);
                        bSuccess = true;
                        break;
                    }
                }
            }

            if (!bSuccess)
            {
                foreach (CommandBarControl cmd in pluginCmdBar.Controls)
                {
                    if (cmd.Caption.Replace("&", string.Empty) == "Properties")
                    {
                        cmdButtonProperties = cmd as CommandBarButton; //must save to a member variable of the class or the event won't fire later
                        cmdButtonProperties.Click += new _CommandBarButtonEvents_ClickEventHandler(cmdButtonProperties_Click);
                        bSuccess = true;
                        break;
                    }
                }
            }

            if (!bSuccess)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("cmd.Caption").Append('|').Append("cmd.Id").Append('|').Append("cmd2.Name").Append('|').Append("cmd2.LocalizedName").Append('|').Append("cmd2.Guid").Append('|').Append("cmd2.ID").AppendLine();
                foreach (CommandBarControl cmd in pluginCmdBar.Controls)
                {
                    int iID = 0;
                    string sGuid = "";
                    this.ApplicationObject.Commands.CommandInfo(cmd, out sGuid, out iID);
                    Command cmd2 = this.ApplicationObject.Commands.Item(sGuid, iID);

                    sb.Append(cmd.Caption).Append('|').Append(cmd.Id).Append('|').Append(cmd2.Name).Append('|').Append(cmd2.LocalizedName).Append('|').Append(cmd2.Guid).Append('|').Append(cmd2.ID).AppendLine();
                }
                System.IO.File.WriteAllText(Microsoft.VisualBasic.FileIO.SpecialDirectories.Temp + "\\BidsHelperDeploySSISPackagesPropertiesMenuDebugLog.txt", sb.ToString());

                sb = new StringBuilder();
                sb.Append("bar.Name").Append('|').Append("bar.NameLocal").Append('|').Append("bar.Visible").Append('|').Append("bar.Type").Append('|').Append("bar.Id").Append('|').Append("bar.Index").Append('|').Append("bar.InstanceId").Append('|').Append("bar.Enabled").AppendLine();
                foreach (CommandBar bar in cmdBars)
                {
                    sb.Append(bar.Name).Append('|').Append(bar.NameLocal).Append('|').Append(bar.Visible).Append('|').Append(bar.Type.ToString()).Append('|').Append(bar.Id).Append('|').Append(bar.Index).Append('|').Append(bar.InstanceId).Append('|').Append(bar.Enabled).AppendLine();
                }
                System.IO.File.WriteAllText(Microsoft.VisualBasic.FileIO.SpecialDirectories.Temp + "\\BidsHelperDeploySSISPackagesMenuDebugLog.txt", sb.ToString());
            }
        }

        private void cmdButtonProperties_Click(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            if (Enabled)
            {
                try
                {
                    UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                    if (((System.Array)solExplorer.SelectedItems).Length != 1)
                        return;

                    Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt proj = GetSelectedProjectReference();

                    if (proj == null || proj.Kind != BIDSProjectKinds.SSIS)
                    {
                        CancelDefault = false; //let the Microsoft code fire
                        return;
                    }

                    object settings = proj.GetIConfigurationSettings();
                    if (settings == null)
                    {
                        CancelDefault = false; //let the Microsoft code fire
                        return;
                    }

                    projectManager = (Microsoft.DataWarehouse.Project.DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);

                    if (!IsLegacyDeploymentMode(projectManager))
                    {
                        //new project deployment mode
                        CancelDefault = false; //let the Microsoft code fire
                        return;
                    }
                    else
                    {
                        CancelDefault = true; //don't let the Microsoft code fire as I'm going to pop up the dialog myself
                    }

                    string sFileName = projectManager.GetSelectedProjectNode().FullPath + ".bidsHelper.user";
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    if (System.IO.File.Exists(sFileName))
                    {
                        doc.Load(sFileName);
                    }

                    ConfigurationManager = projectManager.ConfigurationManager;
                    foreach (IProjectConfiguration config in projectManager.ConfigurationManager.Configurations)
                    {
                        DtsProjectExtendedConfigurationOptions newOptions = new DtsProjectExtendedConfigurationOptions((DataTransformationsProjectConfigurationOptions)config.Options);
                        LoadFromBidsHelperConfiguration(doc, config.DisplayName, newOptions);

                        if (!(config.Options is DtsProjectExtendedConfigurationOptions))
                        {
                            config.Options = newOptions; //override the Options object in memory so the configuration properties dialog will show our dialog
                        }
                    }

                    //pop up the configuration properties dialog
                    IVsPropertyPageFrame frame = (IVsPropertyPageFrame)((System.IServiceProvider)proj).GetService(typeof(SVsPropertyPageFrame));
                    int hr = frame.ShowFrame(guidForCustomPropertyFrame);
                    if (hr < 0)
                    {
                        frame.ReportError(hr);
                        MessageBox.Show("Could not open BIDS Helper properties window customizations.");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                    CancelDefault = false; //let the Microsoft code fire
                }
            }

        }


        /// <summary>
        /// Determines whether the project users the legacy Package deployment model, as opposed to the newer Project deployment model.
        /// </summary>
        /// <param name="project">The project to check.</param>
        /// <returns>true if the project uses the legacy package deployment model; otherwise else false.</returns>
        public static bool IsLegacyDeploymentMode(Project project)
        {
            object settings = project.GetIConfigurationSettings();
            if (settings == null) return false;
            DataWarehouseProjectManager projectManager = (DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);
            return IsLegacyDeploymentMode(projectManager);
        }

        /// <summary>
        /// Determines whether the project users the legacy Package deployment model, as opposed to the newer Project deployment model.
        /// </summary>
        /// <param name="projectManager">The project to check.</param>
        /// <returns>true if the project uses the legacy package deployment model; otherwise else false.</returns>
        public static bool IsLegacyDeploymentMode(DataWarehouseProjectManager projectManager)
        {
            Microsoft.DataTransformationServices.Design.Project.IObjectModelProjectManager manager = projectManager as Microsoft.DataTransformationServices.Design.Project.IObjectModelProjectManager;
            if ((manager != null) && (manager.ObjectModelProject != null))
            {
                //new project deployment mode
                return false;
            }
            else
            {
                return true;
            }
        }

        private void LoadFromBidsHelperConfiguration(System.Xml.XmlDocument doc, string sConfigurationName, DtsProjectExtendedConfigurationOptions newOptions)
        {
            System.Xml.XmlNode nodeOptions = doc.SelectSingleNode("/Configurations/Configuration/Name[text()='" + sConfigurationName.Replace("'", "&apos;") + "']/../Options");
            if (nodeOptions != null)
            {
                System.Xml.XmlNode node;
                node = nodeOptions.SelectSingleNode("DeploymentType");
                try
                {
                    if (node != null)
                        newOptions.DeploymentType = (DtsProjectExtendedConfigurationOptions.DeploymentTypes)System.Enum.Parse(typeof(DtsProjectExtendedConfigurationOptions.DeploymentTypes), node.InnerText);
                }
                catch { }

                node = nodeOptions.SelectSingleNode("FilePath");
                if (node != null)
                    newOptions.FilePath = node.InnerText;
                node = nodeOptions.SelectSingleNode("DestinationServer");
                if (node != null)
                    newOptions.DestinationServer = node.InnerText;
                node = nodeOptions.SelectSingleNode("DestinationFolder");
                if (node != null)
                    newOptions.DestinationFolder = node.InnerText;
            }
        }


        private static DataWarehouseProjectManager projectManager = null;
        private static IProjectConfigurationManager ConfigurationManager = null;
        public static void ResetConfigurations()
        {
            foreach (IProjectConfiguration config in ConfigurationManager.Configurations)
            {
                DtsProjectExtendedConfigurationOptions newConfig = config.Options as DtsProjectExtendedConfigurationOptions;
                while (newConfig != null)
                {
                    config.Options = newConfig.GetBaseType();
                    newConfig = config.Options as DtsProjectExtendedConfigurationOptions;
                    System.Diagnostics.Debug.WriteLine("rolling back to plain project config options for " + config.DisplayName);
                }
            }
        }

        public static void SaveConfigurations()
        {
            //serialize the old fashioned way (because I can't serialize it using the normal way because that would pick up all the properties from the Microsoft base classes)
            System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(projectManager.GetSelectedProjectNode().FullPath + ".bidsHelper.user", Encoding.ASCII);
            writer.Formatting = System.Xml.Formatting.Indented;
            writer.WriteStartElement("Configurations");
            foreach (IProjectConfiguration config in ConfigurationManager.Configurations)
            {
                DtsProjectExtendedConfigurationOptions options = config.Options as DtsProjectExtendedConfigurationOptions;
                if (options != null)
                {
                    writer.WriteStartElement("Configuration");
                    writer.WriteElementString("Name", config.DisplayName);
                    writer.WriteStartElement("Options");
                    writer.WriteElementString("DeploymentType", options.DeploymentType.ToString());
                    writer.WriteElementString("FilePath", options.FilePath);
                    writer.WriteElementString("DestinationServer", options.DestinationServer);
                    writer.WriteElementString("DestinationFolder", options.DestinationFolder);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
            writer.Close();

            //mark project file as dirty
            projectManager.GetType().InvokeMember("MarkTextBufferAsUnsaved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.InvokeMethod, null, projectManager, new object[] { });
        }



        /// <summary>
        /// A subclassed version of the Microsoft's SSIS project configuration options class.
        /// This class will only be temporarily added to the configuration manager so that the project properties dialog can show it's properties.
        /// Immediately after closing the project properties dialog, it must be removed as it cannot be serialized with Microsoft's serialization code without causing problems.
        /// </summary>
        [DisplayableByPropertyPage(new Type[] { 
#if !(YUKON || KATMAI || DENALI)
            typeof(DtsGeneralPropertyPage),
#endif
            typeof(DataTransformationsBuildPropertyPage), 
            typeof(DataTransformationsDeploymentUtilityPropertyPage), 
            typeof(DebugPropertyPage), 
            typeof(DtsProjectExtendedDeployPropertyPage) })]
        public class DtsProjectExtendedConfigurationOptions : DataTransformationsProjectConfigurationOptions
#if !(YUKON || KATMAI)
            , ICustomTypeDescriptor
#endif
        {
            private DataTransformationsProjectConfigurationOptions oldConfig;
            public DtsProjectExtendedConfigurationOptions() : base() { }

            public DtsProjectExtendedConfigurationOptions(DataTransformationsProjectConfigurationOptions old)
            {
                try
                {
                    oldConfig = old;
                    Type type = old.GetType();
                    foreach (System.Reflection.PropertyInfo info in type.GetProperties())
                    {
                        if (info.GetSetMethod() != null)
                        {
                            object obj3 = info.GetValue(old, null);
                            if (obj3 is ICloneable)
                            {
                                obj3 = ((ICloneable)obj3).Clone();
                            }
                            this.GetType().InvokeMember(info.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, this, new object[] { info.GetGetMethod().Invoke(old, null) });
                        }
                    }

                    this.Disposed += new EventHandler(DtsProjectExtendedConfigurationOptions_Disposed);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem opening configurations properties dialog.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
            }

            void DtsProjectExtendedConfigurationOptions_Disposed(object sender, EventArgs e)
            {
                try
                {
                    DeployPackagesPlugin.ResetConfigurations();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem closing configurations properties dialog. Please restart Visual Studio.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
            }

            public DataTransformationsProjectConfigurationOptions GetBaseType()
            {
                DataTransformationsProjectConfigurationOptions basetype = new DataTransformationsProjectConfigurationOptions();
                Type type = typeof(DataTransformationsProjectConfigurationOptions);
                foreach (System.Reflection.PropertyInfo info in type.GetProperties())
                {
                    if (info.GetSetMethod() != null)
                    {
                        object obj3 = info.GetValue(this, null);
                        if (obj3 is ICloneable)
                        {
                            obj3 = ((ICloneable)obj3).Clone();
                        }
                        info.SetValue(basetype, obj3, null);
                    }
                }
                return basetype;
            }

            public enum DeploymentTypes
            {
                FilePathDestination,
                SsisPackageStoreFileSystemDestination,
                SsisPackageStoreMsdbDestination,
                SqlServerDestination
            }

            private DeploymentTypes _DeploymentType = DeploymentTypes.FilePathDestination;
            [Browsable(true),
            Description("Determines how the packages are deployed by BIDS Helper."),
            Category("Deployment"),
            DisplayableByPropertyPage(typeof(DtsProjectExtendedDeployPropertyPage)),
            DefaultValue(DeploymentTypes.FilePathDestination)]
            public DeploymentTypes DeploymentType
            {
                get
                {
                    return _DeploymentType;
                }
                set
                {
                    _DeploymentType = value;
                    if (value == DeploymentTypes.FilePathDestination)
                    {
                        ExtraPropertiesPlugin.SetAttribute(this, "FilePath", new BrowsableAttribute(true), true);
                        ExtraPropertiesPlugin.SetAttribute(this, "DestinationServer", new BrowsableAttribute(false), true);
                        ExtraPropertiesPlugin.SetAttribute(this, "DestinationFolder", new BrowsableAttribute(false), true);
                    }
                    else
                    {
                        ExtraPropertiesPlugin.SetAttribute(this, "FilePath", new BrowsableAttribute(false), true);
                        ExtraPropertiesPlugin.SetAttribute(this, "DestinationServer", new BrowsableAttribute(true), true);
                        ExtraPropertiesPlugin.SetAttribute(this, "DestinationFolder", new BrowsableAttribute(true), true);
                    }
                    TypeDescriptor.Refresh(this);
                }
            }

            private string _FilePath = "";
            [Browsable(true),
            Description("Determines the directory the packages are file copied to. Can be in the form of a local path (i.e. c:\\directory) or a UNC path (i.e. \\\\server\\share)."),
            Category("Deployment"),
            DisplayableByPropertyPage(typeof(DtsProjectExtendedDeployPropertyPage)),
            DefaultValue("")]
            public string FilePath
            {
                get
                {
                    return _FilePath;
                }
                set
                {
                    _FilePath = value;
                }
            }

            private string _DestinationServer = "localhost";
            [Browsable(false),
            Description("Determines the destination server name where the packages are copied to. Can be in the form of a simple server name (i.e. localhost) or an instance (i.e. localhost\\yukon)."),
            Category("Deployment"),
            DisplayableByPropertyPage(typeof(DtsProjectExtendedDeployPropertyPage)),
            DefaultValue("localhost")]
            public string DestinationServer
            {
                get
                {
                    return _DestinationServer;
                }
                set
                {
                    _DestinationServer = value;
                }
            }

            private string _DestinationFolder = "";
            [Browsable(false),
            Description("Determines the destination folder name where the packages are copied to. Should be in the form \"FolderName\" or \"FolderName\\SubFolderName\"."),
            Category("Deployment"),
            DisplayableByPropertyPage(typeof(DtsProjectExtendedDeployPropertyPage)),
            DefaultValue("")]
            public string DestinationFolder
            {
                get
                {
                    return _DestinationFolder;
                }
                set
                {
                    _DestinationFolder = value;
                }
            }




#if !(YUKON || KATMAI) //in Denali they changed the DataTransformationsProjectConfigurationOptions class to implement ICustomTypeDescriptor so that it could conditionally show different project properties panes dependent on whether we're in project deployment mode or legacy deployment mode

#pragma warning disable //disable warning that I'm hiding the GetAttributes method in the base class
            public AttributeCollection GetAttributes()
            {
                try
                {
                    if (!DeployPackagesPlugin.IsLegacyDeploymentMode(this.Manager))
                    {
                        //new project deployment mode
                        throw new Exception("Should not be opening this BIDS Helper custom project properties dialog in a new project deployment mode project.");
                    }
                    else
                    {
                        //AttributeCollection coll = base.GetAttributes();
                        //Attribute[] attrs = new Attribute[coll.Count];
                        //for (int i=0; i < coll.Count; i++)
                        //{
                        //    Attribute a = coll[i];
                        //    attrs[i] = a;
                        //    DisplayableByPropertyPageAttribute pageAttr = a as DisplayableByPropertyPageAttribute;
                        //    if (pageAttr == null) continue;
                        //    System.Collections.Generic.List<Type> types = new System.Collections.Generic.List<Type>(pageAttr.PropertyPageTypes);
                        //    types.Add(typeof(DtsProjectExtendedDeployPropertyPage));
                        //    DisplayableByPropertyPageAttribute pageAttrNew = new DisplayableByPropertyPageAttribute(types.ToArray());
                        //    attrs[i] = pageAttrNew;
                        //}
                        //return new AttributeCollection(attrs);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem opening configurations properties dialog.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    throw ex;
                }
            }

            public string GetClassName()
            {
                return TypeDescriptor.GetClassName(this, true);
            }

            public string GetComponentName()
            {
                return TypeDescriptor.GetComponentName(this, true);
            }

            public TypeConverter GetConverter()
            {
                return TypeDescriptor.GetConverter(this, true);
            }

            public EventDescriptor GetDefaultEvent()
            {
                return TypeDescriptor.GetDefaultEvent(this, true);
            }

            public PropertyDescriptor GetDefaultProperty()
            {
                return TypeDescriptor.GetDefaultProperty(this, true);
            }

            public object GetEditor(Type editorBaseType)
            {
                return TypeDescriptor.GetEditor(this, editorBaseType, true);
            }

            public EventDescriptorCollection GetEvents()
            {
                return TypeDescriptor.GetEvents(this, true);
            }

            public EventDescriptorCollection GetEvents(Attribute[] attributes)
            {
                return TypeDescriptor.GetEvents(this, attributes, true);
            }

            public PropertyDescriptorCollection GetProperties()
            {
                return TypeDescriptor.GetProperties(this, true);
            }

            public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(this, attributes, true);
            }

            public object GetPropertyOwner(PropertyDescriptor pd)
            {
                return this;
            }
#pragma warning restore
#endif



        }
        #endregion

    }

    [Guid("BAB0643E-D93A-11DC-9304-0A8755D89593"), ComVisible(true), ClassInterface(ClassInterfaceType.None), Microsoft.DataWarehouse.VsIntegration.Shell.PropertyPageInfo("Deploy (BIDS Helper)")]
    public class DtsProjectExtendedDeployPropertyPage : Microsoft.DataWarehouse.VsIntegration.Shell.PropertyGridPagePane
    {
        private DeployPackagesPlugin.DtsProjectExtendedConfigurationOptions options = null;
        private PropertyGrid propertyGrid = null;
        public DtsProjectExtendedDeployPropertyPage()
        {
        }

        protected override IWin32Window CreateWindow()
        {
            this.propertyGrid = (PropertyGrid)base.CreateWindow();
            this.propertyGrid.ToolbarVisible = true;
            this.propertyGrid.PropertySort = PropertySort.Categorized;
            Console.WriteLine(this.propertyGrid.SelectedObjects.Length);
            if (this.propertyGrid.SelectedObjects.Length == 1)
            {
                Microsoft.DataWarehouse.VsIntegration.Designer.AttributedPropertiesTypeDescriptor descriptor = (Microsoft.DataWarehouse.VsIntegration.Designer.AttributedPropertiesTypeDescriptor)this.propertyGrid.SelectedObjects[0];
                options = (DeployPackagesPlugin.DtsProjectExtendedConfigurationOptions)descriptor.GetType().InvokeMember("SelectedObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly, null, descriptor, null);
            }
            this.propertyGrid.Disposed += new EventHandler(propertyGrid_Disposed);
            TypeDescriptor.Refreshed += new RefreshEventHandler(TypeDescriptor_Refreshed); //changing the DeploymentType causes different properties to be visible
            return this.propertyGrid;
        }

        void TypeDescriptor_Refreshed(RefreshEventArgs e)
        {
            this.propertyGrid.Refresh();
        }

        void propertyGrid_Disposed(object sender, EventArgs e)
        {
            try
            {
                DeployPackagesPlugin.ResetConfigurations();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem closing configurations properties dialog. Please restart Visual Studio.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        protected override void ApplyChanges()
        {
            try
            {
                base.ApplyChanges();
                DeployPackagesPlugin.SaveConfigurations();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem saving BIDS Helper configurations properties dialog.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }

}
