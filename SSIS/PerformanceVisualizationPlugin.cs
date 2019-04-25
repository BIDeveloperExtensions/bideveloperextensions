using EnvDTE;
using EnvDTE80;
using System.Text;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using System;
using Microsoft.SqlServer.Dts.Runtime;
using System.Windows.Forms;
using Microsoft.DataWarehouse.Controls;
using System.Collections.Generic;
using BIDSHelper.SSIS.PerformanceVisualization;
using BIDSHelper.Core;

namespace BIDSHelper.SSIS
{
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class PerformanceVisualizationPlugin : BIDSHelperPluginBase
    {
        //private DTEEvents events;
        public PerformanceVisualizationPlugin(BIDSHelperPackage package)
            : base(package)
        {
         //   this.events = this.ApplicationObject.Events.DTEEvents;
         //   this.events.ModeChanged += new _dispDTEEvents_ModeChangedEventHandler(DTEEvents_ModeChanged);

            CreateContextMenu(CommandList.PerformanceVisualizationId, ".dtsx");

        }

        public override void OnEnable()
        {
            base.OnEnable();
            package.IdeModeChanged += Package_IdeModeChanged;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            package.IdeModeChanged -= Package_IdeModeChanged;
        }

        private void Package_IdeModeChanged(object sender, enumIDEMode newMode)
        {
       
            try
            {
                if (newMode != enumIDEMode.Debug) return;
                StringBuilder sb = new StringBuilder();
                foreach (PerformanceTab tab in PerformanceEditorViews.Values)
                {
                    if (tab.IsExecuting)
                        sb.AppendLine(tab.PackageName);
                }
                if (sb.Length > 0)
                {
                    DialogResult result = MessageBox.Show("BIDS Helper is currently executing the following packages to capture performance statistics:\r\n\r\n" + sb.ToString() + "\r\nAre you sure you want to run something else?\r\n\r\nClick OK to continue debugging. BIDS Helper will continue executing these packages.\r\nClick Cancel to cancel debugging. BIDS Helper will continue executing these packages.", "BIDS Helper SSIS Performance Visualization In Progress", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.Cancel)
                    {
                        this.ApplicationObject.Debugger.Stop(false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public override string ShortName
        {
            get { return "SSISPerformanceVisualizationPlugin"; }
        }

        //public override int Bitmap
        //{
        //    get { return 0; }
        //}

        //public override System.Drawing.Icon CustomMenuIcon
        //{
        //    get { return BIDSHelper.Resources.Common.Performance; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Execute and Visualize Performance"; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        public override string FeatureName
        {
            get { return "SSIS Performance Visualization"; }
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
            get { return "Adds a new Performance tab with a graphical Gantt chart view of the execution durations and dependencies for your package to help you visualize performance."; }
        }

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                ProjectItem pi = (ProjectItem)hierItem.Object;

                ExecutePackage(pi, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        internal static Dictionary<EditorWindow, PerformanceTab> PerformanceEditorViews = new Dictionary<EditorWindow, PerformanceTab>();

        internal static void ExecutePackage(ProjectItem pi, string DataFlowGUID)
        {
            try
            {
                if (pi.DTE.Mode == vsIDEMode.vsIDEModeDebug)
                {
                    MessageBox.Show("Please stop the debugger first.");
                    return;
                }

                Window w = pi.Open(BIDSViewKinds.Designer); //opens the designer
                w.Activate();

                IDesignerHost designer = w.Object as IDesignerHost;
                if (designer == null) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                if (win == null || (win.PropertiesLinkComponent as Package) == null)
                {
                    MessageBox.Show("Package designer is not open yet. Try again in a moment.");
                    return;
                }
                Package package = (Package)win.PropertiesLinkComponent;
                PackageHelper.SetTargetServerVersion(package);

                EditorWindow.EditorView view = null;
                if (PerformanceEditorViews.ContainsKey(win))
                {
                    PerformanceTab tab = PerformanceEditorViews[win];
                    view = tab.ParentEditorView;
                    if (tab.IsExecuting)
                    {
                        MessageBox.Show("You may not execute the package until the previous execution of the package completes.");
                    }
                    else
                    {
                        if (DataFlowGUID == null)
                            tab.ExecutePackage();
                        else
                            tab.BreakdownPipelinePerformance(DataFlowGUID);
                    }
                }
                else
                {
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);

                    PerformanceTabControlDelegateContainer delegateContainer = new PerformanceTabControlDelegateContainer();
                    view = new EditorWindow.EditorView(new EditorViewLoadDelegate(delegateContainer.CreatePerformanceTabControl), "Performance", "Visualize SSIS package execution performance (BIDS Helper)", 0, BIDSHelper.Resources.Common.Performance);
                    delegateContainer.win = win;
                    delegateContainer.view = view;
                    delegateContainer.projectItem = pi;
                    delegateContainer.DataFlowGUID = DataFlowGUID;
                    win.Views.Add(view);
                    win.EnsureViewIsLoaded(view); //delegate will be called here
                }
                win.SelectedView = view;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private static void win_ActiveViewChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is EditorWindow)
                {
                    EditorWindow win = (EditorWindow)sender;
                    if (PerformanceEditorViews.ContainsKey(win))
                    {
                        PerformanceTab tab = PerformanceEditorViews[(EditorWindow)sender];
                        if (win.SelectedView == tab.ParentEditorView)
                        {
                            //flip a flag so some Microsoft code doesn't run... the PerformanceTab.OnGotFocus event will restore the value
                            win.GetType().InvokeMember("bSetNewSelection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.Instance, null, win, new object[] { false });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        internal class PerformanceTabControlDelegateContainer
        {
            public EditorWindow win;
            public ProjectItem projectItem;
            public EditorWindow.EditorView view;
            public string DataFlowGUID;

            public Control CreatePerformanceTabControl(VsStyleToolBar pageViewToolBar)
            {
                SSIS.PerformanceVisualization.PerformanceTab tab = new SSIS.PerformanceVisualization.PerformanceTab();
                tab.LayoutToolBar(pageViewToolBar);
                tab.Init(win, view, projectItem, DataFlowGUID);
                PerformanceVisualizationPlugin.PerformanceEditorViews.Add(win, tab);
                return tab;
            }
        }
    }
}