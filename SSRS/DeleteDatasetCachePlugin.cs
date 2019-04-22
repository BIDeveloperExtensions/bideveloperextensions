using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Collections.Generic;
using BIDSHelper.Core;

namespace BIDSHelper.SSRS
{
    [FeatureCategory(BIDSFeatureCategories.SSRS)]
    public class DeleteDatasetCachePlugin : BIDSHelperPluginBase
    {
        public DeleteDatasetCachePlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.DeleteDatasetCacheId);
        }

        public override string ShortName
        {
            get { return "DeleteDatasetCache"; }
        }

        //public override int Bitmap
        //{
        //    get { return 214; }
        //}

        public override string FeatureName
        {
            get { return "Delete Dataset Cache Files"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        //public override string MenuName
        //{
        //    get { return "Project,Solution"; }
        //}

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
            get { return "Allows you to quickly and easily delete dataset cache files, ensuring you see current data when previewing your reports."; }
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
                Project p = GetSelectedProjectReferenceRS();

                SolutionClass solution = hierItem.Object as SolutionClass;
                if (p != null)
                {
                    return (p.Kind == BIDSProjectKinds.SSRS);
                }
                else if (solution != null)
                {
                    foreach (Project pp in solution.Projects)
                    {
                        if (p.Kind == BIDSProjectKinds.SSRS)
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
            string sCurrentFile = string.Empty;
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;
                Project proj = GetSelectedProjectReferenceRS();

                List<string> lstRdls = new List<string>();
                if (proj != null)
                {
                    lstRdls.AddRange(DatasetScanner.GetRdlFilesInProjectItems(proj.ProjectItems, false));
                }
                else if (solution != null)
                {
                    foreach (Project p in solution.Projects)
                    {
                        lstRdls.AddRange(DatasetScanner.GetRdlFilesInProjectItems(p.ProjectItems, true));
                    }
                }

                int iDeleted = 0;
                foreach (string file in lstRdls)
                {
                    sCurrentFile = file;

                    string sCachePath = file + ".data";
                    if (System.IO.File.Exists(sCachePath))
                    {
                        System.IO.File.SetAttributes(sCachePath, System.IO.FileAttributes.Normal);
                        System.IO.File.Delete(sCachePath);
                        iDeleted++;
                    }
                }

                MessageBox.Show("Scanned " + lstRdls.Count + " report(s).\r\nDeleted " + iDeleted + " rdl.data file(s).", "BIDS Helper Delete Dataset Cache Files");
            }
            catch (System.Exception ex)
            {
                string sError = string.Empty;
                if (!string.IsNullOrEmpty(sCurrentFile)) sError += "Error while scanning report: " + sCurrentFile + "\r\n";
                sError += ex.Message + "\r\n" + ex.StackTrace;
                MessageBox.Show(sError);
            }
        }
    }
}
