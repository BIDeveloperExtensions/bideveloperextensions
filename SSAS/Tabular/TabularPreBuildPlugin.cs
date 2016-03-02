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
using Microsoft.AnalysisServices.Common;
using Microsoft.DataWarehouse.Design;

namespace BIDSHelper
{
    public class TabularPreBuildPlugin : BIDSHelperPluginBase
    {
        private EnvDTE.BuildEvents _buildEvents;

        #region Standard Plugin Overrides
        public TabularPreBuildPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
            _buildEvents = appObject.Events.BuildEvents;
            _buildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(BuildEvents_OnBuildBegin);
        }

        public override string ShortName
        {
            get { return "TabularPreBuild"; }
        }

        public override int Bitmap
        {
            get { return 144; }
        }

        public override string ButtonText
        {
            get { return "Tabular Pre-Build..."; }
        }

        public override string FeatureName
        {
            get { return "Tabular Pre-Build"; }
        }

        public override string MenuName
        {
            get { return "Item"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSAS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Hooks the pre-build event in Visual Studio in order to restore some BIDS Helper customizations to the Tabular model if they have been lost."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            return false;
        }


        #endregion



        private void BuildEvents_OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
        {
            if (!this.Enabled) return;
            if (Action == vsBuildAction.vsBuildActionClean) return;

            foreach (UIHierarchyItem hierItem in VisualStudioHelpers.GetAllItemsFromSolutionExplorer(this.ApplicationObject.ToolWindows.SolutionExplorer))
            {
                if (hierItem.Name != null && hierItem.Name.ToLower().EndsWith(".bim"))
                {
                    Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = TabularHelpers.GetTabularSandboxFromBimFile(hierItem, false);
                    if (sandbox != null)
                    {
                        List<BIDSHelperPluginBase> checks = new List<BIDSHelperPluginBase>();
                        foreach (BIDSHelperPluginBase plugin in Connect.Plugins.Values)
                        {
                            Type t = plugin.GetType();
                            if (plugin is ITabularOnPreBuildAnnotationCheck
                                && !t.IsInterface
                                && !t.IsAbstract)
                            {
                                ITabularOnPreBuildAnnotationCheck check = (ITabularOnPreBuildAnnotationCheck)plugin;
                                if (check.TabularOnPreBuildAnnotationCheckPriority == TabularOnPreBuildAnnotationCheckPriority.HighPriority)
                                {
                                    checks.Insert(0, plugin); //insert the high priority checks at the front of the list so they get run first
                                }
                                else
                                {
                                    checks.Add(plugin);
                                }
                            }
                        }

                        foreach (BIDSHelperPluginBase plugin in checks)
                        {
                            ITabularOnPreBuildAnnotationCheck check = (ITabularOnPreBuildAnnotationCheck)plugin;
                            string sWarning = check.GetPreBuildWarning(sandbox);
                            if (sWarning != null)
                            {
                                if (MessageBox.Show(sWarning, "BIDS Helper Pre-Build Warning - " + plugin.FeatureName, MessageBoxButtons.OKCancel) == DialogResult.OK)
                                {
                                    Microsoft.VisualStudio.Project.Automation.OAFileItem project = hierItem.Object as Microsoft.VisualStudio.Project.Automation.OAFileItem;
                                    Window win = project.Open(EnvDTE.Constants.vsViewKindPrimary);
                                    win.Activate();
                                    check.FixPreBuildWarning(sandbox);
                                    project.Save(hierItem.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Exec()
        {
        }
    
    }
}
