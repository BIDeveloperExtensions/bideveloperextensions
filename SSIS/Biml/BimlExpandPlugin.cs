using EnvDTE;
using EnvDTE80;
using System.Text;
using Microsoft.DataWarehouse.Design;
using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.DataTransformationServices.Project;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project.Configuration;
using Microsoft.DataWarehouse.Project;
using Microsoft.DataWarehouse.VsIntegration.Shell;
using System.Windows.Forms;
using Microsoft.VisualStudio.CommandBars;
using System.IO;

using Varigence.Flow.FlowFramework;
using Varigence.Flow.FlowFramework.Engine;
using Varigence.Flow.FlowFramework.Engine.Binding;
using Varigence.Flow.FlowFramework.Engine.IR;
using Varigence.Flow.FlowFramework.Engine.Kernel;
using Varigence.Flow.FlowFramework.Model;
using Varigence.Flow.FlowFramework.Utility;
using Varigence.Flow.FlowFramework.Validation;
using Varigence.Languages.Biml;
using Varigence.Languages.Biml.Platform;
using Varigence.Utility.TextTemplating.Engine;
using Varigence.Utility.TextTemplating.Hosts;

namespace BIDSHelper.SSIS.Biml
{
    public class BimlExpandPlugin : BIDSHelperPluginBase
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
            get { return string.Empty; }
        }

        public override string MenuName
        {
            get { return "Item"; }
        }

        public override string FriendlyName
        {
            get { return "Expand Biml File"; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get { return "Expand BimlScript file into your project."; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
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
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Hadron.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPath, tempTargetDirectory, string.Empty, SqlServerVersion.SqlServer2008, SsisVersion.Ssis2008, SsasVersion.Ssas2008);
                #else
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Hadron.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPath, tempTargetDirectory, string.Empty, SqlServerVersion.SqlServer2005, SsisVersion.Ssis2005, SsasVersion.Ssas2005);
                #endif

                if (validationReporter.HasErrors)
                {
                    var form = new BimlValidationListForm(validationReporter);
                    form.ShowDialog();
                }
                else
                {
                    foreach (var tempFileName in Directory.GetFiles(tempTargetDirectory, "*.dtsx", SearchOption.AllDirectories))
                    {
                        string projectItemFileName = Path.Combine(projectDirectory, Path.GetFileName(tempFileName));
                        File.Copy(tempFileName, projectItemFileName, true);
                        project.ProjectItems.AddFromFile(projectItemFileName);
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