using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Windows.Forms;
using BIDSHelper.SSAS;
using EnvDTE;
using EnvDTE80;
using Microsoft.AnalysisServices;
using Microsoft.VisualStudio.Shell;

namespace BIDSHelper
{



    public class CustomDesignRulesPlugin : BIDSHelperPluginBase, System.IServiceProvider
    {
        const string DESIGN_RULE_CATEGORY = "Custom Design Rules";

        ErrorListProvider m_objErrorListProvider;
        List<ErrorTask> m_colErrorTasks;

        public CustomDesignRulesPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
            InitializeErrorProvider();
        }

        public override string ShortName
        {
            get { return "CustomDesignRules"; }
        }

        public override string FeatureName
        {
            get
            {
                return "Runs check scripts for a set of custom design rules";
            }
        }

        public override int Bitmap
        {
            get { return 313; }
        }

        public override string ButtonText
        {
            get { return "Check Design Rules"; }
        }

        public override string ToolTip
        {
            get { return "Checks the design rules for this project"; }
        }

        public override string MenuName
        {
            get { return "Project,Item,Solution"; }
        }

        public override bool ShouldPositionAtEnd
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        public override string HelpUrl
        {
            get { return this.GetCodePlexHelpUrl("Custom Design Rules"); }
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
            get { return "Allows you to right click on an Analysis Services solution and run powershell scripts to check a set of custom design rules."; }
        }

        private OutputWindowPane GetBuildOutputWindowPane()
        {
            //const string BUILD_OUTPUT_PANE_GUID = "{1BD8A850-02D1-11D1-BEE7-00A0C913D1F8}";
            const string DEBUG_OUTPUT_PANE_GUID = "{FC076020-078A-11D1-A7DF-00A0C9110051}";

            Window objOutputToolWindow = this.AddInInstance.DTE.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            OutputWindow objOutputWindow = (OutputWindow)objOutputToolWindow.Object;
            OutputWindowPane objBuildOutputWindowPane = null;
            foreach (OutputWindowPane objOutputWindowPane in objOutputWindow.OutputWindowPanes)
            {
                if (objOutputWindowPane.Guid.ToUpper() == DEBUG_OUTPUT_PANE_GUID)
                {
                    objBuildOutputWindowPane = objOutputWindowPane;
                    break;
                }
            }

            return objBuildOutputWindowPane;
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
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;

                // test if this is a Multi-Dim project
                if (hierItem.Object is EnvDTE.Project)
                {
                    EnvDTE.Project p = (EnvDTE.Project)hierItem.Object;
                    Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = p as Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
                    if (p == null) return false;

                    if (projExt.Kind == BIDSProjectKinds.SSAS)
                    {

                        return true;
                        //Database db = (Database)p.Object;
                        //ScanAnalysisServicesProperties(db);
                    }
                }
                // else test if this is a tabular .bim file
                if (!(hierItem.Object is ProjectItem)) return false;
                string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                return (sFileName.EndsWith(".bim"));

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
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                //SolutionClass solution = hierItem.Object as SolutionClass;
                
                Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = null;
                Database db = null;
                bool targetFound=false;
                if (hierItem.Object is EnvDTE.Project)
                {
                    EnvDTE.Project p = (EnvDTE.Project)hierItem.Object;
                    Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)p;
                    if (projExt.Kind == BIDSProjectKinds.SSAS)
                    {
                        db = (Database)p.Object;
                        targetFound = true;
                    }
                }
                if (hierItem.Object is ProjectItem)
                {
                    string sFileName = ((ProjectItem) hierItem.Object).Name.ToLower();
                    if (sFileName.EndsWith(".bim"))
                    {
                        sandbox = TabularHelpers.GetTabularSandboxFromBimFile(hierItem, true);
                        targetFound = true;
                    }
                }

