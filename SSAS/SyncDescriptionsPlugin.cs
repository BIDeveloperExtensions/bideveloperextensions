using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.Data;
using System.ComponentModel.Design;
using BIDSHelper.Core;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class SyncDescriptionsPlugin : BIDSHelperPluginBase
    {
        public SyncDescriptionsPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.SyncDescriptionsId, typeof(Dimension));
        }

        public override string ShortName
        {
            get { return "SyncDescriptionsPlugin"; }
        }

        //public override int Bitmap
        //{
        //    get { return 223; }
        //}


        public override string FeatureName
        {
            get
            {
                return "Sync Descriptions";
            }
        }

        public override string ToolTip
        {
            get { return string.Empty; /*doesn't show anywhere*/ }
        }

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
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
            get { return "Sync descriptions from extended properties on SQL Sever tables to your dimensions."; }
        }

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;

                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(true, "Syncing Descriptions...", 0, ((System.Array)solExplorer.SelectedItems).Length);

                int iDimension = 0;
                int iDescriptionsSet = 0;
                bool bFirstDimension = true;

                foreach (object selectedItem in ((System.Array)solExplorer.SelectedItems))
                {
                    UIHierarchyItem hierItem = ((UIHierarchyItem)selectedItem);
                    ProjectItem projItem = (ProjectItem)hierItem.Object;
                    Dimension d = (Dimension)projItem.Object;

                    if (d.DataSource == null)
                    {
                        if (d.Source is TimeBinding)
                        {
                            MessageBox.Show("Sync Descriptions is not supported on a Server Time dimension.");
                        }
                        else
                        {
                            MessageBox.Show("The data source for this dimension is not set. Sync Descriptions cannot be run.");
                        }
                        continue;
                    }
                    else if (d.Source is DimensionBinding)
                    {
                        MessageBox.Show("Sync Descriptions is not supported on a linked dimension.");
                        continue;
                    }

                    iDescriptionsSet += SyncDescriptions(d, bFirstDimension, null, false);
                    bFirstDimension = false;
                    ApplicationObject.StatusBar.Progress(true, "Syncing Descriptions...", ++iDimension, ((System.Array)solExplorer.SelectedItems).Length);
                }

                MessageBox.Show("Set " + iDescriptionsSet + " descriptions successfully.", "BIDS Helper - Sync Descriptions");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(false, "Syncing Descriptions...", 2, 2);
            }
        }


        //making these static allows me not to have to change all the function signatures below... and it is fine as static because multiple dimension health checks can't be run in parallel
        private static string sq = "[";
        private static string fq = "]";
        private static Microsoft.DataWarehouse.Design.RDMSCartridge cartridge = null;
        private static string DBServerName = ""; //will say Oracle or Microsoft SQL Server

        private static string DescriptionPropertyName = "MS_Description";
        private static string[] OtherPropertyNamesToInclude = { "Example Values" }; //TODO: set these via UI
        private static bool OverwriteExistingDescriptions = false;

        internal static int SyncDescriptions(Dimension d, bool bPromptForProperties, IServiceProvider provider, bool bIsTabular)
        {
            int iUpdatedDescriptions = 0;
            DataSource dataSource = d.DataSource;

            ColumnBinding colDimensionKey = null;
#if !(YUKON || KATMAI)
            if (d.KeyAttribute.KeyColumns[0].Source is RowNumberBinding)
            {
                foreach (DimensionAttribute a in d.Attributes)
                {
                    if (a.KeyColumns != null && a.KeyColumns.Count > 0 && a.KeyColumns[0].Source is ColumnBinding)
                    {
                        colDimensionKey = GetColumnBindingForDataItem(a.KeyColumns[0]);
                        break;
                    }
                }
                if (colDimensionKey == null)
                {
                    throw new Exception("Couldn't find an attribute with a ColumnBinding, so couldn't find DSV table.");
                }
            }
            else
            {
                colDimensionKey = GetColumnBindingForDataItem(d.KeyAttribute.KeyColumns[0]);
            }
#else
            colDimensionKey = GetColumnBindingForDataItem(d.KeyAttribute.KeyColumns[0]);
#endif

            DataTable oDimensionKeyTable = d.DataSourceView.Schema.Tables[colDimensionKey.TableID];

            //if this is a Tabular model, the Dimension.DataSource may point at the default data source for the data source view
            if (oDimensionKeyTable.ExtendedProperties.ContainsKey("DataSourceID"))
            {
                dataSource = d.Parent.DataSources[oDimensionKeyTable.ExtendedProperties["DataSourceID"].ToString()];
            }

            IServiceProvider settingService = dataSource.Site;
            if (settingService == null)
            {
                settingService = provider;
            }

            Microsoft.DataWarehouse.Design.DataSourceConnection openedDataSourceConnection = Microsoft.DataWarehouse.DataWarehouseUtilities.GetOpenedDataSourceConnection((object)null, dataSource.ID, dataSource.Name, dataSource.ManagedProvider, dataSource.ConnectionString, settingService, false);
            try
            {
                if (d.MiningModelID != null) return iUpdatedDescriptions;

                try
                {
                    if (openedDataSourceConnection != null)
                    {
                        openedDataSourceConnection.QueryTimeOut = (int)dataSource.Timeout.TotalSeconds;
                    }
                }
                catch { }

                if (openedDataSourceConnection == null)
                {
                    throw new Exception("Unable to connect to data source [" + d.DataSource.Name + "].");
                }
                else
                {
                    sq = openedDataSourceConnection.Cartridge.IdentStartQuote;
                    fq = openedDataSourceConnection.Cartridge.IdentEndQuote;
                    DBServerName = openedDataSourceConnection.DBServerName;
                    cartridge = openedDataSourceConnection.Cartridge;

                    if (DBServerName != "Microsoft SQL Server")
                    {
                        MessageBox.Show("Data source [" + d.DataSource.Name + "] connects to " + DBServerName + " which may not be supported.");
                    }

                    String sql = "select distinct Name from sys.extended_properties order by Name";

                    if (bPromptForProperties)
                    {
                        DataSet dsExtendedProperties = new DataSet();
                        openedDataSourceConnection.Fill(dsExtendedProperties, sql);

                        SSAS.SyncDescriptionsForm form = new SSAS.SyncDescriptionsForm();
                        form.cmbDescriptionProperty.DataSource = dsExtendedProperties.Tables[0];
                        form.cmbDescriptionProperty.DisplayMember = "Name";
                        form.cmbDescriptionProperty.ValueMember = "Name";

                        foreach (DataRow row in dsExtendedProperties.Tables[0].Rows)
                        {
                            form.listOtherProperties.Items.Add(row["Name"].ToString());
                        }

                        DialogResult result = form.ShowDialog();

                        if (result != DialogResult.OK) return iUpdatedDescriptions;

                        DescriptionPropertyName = form.cmbDescriptionProperty.GetItemText(form.cmbDescriptionProperty.SelectedItem);
                        List<string> listOtherProperties = new List<string>();
                        for (int i = 0; i < form.listOtherProperties.CheckedItems.Count; i++)
                        {
                            listOtherProperties.Add(form.listOtherProperties.GetItemText(form.listOtherProperties.CheckedItems[i]));
                        }
                        OtherPropertyNamesToInclude = listOtherProperties.ToArray();
                        OverwriteExistingDescriptions = form.chkOverwriteExistingDescriptions.Checked;
                    }

                    if ((string.IsNullOrEmpty(d.Description) || OverwriteExistingDescriptions)
                    && (!oDimensionKeyTable.ExtendedProperties.ContainsKey("QueryDefinition") || bIsTabular) //Tabular always has a QueryDefinition, even when it's just a table binding
                    && oDimensionKeyTable.ExtendedProperties.ContainsKey("DbTableName")
                    && oDimensionKeyTable.ExtendedProperties.ContainsKey("DbSchemaName"))
                    {
                        sql = "SELECT PropertyName = p.name" + "\r\n"
                         + ",PropertyValue = CAST(p.value AS sql_variant)" + "\r\n"
                         + "FROM sys.all_objects AS tbl" + "\r\n"
                         + "INNER JOIN sys.schemas sch ON sch.schema_id = tbl.schema_id" + "\r\n"
                         + "INNER JOIN sys.extended_properties AS p ON p.major_id=tbl.object_id AND p.minor_id=0 AND p.class=1" + "\r\n"
                         + "where sch.name = '" + oDimensionKeyTable.ExtendedProperties["DbSchemaName"].ToString().Replace("'", "''") + "'\r\n"
                         + "and tbl.name = '" + oDimensionKeyTable.ExtendedProperties["DbTableName"].ToString().Replace("'", "''") + "'\r\n"
                         + "order by p.name";

                        string sNewDimensionDescription = "";
                        DataSet dsTableProperties = new DataSet();
                        openedDataSourceConnection.Fill(dsTableProperties, sql);

                        foreach (DataRow row in dsTableProperties.Tables[0].Rows)
                        {
                            if (string.Compare((string)row["PropertyName"], DescriptionPropertyName, true) == 0)
                            {
                                sNewDimensionDescription = (string)row["PropertyValue"];
                            }
                        }

                        foreach (DataRow row in dsTableProperties.Tables[0].Rows)
                        {
                            foreach (string sProp in OtherPropertyNamesToInclude)
                            {
                                if (string.Compare((string)row["PropertyName"], sProp, true) == 0 && !string.IsNullOrEmpty((string)row["PropertyValue"]))
                                {
                                    if (sNewDimensionDescription.Length > 0) sNewDimensionDescription += "\r\n";
                                    sNewDimensionDescription += (string)row["PropertyName"] + ": " + (string)row["PropertyValue"];
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(sNewDimensionDescription))
                        {
                            d.Description = sNewDimensionDescription;
                            iUpdatedDescriptions++;
                        }
                    }

                    foreach (DimensionAttribute a in d.Attributes)
                    {
                        ColumnBinding col = null;

#if !(YUKON || KATMAI)
                        if (a.Type == AttributeType.RowNumber)
                        {
                            continue;
                        }
#endif
                        if (a.NameColumn != null)
                        {
                            if (!(a.NameColumn.Source is ColumnBinding))
                            {
                                continue;
                            }
                            col = GetColumnBindingForDataItem(a.NameColumn);
                        }
                        else if (a.KeyColumns.Count == 1)
                        {
                            if (!(a.KeyColumns[0].Source is ColumnBinding))
                            {
                                continue;
                            }
                            col = GetColumnBindingForDataItem(a.KeyColumns[0]);
                        }
                        else
                        {
                            continue; //skip this attribute since we don't know which column to use
                        }
                        DataTable oDsvTable = d.DataSourceView.Schema.Tables[col.TableID];

                        if ((string.IsNullOrEmpty(a.Description) || OverwriteExistingDescriptions)
                        && (!oDsvTable.ExtendedProperties.ContainsKey("QueryDefinition") || bIsTabular)
                        && oDsvTable.ExtendedProperties.ContainsKey("DbTableName")
                        && oDsvTable.ExtendedProperties.ContainsKey("DbSchemaName"))
                        {
                            sql = "SELECT PropertyName = p.name" + "\r\n"
                             + ",PropertyValue = CAST(p.value AS sql_variant)" + "\r\n"
                             + "FROM sys.all_objects AS tbl" + "\r\n"
                             + "INNER JOIN sys.schemas sch ON sch.schema_id = tbl.schema_id" + "\r\n"
                             + "INNER JOIN sys.all_columns AS clmns ON clmns.object_id=tbl.object_id" + "\r\n"
                             + "INNER JOIN sys.extended_properties AS p ON p.major_id=clmns.object_id AND p.minor_id=clmns.column_id AND p.class=1" + "\r\n"
                             + "where sch.name = '" + oDsvTable.ExtendedProperties["DbSchemaName"].ToString().Replace("'", "''") + "'\r\n"
                             + "and tbl.name = '" + oDsvTable.ExtendedProperties["DbTableName"].ToString().Replace("'", "''") + "'\r\n"
                             + "and clmns.name = '" + oDsvTable.Columns[col.ColumnID].ColumnName.Replace("'", "''") + "'\r\n"
                             + "order by p.name";

                            string sNewDescription = "";
                            DataSet dsProperties = new DataSet();
                            openedDataSourceConnection.Fill(dsProperties, sql);

                            foreach (DataRow row in dsProperties.Tables[0].Rows)
                            {
                                if (string.Compare((string)row["PropertyName"], DescriptionPropertyName, true) == 0)
                                {
                                    sNewDescription = (string)row["PropertyValue"];
                                }
                            }

                            foreach (DataRow row in dsProperties.Tables[0].Rows)
                            {
                                foreach (string sProp in OtherPropertyNamesToInclude)
                                {
                                    if (string.Compare((string)row["PropertyName"], sProp, true) == 0 && !string.IsNullOrEmpty((string)row["PropertyValue"]))
                                    {
                                        if (sNewDescription.Length > 0) sNewDescription += "\r\n";
                                        sNewDescription += (string)row["PropertyName"] + ": " + (string)row["PropertyValue"];
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(sNewDescription))
                            {
                                a.Description = sNewDescription;
                                iUpdatedDescriptions++;
                            }
                        }
                    }


                    if (d.Site != null) //if not Tabular
                    {
                        //mark dimension as dirty
                        IComponentChangeService changesvc = (IComponentChangeService)d.Site.GetService(typeof(IComponentChangeService));
                        changesvc.OnComponentChanging(d, null);
                        changesvc.OnComponentChanged(d, null, null, null);
                    }
                }
            }
            finally
            {
                try
                {
                    cartridge = null;
                    openedDataSourceConnection.Close();
                }
                catch { }
            }
            return iUpdatedDescriptions;
        }

        internal static int SyncDescriptions(Microsoft.AnalysisServices.BackEnd.DataModelingTable d, bool bPromptForProperties)
        {
            int iUpdatedDescriptions = 0;

            Microsoft.AnalysisServices.BackEnd.EditMappingUtility util = new Microsoft.AnalysisServices.BackEnd.EditMappingUtility(d.Sandbox);

            var conn = ((Microsoft.AnalysisServices.BackEnd.RelationalDataStorage)((util.GetDataSourceConnection(util.GetDataSourceID(d.Id), d.Sandbox)))).DataSourceConnection;
            conn.Open();
            System.Data.Common.DbCommand cmd = conn.CreateCommand();

            string sDBTableName = d.SourceTableName;

            sq = conn.Cartridge.IdentStartQuote;
            fq = conn.Cartridge.IdentEndQuote;
            //cartridge = conn.Cartridge;


            if (conn.SourceType != Microsoft.AnalysisServices.BackEnd.DataSourceType.SqlServer && conn.SourceType != Microsoft.AnalysisServices.BackEnd.DataSourceType.SqlAzure)
            {
                MessageBox.Show("Data source [" + conn.ConnectionName + "] connects to " + conn.SourceType.ToString() + " which may not be supported.");
            }

            String sql = "select distinct Name from sys.extended_properties order by Name";

            if (bPromptForProperties)
            {

                SSAS.SyncDescriptionsForm form = new SSAS.SyncDescriptionsForm();

                cmd.CommandText = sql;
                System.Data.Common.DbDataReader reader = cmd.ExecuteReader();
                List<string> listNames = new List<string>();
                while (reader.Read())
                {
                    listNames.Add(Convert.ToString(reader["Name"]));
                    form.listOtherProperties.Items.Add(Convert.ToString(reader["Name"]));
                }
                reader.Close();

                form.cmbDescriptionProperty.DataSource = listNames;

                DialogResult result = form.ShowDialog();

                if (result != DialogResult.OK) return iUpdatedDescriptions;

                DescriptionPropertyName = form.cmbDescriptionProperty.GetItemText(form.cmbDescriptionProperty.SelectedItem);
                List<string> listOtherProperties = new List<string>();
                for (int i = 0; i < form.listOtherProperties.CheckedItems.Count; i++)
                {
                    listOtherProperties.Add(form.listOtherProperties.GetItemText(form.listOtherProperties.CheckedItems[i]));
                }
                OtherPropertyNamesToInclude = listOtherProperties.ToArray();
                OverwriteExistingDescriptions = form.chkOverwriteExistingDescriptions.Checked;
            }

            if ((string.IsNullOrEmpty(d.Description) || OverwriteExistingDescriptions)
            && !string.IsNullOrEmpty(sDBTableName))
            {
                sql = "SELECT PropertyName = p.name" + "\r\n"
                 + ",PropertyValue = CAST(p.value AS sql_variant)" + "\r\n"
                 + "FROM sys.all_objects AS tbl" + "\r\n"
                 + "INNER JOIN sys.schemas sch ON sch.schema_id = tbl.schema_id" + "\r\n"
                 + "INNER JOIN sys.extended_properties AS p ON p.major_id=tbl.object_id AND p.minor_id=0 AND p.class=1" + "\r\n"
                 //+ "where sch.name = '" + oDimensionKeyTable.ExtendedProperties["DbSchemaName"].ToString().Replace("'", "''") + "'\r\n"
                 + "where tbl.object_id = object_id('" + sDBTableName.Replace("'", "''") + "')\r\n"
                 + "order by p.name";

                string sNewDimensionDescription = "";
                //DataSet dsTableProperties = new DataSet();
                cmd.CommandText = sql;
                System.Data.Common.DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (string.Compare((string)reader["PropertyName"], DescriptionPropertyName, true) == 0)
                    {
                        sNewDimensionDescription = (string)reader["PropertyValue"];
                    }
                }
                reader.Close();

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    foreach (string sProp in OtherPropertyNamesToInclude)
                    {
                        if (string.Compare((string)reader["PropertyName"], sProp, true) == 0 && !string.IsNullOrEmpty((string)reader["PropertyValue"]))
                        {
                            if (sNewDimensionDescription.Length > 0) sNewDimensionDescription += "\r\n";
                            sNewDimensionDescription += (string)reader["PropertyName"] + ": " + (string)reader["PropertyValue"];
                        }
                    }
                }
                reader.Close();



                if (!string.IsNullOrEmpty(sNewDimensionDescription))
                {
                    d.Description = sNewDimensionDescription;
                    iUpdatedDescriptions++;
                }
            }

            foreach (Microsoft.AnalysisServices.BackEnd.DataModelingColumn a in d.Columns)
            {
                if (a.IsRowNumber || a.IsCalculated)
                {
                    continue;
                }
                if ((string.IsNullOrEmpty(a.Description) || OverwriteExistingDescriptions)
                && (!string.IsNullOrEmpty(sDBTableName)))
                {
                    sql = "SELECT PropertyName = p.name" + "\r\n"
                     + ",PropertyValue = CAST(p.value AS sql_variant)" + "\r\n"
                     + "FROM sys.all_objects AS tbl" + "\r\n"
                     + "INNER JOIN sys.schemas sch ON sch.schema_id = tbl.schema_id" + "\r\n"
                     + "INNER JOIN sys.all_columns AS clmns ON clmns.object_id=tbl.object_id" + "\r\n"
                     + "INNER JOIN sys.extended_properties AS p ON p.major_id=clmns.object_id AND p.minor_id=clmns.column_id AND p.class=1" + "\r\n"
                     //+ "where sch.name = '" + oDsvTable.ExtendedProperties["DbSchemaName"].ToString().Replace("'", "''") + "'\r\n"
                     + "where tbl.object_id = object_id('" + sDBTableName.Replace("'", "''") + "')\r\n"
                     + "and clmns.name = '" + a.DBColumnName.Replace("'", "''") + "'\r\n"
                     + "order by p.name";

                    string sNewDescription = "";
                    cmd.CommandText = sql;
                    System.Data.Common.DbDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (string.Compare((string)reader["PropertyName"], DescriptionPropertyName, true) == 0)
                        {
                            sNewDescription = (string)reader["PropertyValue"];
                        }
                    }
                    reader.Close();

                    cmd.CommandText = sql;
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        foreach (string sProp in OtherPropertyNamesToInclude)
                        {
                            if (string.Compare((string)reader["PropertyName"], sProp, true) == 0 && !string.IsNullOrEmpty((string)reader["PropertyValue"]))
                            {
                                if (sNewDescription.Length > 0) sNewDescription += "\r\n";
                                sNewDescription += (string)reader["PropertyName"] + ": " + (string)reader["PropertyValue"];
                            }
                        }
                    }
                    reader.Close();

                    if (!string.IsNullOrEmpty(sNewDescription))
                    {
                        a.Description = sNewDescription;
                        iUpdatedDescriptions++;
                    }
                }
            }


            return iUpdatedDescriptions;
        }

        private static ColumnBinding GetColumnBindingForDataItem(DataItem di)
        {
            if (di.Source is ColumnBinding)
            {
                return (ColumnBinding)di.Source;
            }
            else
            {
                throw new Exception("Binding for column was unexpected type.");
            }
        }

    }
}