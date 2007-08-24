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
using MSDDS;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.ComponentModel;

namespace BIDSHelper
{
    public class ExpressionListPlugin : BIDSHelperPluginBase
    {
        private const string REGISTRY_EXTENDED_PATH = "ExpressionListPlugin";
        private const string REGISTRY_SETTING_NAME = "InEffect";

        private WindowEvents windowEvents;
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.List<string> windowHandlesFixedForExpressionHighlighter = new System.Collections.Generic.List<string>();
        private System.Collections.Generic.List<string> windowHandlesInProgressStatus = new System.Collections.Generic.List<string>();
        private ExpressionListControl expressionListWindow = null;
        private DTE2 appObject = null;
        Window toolWindow = null;

            
        EditorWindow win = null;
        System.ComponentModel.BackgroundWorker processPackage = null;
        bool windowIsVisible = false;

        public ExpressionListPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(Connect.REGISTRY_BASE_PATH + "\\" + REGISTRY_EXTENDED_PATH);
            if (rk != null)
            {
                windowIsVisible = (1 == (int)rk.GetValue(REGISTRY_SETTING_NAME, 0));
                rk.Close();
            }

            this.appObject= appObject;
            windowEvents = appObject.Events.get_WindowEvents(null);
            windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
            windowEvents.WindowClosing += new _dispWindowEvents_WindowClosingEventHandler(windowEvents_WindowClosing);

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
            EnvDTE80.Windows2 windows2 = (EnvDTE80.Windows2)appObject.Windows;
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            toolWindow = windows2.CreateToolWindow2(addinInstance, asm.Location, "BIDSHelper.ExpressionListControl", "Expressions", guidstr, ref programmableObject);
            expressionListWindow = (ExpressionListControl)programmableObject;
            expressionListWindow.RefreshExpressions += new EventHandler(expressionListWindow_RefreshExpressions);

            //Set the picture displayed when the window is tab docked
            //expressionListWindow.SetTabPicture(BIDSHelper.Resources.Resource.ExpressionList.ToBitmap().GetHbitmap());

            //toolWindow.Visible = true;

        }

        void expressionListWindow_RefreshExpressions(object sender, EventArgs e)
        {
            IDTSSequence container = null;
            TaskHost taskHost = null;

            DataGridView dgv = (DataGridView)expressionListWindow.Controls["DataGridView1"];
            dgv.Rows.Clear();

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

                processPackage.RunWorkerAsync(container);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Window Events

        void windowEvents_WindowClosing(Window Window)
        {
            processPackage.CancelAsync();
            win = null;
        }

        void windowEvents_WindowCreated(Window Window)
        {
            windowEvents_WindowActivated(Window, null);
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            windowEvents_WindowActivated(this.ApplicationObject.ActiveWindow, null);
        }

        //TODO: need to find a way to pick up changes to the package more quickly than just the WindowActivated event
        //The DtsPackageView object seems to have the appropriate methods, but it's internal to the Microsoft.DataTransformationServices.Design assembly.
        void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
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
                IDesignerHost designer = (IDesignerHost)GotFocus.Object;
                if (designer == null)
                {
                    return;
                }
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
                string[] newRow = { info.ObjectType, info.ObjectPath, info.ObjectName, info.PropertyName, info.Expression };

                DataGridView dgv = (DataGridView)expressionListWindow.Controls["DataGridView1"];
                dgv.Rows.Add(newRow);
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
                if (exec is IDTSSequence)
                {
                    IterateContainer((DtsContainer)exec, worker, path);
                }
                else if (exec is IDTSPropertiesProvider)
                {
                    CheckProperties((IDTSPropertiesProvider)exec, worker, path);
                }
            }
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
                ScanProperties(worker, path, container.GetType().ToString(), container.ID, container.Name, propProvider);
                ScanVariables(worker, path, container.GetType().ToString(), container.ID, container.Name, container.Variables);
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
                    info.ObjectPath = objectPath + "Properties["+p.Name+"]";
                    info.ObjectType = objectType;
                    info.PropertyName = p.Name;
                    info.Expression = expression;
                    info.HasExpression = (info.Expression != null);
                    worker.ReportProgress(0, info);
                }
                catch { }
            }
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
            get { return windowIsVisible; }
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
                windowIsVisible = !windowIsVisible;
                toolWindow.Visible = windowIsVisible;
                string path = Connect.REGISTRY_BASE_PATH + "\\" + REGISTRY_EXTENDED_PATH;
                RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(path, true);
                if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(path);
                settingKey.SetValue(REGISTRY_SETTING_NAME, windowIsVisible, RegistryValueKind.DWord);
                settingKey.Close();

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