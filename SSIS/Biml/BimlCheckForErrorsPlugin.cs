using System;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Varigence.Flow.FlowFramework.Validation;
using Varigence.Languages.Biml;
using Varigence.Languages.Biml.Platform;

namespace BIDSHelper.SSIS.Biml
{
    public class BimlCheckForErrorsPlugin : BimlFeaturePluginBase
    {
        public BimlCheckForErrorsPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "BimlCheckForErrorsPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override System.Drawing.Icon CustomMenuIcon
        {
            get { return Properties.Resources.CheckBiml; }
        }

        public override string ButtonText
        {
            get { return "Check Biml for Errors"; }
        }

        public override string ToolTip
        {
            get { return "Checks the BimlScript file for errors and warnings."; }
        }

        public override string MenuName
        {
            get { return "Item"; }
        }

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

                        ShowValidationItems(filePath, projectItem.ContainingProject, projectDirectory);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void ShowValidationItems(string bimlScriptPath, Project project, string projectDirectory)
        {
            var tempTargetDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(tempTargetDirectory);

                // TODO: How to distinguish between SQL Server 2008 and 2008R2?  Check for 10.5 version in registry.
                // TODO: DENALI support
                #if KATMAI
                SsisVersion ssisVersion = BimlUtility.GetSsisVersion2008Variant();
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Hadron.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPath, tempTargetDirectory, projectDirectory, string.Empty, SqlServerVersion.SqlServer2008, ssisVersion, SsasVersion.Ssas2008);
                #else
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Hadron.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPath, tempTargetDirectory, projectDirectory, string.Empty, SqlServerVersion.SqlServer2005, SsisVersion.Ssis2005, SsasVersion.Ssas2005);
                #endif

                if (!validationReporter.HasErrors && !validationReporter.HasWarnings)
                {
                    MessageBox.Show("No errors or warnings were found.");
                }
                else
                {
                    var form = new BimlValidationListForm(validationReporter);
                    form.ShowDialog();
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