                if (targetFound)
                {
                    RunCustomDesignRules(db, sandbox);
                }
                else
                {
                    MessageBox.Show("No valid design rule target found");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void RunCustomDesignRules(Database db, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            if (((System.Array)solExplorer.SelectedItems).Length != 1)
                return;

            //UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
            // TODO - get database properties
            //ProjectItem pi = (ProjectItem)hierItem.Object;

            //Window w = pi.Open(BIDSViewKinds.Designer); //opens the designer
            //w.Activate();

            try
            {
                //ClearTaskList();
                ClearErrorList();
                ApplicationObject.StatusBar.Text = "Starting Powershell Design Rules Engine...";
                var ps = PowerShell.Create();
                string modelType;
                if (db == null)
                {
                    modelType = "Tabular";
                    ps.Runspace.SessionStateProxy.SetVariable("WorkspaceConnection", sandbox.AdomdConnection);
                    ps.Runspace.SessionStateProxy.SetVariable("CurrentCube", sandbox.Cube);
                }
                else
                {
                    modelType = "MultiDim";
                    ps.Runspace.SessionStateProxy.SetVariable("CurrentDB", db);
                }
                ps.Runspace.SessionStateProxy.SetVariable("ModelType", modelType);
                ps.Runspace.SessionStateProxy.SetVariable("VerbosePreference", "Continue");


                ps.Streams.Debug.DataAdded += new EventHandler<DataAddedEventArgs>(Debug_DataAdded);
                ps.Streams.Warning.DataAdded += new EventHandler<DataAddedEventArgs>(Warning_DataAdded);
                ps.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>(Error_DataAdded);
                ps.Streams.Verbose.DataAdded += new EventHandler<DataAddedEventArgs>(Verbose_DataAdded);

                ApplicationObject.StatusBar.Text = "Loading Custom Design Rules...";

                // TODO - look into adding a script to allow calling a copy from a central rule repository

                string dllFile = (new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
                System.Diagnostics.Debug.WriteLine(dllFile);
                FileInfo dll = new FileInfo(dllFile);
                DirectoryInfo scripts = new DirectoryInfo(dll.Directory.FullName + "\\SSAS_Design_Rules");

                foreach (FileInfo f in scripts.GetFiles(modelType + "_*.ps1"))
                {
                    TextReader tr = new StreamReader(f.OpenRead());

                    string psScript = tr.ReadToEnd();
                    ps.Commands.Clear();
                    ps.AddScript(psScript);
                    ps.Invoke();
                }
                ps.Commands.Clear();
                // get a list of functions
                var pipeline = ps.Runspace.CreatePipeline();
                
                var cmd1 = new System.Management.Automation.Runspaces.Command("get-childitem");
                cmd1.Parameters.Add("Path", @"function:\Check*");
                pipeline.Commands.Add(cmd1);

                var cmd2 = new System.Management.Automation.Runspaces.Command("where-object");
                var sb = ScriptBlock.Create("$_.Parameters.Count -eq 0");
                cmd2.Parameters.Add("FilterScript", sb);
                pipeline.Commands.Add(cmd2);  

                var funcs = pipeline.Invoke();
                var funcList = new List<string>();
                foreach (var f in funcs)
                {
                    funcList.Add(f.ToString());
                }
                ps.Commands.Clear();
                ApplicationObject.StatusBar.Text = "";
                // show dialog
                var dialog = new CustomDesignRulesCheckerForm();
                dialog.Functions = funcList;
                dialog.Plugin = this;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var iCnt = 0;
                    // run selected functions
                    foreach (var f in dialog.Functions)
                    {
                        ApplicationObject.StatusBar.Progress(true,string.Format("Running rule '{0}'",f),iCnt, dialog.Functions.Count );
                        ps.AddCommand(f);
                        ps.Invoke();
                        iCnt++;
                    }
                    ApplicationObject.StatusBar.Progress(false);
                }
            }
            catch (System.Exception ex)
            {
                AddTaskItem(PriorityType.Error, "ERROR RUNNING DESIGN CHECKS: " + ex.Message);
            }
        }

        //############################

        private void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<DebugRecord> warningStream = (PSDataCollection<DebugRecord>)sender;
            //AddErrorsToVSErrorList(warningStream[e.Index].Message, TaskItemPriority.Normal);
            AddTaskItem(PriorityType.Information, warningStream[e.Index].Message);
        }

