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
    public class DtsGrid : DataGridView
    {
        protected const int INDENTATION_WIDTH = 16;
        private bool?[] _Expanded;
        private string _LastPackageId;

        public DtsGrid()
        {
            InitializeComponent();
        }

        public DtsGrid(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        private void SetupComponent()
        {
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.AllowUserToResizeColumns = false;
            this.AllowUserToResizeRows = false;
            this.AutoGenerateColumns = false;
            this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ColumnHeadersVisible = false;
            this.ShowCellToolTips = false;

            System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            nameDataGridViewTextBoxColumn.HeaderText = "Task";
            nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            nameDataGridViewTextBoxColumn.ReadOnly = true;
            nameDataGridViewTextBoxColumn.Width = 400;
            nameDataGridViewTextBoxColumn.Frozen = true;
            this.Columns.Clear();
            this.Columns.Add(nameDataGridViewTextBoxColumn);

            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DefaultCellStyle = dataGridViewCellStyle1;
            this.MultiSelect = true;
            this.ReadOnly = true;
            this.RowHeadersVisible = false;
            this.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.DoubleBuffered = true;
            this.ScrollBars = ScrollBars.Both;
        }

        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            SetupComponent();
            int i = 0;

            bool bResetExpanded = true;
            if (this.Rows.Count > 0)
            {
                IDtsGridRowData dataPackage = ((BindingSource)this.DataSource)[0] as IDtsGridRowData;
                if (dataPackage != null)
                {
                    if (_LastPackageId == dataPackage.UniqueId && _Expanded.Length == this.Rows.Count)
                    {
                        bResetExpanded = false;
                    }
                    _LastPackageId = dataPackage.UniqueId;
                }
            }
            if (bResetExpanded)
            {
                _Expanded = new bool?[this.Rows.Count];
            }
            foreach (object oDataRow in ((BindingSource)this.DataSource))
            {
                IDtsGridRowData ganttRowData = oDataRow as IDtsGridRowData;
                if (ganttRowData != null)
                {
                    this.Rows[i].Cells[0].Style.Padding = new Padding((ganttRowData.Indent + 2) * INDENTATION_WIDTH, 2, 2, 2);
                }
                if (ganttRowData.Type == typeof(ExecutionTree) && _Expanded[i] != true)
                {
                    _Expanded[i] = false;
                    SetIsExpanded(i, false);
                }
                else if (ganttRowData.Type != typeof(ExecutionTree) && _Expanded[i] == false)
                {
                    _Expanded[i] = false;
                    SetIsExpanded(i, false);
                }
                else
                {
                    _Expanded[i] = true;
                }
                i++;
            }
            base.OnDataBindingComplete(e);
        }

        protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
        {
            if (this.DataSource == null) return;
            IDtsGridRowData ganttRowData = ((BindingSource)this.DataSource)[e.RowIndex] as IDtsGridRowData;
            if (ganttRowData != null)
            {
                Rectangle rectangle2 = base.GetCellDisplayRectangle(0, e.RowIndex, false);
                int num4 = this.Columns[0].Width - rectangle2.Width;
                e.Graphics.SetClip(rectangle2);
                if (ganttRowData.HasChildren)
                {
                    Icon iconPlusMinus = this.GetIsExpanded(e.RowIndex) ? Properties.Resources.MinusSign : Properties.Resources.PlusSign;
                    int y = Math.Max(0, (rectangle2.Y + (rectangle2.Height / 2)) - (iconPlusMinus.Height / 2) - 1);
                    e.Graphics.DrawIcon(iconPlusMinus, (rectangle2.X + (ganttRowData.Indent * INDENTATION_WIDTH)) - num4, y);
                }
                Icon icon = null;
                if (ganttRowData.Type == typeof(Microsoft.SqlServer.Dts.Runtime.Package))
                {
                    icon = Properties.Resources.Package;
                }
                else if (ganttRowData.Type == typeof(DtsPipelinePerformance))
                {
                    icon = Properties.Resources.DataFlow;
                }
                else if (ganttRowData.Type == typeof(ExecutionTree))
                {
                    icon = Properties.Resources.TreeViewTab;
                }
                else if (ganttRowData.Type == typeof(PipelinePath))
                {
                    icon = Properties.Resources.Path;
                }
                else
                {
                    icon = Properties.Resources.TaskSmall;
                }
                if (icon != null)
                {
                    int y = Math.Max(0, (rectangle2.Y + (rectangle2.Height / 2)) - (icon.Height / 2) - 1);
                    e.Graphics.DrawIcon(icon, (rectangle2.X + (ganttRowData.Indent * INDENTATION_WIDTH)) - num4 + icon.Width, y);
                }
                e.Graphics.ResetClip();
                Rows[e.RowIndex].Cells[0].Style.ForeColor = (ganttRowData.IsError ? Color.Red : Color.Black);
            }
            base.OnRowPostPaint(e);
        }

        protected override void OnCellMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if (((e.Button == MouseButtons.Left)) && ((e.ColumnIndex == 0) && (e.RowIndex >= 0)))
            {
                Point location = base.GetCellDisplayRectangle(0, e.RowIndex, false).Location;
                location.Offset(e.Location);
                HitTestInfo hitInfo = base.HitTest(location.X, location.Y);
                if (hitInfo.ColumnIndex != 0) return;
                DataGridViewCell cell = base.Rows[e.RowIndex].Cells[0];
                if (e.X < cell.Style.Padding.Left)
                {
                    IDtsGridRowData ganttRowData = ((BindingSource)this.DataSource)[e.RowIndex] as IDtsGridRowData;
                    if (ganttRowData != null && ganttRowData.HasChildren)
                    {
                        base.EndEdit();
                        if ((e.X >= ((cell.Style.Padding.Left - Properties.Resources.Package.Width) - INDENTATION_WIDTH)) && (e.X < (cell.Style.Padding.Left - Properties.Resources.Package.Width)))
                        {
                            this.SetIsExpanded(e.RowIndex, !this.GetIsExpanded(e.RowIndex));
                            //this.InvokePaint(this, new PaintEventArgs(this.CreateGraphics(), this.GetColumnDisplayRectangle(1, false))); //don't think this is necessary
                        }
                    }
                }
            }
            base.OnCellMouseUp(e);
        }

        private void SetIsExpanded(int RowIndex, bool Expanded)
        {
            this.SuspendLayout();
            try
            {
                if (this.Rows.Count > 0 && this.Rows[0].Cells.Count > 0)
                {
                    this.CurrentCell = this.Rows[0].Cells[0]; //helps prevent an error when you make the currently selected cell invisible
                }

                int iMasterRowIndex = RowIndex;
                _Expanded[RowIndex] = Expanded;
                IDtsGridRowData ganttRowData = (IDtsGridRowData)((BindingSource)this.DataSource)[RowIndex];
                int iIndent = ganttRowData.Indent;
                RowIndex++;
                ganttRowData = (IDtsGridRowData)((BindingSource)this.DataSource)[RowIndex];
                while (RowIndex < this.Rows.Count && ganttRowData.Indent > iIndent)
                {
                    if (!Expanded && this.Rows[RowIndex].Tag == null)
                    {
                        this.Rows[RowIndex].Visible = Expanded;
                        this.Rows[RowIndex].Tag = iMasterRowIndex;
                    }
                    else
                    {
                        if ((this.Rows[RowIndex].Tag as int?) == iMasterRowIndex)
                        {
                            this.Rows[RowIndex].Visible = Expanded;
                            this.Rows[RowIndex].Tag = null;
                        }
                    }
                    RowIndex++;
                    if (RowIndex < this.Rows.Count)
                        ganttRowData = (IDtsGridRowData)((BindingSource)this.DataSource)[RowIndex];
                    else
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error in SetIsExpanded for row " + RowIndex + ", Expanded=" + Expanded.ToString() + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                this.ResumeLayout();
            }
        }

        private bool GetIsExpanded(int RowIndex)
        {
            return (bool)(_Expanded[RowIndex] ?? false);
        }

        private int CurrentScrollingRowIndex
        {
            get
            {
                return Math.Max(FirstDisplayedScrollingRowIndex, 0);
            }
        }

        private int LastDisplayedRowIndex
        {
            get
            {
                int num = Math.Max(0, this.CurrentScrollingRowIndex);
                int rowIndex = num;
                int displayedRowCount = DisplayedRowCount(false);
                int num4 = 0;
                while (num4 < displayedRowCount)
                {
                    if (this.IsRowVisible(rowIndex))
                    {
                        num4++;
                    }
                    rowIndex++;
                }

                return rowIndex;
            }
        }

        protected bool IsRowVisible(int rowIndex)
        {
            return (((rowIndex < RowCount)) && Rows[rowIndex].Visible);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        protected System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        protected void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion    
    }
}
