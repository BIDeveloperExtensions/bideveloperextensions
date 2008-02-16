using EnvDTE;
using EnvDTE80;
using System.Text;
using Microsoft.DataWarehouse.Design;
using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.DataTransformationServices.Project;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project.Configuration;
using Microsoft.DataWarehouse.Project;
using Microsoft.DataWarehouse.VsIntegration.Shell;
using System.Windows.Forms;
using Microsoft.VisualStudio.CommandBars;

namespace BIDSHelper
{
    public class DeployPackagesPlugin : BIDSHelperPluginBase
    {
        private const string SSIS_PROJECT_KIND = "{d183a3d8-5fd8-494b-b014-37f57b35e655}";
        private CommandBarButton cmdButtonProperties = null;

        public DeployPackagesPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
            CaptureClickEventForProjectPropertiesMenu();
        }

        #region Standard Property Overrides
        public override string ShortName
        {
            get { return "DeployPackagesPlugin"; }
        }

        public override int Bitmap
        {
            get { return 1812; }
        }

        public override string ButtonText
        {
            get { return "Deploy"; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }

        public override string MenuName
        {
            get { return "Item,Project"; }
        }

        public override string FriendlyName
        {
            get { return "Deploy SSIS Packages"; }
        }
        #endregion


        public override bool DisplayCommand(UIHierarchyItem item)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            if (((System.Array)solExplorer.SelectedItems).Length == 1)
            {
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                Project proj = hierItem.Object as Project;
                if (proj != null)
                {
                    return (proj.Kind == SSIS_PROJECT_KIND);
                }
                else
                {
                    string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                    return (sFileName.EndsWith(".dtsx"));
                }
            }
            else
            {
                bool bAllDtsx = true;
                foreach (object selected in ((System.Array)solExplorer.SelectedItems))
                {
                    UIHierarchyItem hierItem = (UIHierarchyItem)selected;
                    string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                    bAllDtsx = bAllDtsx && sFileName.EndsWith(".dtsx");
                }
                return bAllDtsx;
            }
        }

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                Project proj;
                if (hierItem.Object is Project)
                {
                    proj = (Project)hierItem.Object;
                }
                else
                {
                    ProjectItem pi = (ProjectItem)hierItem.Object;
                    proj = pi.ContainingProject;
                }
                Microsoft.DataWarehouse.Interfaces.IConfigurationSettings settings = (Microsoft.DataWarehouse.Interfaces.IConfigurationSettings)((System.IServiceProvider)proj).GetService(typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings));
                DataWarehouseProjectManager projectManager = (DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);

                this.ApplicationObject.ToolWindows.OutputWindow.Parent.SetFocus();
                IOutputWindowFactory service = ((System.IServiceProvider)proj).GetService(typeof(IOutputWindowFactory)) as IOutputWindowFactory;
                IOutputWindow outputWindow = service.GetStandardOutputWindow(StandardOutputWindow.Deploy);
                outputWindow.Activate();
                outputWindow.Clear();
                outputWindow.ReportStatusMessage("BIDS Helper is deploying packages...");
                this.ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                this.ApplicationObject.StatusBar.Text = "Deploying package(s)...";

                try
                {
                    string sConfigFileName = ((Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)proj).FullName + ".bidsHelper.user";
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    if (System.IO.File.Exists(sConfigFileName))
                    {
                        doc.Load(sConfigFileName);
                    }

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

                    //load project items to loop through
                    System.Array selectedItems = ((System.Array)solExplorer.SelectedItems);
                    hierItem = ((UIHierarchyItem)((System.Array)selectedItems).GetValue(0));
                    proj = hierItem.Object as Project;
                    if (proj != null)
                    {
                        System.Collections.Generic.List<ProjectItem> list = new System.Collections.Generic.List<ProjectItem>(proj.ProjectItems.Count);
                        foreach (ProjectItem item in proj.ProjectItems)
                        {
                            list.Add(item);
                        }
                        selectedItems = list.ToArray();
                    }

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

                    //setup Process object to call the dtutil EXE
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.FileName = "dtutil";

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
                            string sDestinationPath = newOptions.FilePath;
                            if (!sDestinationPath.EndsWith("\\")) sDestinationPath += "\\";
                            if (!System.IO.Directory.Exists(sDestinationPath))
                                System.IO.Directory.CreateDirectory(sDestinationPath);
                            sDestinationPath += sFileName;
                            if (System.IO.File.Exists(sDestinationPath))
                            {
                                System.IO.File.SetAttributes(sDestinationPath, System.IO.FileAttributes.Normal);
                            }
                            System.IO.File.Copy(sFilePath, sDestinationPath, true);
                            outputWindow.ReportStatusMessage("Deployed " + sFileName);
                        }
                        else
                        {
                            process.Refresh();
                            process.StartInfo.Arguments = string.Format("/FILE \"{0}\" /DestServer {1} /COPY {2};\"{3}\" /Q", sFilePath, newOptions.DestinationServer, sDestType, sDestFolder + sFileName.Substring(0, sFileName.Length - ".dtsx".Length));
                            process.Start();
                            string sError = process.StandardError.ReadToEnd();
                            string sStandardOutput = process.StandardOutput.ReadToEnd();
                            process.WaitForExit();
                            if (process.ExitCode > 0)
                            {
                                outputWindow.ReportStatusError(OutputWindowErrorSeverity.Error, "BIDS Helper encountered an error when deploying package " + sFileName + "!\r\ndtutil " + process.StartInfo.Arguments + "\r\nexit code = " + process.ExitCode + "\r\n" + sStandardOutput);
                                this.ApplicationObject.ToolWindows.OutputWindow.Parent.AutoHides = false; //pin the window open so you can see the problem
                                return;
                            }
                            outputWindow.ReportStatusMessage("Deployed " + sFileName);
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                    outputWindow.ReportStatusMessage("BIDS Helper completed deploying packages successfully.");
                }
                catch (Exception ex)
                {
                    outputWindow.ReportStatusError(OutputWindowErrorSeverity.Error, "BIDS Helper encountered an error when deploying packages:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    this.ApplicationObject.ToolWindows.OutputWindow.Parent.AutoHides = false; //pin the window open so you can see the problem
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

        #region Override Project Properties Dialog
        private void CaptureClickEventForProjectPropertiesMenu()
        {
            CommandBars cmdBars = (CommandBars)this.ApplicationObject.CommandBars;
            CommandBar pluginCmdBar = cmdBars["Project"];
            foreach (CommandBarControl cmd in pluginCmdBar.Controls)
            {
                if (cmd.Caption.Replace("&", string.Empty) == "Properties")
                {
                    cmdButtonProperties = cmd as CommandBarButton; //must save to a member variable of the class or the event won't fire later
                    cmdButtonProperties.Click += new _CommandBarButtonEvents_ClickEventHandler(cmdButtonProperties_Click);
                }
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

                    UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                    Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt proj = hierItem.Object as Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
                    if (proj == null || proj.Kind != SSIS_PROJECT_KIND) return;

                    CancelDefault = true; //don't let the Microsoft code fire as I'm going to pop up the dialog myself

                    Microsoft.DataWarehouse.Interfaces.IConfigurationSettings settings = (Microsoft.DataWarehouse.Interfaces.IConfigurationSettings)((System.IServiceProvider)proj).GetService(typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings));
                    projectManager = (Microsoft.DataWarehouse.Project.DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);

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
                        config.Options = newOptions; //override the Options object in memory so the configuration properties dialog will show our dialog
                    }

                    //pop up the configuration properties dialog
                    IVsPropertyPageFrame frame = (IVsPropertyPageFrame)((System.IServiceProvider)proj).GetService(typeof(SVsPropertyPageFrame));
                    int hr = frame.ShowFrame(Guid.Empty); //could pass in the Guid for our custom frame to have it default to it
                    if (hr < 0)
                    {
                        frame.ReportError(hr);
                        MessageBox.Show("Could not open properties window");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                }
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
                if (newConfig != null)
                    config.Options = newConfig.GetBaseType();
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
        [DisplayableByPropertyPage(new Type[] { typeof(DataTransformationsBuildPropertyPage), typeof(DataTransformationsDeploymentUtilityPropertyPage), typeof(DebugPropertyPage), typeof(DtsProjectExtendedDeployPropertyPage) })]
        public class DtsProjectExtendedConfigurationOptions : DataTransformationsProjectConfigurationOptions
        {
            private DataTransformationsProjectConfigurationOptions oldConfig;
            public DtsProjectExtendedConfigurationOptions() : base() { }

            public DtsProjectExtendedConfigurationOptions(DataTransformationsProjectConfigurationOptions old)
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
