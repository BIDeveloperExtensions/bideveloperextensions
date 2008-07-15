using EnvDTE;
using EnvDTE80;
using System.Text;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using System;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualStudio.CommandBars;
using System.ComponentModel.Design;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.DataTransformationServices.Design;
using Microsoft.DataWarehouse.Project;

namespace BIDSHelper
{
    public class RelativePathsPlugin : BIDSHelperPluginBase
    {
        private CommandBarButton cmdButtonProperties = null;
        private Package packageForFixButton = null;
        private string pathForPackageForFixButton;

        public RelativePathsPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
            CaptureClickEventForSSISMenu();
        }

        #region Running from the Package Configurations dialog
        private void CaptureClickEventForSSISMenu()
        {
            CommandBars cmdBars = (CommandBars)this.ApplicationObject.CommandBars;
            CommandBar pluginCmdBar = cmdBars["&SSIS"];
            foreach (CommandBarControl cmd in pluginCmdBar.Controls)
            {
                if (cmd.Caption.Replace("&", string.Empty).StartsWith("Package Configurations"))
                {
                    cmdButtonProperties = cmd as CommandBarButton; //must save to a member variable of the class or the event won't fire later
                    cmdButtonProperties.Click += new _CommandBarButtonEvents_ClickEventHandler(cmdButtonProperties_Click);
                    return;
                }
                //if (cmd.Id == (int)BIDSToolbarButtonID.PackageConfigurations) //cmd.Caption.Replace("&", string.Empty) == "Package Configurations"
                //{
                //    cmdButtonProperties = cmd as CommandBarButton; //must save to a member variable of the class or the event won't fire later
                //    cmdButtonProperties.Click += new _CommandBarButtonEvents_ClickEventHandler(cmdButtonProperties_Click);
                //    return;
                //}
            }

            if (Enabled) //could get annoying if you disable this and it kept showing up
                MessageBox.Show("BIDS Helper SSIS Relative Paths plugin could not find Package Configurations button.");
        }

        //they asked to pop up the Package Configurations dialog, so replace the Microsoft functionality so we can control the popup form
        void cmdButtonProperties_Click(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            if (cmdButtonProperties.Caption != Ctrl.Caption) return; //for some reason this fires on other commands like Add Annotation

            if (Enabled)
            {
                try
                {
                    if (this.ApplicationObject.ActiveWindow == null || this.ApplicationObject.ActiveWindow.ProjectItem == null) return;
                    ProjectItem pi = this.ApplicationObject.ActiveWindow.ProjectItem;
                    if (!pi.Name.ToLower().EndsWith(".dtsx")) return;

                    if (pi.ContainingProject == null || pi.ContainingProject.Kind != BIDSProjectKinds.SSIS) return; //if the dtsx isn't in an SSIS project, or if you're editing the package standalone (not as a part of a project)

                    IDesignerHost designer = this.ApplicationObject.ActiveWindow.Object as IDesignerHost;
                    if (designer == null) return;
                    EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                    Package package = (Package)win.PropertiesLinkComponent;
                    this.packageForFixButton = package;
                    this.pathForPackageForFixButton = pi.get_FileNames(0);

                    DtsConfigurationsForm form = new DtsConfigurationsForm(package);
                    if (win.SelectedIndex == 0)
                    {
                        //control flow
                        EditorWindow.EditorView view = win.SelectedView;
                        System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                        Control viewControl = (Control)view.GetType().InvokeMember("ViewControl", getflags, null, view, null);
                        DdsDiagramHostControl diagram = viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"] as DdsDiagramHostControl;
                        if (diagram == null || diagram.DDS == null) return;
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

                        if (DesignUtils.ShowDialog((Form)form, (IWin32Window)diagram.DDS, (IServiceProvider)package.Site) == DialogResult.OK)
                        {
                            DesignUtils.MarkPackageDirty(package);
                        }
                    }

                    CancelDefault = true;
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

        public override int Bitmap
        {
            get { return 1021; }
        }

        public override string ButtonText
        {
            get { return "Fix Relative Paths..."; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }

        public override string MenuName
        {
            get { return "Project"; }
        }

        public override string FriendlyName
        {
            get { return "SSIS Relative Paths"; }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }
        #endregion

        #region Running from the Project node menu
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            if (((System.Array)solExplorer.SelectedItems).Length == 1)
            {
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                Project proj = hierItem.Object as Project;
                if (proj != null)
                {
                    return (proj.Kind == BIDSProjectKinds.SSIS);
                }
            }
            return false;
        }

        public override void Exec()
        {
            if (MessageBox.Show("Are you sure you want to change any hardcoded paths pointing to files in the packages directory to relative paths?\r\n\r\nThis command impacts configurations and connection managers.", "BIDS Helper - Fix Relative Paths", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;

            try
            {
                this.ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationFind);
                this.ApplicationObject.StatusBar.Text = "Fixing relative paths...";

                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                Project proj = (Project)hierItem.Object;

                Microsoft.DataWarehouse.Interfaces.IConfigurationSettings settings = (Microsoft.DataWarehouse.Interfaces.IConfigurationSettings)((System.IServiceProvider)proj).GetService(typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings));
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
                        if (!item.get_IsOpen(BIDSViewKinds.SsisDesigner))
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

                    Window w = item.Open(BIDSViewKinds.SsisDesigner); //opens the designer
                    w.Activate();

                    IDesignerHost designer = w.Object as IDesignerHost;
                    if (designer == null) continue;
                    EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                    Package package = win.PropertiesLinkComponent as Package;
                    if (package == null) continue;

                    outputWindow.ReportStatusMessage("Package " + item.Name);

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
                        }
                    }
                    if (bChanged)
                    {
                        DesignUtils.MarkPackageDirty(package);
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
