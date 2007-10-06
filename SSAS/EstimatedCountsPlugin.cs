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

namespace BIDSHelper
{
    public class EstimatedCountsPlugin : BIDSHelperPluginBase
    {
        private WindowEvents windowEvents;
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.List<string> windowHandlesFixedPartitionsView = new System.Collections.Generic.List<string>();

        private const string SET_ESTIMATED_COUNTS_ICON_KEY = "EstimatedCounts";
        private const string EDIT_AGGREGATIONS_ICON_KEY = "EditAggregations";
        private const string DEPLOY_AGGREGATION_DESIGNS_ICON_KEY = "DeployAggDesigns";
        private const string STOP_ICON_KEY = "Stop";
        private const string EDIT_AGGREGATIONS_BUTTON_SUFFIX = "EditAggregationsButton";
        private const string SET_ESTIMATED_COUNTS_BUTTON = "SetEstimatedCountsButton";
        private const string DEPLOY_AGGREGATION_DESIGNS_BUTTON = "DeployAggDesignsButton";

        private bool bCancelEstimatedCountsClicked = false;

        public EstimatedCountsPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
            windowEvents = appObject.Events.get_WindowEvents(null);
            windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
        }

        void windowEvents_WindowCreated(Window Window)
        {
            windowEvents_WindowActivated(Window, null);
        }

