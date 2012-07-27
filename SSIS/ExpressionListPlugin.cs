using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.DataWarehouse.Controls;
using Microsoft.DataWarehouse.Design;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.Win32;
using System.Drawing;

namespace BIDSHelper.SSIS
{
    public class ExpressionListPlugin : BIDSHelperWindowActivatedPluginBase
    {
        private const string REGISTRY_EXTENDED_PATH = "ExpressionListPlugin";
        private const string REGISTRY_SETTING_NAME = "InEffect";
        public static bool shouldSkipExpressionHighlighting = false;
        
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private ExpressionListControl expressionListWindow = null;
        private Window toolWindow = null;
        private EditorWindow win = null;
        private IDesignerHost designer = null;
        private BackgroundWorker processPackage = null;

        public ExpressionListPlugin(Connect con, DTE2 appObject, AddIn addinInstance) : base(con, appObject, addinInstance)
        {
        }

        public override bool ShouldHookWindowCreated
        {
            get { return true; }
        }

        public override bool ShouldHookWindowClosing
        {
            get { return true; }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            // Hide the tool window
            toolWindow.Visible = false;    
        }

        public override void OnEnable()
        {
            base.OnEnable();

            RegistryKey rk = Registry.CurrentUser.OpenSubKey(Connect.REGISTRY_BASE_PATH + "\\" + REGISTRY_EXTENDED_PATH);
            bool windowIsVisible = false;
            if (rk != null)
            {
                windowIsVisible = (1 == (int)rk.GetValue(REGISTRY_SETTING_NAME, 0));
                rk.Close();
            }

            processPackage = new System.ComponentModel.BackgroundWorker();
            processPackage.WorkerReportsProgress = true;
            processPackage.WorkerSupportsCancellation = true;
            processPackage.DoWork += new System.ComponentModel.DoWorkEventHandler(processPackage_DoWork);
            processPackage.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(processPackage_ProgressChanged);
            processPackage.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(processPackage_RunWorkerCompleted);

            object programmableObject = null;

            //This guid must be unique for each different tool window,
            // but you may use the same guid for the same tool window.
            //This guid can be used for indexing the windows collection,
            // for example: applicationObject.Windows.Item(guidstr)
            String guidstr = "{6679390F-A712-40EA-8729-E2184A1436BF}";
            EnvDTE80.Windows2 windows2 = (EnvDTE80.Windows2)this.ApplicationObject.Windows;
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            toolWindow = windows2.CreateToolWindow2( this.AddInInstance, asm.Location, "BIDSHelper.SSIS.ExpressionListControl", "Expressions", guidstr, ref programmableObject);
            expressionListWindow = (ExpressionListControl)programmableObject;
            expressionListWindow.RefreshExpressions += new EventHandler(expressionListWindow_RefreshExpressions);
            expressionListWindow.EditExpressionSelected += new EventHandler<EditExpressionSelectedEventArgs>(expressionListWindow_EditExpressionSelected);

            // Set the picture displayed when the window is tab docked
            // Clean build required when switching between VS 2005 and VS 2008 
            // during testing, otherwise we get some strange behaviour with this
            IntPtr icon = BIDSHelper.Resources.Common.ExpressionListIcon.ToBitmap().GetHbitmap();

#if KATMAI || DENALI
            toolWindow.SetTabPicture(icon.ToInt32()); 
#else
            toolWindow.SetTabPicture(icon); 
#endif

            //if (windowIsVisible)
            //    toolWindow.Visible = true;
        }

