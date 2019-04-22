using System;
using System.Reflection;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.DataWarehouse.VsIntegration.Hierarchy;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project;
using BIDSHelper.Core;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

namespace BIDSHelper.SSIS
{
    /// <summary>
    ///     Automatically sort packages in SSIS project. Sorting will not get persisted with Project deployment projects.
    /// </summary>
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class AutoSortProjectFilesPlugin : BIDSHelperPluginBase, IVsSolutionEvents
    {
        // Declare as field, we need to keep the reference alive for it to work
        //private readonly SolutionEvents solutionEvents;
        private readonly IVsSolution _solutionService;
        private uint _solutionEventsCookie;

        public AutoSortProjectFilesPlugin(BIDSHelperPackage package)
            : base(package)
        {
            _solutionService = this.ServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
        }

        public override string ShortName
        {
            get { return "AutoSortProjectFilesPlugin"; }
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

        public override string FeatureName { get { return "Auto sort by name"; } }

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

                var solution = solExplorer.UIHierarchyItems.Item(1).Object as SolutionClass;
                if (solution == null)
                {
                    return;
                }

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

        public override void Exec()
        {
            // Nothing to do here, all the work is done via the solution opened event handler method
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (_solutionService != null)
            {
                _solutionService.AdviseSolutionEvents(this, out _solutionEventsCookie);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _solutionService.UnadviseSolutionEvents(_solutionEventsCookie);
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) { return VSConstants.S_OK; }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) { return VSConstants.S_OK; }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) { return VSConstants.S_OK; }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) { return VSConstants.S_OK; }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) { return VSConstants.S_OK; }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) { return VSConstants.S_OK; }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution) {
            SolutionOpened();
            return VSConstants.S_OK; }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) { return VSConstants.S_OK; }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved) { return VSConstants.S_OK; }

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved) { return VSConstants.S_OK; }
    }
}