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
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using BIDSHelper.Core;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class DataTypeDiscrepancyCheckPlugin : BIDSHelperPluginBase
    {
        private const System.Reflection.BindingFlags getfieldflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;

        public DataTypeDiscrepancyCheckPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.DataTypeDiscrepancyCheckId);
        }

        public override string ShortName
        {
            get { return "DataTypeDiscrepancyCheck"; }
        }

        //public override int Bitmap
        //{
        //    get { return 163; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Data Type Discrepancy Check..."; }
        //}

        public override string FeatureName
        {
            get { return "Dimension Data Type Discrepancy Check"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; /*doesn't show anywhere*/ }
        }

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
        //}

        //public override string MenuName
        //{
        //    get { return "Folder Node"; }
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
            get { return "Allows you check that DSV data types match the data types on the KeyColumns and NameColumn of dimension attributes. As well as displaying discrepancies it allows you to easily fix them too."; }
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
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                // this figures out if this is the dimensions node without using the name
                // by checking the type of the first child item. 
                return (hierItem.UIHierarchyItems.Count >= 1 
                    && ((ProjectItem)hierItem.UIHierarchyItems.Item(1).Object).Object is Dimension);
                //return (hierItem.Name == "Dimensions" && ((ProjectItem)hierItem.Object).Object == null);
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
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                Database db = projItem.ContainingProject.Object as Database;

                listDiscrepancies = new List<DataTypeDiscrepancy>();
                foreach (Microsoft.AnalysisServices.Dimension d in db.Dimensions)
                {
                    foreach (DimensionAttribute da in d.Attributes)
                    {
                        foreach (DataItem di in da.KeyColumns)
                            CheckDataTypeDiscrepancies(di, ColumnType.KeyColumn);
                        CheckDataTypeDiscrepancies(da.NameColumn, ColumnType.NameColumn);
                        CheckDataTypeDiscrepancies(da.CustomRollupColumn, ColumnType.CustomRollup);
                        CheckDataTypeDiscrepancies(da.CustomRollupPropertiesColumn, ColumnType.CustomRollupProperties);
                        CheckDataTypeDiscrepancies(da.UnaryOperatorColumn, ColumnType.UnaryOperator);
                        CheckDataTypeDiscrepancies(da.ValueColumn, ColumnType.ValueColumn);
                        foreach (AttributeTranslation t in da.Translations)
                            CheckDataTypeDiscrepancies(t.CaptionColumn, ColumnType.TranslationCaption);
                    }
                }

                if (listDiscrepancies.Count == 0)
                {
                    MessageBox.Show("No dimension data type discrepancies found.");
                    return;
                }

                BIDSHelper.SSAS.DataTypeDiscrepancyCheckForm form = new BIDSHelper.SSAS.DataTypeDiscrepancyCheckForm();
                form.gridBindingSource.DataSource = listDiscrepancies;
                DialogResult dialogResult = form.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    foreach (DataTypeDiscrepancy discrepancy in listDiscrepancies)
                    {
                        if (discrepancy.SaveChange)
                        {
                            discrepancy.AnalysisServicesColumn.DataType = discrepancy.NewDataType;
                            discrepancy.AnalysisServicesColumn.DataSize = discrepancy.NewDataTypeLength;

                            //mark dimension designer as dirty
                            IComponentChangeService changesvc = (IComponentChangeService)discrepancy.DimensionAttribute.Parent.Site.GetService(typeof(IComponentChangeService));
                            changesvc.OnComponentChanging(discrepancy.DimensionAttribute.Parent, null);
                            changesvc.OnComponentChanged(discrepancy.DimensionAttribute.Parent, null, null, null);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private List<DataTypeDiscrepancy> listDiscrepancies;

        /*
         * cavaets:
         * doesn't handle varchar(max) DSV columns as the DSV doesn't report the length. The length needs to be set manually on the KeyColumn or NameColumn
         */
        private void CheckDataTypeDiscrepancies(DataItem di, ColumnType ColumnType)
        {
            if (di == null) return;

            ColumnBinding cb = di.Source as ColumnBinding;
            if (cb == null) return;

            IModelComponent parent = di.Parent;
            while (parent != null && !(parent is DimensionAttribute))
                parent = parent.Parent;
            DimensionAttribute da = (DimensionAttribute)parent;

            if (!da.Parent.DataSourceView.Schema.Tables.Contains(cb.TableID)) return;
            if (!da.Parent.DataSourceView.Schema.Tables[cb.TableID].Columns.Contains(cb.ColumnID)) return;
            DataColumn col = da.Parent.DataSourceView.Schema.Tables[cb.TableID].Columns[cb.ColumnID];

            if (ColumnType == ColumnType.NameColumn)
            {
                if (col.MaxLength <= 0) return;
                if (col.DataType != typeof(string)) return;
                if (di.DataType != OleDbType.WChar) return;
                if (col.MaxLength != di.DataSize)
                {
                    DataTypeDiscrepancy discrepancy = new DataTypeDiscrepancy();
                    discrepancy.AnalysisServicesColumnType = ColumnType;
                    discrepancy.AnalysisServicesColumn = di;
                    discrepancy.DimensionAttribute = da;
                    discrepancy.DSVColumn = col;
                    listDiscrepancies.Add(discrepancy);
                }
            }
            else //KeyColumn
            {
                bool bDiscrepancy = false;
                if (Microsoft.AnalysisServices.OleDbTypeConverter.Convert(di.DataType) != col.DataType && Microsoft.AnalysisServices.OleDbTypeConverter.GetRestrictedOleDbType(col.DataType) != di.DataType)
                    bDiscrepancy = true;
                if (di.DataSize >= 0 && col.MaxLength >= 0 && di.DataSize != col.MaxLength)
                    bDiscrepancy = true;
                if (bDiscrepancy)
                {
                    DataTypeDiscrepancy discrepancy = new DataTypeDiscrepancy();
                    discrepancy.AnalysisServicesColumnType = ColumnType;
                    discrepancy.AnalysisServicesColumn = di;
                    discrepancy.DimensionAttribute = da;
                    discrepancy.DSVColumn = col;
                    listDiscrepancies.Add(discrepancy);
                }
            }
        }

        #region DataTypeDiscrepancy class
        public enum ColumnType { KeyColumn, NameColumn, CustomRollup, CustomRollupProperties, SkippedLevels, TranslationCaption, UnaryOperator, ValueColumn };
        public class DataTypeDiscrepancy
        {
            public DimensionAttribute DimensionAttribute;
            public ColumnType AnalysisServicesColumnType;
            public DataColumn DSVColumn;
            public DataItem AnalysisServicesColumn;

            public string DimensionName
            {
                get { return DimensionAttribute.Parent.Name; }
            }

            public string AttributeName
            {
                get { return DimensionAttribute.Name; }
            }

            public string ColumnTypeName
            {
                get { return AnalysisServicesColumnType.ToString(); }
            }

            public string dsvColumnName
            {
                get { return DSVColumn.ColumnName; }
            }

            public string dsvDataTypeName
            {
                get { return DSVColumn.DataType.Name; }
            }

            public int dsvDataTypeLength
            {
                get { return DSVColumn.MaxLength; }
            }

            public string OldDataTypeName
            {
                get { return AnalysisServicesColumn.DataType.ToString(); }
            }

            public int OldDataTypeLength
            {
                get { return AnalysisServicesColumn.DataSize; }
            }

            public System.Data.OleDb.OleDbType NewDataType
            {
                get
                {
                    if (AnalysisServicesColumnType == ColumnType.KeyColumn)
                        return Microsoft.AnalysisServices.OleDbTypeConverter.GetRestrictedOleDbType(DSVColumn.DataType);
                    else
                        return System.Data.OleDb.OleDbType.WChar;
                }
            }

            public int NewDataTypeLength
            {
                get { return DSVColumn.MaxLength; }
            }

            private bool _SaveChange = true;
            public bool SaveChange
            {
                get { return _SaveChange; }
                set { _SaveChange = value; }
            }

            public bool MayRequireDimensionUsageChanges
            {
                get
                {
                    if (AnalysisServicesColumnType != ColumnType.KeyColumn) return false;
                    foreach (Cube c in DimensionAttribute.ParentDatabase.Cubes)
                    {
                        foreach (MeasureGroup mg in c.MeasureGroups)
                        {
                            foreach (MeasureGroupDimension mgd in mg.Dimensions)
                            {
                                if (mgd.Dimension.ID == DimensionAttribute.Parent.ID)
                                {
                                    if (mgd is RegularMeasureGroupDimension)
                                    {
                                        RegularMeasureGroupDimension rmgd = (RegularMeasureGroupDimension)mgd;
                                        MeasureGroupAttribute mga = rmgd.Attributes.Find(DimensionAttribute.ID);
                                        if (mga == null) continue;
                                        int iKeyIndex = DimensionAttribute.KeyColumns.IndexOf(AnalysisServicesColumn);
                                        if (iKeyIndex >= mga.KeyColumns.Count) continue;

                                        if (this.NewDataType != mga.KeyColumns[iKeyIndex].DataType)
                                            return true;
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }
            }
        }
        #endregion
    }
}