        void expressionListWindow_EditExpressionSelected(object sender, EditExpressionSelectedEventArgs e)
        {
            try
            {
                Package package = null;
                DtsContainer container = null;

                if (win == null)
                {
                    return;
                }

                try
                {
                    package = GetCurrentPackage();
                    if (package == null)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Assert(false, ex.ToString());
                    return;
                }

                // Parameters for Expression Editor
                Variables variables = null;
                VariableDispenser variableDispenser = null;
                string propertyName = string.Empty;
                Type propertyType = null;

                // Target objects
                IDTSPropertiesProvider propertiesProvider = null;
                Variable variable = null;
                PrecedenceConstraint constraint = null;

                // Get the container
                container = FindContainer(package, e.ContainerID);

                // Get the property details and variable objects for the editor
                if (e.Type == typeof(Variable))
                {
                    variable = FindVariable(container, e.ObjectID);

                    propertyName = "Value";
                    propertyType = System.Type.GetType("System." + variable.DataType.ToString());

                    variables = container.Variables;
                    variableDispenser = container.VariableDispenser;
                }
                else if (e.Type == typeof(PrecedenceConstraint))
                {
                    constraint = FindConstraint(container, e.ObjectID);
                    
                    propertyName = "Expression";
                    propertyType = typeof(bool);

                    variables = container.Variables;
                    variableDispenser = container.VariableDispenser;
                }
                else
                {
                    if (e.Type == typeof(ConnectionManager))
                    {
                        propertiesProvider = FindConnectionManager(package, e.ObjectID) as IDTSPropertiesProvider;
                    }
                    else if (e.Type == typeof(ForEachEnumerator))
                    {
                        ForEachLoop forEachLoop = container as ForEachLoop;
                        propertiesProvider = forEachLoop.ForEachEnumerator as IDTSPropertiesProvider;
                    }
                    else
                    {
                        propertiesProvider = container as IDTSPropertiesProvider;
                    }

                    if (propertiesProvider != null)
                    {
                        DtsProperty property = propertiesProvider.Properties[e.Property];
                        propertyName = property.Name;
                        propertyType = PackageHelper.GetTypeFromTypeCode(property.Type);
                        variables = container.Variables;
                        variableDispenser = container.VariableDispenser;
                    }
                    else
                    {
                        throw new Exception(string.Format(CultureInfo.InvariantCulture, "Expression editing not supported on this object ({0}).", e.ObjectID));
                    }
                }

                // Show the editor
                Konesans.Dts.ExpressionEditor.ExpressionEditorPublic editor = new Konesans.Dts.ExpressionEditor.ExpressionEditorPublic(variables, variableDispenser, propertyType, propertyName, e.Expression);
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    // Get expression
                    string expression = editor.Expression;
                    if (expression == null || string.IsNullOrEmpty(expression.Trim()))
                    {
                        expression = null;
                    }

                    // Set the new expression on the target object
                    object objectChanged = null;
                    if (variable != null)
                    {
                        if (expression == null)
                        {
                            variable.EvaluateAsExpression = false;
                        }

                        variable.Expression = expression;
                        objectChanged = variable;
                    }
                    else if (constraint != null)
                    {
                        if (expression == null)
                        {
                            constraint.EvalOp = DTSPrecedenceEvalOp.Constraint;
                        }

                        constraint.Expression = expression;                        
                        objectChanged = constraint;
                    }
                    else if (propertiesProvider != null)
                    {
                        // TaskHost, Sequence, ForLoop, ForEachLoop and ConnectionManager
                        propertiesProvider.SetExpression(e.Property, expression);
                        objectChanged = propertiesProvider;
                    }

                    expressionListWindow_RefreshExpressions(null, null);

                    // Finish displaying expressions list before you mark the package 
                    // as dirty (which runs the expression highlighter)
                    System.Windows.Forms.Application.DoEvents(); 

                    SetPackageAsDirty(package, expression, objectChanged);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetPackageAsDirty(IDTSSequence container, string expression, object objectChanged)
        {
            try
            {
                if (!string.IsNullOrEmpty(expression))
                {
                    shouldSkipExpressionHighlighting = true; //this flag is used by the expression highlighter to skip re-highlighting if all that's changed is the string of an existing expression... if one has been removed, then re-highlight
                }

                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(objectChanged);
                System.ComponentModel.PropertyDescriptor expressionsProperty = properties.Find("Expressions", false);

                // Mark package object as dirty
                IComponentChangeService changeService = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));
                if (objectChanged == null)
                {
                    changeService.OnComponentChanging(container, null);
                    changeService.OnComponentChanged(container, null, null, null); //marks the package designer as dirty
                }
                else
                {
                    changeService.OnComponentChanging(objectChanged, expressionsProperty);
                    changeService.OnComponentChanged(objectChanged, expressionsProperty, null, null); //marks the package designer as dirty
                }
                if (container is Package)
                {
                    SSISHelpers.MarkPackageDirty((Package)container);
                }
            }
            finally
            {
                shouldSkipExpressionHighlighting = false;
            }
        }

