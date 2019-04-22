#if SQL2019
extern alias asAlias;
using asAlias.Microsoft.DataWarehouse;
using asAlias.Microsoft.DataWarehouse.Design;
using asAlias.Microsoft.DataWarehouse.Controls;
using asAlias::Microsoft.AnalysisServices.Controls;
using asAlias::Microsoft.DataWarehouse.ComponentModel;
#else
using Microsoft.DataWarehouse;
using Microsoft.DataWarehouse.Design;
using Microsoft.AnalysisServices.Design;
using Microsoft.AnalysisServices.Controls;
using Microsoft.DataWarehouse.ComponentModel;
#endif

using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.Data;
using System.Data.OleDb;
using System.ComponentModel.Design;
//using Microsoft.DataWarehouse.Design;
//using Microsoft.DataWarehouse.Controls;
using Microsoft.Win32;
using BIDSHelper.Core;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class MeasureGroupHealthCheckPlugin : BIDSHelperPluginBase
    {
        private const System.Reflection.BindingFlags getfieldflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;

        public MeasureGroupHealthCheckPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.MeasureGroupHealthCheckId);
        }

        private static string REGISTRY_FREE_SPACE_FACTOR_SETTING_NAME = "FreeSpaceFactor";
        public static int FreeSpaceFactorDefault = 20;
        private static int? _FreeSpaceFactor = null;
        public static int FreeSpaceFactor
        {
            get
            {
                if (_FreeSpaceFactor == null)
                {
                    int i = FreeSpaceFactorDefault;
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey(BIDSHelperPackage.PluginRegistryPath(typeof(SmartDiffPlugin)));
                    if (rk != null)
                    {
                        i = (int)rk.GetValue(REGISTRY_FREE_SPACE_FACTOR_SETTING_NAME, i);
                        rk.Close();
                    }
                    return i;
                }
                else
                {
                    return _FreeSpaceFactor.Value;
                }
            }
            set
            {
                var regPath = BIDSHelperPackage.PluginRegistryPath(typeof(SmartDiffPlugin));
                RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(regPath, true);
                if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(regPath);
                settingKey.SetValue(REGISTRY_FREE_SPACE_FACTOR_SETTING_NAME, value, RegistryValueKind.DWord);
                settingKey.Close();
                _FreeSpaceFactor = value;
            }
        }

        public override string ShortName
        {
            get { return "MeasureGroupHealthCheck"; }
        }

        //public override int Bitmap
        //{
        //    get { return 4380; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Measure Group Health Check"; }
        //}

        //public override string ToolTip
        //{
        //    get { return string.Empty; /*doesn't show anywhere*/ }
        //}

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
        //}

