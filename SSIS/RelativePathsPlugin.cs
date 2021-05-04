extern alias sharedDataWarehouseInterfaces;
using EnvDTE;
using EnvDTE80;
using System.Text;
using Microsoft.DataWarehouse.Design;
using System;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.DataTransformationServices.Design;
using Microsoft.DataWarehouse.Project;
using BIDSHelper.Core;

namespace BIDSHelper.SSIS
{
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class RelativePathsPlugin : BIDSHelperPluginBase
    {
        private EnvDTE.CommandEvents cmdPackageConfigurations;
        private Package packageForFixButton = null;
        private string pathForPackageForFixButton;

        public RelativePathsPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.FixRelativePathsId, new Guid(BIDSProjectKinds.SSIS));
            CaptureClickEventForSSISMenu();
        }

        #region Running from the Package Configurations dialog
        private void CaptureClickEventForSSISMenu()
        {
            foreach (Command cmd in this.ApplicationObject.Commands)
            {
                if (cmd.Name == "SSIS.PackageConfigurations")
                {
                    this.cmdPackageConfigurations = this.ApplicationObject.Events.get_CommandEvents(cmd.Guid, cmd.ID);
                    this.cmdPackageConfigurations.BeforeExecute += new _dispCommandEvents_BeforeExecuteEventHandler(cmdEvent_BeforeExecute);
                    return;
                }
            }
        }

        //they asked to pop up the Package Configurations dialog, so replace the Microsoft functionality so we can control the popup form
        void cmdEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            if (Enabled)
            {
                try
                {
                    if (this.ApplicationObject.ActiveWindow == null || this.ApplicationObject.ActiveWindow.ProjectItem == null) return;
                    ProjectItem pi = this.ApplicationObject.ActiveWindow.ProjectItem;
                    if (!pi.Name.ToLower().EndsWith(".dtsx")) return;

                    IDesignerHost designer = this.ApplicationObject.ActiveWindow.Object as IDesignerHost;
                    if (designer == null) return;
                    EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                    Package package = (Package)win.PropertiesLinkComponent;
                    this.packageForFixButton = package;
                    this.pathForPackageForFixButton = pi.get_FileNames(1);

                    DtsConfigurationsForm form = new DtsConfigurationsForm(package);
                    if (win.SelectedIndex == 0)
                    {
                        //control flow
                        EditorWindow.EditorView view = win.SelectedView;
                        System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                        Control viewControl = (Control)view.GetType().InvokeMember("ViewControl", getflags, null, view, null);
                        
                        IWin32Window parentWin;

                        parentWin = viewControl;
                        
                        Button editSelectedButton = (Button)form.Controls["editSelectedConfiguration"];
                        Control packageConfigurationsGridControl1 = form.Controls["packageConfigurationsGridControl1"];
                        Button btnRelativePaths = new Button();
                        btnRelativePaths.Text = "Fix All Relative Paths";
                        btnRelativePaths.Width = 140;
                        btnRelativePaths.Left = packageConfigurationsGridControl1.Left;
                        btnRelativePaths.Top = editSelectedButton.Top;
                        btnRelativePaths.Height = editSelectedButton.Height;
                        btnRelativePaths.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                        btnRelativePaths.Click += new EventHandler(btnRelativePaths_Click);
                        form.Controls.Add(btnRelativePaths);

                        if (DesignUtils.ShowDialog((Form)form, parentWin, (IServiceProvider)package.Site) == DialogResult.OK)
                        {
                            SSISHelpers.MarkPackageDirty(package);
                        }

                        CancelDefault = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }

        //inside the configuration dialog they clicked the "Fix All Relative Paths" button
        void btnRelativePaths_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("The current working directory is:");
            sb.AppendLine(System.IO.Directory.GetCurrentDirectory());
            sb.AppendLine();
            sb.AppendLine("The path for the current package is:");
            sb.AppendLine(this.pathForPackageForFixButton);
            sb.AppendLine();

            if (string.Compare(System.IO.Path.GetDirectoryName(this.pathForPackageForFixButton), System.IO.Directory.GetCurrentDirectory(), true) != 0)
            {
                sb.AppendLine("Since the package is not in the current working directory, no changes to configurations will be made. Please start Visual Studio by double clicking on the .sln file for the project, and make sure the .sln file is in the same directory as the packages.");
                MessageBox.Show(sb.ToString(), "BIDS Helper - Fix Relative Paths - Problem");
            }
            else
            {
                Cud.Transaction trans = Cud.BeginTransaction(this.packageForFixButton);

                bool bChanged = false;
                foreach (Microsoft.SqlServer.Dts.Runtime.Configuration config in this.packageForFixButton.Configurations)
                {
                    if (config.ConfigurationType == DTSConfigurationType.ConfigFile)
                    {
                        if (string.Compare(System.IO.Path.GetDirectoryName(config.ConfigurationString), System.IO.Directory.GetCurrentDirectory(), true) == 0)
                        {
                            config.ConfigurationString = System.IO.Path.GetFileName(config.ConfigurationString);
                            sb.Append("Configuration ").Append(config.Name).AppendLine(" changed to relative path");
                            bChanged = true;

                            trans.ChangeProperty(config, "ConfigurationString");
                        }
                    }
                }
                if (!bChanged)
                {
                    sb.AppendLine("No configurations were XML configurations with a full hardcoded path to a dtsConfig file in the same directory as the package were found.");
                }
                else
                {
                    //refresh the grid
                    Button btn = (Button)sender;
                    Form form = (Form)btn.Parent;
                    Control packageConfigurationsGridControl1 = form.Controls["packageConfigurationsGridControl1"];
                    packageConfigurationsGridControl1.GetType().InvokeMember("RefreshConfigurations", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, packageConfigurationsGridControl1, new object[] { });

                    trans.Commit();
                }
                MessageBox.Show(sb.ToString(), "BIDS Helper - Fix Relative Paths");
            }
        }
        #endregion

        #region Standard Property Overrides
        public override string ShortName
        {
            get { return "RelativePathsPlugin"; }
        }

        //public override int Bitmap
        //{
        //    get { return 1021; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        //public override string MenuName
        //{
        //    get { return "Project"; }
        //}


        /// <summary>
        /// Gets the name of the friendly name of the plug-in.
        /// </summary>
        /// <value>The friendly name.</value>
        /// <remarks>Used for HelpUrl as ButtonText does not match Wiki page.</remarks>
        public override string FeatureName
        {
            get { return "Fix Relative Paths"; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Allows you to change all configuration and connection file paths to be relative paths at the click of a button."; }
        }
        #endregion

        #region Running from the Project node menu
        //public override bool ShouldDisplayCommand()
        //{
        //    UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
        //    if (((System.Array)solExplorer.SelectedItems).Length == 1)
        //    {
        //        UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
        //        EnvDTE.Project proj = GetSelectedProjectReference();
        //        if (proj != null)
        //        {
        //            return (proj.Kind == BIDSProjectKinds.SSIS);
        //        }
        //    }
        //    return false;
        //}

        public override void Exec()
        {
            if (MessageBox.Show("Are you sure you want to change any hardcoded paths pointing to files in the packages directory to relative paths?\r\n\r\nThis command impacts configurations and connection managers.", "BIDS Helper - Fix Relative Paths", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;

            try
            {
                this.ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationFind);
                this.ApplicationObject.StatusBar.Text = "Fixing relative paths...";

                EnvDTE.Project proj = GetSelectedProjectReference();
                object settings = proj.GetIConfigurationSettings();
                DataWarehouseProjectManager projectManager = (DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);

                this.ApplicationObject.ToolWindows.OutputWindow.Parent.SetFocus();
                IOutputWindowFactory service = ((System.IServiceProvider)proj).GetService(typeof(IOutputWindowFactory)) as IOutputWindowFactory;
                IOutputWindow outputWindow = service.GetStandardOutputWindow(StandardOutputWindow.Deploy);
                outputWindow.Activate();
                outputWindow.Clear();
                outputWindow.ReportStatusMessage("BIDS Helper is fixing relative paths in all open packages...\r\n");

                bool bFoundOpenPackage = false;
                StringBuilder sb = new StringBuilder();
                string sPackageDirectory = string.Empty;
                foreach (ProjectItem item in proj.ProjectItems)
                {
                    try
                    {
                        if (!item.get_IsOpen(BIDSViewKinds.Designer))
                            continue;
                    }
                    catch
                    {
                        continue;
                    }
                    if (!bFoundOpenPackage)
                    {
                        bFoundOpenPackage = true;

                        sPackageDirectory = System.IO.Path.GetDirectoryName(item.get_FileNames(0));
                        outputWindow.ReportStatusMessage("The current working directory is:");
                        outputWindow.ReportStatusMessage(System.IO.Directory.GetCurrentDirectory());
                        outputWindow.ReportStatusMessage(string.Empty);
                        outputWindow.ReportStatusMessage("The directory for the packages is:");
                        outputWindow.ReportStatusMessage(sPackageDirectory);
                        outputWindow.ReportStatusMessage(string.Empty);

                        if (string.Compare(sPackageDirectory, System.IO.Directory.GetCurrentDirectory(), true) != 0)
                        {
                            outputWindow.ReportStatusMessage("PROBLEM:");
                            outputWindow.ReportStatusMessage("Since the packages are not in the current working directory, no changes to configurations will be made. Please start Visual Studio by double clicking on the .sln file for the project, and make sure the .sln file is in the same directory as the packages.");
                            return;
                        }
                    }

                    Window w = item.Open(BIDSViewKinds.Designer); //opens the designer
                    w.Activate();

                    IDesignerHost designer = w.Object as IDesignerHost;
                    if (designer == null) continue;
                    EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                    Package package = win.PropertiesLinkComponent as Package;
                    if (package == null) continue;

                    outputWindow.ReportStatusMessage("Package " + item.Name);

                    Cud.Transaction trans = Cud.BeginTransaction(package);

                    bool bChanged = false;
                    foreach (Microsoft.SqlServer.Dts.Runtime.Configuration config in package.Configurations)
                    {
                        if (config.ConfigurationType == DTSConfigurationType.ConfigFile)
                        {
                            if (string.Compare(System.IO.Path.GetDirectoryName(config.ConfigurationString), System.IO.Directory.GetCurrentDirectory(), true) == 0)
                            {
                                config.ConfigurationString = System.IO.Path.GetFileName(config.ConfigurationString);
                                outputWindow.ReportStatusMessage("  Configuration " + config.Name + " changed to relative path");
                                bChanged = true;

                                trans.ChangeProperty(config, "ConfigurationString");
                            }
                        }
                    }
                    foreach (ConnectionManager conn in package.Connections)
                    {
                        if (string.IsNullOrEmpty(conn.GetExpression("ConnectionString")) && string.Compare(System.IO.Path.GetDirectoryName(conn.ConnectionString), System.IO.Directory.GetCurrentDirectory(), true) == 0)
                        {
                            conn.ConnectionString = System.IO.Path.GetFileName(conn.ConnectionString);
                            outputWindow.ReportStatusMessage("  Connection " + conn.Name + " changed to relative path");
                            bChanged = true;

                            trans.ChangeProperty(conn, "ConnectionString");
                        }
                    }
                    if (bChanged)
                    {
                        SSISHelpers.MarkPackageDirty(package);
                    }
                    else
                    {
                        outputWindow.ReportStatusMessage("  No changes");
                    }
                }

                if (!bFoundOpenPackage)
                {
                    outputWindow.ReportStatusMessage("PROBLEM:");
                    outputWindow.ReportStatusMessage("No packages in this project were open.");
                    return;
                }

                outputWindow.Activate();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                this.ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationFind);
                this.ApplicationObject.StatusBar.Text = string.Empty;
            }
        }
        #endregion

    }

}