        void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus == null) return;
                IDesignerHost designer = (IDesignerHost)GotFocus.Object;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if (!(pi.Object is Cube)) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);

                IntPtr ptr = win.Handle;
                string sHandle = ptr.ToInt64().ToString();

                if (!windowHandlesFixedPartitionsView.Contains(sHandle))
                {
                    windowHandlesFixedPartitionsView.Add(sHandle);
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                }

                if (win.SelectedView.Caption == "Partitions")
                {
                    if (!toolbar.Buttons.ContainsKey(this.FullName + "." + SET_ESTIMATED_COUNTS_BUTTON))
                    {
                        ToolBarButton separator = new ToolBarButton();
                        separator.Style = ToolBarButtonStyle.Separator;
                        toolbar.Buttons.Add(separator);

                        toolbar.ImageList.Images.Add(SET_ESTIMATED_COUNTS_ICON_KEY, Properties.Resources.EstimatedCounts);
                        ToolBarButton oSetAllEstimatedCountsButton = new ToolBarButton();
                        oSetAllEstimatedCountsButton.ToolTipText = "Update All Estimated Counts (BIDS Helper)";
                        oSetAllEstimatedCountsButton.Name = this.FullName + "." + SET_ESTIMATED_COUNTS_BUTTON;
                        oSetAllEstimatedCountsButton.Tag = oSetAllEstimatedCountsButton.Name;
                        oSetAllEstimatedCountsButton.ImageIndex = toolbar.ImageList.Images.IndexOfKey(SET_ESTIMATED_COUNTS_ICON_KEY);
                        oSetAllEstimatedCountsButton.Enabled = true;
                        oSetAllEstimatedCountsButton.Style = ToolBarButtonStyle.PushButton;
                        toolbar.Buttons.Add(oSetAllEstimatedCountsButton);

                        toolbar.ImageList.Images.Add(EDIT_AGGREGATIONS_ICON_KEY, Properties.Resources.EditAggregations);
                        ToolBarButton oEditAggregationsButton = new ToolBarButton();
                        oEditAggregationsButton.ToolTipText = "Edit Aggregations (BIDS Helper)";
                        oEditAggregationsButton.Name = this.FullName + "." + EDIT_AGGREGATIONS_BUTTON_SUFFIX;
                        oEditAggregationsButton.Tag = oEditAggregationsButton.Name;
                        oEditAggregationsButton.ImageIndex = toolbar.ImageList.Images.IndexOfKey(EDIT_AGGREGATIONS_ICON_KEY);
                        oEditAggregationsButton.Enabled = true;
                        oEditAggregationsButton.Style = ToolBarButtonStyle.PushButton;
                        toolbar.Buttons.Add(oEditAggregationsButton);

                        toolbar.ImageList.Images.Add(DEPLOY_AGGREGATION_DESIGNS_ICON_KEY, Properties.Resources.DeployAggDesignsIcon);
                        ToolBarButton oDeployAggDesignsButton = new ToolBarButton();
                        oDeployAggDesignsButton.ToolTipText = "Deploy Aggregation Designs (BIDS Helper)";
                        oDeployAggDesignsButton.Name = this.FullName + "." + DEPLOY_AGGREGATION_DESIGNS_BUTTON;
                        oDeployAggDesignsButton.Tag = oDeployAggDesignsButton.Name;
                        oDeployAggDesignsButton.ImageIndex = toolbar.ImageList.Images.IndexOfKey(DEPLOY_AGGREGATION_DESIGNS_ICON_KEY);
                        oDeployAggDesignsButton.Enabled = true;
                        oDeployAggDesignsButton.Style = ToolBarButtonStyle.PushButton;
                        toolbar.Buttons.Add(oDeployAggDesignsButton);

                        toolbar.ImageList.Images.Add(STOP_ICON_KEY, Properties.Resources.Stop);

                        //catch the button clicks of the new buttons we just added
                        toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);
                    }
                }
            }
            catch { }
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            windowEvents_WindowActivated(this.ApplicationObject.ActiveWindow, null);
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
                if (info.instance.CheckCancelled()) return;
                Microsoft.AnalysisServices.Design.PartitionUtilities.SetEstimatedCountInAttributes(info.aggDesign, info.measureGroupDimension, new Partition[] { info.partition }, null);
                if (info.instance.CheckCancelled()) return;
                Microsoft.AnalysisServices.Design.PartitionUtilities.SetEstimatedCountInPartition(info.partition, null);
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
                object cnt = typeof(Microsoft.AnalysisServices.Design.PartitionUtilities).InvokeMember("GetAttributeCountInDimension", getflags, null, null, new object[] { info.attribute, info.connection.Cartridge, info.connection });
                info.attribute.EstimatedCount = Convert.ToInt64(cnt);
            }
            catch { } //silently catch errors... don't want to message box errors because there could be hundreds of them
            finally
            {
                info.done = true;
            }
        }


        void SetAllEstimatedCounts(ToolBarButton button)
        {
            //grab the objects I need before the user has a chance to flip to another active window
            Project proj = ApplicationObject.ActiveWindow.Project;
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

                    int iProgress = 0;
                    foreach (MeasureGroup mg in cube.MeasureGroups)
                    {
                        ApplicationObject.StatusBar.Progress(true, "Setting Estimated Counts on Measure Group: " + mg.Name, ++iProgress, cube.MeasureGroups.Count + cube.Parent.Dimensions.Count);
                        if (mg.Partitions.Count > 0)
                        {
                            foreach (AggregationDesign aggd in mg.AggregationDesigns)
                            {
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
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                            }
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
                                DataSourceConnection openedDataSourceConnection = Microsoft.AnalysisServices.Design.DSVUtilities.GetOpenedDataSourceConnection(dim.DataSource);
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
                                }
                            }
                            catch { }
                        }
                        changesvc.OnComponentChanged(dim, null, null, null);
                    }
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

        private bool CheckCancelled()
        {
            if (bCancelEstimatedCountsClicked)
            {
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


        public override string ShortName
        {
            get { return "SetAllEstimatedCounts"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "Set All Estimated Counts"; }
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

        #region Internal Thread Info Classes
        public class SetEstimatedCountsOnPartitionThreadInfo {
            public EstimatedCountsPlugin instance;
            public AggregationDesign aggDesign;
            public RegularMeasureGroupDimension measureGroupDimension;
            public Partition partition;
            public bool done = false;
        }

        public class SetEstimatedCountsOnDimensionThreadInfo
        {
            public EstimatedCountsPlugin instance;
            public DimensionAttribute attribute;
            public DataSourceConnection connection;
            public bool done = false;
        }
        #endregion
    }
}