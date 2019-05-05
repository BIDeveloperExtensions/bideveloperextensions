
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using BIDSHelper.Core;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class AggregationManagerPlugin : BIDSHelperPluginBase
    {
        public AggregationManagerPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.AggregationManagerId, typeof(Cube));
        }

        public override string ShortName
        {
            get { return "AggregationManager"; }
        }

        //public override int Bitmap
        //{
        //    get { return 3984; }
        //}

        public override string FeatureName
        {
            get { return "Edit Aggregations..."; }
        }

        public override string ToolTip
        {
            get { return "Allows for manually editing aggregation designs"; }
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
            get { return "Provides an advanced interface for manually editing and managing aggregations."; }
        }


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                Cube cub = (Cube)projItem.Object;

                AggManager.MainForm frm = new AggManager.MainForm(cub, projItem);
                frm.ShowDialog(); //show as a modal dialog so there's no way to continue editing the cube with the normal designer until you're done with the agg manager
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}