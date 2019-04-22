using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using BIDSHelper.Core;
using System;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class VisualizeAttributeLatticePlugin : BIDSHelperPluginBase
    {
        public VisualizeAttributeLatticePlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.VisualizeAttributeLatticeId, typeof(Dimension));
        }

        public override string ShortName
        {
            get { return "VisualizeAttributeLattice"; }
        }

        //public override int Bitmap
        //{
        //    get { return 702; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Visualize Attribute Lattice"; }
        //}

        public override string ToolTip
        {
            get { return "Displays a visualization of the attribute relationships in a dimension"; }
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
            get { return "Allows you to visually see the attribute relationships you have defined for a dimension."; }
        }

        public override string FeatureName
        {
            get
            {
                return "Visualize Attribute Lattice";
            }
        }

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                Dimension dim = (Dimension)projItem.Object;

                VisualizeAttributeLatticeForm frm = new VisualizeAttributeLatticeForm();
                frm.dimension = dim;
                frm.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}