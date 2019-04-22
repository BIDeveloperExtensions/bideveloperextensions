#if SQL2019
extern alias asAlias;
//extern alias sharedDataWarehouseInterfaces;
using asAlias::Microsoft.DataWarehouse.Design;
using asAlias::Microsoft.DataWarehouse.Controls;
//using sharedDataWarehouseInterfaces::Microsoft.DataWarehouse.Design;
//using asAlias::Microsoft.AnalysisServices.Design;
using asAlias::Microsoft.DataWarehouse.ComponentModel;
#else
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
//using Microsoft.AnalysisServices.Design;
using Microsoft.DataWarehouse.ComponentModel;
#endif

using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.ComponentModel.Design;
//using Microsoft.DataWarehouse.Design;
//using Microsoft.DataWarehouse.Controls;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class M2MMatrixCompressionPlugin : BIDSHelperWindowActivatedPluginBase
    {
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.Dictionary<string, EditorWindow> windowHandlesFixedForM2M = new System.Collections.Generic.Dictionary<string, EditorWindow>();
        private System.Collections.Generic.Dictionary<string, EditorWindow> windowHandlesFixedForPrintFriendly = new System.Collections.Generic.Dictionary<string, EditorWindow>();

        private ToolBarButton newM2MButton = null;
        private ToolBarButton newPrintFriendlyButton = null;
        private ToolBarButton newSeparatorButton = null;

        public override void OnDisable()
        {
            base.OnDisable();

            foreach (EditorWindow win in windowHandlesFixedForM2M.Values)
            {
                win.ActiveViewChanged -= win_ActiveViewChanged;

                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);
                if (toolbar != null)
                {
                    toolbar.ButtonClick -= toolbar_ButtonClick;

                    if (newM2MButton != null)
                    {
                        if (toolbar.Buttons.ContainsKey(newM2MButton.Name))
                        { toolbar.Buttons.RemoveByKey(newM2MButton.Name); }

                        if (newSeparatorButton != null)
                        {
                            if (toolbar.Buttons.ContainsKey(newSeparatorButton.Name))
                            { toolbar.Buttons.RemoveByKey(newSeparatorButton.Name); }
                        }
                    }

                    if (newPrintFriendlyButton != null)
                    {
                        if (toolbar.Buttons.ContainsKey(newPrintFriendlyButton.Name))
                        { toolbar.Buttons.RemoveByKey(newPrintFriendlyButton.Name); }
                    }
                }
            }
        }

        public override bool ShouldHookWindowCreated
        {
            get { return true; }
        }

        void windowEvents_WindowCreated(Window Window)
        {
            OnWindowActivated(Window, null);
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
        }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus == null) return;
                IDesignerHost designer = GotFocus.Object as IDesignerHost;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if ((pi == null) || (!(pi.Object is Cube))) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(IComponentNavigator));
                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);

                IntPtr ptr = win.Handle;
                string sHandle = ptr.ToInt64().ToString();

                if (!windowHandlesFixedForM2M.ContainsKey(sHandle))
                {
                    windowHandlesFixedForM2M.Add(sHandle, win);
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                }

                if (win.SelectedView.MenuItemCommandID.ID == 12898) //language neutral way of saying win.SelectedView.Caption == "Dimension Usage"
                {
                    if (!toolbar.Buttons.ContainsKey(this.FullName + ".M2M"))
                    {
                        newSeparatorButton = new ToolBarButton();
                        newSeparatorButton.Name = this.FullName + ".Separator";
                        newSeparatorButton.Style = ToolBarButtonStyle.Separator;

                        toolbar.ImageList.Images.Add(BIDSHelper.Resources.Common.M2MIcon);
                        newM2MButton = new ToolBarButton();
                        newM2MButton.ToolTipText = this.FeatureName + " (BIDS Helper)";
                        newM2MButton.Name = this.FullName + ".M2M";
                        newM2MButton.Tag = newM2MButton.Name;
                        newM2MButton.ImageIndex = toolbar.ImageList.Images.Count - 1;
                        newM2MButton.Enabled = true;
                        newM2MButton.Style = ToolBarButtonStyle.PushButton;

                        if (BIDSHelperPackage.Plugins[BaseName + typeof(PrinterFriendlyDimensionUsagePlugin).Name].Enabled)
                        {
                            toolbar.ImageList.Images.Add(BIDSHelper.Resources.Common.PrinterFriendlyDimensionUsageIcon);
                            newPrintFriendlyButton = new ToolBarButton();
                            newPrintFriendlyButton.ToolTipText = "Printer Friendly Dimension Usage (BIDS Helper)";
                            newPrintFriendlyButton.Name = this.FullName + ".PrintFriendly";
                            newPrintFriendlyButton.Tag = newPrintFriendlyButton.Name;
                            newPrintFriendlyButton.ImageIndex = toolbar.ImageList.Images.Count - 1;
                            newPrintFriendlyButton.Enabled = true;
                            newPrintFriendlyButton.Style = ToolBarButtonStyle.PushButton;
                        }

                        //catch the button clicks of the new buttons we just added
                        toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);
                    }

                    if (newM2MButton != null && !toolbar.Buttons.Contains(newM2MButton))
                    {
                        toolbar.Buttons.Add(newSeparatorButton);
                        toolbar.Buttons.Add(newM2MButton);
                    }
                    if (newPrintFriendlyButton != null && !toolbar.Buttons.Contains(newPrintFriendlyButton))
                    {
                        toolbar.Buttons.Add(newPrintFriendlyButton);
                    }
                }
            }
            catch { }
        }

        void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button.Tag != null)
                {
                    string sButtonTag = e.Button.Tag.ToString();
                    if (sButtonTag == this.FullName + ".M2M")
                    {
                        this.Exec();
                    }
                    else if (sButtonTag == this.FullName + ".PrintFriendly")
                    {
                        BIDSHelperPackage.Plugins[BaseName + typeof(PrinterFriendlyDimensionUsagePlugin).Name].Exec();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public M2MMatrixCompressionPlugin(BIDSHelperPackage package)
            : base(package)
        {
        }

        #region Standard Properties
        public override string ShortName
        {
            get { return "M2MMatrixCompressionPlugin"; }
        }

        //public override int Bitmap
        //{
        //    get { return 4380; }
        //}


        public override string FeatureName
        {
            get { return "M2M Matrix Compression"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; /*doesn't show anywhere*/ }
        }


        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        public override string HelpUrl
        {
            get { return this.GetCodePlexHelpUrl("Many-to-Many Matrix Compression"); }
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
            get { return "Analyzes the data for each many to many relationship to determine whether it can be compressed, presenting the results in an easy to interpret format."; }
        }
        #endregion

        ///// <summary>
        ///// Determines if the command should be displayed or not.
        ///// </summary>
        ///// <param name="item"></param>
        ///// <returns></returns>
        //public override bool DisplayCommand(UIHierarchyItem item)
        //{
        //    try
        //    {
        //        if (this.ApplicationObject.ActiveWindow == null || this.ApplicationObject.ActiveWindow.ProjectItem == null)
        //            return false;

        //        ProjectItem pi = this.ApplicationObject.ActiveWindow.ProjectItem;
        //        if (pi.Object is Cube)
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}


        public override void Exec()
        {
            try
            {
                ProjectItem pi = this.ApplicationObject.ActiveWindow.ProjectItem;
                Cube cube = (Cube)pi.Object;

                List<M2MMatrixCompressionStat> listStats = BuildQueries(cube);

                BIDSHelper.SSAS.M2MMatrixCompressionForm form = new BIDSHelper.SSAS.M2MMatrixCompressionForm();
                form.m2mMatrixCompressionStatBindingSource.DataSource = listStats;
                form.ShowDialog();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public List<M2MMatrixCompressionStat> BuildQueries(Cube cube)
        {
            List<M2MMatrixCompressionStat> listStats = new List<M2MMatrixCompressionStat>();
            foreach (MeasureGroup mg in cube.MeasureGroups)
            {
                if (mg.IsLinked) continue;
                Dictionary<MeasureGroup, List<ManyToManyMeasureGroupDimension>> dictM2M = new Dictionary<MeasureGroup, List<ManyToManyMeasureGroupDimension>>();
                foreach (MeasureGroupDimension mgd in mg.Dimensions)
                {
                    if (mgd is ManyToManyMeasureGroupDimension)
                    {
                        ManyToManyMeasureGroupDimension m2mmgd = (ManyToManyMeasureGroupDimension)mgd;
                        if (!dictM2M.ContainsKey(m2mmgd.MeasureGroup))
                            dictM2M.Add(m2mmgd.MeasureGroup, new List<ManyToManyMeasureGroupDimension>());
                        dictM2M[m2mmgd.MeasureGroup].Add(m2mmgd);
                    }
                }
                if (dictM2M.Count > 0)
                {
                    //there are m2m dimensions used by this data measure group
                    foreach (MeasureGroup intermediateMG in dictM2M.Keys)
                    {
                        if (intermediateMG.IsLinked) continue;
                        try
                        {
                            List<CubeAttribute> commonDimensions = new List<CubeAttribute>();
                            foreach (CubeDimension cd in cube.Dimensions)
                            {
                                if (mg.Dimensions.Contains(cd.ID) && intermediateMG.Dimensions.Contains(cd.ID))
                                {
                                    if (mg.Dimensions[cd.ID] is RegularMeasureGroupDimension
                                        && intermediateMG.Dimensions[cd.ID] is RegularMeasureGroupDimension)
                                    {
                                        //it's a common dimension
                                        RegularMeasureGroupDimension rmgdData = (RegularMeasureGroupDimension)mg.Dimensions[cd.ID];
                                        MeasureGroupAttribute mgaData = GetGranularityAttribute(rmgdData);
                                        RegularMeasureGroupDimension rmgdIntermediate = (RegularMeasureGroupDimension)intermediateMG.Dimensions[cd.ID];
                                        MeasureGroupAttribute mgaIntermediate = GetGranularityAttribute(rmgdIntermediate);
                                        CubeAttribute ca = mgaData.CubeAttribute;
                                        if (mgaData.AttributeID != mgaIntermediate.AttributeID)
                                        {
                                            if (IsParentOf(mgaIntermediate.Attribute, mgaData.Attribute))
                                            {
                                                ca = mgaIntermediate.CubeAttribute;
                                            }
                                        }
                                        commonDimensions.Add(ca);
                                    }
                                }
                            }

                            //fine while we're just doing this for SQL server
                            MeasureGroupHealthCheckPlugin.sq = "[";
                            MeasureGroupHealthCheckPlugin.fq = "]";

                            DsvTableBinding oTblBinding = new DsvTableBinding(intermediateMG.Parent.DataSourceView.ID, MeasureGroupHealthCheckPlugin.GetTableIdForDataItem(intermediateMG.Measures[0].Source));
                            string sFactQuery = "(" + MeasureGroupHealthCheckPlugin.GetQueryDefinition(intermediateMG.ParentDatabase, intermediateMG, oTblBinding, null) + ")";

                            List<string> listCommonDimensionsSeen = new List<string>();
                            string sCommonDimensions = "";
                            string sCommonDimensionsJoin = "";
                            foreach (CubeAttribute ca in commonDimensions)
                            {
                                RegularMeasureGroupDimension rmgd = (RegularMeasureGroupDimension)intermediateMG.Dimensions[ca.Parent.ID];
                                if (rmgd is ReferenceMeasureGroupDimension)
                                {
                                    if (mg.Dimensions.Contains(((ReferenceMeasureGroupDimension)rmgd).IntermediateCubeDimensionID)
                                        && mg.Dimensions[((ReferenceMeasureGroupDimension)rmgd).IntermediateCubeDimensionID] is RegularMeasureGroupDimension)
                                    {
                                        continue; //skip reference dimensions in the intermediate measure group because it won't change the cardinality
                                    }
                                    else
                                    {
                                        throw new Exception(rmgd.CubeDimension.Name + " dimension in intermediate measure group " + intermediateMG.Name + " is not supported by BIDS Helper M2M Matrix Compression");
                                    }
                                }

                                MeasureGroupAttribute mga = rmgd.Attributes[ca.AttributeID];
                                foreach (DataItem di in mga.KeyColumns)
                                {
                                    if (di.Source is ColumnBinding)
                                    {
                                        if (!listCommonDimensionsSeen.Contains("[" + ((ColumnBinding)di.Source).ColumnID + "]")) //if this column is already mentioned, then don't mention it again
                                        {
                                            listCommonDimensionsSeen.Add("[" + ((ColumnBinding)di.Source).ColumnID + "]");
                                            if (sCommonDimensionsJoin.Length == 0)
                                            {
                                                sCommonDimensionsJoin += "WHERE ";
                                            }
                                            else
                                            {
                                                sCommonDimensionsJoin += "\r\nAND ";
                                                sCommonDimensions += ", ";
                                            }
                                            sCommonDimensionsJoin += "f.[" + ((ColumnBinding)di.Source).ColumnID + "] = s.[" + ((ColumnBinding)di.Source).ColumnID + "]";
                                            sCommonDimensions += "[" + ((ColumnBinding)di.Source).ColumnID + "]";
                                        }
                                    }
                                }
                            }

                            List<string> listM2MDimensionsSeen = new List<string>();
                            string sM2MDimensions = "";
                            string sM2MDimensionsOrderBy = "";
                            foreach (ManyToManyMeasureGroupDimension m2mmgd in dictM2M[intermediateMG])
                            {
                                if (intermediateMG.Dimensions[m2mmgd.CubeDimensionID] is RegularMeasureGroupDimension)
                                {
                                    RegularMeasureGroupDimension rmgd = (RegularMeasureGroupDimension)intermediateMG.Dimensions[m2mmgd.CubeDimensionID];
                                    if (rmgd is ReferenceMeasureGroupDimension) continue; //won't change 
                                    if (rmgd is ReferenceMeasureGroupDimension)
                                    {
                                        if (mg.Dimensions.Contains(((ReferenceMeasureGroupDimension)rmgd).IntermediateCubeDimensionID)
                                            && mg.Dimensions[((ReferenceMeasureGroupDimension)rmgd).IntermediateCubeDimensionID] is ManyToManyMeasureGroupDimension)
                                        {
                                            continue; //skip reference dimensions in the intermediate measure group because it won't change the cardinality
                                        }
                                        else
                                        {
                                            throw new Exception(rmgd.CubeDimension.Name + " dimension in intermediate measure group " + intermediateMG.Name + " is not supported by BIDS Helper M2M Matrix Compression");
                                        }
                                    }

                                    MeasureGroupAttribute mga = GetGranularityAttribute(rmgd);
                                    foreach (DataItem di in mga.KeyColumns)
                                    {
                                        if (di.Source is ColumnBinding)
                                        {
                                            if (!listM2MDimensionsSeen.Contains("[" + ((ColumnBinding)di.Source).ColumnID + "]")) //if this column is already mentioned, then don't mention it again
                                            {
                                                listM2MDimensionsSeen.Add("[" + ((ColumnBinding)di.Source).ColumnID + "]");
                                                if (sM2MDimensions.Length > 0)
                                                {
                                                    sM2MDimensions += " + '|' + ";
                                                    sM2MDimensionsOrderBy += ", ";
                                                }
                                                sM2MDimensions += "isnull(cast([" + ((ColumnBinding)di.Source).ColumnID + "] as nvarchar(max)),'')";
                                                sM2MDimensionsOrderBy += "[" + ((ColumnBinding)di.Source).ColumnID + "]";
                                            }
                                        }
                                    }
                                }
                            }

                            string sSQL = @"
SELECT (SELECT COUNT(*) FROM " + sFactQuery + @" x) OriginalRecordCount
, COUNT(MatrixKey) MatrixDimensionRecordCount
, SUM(cast(KeyCount AS FLOAT)) CompressedRecordCount
FROM (
 SELECT DISTINCT COUNT(*) KeyCount
 , MatrixKey = (
  SELECT " + sM2MDimensions + @" AS [data()] 
  FROM " + sFactQuery + @" f
  " + sCommonDimensionsJoin + @"
  ORDER BY " + sM2MDimensionsOrderBy + @"
  FOR XML PATH ('')
 )
 FROM " + sFactQuery + @" s 
 GROUP BY " + sCommonDimensions + @"
) SUBQ
";
                            M2MMatrixCompressionStat stat = new M2MMatrixCompressionStat();
                            stat.IntermediateMeasureGroup = intermediateMG;
                            stat.DataMeasureGroup = mg;
                            stat.SQL = sSQL;
                            listStats.Add(stat);


                        }
                        catch (Exception ex)
                        {
                            M2MMatrixCompressionStat stat = new M2MMatrixCompressionStat();
                            stat.IntermediateMeasureGroup = intermediateMG;
                            stat.DataMeasureGroup = mg;
                            stat.Error = ex.Message + "\r\n" + ex.StackTrace;
                            listStats.Add(stat);
                        }

                    }
                }
            }

            return listStats;
        }

        private bool IsParentOf(DimensionAttribute parent, DimensionAttribute child)
        {
            foreach (AttributeRelationship rel in child.AttributeRelationships)
            {
                if (rel.AttributeID == parent.ID)
                    return true;
                else if (IsParentOf(parent, rel.Attribute))
                    return true;
            }
            return false;
        }

        private MeasureGroupAttribute GetGranularityAttribute(RegularMeasureGroupDimension rmgd)
        {
            foreach (MeasureGroupAttribute mga in rmgd.Attributes)
            {
                if (mga.Type == MeasureGroupAttributeType.Granularity)
                {
                    return mga;
                }
            }
            throw new Exception("Can't find granularity attribute for dimension " + rmgd.CubeDimension.Name + " in measure group " + rmgd.Parent.Name);
        }

        public class M2MMatrixCompressionStat
        {
            private string _SQL;
            public string SQL
            {
                get { return _SQL; }
                set { _SQL = value; }
            }

            private MeasureGroup _DataMeasureGroup;
            public MeasureGroup DataMeasureGroup
            {
                get { return _DataMeasureGroup; }
                set { _DataMeasureGroup = value; }
            }

            public string DataMeasureGroupName
            {
                get { return _DataMeasureGroup.Name; }
            }

            private MeasureGroup _IntermediateMeasureGroup;
            public MeasureGroup IntermediateMeasureGroup
            {
                get { return _IntermediateMeasureGroup; }
                set { _IntermediateMeasureGroup = value; }
            }

            public string IntermediateMeasureGroupName
            {
                get { return IntermediateMeasureGroup.Name; }
            }

            private bool _RunQuery = true;
            public bool RunQuery
            {
                get { return _RunQuery; }
                set
                {
                    _RunQuery = value;
                    if (!_RunQuery && _Status != M2MMatrixCompressionStatStatus.Complete)
                    {
                        _Status = M2MMatrixCompressionStatStatus.Cancelled;
                    }
                    else if (_RunQuery && _Status != M2MMatrixCompressionStatStatus.Complete)
                    {
                        _Status = M2MMatrixCompressionStatStatus.Pending;
                    }
                }
            }

            public enum M2MMatrixCompressionStatStatus
            {
                Pending,
                Running,
                Cancelled,
                Complete,
                Error
            }

            private M2MMatrixCompressionStatStatus _Status = M2MMatrixCompressionStatStatus.Pending;
            public M2MMatrixCompressionStatStatus Status
            {
                get { return _Status; }
                set { _Status = value; }
            }
            
            private Int64? _OriginalRecordCount;
            public Int64? OriginalRecordCount
            {
                get { return _OriginalRecordCount; }
                set { _OriginalRecordCount = value; }
            }

            private Int64? _CompressedRecordCount;
            public Int64? CompressedRecordCount
            {
                get { return _CompressedRecordCount; }
                set { _CompressedRecordCount = value; }
            }

            public float? ReductionPercent
            {
                get
                {
                    if (_OriginalRecordCount != null && _OriginalRecordCount != 0 && _CompressedRecordCount != null)
                        return ((float)(_OriginalRecordCount - _CompressedRecordCount)) / ((float)_OriginalRecordCount);
                    else
                        return null;
                }
            }

            private Int64? _MatrixDimensionRecordCount;
            public Int64? MatrixDimensionRecordCount
            {
                get { return _MatrixDimensionRecordCount; }
                set { _MatrixDimensionRecordCount = value; }
            }

            private string _Error;
            public string Error
            {
                get { return _Error; }
                set
                {
                    _Error = value;
                    _Status = M2MMatrixCompressionStatStatus.Error;
                }
            }
        }

    }
}
