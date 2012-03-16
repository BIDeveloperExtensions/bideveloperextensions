using System;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Varigence.Flow.FlowFramework.Validation;
using Varigence.Languages.Biml;
using Varigence.Languages.Biml.Platform;
using System.Collections.Generic;

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
            get { return BIDSHelper.Resources.Common.CheckBiml; }
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
            var emittableFilePaths = new List<string>();
            Project containingProject = null;
            string projectDirectory = null;
            bool sameProject = false;
            foreach (object selected in ((System.Array)solExplorer.SelectedItems))
            {
                UIHierarchyItem hierItem = (UIHierarchyItem)selected;
                ProjectItem projectItem = hierItem.Object as ProjectItem;
                if (projectItem != null && projectItem.Name.ToLower().EndsWith(".biml")) 
                {
                    if (projectItem.Document != null && !projectItem.Document.Saved)
                    {
                        projectItem.Document.Save(null);
                    }

                    Project newContainingProject = projectItem.ContainingProject;
                    sameProject = containingProject == null || containingProject == newContainingProject;
                    containingProject = projectItem.ContainingProject;
                    projectDirectory = Path.GetDirectoryName(containingProject.FullName);
                    string filePath = Path.Combine(projectDirectory, projectItem.Name);
                    emittableFilePaths.Add(filePath);
                }
            }

            if (sameProject && containingProject != null && projectDirectory != null)
            {
                try
                {
                    ShowValidationItems(emittableFilePaths, containingProject, projectDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ShowValidationItems(List<string> bimlScriptPaths, Project project, string projectDirectory)
        {
            var tempTargetDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(tempTargetDirectory);

                // TODO: How to distinguish between SQL Server 2008 and 2008R2?  Check for 10.5 version in registry.
                // TODO: DENALI support
                #if KATMAI
                SsisVersion ssisVersion = BimlUtility.GetSsisVersion2008Variant();
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Hadron.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2008, ssisVersion, SsasVersion.Ssas2008);
                #elif DENALI
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Hadron.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2008, SsisVersion.Ssis2012, SsasVersion.Ssas2008);
                #else
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Hadron.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2005, SsisVersion.Ssis2005, SsasVersion.Ssas2005);
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