        private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<VerboseRecord> warningStream = (PSDataCollection<VerboseRecord>)sender;
            //AddErrorsToVSErrorList(warningStream[e.Index].Message, TaskItemPriority.Normal);
            //AddTaskItem(PriorityType.Information, warningStream[e.Index].Message);
            AddErrorItem(PriorityType.Information, warningStream[e.Index].Message);
        }
        private void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<WarningRecord> warningStream = (PSDataCollection<WarningRecord>)sender;
            //AddErrorsToVSErrorList(warningStream[e.Index].Message, TaskItemPriority.Normal);
            //AddTaskItem(PriorityType.Warning, warningStream[e.Index].Message);
            AddErrorItem(PriorityType.Warning, warningStream[e.Index].Message);
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<ErrorRecord> warningStream = (PSDataCollection<ErrorRecord>)sender;
            //AddErrorsToVSErrorList(warningStream[e.Index].Message, TaskItemPriority.Normal);
            //AddTaskItem(PriorityType.Error, warningStream[e.Index].Exception.Message);
            AddErrorItem(PriorityType.Error, warningStream[e.Index].Exception.Message);
        }

        /*
        private void AddErrorsToVSErrorList( string message, TaskItemPriority priority)
        {
            ErrorList errorList = this.ApplicationObject.ToolWindows.ErrorList;
            Window2 errorWin2 = (Window2)(errorList.Parent);
            
            if (!errorWin2.Visible)
            {
                this.ApplicationObject.ExecuteCommand("View.ErrorList", " ");
            }
            errorWin2.SetFocus();
            

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


            //foreach (Result result in errors)
            //{
                //if (result.Passed) continue;
                ICustomTaskItem item = (ICustomTaskItem)service.CreateTaskItem(TaskItemType.Custom, message);
                item.Category = TaskItemCategory.Misc;
                item.Appearance = TaskItemAppearance.Squiggle;
                //switch (result.Severity)
                //{
                //    case ResultSeverity.Low:
                //        item.Priority = TaskItemPriority.Low;
                //        break;
                //    case ResultSeverity.Normal:
                //        item.Priority = TaskItemPriority.Normal;
                //        break;
                //    case ResultSeverity.High:
                        item.Priority = TaskItemPriority.High;
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException();
                //}
                item.Document = window.ProjectItem.Name;
                item.CustomInfo = this;
            
                service.Add(item);
            //}
        }
        */

        private void AddTaskItem(PriorityType priority, string message)
        {
            //OutputWindowPane objBuildOutputWindowPane = GetBuildOutputWindowPane(); 
            vsTaskIcon icon;
            vsTaskPriority vsPriority;
            switch (priority)
            {
                case PriorityType.Error:
                    icon = vsTaskIcon.vsTaskIconCompile;
                    vsPriority = vsTaskPriority.vsTaskPriorityHigh;
                    break;
                case PriorityType.Warning:
                    icon = vsTaskIcon.vsTaskIconSquiggle;
                    vsPriority = vsTaskPriority.vsTaskPriorityMedium;
                    break;
                default:
                    icon = vsTaskIcon.vsTaskIconComment;
                    vsPriority = vsTaskPriority.vsTaskPriorityLow;
                    break;

            }

            //if (objBuildOutputWindowPane != null) 
            // {
            //    objBuildOutputWindowPane.OutputTaskItemString(message, vsPriority, DESIGN_RULE_CATEGORY, icon,"", 0, message);
            //}

            TaskList tl = (TaskList)((DTE2)this.ApplicationObject).ToolWindows.TaskList;
            tl.TaskItems.Add(DESIGN_RULE_CATEGORY, "", message, vsPriority, icon);
            ////tl.TaskItems.ForceItemsToTaskList();
            
           
        }

        private void ClearTaskList()
        {
            TaskList tl = (TaskList)((DTE2)this.ApplicationObject).ToolWindows.TaskList;

            foreach (TaskItem ti in tl.TaskItems)
            {
                if (ti.Category == DESIGN_RULE_CATEGORY)
                {
                    ti.Delete();
                }
            }

            tl.Parent.Visible = true;
            tl.Parent.Activate();
        }

        public void ClearErrorList()
        {
            //TODO
            ErrorTask objErrorTask;

            try
            {
                if (m_colErrorTasks != null)
                {

                    for (int iCounter = m_colErrorTasks.Count - 1; iCounter >= 0; iCounter--)
                    {
                        objErrorTask = m_colErrorTasks[iCounter];
                        RemoveTask(objErrorTask);
                    }

                }
            }
            catch (Exception objException)
            {
                MessageBox.Show(objException.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveTask(ErrorTask objErrorTask)
        {
            try
            {
                m_objErrorListProvider.SuspendRefresh();
                //RemoveHandler objErrorTask.Navigate, AddressOf ErrorTaskNavigate
                m_colErrorTasks.Remove(objErrorTask);
                m_objErrorListProvider.Tasks.Remove(objErrorTask);
            }
            catch (Exception objException)
            {
                MessageBox.Show(objException.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                m_objErrorListProvider.ResumeRefresh();
            }
        }



        private void AddErrorItem(PriorityType priority, string message)
        {
            OutputWindowPane objBuildOutputWindowPane = GetBuildOutputWindowPane();
            //vsTaskIcon icon;
            //vsTaskPriority vsPriority;
            TaskErrorCategory vsCat;
            switch (priority)
            {
                case PriorityType.Error:
                    //icon = vsTaskIcon.vsTaskIconCompile;
                    //vsPriority = vsTaskPriority.vsTaskPriorityHigh;
                    vsCat = TaskErrorCategory.Error;
                    break;
                case PriorityType.Warning:
                    //icon = vsTaskIcon.vsTaskIconSquiggle;
                    //vsPriority = vsTaskPriority.vsTaskPriorityMedium;
                    vsCat = TaskErrorCategory.Warning;
                    break;
                default:
                    //icon = vsTaskIcon.vsTaskIconComment;
                    //vsPriority = vsTaskPriority.vsTaskPriorityLow;
                    vsCat = TaskErrorCategory.Message;
                    break;

            }

            //if (objBuildOutputWindowPane != null)
            //{
            //    objBuildOutputWindowPane.OutputTaskItemString(message, vsPriority, DESIGN_RULE_CATEGORY, icon, "", 0, message);
            //}

            AddErrorToErrorList(message, vsCat, -1, 1);
        }

        private void AddErrorToErrorList(
            //Project objProject
            //, ProjectItem objProjectItem 
            //, 
            string sErrorText 
            , TaskErrorCategory eTaskErrorCategory 
            , int iLine 
            , int iColumn)
    {

      ErrorTask objErrorTask;
      //Microsoft.VisualStudio.Shell.Interop.IVsSolution objIVsSolution;
      //IVsHierarchy objVsHierarchy = null;

      try
      {
         //objIVsSolution = DirectCast(GetService(GetType(IVsSolution)), IVsSolution)

         //ErrorHandler.ThrowOnFailure(objIVsSolution.GetProjectOfUniqueName(objProject.UniqueName, objVsHierarchy))

         objErrorTask = new Microsoft.VisualStudio.Shell.ErrorTask();
         objErrorTask.ErrorCategory = eTaskErrorCategory;
         //objErrorTask.HierarchyItem = null;// objVsHierarchy;
         //objErrorTask.Document = objProjectItem.FileNames[0];
         //' VS uses indexes starting at 0 while the automation model uses indexes starting at 1
         objErrorTask.Line = iLine - 1;
         objErrorTask.Column = iColumn;

         objErrorTask.Text = sErrorText;
         //AddHandler objErrorTask.Navigate, AddressOf ErrorTaskNavigate

         m_colErrorTasks.Add(objErrorTask);

         m_objErrorListProvider.Tasks.Add(objErrorTask);
      }
      catch (Exception objException)
      {
         MessageBox.Show(objException.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      }

        private void InitializeErrorProvider()
        {
            m_colErrorTasks = new List<ErrorTask>();
            m_objErrorListProvider = new Microsoft.VisualStudio.Shell.ErrorListProvider(this);
            m_objErrorListProvider.ProviderName = "BIDS Helper SSAS Design Rules Error Provider";
            m_objErrorListProvider.ProviderGuid = new Guid("570A92B8-49B7-4FD2-8A33-14245AB7E829");
            m_objErrorListProvider.Show();
        }

        public enum PriorityType
        {
            Error,
            Warning,
            Information
        }


        object IServiceProvider.GetService(Type serviceType)
        {
            Object objService = null;

            try
            {
                objService = Microsoft.VisualStudio.Shell.Package.GetGlobalService(serviceType);
            }
            catch (Exception objException)
            {
                MessageBox.Show(objException.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return objService;
        }
    }
}