using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using System;
using Microsoft.Win32;
using Microsoft.DataWarehouse.Interfaces;
using Microsoft.DataTransformationServices.Design;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Design;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.UI.Grid;
using System.ComponentModel;

namespace BIDSHelper
{
    public class VariablesWindowPlugin : BIDSHelperWindowActivatedPluginBase
    {
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private static string SSIS_VARIABLES_TOOL_WINDOW_KIND = "{587B69DC-A87E-42B6-B92A-714016B29C6D}";
        private ToolBarButton button;
        private GridControl grid;
        private UserControl variablesToolWindowControl;
        private IComponentChangeService changesvc;
        private IDesignerHost serviceProvider;
        private ComponentDesigner packageDesigner;

        public VariablesWindowPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override bool ShouldHookWindowCreated
        {
            get
            {
                return true;
            }
        }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                try
                {
                    if (GotFocus == null) return;
                    if (GotFocus.ObjectKind != SSIS_VARIABLES_TOOL_WINDOW_KIND) return; //if not the variables window
                }
                catch //ObjectKind property blows up on some windows
                {
                    return;
                }

                //they've highlighted the Variables window, so add the extra toolbar buttons
                //find a package designer window
                IDesignerHost designer = null;
                foreach (Window w in this.ApplicationObject.Windows)
                {
                    designer = w.Object as IDesignerHost;
                    if (designer == null) continue;
                    ProjectItem pi = w.ProjectItem;
                    if (pi != null && !(pi.Name.ToLower().EndsWith(".dtsx")))
                    {
                        designer = null;
                        continue;
                    }
                    IDesignerToolWindowService service = (IDesignerToolWindowService)designer.GetService(typeof(IDesignerToolWindowService));
                    if (service == null) continue;
                    IDesignerToolWindow toolWindow = service.GetToolWindow(new Guid(SSIS_VARIABLES_TOOL_WINDOW_KIND), 0);
                    if (toolWindow == null) continue;
                    variablesToolWindowControl = (UserControl)toolWindow.Client; //actually Microsoft.DataTransformationServices.Design.VariablesToolWindow which is internal

                    serviceProvider = designer;
                    changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));

                    grid = (GridControl)variablesToolWindowControl.Controls["dlgGridControl1"];
                    ToolBar toolbar = (ToolBar)variablesToolWindowControl.Controls["toolBar1"];
                    if (this.button != null && toolbar.Buttons.Contains(this.button)) return;

                    grid.SelectionChanged += new SelectionChangedEventHandler(grid_SelectionChanged);

                    ToolBarButton separator = new ToolBarButton();
                    separator.Style = ToolBarButtonStyle.Separator;
                    toolbar.Buttons.Add(separator);

