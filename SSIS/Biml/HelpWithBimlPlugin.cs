using EnvDTE;
using EnvDTE80;

namespace BIDSHelper.SSIS.Biml
{
    public class HelpWithBimlPlugin : BimlFeaturePluginBase
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
            get { return BIDSHelper.Resources.Common.Question; }
        }

        public override string ButtonText
        {
            get { return "Learn More About Biml"; }
        }

        public override string ToolTip
        {
            get { return "Obtain detailed reference and walkthrough documentation to help write Biml."; }
        }

        public override string MenuName
        {
            get { return "Item"; }
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
            System.Diagnostics.Process.Start(BIDSHelper.Resources.Common.BimlHelpUrl);
        }
    }
}