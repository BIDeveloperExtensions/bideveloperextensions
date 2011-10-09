using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using Microsoft.AnalysisServices;

namespace BIDSHelper.SSAS
{
    public partial class M2MMatrixCompressionForm : Form
    {
        private bool _complete = false;
        private List<M2MMatrixCompressionPlugin.M2MMatrixCompressionStat> _list;

        public M2MMatrixCompressionForm()
        {
            InitializeComponent();
            this.Icon = BIDSHelper.Resources.Common.BIDSHelper;
        }

        private void M2mMatrixCompressionForm_Load(object sender, EventArgs e)
        {
            try
            {
                _list = (List<M2MMatrixCompressionPlugin.M2MMatrixCompressionStat>)((BindingSource)(this.dataGridView1.DataSource)).DataSource;
                FindWork();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private M2MMatrixCompressionPlugin.M2MMatrixCompressionStat currentStat;
        private System.Data.Common.DbCommand command;

        private void FindWork()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new MethodInvoker(delegate() { FindWork(); }));
                }
                else
                {
                    FormatGrid();
                    
                    int iTotal = 0;
                    int iComplete = 0;
                    foreach (M2MMatrixCompressionPlugin.M2MMatrixCompressionStat stat in _list)
                    {
                        iTotal++;
                        if (stat.Status == M2MMatrixCompressionPlugin.M2MMatrixCompressionStat.M2MMatrixCompressionStatStatus.Complete
                            || stat.Status == M2MMatrixCompressionPlugin.M2MMatrixCompressionStat.M2MMatrixCompressionStatStatus.Error)
                            iComplete++;
                        else if (!stat.RunQuery)
                            iTotal--;
                    }
                    this.Text = "BIDS Helper M2M Matrix Compression - " + iComplete + " of " + iTotal + " complete";

                    bool bFoundWork = false;
                    foreach (M2MMatrixCompressionPlugin.M2MMatrixCompressionStat stat in _list)
                    {
                        if (stat.RunQuery 
                            && stat.Status != M2MMatrixCompressionPlugin.M2MMatrixCompressionStat.M2MMatrixCompressionStatStatus.Complete
                            && stat.Status != M2MMatrixCompressionPlugin.M2MMatrixCompressionStat.M2MMatrixCompressionStatStatus.Error)
                        {
                            currentStat = stat;
                            stat.Status = M2MMatrixCompressionPlugin.M2MMatrixCompressionStat.M2MMatrixCompressionStatStatus.Running;
                            backgroundWorker = new System.ComponentModel.BackgroundWorker();
                            backgroundWorker.WorkerSupportsCancellation = true;
                            backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_DoWork);
                            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
                            backgroundWorker.RunWorkerAsync(stat);
                            bFoundWork = true;
                            break;
                        }
                    }
                    _complete = !bFoundWork;

                    this.dataGridView1.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                FindWork();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                MeasureGroup mg = currentStat.IntermediateMeasureGroup;
                DataSource oDataSource = mg.Parent.DataSource;
                DsvTableBinding oTblBinding = new DsvTableBinding(mg.Parent.DataSourceView.ID, MeasureGroupHealthCheckPlugin.GetTableIdForDataItem(mg.Measures[0].Source));
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
                        openedDataSourceConnection.QueryTimeOut = 0;
                    }
                    else
                    {
                        throw new Exception("Couldn't open connection from data source " + oDataSource.Name);
                    }

                    command = openedDataSourceConnection.CreateCommand();
                    command.CommandText = currentStat.SQL;

                    if (backgroundWorker.CancellationPending)
                    {
                        return;
                    }
                    
