using System;
using System.Reflection;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.DataWarehouse.VsIntegration.Hierarchy;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project;

namespace BIDSHelper
{
    /// <summary>
    ///     Automatically sort packages in SSIS project. Sorting will not get persisted with Project deployment projects.
    /// </summary>
    public class AutoSortProjectFilesPlugin : BIDSHelperPluginBase
    {
        // Declare as field, we need to keep the reference alive for it to work
        private readonly SolutionEvents solutionEvents;

        public AutoSortProjectFilesPlugin(Connect connect, DTE2 appObject, AddIn addinInstance)
            : base(connect, appObject, addinInstance)
        {
            solutionEvents = appObject.Events.SolutionEvents;
            solutionEvents.Opened += SolutionOpened;
        }

        public override string ShortName
        {
            get { return "AutoSortProjectFilesPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "Auto sort by name"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        /// <summary>
        ///     Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
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
            get { return "Automatically sorts by name the contents of the Connection Managers, SSIS Packages and Miscellaneous folders in a SSIS project when it is opened. Removes the need to manually use the sort by name option."; }
        }

        private void SolutionOpened()
        {
            try
            {
                var solExplorer = ApplicationObject.ToolWindows.SolutionExplorer;
                if (solExplorer.UIHierarchyItems.Count != 1)
                {
                    return;
                }

                var rootItem = solExplorer.UIHierarchyItems.Item(1);
                if (rootItem == null)
                {
                    return;
                }

                // TODO: Try on other versions? Removed this test, as never succeeds on SQL2012 on VS2012
                //var solution = rootItem.Object as SolutionClass;
                //if (solution == null)
                //{
                //    return;
                //}

                ProcessHierarchyItem(rootItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ProcessHierarchyItem(UIHierarchyItem hierarchyItem)
        {
            for (var i = 1; i <= hierarchyItem.UIHierarchyItems.Count; i++)
            {
                // Get the child item in the solution or folder
                var projectItem = hierarchyItem.UIHierarchyItems.Item(i);

                // Check if it is a Project, skip if not (Solution folders are a type of project too)
                var project = projectItem.Object as Project;
                if (project == null)
                {
                    continue;
                }

                // Check if it is a SSIS Project, process if it is
                if (project.Kind == BIDSProjectKinds.SSIS)
                {
                    ProcessProject(projectItem);
                }
                else if (project.Kind == BIDSProjectKinds.SolutionFolder)
                {
                    ProcessHierarchyItem(projectItem);
                }
            }
        }

        private void ProcessProject(UIHierarchyItem projectItem)
        {
            for (var i = 1; i <= projectItem.UIHierarchyItems.Count; i++)
            {
                // Get a project folder, although not all items will be folders, e.g. connections and parameters
                var folderItem = projectItem.UIHierarchyItems.Item(i);
                var folder =
                    folderItem.Object.GetType()
                        .InvokeMember("projectNode",
                            BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance |
                            BindingFlags.FlattenHierarchy, null, folderItem.Object, null) as FileProjectVirtualFolder;

                if (folder == null)
                {
                    continue;
                }

                // Sort folder items, packages
                var children = folder.Children as ISortableHierarchyCollection;
                if (children == null) throw new Exception("Could not get ISortableHierarchyCollection");
                children.Sort();
            }
        }

        public override bool DisplayCommand(UIHierarchyItem item)
        {
            // No menu item to display, so always false
            return false;
        }

        public override void Exec()
        {
            // Nothing to do here, all the work is done via the solution opened event handler method
        }
    }
}