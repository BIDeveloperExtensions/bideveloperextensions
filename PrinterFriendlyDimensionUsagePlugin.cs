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
    public class PrinterFriendlyDimensionUsagePlugin : BIDSHelperPluginBase
    {
        public PrinterFriendlyDimensionUsagePlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "PrinterFriendlyDimensionUsage"; }
        }

        public override int Bitmap
        {
            get { return 3983; }
        }

        public override string ButtonText
        {
            get { return "Printer Friendly DimensionUsage"; }
        }

        public override string ToolTip
        {
            get { return "Displays a Printer Friendly version of the DimensionUsage tab"; }
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

                ReportViewerForm frm = new ReportViewerForm();
                frm.ReportBindingSource.DataSource = PrinterFriendlyDimensionUsage.GetDimensionUsage(cub);
                frm.Report = "PrinterFriendlyDimensionUsage.rdlc";
                frm.Caption = "Printer Friendly Dimension Usage";
                frm.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}