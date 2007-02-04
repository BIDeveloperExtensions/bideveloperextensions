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
    public class VisualizeAttributeLatticePlugin : BIDSHelperPluginBase
    {
        public VisualizeAttributeLatticePlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "VisualizeAttributeLattice"; }
        }

        public override int Bitmap
        {
            get { return 702; }
        }

        public override string ButtonText
        {
            get { return "Visualize Attribute Lattice"; }
        }

        public override string ToolTip
        {
            get { return "Displays a visualization of the attribute relationships in a dimension"; }
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
                if (hierItem.Name.ToLower().EndsWith(".dim"))
                    return true;
                else
                    return false;
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
                Dimension dim = (Dimension)projItem.Object;

                VisualizeAttributeLatticeForm frm = new VisualizeAttributeLatticeForm();
                frm.dimension = dim;
                frm.Show();

                //could use the icon from MicrosoftXMLEditor.CreateSchema command
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}