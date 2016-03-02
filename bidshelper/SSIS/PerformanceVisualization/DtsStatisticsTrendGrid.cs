using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BIDSHelper.SSIS.PerformanceVisualization
{
    public class DtsStatisticsTrendGrid : DtsGrid
    {
        private bool _AddNewColumnOnNextDataBinding = false;
        public bool AddNewColumnOnNextDataBinding
        {
            set { _AddNewColumnOnNextDataBinding = value; }
        }

        public DtsStatisticsTrendGrid()
        {
            InitializeComponent();
        }

        public DtsStatisticsTrendGrid(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        private void SetupComponent()
        {
            this.AllowUserToResizeColumns = true;
            this.ColumnHeadersVisible = true;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            for (int i = _History.Count - 1; i >= 0; i--)
            {
                System.Windows.Forms.DataGridViewTextBoxColumn statDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
                statDataGridViewTextBoxColumn.HeaderText = "Trial " + (i + 1);
                statDataGridViewTextBoxColumn.Name = "statDataGridViewTextBoxColumn" + i;
                statDataGridViewTextBoxColumn.ReadOnly = true;
                statDataGridViewTextBoxColumn.Width = 70;
                if (!this.Columns.Contains(statDataGridViewTextBoxColumn.Name))
                {
                    this.Columns.Add(statDataGridViewTextBoxColumn);
                }
            }
            if (_History.Count > 1)
            {
                System.Windows.Forms.DataGridViewTextBoxColumn avgDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
                avgDataGridViewTextBoxColumn.HeaderText = "Average";
                avgDataGridViewTextBoxColumn.Name = "avgDataGridViewTextBoxColumn";
                avgDataGridViewTextBoxColumn.ReadOnly = true;
                avgDataGridViewTextBoxColumn.Width = 80;
                if (!this.Columns.Contains(avgDataGridViewTextBoxColumn.Name))
                {
                    this.Columns.Add(avgDataGridViewTextBoxColumn);
                }
            }
        }

        private List<List<IDtsGridRowData>> _History = new List<List<IDtsGridRowData>>();

        /// <summary>
        /// Clears the trending columns to start over
        /// </summary>
        public void ClearHistory()
        {
            _History = new List<List<IDtsGridRowData>>();
        }

        //TODO: allow you to trend any of the other available stats, not just duration... add a dropdown button listing these stats in the button bar

        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            List<IDtsGridRowData> list = ((BindingSource)this.DataSource).DataSource as List<IDtsGridRowData>;
            if (list != null)
            {
                if (_AddNewColumnOnNextDataBinding)
                {
                    _History.Add(list);
                    _AddNewColumnOnNextDataBinding = false;
                }
                else
                {
                    _History[_History.Count - 1] = list;
                }
            }
            base.OnDataBindingComplete(e);
            SetupComponent();
        }


        protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
        {
            if (_History.Count == 0 || _History[_History.Count - 1].Count <= e.RowIndex) return;
            IDtsGridRowData ganttRowData = _History[_History.Count - 1][e.RowIndex] as IDtsGridRowData;
            if (ganttRowData == null) return;

            long iAvgTotal = 0;
            int iAvgCount = 0;

            for (int i = _History.Count - 1; i >= 0; i--)
            {
                //find matching UniqueId
                DataGridViewCell cell = Rows[e.RowIndex].Cells[_History.Count - i];
                IDtsGridRowData dataThisColumn = null;
                foreach (IDtsGridRowData data in _History[i])
                {
                    if (data.UniqueId == ganttRowData.UniqueId)
                    {
                        dataThisColumn = data;
                        int? iTotalSeconds = data.TotalSeconds;
                        if (iTotalSeconds != null)
                        {
                            cell.Value = ((float)iTotalSeconds).ToString("F0");
                            iAvgCount++;
                            iAvgTotal += (long)iTotalSeconds;
                        }

                        cell.Style.ForeColor = (data.IsError ? Color.Red : Color.Black);
                        break;
                    }
                }

                Bitmap bmp = null;
                if (i - 1 >= 0 && dataThisColumn != null)
                {
                    foreach (IDtsGridRowData data in _History[i - 1])
                    {
                        if (data.UniqueId == dataThisColumn.UniqueId)
                        {
                            int? iDataTotalSeconds = data.TotalSeconds;
                            int? iMainTotalSeconds = dataThisColumn.TotalSeconds;
                            if (iDataTotalSeconds == null || iMainTotalSeconds == null)
                                break;
                            else if (iMainTotalSeconds > iDataTotalSeconds)
                                bmp = BIDSHelper.Resources.Common.arrowUp;
                            else if (iMainTotalSeconds < iDataTotalSeconds)
                                bmp = BIDSHelper.Resources.Common.arrowDown;
                            else
                                bmp = BIDSHelper.Resources.Common.arrowFlat;
                            break;
                        }
                    }
                }

                if (_History.Count > 1)
                    cell.Style.Padding = new Padding(2, 2, 18, 2);

                if (bmp != null)
                {
                    Rectangle rectangle2 = base.GetCellDisplayRectangle(cell.ColumnIndex, e.RowIndex, false);
                    int num4 = this.Columns[cell.ColumnIndex].Width - rectangle2.Width;
                    e.Graphics.SetClip(rectangle2);
                    int y = Math.Max(0, (rectangle2.Y + (rectangle2.Height / 2)) - (bmp.Height / 2) - 1);
                    e.Graphics.DrawImage(bmp, rectangle2.Right - bmp.Width - num4 - 2, y);
                    e.Graphics.ResetClip();
                }
            }

            if (_History.Count > 1 && iAvgCount > 0)
            {
                //fill in average column
                DataGridViewCell cell = Rows[e.RowIndex].Cells[this.Columns.Count - 1];
                cell.Value = ((float)(((float)iAvgTotal) / iAvgCount)).ToString("F2");
            }

            base.OnRowPostPaint(e);
        }
    }
}
