using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using BIDSHelper.Core;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class AttributeRelationshipNameFixPlugin : BIDSHelperPluginBase
    {
        
        public AttributeRelationshipNameFixPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.AttributeRelationshipFixId, ".dim");
        }

        public override string ShortName
        {
            get { return "AttributeRelationshipNameFix"; }
        }

        //public override int Bitmap
        //{
        //    get { return 4380; } // TODO - choose new bitmap
        //}

        public override string FeatureName
        {
            get { return "Attribute Relationship Name Fix"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; /*doesn't show anywhere*/ }
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
            get { return "Will synchronise the attribute relationship name with the attribute id to fix the warnings caused by renaming attributes without renaming the relationships."; }
        }

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = ApplicationObject.ToolWindows.SolutionExplorer;
                var hierItem = (UIHierarchyItem)((Array)solExplorer.SelectedItems).GetValue(0);
                var projItem = (ProjectItem)hierItem.Object;
                var d = (Dimension)projItem.Object;

                if (d.DataSource == null)
                {
                    if (d.Source is TimeBinding)
                    {
                        MessageBox.Show("Attribute Relationship Name Fix is not supported on a Server Time dimension.");
                        return;
                    }
                    
                    
                }
                else if (d.Source is DimensionBinding)
                {
                    MessageBox.Show("Attribute Relationship Name Fix is not supported on a linked dimension.");
                    return;
                }

                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(true, "Fixing Attribute Relationship Names...", 0, d.Attributes.Count);

                bool bAttributesChanged = FixAttributeRelationshipNames(d);

                if (bAttributesChanged)
                {
                    EnvDTE.Window w = projItem.Open(BIDSViewKinds.Designer); //opens the designer
                    System.ComponentModel.Design.IDesignerHost designer = (System.ComponentModel.Design.IDesignerHost)w.Object;
                    System.ComponentModel.Design.IComponentChangeService changesvc = (System.ComponentModel.Design.IComponentChangeService)designer.GetService(typeof(System.ComponentModel.Design.IComponentChangeService));

                    changesvc.OnComponentChanged(d, null, null, null); //marks the cube designer as dirty
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(false, "Fixing Attribute Relationship Names...", 2, 2);
            }
        }

        private bool FixAttributeRelationshipNames(Dimension dimension)
        {
            int attribCount = dimension.Attributes.Count;
            int iAttrib = 0;
            bool bAttributesChanged = false;
            foreach (DimensionAttribute attribute in dimension.Attributes)
            {
                iAttrib++;
                foreach (AttributeRelationship attribRel in attribute.AttributeRelationships)
                {
                    if (attribRel.Name != attribRel.Attribute.Name) bAttributesChanged = true;
                    attribRel.Name = attribRel.Attribute.Name;
                }
                ApplicationObject.StatusBar.Progress(true, "Fixing Attribute Relationship Names...", iAttrib, attribCount);
            }
            return bAttributesChanged;
        }

    }
}