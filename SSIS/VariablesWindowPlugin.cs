namespace BIDSHelper.SSIS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.DataTransformationServices.Design;
    using Microsoft.DataWarehouse.Design;
    using Microsoft.DataWarehouse.Interfaces;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.SqlServer.Dts.Design;
    using Microsoft.SqlServer.Management.UI.Grid;

    #if KATMAI || DENALI
    using IDTSInfoEventsXX = Microsoft.SqlServer.Dts.Runtime.Wrapper.IDTSInfoEvents100;
    #else
    using IDTSInfoEventsXX = Microsoft.SqlServer.Dts.Runtime.Wrapper.IDTSInfoEvents90;
    #endif
    
    public class VariablesWindowPlugin : BIDSHelperWindowActivatedPluginBase
    {
#if DENALI
        private const string SSIS_VARIABLES_TOOL_WINDOW_KIND = "{41C287E9-BCD9-4D20-8D38-B6FD9CFB73C9}";
#else
        private const string SSIS_VARIABLES_TOOL_WINDOW_KIND = "{587B69DC-A87E-42B6-B92A-714016B29C6D}";
#endif
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private ToolBarButton moveCopyButton;
        private ToolBarButton editExpressionButton;
        private static DlgGridControl grid;
        private static UserControl variablesToolWindowControl;
        private static IComponentChangeService changesvc;
        private static IDesignerHost serviceProvider;
        private static ComponentDesigner packageDesigner;
        private static bool bSkipHighlighting = false;

        public VariablesWindowPlugin(Connect con, DTE2 appObject, AddIn addinInstance) : base(con, appObject, addinInstance)
        {
        }

        public override bool ShouldHookWindowCreated
        {
            get { return true; }
        }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (grid != null) return; //short circuit if we've already hooked up the variables window

                //scan all windows so you don't have to focus the variables window before it starts highlighting
                foreach (Window win in this.ApplicationObject.Windows)
                {
                    HookupVariablesWindow(win);
                }
            }
            catch { }
        }

        private void HookupVariablesWindow(Window GotFocus)
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
                    try
                    {
                        designer = w.Object as IDesignerHost;
                        if (designer == null) continue;
                        ProjectItem pi = w.ProjectItem;
                        if (pi != null && !(pi.Name.ToLower().EndsWith(".dtsx")))
                        {
                            designer = null;
                            continue;
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    IDesignerToolWindowService service = (IDesignerToolWindowService)designer.GetService(typeof(IDesignerToolWindowService));
                    if (service == null) continue;
                    IDesignerToolWindow toolWindow = service.GetToolWindow(new Guid(SSIS_VARIABLES_TOOL_WINDOW_KIND), 0);
                    if (toolWindow == null) continue;
                    variablesToolWindowControl = (UserControl)toolWindow.Client; //actually Microsoft.DataTransformationServices.Design.VariablesToolWindow which is internal

                    serviceProvider = designer;
                    changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));

                    // Get grid and toolbar
#if DENALI
                    // "tableLayoutPanelMain" - "tableLayoutPanelVariable" - "dlgGridControl1" | "toolBarVariable"
                    grid = (DlgGridControl)variablesToolWindowControl.Controls[0].Controls[0].Controls[0];
                    ToolBar toolbar = (ToolBar)variablesToolWindowControl.Controls[0].Controls[0].Controls[1];
#else
                    grid = (DlgGridControl)variablesToolWindowControl.Controls["dlgGridControl1"];
                    ToolBar toolbar = (ToolBar)variablesToolWindowControl.Controls["toolBar1"];