        private void expressionListWindow_RefreshExpressions(object sender, EventArgs e)
        {
            expressionListWindow.ClearResults();

            if (win == null)
            {
                return;
            }

            try
            {
                Package package = GetCurrentPackage();
                if (package == null)
                {
                    return;
                }

                expressionListWindow.StartProgressBar();

                IDTSSequence sequence = (IDTSSequence)package;
                processPackage.RunWorkerAsync(sequence);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Package GetCurrentPackage()
        {
            #if DENALI
            // This seems to simple, but it appears to work (Green)
            return (Package)win.PropertiesLinkComponent;
            #else
            Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);

            // Get the package, but may have to walk up the object hierarchy.
            // We need to start at the top with the Package in the worker thread, 
            // otherwise string path functions fail and stuff gets missed.
            DtsContainer container = null;

            if (win.SelectedIndex == 0) //Control Flow
            {
                // Parent of Control Flow diagram is the Package
                DdsDiagramHostControl diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];
                container = (DtsContainer)diagram.ComponentDiagram.RootComponent;
            }
            else if (win.SelectedIndex == 1) // Data flow
            {
                // Parent of Data Flow diagram is a Task
                DdsDiagramHostControl diagram = (DdsDiagramHostControl)viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls["PipelineTaskView"];
                TaskHost taskHost = (TaskHost)diagram.ComponentDiagram.RootComponent;
                container = (DtsContainer)taskHost.Parent; // container is Package
            }
            else if (win.SelectedIndex == 2) //Event Handlers
            {
                // Parent of Event Handlers diagram is a DtsEventHandler
                DdsDiagramHostControl diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
                container = (DtsContainer)diagram.ComponentDiagram.RootComponent;
            }
            else
            {
                return null;
            }

            // Get the root container, i.e the Package
            return GetPackageFromContainer(container);
            #endif
        }

        #region Window Events

        public override void OnWindowClosing(Window Window)
        {
            processPackage.CancelAsync();
            win = null;
        }

        
        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
        }

