using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;


namespace BIDSHelper
{
    public class AggregationManagerPlugin : BIDSHelperPluginBase
    {
        public AggregationManagerPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "AggregationManager"; }
        }

        public override int Bitmap
        {
            get { return 3984; }
        }

        public override string ButtonText
        {
            get { return "Edit Aggregations..."; }
        }

        public override string ToolTip
        {
            get { return "Allows for Manually editing aggregation designs"; }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
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
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                return (((ProjectItem)hierItem.Object).Object is Cube);
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