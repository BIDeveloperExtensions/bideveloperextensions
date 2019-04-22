#if SQL2019
extern alias asAlias;
using asAlias.Microsoft.DataWarehouse;
using asAlias.Microsoft.DataWarehouse.Design;
using asAlias::Microsoft.AnalysisServices.Design;
using asAlias::Microsoft.DataWarehouse.ComponentModel;
using asAlias::Microsoft.DataWarehouse.Controls;
#else
using Microsoft.DataWarehouse;
using Microsoft.DataWarehouse.Design;
using Microsoft.AnalysisServices.Design;
using Microsoft.DataWarehouse.ComponentModel;
using Microsoft.DataWarehouse.Controls;
#endif

using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.ComponentModel.Design;
//using Microsoft.DataWarehouse.Design;
//using Microsoft.DataWarehouse.Controls;
using System;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class EstimatedCountsPlugin : BIDSHelperWindowActivatedPluginBase
    {
        //private WindowEvents windowEvents;
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.Dictionary<string, EditorWindow> windowHandlesFixedPartitionsView = new System.Collections.Generic.Dictionary<string, EditorWindow>();

        private const string SET_ESTIMATED_COUNTS_ICON_KEY = "EstimatedCounts";
        private const string EDIT_AGGREGATIONS_ICON_KEY = "EditAggregations";
        private const string DEPLOY_AGGREGATION_DESIGNS_ICON_KEY = "DeployAggDesigns";
        private const string STOP_ICON_KEY = "Stop";
        private const string EDIT_AGGREGATIONS_BUTTON_SUFFIX = "EditAggregationsButton";
        private const string SET_ESTIMATED_COUNTS_BUTTON = "SetEstimatedCountsButton";
        private const string DEPLOY_AGGREGATION_DESIGNS_BUTTON = "DeployAggDesignsButton";

        private bool bCancelEstimatedCountsClicked = false;

        public EstimatedCountsPlugin(BIDSHelperPackage package)
            : base(package)
        {
        }

        public override bool ShouldHookWindowCreated { get { return true;} }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus == null) return;
                IDesignerHost designer = GotFocus.Object as IDesignerHost;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if (!(pi.Object is Cube)) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(IComponentNavigator));
                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);

                IntPtr ptr = win.Handle;
                string sHandle = ptr.ToInt64().ToString();

                if (!windowHandlesFixedPartitionsView.ContainsKey(sHandle))
                {
                    windowHandlesFixedPartitionsView.Add(sHandle,win);
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                }

                //if (win.SelectedView.Caption == "Partitions")
                if (win.SelectedView.MenuItemCommandID.ID == (int)BIDSViewMenuItemCommandID.Partitions)
                {
                    if (!toolbar.Buttons.ContainsKey(this.FullName + "." + SET_ESTIMATED_COUNTS_BUTTON))
                    {
                        ToolBarButton separator = new ToolBarButton();
                        separator.Style = ToolBarButtonStyle.Separator;
                        toolbar.Buttons.Add(separator);

                        toolbar.ImageList.Images.Add(SET_ESTIMATED_COUNTS_ICON_KEY, BIDSHelper.Resources.Common.EstimatedCounts);
                        ToolBarButton oSetAllEstimatedCountsButton = new ToolBarButton();
                        oSetAllEstimatedCountsButton.ToolTipText = "Update All Estimated Counts (BIDS Helper)";
                        oSetAllEstimatedCountsButton.Name = this.FullName + "." + SET_ESTIMATED_COUNTS_BUTTON;
                        oSetAllEstimatedCountsButton.Tag = oSetAllEstimatedCountsButton.Name;
                        oSetAllEstimatedCountsButton.ImageIndex = toolbar.ImageList.Images.IndexOfKey(SET_ESTIMATED_COUNTS_ICON_KEY);
                        oSetAllEstimatedCountsButton.Enabled = true;
                        oSetAllEstimatedCountsButton.Style = ToolBarButtonStyle.PushButton;
                        toolbar.Buttons.Add(oSetAllEstimatedCountsButton);

                        toolbar.ImageList.Images.Add(EDIT_AGGREGATIONS_ICON_KEY, BIDSHelper.Resources.Common.EditAggregations);
                        ToolBarButton oEditAggregationsButton = new ToolBarButton();
                        oEditAggregationsButton.ToolTipText = "Edit Aggregations (BIDS Helper)";
                        oEditAggregationsButton.Name = this.FullName + "." + EDIT_AGGREGATIONS_BUTTON_SUFFIX;
                        oEditAggregationsButton.Tag = oEditAggregationsButton.Name;
                        oEditAggregationsButton.ImageIndex = toolbar.ImageList.Images.IndexOfKey(EDIT_AGGREGATIONS_ICON_KEY);
                        oEditAggregationsButton.Enabled = true;
                        oEditAggregationsButton.Style = ToolBarButtonStyle.PushButton;
                        toolbar.Buttons.Add(oEditAggregationsButton);

                        if (pi.Name.ToLower().EndsWith(".cube")) //checking the file extension is adequate because this feature is not needed for in online mode (when live connected to the server)
                        {
                            toolbar.ImageList.Images.Add(DEPLOY_AGGREGATION_DESIGNS_ICON_KEY, BIDSHelper.Resources.Common.DeployAggDesignsIcon);
                            ToolBarButton oDeployAggDesignsButton = new ToolBarButton();
                            oDeployAggDesignsButton.ToolTipText = "Deploy Aggregation Designs (BIDS Helper)";
                            oDeployAggDesignsButton.Name = this.FullName + "." + DEPLOY_AGGREGATION_DESIGNS_BUTTON;
                            oDeployAggDesignsButton.Tag = oDeployAggDesignsButton.Name;
                            oDeployAggDesignsButton.ImageIndex = toolbar.ImageList.Images.IndexOfKey(DEPLOY_AGGREGATION_DESIGNS_ICON_KEY);
                            oDeployAggDesignsButton.Enabled = true;
                            oDeployAggDesignsButton.Style = ToolBarButtonStyle.PushButton;
                            toolbar.Buttons.Add(oDeployAggDesignsButton);
                        }

                        toolbar.ImageList.Images.Add(STOP_ICON_KEY, BIDSHelper.Resources.Common.Stop);

                        //catch the button clicks of the new buttons we just added
                        toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);
                    }
                }
            }
            catch { }
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
            
        }

        void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button.Tag != null)
                {
                    string sButtonTag = e.Button.Tag.ToString();
                    if (sButtonTag == this.FullName + "." + SET_ESTIMATED_COUNTS_BUTTON)
                    {
                        if (e.Button.ImageIndex == e.Button.Parent.ImageList.Images.IndexOfKey(SET_ESTIMATED_COUNTS_ICON_KEY))
                        {
                            bMessageBoxShown = false;
                            bCancelEstimatedCountsClicked = false;
                            SetAllEstimatedCounts(e.Button);
                        }
                        else
                        {
                            bCancelEstimatedCountsClicked = true;
                        }
                    }
                    else if (sButtonTag == this.FullName + "." + EDIT_AGGREGATIONS_BUTTON_SUFFIX)
                    {
                        EditAggregations();
                    }
                    else if (sButtonTag == this.FullName + "." + DEPLOY_AGGREGATION_DESIGNS_BUTTON)
                    {
                        DeployAggDesigns();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private static void StartSetEstimatedCountsOnPartition(object threadInfo)
        {
            SetEstimatedCountsOnPartitionThreadInfo info = (SetEstimatedCountsOnPartitionThreadInfo)threadInfo;
            try
            {
                if (info.aggDesign != null && info.measureGroupDimension != null)
                {
                    if (info.instance.CheckCancelled()) return;
                    try
                    {
                        PartitionUtilities.SetEstimatedCountInAttributes(info.aggDesign, info.measureGroupDimension, new Partition[] { info.partition }, null);
                    }
                    catch (Exception ex)
                    {
                        info.errors.Add("BIDS Helper error setting estimated counts for dimension attributes of " + info.measureGroupDimension.CubeDimension.Name + " on partition " + info.partition.Name + " of measure group " + info.partition.Parent.Name + ": " + ex.Message);
                    }
                }

                if (info.instance.CheckCancelled() || info.partition.EstimatedRows > 0) return;
                try
                {
                    PartitionUtilities.SetEstimatedCountInPartition(info.partition, null);
                }
                catch (Exception ex)
                {
                    info.errors.Add("BIDS Helper error setting estimated counts for partition " + info.partition.Name + " of measure group " + info.partition.Parent.Name + ": " + ex.Message);
                }
            }
            catch { } //silently catch errors... don't want to message box errors because there could be hundreds of them
            finally
            {
                info.done = true;
                //info.autoResetEvent.Set();
            }
        }

        private static void StartSetEstimatedCountsOnDimension(object threadInfo)
        {
            SetEstimatedCountsOnDimensionThreadInfo info = (SetEstimatedCountsOnDimensionThreadInfo)threadInfo;
            try
            {
                if (info.instance.CheckCancelled()) return;
                System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static;
                object cnt = typeof(PartitionUtilities).InvokeMember("GetAttributeCountInDimension", getflags, null, null, new object[] { info.attribute, info.connection.Cartridge, info.connection });
                info.attribute.EstimatedCount = Convert.ToInt64(cnt);
            }
            catch (Exception ex)
            {
                info.errors.Add("BIDS Helper error setting estimated counts for attribute " + info.attribute.Name + " in dimension " + info.attribute.Parent.Name + ": " + ex.Message);
            }
            finally
            {
                info.done = true;
            }
        }


        void SetAllEstimatedCounts(ToolBarButton button)
        {
            //grab the objects I need before the user has a chance to flip to another active window
            Project proj = ApplicationObject.ActiveWindow.Project;
            Window window = ApplicationObject.ActiveWindow;
            Cube cube = (Cube)this.ApplicationObject.ActiveWindow.ProjectItem.Object;
            IDesignerHost designer = (IDesignerHost)ApplicationObject.ActiveWindow.Object;

            if (MessageBox.Show("Updating all estimated counts with exact counts for all partitions and dimensions\r\ncould take an extremely long time.\r\n\r\nAre you sure you want to continue?", "BIDS Helper - Update All Estimated Counts", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            button.ImageIndex = button.Parent.ImageList.Images.IndexOfKey(STOP_ICON_KEY); //change to a stop icon to allow the user to cancel
            Application.DoEvents();

            try
            {
                using (WaitCursor cursor1 = new WaitCursor())
                {
                    ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationGeneral);

                    IComponentChangeService changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));
                    changesvc.OnComponentChanging(cube, null);

                    System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
                    int iProgress = 0;
                    foreach (MeasureGroup mg in cube.MeasureGroups)
                    {
                        ApplicationObject.StatusBar.Progress(true, "Setting Estimated Counts on Measure Group: " + mg.Name, ++iProgress, cube.MeasureGroups.Count + cube.Parent.Dimensions.Count);
                        if (mg.Partitions.Count > 0)
                        {
                            foreach (Partition p in mg.Partitions)
                            {
                                p.EstimatedRows = 0;
                            }
                            foreach (AggregationDesign aggd in mg.AggregationDesigns)
                            {
                                //make sure each measure group dimension and attribute is in each agg design... fixes issue 21220
                                foreach (MeasureGroupDimension mgd in mg.Dimensions)
                                {
                                    if (mgd is RegularMeasureGroupDimension)
                                    {
                                        if (!aggd.Dimensions.Contains(mgd.CubeDimensionID))
                                        {
                                            aggd.Dimensions.Add(mgd.CubeDimensionID);
                                        }
                                        AggregationDesignDimension aggdd = aggd.Dimensions[mgd.CubeDimensionID];
                                        foreach (DimensionAttribute da in mgd.Dimension.Attributes)
                                        {
                                            if (da.AttributeHierarchyEnabled && mgd.CubeDimension.Attributes[da.ID].AttributeHierarchyEnabled && !aggdd.Attributes.Contains(da.ID))
                                            {
                                                aggdd.Attributes.Add(da.ID);
                                            }
                                        }
                                    }
                                }
                                
                                foreach (AggregationDesignDimension aggdim in aggd.Dimensions)
                                {
                                    foreach (AggregationDesignAttribute attr in aggdim.Attributes)
                                    {
                                        try
                                        {
                                            attr.EstimatedCount = 0;
                                            attr.Attribute.EstimatedCount = 0;
                                        }
                                        catch { }
                                    }
                                }
                                foreach (MeasureGroupDimension mgd in mg.Dimensions)
                                {
                                    if (mgd is RegularMeasureGroupDimension)
                                    {
                                        RegularMeasureGroupDimension dim = (RegularMeasureGroupDimension)mgd;
                                        foreach (Partition p in mg.Partitions)
                                        {
                                            if (p.AggregationDesignID == aggd.ID)
                                            {
                                                try
                                                {
                                                    SetEstimatedCountsOnPartitionThreadInfo info = new SetEstimatedCountsOnPartitionThreadInfo();
                                                    info.instance = this;
                                                    info.aggDesign = aggd;
                                                    info.measureGroupDimension = dim;
                                                    info.partition = p;

                                                    //run as a separate thread so that the main app stays responsive (so you can click the cancel button)
                                                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(StartSetEstimatedCountsOnPartition), info);
                                                    while (!info.done)
                                                    {
                                                        System.Threading.Thread.Sleep(100);
                                                        Application.DoEvents(); //keeps main app responsive
                                                        if (CheckCancelled()) return;
                                                    }
                                                    errors.AddRange(info.errors);
                                                }
                                                catch (Exception ex)
                                                {
                                                    errors.Add("BIDS Helper error setting estimated counts on partition " + p.Name + " of measure group " + mg.Name + ": " + ex.Message);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //now fill in the count on partitions without agg designs
                            foreach (Partition p in mg.Partitions)
                            {
                                if (p.AggregationDesign == null)
                                {
                                    try
                                    {
                                        SetEstimatedCountsOnPartitionThreadInfo info = new SetEstimatedCountsOnPartitionThreadInfo();
                                        info.instance = this;
                                        info.partition = p;

                                        //run as a separate thread so that the main app stays responsive (so you can click the cancel button)
                                        System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(StartSetEstimatedCountsOnPartition), info);
                                        while (!info.done)
                                        {
                                            System.Threading.Thread.Sleep(100);
                                            Application.DoEvents(); //keeps main app responsive
                                            if (CheckCancelled()) return;
                                        }
                                        errors.AddRange(info.errors);
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add("BIDS Helper error setting estimated counts on partition " + p.Name + " of measure group " + mg.Name + ": " + ex.Message);
                                    }
                                }
                            }

                            long iMeasureGroupRowsCount = 0;
                            foreach (Partition p in mg.Partitions)
                            {
                                iMeasureGroupRowsCount += p.EstimatedRows;
                            }
                            mg.EstimatedRows = iMeasureGroupRowsCount;
                        }
                    }
                    changesvc.OnComponentChanged(cube, null, null, null); //marks the cube designer as dirty

                    
                    foreach (ProjectItem pi in proj.ProjectItems)
                    {
                        try
                        {
                            if (!(pi.Object is Dimension)) continue;
                        }
                        catch
                        {
                            continue; //doing the above seems to blow up on certain objects because of threading? this fixes the problem
                        }
                        Dimension dim = (Dimension)pi.Object;
                        ApplicationObject.StatusBar.Progress(true, "Setting Estimated Counts on Dimension: " + dim.Name, ++iProgress, cube.MeasureGroups.Count + cube.Parent.Dimensions.Count);
                        
                        //open but don't show the dimension designer so you can get at the change service so you can mark it dirty
                        bool bIsOpen = pi.get_IsOpen(EnvDTE.Constants.vsViewKindDesigner);
                        Window win = null;
                        if (bIsOpen)
                        {
                            foreach (Window w in ApplicationObject.Windows)
                            {
                                if (w.ProjectItem != null && w.ProjectItem.Document != null && w.ProjectItem.Document.FullName == pi.Document.FullName)
                                {
                                    win = w;
                                    break;
                                }
                            }
                        }
                        if (win == null)
                        {
                            win = pi.Open(EnvDTE.Constants.vsViewKindDesigner);
                            if (!bIsOpen) win.Visible = false;
                        }
                        designer = (IDesignerHost)win.Object;
                        changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));
                        changesvc.OnComponentChanging(dim, null);

                        if (dim.DataSource != null)
                        {
                            try
                            {
                                DataSourceConnection openedDataSourceConnection = DSVUtilities.GetOpenedDataSourceConnection(dim.DataSource);
                                foreach (DimensionAttribute attr in dim.Attributes)
                                {
                                    SetEstimatedCountsOnDimensionThreadInfo info = new SetEstimatedCountsOnDimensionThreadInfo();
                                    info.instance = this;
                                    info.attribute = attr;
                                    info.connection = openedDataSourceConnection;

                                    //run as a separate thread so that the main app stays responsive (so you can click the cancel button)
                                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(StartSetEstimatedCountsOnDimension), info);
                                    while (!info.done)
                                    {
                                        System.Threading.Thread.Sleep(100);
                                        Application.DoEvents(); //keeps main app responsive
                                        if (CheckCancelled()) return;
                                    }
                                    errors.AddRange(info.errors);
                                }
                            }
                            catch (Exception ex)
                            {
                                errors.Add("BIDS Helper error setting estimated counts on dimension " + dim.Name + ": " + ex.Message);
                            }
                        }
                        changesvc.OnComponentChanged(dim, null, null, null);
                    }
                    AddErrorsToVSErrorList(window, errors.ToArray());
                }
            }
            finally
            {
                try
                {
                    button.ImageIndex = button.Parent.ImageList.Images.IndexOfKey(SET_ESTIMATED_COUNTS_ICON_KEY);
                    ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationGeneral);
                    ApplicationObject.StatusBar.Progress(false, "", 1, 1);
                }
                catch { }
            }
        }

        private bool bMessageBoxShown = false;
        private bool CheckCancelled()
        {
            if (bCancelEstimatedCountsClicked && !bMessageBoxShown)
            {
                bMessageBoxShown = true;
                if (MessageBox.Show("Cancelling now could leave many counts set at zero. Are you sure you wish to cancel?", "BIDS Helper - Update All Estimated Counts", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ApplicationObject.StatusBar.Text = "Cancelling...";
                    return true;
                }
                else
                {
                    bCancelEstimatedCountsClicked = false;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void EditAggregations()
        {
            Cube cube = (Cube)this.ApplicationObject.ActiveWindow.ProjectItem.Object;
            ProjectItem pi = ApplicationObject.ActiveWindow.ProjectItem;

            AggManager.MainForm frm = new AggManager.MainForm(cube, pi);
            frm.ShowDialog(); //show as a modal dialog so there's no way to continue editing the cube with the normal designer until you're done with the agg manager
        }

        private void DeployAggDesigns()
        {
            ProjectItem pi = ApplicationObject.ActiveWindow.ProjectItem;
            DeployAggDesignsPlugin.DeployAggDesigns(pi, this.ApplicationObject);
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
                item.Document = window.ProjectItem.get_FileNames(0);
                item.CustomInfo = this;
                service.Add(item);
            }
        }

        public override string ShortName
        {
            get { return "SetAllEstimatedCounts"; }
        }

        //public override int Bitmap
        //{
        //    get { return 0; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Set All Estimated Counts"; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; }
        }


        /// <summary>
        /// Gets the name of the friendly name of the plug-in.
        /// </summary>
        /// <value>The friendly name.</value>
        public override string FeatureName
        {
            get
            {
                return "Update Estimated Counts";
            }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASMulti; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Allows you to update the EstimatedCount property of every dimension attribute and partition with exact counts. Better counts help the Aggregation Design Wizard choose better aggregations."; }
        }


        public override void Exec()
        {
        }

        #region Internal Thread Info Classes
        public class SetEstimatedCountsOnPartitionThreadInfo {
            public EstimatedCountsPlugin instance;
            public AggregationDesign aggDesign;
            public RegularMeasureGroupDimension measureGroupDimension;
            public Partition partition;
            public bool done = false;
            public System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
        }

        public class SetEstimatedCountsOnDimensionThreadInfo
        {
            public EstimatedCountsPlugin instance;
            public DimensionAttribute attribute;
            public DataSourceConnection connection;
            public bool done = false;
            public System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
        }
        #endregion
    }
}