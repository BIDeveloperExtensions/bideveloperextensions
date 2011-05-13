using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Varigence.Flow.FlowFramework.Validation;
using Varigence.Languages.Biml;
using Varigence.Languages.Biml.Platform;

namespace BIDSHelper.SSIS.Biml
{
    public class BimlExpandPlugin : BimlFeaturePluginBase
    {
        public BimlExpandPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        #region Standard Property Overrides
        public override string ShortName
        {
            get { return "BimlExpandPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override System.Drawing.Icon CustomMenuIcon
        {
            get { return Properties.Resources.Biml; }
        }

        public override string ButtonText
        {
            get { return "Expand Biml File"; }
        }

        public override string ToolTip
        {
            get { return "Expand BimlScript file into your project"; }
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
            if (!BimlUtility.CheckRequiredFrameworkVersion())
            {
                return;
            }

            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            foreach (object selected in ((System.Array)solExplorer.SelectedItems))
            {
                UIHierarchyItem hierItem = (UIHierarchyItem)selected;
                ProjectItem projectItem = hierItem.Object as ProjectItem;
                if (projectItem != null && projectItem.Name.ToLower().EndsWith(".biml")) 
                {
                    string projectDirectory = Path.GetDirectoryName(projectItem.ContainingProject.FullName);
                    try
                    {
                        string filePath = Path.Combine(projectDirectory, projectItem.Name);
                        if (projectItem.Document != null && !projectItem.Document.Saved)
                        {
                            projectItem.Document.Save(null);
                        }

                        Expand(filePath, projectItem.ContainingProject, projectDirectory);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void Expand(string bimlScriptPath, Project project, string projectDirectory)
        {
            var tempTargetDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(tempTargetDirectory);

                // TODO: How to distinguish between SQL Server 2008 and 2008R2?
                #if KATMAI
                SsisVersion ssisVersion = BimlUtility.GetSsisVersion2008Variant();
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Hadron.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPath, tempTargetDirectory, projectDirectory, string.Empty, SqlServerVersion.SqlServer2008, ssisVersion, SsasVersion.Ssas2008);
                #else
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Hadron.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPath, tempTargetDirectory, projectDirectory, string.Empty, SqlServerVersion.SqlServer2005, SsisVersion.Ssis2005, SsasVersion.Ssas2005);
                #endif

                if (validationReporter.HasErrors)
                {
                    var form = new BimlValidationListForm(validationReporter);
                    form.ShowDialog();
                }
                else
                {
                    string[] newPackageFiles = Directory.GetFiles(tempTargetDirectory, "*.dtsx", SearchOption.AllDirectories);
                    var safePackageFilePaths = new List<string>();
                    var conflictingPackageFilePaths = new List<string>();
                    foreach (var tempFilePath in newPackageFiles)
                    {
                        string tempFileName = Path.GetFileName(tempFilePath);
                        string projectItemFileName = Path.Combine(projectDirectory, tempFileName);
                        if (File.Exists(projectItemFileName))
                        {
                            conflictingPackageFilePaths.Add(tempFilePath);
                        }
                        else
                        {
                            safePackageFilePaths.Add(tempFilePath);
                        }
                    }

                    if (conflictingPackageFilePaths.Count > 0)
                    {
                        var dialog = new MultipleSelectionConfirmationDialog(conflictingPackageFilePaths, projectDirectory, safePackageFilePaths.Count);
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            foreach (var filePath in dialog.SelectedFilePaths)
                            {
                                safePackageFilePaths.Add(filePath);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    foreach (var tempFilePath in safePackageFilePaths)
                    {
                        string projectItemFilePath = Path.Combine(projectDirectory, Path.GetFileName(tempFilePath));
                        File.Copy(tempFilePath, projectItemFilePath, true);
                        project.ProjectItems.AddFromFile(projectItemFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempTargetDirectory))
                    {
                        Directory.Delete(tempTargetDirectory);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}