// TODO - figure out how to hook the "Measures" menu
        //public override string MenuName
        //{
        //    get { return "Measures"; }
        //}

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
            get { return "Allows you to check various indications of measure group health."; }
        }

        public override string ToolTip
        {
            get
            {
                return string.Empty;
            }
        }

        public override string FeatureName
        {
            get
            {
                return "MeasureGroup Health Check";
            }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ShouldDisplayCommand()
        {
            try
            {
                if (this.ApplicationObject.ActiveWindow == null || this.ApplicationObject.ActiveWindow.ProjectItem == null)
                    return false;

                ProjectItem pi = this.ApplicationObject.ActiveWindow.ProjectItem;
                if (pi.Object is Cube)
                {
                    IDesignerHost designer = (IDesignerHost)pi.Document.ActiveWindow.Object;
                    MeasureGroup mg = GetCurrentlySelectedMeasureGroup(designer);
                    if (mg != null) return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private MeasureGroup GetCurrentlySelectedMeasureGroup(IDesignerHost designer)
        {
            if (designer == null) return null;
            EditorWindow win = (EditorWindow)designer.GetService(typeof(IComponentNavigator));
            //if (win.SelectedView.Caption != "Cube Structure") return null;
            if (win.SelectedView.MenuItemCommandID.ID != (int)BIDSViewMenuItemCommandID.CubeStructure) return null;
            object cubeBuilderView = win.SelectedView.GetType().InvokeMember("viewControl", getfieldflags, null, win.SelectedView, null);
            object measuresView = cubeBuilderView.GetType().InvokeMember("measuresView", getfieldflags, null, cubeBuilderView, null);
            MultipleStateTreeView measuresTreeview = (MultipleStateTreeView)measuresView.GetType().InvokeMember("measuresTreeview", getfieldflags, null, measuresView, null);
            //Microsoft.AnalysisServices.Controls.
            LateNamedComponentTreeNode selectedNode = (LateNamedComponentTreeNode)measuresTreeview.SelectedNode;
            if (selectedNode.NamedComponent is MeasureGroup)
                return (MeasureGroup)selectedNode.NamedComponent;
            return null;
        }


        public override void Exec()
        {
            try
            {
                IDesignerHost designer = (IDesignerHost)this.ApplicationObject.ActiveWindow.Object;
                MeasureGroup mg = GetCurrentlySelectedMeasureGroup(designer);

                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(true, "Checking Measure Group Health...", 1, 2);

                Check(mg, designer);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(false, "Checking Measure Group Health...", 2, 2);
            }
        }

        //TODO: add properties that hold the number of decimals and digits... not recommending single is adequate for now

        public static MeasureDataTypeOption[] dataTypeOptions = new MeasureDataTypeOption[] {
            new MeasureDataTypeOption(MeasureDataType.BigInt, typeof(Int64), (double)Int64.MinValue, (double)Int64.MaxValue, false, false),
            new MeasureDataTypeOption(MeasureDataType.Boolean, typeof(bool), (double)-1, (double)0, false, false),
            new MeasureDataTypeOption(MeasureDataType.Currency, typeof(decimal), -922337203685477.5808m, 922337203685477.5807m, true, false, "up to 4 decimals"),
            new MeasureDataTypeOption(MeasureDataType.Double, typeof(double), double.MinValue, double.MaxValue, true, true, "up to 15 digits"),
            new MeasureDataTypeOption(MeasureDataType.Integer, typeof(int), (double)int.MinValue, (double)int.MaxValue, false, false),
            new MeasureDataTypeOption(MeasureDataType.Single, typeof(Single), Single.MinValue, Single.MaxValue, true, true, "up to 7 digits"),
            new MeasureDataTypeOption(MeasureDataType.SmallInt, typeof(Int16), (double)Int16.MinValue, (double)Int16.MaxValue, false, false),
            new MeasureDataTypeOption(MeasureDataType.TinyInt, typeof(SByte), (double)SByte.MinValue, (double)SByte.MaxValue, false, false),
            new MeasureDataTypeOption(MeasureDataType.UnsignedBigInt, typeof(UInt64), (double)UInt64.MinValue, (double)UInt64.MaxValue, false, false),
            new MeasureDataTypeOption(MeasureDataType.UnsignedInt, typeof(UInt32), (double)UInt32.MinValue, (double)UInt32.MaxValue, false, false),
            new MeasureDataTypeOption(MeasureDataType.UnsignedSmallInt, typeof(UInt16), (double)UInt16.MinValue, (double)UInt16.MaxValue, false, false),
            new MeasureDataTypeOption(MeasureDataType.UnsignedTinyInt, typeof(Byte), (double)Byte.MinValue, (double)Byte.MaxValue, false, false)
        };

        //making these static allows me not to have to change all the function signatures below... and it is fine as static because multiple dimension health checks can't be run in parallel
        internal static string sq = "[";
        internal static string fq = "]";

        public static void Check(MeasureGroup mg, IDesignerHost designer)
        {
            if (mg.Measures.Count == 0) throw new Exception(mg.Name + " has no measures.");
            if (mg.IsLinked) throw new Exception(mg.Name + " is a linked measure group. Run this Measure Group Health Check on the source measure group.");

            DataSource oDataSource = mg.Parent.DataSource;
            DsvTableBinding oTblBinding = new DsvTableBinding(mg.Parent.DataSourceView.ID, GetTableIdForDataItem(mg.Measures[0].Source));
            DataTable dtTable = mg.ParentDatabase.DataSourceViews[oTblBinding.DataSourceViewID].Schema.Tables[oTblBinding.TableID];
            
            //check whether this fact table uses an alternate datasource
            if (dtTable.ExtendedProperties.ContainsKey("DataSourceID"))
            {
                oDataSource = mg.ParentDatabase.DataSources[dtTable.ExtendedProperties["DataSourceID"].ToString()];
            }

            Microsoft.DataWarehouse.Design.DataSourceConnection openedDataSourceConnection = Microsoft.DataWarehouse.DataWarehouseUtilities.GetOpenedDataSourceConnection((object)null, oDataSource.ID, oDataSource.Name, oDataSource.ManagedProvider, oDataSource.ConnectionString, oDataSource.Site, false);
            try
            {
                if (openedDataSourceConnection != null)
                {
                    openedDataSourceConnection.QueryTimeOut = (int)oDataSource.Timeout.TotalSeconds;
                }
            }
            catch { }

            if (openedDataSourceConnection == null)
            {
                MessageBox.Show("Unable to connect to data source [" + oDataSource.Name + "]");
                return;
            }
            
            sq = openedDataSourceConnection.Cartridge.IdentStartQuote;
            fq = openedDataSourceConnection.Cartridge.IdentEndQuote;

            string sBitSqlDatatype = "bit";
            string sCountBig = "count_big(";
            string sCountBigEnd = ")";
            string sFloorFunctionBegin = "floor(";
            string sFloorFunctionEnd = ")";

            if (openedDataSourceConnection.DBServerName == "Oracle")
            {
                sBitSqlDatatype = "number(1,0)";
                sCountBig = "count(";
            }
            else if (openedDataSourceConnection.DBServerName == "Teradata")
            {
                sBitSqlDatatype = "numeric(1,0)";
                sCountBig = "cast(count(";
                sCountBigEnd = ") as bigint)";
                sFloorFunctionBegin = "cast(";
                sFloorFunctionEnd = " as bigint)";
            }

            string sFactQuery = GetQueryDefinition(mg.ParentDatabase, mg, oTblBinding, null);

            StringBuilder sOuterQuery = new StringBuilder();
            foreach (Measure m in mg.Measures)
            {
                //TODO: measure expressions
                if ((m.AggregateFunction == AggregationFunction.Sum && !(m.Source.Source is RowBinding))
                || m.AggregateFunction == AggregationFunction.AverageOfChildren
                || m.AggregateFunction == AggregationFunction.ByAccount
                || m.AggregateFunction == AggregationFunction.FirstChild
                || m.AggregateFunction == AggregationFunction.FirstNonEmpty
                || m.AggregateFunction == AggregationFunction.LastChild
                || m.AggregateFunction == AggregationFunction.LastNonEmpty
                || m.AggregateFunction == AggregationFunction.Min
                || m.AggregateFunction == AggregationFunction.Max
                || m.AggregateFunction == AggregationFunction.None)
                {
                    ColumnBinding cb = GetColumnBindingForDataItem(m.Source);
                    DataColumn col = mg.Parent.DataSourceView.Schema.Tables[cb.TableID].Columns[cb.ColumnID];

                    if (col.DataType == typeof(DateTime))
                        continue; //DateTime not supported by BIDS Helper except for count and distinct count aggregates

                    if (sOuterQuery.Length > 0)
                        sOuterQuery.Append(",");
                    else
                        sOuterQuery.Append("select ");

                    string sNegativeSign = string.Empty;
                    if (col.DataType == typeof(bool))
                        sNegativeSign = "-"; //true in sql is 1, but SSAS treats true as -1

                    if (m.AggregateFunction == AggregationFunction.Min
                    || m.AggregateFunction == AggregationFunction.Max
                    || m.AggregateFunction == AggregationFunction.None)
                        sOuterQuery.Append("max(").Append(sNegativeSign).Append("cast(").Append(sq).Append(cb.ColumnID).Append(fq).Append(" as float))").AppendLine();
                    else
                        sOuterQuery.Append("sum(").Append(sNegativeSign).Append("cast(").Append(sq).Append(cb.ColumnID).Append(fq).Append(" as float))").AppendLine();
                    sOuterQuery.Append(",min(").Append(sNegativeSign).Append("cast(").Append(sq).Append(cb.ColumnID).Append(fq).Append(" as float))").AppendLine();
                    sOuterQuery.Append(",max(").Append(sNegativeSign).Append("cast(").Append(sq).Append(cb.ColumnID).Append(fq).Append(" as float))").AppendLine();
                    sOuterQuery.Append(",cast(max(case when ").Append(sFloorFunctionBegin).Append(sq).Append(cb.ColumnID).Append(fq).Append(sFloorFunctionEnd).Append(" <> ").Append(sq).Append(cb.ColumnID).Append(fq).Append(" then 1 else 0 end) as ").Append(sBitSqlDatatype).Append(")").AppendLine();
                    sOuterQuery.Append(",cast(max(case when ").Append(sFloorFunctionBegin).Append(sq).Append(cb.ColumnID).Append(fq).Append("*10000.0").Append(sFloorFunctionEnd).Append(" <> cast(").Append(sq).Append(cb.ColumnID).Append(fq).Append("*10000.0 as float) then 1 else 0 end) as ").Append(sBitSqlDatatype).Append(")").AppendLine();
                }
                else if (m.AggregateFunction == AggregationFunction.Count
                || m.AggregateFunction == AggregationFunction.DistinctCount
                || (m.AggregateFunction == AggregationFunction.Sum && m.Source.Source is RowBinding))
                {
                    if (sOuterQuery.Length > 0)
                        sOuterQuery.Append(",");
                    else
                        sOuterQuery.Append("select ");
                    if (m.Source.Source is RowBinding)
                    {
                        if (m.AggregateFunction == AggregationFunction.DistinctCount)
                            throw new Exception("RowBinding on a distinct count not allowed by Analysis Services");
                        else
                            sOuterQuery.Append(sCountBig).Append("*").Append(sCountBigEnd).AppendLine();
                        sOuterQuery.Append(",0").AppendLine();
                        sOuterQuery.Append(",1").AppendLine();
                        sOuterQuery.Append(",cast(0 as ").Append(sBitSqlDatatype).Append(")").AppendLine();
                        sOuterQuery.Append(",cast(0 as ").Append(sBitSqlDatatype).Append(")").AppendLine();
                    }
                    else
                    {
                        ColumnBinding cb = GetColumnBindingForDataItem(m.Source);
                        DataColumn col = mg.Parent.DataSourceView.Schema.Tables[cb.TableID].Columns[cb.ColumnID];
                        if (m.AggregateFunction == AggregationFunction.DistinctCount)
                            sOuterQuery.Append(sCountBig).Append("distinct ").Append(sq).Append(cb.ColumnID).Append(fq).Append(sCountBigEnd).AppendLine();
                        else if (col.DataType == typeof(Byte[]))
                            sOuterQuery.Append("sum(cast(case when ").Append(sq).Append(cb.ColumnID).Append(fq).Append(" is not null then 1 else 0 end as float))").AppendLine();
                        else
                            sOuterQuery.Append(sCountBig).Append(sq).Append(cb.ColumnID).Append(fq).Append(sCountBigEnd).AppendLine();
                        sOuterQuery.Append(",0").AppendLine();
                        sOuterQuery.Append(",1").AppendLine();
                        sOuterQuery.Append(",cast(0 as ").Append(sBitSqlDatatype).Append(")").AppendLine();
                        sOuterQuery.Append(",cast(0 as ").Append(sBitSqlDatatype).Append(")").AppendLine();
                    }
                }
                else
                {
                    throw new Exception("Aggregation function " + m.AggregateFunction.ToString() + " not supported!");
                }
            }
            if (sOuterQuery.Length == 0) return;

            sOuterQuery.AppendLine("from (").Append(sFactQuery).AppendLine(") fact");

            DataSet ds = new DataSet();
            //openedDataSourceConnection.QueryTimeOut = 0; //just inherit from the datasource
            openedDataSourceConnection.Fill(ds, sOuterQuery.ToString());
            DataRow row = ds.Tables[0].Rows[0];
            openedDataSourceConnection.Close();

            List<MeasureHealthCheckResult> measureResults = new List<MeasureHealthCheckResult>();

            int i = 0;
            foreach (Measure m in mg.Measures)
            {
                if (m.AggregateFunction == AggregationFunction.Sum
                || m.AggregateFunction == AggregationFunction.AverageOfChildren
                || m.AggregateFunction == AggregationFunction.ByAccount
                || m.AggregateFunction == AggregationFunction.FirstChild
                || m.AggregateFunction == AggregationFunction.FirstNonEmpty
                || m.AggregateFunction == AggregationFunction.LastChild
                || m.AggregateFunction == AggregationFunction.LastNonEmpty
                || m.AggregateFunction == AggregationFunction.Count
                || m.AggregateFunction == AggregationFunction.DistinctCount
                || m.AggregateFunction == AggregationFunction.Min
                || m.AggregateFunction == AggregationFunction.Max
                || m.AggregateFunction == AggregationFunction.None)
                {
                    double dsvColMaxValue = 0;
                    bool dsvColAllowsDecimals = false;
                    if (m.Source.Source is ColumnBinding && m.AggregateFunction != AggregationFunction.Count && m.AggregateFunction != AggregationFunction.DistinctCount)
                    {
                        ColumnBinding cb = GetColumnBindingForDataItem(m.Source);
                        DataColumn col = mg.Parent.DataSourceView.Schema.Tables[cb.TableID].Columns[cb.ColumnID];

                        if (col.DataType == typeof(DateTime))
                            continue; //DateTime not supported by BIDS Helper except for count and distinct count aggregates

                        MeasureDataTypeOption dsvOption = GetMeasureDataTypeOptionForType(col.DataType);
                        if (dsvOption != null)
                        {
                            dsvColMaxValue = dsvOption.max;
                            dsvColAllowsDecimals = dsvOption.allowsDecimals;
                        }
                    }

                    double? total = (!Convert.IsDBNull(row[i * 5]) ? Convert.ToDouble(row[i * 5]) : (double?)null);
                    double? min = (!Convert.IsDBNull(row[i * 5 + 1]) ? Convert.ToDouble(row[i * 5 + 1]) : (double?)null);
                    double? max = (!Convert.IsDBNull(row[i * 5 + 2]) ? Convert.ToDouble(row[i * 5 + 2]) : (double?)null);
                    bool hasDecimals = (!Convert.IsDBNull(row[i * 5 + 3]) ? Convert.ToBoolean(row[i * 5 + 3]) : false);
                    bool hasMoreThan4Decimals = (!Convert.IsDBNull(row[i * 5 + 4]) ? Convert.ToBoolean(row[i * 5 + 4]) : false);

                    MeasureDataTypeOption oldDataTypeOption = GetMeasureDataTypeOptionForMeasure(m);
                    double recommendedMaxValue = double.MaxValue;

                    List<MeasureDataTypeOption> possible = new List<MeasureDataTypeOption>();
                    foreach (MeasureDataTypeOption option in dataTypeOptions)
                    {
                        if (
                         (total == null || (option.max >= total && option.min <= total))
                         && (max == null || option.max >= max)
                         && (min == null || option.min <= min)
                         && (!hasDecimals || option.allowsDecimals)
                         && (!hasMoreThan4Decimals || option.allowsMoreThan4Decimals)
                        )
                        {
                            possible.Add(option);
                            if (
                             (total == null || (total * FreeSpaceFactor < option.max && total * FreeSpaceFactor > option.min))
                             && option.max < recommendedMaxValue
                             && option.max >= dsvColMaxValue
                             && (dsvColAllowsDecimals == option.allowsDecimals)
                             && (option.oleDbType != OleDbType.Single) //never recommend Single
                            )
                            {
                                recommendedMaxValue = option.max;
                            }
                        }
                    }

                    foreach (MeasureDataTypeOption option in dataTypeOptions)
                    {
                        if (option.max == recommendedMaxValue)
                        {
                            Type dsvDataType = null; //don't bother getting the DSV datatype for count or DistinctCount measures
                            if (m.Source.Source is ColumnBinding && m.AggregateFunction != AggregationFunction.Count && m.AggregateFunction != AggregationFunction.DistinctCount)
                            {
                                ColumnBinding cb = GetColumnBindingForDataItem(m.Source);
                                DataColumn col = mg.Parent.DataSourceView.Schema.Tables[cb.TableID].Columns[cb.ColumnID];
                                dsvDataType = col.DataType;
                            }

                            MeasureHealthCheckResult result = new MeasureHealthCheckResult(m, FormatDouble(total), FormatDouble(min), FormatDouble(max), hasDecimals, hasMoreThan4Decimals, possible.ToArray(), option, oldDataTypeOption, dsvDataType);
                            measureResults.Add(result);

                            break;
                        }
                    }

                    i++;
                }
                else
                {
                    throw new Exception("Aggregation function " + m.AggregateFunction.ToString() + " not supported");
                }
            }
            BIDSHelper.SSAS.MeasureGroupHealthCheckForm form = new BIDSHelper.SSAS.MeasureGroupHealthCheckForm();
            form.measureDataTypeOptionBindingSource.DataSource = dataTypeOptions;
            form.measureHealthCheckResultBindingSource.DataSource = measureResults;
            form.Text = "Measure Group Health Check: " + mg.Name;
            DialogResult dialogResult = form.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                foreach (MeasureHealthCheckResult r in measureResults)
                {
                    if (r.CurrentDataType != r.DataType)
                    {
                        //save change
                        if (r.Measure.AggregateFunction == AggregationFunction.Count
                        || r.Measure.AggregateFunction == AggregationFunction.DistinctCount)
                        {
                            r.Measure.DataType = r.DataType.dataType;
                        }
                        else
                        {
                            r.Measure.Source.DataType = r.DataType.oleDbType;
                            r.Measure.DataType = MeasureDataType.Inherited;
                        }

                        //mark cube object as dirty
                        IComponentChangeService changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));
                        changesvc.OnComponentChanging(mg.Parent, null);
                        changesvc.OnComponentChanged(mg.Parent, null, null, null); //marks the cube designer as dirty
                    }
                }
            }
        }

        private static MeasureDataTypeOption GetMeasureDataTypeOptionForType(Type type)
        {
            foreach (MeasureDataTypeOption mdto in dataTypeOptions)
            {
                if (mdto.type == type)
                    return mdto;
            }
            return null;
        }

        private static MeasureDataTypeOption GetMeasureDataTypeOptionForMeasure(Measure m)
        {
            if (m.DataType != MeasureDataType.Inherited)
            {
                foreach (MeasureDataTypeOption mdto in dataTypeOptions)
                {
                    if (mdto.dataType == m.DataType)
                        return mdto;
                }
            }
            else
            {
                foreach (MeasureDataTypeOption mdto in dataTypeOptions)
                {
                    if (mdto.oleDbType == m.Source.DataType)
                        return mdto;
                }
            }
            return null;
        }

        //private static MeasureDataTypeOption GetMeasureDataTypeOptionForOleDbType(OleDbType oleDbType)
        //{
        //    foreach (MeasureDataTypeOption mdto in dataTypeOptions)
        //    {
        //        if (mdto.oleDbType == oleDbType)
        //            return mdto;
        //    }
        //    return null;
        //}

        public static string FormatDouble(double? d)
        {
            if (d == null)
                return string.Empty;
            else if (d > 999999999999999 || d < -999999999999999)
                return ((double)d).ToString("G");
            else if (d == Math.Floor((double)d))
                return ((double)d).ToString("n0");
            else
                return ((double)d).ToString("n4");
        }

        #region DSV Query Functions
        internal static string GetQueryDefinition(Database d, NamedComponent nc, Microsoft.AnalysisServices.Binding b, List<DataItem> columnsNeeded)
        {
            StringBuilder sQuery = new StringBuilder();
            if (b is DsvTableBinding)
            {
                DsvTableBinding oDsvTableBinding = (DsvTableBinding)b;
                DataSourceView oDSV = d.DataSourceViews[oDsvTableBinding.DataSourceViewID];
                DataTable oTable = oDSV.Schema.Tables[oDsvTableBinding.TableID];

                if (oTable == null)
                {
                    throw new Exception("DSV table " + oDsvTableBinding.TableID + " not found");
                }
                else if (!oTable.ExtendedProperties.ContainsKey("QueryDefinition") && oTable.ExtendedProperties.ContainsKey("DbTableName"))
                {
                    foreach (DataColumn oColumn in oTable.Columns)
                    {
                        bool bFoundColumn = false;
                        if (columnsNeeded == null)
                        {
                            bFoundColumn = true;
                        }
                        else
                        {
                            foreach (DataItem di in columnsNeeded)
                            {
                                if (GetColumnBindingForDataItem(di).TableID == oTable.TableName && GetColumnBindingForDataItem(di).ColumnID == oColumn.ColumnName)
                                {
                                    bFoundColumn = true;
                                }
                            }
                        }
                        if (bFoundColumn)
                        {
                            if (sQuery.Length == 0)
                            {
                                sQuery.Append("select ");
                            }
                            else
                            {
                                sQuery.Append(",");
                            }
                            if (!oColumn.ExtendedProperties.ContainsKey("ComputedColumnExpression"))
                            {
                                sQuery.Append(sq).Append((oColumn.ExtendedProperties["DbColumnName"] ?? oColumn.ColumnName).ToString()).AppendLine(fq);
                            }
                            else
                            {
                                sQuery.Append(oColumn.ExtendedProperties["ComputedColumnExpression"].ToString()).Append(" as ").Append(sq).Append((oColumn.ExtendedProperties["DbColumnName"] ?? oColumn.ColumnName).ToString()).AppendLine(fq);
                            }
                        }
                    }
                    if (sQuery.Length == 0)
                    {
                        throw new Exception("There was a problem constructing the query.");
                    }
                    sQuery.Append("from ");
                    if (oTable.ExtendedProperties.ContainsKey("DbSchemaName")) sQuery.Append(sq).Append(oTable.ExtendedProperties["DbSchemaName"].ToString()).Append(fq).Append(".");
                    sQuery.Append(sq).Append(oTable.ExtendedProperties["DbTableName"].ToString());
                    sQuery.Append(fq).Append(" ").Append(sq).Append(oTable.ExtendedProperties["FriendlyName"].ToString()).AppendLine(fq);
                }
                else if (oTable.ExtendedProperties.ContainsKey("QueryDefinition"))
                {
                    sQuery.AppendLine("select *");
                    sQuery.AppendLine("from (");
                    sQuery.AppendLine(oTable.ExtendedProperties["QueryDefinition"].ToString());
                    sQuery.AppendLine(") x");
                }
                else
                {
                    throw new Exception("Current the code does not support this type of query.");
                }
            }
            else if (b is QueryBinding)
            {
                QueryBinding oQueryBinding = (QueryBinding)b;
                sQuery.Append(oQueryBinding.QueryDefinition);
            }
            else if (b is ColumnBinding)
            {
                ColumnBinding cb = (ColumnBinding)b;
                object parent = cb.Parent;
                DataTable dt = d.DataSourceViews[0].Schema.Tables[cb.TableID];
                if (nc is DimensionAttribute)
                {
                    DimensionAttribute da = (DimensionAttribute)nc;

                    if (da.Parent.KeyAttribute.KeyColumns.Count != 1)
                    {
                        throw new Exception("Attribute " + da.Parent.KeyAttribute.Name + " has a composite key. This is not supported for a key attribute of a dimension.");
                    }

                    string sDsvID = ((DimensionAttribute)nc).Parent.DataSourceView.ID;
                    columnsNeeded.Add(new DataItem(cb.Clone()));
                    columnsNeeded.Add(da.Parent.KeyAttribute.KeyColumns[0]);
                    return GetQueryDefinition(d, nc, new DsvTableBinding(sDsvID, cb.TableID), columnsNeeded);
                }
                else
                {
                    throw new Exception("GetQueryDefinition does not currently support a ColumnBinding on a object of type " + nc.GetType().Name);
                }
            }
            else
            {
                throw new Exception("Not a supported query binding type: " + b.GetType().FullName);
            }

            return sQuery.ToString();
        }

        private static ColumnBinding GetColumnBindingForDataItem(DataItem di)
        {
            if (di.Source is ColumnBinding)
            {
                return (ColumnBinding)di.Source;
            }
            else
            {
                throw new Exception("Binding for column was unexpected type: " + di.Source.GetType().FullName);
            }
        }

        internal static string GetTableIdForDataItem(DataItem di)
        {
            if (di.Source is ColumnBinding)
            {
                return ((ColumnBinding)di.Source).TableID;
            }
            else if (di.Source is RowBinding)
            {
                return ((RowBinding)di.Source).TableID;
            }
            else
            {
                throw new Exception("GetTableIdForDataItem: Binding for column was unexpected type: " + di.Source.GetType().FullName);
            }
        }
        #endregion

        public class MeasureDataTypeOption
        {
            public MeasureDataType dataType;
            public Type type;
            public OleDbType oleDbType;
            public double min;
            public double max;
            public string displayMin;
            public string displayMax;
            public bool allowsDecimals;
            public bool allowsMoreThan4Decimals;
            public string limitations;
            public MeasureDataTypeOption(MeasureDataType dataType, Type type, double min, double max, bool allowsDecimals, bool allowsMoreThan4Decimals)
            {
                SetProperties(dataType, type, min, max, allowsDecimals, allowsMoreThan4Decimals);
            }
            public MeasureDataTypeOption(MeasureDataType dataType, Type type, double min, double max, bool allowsDecimals, bool allowsMoreThan4Decimals, string limitations)
            {
                SetProperties(dataType, type, min, max, allowsDecimals, allowsMoreThan4Decimals);
                this.limitations = limitations;
            }
            public MeasureDataTypeOption(MeasureDataType dataType, Type type, decimal min, decimal max, bool allowsDecimals, bool allowsMoreThan4Decimals)
            {
                SetProperties(dataType, type, (double)min, (double)max, allowsDecimals, allowsMoreThan4Decimals);
                this.displayMin = min.ToString("n4");
                this.displayMax = max.ToString("n4");
            }
            public MeasureDataTypeOption(MeasureDataType dataType, Type type, decimal min, decimal max, bool allowsDecimals, bool allowsMoreThan4Decimals, string limitations)
            {
                SetProperties(dataType, type, (double)min, (double)max, allowsDecimals, allowsMoreThan4Decimals);
                this.displayMin = min.ToString("n4");
                this.displayMax = max.ToString("n4");
                this.limitations = limitations;
            }
            private void SetProperties(MeasureDataType dataType, Type type, double min, double max, bool allowsDecimals, bool allowsMoreThan4Decimals)
            {
                this.dataType = dataType;
                this.type = type;

                if (dataType == MeasureDataType.Currency) //Type=Decimal isn't enough for it to correctly determine Currency
                    this.oleDbType = OleDbType.Currency;
                else
                    this.oleDbType = OleDbTypeConverter.GetRestrictedOleDbType(type);

                this.min = min;
                this.max = max;
                this.displayMin = FormatDouble(min);
                this.displayMax = FormatDouble(max);
                this.allowsDecimals = allowsDecimals;
                this.allowsMoreThan4Decimals = allowsMoreThan4Decimals;
            }
            public string DataTypeName
            {
                get { return dataType.ToString(); }
            }
            public override string ToString()
            {
                return this.DataTypeName;
            }
            public MeasureDataTypeOption SelfReference
            {
                get { return this; }
            }
        }

        public class MeasureHealthCheckResult
        {
            public MeasureHealthCheckResult(Measure Measure, string Total, string Min, string Max, bool HasDecimals, bool HasMoreThan4Decimals, MeasureDataTypeOption[] PossibleDataTypes, MeasureDataTypeOption RecommendedDataType, MeasureDataTypeOption CurrentDataType, Type dsvDataType)
            {
                this.Measure = Measure;
                this.mTotal = Total;
                this.mMin = Min;
                this.mMax = Max;
                this.mHasDecimals = HasDecimals;
                this.mHasMoreThan4Decimals = HasMoreThan4Decimals;
                this.PossibleDataTypes = PossibleDataTypes;
                this.RecommendedDataType = RecommendedDataType;
                this.mDataType = RecommendedDataType;
                this.mCurrentDataType = CurrentDataType;
                this.m_dsvDataType = dsvDataType;
            }

            private string mTotal;
            public string Total
            {
                get { return mTotal; }
            }

            private string mMin;
            public string Min
            {
                get { return mMin; }
            }

            private string mMax;
            public string Max
            {
                get { return mMax; }
            }

            private bool mHasMoreThan4Decimals;
            private bool mHasDecimals;
            public string HasDecimals
            {
                get
                {
                    if (mHasMoreThan4Decimals)
                        return "4+";
                    else
                        return mHasDecimals.ToString();
                }
            }

            private MeasureDataTypeOption mCurrentDataType;
            public MeasureDataTypeOption CurrentDataType
            {
                get { return mCurrentDataType; }
            }

            private Type m_dsvDataType;
            public string dsvDataType
            {
                get { return (m_dsvDataType != null ? m_dsvDataType.Name : string.Empty); }
            }

            public Measure Measure;
            public string MeasureName
            {
                get { return Measure.Name; }
            }
            public string AggregateFunction
            {
                get { return Measure.AggregateFunction.ToString(); }
            }

            public MeasureDataTypeOption[] PossibleDataTypes;
            private MeasureDataTypeOption mDataType;
            protected MeasureDataTypeOption RecommendedDataType;
            public MeasureDataTypeOption DataType
            {
                get { return mDataType; }
                set { mDataType = value; }
            }
        }

    }
}
