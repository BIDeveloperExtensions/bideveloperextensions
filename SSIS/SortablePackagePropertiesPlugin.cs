using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.DataWarehouse.Controls;
using BIDSHelper.Core;

namespace BIDSHelper.SSIS
{
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class SortablePackagePropertiesPlugin : BIDSHelperPluginBase
    {
        private List<PackageProperties> listPackageProperties;
        private string DatabaseName;
        private string PackagePathPrefix;

        #region Standard Plugin Overrides
        public SortablePackagePropertiesPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.SortablePackagePropertiesId);
        }

        public override string ShortName
        {
            get { return "SortablePackageProperties"; }
        }

        //public override int Bitmap
        //{
        //    get { return 9904; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Sortable Package Properties Report..."; }
        //}

        public override string FeatureName
        {
            get { return "Sortable Package Properties Report"; }
        }

        //public override string MenuName
        //{
        //    get { return "Project,Solution"; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
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
            get { return "Displays a report of all packages in a project or solution, with key properties such as Name, Id, Description and Version information."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ShouldDisplayCommand()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;
                EnvDTE.Project proj = GetSelectedProjectReference();

                if (proj != null)
                {
                    if (proj.Object == null || proj.Object.GetType().FullName != "Microsoft.AnalysisServices.Database") return false; //this should be a reference to Microsoft.AnalysisServices.AppLocal.dll
                    //Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)proj;
                    return (proj.Kind == BIDSProjectKinds.SSIS);
                }
                else if (solution != null)
                {
                    foreach (EnvDTE.Project p in solution.Projects)
                    {
                        if (p.Kind != BIDSProjectKinds.SSIS) return false;
                    }
                    return (solution.Projects.Count > 0);
                }
            }
            catch { }
            return false;
        }
        #endregion

        public override void Exec()
        {
            ProjectItem piCurrent = null;
            try
            {
                listPackageProperties = new List<PackageProperties>();

                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;
                EnvDTE.Project proj = GetSelectedProjectReference();

                if (proj != null)
                {
                    
                    Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)proj;
                        this.DatabaseName = "Project: " + proj.Name;

                    PackageHelper.SetTargetServerVersion(proj); //sets the target version

                    try
                        {
                            using (WaitCursor cursor1 = new WaitCursor())
                            {
                                int iProgress = 0;
                                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationGeneral);
                                foreach (ProjectItem pi in proj.ProjectItems)
                                {
                                    ApplicationObject.StatusBar.Progress(true, "Scanning package " + pi.Name, iProgress++, proj.ProjectItems.Count);
                                    string sFileName = pi.Name.ToLower();
                                    if (!sFileName.EndsWith(".dtsx")) continue;
                                    piCurrent = pi;
                                    this.PackagePathPrefix = pi.Name;
                                    PackageProperties props = GetPackageProperties(pi);
                                    if (props == null) continue;
                                    listPackageProperties.Add(props);
                                }
                            }

                        }
                        finally
                        {
                            try
                            {
                                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationGeneral);
                                ApplicationObject.StatusBar.Progress(false, "", 1, 1);
                            }
                            catch { }
                        }
                }
                else if (solution != null)
                {
                    this.DatabaseName = "Solution: " + System.IO.Path.GetFileNameWithoutExtension(solution.FullName);
                    try
                    {
                        this.DatabaseName = "Solution: " + solution.Properties.Item("Name").Value;
                    }
                    catch { }

                    try
                    {
                        using (WaitCursor cursor1 = new WaitCursor())
                        {
                            ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationGeneral);
                            foreach (EnvDTE.Project p in solution.Projects)
                            {
                                Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)p;
                                if (projExt.Kind == BIDSProjectKinds.SSIS)
                                {
                                    int iProgress = 0;
                                    foreach (ProjectItem pi in p.ProjectItems)
                                    {
                                        ApplicationObject.StatusBar.Progress(true, "Scanning project " + p.Name + " package " + pi.Name, iProgress++, p.ProjectItems.Count);
                                        string sFileName = pi.Name.ToLower();
                                        if (!sFileName.EndsWith(".dtsx")) continue;
                                        piCurrent = pi;
                                        this.PackagePathPrefix = p.Name + "\\" + pi.Name;
                                        PackageProperties props = GetPackageProperties(pi);
                                        if (props == null) continue;
                                        listPackageProperties.Add(props);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        try
                        {
                            ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationGeneral);
                            ApplicationObject.StatusBar.Progress(false, "", 1, 1);
                        }
                        catch { }
                    }
                }

                    ReportViewerForm frm = new ReportViewerForm();
                    frm.ReportBindingSource.DataSource = this.listPackageProperties;
                    frm.Report = "SSIS.SortablePackageProperties.rdlc";
                    Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                    reportDataSource1.Name = "BIDSHelper_PackageProperties";
                    reportDataSource1.Value = frm.ReportBindingSource;
                    frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                    frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = frm.Report;

                    frm.Caption = "Sortable Package Properties Report";
                    frm.WindowState = FormWindowState.Maximized;
                    frm.Show();
            }
            catch (DtsRuntimeException ex)
            {
                if (ex.ErrorCode == -1073659849L)
                {
                    MessageBox.Show((piCurrent == null ? "This package" : piCurrent.Name) + " has a package password. Please open the package designer, specify the password when the dialog prompts you, then rerun the Sortable Package Properties report.\r\n\r\nDetailed error was:\r\n" + ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Password Not Specified");
                }
                else
                {
                    MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error" + (piCurrent == null ? string.Empty : " scanning " + piCurrent.Name));
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error" + (piCurrent == null ? string.Empty : " scanning " + piCurrent.Name));
            }
        }

        /// <summary>
        /// If the package designer is already open, then use the package object on it which may have in-memory modifications.
        /// If the package designer is not open, then just load the package from disk.
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        private PackageProperties GetPackageProperties(ProjectItem pi)
        {
            Package package;
            bool bIsOpen = pi.get_IsOpen(BIDSViewKinds.Designer);

            if (bIsOpen)
            {
                Window w = pi.Open(BIDSViewKinds.Designer); //opens the designer
                w.Activate();

                IDesignerHost designer = w.Object as IDesignerHost;
                if (designer == null) return null;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                package = win.PropertiesLinkComponent as Package;
            }
            else
            {
                //Microsoft.SqlServer.Dts.Runtime.Application app = new Microsoft.SqlServer.Dts.Runtime.Application();
                Microsoft.SqlServer.Dts.Runtime.Application app = SSIS.PackageHelper.Application; //sets the proper TargetServerVersion
                package = app.LoadPackage(pi.get_FileNames(0), null);
            }

            if (package == null) return null;

            PackageProperties props = new PackageProperties(package);
            props.DatabaseName = this.DatabaseName;
            props.PackagePathPrefix = this.PackagePathPrefix;
            return props;
        }

        public class PackageProperties
        {
            public PackageProperties(Package p)
            {
                _CreationDate = p.CreationDate;
                _CreatorComputerName = p.CreatorComputerName;
                _CreatorName = p.CreatorName;
                _VersionBuild = p.VersionBuild;
                _VersionGUID = p.VersionGUID;
                _VersionMajor = p.VersionMajor;
                _VersionMinor = p.VersionMinor;
                _Description = p.Description;
                _ID = p.ID;
                _Name = p.Name;
            }

            private DateTime _CreationDate;
            private string _CreatorComputerName;
            private string _CreatorName;
            private int _VersionBuild;
            private string _VersionGUID;
            private int _VersionMajor;
            private int _VersionMinor;
            private string _Description;
            private string _ID;
            private string _Name;
            private string _DatabaseName;
            private string _PackagePathPrefix;

            public DateTime CreationDate { get { return _CreationDate; } }
            public string CreatorComputerName { get { return _CreatorComputerName; } }
            public string CreatorName { get { return _CreatorName; } }
            public int VersionBuild { get { return _VersionBuild; } }
            public string VersionGUID { get { return _VersionGUID; } }
            public int VersionMajor { get { return _VersionMajor; } }
            public int VersionMinor { get { return _VersionMinor; } }
            public string Description { get { return _Description; } }
            public string ID { get { return _ID; } }
            public string Name { get { return _Name; } }
            public string DatabaseName { get { return _DatabaseName; } set { _DatabaseName = value; } }
            public string PackagePathPrefix { get { return _PackagePathPrefix; } set { _PackagePathPrefix = value; } }
        }
    }
}
