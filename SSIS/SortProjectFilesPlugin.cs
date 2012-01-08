using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using System.Data;

#if KATMAI || DENALI
//Katmai implemented this feature
#else
namespace BIDSHelper
{
    public class SortProjectFilesPlugin : BIDSHelperPluginBase
    {
        public SortProjectFilesPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "SortProjectFilesPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; } //TODO add sort items icon
        }

        public override string ButtonText
        {
            get { return "Sort by name"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        public override bool AddCommandToMultipleMenus
        {
            get { return true; }
        }

        public override string MenuName
        {
            get { return "Project Node"; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Adds a 'Sort by name' menu option to the SSIS Packages folder allowing you to easily re-order the packages. (Available in SQL Server 2005 Only)"; }
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
                // Check that we are looking at the "SSIS Packages" node by checking that there is 
                // more than one child item and that the first item (in the 1 based collection)
                // ends with ".dtsx"
                return (hierItem.Collection.Count > 0 && hierItem.UIHierarchyItems.Item(1).Name.EndsWith(".dtsx"));
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
                ProjectItem pi = (ProjectItem)hierItem.Object;
                Project p = pi.ContainingProject;

                Microsoft.DataWarehouse.VsIntegration.Shell.Project.FileProjectVirtualFolder folder = hierItem.Object.GetType().InvokeMember("projectNode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy, null, hierItem.Object, null) as Microsoft.DataWarehouse.VsIntegration.Shell.Project.FileProjectVirtualFolder;
                if (folder == null) throw new Exception("Could not get FileProjectVirtualFolder");
                Microsoft.DataWarehouse.VsIntegration.Hierarchy.ISortableHierarchyCollection children = folder.Children as Microsoft.DataWarehouse.VsIntegration.Hierarchy.ISortableHierarchyCollection;
                if (children == null) throw new Exception("Could not get ISortableHierarchyCollection");
                children.Sort();

                //mark the project as dirty
                Microsoft.DataWarehouse.Interfaces.IConfigurationSettings settings = (Microsoft.DataWarehouse.Interfaces.IConfigurationSettings)((System.IServiceProvider)p).GetService(typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings));
                Microsoft.DataWarehouse.Project.DataWarehouseProjectManager projectManager = (Microsoft.DataWarehouse.Project.DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);
                projectManager.GetType().InvokeMember("MarkTextBufferAsUnsaved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.InvokeMethod, null, projectManager, new object[] { });
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
#endif