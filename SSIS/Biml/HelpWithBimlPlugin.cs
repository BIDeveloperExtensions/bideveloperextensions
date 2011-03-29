using EnvDTE;
using EnvDTE80;
using System.Text;
using Microsoft.DataWarehouse.Design;
using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.DataTransformationServices.Project;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project.Configuration;
using Microsoft.DataWarehouse.Project;
using Microsoft.DataWarehouse.VsIntegration.Shell;
using System.Windows.Forms;
using Microsoft.VisualStudio.CommandBars;
using System.IO;

namespace BIDSHelper.SSIS.Biml
{
    public class HelpWithBimlPlugin : BIDSHelperPluginBase
    {
        public HelpWithBimlPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        #region Standard Property Overrides
        public override string ShortName
        {
            get { return "HelpWithBimlPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override System.Drawing.Icon CustomMenuIcon
        {
            get { return Properties.Resources.Question; }
        }

        public override string ButtonText
        {
            get { return "Learn More About Biml"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        public override string MenuName
        {
            get { return "Item"; }
        }

        public override string FriendlyName
        {
            get { return "Learn More About Biml"; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get { return "Obtain detailed reference and walkthrough documentation to help write Biml."; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }
        #endregion

        public override bool DisplayCommand(UIHierarchyItem item)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            foreach (object selected in ((System.Array)solExplorer.SelectedItems))
            {
                UIHierarchyItem hierItem = (UIHierarchyItem)selected;
                ProjectItem projectItem = hierItem.Object as ProjectItem;
                if (projectItem == null || !projectItem.Name.ToLower().EndsWith(".biml")) 
                {
                    return false;
                }
            }

            return (((System.Array)solExplorer.SelectedItems).Length > 0);
        }

        public override void Exec()
        {
            System.Diagnostics.Process.Start("http://www.varigence.com/documentation/biml");
        }
    }
}