                    this.button = new ToolBarButton();
                    this.button.Style = ToolBarButtonStyle.PushButton;
                    this.button.ToolTipText = "Move/Copy Variables to New Scope (BIDS Helper)";
                    toolbar.Buttons.Add(this.button);
                    toolbar.ImageList.Images.Add(Properties.Resources.Copy);
                    this.button.ImageIndex = toolbar.ImageList.Images.Count - 1;
                    toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);

                    toolbar.Wrappable = false;

                    SetButtonEnabled();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
        }

        void grid_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                SetButtonEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
        }

        private void SetButtonEnabled()
        {
            List<Variable> variables = GetSelectedVariables();
            this.button.Enabled = (variables.Count > 0);
        }

        void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button != this.button) return;

                List<Variable> variables = GetSelectedVariables();
                if (variables.Count > 0)
                {
                    packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().InvokeMember("PackageDesigner", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, null);
                    Package package = packageDesigner.Component as Package;

                    DtsContainer oCurrentScope = FindObjectForVariablePackagePath(package, variables[0].GetPackagePath());
                    BIDSHelper.SSIS.VariablesMove form = new BIDSHelper.SSIS.VariablesMove(package, oCurrentScope.ID);
                    DialogResult result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        DtsContainer MoveToContainer = (DtsContainer)form.treeView1.SelectedNode.Tag;
                        bool move = form.radMove.Checked;
                        try
                        {
                            CopyVariables(variables, move, MoveToContainer, package);
                        }
                        finally
                        {
                            //refresh the grid after the changes we've made
                            variablesToolWindowControl.GetType().InvokeMember("FillGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, new object[] { });
                            SetButtonEnabled();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Highlight one or more variables before clicking this button.", "BIDS Helper - Variable Scope Change");
                }
            }
            catch (VariableCopyException ex)
            {
                MessageBox.Show(ex.Message, "BIDS Helper - Variable Scope Change Problem");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
        }

        private List<Variable> GetSelectedVariables()
        {
            List<Variable> variables = new List<Variable>();

            int[] selectedRows = (int[])grid.GetType().InvokeMember("SelectedRows", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, grid, null);
            if ((selectedRows != null) && (selectedRows.Length > 0))
            {
                foreach (int iRow in selectedRows)
                {
                    object cell = grid.GetType().InvokeMember("GetCellInfo", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, grid, new object[] { iRow, 1 }); //actually a VariableDesigner
                    DtsBaseDesigner varDesigner = (DtsBaseDesigner)cell.GetType().InvokeMember("Tag", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, cell, null);
                    Variable variable = (Variable)varDesigner.GetType().InvokeMember("Variable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, varDesigner, null);
                    //JCW - Added to prevent button from being enabled if only System variables are selected
                    if (!variable.SystemVariable)
                    {
                        variables.Add(variable);
                    }
                }
            }

            return variables;
        }

        private DtsContainer FindObjectForVariablePackagePath(DtsContainer parent, string PackagePath)
        {
            IDTSSequence seq = (IDTSSequence)parent;

            if (PackagePath.StartsWith(((IDTSPackagePath)parent).GetPackagePath() + ".Variables["))
            {
                return ((DtsContainer)parent);
            }

            foreach (Executable e in seq.Executables)
            {
                if (e is IDTSPackagePath)
                {
                    if (PackagePath.StartsWith(((IDTSPackagePath)e).GetPackagePath() + ".Variables["))
                    {
                        return ((DtsContainer)e);
                    }
                }
                if (e is IDTSSequence)
                {
                    DtsContainer ret = FindObjectForVariablePackagePath((DtsContainer)e, PackagePath);
                    if (ret != null) return ret;
                }
            }

            return null;
        }

        private void CopyVariables(List<Variable> variables, bool move, DtsContainer targetContainer, Package package)
        {
            foreach (Variable sourceVar in variables)
            {
                if (targetContainer is IDTSPackagePath && sourceVar.GetPackagePath().StartsWith(((IDTSPackagePath)targetContainer).GetPackagePath() + ".Variables["))
                {
                    throw new VariableCopyException("You are attempting to copy " + sourceVar.QualifiedName + " to the same scope it is already in.", null);
                }
                else if (sourceVar.SystemVariable)
                {
                    throw new VariableCopyException(sourceVar.QualifiedName + " is a system variable and cannot be copied or moved.", null);
                }
            }

            foreach (Variable sourceVar in variables)
            {
                //Variable targetVar = targetContainer.Variables.Add(sourceVar.Name, sourceVar.ReadOnly, sourceVar.Namespace, sourceVar.Value); //this is the standard way to add a variable, but it doesn't interact well with the variables tool window
                Variable targetVar = (Variable)packageDesigner.GetType().InvokeMember("CreateVariable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, packageDesigner, new object[] { targetContainer, sourceVar.Name, sourceVar.ReadOnly, sourceVar.Namespace, sourceVar.Value });

                try
                {
                    targetVar.Name = sourceVar.Name;
                }
                catch (Exception ex)
                {
                    serviceProvider.DestroyComponent(targetVar);
                    throw new VariableCopyException("Could not copy " + sourceVar.QualifiedName + " to scope \"" + targetContainer.Name + "\" because another variable with that name already exists.", ex);
                }
                //targetVar.DataType is read only
                targetVar.Description = sourceVar.Description;
                targetVar.EvaluateAsExpression = sourceVar.EvaluateAsExpression;
                targetVar.Expression = sourceVar.Expression;
                targetVar.RaiseChangedEvent = sourceVar.RaiseChangedEvent;

                if (move)
                {
                    DtsContainer sourceContainer = FindObjectForVariablePackagePath(package, sourceVar.GetPackagePath());
                    serviceProvider.DestroyComponent(sourceVar);

                    changesvc.OnComponentChanging(sourceContainer, null);
                    changesvc.OnComponentChanged(sourceContainer, null, null, null); //marks the package designer as dirty
                }
            }

            changesvc.OnComponentChanging(targetContainer, null);
            changesvc.OnComponentChanged(targetContainer, null, null, null); //marks the package designer as dirty

            ValidatePackage(package);
        }

        private string[] RecurseContainersAndGetVariableValidationErrors(IDTSSequence parentExecutable)
        {
            List<string> listOtherErrors = new List<string>();
            listOtherErrors.AddRange(ScanDtsObjectVariablesForErrors((DtsContainer)parentExecutable));
            if (parentExecutable is EventsProvider)
            {
                foreach (DtsEventHandler eh in (parentExecutable as EventsProvider).EventHandlers)
                {
                    listOtherErrors.AddRange(RecurseContainersAndGetVariableValidationErrors(eh));
                }
            }
            foreach (Executable e in parentExecutable.Executables)
            {
                if (e is IDTSSequence)
                {
                    listOtherErrors.AddRange(RecurseContainersAndGetVariableValidationErrors((IDTSSequence)e));
                }
                else
                {
                    if (e is DtsContainer)
                    {
                        listOtherErrors.AddRange(ScanDtsObjectVariablesForErrors((DtsContainer)e));
                    }
                    if (e is EventsProvider)
                    {
                        foreach (DtsEventHandler eh in (e as EventsProvider).EventHandlers)
                        {
                            listOtherErrors.AddRange(RecurseContainersAndGetVariableValidationErrors(eh));
                        }
                    }
                }
            }
            return listOtherErrors.ToArray();
        }

        private string[] ScanDtsObjectVariablesForErrors(DtsContainer o)
        {
            List<string> listOtherErrors = new List<string>();
            foreach (Variable v in o.Variables)
            {
                if (v.GetPackagePath().StartsWith(((IDTSPackagePath)o).GetPackagePath() + ".Variables[")) //only variables in this scope
                {
                    try
                    {
                        object val = v.Value; //look at each value to see if each variable expression works
                    }
                    catch (Exception ex)
                    {
                        listOtherErrors.Add(ex.Message);
                    }
                }
            }
            return listOtherErrors.ToArray();
        }

        private void ValidatePackage(Package package)
        {
            Microsoft.DataTransformationServices.Design.ComponentModel.EventHandlerErrorCollector events = new Microsoft.DataTransformationServices.Design.ComponentModel.EventHandlerErrorCollector();
            DTSExecResult result = package.Validate(package.Connections, package.Variables, events, null);

            List<string> listOtherErrors = new List<string>();
            listOtherErrors.AddRange(RecurseContainersAndGetVariableValidationErrors(package));

            foreach (Window w in this.ApplicationObject.Windows)
            {
                IDesignerHost designer = w.Object as IDesignerHost;
                if (designer == null) continue;
                EditorWindow win = designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator)) as EditorWindow;
                if (win == null) continue;
                if ((win.PropertiesLinkComponent as Package) == package)
                {
                    AddErrorsToVSErrorList(w, listOtherErrors.ToArray());
                    break;
                }
            }

            foreach (KeyValuePair<IComponent, ICollection<IComponentErrorInfo>> pair in events.ComponentIssuesMap)
            {
                IComponent key = pair.Key;
                System.Collections.ICollection issues = (System.Collections.ICollection)pair.Value;
                IDesignerHost host = (IDesignerHost)this.packageDesigner.GetType().InvokeMember("DesignerHost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, packageDesigner, null);
                IDesigner containerDesigner = host.GetDesigner(key);
                if (containerDesigner != null)
                {
                    Type t = FindBaseType(containerDesigner.GetType(), "DtsContainerDesigner");
                    if (t == null) continue;

                    //doesn't work... not sure why... so the following is the equivalent of this one line:
                    //t.InvokeMember("OnContainerValidated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, containerDesigner, new object[] { result, issues });

                    Microsoft.DataTransformationServices.Design.ComponentModel.ComponentValidationEventArgs e = new Microsoft.DataTransformationServices.Design.ComponentModel.ComponentValidationEventArgs(containerDesigner.Component);
                    e.Issues = issues;
                    e.ValidationFailed = (issues != null) && (issues.Count > 0);

                    //doesn't work... not sure why... so the following is the equivalent of this one line:
                    //containerDesigner.GetType().InvokeMember("RaiseValidationEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, containerDesigner, new object[] { e });
                    t.InvokeMember("PostTaskListValidationMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance, null, containerDesigner, new object[] { e });
                    object componentValidationService = t.InvokeMember("componentValidationService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance, null, containerDesigner, null);
                    componentValidationService.GetType().InvokeMember("OnValidated", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, componentValidationService, new object[] { e });
                }
            }
        }

        private void AddErrorsToVSErrorList(Window window, string[] errors)
        {
            ErrorList errorList = this.ApplicationObject.ToolWindows.ErrorList;
            Window2 errorWin2 = (Window2)(errorList.Parent);
            if (errors.Length > 0)
            {
                if (!errorWin2.Visible)
                {
                    this.ApplicationObject.ExecuteCommand("View.ErrorList", " ");
                }
                errorWin2.SetFocus();
            }

            IDesignerHost designer = (IDesignerHost)window.Object;
            ITaskListService service = designer.GetService(typeof(ITaskListService)) as ITaskListService;

            //remove old task items from this document and BIDS Helper class
            System.Collections.Generic.List<ITaskItem> tasksToRemove = new System.Collections.Generic.List<ITaskItem>();
            foreach (ITaskItem ti in service.GetTaskItems())
            {
                ICustomTaskItem task = ti as ICustomTaskItem;
                if (task != null && task.CustomInfo == this && task.Document == window.ProjectItem.get_FileNames(0))
                {
                    tasksToRemove.Add(ti);
                }
            }
            foreach (ITaskItem ti in tasksToRemove)
            {
                service.Remove(ti);
            }


            foreach (string s in errors)
            {
                ICustomTaskItem item = (ICustomTaskItem)service.CreateTaskItem(TaskItemType.Custom, s);
                item.Category = TaskItemCategory.Misc;
                item.Appearance = TaskItemAppearance.Squiggle;
                item.Priority = TaskItemPriority.High;
                item.Document = window.ProjectItem.Name;
                item.CustomInfo = this;
                service.Add(item);
            }
        }

        private Type FindBaseType(Type t, string TypeName)
        {
            while (t.BaseType != null)
            {
                if (t.BaseType.Name == TypeName)
                    return t.BaseType;
                else
                    t = t.BaseType;
            }
            return null;
        }

        public override string ShortName
        {
            get { return "VariablesWindowPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "SSIS Variables Window Extensions"; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }

        public override string MenuName
        {
            get { return ""; } //no need to have a menu command
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


        public override void Exec()
        {
        }

        private class VariableCopyException : Exception
        {
            public VariableCopyException(string message, Exception innerException)
                : base(message, innerException) { }
        }

    }
}