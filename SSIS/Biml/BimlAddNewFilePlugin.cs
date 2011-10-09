using System;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;


namespace BIDSHelper.SSIS.Biml
{
    public class BimlAddNewFilePlugin : BimlFeaturePluginBase
    {
        public BimlAddNewFilePlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

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
            get { return BIDSHelper.Resources.Common.BimlFile; }
        }

        public override string ButtonText
        {
            get { return "Add New Biml File"; }
        }

        public override string ToolTip
        {
            get { return "Add a new Biml file to your project."; }
        }

        public override string MenuName
        {
            get { return "Project,Project Node"; }
        }

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