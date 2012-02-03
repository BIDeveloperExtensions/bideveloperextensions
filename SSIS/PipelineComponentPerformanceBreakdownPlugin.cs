namespace BIDSHelper.SSIS
{
    using Extensibility;
    using EnvDTE;
    using EnvDTE80;
    using System.Text;
    using System.ComponentModel.Design;
    using Microsoft.DataWarehouse.Design;
    using System;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.DataTransformationServices.Project;
    using System.Runtime.InteropServices;
    using Microsoft.DataTransformationServices.Project.DebugEngine;
    using System.Windows.Forms;
    using Microsoft.DataWarehouse.Controls;
    using System.Collections.Generic;
    using BIDSHelper.SSIS.PerformanceVisualization;
    using MSDDS;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
    
    public class PipelineComponentPerformanceBreakdownPlugin : BIDSHelperPluginBase
    {
        private static System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;

        public PipelineComponentPerformanceBreakdownPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "PipelineComponentPerformanceBreakdownPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override System.Drawing.Icon CustomMenuIcon
        {
            get { return BIDSHelper.Resources.Common.Performance; }
        }

        public override string ButtonText
        {
            get { return "Performance Breakdown"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        public override string FeatureName
        {
            get { return "Pipeline Component Performance Breakdown"; }
        }

        public override string MenuName
        {
            get
            {
                return "SSIS Designer,Component Menu";
            }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        public override bool BeginMenuGroup
        {
            get { return true; }
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
            get { return "Executes an individual Data Flow task and displays a performance breakdown for each component in the task, helping you to identify any bottlenecks you may have."; }
        }

        public override bool DisplayCommand(UIHierarchyItem item)
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

#if DENALI
                    Microsoft.SqlServer.IntegrationServices.Designer.Model.ControlFlowGraphModelElement ctlFlowModel = (Microsoft.SqlServer.IntegrationServices.Designer.Model.ControlFlowGraphModelElement)viewControl.GetType().InvokeMember("GraphModel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, viewControl, null);
                    if (ctlFlowModel == null) return false;
                    Microsoft.SqlServer.IntegrationServices.Designer.Model.TaskModelElement taskModelEl = ctlFlowModel.SelectedItems[0] as Microsoft.SqlServer.IntegrationServices.Designer.Model.TaskModelElement;
                    if (taskModelEl == null) return false;
                    TaskHost task = taskModelEl.TaskHost;
                    if (task == null) return false;
                    if (!(task.InnerObject is MainPipe)) return false;
                    DataFlowGUID = taskModelEl.LogicalID;
                    return true;
#else
                    DdsDiagramHostControl diagram = viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"] as DdsDiagramHostControl;
                    if (diagram == null || diagram.DDS == null) return false;
                    if (diagram.DDS.Selection.Count != 1) return false;
                    MSDDS.IDdsDiagramObject o = diagram.DDS.Selection.Item(0);
                    if (o.Type != DdsLayoutObjectType.dlotShape) return false;
                    MSDDS.IDdsExtendedProperty prop = o.IDdsExtendedProperties.Item("LogicalObject");
                    if (prop == null) return false;
                    string sObjectGuid = prop.Value.ToString();
                    Executable exe = ExpressionHighlighterPlugin.FindExecutable(package, sObjectGuid);
                    if (exe == null || !(exe is TaskHost)) return false;
                    TaskHost task = (TaskHost)exe;
                    if (!(task.InnerObject is MainPipe)) return false;
                    DataFlowGUID = task.ID;
                    return true;
#endif
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

#if DENALI
                        if (c.GetType().FullName != "Microsoft.DataTransformationServices.Design.PipelineTaskView") continue;
                        object pipelineDesigner = c.GetType().InvokeMember("PipelineTaskDesigner", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, c, null);
                        if (pipelineDesigner == null) continue;
                        Microsoft.SqlServer.IntegrationServices.Designer.Model.DataFlowGraphModelElement dataFlowModel = (Microsoft.SqlServer.IntegrationServices.Designer.Model.DataFlowGraphModelElement)pipelineDesigner.GetType().InvokeMember("DataFlowGraphModel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, pipelineDesigner, null);
                        if (dataFlowModel == null) continue;
                        DataFlowGUID = dataFlowModel.PipelineTask.ID;
#else
                        DdsDiagramHostControl diagram = c as DdsDiagramHostControl;
                        if (diagram == null || diagram.DDS == null) return false;
                        if (diagram.DDS.Selection.Count != 0) return false;
                        TaskHost task = (TaskHost)diagram.ComponentDiagram.RootComponent;
                        DataFlowGUID = task.ID;
#endif

                        return true;
                    }
                }
            }
            catch { }
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