                    System.Data.Common.DbDataReader reader = null;
                    try
                    {
                        try
                        {
                            reader = command.ExecuteReader();
                        }
                        catch (Exception innerEx)
                        {
                            if (backgroundWorker.CancellationPending)
                            {
                                return;
                            }
                            else
                            {
                                throw innerEx;
                            }
                        }

                        if (!backgroundWorker.CancellationPending && reader.Read())
                        {
                            lock (command)
                            {
                                if (Convert.IsDBNull(reader["OriginalRecordCount"]))
                                    currentStat.OriginalRecordCount = null;
                                else
                                    currentStat.OriginalRecordCount = Convert.ToInt64(reader["OriginalRecordCount"]);

                                if (Convert.IsDBNull(reader["CompressedRecordCount"]))
                                    currentStat.CompressedRecordCount = null;
                                else
                                    currentStat.CompressedRecordCount = Convert.ToInt64(reader["CompressedRecordCount"]);

                                if (Convert.IsDBNull(reader["MatrixDimensionRecordCount"]))
                                    currentStat.MatrixDimensionRecordCount = null;
                                else
                                    currentStat.MatrixDimensionRecordCount = Convert.ToInt64(reader["MatrixDimensionRecordCount"]);

                                currentStat.Status = M2MMatrixCompressionPlugin.M2MMatrixCompressionStat.M2MMatrixCompressionStatStatus.Complete;
                            }

                            foreach (M2MMatrixCompressionPlugin.M2MMatrixCompressionStat stat in _list)
                            {
                                if (stat != currentStat && currentStat.IntermediateMeasureGroupName == stat.IntermediateMeasureGroupName && stat.SQL == currentStat.SQL)
                                {
                                    stat.OriginalRecordCount = currentStat.OriginalRecordCount;
                                    stat.CompressedRecordCount = currentStat.CompressedRecordCount;
                                    stat.MatrixDimensionRecordCount = currentStat.MatrixDimensionRecordCount;
                                    stat.Status = M2MMatrixCompressionPlugin.M2MMatrixCompressionStat.M2MMatrixCompressionStatStatus.Complete;
                                }
                            }
                        }
                    }
                    finally
                    {
                        try
                        {
                            if ((reader != null) && !reader.IsClosed)
                            {
                                reader.Close();
                            }
                        }
                        catch { }
                        command = null;
                    }
                }
                finally
                {
                    try
                    {
                        openedDataSourceConnection.Close();
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                currentStat.Error = ex.Message + "\r\n" + ex.StackTrace;
                foreach (M2MMatrixCompressionPlugin.M2MMatrixCompressionStat stat in _list)
                {
                    if (stat != currentStat && currentStat.IntermediateMeasureGroupName == stat.IntermediateMeasureGroupName && stat.SQL == currentStat.SQL)
                    {
                        stat.Error = currentStat.Error;
                    }
                }
            }
        }

        private void FormatGrid()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate() { FormatGrid(); }));
            }
            else
            {
                foreach (DataGridViewRow row in this.dataGridView1.Rows)
                {
                    M2MMatrixCompressionPlugin.M2MMatrixCompressionStat item = (M2MMatrixCompressionPlugin.M2MMatrixCompressionStat)row.DataBoundItem;
                    if (item.Error != null)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.ForeColor = Color.Red;
                            cell.ToolTipText = item.Error;
                        }
                    }
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex >= 0 && this.dataGridView1.Columns[e.ColumnIndex].DataPropertyName == "RunQuery")
                {
                    BindingSource bindingSource = this.dataGridView1.DataSource as BindingSource;
                    M2MMatrixCompressionPlugin.M2MMatrixCompressionStat item = bindingSource.Current as M2MMatrixCompressionPlugin.M2MMatrixCompressionStat;
                    item.RunQuery = !item.RunQuery;

                    //flip all with same SQL and intermediate MG name
                    foreach (M2MMatrixCompressionPlugin.M2MMatrixCompressionStat stat in _list)
                    {
                        if (item.IntermediateMeasureGroupName == stat.IntermediateMeasureGroupName && item.SQL == stat.SQL)
                        {
                            stat.RunQuery = item.RunQuery;
                        }
                    }
                    dataGridView1.Refresh();

                    if (item == currentStat)
                    {
                        if (!item.RunQuery)
                        {
                            backgroundWorker.CancelAsync();
                            try
                            {
                                lock (command)
                                {
                                    command.Cancel();
                                }
                            }
                            catch { }
                        }
                        else
                        {
                        }
                    }

                    if (_complete)
                    {
                        _complete = false;
                        FindWork();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void M2MMatrixCompressionForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                foreach (M2MMatrixCompressionPlugin.M2MMatrixCompressionStat stat in _list)
                {
                    stat.Status = M2MMatrixCompressionPlugin.M2MMatrixCompressionStat.M2MMatrixCompressionStatStatus.Complete;
                }

                try
                {
                    backgroundWorker.CancelAsync();
                }
                catch { }

                try
                {
                    lock (command)
                    {
                        command.Cancel();
                    }
                }
                catch { }
            }
            catch { }
        }

    }
}