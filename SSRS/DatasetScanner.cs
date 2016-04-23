using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BIDSHelper.SSRS
{
    public static class DatasetScanner
    {
        public static void ScanReports(UIHierarchy solExplorer, bool LookForUnusedDatasets)
        {
            string sCurrentFile = string.Empty;
            try
            {
                //UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;

                List<string> lstRdls = new List<string>();
                if (hierItem.Object is Project)
                {
                    Project p = (Project)hierItem.Object;
                    lstRdls.AddRange(GetRdlFilesInProjectItems(p.ProjectItems, true));
                }
                else if (hierItem.Object is ProjectItem)
                {
                    ProjectItem pi = hierItem.Object as ProjectItem;
                    Project p = pi.SubProject;
                    lstRdls.AddRange(GetRdlFilesInProjectItems(p.ProjectItems, true));
                }
                else if (solution != null)
                {
                    foreach (Project p in solution.Projects)
                    {
                        lstRdls.AddRange(GetRdlFilesInProjectItems(p.ProjectItems, true));
                    }
                }

                List<UsedRsDataSets.RsDataSetUsage> lstDataSets = new List<UsedRsDataSets.RsDataSetUsage>();
                foreach (string file in lstRdls)
                {
                    sCurrentFile = file;
                    UsedRsDataSets urds = new UsedRsDataSets(file);
                    foreach (UsedRsDataSets.RsDataSet ds in urds.DataSets)
                    {
                        if (LookForUnusedDatasets && ds.Usages.Count == 0)
                        {
                            UsedRsDataSets.RsDataSetUsage u = new UsedRsDataSets.RsDataSetUsage();
                            u.ReportName = ds.ReportName;
                            u.DataSetName = ds.DataSetName;
                            lstDataSets.Add(u);
                        }
                        else if (!LookForUnusedDatasets && ds.Usages.Count > 0)
                        {
                            foreach (string usage in ds.Usages)
                            {
                                UsedRsDataSets.RsDataSetUsage u = new UsedRsDataSets.RsDataSetUsage();
                                u.ReportName = ds.ReportName;
                                u.DataSetName = ds.DataSetName;
                                u.Usage = usage;
                                lstDataSets.Add(u);
                            }
                        }
                    }
                }

                if (lstDataSets.Count == 0)
                {
                    if (LookForUnusedDatasets)
                        MessageBox.Show("All datasets are in use.", "BIDS Helper Unused Datasets Report");
                    else
                        MessageBox.Show("No datasets found.", "BIDS Helper Used Datasets Report");
                }
                else
                {
                    ReportViewerForm frm = new ReportViewerForm();
                    frm.ReportBindingSource.DataSource = lstDataSets;
                    if (LookForUnusedDatasets)
                    {
                        frm.Report = "SSRS.UnusedDatasets.rdlc";
                        frm.Caption = "Unused Datasets Report";
                    }
                    else
                    {
                        frm.Report = "SSRS.UsedDatasets.rdlc";
                        frm.Caption = "Used Datasets Report";
                    }
                    Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                    reportDataSource1.Name = "BIDSHelper_SSRS_RsDataSetUsage";
                    reportDataSource1.Value = frm.ReportBindingSource;
                    frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);

                    frm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                    frm.Show();
                }
            }
            catch (System.Exception ex)
            {
                string sError = string.Empty;
                if (!string.IsNullOrEmpty(sCurrentFile)) sError += "Error while scanning report: " + sCurrentFile + "\r\n";
                while (ex != null)
                {
                    sError += ex.Message + "\r\n" + ex.StackTrace + "\r\n\r\n";
                    ex = ex.InnerException;
                }
                MessageBox.Show(sError);
            }
        }

        public static string[] GetRdlFilesInProjectItems(ProjectItems pis, bool bGetRDLC)
        {
            if (pis == null) return new string[] { };

            List<string> lst = new List<string>();
            foreach (ProjectItem pi in pis)
            {
                if (pi.SubProject != null)
                {
                    lst.AddRange(GetRdlFilesInProjectItems(pi.SubProject.ProjectItems, bGetRDLC));
                }
                else if (pi.Name.ToLower().EndsWith(".rdl") || (bGetRDLC && pi.Name.ToLower().EndsWith(".rdlc")))
                {
                    lst.Add(pi.get_FileNames(1));
                }
                lst.AddRange(GetRdlFilesInProjectItems(pi.ProjectItems, bGetRDLC));
            }
            return lst.ToArray();
        }

    }
}