        //TODO: need to find a way to pick up changes to the package more quickly than just the WindowActivated event
        //The DtsPackageView object seems to have the appropriate methods, but it's internal to the Microsoft.DataTransformationServices.Design assembly.
        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus.Caption == "Expressions") return;
                if (GotFocus == null)
                {
                    return;
                }
                if (GotFocus.DTE.Mode == vsIDEMode.vsIDEModeDebug)
                {
                    return;
                }
                if (GotFocus.Object == null)
                {
                    return;
                }
                designer = GotFocus.Object as IDesignerHost;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if (!(pi.Name.ToLower().EndsWith(".dtsx")))
                {
                    return;
                }

                win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));

                return;

            }
            catch { }
            finally
            {
            }
        }

        #endregion

        #region BackgroundWorker Events

        private void processPackage_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            expressionListWindow.StopProgressBar();
        }

        private void processPackage_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            ExpressionInfo info = (ExpressionInfo)e.UserState;

            if (info.HasExpression)
            {
                expressionListWindow.AddExpression(info.Type, info.ContainerID, info.ObjectID, info.ObjectType, info.ObjectPath, info.ObjectName, info.PropertyName, info.Expression, info.Icon);
            }
        }

        private void processPackage_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker = (System.ComponentModel.BackgroundWorker)sender;

            DtsContainer container = (DtsContainer)e.Argument;
            ProcessObject(container, worker, string.Empty);
        }

        #endregion

        #region Package Scanning

        private void ProcessObject(object component, System.ComponentModel.BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending)
            {
                return;
            }

            DtsContainer container = component as DtsContainer;
            
            // Should only get package as we call GetPackage up front. Could make scope like, but need UI indicator that this is happening
            Package package = component as Package;
            if (package != null)
            {
                path = "\\Package";
                CheckConnectionManagers(package, worker, path);
            }
            else if (!(component is DtsEventHandler))
            {
                path = path + "\\" + container.Name;
            }

            IDTSPropertiesProvider propertiesProvider = component as IDTSPropertiesProvider;
            if (propertiesProvider != null)
            {
                CheckProperties(propertiesProvider, worker, path);
            }

            EventsProvider eventsProvider = component as EventsProvider;
            if (eventsProvider != null)
            {
                foreach (DtsEventHandler eventhandler in eventsProvider.EventHandlers)
                {
                    ProcessObject(eventhandler, worker, path + ".EventHandlers[" + eventhandler.Name + "]");
                }
            }

            IDTSSequence sequence = component as IDTSSequence;
            if (sequence != null)
            {                
                ProcessSequence(container, sequence, worker, path);
                ScanPrecedenceConstraints(worker, path, container.ID, sequence.PrecedenceConstraints);
            }
        }

        private void ProcessSequence(DtsContainer container, IDTSSequence sequence, System.ComponentModel.BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending)
            {
                return;
            }

            foreach (Executable executable in sequence.Executables)
            {
                ProcessObject(executable, worker, path);
            }           
        }

        private void CheckConnectionManagers(Package package, BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending) return;

            foreach (ConnectionManager cm in package.Connections)
            {
                DtsContainer container = (DtsContainer)package;
                // TODO; Fix - Cheat and hard code creation name as icon routines cannot get the correct connection icon
                ScanProperties(worker, path + ".Connections[" + cm.Name + "].", typeof(ConnectionManager), cm.GetType().Name, package.ID, cm.ID, cm.Name, (IDTSPropertiesProvider)cm, PackageHelper.ConnectionCreationName);
            }
        }

        private void CheckProperties(IDTSPropertiesProvider propProvider, BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending) return;

            if (propProvider is DtsContainer)
            {
                DtsContainer container = (DtsContainer)propProvider;
                string containerKey = PackageHelper.GetContainerKey(container);
                string objectTypeName = container.GetType().Name;

                if (container is TaskHost)
                {
                    if (((TaskHost)container).InnerObject is MainPipe)
                    {
                        objectTypeName = typeof(MainPipe).Name; // Prevents it from saying COM Object
                    }
                    else
                    {
                        objectTypeName = ((TaskHost)container).InnerObject.GetType().Name;
                    }

                    ScanProperties(worker, path, typeof(TaskHost), objectTypeName, container.ID, container.ID, container.Name, propProvider, containerKey);
                }
                else if (container is ForEachLoop)
                {
                    ForEachLoop loop = container as ForEachLoop;
                    ScanProperties(worker, path, typeof(ForEachLoop), objectTypeName, container.ID, container.ID, container.Name, propProvider, containerKey);
                    ScanProperties(worker, path + "\\ForEachEnumerator.", typeof(ForEachEnumerator), objectTypeName, container.ID, loop.ForEachEnumerator.ID, container.Name, loop.ForEachEnumerator, containerKey);
                }
                else
                {
                    ScanProperties(worker, path, container.GetType(), objectTypeName, container.ID, container.ID, container.Name, propProvider, containerKey);
                }

                ScanVariables(worker, path, objectTypeName, container.ID, container.Variables);
            }
        }

        private void ScanPrecedenceConstraints(BackgroundWorker worker, string objectPath, string containerID, PrecedenceConstraints constraints)
        {
            if (worker.CancellationPending)
            {
                return;
            }

            foreach (PrecedenceConstraint constraint in constraints)
            {
                if (constraint.EvalOp == DTSPrecedenceEvalOp.Constraint)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(constraint.Expression))
                {
                    continue;
                }

                ExpressionInfo info = new ExpressionInfo();
                info.ContainerID = containerID;
                info.ObjectID = constraint.ID;
                info.ObjectName = ((DtsContainer)constraint.PrecedenceExecutable).Name;
                info.ObjectPath = objectPath + ".PrecedenceConstraints[" + constraint.Name + "]";
                info.Type = typeof(PrecedenceConstraint);
                info.ObjectType = constraint.GetType().Name;
                info.PropertyName = constraint.Name;
                info.Expression = constraint.Expression;
                info.HasExpression = true;
                info.Icon = BIDSHelper.Resources.Common.Path;
                worker.ReportProgress(0, info);
            }
        }

        private void ScanVariables(BackgroundWorker worker, string objectPath, string objectName, string containerID, Variables variables)
        {
            if (worker.CancellationPending)
            {
                return;
            }

            foreach (Variable variable in variables)
            {
                try
                {
                    if (!variable.EvaluateAsExpression)
                    {
                        continue;
                    }

                    // Check path to ensure variable is parented by current scope 
                    // only, not by child containers that inherit the variable
                    if (!variable.GetPackagePath().StartsWith(objectPath + ".Variables["))
                    {
                        continue;
                    }

                    ExpressionInfo info = new ExpressionInfo();
                    info.ContainerID = containerID;
                    info.ObjectID = variable.ID;
                    info.ObjectName = objectName;
                    info.Type = typeof(Variable);
                    info.ObjectPath = objectPath + ".Variables[" + variable.QualifiedName + "]";
                    info.ObjectType = variable.GetType().Name;
                    info.PropertyName = variable.QualifiedName;
                    info.Expression = variable.Expression;
                    info.HasExpression = variable.EvaluateAsExpression;
                    info.Icon = BIDSHelper.Resources.Versioned.Variable;
                    worker.ReportProgress(0, info);
                }
                catch { }
            }
        }

        private void ScanProperties(BackgroundWorker worker, string objectPath, Type objectType, string objectTypeName, string containerID, string objectID, string objectName, IDTSPropertiesProvider provider, string containerKey)
        {
            if (worker.CancellationPending)
            {
                return;
            }

            foreach (DtsProperty property in provider.Properties)
            {
                try
                {
                    string expression = provider.GetExpression(property.Name);
                    if (expression == null)
                    {
                        continue;
                    }

                    System.Diagnostics.Debug.Assert(PackageHelper.ControlFlowInfos.ContainsKey(containerKey));

                    ExpressionInfo info = new ExpressionInfo();
                    info.ContainerID = containerID;
                    info.ObjectID = objectID;
                    info.Type = objectType;
                    info.ObjectName = objectName;

                    if (property.Name.StartsWith("["))
                    {
                        info.ObjectPath = objectPath + ".Properties" + property.Name + "";
                    }
                    else
                    {
                        info.ObjectPath = objectPath + ".Properties[" + property.Name + "]";
                    }

                    info.ObjectType = objectTypeName;
                    info.PropertyName = property.Name;
                    info.Expression = expression;
                    info.HasExpression = (info.Expression != null);
                    info.Icon = PackageHelper.ControlFlowInfos[containerKey].Icon;
                    worker.ReportProgress(0, info);
                }
                catch { }
            }
        }

        private DtsContainer FindContainer(DtsContainer component, string objectId)
        {
            //DtsContainer container = component as DtsContainer;

            if (component == null)
            {
                return null;
            }
            else if (component.ID == objectId)
            {
                return component;
            }

            EventsProvider eventsProvider = component as EventsProvider;
            if (eventsProvider != null)
            {
                foreach (DtsEventHandler eventhandler in eventsProvider.EventHandlers)
                {
                    DtsContainer container = FindContainer(eventhandler, objectId);
                    if (container != null)
                    {
                        return container;
                    }
                }
            }

            IDTSSequence sequence = component as IDTSSequence;
            if (sequence != null)
            {
                foreach (Executable executable in sequence.Executables)
                {
                    DtsContainer container = FindContainer((DtsContainer)executable, objectId);
                    if (container != null)
                    {
                        return container;
                    }
                }   
            }

            return null;
        }

        private Variable FindVariable(DtsContainer container, string objectID)
        {
            if (container != null)
            {
                if (container.Variables.Contains(objectID))
                {
                    return container.Variables[objectID];
                }
            }

            return null;
        }

        private ConnectionManager FindConnectionManager(Package package, string objectID)
        {
            if (package.Connections.Contains(objectID))
            {
                return package.Connections[objectID];
            }

            return null;
        }

        private PrecedenceConstraint FindConstraint(DtsContainer container, string objectID)
        {
            IDTSSequence sequence = container as IDTSSequence;

            if (sequence == null)
            {
                System.Diagnostics.Debug.Assert(false, "sequence cannot be found");
                return null;
            }

            if (sequence.PrecedenceConstraints.Contains(objectID))
            {
                return sequence.PrecedenceConstraints[objectID];
            }

            return null;
        }

        private Package GetPackageFromContainer(DtsContainer container)
        {
            while (!(container is Package))
            {
                container = container.Parent;
            }
            return (Package)container;
        }

        #endregion

        public override string ShortName
        {
            get { return "ExpressionList"; }
        }

        public override int Bitmap
        {
            get { return 6; }
        }

        public override string ButtonText
        {
            get { return "Expression List (BIDS Helper)"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        public override string MenuName
        {
            get { return "Other Windows"; }
        }

        public override string FeatureName
        {
            get { return "Expression List"; }
        }

        public override bool Checked
        {
            get { return toolWindow.Visible; }
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
            get { return "Provides a tool window listing expressions defined in a package, making it easy to review and manage expressions. Editing uses the integrated advanced expression editor."; }
        }

        public override bool AddCommandToMultipleMenus
        {
            get { return false; } //the Other Windows menu is nested under other menus, so this Add to multiple menus logic won't work here
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
                if (toolWindow.Visible) return true;
                if (this.ApplicationObject.Solution == null) return false;
                foreach (EnvDTE.Project p in this.ApplicationObject.Solution.Projects)
                {
                    if (p.Kind == BIDSProjectKinds.SSIS) return true;
                }
                return false;
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
                toolWindow.Visible = !toolWindow.Visible;
                string path = Connect.REGISTRY_BASE_PATH + "\\" + REGISTRY_EXTENDED_PATH;
                RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(path, true);
                if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(path);
                settingKey.SetValue(REGISTRY_SETTING_NAME, toolWindow.Visible, RegistryValueKind.DWord);
                settingKey.Close();
                expressionListWindow.ClearResults();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("The Expression List could not be toggled. Error: " + e.Message);
            }
        }

        private struct ExpressionInfo
        {
            public Type Type;
            public string ObjectType;
            public string ObjectName;
            public string ContainerID;
            public string ObjectID;
            public string ObjectPath;
            public string PropertyName;
            public string Expression;
            public bool HasExpression;
            public Icon Icon;
        }

    }
}