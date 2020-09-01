extern alias sharedDataWarehouseInterfaces;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.DataWarehouse.Design;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper.SSIS.PerformanceVisualization
{
    public class PerformanceTab : UserControl
    {
        private Container components;
        private EditorWindow parentWin;
        private EditorWindow.EditorView parentView;
        private ProjectItem projectItem;
        private string packagePassword;
        private IOutputWindow standardOutputWindow;
        private Microsoft.DataWarehouse.Project.DataWarehouseProjectManager projectManager;
        private bool use64Bit;
        private string dtexecPath;
        private Microsoft.SqlServer.Dts.Runtime.Application ssisApp = SSIS.PackageHelper.Application; //sets the proper TargetServerVersion
        private System.Diagnostics.Process process = null;
        private DtsPerformanceLogEventParser eventParser = null;
        private DtsTextLogFileLoader logFileLoader = null;
        private string modifiedPackagePath;
        private Package modifiedPackage;
        private string logFilePath;
        private Timer timer1;
        private DtsStatisticsTrendGrid dtsStatisticsTrendGrid1;
        private DtsGanttGrid ganttGrid;
        private DtsStatisticsGrid dtsStatisticsGrid1;
        private DtsStatisticsTrendGrid dtsPipelineBreakdownGrid;
        private BindingSource iDtsGanttGridRowDataBindingSource;
        private bool ExecutionCancelled = false;
        private object syncRoot = new object();
        private TableLayoutPanel tableLayoutPanel1;
        private Label lblStatus;
        private long lTickerCounter;
        private BackgroundWorker backgroundWorkerPipelineBreakdown;
        private string lastDataFlowTaskID;

        public ToolBarButton StartButton;
        public ToolBarButton StopButton;
        public ToolBarButton ViewDropDownButton;
        public ToolBarButton CopyButton;
        private MenuItem menuGantt;
        private MenuItem menuTrend;
        private MenuItem menuGrid;
        private MenuItem menuPipelineBreakdown;

        //pipeline component performance breakdown
        private DtsPipelineTestDirector pipelineBreakdownTestDirector = null;

        // Get the registery path, based on the deployment target version.
        private static string DtsPathRegistryPath
        {
            get
            {
                switch (PackageHelper.TargetServerVersion)
                {
                    case SsisTargetServerVersion.SQLServer2012:
                        return @"SOFTWARE\Microsoft\Microsoft SQL Server\110\SSIS\Setup\DTSPath";
                    case SsisTargetServerVersion.SQLServer2014:
                        return @"SOFTWARE\Microsoft\Microsoft SQL Server\120\SSIS\Setup\DTSPath";
                    case SsisTargetServerVersion.SQLServer2016:
                        return @"SOFTWARE\Microsoft\Microsoft SQL Server\130\SSIS\Setup\DTSPath";
                    case SsisTargetServerVersion.SQLServer2017:
                        return @"SOFTWARE\Microsoft\Microsoft SQL Server\140\SSIS\Setup\DTSPath";
                    case SsisTargetServerVersion.SQLServer2019:
                        return @"SOFTWARE\Microsoft\Microsoft SQL Server\150\SSIS\Setup\DTSPath";
                    default:
                        throw new Exception("Unknown deployment version, DTSPATH_REGISTRY_PATH cannot be determined.");
                }
            }
        }

        private static string LogProviderCreationName
        {
            get
            {
                // It appears that when using the OneDesigner API to edit a package in downgraded SSIS2012 mode, 
                // you tell it to use a SSIS2014 (DTS.LogProviderTextFile.4) log provider and when it persists the 
                // package to a .dtsx file on disk it internally and automatically downgrades that to an 
                // SSIS2012 (DTS.LogProviderTextFile.3) log provider
                return string.Format("DTS.LogProviderTextFile.{0}", SSISHelpers.CreationNameIndex);
            }
        }        

        #region Layout
        public PerformanceTab()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.components = new Container();

            this.timer1 = new Timer(this.components);

            this.dtsStatisticsGrid1 = new DtsStatisticsGrid(this.components);
            this.dtsStatisticsTrendGrid1 = new DtsStatisticsTrendGrid(this.components);
            this.ganttGrid = new DtsGanttGrid(this.components);
            this.iDtsGanttGridRowDataBindingSource = new BindingSource(this.components);
            this.dtsPipelineBreakdownGrid = new DtsStatisticsTrendGrid(this.components);

            this.dtsStatisticsGrid1.ParentPerformanceTab = this;
            this.dtsStatisticsTrendGrid1.ParentPerformanceTab = this;
            this.ganttGrid.ParentPerformanceTab = this;
            this.dtsPipelineBreakdownGrid.ParentPerformanceTab = this;

            ((System.ComponentModel.ISupportInitialize)(this.dtsStatisticsTrendGrid1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ganttGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iDtsGanttGridRowDataBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtsStatisticsGrid1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtsPipelineBreakdownGrid)).BeginInit();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.lblStatus = new Label();


            this.SuspendLayout();

            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 13);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(451, 297);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.Dock = DockStyle.Fill;
            this.tableLayoutPanel1.Controls.Add(this.ganttGrid, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dtsStatisticsGrid1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dtsStatisticsTrendGrid1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dtsPipelineBreakdownGrid, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblStatus, 0, 1);
            this.tableLayoutPanel1.ResumeLayout(false);

            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(400, 14);
            this.lblStatus.Dock = DockStyle.Fill;

            // 
            // ganttGrid
            // 
            this.ganttGrid.BackgroundColor = System.Drawing.Color.White;
            this.ganttGrid.DataSource = this.iDtsGanttGridRowDataBindingSource;
            //this.ganttGrid.Location = new System.Drawing.Point(13, 49);
            this.ganttGrid.Dock = DockStyle.Fill;
            this.ganttGrid.Name = "ganttGrid";
            this.ganttGrid.Size = new System.Drawing.Size(1216, 800);

            // 
            // dtsStatisticsGrid1
            // 
            this.dtsStatisticsGrid1.BackgroundColor = System.Drawing.Color.White;
            this.dtsStatisticsGrid1.Dock = DockStyle.Fill;
            this.dtsStatisticsGrid1.Visible = false;
            this.dtsStatisticsGrid1.Name = "dtsStatisticsGrid1";
            this.dtsStatisticsGrid1.DataSource = this.iDtsGanttGridRowDataBindingSource;

            // 
            // dtsStatisticsTrendGrid1
            // 
            this.dtsStatisticsTrendGrid1.BackgroundColor = System.Drawing.Color.White;
            this.dtsStatisticsTrendGrid1.Dock = DockStyle.Fill;
            this.dtsStatisticsTrendGrid1.Visible = false;
            this.dtsStatisticsTrendGrid1.Name = "dtsStatisticsGrid1";
            this.dtsStatisticsTrendGrid1.DataSource = this.iDtsGanttGridRowDataBindingSource;

            // 
            // dtsBreakdownGrid
            // 
            this.dtsPipelineBreakdownGrid.BackgroundColor = System.Drawing.Color.White;
            this.dtsPipelineBreakdownGrid.Dock = DockStyle.Fill;
            this.dtsPipelineBreakdownGrid.Visible = false;
            this.dtsPipelineBreakdownGrid.Name = "dtsBreakdownGrid";

            // 
            // iDtsGanttGridRowDataBindingSource
            // 
            this.iDtsGanttGridRowDataBindingSource.DataSource = typeof(IDtsGridRowData);

            //
            // timer1
            //
            this.timer1.Interval = 1000; //1 second
            this.timer1.Tick += new EventHandler(this.timer1_Tick);


            base.Name = "PerformanceTab";
            base.Size = new Size(0x248, 0x210);
            this.Controls.Add(this.tableLayoutPanel1);

            ((System.ComponentModel.ISupportInitialize)(this.dtsStatisticsTrendGrid1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ganttGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iDtsGanttGridRowDataBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtsStatisticsGrid1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtsPipelineBreakdownGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public void LayoutToolBar(Microsoft.DataWarehouse.Controls.VsStyleToolBar pageViewToolBar)
        {
            this.StartButton = new ToolBarButton();
            this.StartButton.Style = ToolBarButtonStyle.PushButton;
            pageViewToolBar.ImageList.Images.Add(BIDSHelper.Resources.Common.Run);
            this.StartButton.ImageIndex = pageViewToolBar.ImageList.Images.Count - 1;
            this.StartButton.ToolTipText = "Execute package";
            pageViewToolBar.Buttons.Add(this.StartButton);

            this.StopButton = new ToolBarButton();
            this.StopButton.Style = ToolBarButtonStyle.PushButton;
            pageViewToolBar.ImageList.Images.Add(BIDSHelper.Resources.Common.End);
            this.StopButton.ImageIndex = pageViewToolBar.ImageList.Images.Count - 1;
            this.StopButton.ToolTipText = "Stop execution of package";
            pageViewToolBar.Buttons.Add(this.StopButton);

            ToolBarButton separator = new ToolBarButton();
            separator.Style = ToolBarButtonStyle.Separator;
            pageViewToolBar.Buttons.Add(separator);

            this.ViewDropDownButton = new ToolBarButton();
            this.ViewDropDownButton.Style = ToolBarButtonStyle.DropDownButton;
            pageViewToolBar.ImageList.Images.Add(BIDSHelper.Resources.Common.TreeViewTab);
            this.ViewDropDownButton.ImageIndex = pageViewToolBar.ImageList.Images.Count - 1;
            this.ViewDropDownButton.ToolTipText = "Switch to alternate view";
            this.ViewDropDownButton.DropDownMenu = this.CreateViewDropDownMenu();
            pageViewToolBar.Buttons.Add(this.ViewDropDownButton);

            this.CopyButton = new ToolBarButton();
            this.CopyButton.Style = ToolBarButtonStyle.PushButton;
            pageViewToolBar.ImageList.Images.Add(BIDSHelper.Resources.Common.Copy);
            this.CopyButton.ImageIndex = pageViewToolBar.ImageList.Images.Count - 1;
            this.CopyButton.ToolTipText = "Copy the grid to clipboard";
            this.CopyButton.Enabled = false;
            pageViewToolBar.Buttons.Add(this.CopyButton);

            pageViewToolBar.ButtonClick += new ToolBarButtonClickEventHandler(this.ToolbarButtonClicked);
        }

        public ContextMenu CreateViewDropDownMenu()
        {
            this.menuGantt = new MenuItem("Gantt Chart");
            this.menuGantt.Checked = true;
            this.menuGantt.Visible = false;
            this.menuGantt.Click += new EventHandler(this.SwitchToGanttGridMenuClicked);
            this.menuGrid = new MenuItem("Statistics Grid");
            this.menuGrid.Visible = false;
            this.menuGrid.Click += new EventHandler(this.SwitchToStatisticsGridMenuClicked);
            this.menuTrend = new MenuItem("Statistics Trend");
            this.menuTrend.Visible = false;
            this.menuTrend.Click += new EventHandler(this.SwitchToStatisticsTrendGridMenuClicked);
            this.menuPipelineBreakdown = new MenuItem("Pipeline Component Performance Breakdown");
            this.menuPipelineBreakdown.Visible = false;
            this.menuPipelineBreakdown.Click += new EventHandler(this.SwitchToPipelineBreakdownGridMenuClicked);
            return new ContextMenu(new MenuItem[] { this.menuGantt, this.menuGrid, this.menuTrend, this.menuPipelineBreakdown });
        }
        #endregion

        public void Init(EditorWindow parentWin, EditorWindow.EditorView view, ProjectItem pi, string DataFlowGUID)
        {
            this.parentWin = parentWin;
            this.parentView = view;
            this.projectItem = pi;

            //capture the project manager object
            object settings = pi.ContainingProject.GetIConfigurationSettings();
            this.projectManager = (Microsoft.DataWarehouse.Project.DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);

            //capture the output window object
            IOutputWindowFactory service = ((System.IServiceProvider)pi.ContainingProject).GetService(typeof(IOutputWindowFactory)) as IOutputWindowFactory;
            this.standardOutputWindow = service.GetStandardOutputWindow(StandardOutputWindow.Debug);

            //run the package immediately
            if (DataFlowGUID == null)
                ExecutePackage();
            else
                BreakdownPipelinePerformance(DataFlowGUID);
        }

        public EditorWindow.EditorView ParentEditorView
        {
            get { return parentView; }
        }

        public bool IsExecuting
        {
            get { return (process != null || pipelineBreakdownTestDirector != null); }
        }

        /// <summary>
        /// Returns true if the currently running package is for package performance visualization
        /// Returns false if the currently running package is for pipeline component performance breakdown
        /// </summary>
        public bool IsPackagePerformanceVisualization
        {
            get { return (this.pipelineBreakdownTestDirector == null); }
        }

        public void ExecutePackage()
        {
            try
            {
                if (this.projectItem.DTE.Mode == vsIDEMode.vsIDEModeDebug)
                {
                    MessageBox.Show("Please stop the debugger first.");
                    return;
                }

                lTickerCounter = 0;
                ExecutionCancelled = false;
                this.lblStatus.Text = "Status: Preparing Temporary Package";

                RefreshProjectAndPackageProperties();

                this.modifiedPackage = ssisApp.LoadPackage(projectItem.get_FileNames(0), null);
                this.modifiedPackagePath = System.IO.Path.GetTempFileName();
                this.logFilePath = System.IO.Path.GetTempFileName();

                SetupCustomLogging(this.modifiedPackage, this.logFilePath);
                ssisApp.SaveToXml(this.modifiedPackagePath, this.modifiedPackage, null);

                eventParser = new DtsPerformanceLogEventParser(this.modifiedPackage);

                //setup Process object to call the dtexec EXE
                process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory(); //inherit the working directory from the current BIDS process (so that relative dtsConfig paths will work)
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = "\"" + this.dtexecPath + "\"";
                process.StartInfo.Arguments = "/Rep N /F \"" + this.modifiedPackagePath + "\"";
                if (!string.IsNullOrEmpty(this.packagePassword))
                {
                    process.StartInfo.Arguments += " /Decrypt \"" + this.packagePassword + "\"";
                }
                process.Start();
                timer1.Enabled = true;
                timer1.Start();

                logFileLoader = new DtsTextLogFileLoader(this.logFilePath);

                //TODO: capture perfmon: SQL Server:SSISPipeline:Buffers Spooled

                this.dtsStatisticsTrendGrid1.AddNewColumnOnNextDataBinding = true;

                this.iDtsGanttGridRowDataBindingSource.DataSource = eventParser.GetAllDtsGanttGridRowDatas();
                this.ganttGrid.Refresh();

                this.StopButton.Enabled = true;
                this.StartButton.Enabled = false;
                this.lblStatus.Text = "Status: Executing";

                this.menuGantt.Visible = true;
                this.menuGrid.Visible = true;
                this.menuTrend.Visible = true;

                if (this.dtsPipelineBreakdownGrid.Visible)
                    SwitchToGanttGridMenuClicked(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void BreakdownPipelinePerformance(string DataFlowTaskID)
        {
            try
            {
                if (this.projectItem.DTE.Mode == vsIDEMode.vsIDEModeDebug)
                {
                    MessageBox.Show("Please stop the debugger first.");
                    return;
                }

                if (this.IsExecuting)
                {
                    MessageBox.Show("You may not execute the package until the previous execution of the package completes.");
                    return;
                }

                lTickerCounter = 0;
                ExecutionCancelled = false;

                if (this.lastDataFlowTaskID != DataFlowTaskID)
                {
                    this.dtsPipelineBreakdownGrid.ClearHistory(); //TODO: create multiple instances of the breakdown grid, one for each data flow?
                }

                this.lastDataFlowTaskID = DataFlowTaskID; //TODO: add dropdown
                this.lblStatus.Text = "Status: Preparing Testing Iterations";

                RefreshProjectAndPackageProperties();

                pipelineBreakdownTestDirector = new DtsPipelineTestDirector(projectItem.get_FileNames(0), DataFlowTaskID);
                pipelineBreakdownTestDirector.DtexecPath = this.dtexecPath;
                pipelineBreakdownTestDirector.PackagePassword = this.packagePassword;
                pipelineBreakdownTestDirector.PerformanceTab = this;

                backgroundWorkerPipelineBreakdown = new BackgroundWorker();
                backgroundWorkerPipelineBreakdown.DoWork += new DoWorkEventHandler(backgroundWorkerPipelineBreakdown_DoWork);
                backgroundWorkerPipelineBreakdown.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerPipelineBreakdown_RunWorkerCompleted);
                backgroundWorkerPipelineBreakdown.RunWorkerAsync();

                this.dtsPipelineBreakdownGrid.AddNewColumnOnNextDataBinding = true;

                timer1.Enabled = true;
                timer1.Start();

                this.StopButton.Enabled = true;
                this.StartButton.Enabled = false;
                this.lblStatus.Text = "Status: Executing Testing Iterations";

                SwitchToPipelineBreakdownGridMenuClicked(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        void backgroundWorkerPipelineBreakdown_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                UpdatePipelineBreakdownVisualization();
                pipelineBreakdownTestDirector = null;
            }
            catch { }
        }

        void backgroundWorkerPipelineBreakdown_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                pipelineBreakdownTestDirector.ExecuteTests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void RefreshProjectAndPackageProperties()
        {
            //save package to disk
            if (projectItem.Document != null && !projectItem.Document.Saved)
                projectItem.Save("");

            //get package password
            Microsoft.DataTransformationServices.Design.DtsBasePackageDesigner rootDesigner = typeof(EditorWindow).InvokeMember("designer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.GetField, null, parentWin, null) as Microsoft.DataTransformationServices.Design.DtsBasePackageDesigner;
            if (rootDesigner == null) throw new Exception("Can't find SSIS Package designer control.");
            this.packagePassword = rootDesigner.GetPackagePassword();
            ssisApp.PackagePassword = this.packagePassword;

            //get setting that says whether to use 64-bit dtexec
            Microsoft.DataTransformationServices.Project.DataTransformationsProjectConfigurationOptions options = (Microsoft.DataTransformationServices.Project.DataTransformationsProjectConfigurationOptions)projectManager.ConfigurationManager.CurrentConfiguration.Options;
            this.use64Bit = options.Run64BitRuntime;

            // Refreshes cached target version which is needed in GetPathToDtsExecutable below
            PackageHelper.SetTargetServerVersion(projectItem.ContainingProject);

            //get a new copy of the SSIS app because we just set the target server version
            ssisApp = SSIS.PackageHelper.Application;

            //get path to dtexec
            this.dtexecPath = GetPathToDtsExecutable("dtexec.exe", this.use64Bit);
            if (this.dtexecPath == null && this.use64Bit)
                this.dtexecPath = GetPathToDtsExecutable("dtexec.exe", false);
            if (this.dtexecPath == null)
            {
                throw new Exception("Can't find path to dtexec in registry! Please make sure you have the SSIS service installed from the " + PackageHelper.TargetServerVersion.ToString() + " install media");
            }
        }

        internal static void SetupCustomLogging(Package pkg, string sLogFilePath)
        {
            //add connection manager for BIDS Helper custom logging
            ConnectionManager cm = pkg.Connections.Add("FILE");
            cm.Name = "BidsHelperPerformanceLoggingConnectionMgr";
            cm.ConnectionString = sLogFilePath;

            //remove existing log providers because we don't want to send all our log stuff to their log provider
            while (pkg.LogProviders.Count > 0)
                pkg.LogProviders.Remove(0);

            //remove existing selected log providers
            while (pkg.LoggingOptions.SelectedLogProviders != null && pkg.LoggingOptions.SelectedLogProviders.Count > 0)
                pkg.LoggingOptions.SelectedLogProviders.Remove(0);

            RecurseTasksAndSetupLogging(pkg);

            //add BIDS Helper custom logging settings
            LogProvider log = pkg.LogProviders.Add(LogProviderCreationName);
            log.ConfigString = cm.Name;
            log.Name = "BidsHelperPerformanceLogging";
            log.Description = log.Name;
            pkg.LoggingOptions.SelectedLogProviders.Add(log);
            pkg.LoggingMode = DTSLoggingMode.Enabled;

            //specify events to log
            LoggingOptions LoggingOptions = pkg.LoggingOptions;
            LoggingOptions.EventFilterKind = DTSEventFilterKind.Inclusion;
            LoggingOptions.EventFilter = System.Enum.GetNames(typeof(BidsHelperCapturedDtsLogEvent));

            //specify columns to log
            DTSEventColumnFilter colFilter = new DTSEventColumnFilter();
            colFilter.SourceID = true;
            colFilter.SourceName = true;
            colFilter.MessageText = true;
            foreach (string sEvent in LoggingOptions.EventFilter)
            {
                LoggingOptions.SetColumnFilter(sEvent, colFilter);
            }
        }

        private static void RecurseTasksAndSetupLogging(IDTSSequence container)
        {
            foreach (Executable exe in container.Executables)
            {
                if (exe is DtsContainer)
                {
                    DtsContainer child = (DtsContainer)exe;
                    child.LoggingMode = DTSLoggingMode.UseParentSetting;

                    //remove existing selected log providers
                    while (child.LoggingOptions.SelectedLogProviders != null && child.LoggingOptions.SelectedLogProviders.Count > 0)
                        child.LoggingOptions.SelectedLogProviders.Remove(0);
                }
                if (exe is IDTSSequence)
                {
                    RecurseTasksAndSetupLogging((IDTSSequence)exe);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (IsPackagePerformanceVisualization)
                    UpdatePackagePerformanceVisualization();
                else
                    UpdatePipelineBreakdownVisualization();
            }
            catch { }
        }

        private void UpdatePackagePerformanceVisualization()
        {
            try
            {
                lock (syncRoot)
                {
                    if (process == null) return;

                    lTickerCounter++;

                    bool bProcessExited = process.HasExited; //capture the status of the process at this point so we can ensure to execute the logic below as a unit rather than the process exiting halfway through the logic below and leaving things in an inconsistent state
                    if (bProcessExited)
                    {
                        string sStatus = "Finished";
                        if (this.ExecutionCancelled)
                            sStatus = "Cancelled";

                        TimeSpan ts = new TimeSpan();
                        ts = ts.Add(DateTime.Now.Subtract(process.StartTime));
                        this.lblStatus.Text = "Status: " + sStatus + "            Time: " + ts.ToString().Substring(0, 8);
                    }
                    else
                    {
                        TimeSpan ts = new TimeSpan();
                        ts = ts.Add(DateTime.Now.Subtract(process.StartTime));
                        this.lblStatus.Text = "Status: Executing            Time: " + ts.ToString().Substring(0, 8);

                        //if the package is still running, then only update the grid every other call to this function
                        if (lTickerCounter % 2 == 1) return;
                    }

                    if (bProcessExited)
                    {
                        timer1.Enabled = false;
                        timer1.Stop();
                        StopButton.Enabled = false;
                        System.Threading.Thread.Sleep(1000); //pause just in case we need another second for the log events to quit flowing
                    }

                    DtsLogEvent[] events = logFileLoader.GetEvents(bProcessExited);
                    foreach (DtsLogEvent ee in events)
                    {
                        eventParser.LoadEvent(ee);
                        if (ee.Event == BidsHelperCapturedDtsLogEvent.OnError)
                        {
                            standardOutputWindow.ReportCustomError(OutputWindowErrorSeverity.Error, ee.Message, null, null, this.projectItem.Name);
                        }
                    }

                    //TODO: future: only refresh when the Performance tab is visible? only if there are performance problems
                    this.ganttGrid.SuspendLayout();
                    this.iDtsGanttGridRowDataBindingSource.DataSource = eventParser.GetAllDtsGanttGridRowDatas();
                    this.ganttGrid.Refresh();
                    this.ganttGrid.ResumeLayout();

                    if (bProcessExited)
                    {
                        System.IO.File.Delete(this.modifiedPackagePath);

                        for (int i = 0; i < 20; i++)
                        {
                            try
                            {
                                System.IO.File.Delete(this.logFilePath);
                                break;
                            }
                            catch
                            {
                                //problem deleting... wait half a second then try again
                                System.Threading.Thread.Sleep(500);
                            }
                        }
                        if (System.IO.File.Exists(this.logFilePath))
                        {
                            MessageBox.Show("Unable to delete log file because another process was using it:\r\n" + this.logFilePath);
                        }

                        timer1.Enabled = false;
                        timer1.Stop();

                        process = null;
                        this.StartButton.Enabled = true;
                        this.StopButton.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                timer1.Stop();
            }
        }

        private void UpdatePipelineBreakdownVisualization()
        {
            try
            {
                lock (syncRoot)
                {
                    if (pipelineBreakdownTestDirector == null) return;
                    bool bFailed = pipelineBreakdownTestDirector.Failed;

                    lTickerCounter++;

                    TimeSpan ts = new TimeSpan();
                    ts = ts.Add(DateTime.Now.Subtract(pipelineBreakdownTestDirector.StartTime));
                    this.lblStatus.Text = "Status: " + pipelineBreakdownTestDirector.Status + "            Time: " + ts.ToString().Substring(0, 8);

                    //if the package is still running, then only update the grid every other call to this function
                    if (lTickerCounter % 2 == 1 && backgroundWorkerPipelineBreakdown.IsBusy) return;

                    if (!backgroundWorkerPipelineBreakdown.IsBusy || bFailed)
                    {
                        timer1.Enabled = false;
                        timer1.Stop();
                        StopButton.Enabled = false;
                        System.Threading.Thread.Sleep(1000); //pause just in case we need another second for the log events to quit flowing
                    }

                    //TODO: future: only refresh when the Performance tab is visible? only if there are performance problems
                    BindingSource binding = new BindingSource(this.components);
                    binding.DataSource = pipelineBreakdownTestDirector.GetTestsToDisplay();
                    this.dtsPipelineBreakdownGrid.DataSource = binding;

                    if (!backgroundWorkerPipelineBreakdown.IsBusy || bFailed)
                    {
                        timer1.Enabled = false;
                        timer1.Stop();

                        this.pipelineBreakdownTestDirector = null;
                        this.StartButton.Enabled = true;
                        this.StopButton.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                timer1.Stop();
            }
        }

        public void ToolbarButtonClicked(object sender, ToolBarButtonClickEventArgs e)
        {
            if (e.Button == this.StopButton)
            {
                CancelExecution();
            }
            else if (e.Button == this.ViewDropDownButton)
            {
                if (this.ganttGrid.Visible)
                    this.SwitchToStatisticsGridMenuClicked(null, null);
                else if (this.dtsStatisticsGrid1.Visible)
                    this.SwitchToStatisticsTrendGridMenuClicked(null, null);
                else if (this.menuPipelineBreakdown.Visible && !this.dtsPipelineBreakdownGrid.Visible)
                    this.SwitchToPipelineBreakdownGridMenuClicked(null, null);
                else if (this.menuGantt.Visible)
                    this.SwitchToGanttGridMenuClicked(null, null);
            }
            else if (e.Button == this.StartButton)
            {
                if (this.dtsPipelineBreakdownGrid.Visible)
                    this.BreakdownPipelinePerformance(lastDataFlowTaskID);
                else
                    this.ExecutePackage();
            }
            else if (e.Button == this.CopyButton)
            {
                if (this.dtsStatisticsGrid1.Visible)
                {
                    this.dtsStatisticsGrid1.SelectAll();
                    Clipboard.SetDataObject(this.dtsStatisticsGrid1.GetClipboardContent());
                }
                else if (this.dtsStatisticsTrendGrid1.Visible)
                {
                    this.dtsStatisticsTrendGrid1.SelectAll();
                    Clipboard.SetDataObject(this.dtsStatisticsTrendGrid1.GetClipboardContent());
                }
                else if (this.dtsPipelineBreakdownGrid.Visible)
                {
                    this.dtsPipelineBreakdownGrid.SelectAll();
                    Clipboard.SetDataObject(this.dtsPipelineBreakdownGrid.GetClipboardContent());
                }
            }
        }

        private void SwitchToGanttGridMenuClicked(object sender, EventArgs e)
        {
            this.ganttGrid.Visible = true;
            this.dtsStatisticsTrendGrid1.Visible = false;
            this.dtsStatisticsGrid1.Visible = false;
            this.dtsPipelineBreakdownGrid.Visible = false;
            this.menuGantt.Checked = true;
            this.menuTrend.Checked = false;
            this.menuGrid.Checked = false;
            this.menuPipelineBreakdown.Checked = false;
            this.CopyButton.Enabled = false;
        }

        private void SwitchToStatisticsGridMenuClicked(object sender, EventArgs e)
        {
            this.ganttGrid.Visible = false;
            this.dtsStatisticsTrendGrid1.Visible = false;
            this.dtsStatisticsGrid1.Visible = true;
            this.dtsPipelineBreakdownGrid.Visible = false;
            this.menuGantt.Checked = false;
            this.menuTrend.Checked = false;
            this.menuGrid.Checked = true;
            this.menuPipelineBreakdown.Checked = false;
            this.CopyButton.Enabled = true;
        }

        private void SwitchToStatisticsTrendGridMenuClicked(object sender, EventArgs e)
        {
            this.ganttGrid.Visible = false;
            this.dtsStatisticsTrendGrid1.Visible = true;
            this.dtsStatisticsGrid1.Visible = false;
            this.dtsPipelineBreakdownGrid.Visible = false;
            this.menuGantt.Checked = false;
            this.menuTrend.Checked = true;
            this.menuGrid.Checked = false;
            this.menuPipelineBreakdown.Checked = false;
            this.CopyButton.Enabled = true;
        }

        public void SwitchToPipelineBreakdownGridMenuClicked(object sender, EventArgs e)
        {
            this.ganttGrid.Visible = false;
            this.dtsStatisticsTrendGrid1.Visible = false;
            this.dtsStatisticsGrid1.Visible = false;
            this.dtsPipelineBreakdownGrid.Visible = true;
            this.menuGantt.Checked = false;
            this.menuTrend.Checked = false;
            this.menuGrid.Checked = false;
            this.menuPipelineBreakdown.Visible = true;
            this.menuPipelineBreakdown.Checked = true;
            this.CopyButton.Enabled = true;
        }

        public void CancelExecution()
        {
            try
            {
                ExecutionCancelled = true;
                if (this.IsPackagePerformanceVisualization)
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }
                else
                {
                    if (pipelineBreakdownTestDirector != null)
                        pipelineBreakdownTestDirector.CancelExecution();
                }
                timer1_Tick(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem stopping execution of package:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public string PackageName
        {
            get
            {
                if (this.projectItem != null)
                    return this.projectItem.Name;
                else
                    return "<Unknown Package>";
            }
        }

        #region Handle Turning Off and On Microsoft OnActiveViewChanged code
        protected override void OnGotFocus(EventArgs e)
        {
            try
            {
                if (this.parentWin != null)
                {
                    bool bSetNewSelection = (bool)parentWin.GetType().InvokeMember("bSetNewSelection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.Instance, null, parentWin, null);
                    if (!bSetNewSelection)
                    {
                        //we've turned off that flag temporarily to skip some Microsoft code that causes an exception message box
                        //fire up a background thread to flip this flag back in a few seconds (after the Microsoft code has been skipped)
                        System.ComponentModel.BackgroundWorker backgroundThreadSetNewSelection = new System.ComponentModel.BackgroundWorker();
                        backgroundThreadSetNewSelection.DoWork += new DoWorkEventHandler(backgroundThreadSetNewSelection_DoWork);
                        backgroundThreadSetNewSelection.RunWorkerAsync();
                    }
                }
            }
            catch { }
            base.OnGotFocus(e);
        }

        void backgroundThreadSetNewSelection_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                System.Threading.Thread.Sleep(2000); //wait 2 seconds for the window to finish opening then flip the flag back
                parentWin.GetType().InvokeMember("bSetNewSelection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.Instance, null, parentWin, new object[] { true });
            }
            catch { }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelExecution();
                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Path to dtexec
        public static string GetPathToDtsExecutable(string executable, bool is64bit)
        {
            int sam = 1;
            if (is64bit)
            {
                sam |= 0x100;
            }
            string str = null;
            IntPtr zero = IntPtr.Zero;
            IntPtr HKEY_LOCAL_MACHINE = (IntPtr)(-2147483646);

            if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, DtsPathRegistryPath, 0, sam, out zero) == 0)
            {
                StringBuilder lpValue = new StringBuilder(260);
                int lpcbValue = lpValue.Capacity * Marshal.SizeOf(typeof(char));
                if (RegQueryValue(zero, "", lpValue, ref lpcbValue) == 0)
                {
                    str = lpValue.ToString();
                }
                RegCloseKey(zero);
            }
            if (str != null)
            {
                string path = Path.Combine(Path.Combine(str, "binn"), executable);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }

        [DllImport("advapi32", CharSet = CharSet.Unicode)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int sam, out IntPtr phkResult);

        [DllImport("advapi32")]
        private static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32", CharSet = CharSet.Unicode)]
        public static extern int RegQueryValue(IntPtr hKey, string lpSubKey, StringBuilder lpValue, ref int lpcbValue);
        #endregion
    }
}