using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using BIDSHelper.Core;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class DeleteUnusedIndexesPlugin : BIDSHelperPluginBase
    {

        public DeleteUnusedIndexesPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.DeleteUnusedIndexesId, typeof(Cube));
        }

        public override string ShortName
        {
            get { return "DeleteUnusedIndexes"; }
        }

        public override string FeatureName
        {
            get
            {
                return "Delete Unused Indexes";
            }
        }

        //public override int Bitmap
        //{
        //    get { return 214; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Delete Unused Indexes..."; }
        //}

        public override string ToolTip
        {
            get { return ""; }
        }
        
        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        public override string HelpUrl
        {
            get { return this.GetCodePlexHelpUrl("Delete Unused Indexes"); }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASMulti; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Analyzes the Profiler trace event Query Subcube Verbose to determine which indexes can be disabled to save processing time."; }
        }


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                Cube cub = (Cube)projItem.Object;

                DeleteUnusedIndexesForm form1 = new DeleteUnusedIndexesForm();
                form1.Init(projItem, cub.Parent, cub);
                form1.ShowDialog();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "BIDS Helper Delete Unused Indexes");
            }
        }

    }
}