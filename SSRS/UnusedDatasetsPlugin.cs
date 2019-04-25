using System;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using System.Windows.Forms;
using System.Collections.Generic;
using BIDSHelper.Core;

namespace BIDSHelper.SSRS
{
    [FeatureCategory(BIDSFeatureCategories.SSRS)]
    public class UnusedDatasetsPlugin : BIDSHelperPluginBase
    {
        public UnusedDatasetsPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.UnusedDatasetsId);
        }

        public override string ShortName
        {
            get { return "UnusedDatasets"; }
        }

        //public override int Bitmap
        //{
        //    get { return 543; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Unused Report Datasets..."; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        //public override string MenuName
        //{
        //    get { return "Project,Solution"; }
        //}
        

        /// <summary>
        /// Gets the name of the friendly name of the plug-in.
        /// </summary>
        /// <value>The friendly name.</value>
        public override string FeatureName
        {
            get { return "Unused Report Datasets"; }
        }

        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        public override string  HelpUrl
        {
	        get { return this.GetCodePlexHelpUrl("Dataset Usage Reports"); }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSRS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Provides a report of unused datasets. You can then delete the unused datasets, thus speeding report performance and scalability."; }
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
                Project project = hierItem.Object as Project;
                if (project == null && hierItem.Object is ProjectItem)
                {
                    ProjectItem pi = hierItem.Object as ProjectItem;
                    project = pi.SubProject;
                }
                if (project != null)
                {
                    if (DatasetScanner.GetRdlFilesInProjectItems(project.ProjectItems, true).Length > 0)
                        return true;
                }
                else if (solution != null)
                {
                    foreach (Project p in solution.Projects)
                    {
                        if (DatasetScanner.GetRdlFilesInProjectItems(p.ProjectItems, true).Length > 0)
                            return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        

        public override void Exec()
        {
            DatasetScanner.ScanReports(this.ApplicationObject.ToolWindows.SolutionExplorer , true);
        }

        
    }

    
}
