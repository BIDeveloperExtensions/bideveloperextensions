
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using BIDSHelper.Core;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class PrinterFriendlyDimensionUsagePlugin : BIDSHelperPluginBase
    {
        public PrinterFriendlyDimensionUsagePlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.PrinterFriendlyDimensionUsageId, new string[] { ".cube", ".bim" } );
        }

        public override string ShortName
        {
            get { return "PrinterFriendlyDimensionUsage"; }
        }

        //public override int Bitmap
        //{
        //    get { return 3983; }
        //}

        public override string FeatureName
        {
            get { return "Printer Friendly Dimension Usage"; }
        }

        public override string ToolTip
        {
            get { return "Displays a Printer Friendly version of the DimensionUsage tab"; }
        }

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
        //}

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
            get { return "Displays a printer friendly version of Dimension Usage, showing relationships between dimensions and measure groups."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        //public override bool DisplayCommand(UIHierarchyItem item)
        //{
        //    try
        //    {
        //        UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
        //        if (((System.Array)solExplorer.SelectedItems).Length != 1)
        //            return false;
                
        //        UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
        //        string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
        //        if (sFileName.EndsWith(".bim"))
        //        {
        //            return true;
        //        }

        //        return (((ProjectItem)hierItem.Object).Object is Cube);
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;

                string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
#if DENALI || SQL2014
                Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = null;
#else
                //Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo sandbox = null;
                DataModelingSandboxWrapper sandbox = null;
#endif
                bool bIsTabular = false;
                Cube cub = null;
                if (projItem.Object is Cube)
                {
                    cub = (Cube)projItem.Object;
                }
                else if (sFileName.EndsWith(".bim"))
                {
#if DENALI || SQL2014
                    sandbox = TabularHelpers.GetTabularSandboxFromBimFile(this, true);
                    cub = sandbox.Cube;
                    bIsTabular = true;
                    Microsoft.AnalysisServices.BackEnd.IDataModelingObjectCollection<Microsoft.AnalysisServices.BackEnd.DataModelingMeasure> measures = sandbox.Measures;
#else
                    sandbox = new DataModelingSandboxWrapper(this);
                    //sandbox = TabularHelpers.GetTabularSandboxAmoFromBimFile(this, true);
                    //cub = sandbox.Cube;
                    bIsTabular = true;
                    //Microsoft.AnalysisServices.BackEnd.IDataModelingObjectCollection<Microsoft.AnalysisServices.BackEnd.DataModelingMeasure> measures = sandbox.Cube.AllMeasures;
#endif

                }
                else
                {
                    //if you are launching this feature from the Dimension Usage tab, but some other item in Solution Explorer is highlighted, then this code works and the above doesn't
                    projItem = this.ApplicationObject.ActiveWindow.ProjectItem;
                    cub = (Cube)projItem.Object;
                }

                DialogResult res = MessageBox.Show("Would you like a detailed report?\r\n\r\nPress Yes to see a detailed dimension usage report.\r\n\r\nPress No to see a summary level Bus Matrix dimension usage report.", "BIDS Helper - Printer Friendly Dimension Usage - Which Report Type?", MessageBoxButtons.YesNo);

                ReportViewerForm frm = new ReportViewerForm();

                if (bIsTabular)
                {

                    bool bIsBusMatrix = (res == DialogResult.No);
                    System.Collections.Generic.List<DimensionUsage> list = PrinterFriendlyDimensionUsage.GetTabularDimensionUsage(sandbox, bIsBusMatrix);
                    DeploymentSettings _deploymentSettings = new DeploymentSettings(projItem);

                    //reset the database and cube name per the deployment settings
                    foreach (DimensionUsage du in list)
                    {
                        du.DatabaseName = _deploymentSettings.TargetDatabase;
                        du.CubeName = _deploymentSettings.TargetCubeName;
                    }

                    frm.ReportBindingSource.DataSource = list;

                }
                else
                {
                    frm.ReportBindingSource.DataSource = PrinterFriendlyDimensionUsage.GetDimensionUsage(cub);
                }

                if (res == DialogResult.No)
                    frm.Report = "SSAS.PrinterFriendlyDimensionUsageBusMatrix.rdlc";
                else
                    frm.Report = "SSAS.PrinterFriendlyDimensionUsage.rdlc";

                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "BIDSHelper_DimensionUsage";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                if (res == DialogResult.No)
                {
                    frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = "BIDSHelper.PrinterFriendlyDimensionUsageBusMatrix.rdlc";
                    frm.Caption = "Printer Friendly Dimension Usage Bus Matrix";
                }
                else
                {
                    frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = "BIDSHelper.PrinterFriendlyDimensionUsage.rdlc";
                    frm.Caption = "Printer Friendly Dimension Usage";
                }
                frm.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}