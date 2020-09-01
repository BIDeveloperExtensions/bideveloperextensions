extern alias sharedDataWarehouseInterfaces;
using BIDSHelper.Core;
using EnvDTE;
using sharedDataWarehouseInterfaces::Microsoft.DataWarehouse.Interfaces;
using Microsoft.DataWarehouse.Project;
using Microsoft.DataWarehouse.VsIntegration.Hierarchy;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace BIDSHelper.SSIS
{
    /// <summary>
    /// Sort packages in SSIS project. This is very usefull for SQL2005 but was implemented natively in SQL 2008, however was not persisted.
    /// This persistence only works on projects which use the older Package deployment model. 
    /// The sorting itself works on newer Project deployment model projects but it doesn't get saved, so no benefit over native SSDT sorting.
    /// </summary>
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class SortProjectFilesPlugin : BIDSHelperPluginBase
    {
        public SortProjectFilesPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.SortProjectFilesId);
        }

        public override string ShortName
        {
            get { return "SortProjectFilesPlugin"; }
        }

        public override string FeatureName
        {
            get { return "Sort by name, persisted"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }

        /// <summary>
        ///     Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Adds a 'Sort by name, persisted' menu option to the SSIS Packages folder allowing you to easily re-order the packages. Only available for Package deployment model projects."; }
        }

        /// <summary>
        ///     Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ShouldDisplayCommand()
        {
            try
            {
                // Get a referebce to teh solution explorer
                var solExplorer = ApplicationObject.ToolWindows.SolutionExplorer;

                // Check if we have multiple items selected
                if (((Array) solExplorer.SelectedItems).Length != 1)
                {
                    return false;
                }

                // Get the project
                Project project = this.GetSelectedProjectReference();
                if (project == null)
                {
                    return false;
                }

                // Persistence doesn't work in Project deployment mode, so don't show command
                if (!DeployPackagesPlugin.IsLegacyDeploymentMode(project))
                {
                    return false;
                }

                // Get item should be a folder like "SSIS Packages"
                var hierItem = ((UIHierarchyItem)((Array) solExplorer.SelectedItems).GetValue(0));

                // Check that we are looking at the "SSIS Packages" node by checking that there is 
                // more than one child item and that the first item (in the 1 based collection)
                // ends with ".dtsx"
                return (hierItem.Collection.Count > 0 && hierItem.UIHierarchyItems.Item(1).Name.EndsWith(".dtsx"));
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
                var solExplorer = ApplicationObject.ToolWindows.SolutionExplorer;
                var hierItem = (UIHierarchyItem) ((Array) solExplorer.SelectedItems).GetValue(0);
                var pi = (ProjectItem) hierItem.Object;
                var p = pi.ContainingProject;

                var folder =
                    hierItem.Object.GetType()
                        .InvokeMember("projectNode",
                            BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance |
                            BindingFlags.FlattenHierarchy, null, hierItem.Object, null) as FileProjectVirtualFolder;
                if (folder == null) throw new Exception("Could not get FileProjectVirtualFolder");
                var children = folder.Children as ISortableHierarchyCollection;
                if (children == null) throw new Exception("Could not get ISortableHierarchyCollection");
                children.Sort();

                // Mark the project as dirty
                var settings = p.GetIConfigurationSettings();
                var projectManager =
                    (DataWarehouseProjectManager)
                        settings.GetType()
                            .InvokeMember("ProjectManager",
                                BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty |
                                BindingFlags.FlattenHierarchy, null, settings, null);
                projectManager.GetType()
                    .InvokeMember("MarkTextBufferAsUnsaved",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                        BindingFlags.InvokeMethod, null, projectManager, new object[] {});
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}