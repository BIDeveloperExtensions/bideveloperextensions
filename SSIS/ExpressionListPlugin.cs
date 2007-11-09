using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
//using Microsoft.VisualStudio.CommandBars;
//using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using System;
using Microsoft.Win32;
using MSDDS;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.ComponentModel;
//using Konesans.Dts.Design.Controls;
//using Konesans.Dts.Design.PropertyHelp;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
 


namespace BIDSHelper
{
    public class ExpressionListPlugin : BIDSHelperWindowActivatedPluginBase
    {
        private const string REGISTRY_EXTENDED_PATH = "ExpressionListPlugin";
        private const string REGISTRY_SETTING_NAME = "InEffect";
        public static bool bShouldSkipExpressionHighlighting = false;

        //private WindowEvents windowEvents;
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        //private System.Collections.Generic.List<string> windowHandlesFixedForExpressionHighlighter = new System.Collections.Generic.List<string>();
        //private System.Collections.Generic.List<string> windowHandlesInProgressStatus = new System.Collections.Generic.List<string>();
        private ExpressionListControl expressionListWindow = null;
        //private DTE2 appObject = null;
        Window toolWindow = null;
        System.Reflection.Assembly konesansAssembly = null;
        Type typePropertyVariables = null;

        EditorWindow win = null;
        IDesignerHost designer = null;
        System.ComponentModel.BackgroundWorker processPackage = null;

