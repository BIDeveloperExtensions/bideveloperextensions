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
    public class BimlAddNewFilePlugin : BIDSHelperPluginBase
    {
        public BimlAddNewFilePlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        #region Standard Property Overrides
        public override string ShortName
        {
            get { return "BimlAddNewFilePlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override System.Drawing.Icon CustomMenuIcon
        {
            get { return Properties.Resources.BimlFile; }
        }

        public override string ButtonText
        {
            get { return "Add New Biml File"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        public override string MenuName
        {
            get { return "Project,Project Node"; }
        }

        public override string FriendlyName
        {
            get { return "Add New Biml File"; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get { return "Add a new Biml file to your project."; }
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
            if (((System.Array)solExplorer.SelectedItems).Length == 1)
            {
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                Project project = hierItem.Object as Project;
                ProjectItem projectItem = hierItem.Object as ProjectItem;
                if (project != null)
                {
                    return (project.Kind == BIDSProjectKinds.SSIS);
                }
                else if (projectItem != null)
                {
                    return projectItem.ContainingProject.Kind == BIDSProjectKinds.SSIS;
                }
            }

            return false;
        }

        private const string NewBimlFileContents = @"<Biml xmlns=""http://schemas.varigence.com/biml.xsd"">
</Biml>";

        public override void Exec()
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
            Project project = hierItem.Object as Project;
            ProjectItem projectItem = hierItem.Object as ProjectItem;
            if (project == null && projectItem != null)
            {
                project = projectItem.ContainingProject;
            }
            
            string projectDirectory = Path.GetDirectoryName(project.FullName);
            try
            {
                int index = 0;
                string fileRoot = Path.Combine(projectDirectory, "BimlScript");
                string currentFileName = fileRoot + ".biml";
                while (File.Exists(currentFileName))
                {
                    ++index;
                    currentFileName = fileRoot + index + ".biml";
                }

                File.WriteAllText(currentFileName, NewBimlFileContents);
                project.ProjectItems.AddFromFile(currentFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}