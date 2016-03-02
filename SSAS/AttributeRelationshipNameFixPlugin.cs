using System;
using System.Collections.Generic;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.Data;
using System.ComponentModel.Design;

namespace BIDSHelper
{
    public class AttributeRelationshipNameFixPlugin : BIDSHelperPluginBase
    {
        
        public AttributeRelationshipNameFixPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "AttributeRelationshipNameFix"; }
        }

        public override int Bitmap
        {
            get { return 4380; } // TODO - choose new bitmap
        }

        public override string ButtonText
        {
            get { return "Attribute Relationship Name Fix"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; /*doesn't show anywhere*/ }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSAS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Will synchronise the attribute relationship name with the attribute id to fix the warnings caused by renaming attributes without renaming the relationships."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            try
            {
                UIHierarchy solExplorer = ApplicationObject.ToolWindows.SolutionExplorer;
                if (((Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                var hierItem = ((UIHierarchyItem)((Array)solExplorer.SelectedItems).GetValue(0));
                return (((ProjectItem)hierItem.Object).Object is Dimension);
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

                FixAttributeRelationshipNames(d);

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

        private void FixAttributeRelationshipNames(Dimension dimension)
        {
            int attribCount = dimension.Attributes.Count;
            int iAttrib = 0;
            foreach (DimensionAttribute attribute in dimension.Attributes)
            {
                iAttrib++;
                foreach (AttributeRelationship attribRel in attribute.AttributeRelationships)
                {
                    attribRel.Name = attribRel.AttributeID;
                }
                ApplicationObject.StatusBar.Progress(true, "Fixing Attribute Relationship Names...", iAttrib, attribCount);   
            }
        }
       
    }
}