        public ExpressionListPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
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
        public override bool ShouldHookWindowClosing
        {
            get
            {
                return true;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            // TODO - unhook other event handlers
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
            toolWindow = windows2.CreateToolWindow2(this.AddInInstance, asm.Location, "BIDSHelper.ExpressionListControl", "Expressions", guidstr, ref programmableObject);
            expressionListWindow = (ExpressionListControl)programmableObject;
            expressionListWindow.RefreshExpressions += new EventHandler(expressionListWindow_RefreshExpressions);
            expressionListWindow.EditExpressionSelected += new EventHandler<EditExpressionSelectedEventArgs>(expressionListWindow_EditExpressionSelected);

            //Set the picture displayed when the window is tab docked
            //expressionListWindow.SetTabPicture(BIDSHelper.Resources.Resource.ExpressionList.ToBitmap().GetHbitmap());

            //toolWindow.Visible = true;

        }

        void expressionListWindow_EditExpressionSelected(object sender, EditExpressionSelectedEventArgs e)
        {
            try
            {

                IDTSSequence container = null;
                if (win == null) return;

                try
                {
                    Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
                    DdsDiagramHostControl diagram = null;

                    if (win.SelectedIndex == 0) //Control Flow
                    {
                        diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];
                        container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                    }
                    else if (win.SelectedIndex == 1) //data flow
                    {
                        diagram = (DdsDiagramHostControl)viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls["PipelineTaskView"];
                        container = (IDTSSequence)((TaskHost)diagram.ComponentDiagram.RootComponent).Parent;
                    }
                    else if (win.SelectedIndex == 2) //Event Handlers
                    {
                        diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
                        container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                    }
                    else
                    {
                        return;
                    }

                    container = (IDTSSequence)GetPackageFromContainer((DtsContainer)container);
                }
                catch (Exception)
                {
                    return;
                }

                EnsurePropertyVariablesTypeLoaded();

                Package package = (Package)container;
                Variables variables = null;
                VariableDispenser variableDispenser = null;
                PropertyDescriptor property = null;
                Type propertyType = null;
                TaskHost taskHost = FindTaskHost(container, e.ObjectID);
                DtsContainer foundContainer = FindContainer(container, e.ObjectID);
                Variable variable = null;
                if (e.ObjectType.StartsWith("Variable "))
                {
                    variable = FindVariable(package, taskHost, foundContainer, e.Property);
                }
                ConnectionManager connection = FindConnectionManager(package, e.ObjectID);
                if (variable != null)
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(variable);
                    property = properties.Find("Value", false);
                    propertyType = System.Type.GetType("System." + variable.DataType.ToString());
                    if (taskHost != null)
                    {
                        variables = taskHost.Variables;
                        variableDispenser = taskHost.VariableDispenser;
                    }
                    else if (foundContainer != null)
                    {
                        variables = foundContainer.Variables;
                        variableDispenser = foundContainer.VariableDispenser;
                    }
                    else
                    {
                        variables = package.Variables;
                        variableDispenser = package.VariableDispenser;
                    }
                }
                else if (taskHost != null)
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(taskHost);
                    property = properties.Find(e.Property, false);
                    propertyType = property.PropertyType;
                    variables = taskHost.Variables;
                    variableDispenser = taskHost.VariableDispenser;
                }
                else if (connection != null)
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(connection);
                    property = properties.Find(e.Property, false);
                    propertyType = property.PropertyType;
                    variables = package.Variables;
                    variableDispenser = package.VariableDispenser;
                }
                else if (e.ObjectID == ((Package)container).ID)
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(container);
                    property = properties.Find(e.Property, false);
                    propertyType = property.PropertyType;
                    variables = package.Variables;
                    variableDispenser = package.VariableDispenser;
                }
                else
                {
                    throw new Exception("Expression editing not supported on this object."); //will usually be when trying to edit an expression on the Package object itself; TODO: figure out a way to see if this is possible
                }

                System.Reflection.BindingFlags getpropflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance;
                System.Reflection.BindingFlags setpropflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance;
                object oPropertyVariables = typePropertyVariables.GetConstructors()[0].Invoke(new object[] { });
                oPropertyVariables.GetType().InvokeMember("Variables", setpropflags, null, oPropertyVariables, new object[] { variables });
                oPropertyVariables.GetType().InvokeMember("VariableDispenser", setpropflags, null, oPropertyVariables, new object[] { variableDispenser });
                oPropertyVariables.GetType().InvokeMember("Type", setpropflags, null, oPropertyVariables, new object[] { propertyType });

                Type typeExpressionEditorPublic = konesansAssembly.GetType("Konesans.Dts.Design.Controls.ExpressionEditorPublic");
                Form editor = (Form)typeExpressionEditorPublic.GetConstructors()[0].Invoke(new object[] { e.Expression, oPropertyVariables, property });
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    string sExpression = (string)editor.GetType().InvokeMember("Expression", getpropflags, null, editor, null);

                    if (variable != null)
                    {
                        variable.Expression = sExpression;
                    }
                    else if (taskHost != null)
                    {
                        taskHost.SetExpression(e.Property, sExpression);
                    }
                    else if (connection != null)
                    {
                        connection.SetExpression(e.Property, sExpression);
                    }
                    else if (e.ObjectID == ((Package)container).ID)
                    {
                        package.SetExpression(e.Property, sExpression);
                    }

                    expressionListWindow_RefreshExpressions(null, null);
                    System.Windows.Forms.Application.DoEvents(); //finish displaying expressions list before you mark the package as dirty (which runs the expression highlighter)

                    try
                    {
                        if (!string.IsNullOrEmpty(sExpression))
                            bShouldSkipExpressionHighlighting = true; //this flag is used by the expression highlighter to skip re-highlighting if all that's changed is the string of an existing expression... if one has been removed, then re-highlight

                        //mark package object as dirty
                        IComponentChangeService changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));
                        changesvc.OnComponentChanging(container, null);
                        changesvc.OnComponentChanged(container, null, null, null); //marks the package designer as dirty
                    }
                    finally
                    {
                        bShouldSkipExpressionHighlighting = false;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        #region Late Binding to Konesans Assembly
        private void EnsureKonesansAssemblyLoaded()
        {
            if (konesansAssembly == null)
            {
                konesansAssembly = System.Reflection.Assembly.Load(BIDSHelper.Properties.Resources.Konesans_Dts_CommonLibrary);
            }
        }

        private void EnsurePropertyVariablesTypeLoaded()
        {
            EnsureKonesansAssemblyLoaded();
            if (typePropertyVariables != null) return;

            Type typeInterface = konesansAssembly.GetType("Konesans.Dts.Design.PropertyHelp.IPropertyRuntimeVariables");

            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "BIDSHelperKonesans";

            AssemblyBuilder newAssembly = System.Threading.Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModule = newAssembly.DefineDynamicModule("KonesansInterfaces");
            TypeBuilder myTypeBuilder = newModule.DefineType("PropertyVariables", TypeAttributes.Public);
            myTypeBuilder.AddInterfaceImplementation(typeInterface);

            FieldBuilder fieldVariables = myTypeBuilder.DefineField("_variables", typeof(Variables), FieldAttributes.Private);
            FieldBuilder fieldVariableDispenser = myTypeBuilder.DefineField("_variableDispenser", typeof(VariableDispenser), FieldAttributes.Private);
            FieldBuilder fieldType = myTypeBuilder.DefineField("_type", typeof(Type), FieldAttributes.Private);



            PropertyBuilder propertyBuilderVariables = myTypeBuilder.DefineProperty("Variables",
                                     PropertyAttributes.HasDefault,
                                     typeof(Variables),
                                     new Type[] { typeof(Variables) });
            
            //define the behavior of the "get" property
            MethodBuilder methodBuilderGetVariables = myTypeBuilder.DefineMethod("GetVariables",
                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                    typeof(Variables),
                                    new Type[] { });

            ILGenerator ilGetVariables = methodBuilderGetVariables.GetILGenerator();
            ilGetVariables.Emit(OpCodes.Ldarg_0);
            ilGetVariables.Emit(OpCodes.Ldfld, fieldVariables);
            ilGetVariables.Emit(OpCodes.Ret);

            //define the behavior of the "set" property
            MethodBuilder methodBuilderSetVariables = myTypeBuilder.DefineMethod("SetVariables",
                                    MethodAttributes.Public,
                                    null,
                                    new Type[] { typeof(Variables) });

            ILGenerator ilSetVariables = methodBuilderSetVariables.GetILGenerator();
            ilSetVariables.Emit(OpCodes.Ldarg_0);
            ilSetVariables.Emit(OpCodes.Ldarg_1);
            ilSetVariables.Emit(OpCodes.Stfld, fieldVariables);
            ilSetVariables.Emit(OpCodes.Ret);

            //Map the two methods created above to our PropertyBuilder to 
            //their corresponding behaviors, "get" and "set" respectively. 
            propertyBuilderVariables.SetGetMethod(methodBuilderGetVariables);
            propertyBuilderVariables.SetSetMethod(methodBuilderSetVariables);

            MethodInfo methodInfoGetVariables = typeInterface.GetProperty("Variables").GetGetMethod();
            myTypeBuilder.DefineMethodOverride(methodBuilderGetVariables, methodInfoGetVariables);



            PropertyBuilder propertyBuilderVariableDispenser = myTypeBuilder.DefineProperty("VariableDispenser",
                         PropertyAttributes.HasDefault,
                         typeof(VariableDispenser),
                         new Type[] { typeof(VariableDispenser) });

            //define the behavior of the "get" property
            MethodBuilder methodBuilderGetVariableDispenser = myTypeBuilder.DefineMethod("GetVariableDispenser",
                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                    typeof(VariableDispenser),
                                    new Type[] { });

            ILGenerator ilGetVariableDispenser = methodBuilderGetVariableDispenser.GetILGenerator();
            ilGetVariableDispenser.Emit(OpCodes.Ldarg_0);
            ilGetVariableDispenser.Emit(OpCodes.Ldfld, fieldVariableDispenser);
            ilGetVariableDispenser.Emit(OpCodes.Ret);

            //define the behavior of the "set" property
            MethodBuilder methodBuilderSetVariableDispenser = myTypeBuilder.DefineMethod("SetVariableDispenser",
                                    MethodAttributes.Public,
                                    null,
                                    new Type[] { typeof(VariableDispenser) });

            ILGenerator ilSetVariableDispenser = methodBuilderSetVariableDispenser.GetILGenerator();
            ilSetVariableDispenser.Emit(OpCodes.Ldarg_0);
            ilSetVariableDispenser.Emit(OpCodes.Ldarg_1);
            ilSetVariableDispenser.Emit(OpCodes.Stfld, fieldVariableDispenser);
            ilSetVariableDispenser.Emit(OpCodes.Ret);

            //Map the two methods created above to our PropertyBuilder to 
            //their corresponding behaviors, "get" and "set" respectively. 
            propertyBuilderVariableDispenser.SetGetMethod(methodBuilderGetVariableDispenser);
            propertyBuilderVariableDispenser.SetSetMethod(methodBuilderSetVariableDispenser);

            MethodInfo methodInfoGetVariableDispenser = typeInterface.GetProperty("VariableDispenser").GetGetMethod();
            myTypeBuilder.DefineMethodOverride(methodBuilderGetVariableDispenser, methodInfoGetVariableDispenser);



            PropertyBuilder propertyBuilderType = myTypeBuilder.DefineProperty("Type",
                                     PropertyAttributes.HasDefault,
                                     typeof(Type),
                                     new Type[] { typeof(Type) });

            //define the behavior of the "get" property
            MethodBuilder methodBuilderGetType = myTypeBuilder.DefineMethod("GetType",
                                    MethodAttributes.Public,
                                    typeof(Type),
                                    new Type[] { });

            ILGenerator ilGetType = methodBuilderGetType.GetILGenerator();
            ilGetType.Emit(OpCodes.Ldarg_0);
            ilGetType.Emit(OpCodes.Ldfld, fieldType);
            ilGetType.Emit(OpCodes.Ret);

            //define the behavior of the "set" property
            MethodBuilder methodBuilderSetType = myTypeBuilder.DefineMethod("SetType",
                                    MethodAttributes.Public,
                                    null,
                                    new Type[] { typeof(Type) });

            ILGenerator ilSetType = methodBuilderSetType.GetILGenerator();
            ilSetType.Emit(OpCodes.Ldarg_0);
            ilSetType.Emit(OpCodes.Ldarg_1);
            ilSetType.Emit(OpCodes.Stfld, fieldType);
            ilSetType.Emit(OpCodes.Ret);

            //Map the two methods created above to our PropertyBuilder to 
            //their corresponding behaviors, "get" and "set" respectively. 
            propertyBuilderType.SetGetMethod(methodBuilderGetType);
            propertyBuilderType.SetSetMethod(methodBuilderSetType);



            MethodBuilder methodBuilderGetPropertyType =
               myTypeBuilder.DefineMethod(
               "GetPropertyType",
               MethodAttributes.Public | MethodAttributes.Virtual,
               typeof(Type),
               new Type[] { typeof(string) });

            ILGenerator ilGetPropertyType = methodBuilderGetPropertyType.GetILGenerator();
            ilGetPropertyType.Emit(OpCodes.Ldarg_0);
            ilGetPropertyType.Emit(OpCodes.Ldfld, fieldType);
            ilGetPropertyType.Emit(OpCodes.Ret);

            MethodInfo methodInfoGetPropertyType = typeInterface.GetMethod(methodBuilderGetPropertyType.Name);
            myTypeBuilder.DefineMethodOverride(methodBuilderGetPropertyType, methodInfoGetPropertyType);



            typePropertyVariables = myTypeBuilder.CreateType();
        }
        #endregion


        void expressionListWindow_RefreshExpressions(object sender, EventArgs e)
        {
            IDTSSequence container = null;
            TaskHost taskHost = null;

            expressionListWindow.ClearResults();

            if (win == null) return;

            try
            {
                Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
                DdsDiagramHostControl diagram = null;

                if (win.SelectedIndex == 0) //Control Flow
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];
                    container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                }
                else if (win.SelectedIndex == 1) //data flow
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls["PipelineTaskView"];
                    taskHost = (TaskHost)diagram.ComponentDiagram.RootComponent;
                    container = (IDTSSequence)taskHost.Parent;
                }
                else if (win.SelectedIndex == 2) //Event Handlers
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
                    container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                }
                else
                {
                    return;
                }

                expressionListWindow.StartProgressBar();
                processPackage.RunWorkerAsync(container);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                designer = (IDesignerHost)GotFocus.Object;
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

        void processPackage_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            expressionListWindow.StopProgressBar();
        }

        void processPackage_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            ExpressionInfo info = (ExpressionInfo)e.UserState;

            if (info.HasExpression)
            {
                expressionListWindow.AddExpression(info.ObjectID, info.ObjectType, info.ObjectPath, info.ObjectName, info.PropertyName, info.Expression);
            }
        }

        void processPackage_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker = (System.ComponentModel.BackgroundWorker)sender;

            DtsContainer sequence = (DtsContainer)e.Argument;

            IterateContainer(sequence, worker, string.Empty);
        }

        #endregion

        #region Package Scanning

        private void IterateContainer(DtsContainer container, System.ComponentModel.BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending) return;

            if (container is Package)
            {
                path = "Package.";
                CheckConnectionManagers((Package)container, worker, path);
            }

            if (container is IDTSPropertiesProvider)
            {
                CheckProperties((IDTSPropertiesProvider)container, worker, path);
            }

            IDTSSequence sequence = (IDTSSequence)container;

            foreach (Executable exec in sequence.Executables)
            {
                string sNewPath = path;
                if (!(container is Package)) sNewPath = path.Substring(0, path.Length - 1) + "\\" + container.Name + ".";
                if (exec is IDTSSequence)
                {
                    IterateContainer((DtsContainer)exec, worker, sNewPath);
                }
                else if (exec is IDTSPropertiesProvider)
                {
                    CheckProperties((IDTSPropertiesProvider)exec, worker, sNewPath);
                }
            }
        }

        private Package GetPackageFromContainer(DtsContainer container)
        {
            while (!(container is Package))
            {
                container = container.Parent;
            }
            return (Package)container;
        }

        private void CheckConnectionManagers(Package package, BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending) return;

            foreach (ConnectionManager cm in package.Connections)
            {
                DtsContainer container = (DtsContainer)package;
                ScanProperties(worker, path + "Connections[" + cm.Name + "].", cm.GetType().ToString(), cm.ID, cm.Name, (IDTSPropertiesProvider)cm);
            }
        }

        private void CheckProperties(IDTSPropertiesProvider propProvider, BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending) return;

            if (propProvider is DtsContainer)
            {
                DtsContainer container = (DtsContainer)propProvider;
                string sNewPath = path;
                if (!(container is Package)) sNewPath = path.Substring(0, path.Length - 1) + "\\" + container.Name + ".";
                if (container is TaskHost)
                {
                    ScanProperties(worker, sNewPath, ((TaskHost)container).InnerObject.GetType().ToString(), container.ID, container.Name, propProvider);
                }
                else
                {
                    ScanProperties(worker, sNewPath, container.GetType().ToString(), container.ID, container.Name, propProvider);
                }
                ScanVariables(worker, sNewPath, container.GetType().ToString(), container.ID, container.Name, container.Variables);
            }
        }

        private void ScanVariables(BackgroundWorker worker, string objectPath, string objectType, string objectID, string objectName, Variables variables)
        {
            if (worker.CancellationPending) return;

            foreach (Variable v in variables)
            {
                try
                {
                    if (!v.EvaluateAsExpression) continue;
                    if (!v.GetPackagePath().StartsWith("\\" + objectPath + "Variables[")) continue;
                    ExpressionInfo info = new ExpressionInfo();
                    info.ObjectID = objectID;
                    info.ObjectName = objectName;
                    info.ObjectPath = objectPath;
                    info.ObjectType = v.GetType().ToString();
                    info.PropertyName = v.Name;
                    info.Expression = v.Expression;
                    info.HasExpression = v.EvaluateAsExpression;
                    worker.ReportProgress(0, info);
                }
                catch { }
            }
        }

        private void ScanProperties(System.ComponentModel.BackgroundWorker worker, string objectPath, string objectType, string objectID, string objectName, IDTSPropertiesProvider provider)
        {
            if (worker.CancellationPending) return;

            foreach (DtsProperty p in provider.Properties)
            {
                try
                {
                    string expression = provider.GetExpression(p.Name);
                    if (expression == null)
                    {
                        continue;
                    }

                    ExpressionInfo info = new ExpressionInfo();
                    info.ObjectID = objectID;
                    info.ObjectName = objectName;
                    info.ObjectPath = objectPath + "Properties[" + p.Name + "]";
                    info.ObjectType = objectType;
                    info.PropertyName = p.Name;
                    info.Expression = expression;
                    info.HasExpression = (info.Expression != null);
                    worker.ReportProgress(0, info);
                }
                catch { }
            }
        }

        TaskHost FindTaskHost(IDTSSequence parentExecutable, string sObjectGuid)
        {
            TaskHost matchingExecutable = null;

            if (parentExecutable.Executables.Contains(sObjectGuid))
            {
                matchingExecutable = parentExecutable.Executables[sObjectGuid] as TaskHost;
            }
            else
            {
                foreach (Executable e in parentExecutable.Executables)
                {
                    if (e is IDTSSequence)
                    {
                        matchingExecutable = FindTaskHost((IDTSSequence)e, sObjectGuid);
                    }
                }
            }
            return matchingExecutable;
        }

        DtsContainer FindContainer(IDTSSequence parentExecutable, string sObjectGuid)
        {
            DtsContainer matchingExecutable = null;

            if (parentExecutable.Executables.Contains(sObjectGuid))
            {
                matchingExecutable = parentExecutable.Executables[sObjectGuid] as DtsContainer;
            }
            else
            {
                foreach (Executable e in parentExecutable.Executables)
                {
                    if (e is IDTSSequence)
                    {
                        matchingExecutable = FindContainer((IDTSSequence)e, sObjectGuid);
                    }
                }
            }
            return matchingExecutable;
        }

        Variable FindVariable(Package package, TaskHost taskHost, DtsContainer container, string variableName)
        {
            if (taskHost == null && container == null)
            {
                if (package.Variables.Contains(variableName))
                {
                    return package.Variables[variableName];
                }
            }
            else if (taskHost != null)
            {
                if (taskHost.Variables.Contains(variableName))
                {
                    return taskHost.Variables[variableName];
                }
            }
            else if (container != null)
            {
                if (container.Variables.Contains(variableName))
                {
                    return container.Variables[variableName];
                }
            }
            return null;
        }

        ConnectionManager FindConnectionManager(Package container, string sObjectGuid)
        {
            if (container.Connections.Contains(sObjectGuid))
            {
                return (ConnectionManager)container.Connections[sObjectGuid];
            }
            return null;
        }

        #endregion

        public override string ShortName
        {
            get { return "ExpressionList"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "Expression List"; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }

        public override bool ShouldPositionAtEnd
        {
            get { return false; }
        }

        public override string MenuName
        {
            get { return "Tools"; }
        }

        public override bool Checked
        {
            get { return toolWindow.Visible; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            return true;
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
            public string ObjectType;
            public string ObjectName;
            public string ObjectID;
            public string ObjectPath;
            public string PropertyName;
            public string Expression;
            public bool HasExpression;
        }

    }
}