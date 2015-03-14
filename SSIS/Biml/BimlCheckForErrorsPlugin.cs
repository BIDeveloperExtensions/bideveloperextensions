namespace BIDSHelper.SSIS.Biml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.DataWarehouse.Design;
    using Varigence.Flow.FlowFramework.Validation;

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

                // Get the General output window, we use it to write out our BIML "compilation" messages
                IOutputWindowFactory service = ((System.IServiceProvider)project).GetService(typeof(IOutputWindowFactory)) as IOutputWindowFactory;
                IOutputWindow outputWindow = service.GetStandardOutputWindow(StandardOutputWindow.General);
                outputWindow.Clear();
                outputWindow.ReportStatusMessage("Validating BIML");

                ValidationReporter validationReporter = BimlUtility.GetValidationReporter(bimlScriptPaths, project, projectDirectory, tempTargetDirectory);

                // If we have no errors and no warnings, say so
                if (!validationReporter.HasErrors && !validationReporter.HasWarnings)
                {
                    // Write a closing message to the output window
                    outputWindow.ReportStatusMessage("No errors or warnings were found.");
                    outputWindow.ReportStatusMessage("BIML validation completed.");

                    // Show message to user
                    MessageBox.Show("No errors or warnings were found.", DefaultMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // We have errors and/or warnings. Show even warnings in for this Check plug-in, as opposed to Expand plug-in which only stops for errors.
                    BimlUtility.ProcessValidationReport(outputWindow, validationReporter, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {

                // Clean up the temporary directory and files, but supress any errors encountered
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