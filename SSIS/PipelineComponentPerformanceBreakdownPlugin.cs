namespace BIDSHelper.SSIS
{
    using EnvDTE;
    using EnvDTE80;
    using System.ComponentModel.Design;
    using Microsoft.DataWarehouse.Design;
    using System;
    using Microsoft.SqlServer.Dts.Runtime;
    using System.Windows.Forms;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
    using Core;

    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class PipelineComponentPerformanceBreakdownPlugin : BIDSHelperPluginBase
    {
        private static System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;

        public PipelineComponentPerformanceBreakdownPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.PerformanceBreakdownId);
        }

        public override string ShortName
        {
            get { return "PipelineComponentPerformanceBreakdownPlugin"; }
        }


        //public override System.Drawing.Icon CustomMenuIcon
        //{
        //    get { return BIDSHelper.Resources.Common.Performance; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Performance Breakdown"; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        public override string FeatureName
        {
            get { return "Pipeline Component Performance Breakdown"; }
        }

        //public override string MenuName
        //{
        //    get
        //    {
        //        return "SSIS Designer,Component Menu";
        //    }
        //}


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
            get { return "Executes an individual Data Flow task and displays a performance breakdown for each component in the task, helping you to identify any bottlenecks you may have."; }
        }

        public override bool ShouldDisplayCommand()
        {
            string sDataFlowGUID;
            return IsContextMenuLocationValid(out sDataFlowGUID);
        }

        private bool IsContextMenuLocationValid(out string DataFlowGUID)
        {
            DataFlowGUID = null;
            try
            {
                if (this.ApplicationObject.ActiveWindow == null || this.ApplicationObject.ActiveWindow.ProjectItem == null) return false;
                ProjectItem pi = this.ApplicationObject.ActiveWindow.ProjectItem;
                if (!pi.Name.ToLower().EndsWith(".dtsx")) return false;

                if (pi.ContainingProject == null || pi.ContainingProject.Kind != BIDSProjectKinds.SSIS) return false; //if the dtsx isn't in an SSIS project, or if you're editing the package standalone (not as a part of a project)

                IDesignerHost designer = this.ApplicationObject.ActiveWindow.Object as IDesignerHost;
                if (designer == null) return false;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                Package package = (Package)win.PropertiesLinkComponent;

                if (win.SelectedIndex == (int)SSISHelpers.SsisDesignerTabIndex.ControlFlow)
                {
                    //control flow
                    EditorWindow.EditorView view = win.SelectedView;
                    Control viewControl = (Control)view.GetType().InvokeMember("ViewControl", getflags, null, view, null);

                    Microsoft.SqlServer.IntegrationServices.Designer.Model.ControlFlowGraphModelElement ctlFlowModel = (Microsoft.SqlServer.IntegrationServices.Designer.Model.ControlFlowGraphModelElement)viewControl.GetType().InvokeMember("GraphModel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, viewControl, null);
                    if (ctlFlowModel == null) return false;
                    Microsoft.SqlServer.IntegrationServices.Designer.Model.TaskModelElement taskModelEl = ctlFlowModel.SelectedItems[0] as Microsoft.SqlServer.IntegrationServices.Designer.Model.TaskModelElement;
                    if (taskModelEl == null) return false;
                    TaskHost task = taskModelEl.TaskHost;
                    if (task == null) return false;
                    if (!(task.InnerObject is MainPipe)) return false;
                    DataFlowGUID = taskModelEl.LogicalID;
                    return true;

                }
                else if (win.SelectedIndex == (int)SSISHelpers.SsisDesignerTabIndex.DataFlow)
                {
                    //data flow
                    EditorWindow.EditorView view = win.SelectedView;
                    Control viewControl = (Control)view.GetType().InvokeMember("ViewControl", getflags, null, view, null);
                    Microsoft.DataTransformationServices.Design.Controls.PipelineComboBox pipelineComboBox = (Microsoft.DataTransformationServices.Design.Controls.PipelineComboBox)(viewControl.Controls["panel1"].Controls["pipelineComboBox"]);
                    foreach (Control c in viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls)
                    {
                        if (!c.Visible) continue;

                        if (c.GetType().FullName != "Microsoft.DataTransformationServices.Design.PipelineTaskView") continue;
                        object pipelineDesigner = c.GetType().InvokeMember("PipelineTaskDesigner", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, c, null);
                        if (pipelineDesigner == null) continue;
                        Microsoft.SqlServer.IntegrationServices.Designer.Model.DataFlowGraphModelElement dataFlowModel = (Microsoft.SqlServer.IntegrationServices.Designer.Model.DataFlowGraphModelElement)pipelineDesigner.GetType().InvokeMember("DataFlowGraphModel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, pipelineDesigner, null);
                        if (dataFlowModel == null) continue;
                        DataFlowGUID = dataFlowModel.PipelineTask.ID;

                        return true;
                    }
                }
            }
            catch (Exception ex){
                package.Log.Exception("Error in PipelineComponentBreakdownPlugin.ShouldDisplay", ex);
            }
            return false;
        }

        public override void Exec()
        {
            try
            {
                string sDataFlowGUID;
                if (IsContextMenuLocationValid(out sDataFlowGUID))
                {
                    PerformanceVisualizationPlugin.ExecutePackage(this.ApplicationObject.ActiveWindow.ProjectItem, sDataFlowGUID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}