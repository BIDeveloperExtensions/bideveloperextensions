using EnvDTE;
using EnvDTE80;

namespace BIDSHelper.SSIS.Biml
{
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class HelpWithBimlPlugin : BimlFeaturePluginBase
    {
        public HelpWithBimlPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(Core.CommandList.LearnMoreAboutBimlId);
        }

        #region Standard Property Overrides
        public override string ShortName
        {
            get { return "HelpWithBimlPlugin"; }
        }
        

        //public override System.Drawing.Icon CustomMenuIcon
        //{
        //    get { return BIDSHelper.Resources.Common.Question; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Learn More About Biml"; }
        //}

        public override string ToolTip
        {
            get { return "Obtain detailed reference and walkthrough documentation to help write Biml."; }
        }
        
        #endregion

        public override bool ShouldDisplayCommand()
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
            if (Biml.BimlUtility.ShowDisabledMessage()) return;
            System.Diagnostics.Process.Start(BIDSHelper.Resources.Common.BimlHelpUrl);
        }
    }
}