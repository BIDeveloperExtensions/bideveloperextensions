using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.Common;
using Microsoft.DataWarehouse.Design;
using BIDSHelper.Core;
using Microsoft.VisualStudio.Shell.Interop;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASTabular)]
    public class TabularPreBuildPlugin : BIDSHelperBuildEventPluginBase
    {

        #region Standard Plugin Overrides
        public TabularPreBuildPlugin(BIDSHelperPackage package)
            : base(package)
        {
            //_buildEvents = appObject.Events.BuildEvents;
            //_buildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(BuildEvents_OnBuildBegin);
        }

        public override string ShortName
        {
            get { return "TabularPreBuild"; }
        }

        //public override int Bitmap
        //{
        //    get { return 144; }
        //}


        public override string FeatureName
        {
            get { return "Tabular Pre-Build"; }
        }

        //public override string MenuName
        //{
        //    get { return "Item"; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
        //}

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASTabular; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Hooks the pre-build event in Visual Studio in order to restore some BIDS Helper customizations to the Tabular model if they have been lost."; }
        }

        #endregion

        public override void Exec()
        {
        }

        internal override void OnUpdateConfigBegin(IVsHierarchy pHierProj, VSSOLNBUILDUPDATEFLAGS dwAction, ref int pfCancel)
        {
            if (!this.Enabled) return;
            if (dwAction ==  VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_CLEAN) return;

            foreach (UIHierarchyItem hierItem in VisualStudioHelpers.GetAllItemsFromSolutionExplorer(this.ApplicationObject.ToolWindows.SolutionExplorer))
            {
                if (hierItem.Name != null && hierItem.Name.ToLower().EndsWith(".bim"))
                {
                    Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = TabularHelpers.GetTabularSandboxFromBimFile(this, false);
                    if (sandbox == null)
                    {
                        var sandboxEditor = TabularHelpers.GetTabularSandboxEditorFromBimFile(hierItem, true);
                        if (sandboxEditor != null) sandbox = sandboxEditor.Sandbox;
                    }
                    if (sandbox != null)
                    {
#if !DENALI && !SQL2014
                        if (sandbox.IsTabularMetadata) return;
#endif
                        List<BIDSHelperPluginBase> checks = new List<BIDSHelperPluginBase>();
                        foreach (BIDSHelperPluginBase plugin in BIDSHelperPackage.Plugins.Values)
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
    }
}