#endif

                    // If buttons already added, no need to do it again so exit 
                    if (this.moveCopyButton != null && toolbar.Buttons.Contains(this.moveCopyButton)) return;

                    grid.SelectionChanged += new SelectionChangedEventHandler(grid_SelectionChanged);
                    grid.Invalidated += new InvalidateEventHandler(grid_Invalidated);

                    ToolBarButton separator = new ToolBarButton();
                    separator.Style = ToolBarButtonStyle.Separator;
                    toolbar.Buttons.Add(separator);

                    // Move/Copy button
                    this.moveCopyButton = new ToolBarButton();
                    this.moveCopyButton.Style = ToolBarButtonStyle.PushButton;
                    this.moveCopyButton.ToolTipText = "Move/Copy Variables to New Scope (BIDS Helper)";
                    toolbar.Buttons.Add(this.moveCopyButton);
                    toolbar.ImageList.Images.Add(BIDSHelper.Resources.Common.Copy);
                    this.moveCopyButton.ImageIndex = toolbar.ImageList.Images.Count - 1;

                    //Edit Variable Expression button
                    this.editExpressionButton = new ToolBarButton();
                    this.editExpressionButton.Style = ToolBarButtonStyle.PushButton;
                    this.editExpressionButton.ToolTipText = "Edit Variable Expression (BIDS Helper)";
                    toolbar.Buttons.Add(this.editExpressionButton);
                    toolbar.ImageList.Images.Add(BIDSHelper.Resources.Versioned.EditVariable);
                    this.editExpressionButton.ImageIndex = toolbar.ImageList.Images.Count - 1;

                    toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);
                    toolbar.Wrappable = false;

                    SetButtonEnabled();
                    RefreshHighlights();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
        }

        //only way I could find to monitor when row data in the grid changes
        void grid_Invalidated(object sender, InvalidateEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("variables grid invalidated");
            RefreshHighlights();
        }

        public static void RefreshHighlights()
        {
            try
            {
                if (bSkipHighlighting) return;
                if (variablesToolWindowControl == null) return;

#if DENALI
                packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().GetProperty("PackageDesigner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance).GetValue(variablesToolWindowControl, null);
#else
                packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().InvokeMember("PackageDesigner", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, null);
#endif
                if (packageDesigner == null) return;

                Package package = packageDesigner.Component as Package;
                if (package == null) return;

                List<string> listConfigPaths;
                lock (HighlightingToDo.cacheConfigPaths)
                {
                    if (HighlightingToDo.cacheConfigPaths.ContainsKey(package))
                        listConfigPaths = HighlightingToDo.cacheConfigPaths[package];
                    else
                        listConfigPaths = new List<string>();
                }

                for (int iRow = 0; iRow < grid.RowsNumber; iRow++)
                {
                    GridCell cell = grid.GetCellInfo(iRow, 0);
                    if (cell.CellData != null)
                    {
                        System.Diagnostics.Debug.WriteLine(cell.CellData.GetType().FullName);
                        Variable variable = GetVariableForRow(iRow);

#if DENALI
                        // Denali doesn't need variable highlighting, it is built in. This is a quick fix to disable the highlighting
                        // for Denali only. The other code stays the same for backward compatability when compiled as 2005 or 2008 projects.
                        // We will retain the configuration highlighting though.
                        bool bHasExpression = false;
#else
                        bool bHasExpression = variable.EvaluateAsExpression && !string.IsNullOrEmpty(variable.Expression);
#endif
                        bool bHasConfiguration = false;
                        string sVariablePath = variable.GetPackagePath();
                        foreach (string configPath in listConfigPaths)
                        {
                            if (configPath.StartsWith(sVariablePath))
                            {
                                bHasConfiguration = true;
                                break;
                            }
                        }

                        System.Drawing.Bitmap icon = (System.Drawing.Bitmap)cell.CellData;
                        if (!bHasExpression && !bHasConfiguration && icon.Tag != null)
                        {
                            // Reset the icon because this one doesn't have an expression anymore
                            cell.CellData = icon.Tag;
                            icon.Tag = null;

                            try
                            {
                                bSkipHighlighting = true;
                                grid.Invalidate(true);
                            }
                            finally
                            {
                                bSkipHighlighting = false;
                            }

                            System.Diagnostics.Debug.WriteLine("un-highlighted variable");
                        }
                        else if ((bHasExpression || bHasConfiguration))
                        {
                            //save what the icon looked like originally so we can go back if they remove the expression
                            if (icon.Tag == null)
                                icon.Tag = icon.Clone();

                            //now update the icon to note this one has an expression
                            if (bHasExpression && !bHasConfiguration)
                                HighlightingToDo.ModifyIcon(icon, HighlightingToDo.expressionColor);
                            else if (bHasConfiguration && !bHasExpression)
                                HighlightingToDo.ModifyIcon(icon, HighlightingToDo.configurationColor);
                            else
                                HighlightingToDo.ModifyIcon(icon, HighlightingToDo.expressionColor, HighlightingToDo.configurationColor);
                            cell.CellData = icon;

                            try
                            {
                                bSkipHighlighting = true;
                                grid.Invalidate(true);
                            }
                            finally
                            {
                                bSkipHighlighting = false;
                            }

                            System.Diagnostics.Debug.WriteLine("highlighted variable");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n\r\n" + ex.StackTrace);
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
            this.moveCopyButton.Enabled = (variables.Count > 0);

            this.editExpressionButton.Enabled = (variables.Count > 0);
        }

        void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            if (e.Button == this.moveCopyButton)
                MoveCopyButtonClick();
            else if (e.Button == this.editExpressionButton)
                EditExpressionButtonClick();
        }

        void EditExpressionButtonClick()
        {
            try
            {
#if DENALI
                packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().GetProperty("PackageDesigner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance).GetValue(variablesToolWindowControl, null);
#else
                packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().InvokeMember("PackageDesigner", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, null);
#endif

                if (packageDesigner == null) return;

                Package package = packageDesigner.Component as Package;
                if (package == null) return;

                int selectedRow;
                int selectedCol;
                grid.GetSelectedCell(out selectedRow, out selectedCol);

                if (selectedRow < 0) return;

                Variable variable = GetVariableForRow(selectedRow);

                if (variable == null) return;

                DtsContainer sourceContainer = FindObjectForVariablePackagePath(package, variable.GetPackagePath());
                Variables variables = sourceContainer.Variables;
                VariableDispenser variableDispenser = sourceContainer.VariableDispenser;

                Konesans.Dts.ExpressionEditor.ExpressionEditorPublic editor = new Konesans.Dts.ExpressionEditor.ExpressionEditorPublic(variables, variableDispenser, variable);
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    string expression = editor.Expression;
                    if (string.IsNullOrEmpty(expression) || string.IsNullOrEmpty(expression.Trim()))
                    {
                        expression = null;
                        variable.EvaluateAsExpression = false;
                    }
                    else
                    {
                        variable.EvaluateAsExpression = true;
                    }

                    variable.Expression = expression;
                    changesvc.OnComponentChanging(sourceContainer, null);
                    changesvc.OnComponentChanged(sourceContainer, null, null, null); //marks the package designer as dirty
                    SSISHelpers.MarkPackageDirty(package);

                    TypeDescriptor.Refresh(variable);
                    System.Windows.Forms.Application.DoEvents();

                    // Refresh the grid
                    variablesToolWindowControl.GetType().InvokeMember("FillGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, new object[] { });
                    SetButtonEnabled();
                    RefreshHighlights();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
        }

        void MoveCopyButtonClick()
        {
            try
            {
                List<Variable> variables = GetSelectedVariables();
                if (variables.Count > 0)
                {
                    System.Collections.ArrayList variableDesigners = GetSelectedVariableDesigners();
#if DENALI
                    packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().GetProperty("PackageDesigner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance).GetValue(variablesToolWindowControl, null);
#else
                    packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().InvokeMember("PackageDesigner", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, null);
#endif
                    Package package = packageDesigner.Component as Package;

                    DtsContainer oCurrentScope = FindObjectForVariablePackagePath(package, variables[0].GetPackagePath());
                    BIDSHelper.SSIS.VariablesMove form = new BIDSHelper.SSIS.VariablesMove(package, oCurrentScope.ID, variables.Count);
                    DialogResult result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        DtsContainer targetContainer = form.TargetContainer;
                        bool move = form.IsMove;
                        try
                        {
                            CopyVariables(variables, move, targetContainer, package, variableDesigners);
                        }
                        finally
                        {
                            //refresh the grid after the changes we've made
                            variablesToolWindowControl.GetType().InvokeMember("FillGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, new object[] { });
                            SetButtonEnabled();
                            RefreshHighlights();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Highlight one or more variables before clicking this button.", "BIDS Helper - Variable Scope Change", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (VariableCopyException ex)
            {
                MessageBox.Show(ex.Message, "BIDS Helper", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "BIDS Helper", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    Variable variable = GetVariableForRow(iRow);
                    if (!variable.SystemVariable)
                    {
                        variables.Add(variable);
                    }
                }
            }

            return variables;
        }

        private static Variable GetVariableForRow(int iRow)
        {
            GridCell cell = grid.GetCellInfo(iRow, 1);
            DtsBaseDesigner varDesigner = (DtsBaseDesigner)cell.Tag;
            Variable variable = (Variable)varDesigner.GetType().InvokeMember("Variable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, varDesigner, null);
            return variable;
        }

        private System.Collections.ArrayList GetSelectedVariableDesigners()
        {
            System.Collections.ArrayList list = new System.Collections.ArrayList();

            int[] selectedRows = (int[])grid.GetType().InvokeMember("SelectedRows", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, grid, null);
            if ((selectedRows != null) && (selectedRows.Length > 0))
            {
                foreach (int iRow in selectedRows)
                {
                    Variable variable = GetVariableForRow(iRow);
                    if (!variable.SystemVariable)
                    {
                        DtsBaseDesigner variableDesigner = GetVariableDesignerForRow(iRow);
                        list.Add(variableDesigner);
                    }
                }
            }

            return list;
        }

        private static DtsBaseDesigner GetVariableDesignerForRow(int iRow)
        {
            GridCell cell = grid.GetCellInfo(iRow, 1);
            return (DtsBaseDesigner)cell.Tag;
        }

        private DtsContainer FindObjectForVariablePackagePath(DtsContainer parent, string PackagePath)
        {
                        
            if (PackagePath.StartsWith(((IDTSPackagePath)parent).GetPackagePath() + ".Variables["))
            {
                return ((DtsContainer)parent);
            }

            IDTSSequence seq = parent as IDTSSequence;
            if (seq != null)
            {
                foreach (Executable e in seq.Executables)
                {
                    if (e is IDTSPackagePath)
                    {
                        if (PackagePath.StartsWith(((IDTSPackagePath)e).GetPackagePath() + ".Variables["))
                        {
                            return ((DtsContainer)e);
                        }
                    }
                    if (e is DtsContainer)
                    {
                        DtsContainer ret = FindObjectForVariablePackagePath((DtsContainer)e, PackagePath);
                        if (ret != null) return ret;
                    }
                }
            }

            EventsProvider prov = parent as EventsProvider;
            if (prov != null)
            {
                foreach (DtsEventHandler eh in prov.EventHandlers)
                {
                    if (eh is IDTSPackagePath)
                    {
                        if (PackagePath.StartsWith(((IDTSPackagePath)eh).GetPackagePath() + ".Variables["))
                        {
                            return ((DtsContainer)eh);
                        }
                    }
                    if (eh is IDTSSequence)
                    {
                        DtsContainer ret = FindObjectForVariablePackagePath((DtsContainer)eh, PackagePath);
                        if (ret != null) return ret;
                    }
                }
            }
            return null;
        }

        private void CopyVariables(List<Variable> variables, bool move, DtsContainer targetContainer, Package package, System.Collections.ArrayList sourceVariableDesigners)
        {
            foreach (Variable sourceVar in variables)
            {
                if (targetContainer is IDTSPackagePath && sourceVar.GetPackagePath().StartsWith(((IDTSPackagePath)targetContainer).GetPackagePath() + ".Variables["))
                {
                    throw new VariableCopyException("You are attempting to copy the variable '" + sourceVar.QualifiedName + "' to the same scope it is already in.", null);
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
                    changesvc.OnComponentChanging(sourceContainer, null);
                    changesvc.OnComponentChanged(sourceContainer, null, null, null); //marks the package designer as dirty
                }
            }

            if (move)
            {
#if DENALI
                //terrible workaround to get the exact right parameter type for the DeleteVariables method in Denali. Guess calling InvokeMember against a function with a parameter of a generic type is tricky
                System.Collections.IList listParam = ((System.Collections.IList)System.Type.GetType("System.Collections.Generic.List`1[[" + ExpressionHighlighterPlugin.GetPrivateType(variablesToolWindowControl.GetType(), "Microsoft.DataTransformationServices.Design.VariableDesigner").AssemblyQualifiedName + "]]").GetConstructor(new Type[] { }).Invoke(new object[] { }));
                foreach (object o in sourceVariableDesigners)
                {
                    listParam.Add(o);
                }
                variablesToolWindowControl.GetType().GetMethod("DeleteVariables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance).Invoke(variablesToolWindowControl, new object[] { listParam });
#else
                variablesToolWindowControl.GetType().InvokeMember("DeleteVariables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, new object[] { sourceVariableDesigners });
#endif
            }

            changesvc.OnComponentChanging(targetContainer, null);
            changesvc.OnComponentChanged(targetContainer, null, null, null); //marks the package designer as dirty
            SSISHelpers.MarkPackageDirty(package);

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
                if (!v.SystemVariable && v.GetPackagePath().StartsWith(((IDTSPackagePath)o).GetPackagePath() + ".Variables[")) //only variables in this scope
                {
                    object val;
                    try
                    {
                        val = v.Value; //look at each value to see if each variable expression works
                    }
                    catch (Exception ex)
                    {
                        listOtherErrors.Add(ex.Message);
                        continue;
                    }

                    //if we haven't already gotten an error, try to validate the expression
                    IDTSInfoEventsWrapper events = new IDTSInfoEventsWrapper();
                    try
                    {
                        if (!string.IsNullOrEmpty(v.Expression))
                        {
                            Microsoft.SqlServer.Dts.Runtime.Wrapper.ExpressionEvaluatorClass eval = new Microsoft.SqlServer.Dts.Runtime.Wrapper.ExpressionEvaluatorClass();
                            eval.Expression = v.Expression;
                            eval.Events = events;
#if KATMAI || DENALI
                            eval.Evaluate(DtsConvert.GetExtendedInterface(o.VariableDispenser), out val, false);
#else
                            eval.Evaluate(DtsConvert.ToVariableDispenser90(o.VariableDispenser), out val, false);
#endif
                        }
                    }
                    catch
                    {
                        if (events.Errors.Length > 0)
                            listOtherErrors.Add("Error in expression for variable " + v.QualifiedName + ": " + events.Errors[0]);
                        continue;
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
                IDesignerHost host = (IDesignerHost)packageDesigner.GetType().InvokeMember("DesignerHost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, packageDesigner, null);
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
                if (task != null && task.CustomInfo == this && task.Document == window.ProjectItem.Name)
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
            get { return string.Empty; }
        }

        public override string MenuName
        {
            get { return string.Empty; } //no need to have a menu command
        }

        /// <summary>
        /// Gets the name of the friendly name of the plug-in.
        /// </summary>
        /// <value>The friendly name.</value>
        /// <remarks>Used for HelpUrl as ButtonText does not match Wiki page.</remarks>
        public override string FeatureName
        {
            get { return "Variables Window Extensions"; }
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
            get { return "Extended features for the Variables window. Move or copy a variable between scopes in a package, expression and configuration highlighting of the variables and the advanced expression editor."; }
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

        private class IDTSInfoEventsWrapper : IDTSInfoEventsXX
        {
            private List<string> m_errors = new List<string>();
            public string[] Errors
            {
                get { return m_errors.ToArray(); }
            }

            public void FireError(int errorCode, string subComponent, string description, string helpFile, int helpContext, out bool cancel)
            {
                cancel = false;
                m_errors.Add(description);
            }

            public void FireInformation(int informationCode, string subComponent, string description, string helpFile, int helpContext, ref bool fireAgain)
            {
            }

            public void FireWarning(int warningCode, string subComponent, string description, string helpFile, int helpContext)
            {
            }
        }
    }
}
