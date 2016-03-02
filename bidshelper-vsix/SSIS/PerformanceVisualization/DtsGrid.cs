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
        private ContextMenu contextMenuBreakdown;
        private string _ContextMenuDataFlowTaskID;
        public PerformanceTab ParentPerformanceTab;

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

            MenuItem menuBreakdown = new MenuItem("Component Performance Breakdown");
            menuBreakdown.Click += new EventHandler(menuBreakdown_Click);
            this.contextMenuBreakdown = new ContextMenu(new MenuItem[] { menuBreakdown });

            if (this.DataSource != null && this.DataSource is BindingSource)
            {
                List<IDtsGridRowData> list = ((BindingSource)this.DataSource).DataSource as List<IDtsGridRowData>;
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        if (list[0] is DtsPipelineTestDirector.DtsPipelineComponentTest)
                        {
                            nameDataGridViewTextBoxColumn.HeaderText = "Pipeline Component";
                        }
                    }
                }
            }
        }

        void menuBreakdown_Click(object sender, EventArgs e)
        {
            try
            {
                this.ParentPerformanceTab.SwitchToPipelineBreakdownGridMenuClicked(null, null);
                this.ParentPerformanceTab.BreakdownPipelinePerformance(this._ContextMenuDataFlowTaskID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            SetupComponent();
            int i = 0;

            bool bResetExpanded = true;
            if (this.Rows.Count > 0)
            {
                IDtsGridRowData dataPackage = ((BindingSource)this.DataSource)[0] as IDtsGridRowData;
                if (dataPackage != null && dataPackage is DtsObjectPerformance)
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
                    Icon iconPlusMinus = this.GetIsExpanded(e.RowIndex) ? BIDSHelper.Resources.Common.MinusSign : BIDSHelper.Resources.Common.PlusSign;
                    int y = Math.Max(0, (rectangle2.Y + (rectangle2.Height / 2)) - (iconPlusMinus.Height / 2) - 1);
                    e.Graphics.DrawIcon(iconPlusMinus, (rectangle2.X + (ganttRowData.Indent * INDENTATION_WIDTH)) - num4, y);
                }
                Icon icon = null;
                if (ganttRowData.Type == typeof(Microsoft.SqlServer.Dts.Runtime.Package))
                {
                    icon = BIDSHelper.Resources.Common.Package;
                }
                else if (ganttRowData.Type == typeof(DtsPipelinePerformance))
                {
                    icon = BIDSHelper.Resources.Versioned.DataFlow;
                }
                else if (ganttRowData.Type == typeof(ExecutionTree))
                {
                    icon = BIDSHelper.Resources.Common.TreeViewTab;
                }
                else if (ganttRowData.Type == typeof(PipelinePath))
                {
                    icon = BIDSHelper.Resources.Common.Path;
                }
                else if (ganttRowData.Type == typeof(DtsPipelineTestDirector.DtsPipelineComponentTest))
                {
                    DtsPipelineTestDirector.DtsPipelineComponentTest test = (DtsPipelineTestDirector.DtsPipelineComponentTest)ganttRowData;
                    if (test.TestType == DtsPipelineTestDirector.DtsPipelineComponentTestType.SourceOnly)
                        icon = BIDSHelper.Resources.Versioned.SourceComponent;
                    else if (test.TestType == DtsPipelineTestDirector.DtsPipelineComponentTestType.DestinationOnly)
                        icon = BIDSHelper.Resources.Versioned.DestinationComponent;
                    else
                        icon = BIDSHelper.Resources.Common.TaskSmall;
                }
                else
                {
                    icon = BIDSHelper.Resources.Common.TaskSmall;
                }
                if (icon != null)
                {
                    if (icon.Height > 16 || icon.Width > 16)
                    {
                        try
                        {
                            icon = new Icon(icon, new Size(16, 16));
                        }
                        catch { }
                    }
                    if (icon.Height > 16 || icon.Width > 16)
                    {
                        //properly deal with 32x32 data flow icon we can't get a 16x16 version with code above
                        int x = (rectangle2.X + (ganttRowData.Indent * INDENTATION_WIDTH)) - num4 + 16;
                        int y = Math.Max(0, (rectangle2.Y + (rectangle2.Height / 2)) - (16 / 2) - 1);
                        e.Graphics.DrawIcon(icon, new Rectangle(x, y, 16, 16));
                    }
                    else
                    {
                        int x = (rectangle2.X + (ganttRowData.Indent * INDENTATION_WIDTH)) - num4 + icon.Width;
                        int y = Math.Max(0, (rectangle2.Y + (rectangle2.Height / 2)) - (icon.Height / 2) - 1);
                        e.Graphics.DrawIcon(icon, x, y);
                    }
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
                        if ((e.X >= ((cell.Style.Padding.Left - BIDSHelper.Resources.Common.Package.Width) - INDENTATION_WIDTH)) && (e.X < (cell.Style.Padding.Left - BIDSHelper.Resources.Common.Package.Width)))
                        {
                            this.SetIsExpanded(e.RowIndex, !this.GetIsExpanded(e.RowIndex));
                            //this.InvokePaint(this, new PaintEventArgs(this.CreateGraphics(), this.GetColumnDisplayRectangle(1, false))); //don't think this is necessary
                        }
                    }
                }
            }
            else if (e.Button == MouseButtons.Right && e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                IDtsGridRowData rowData = ((BindingSource)this.DataSource)[e.RowIndex] as IDtsGridRowData;
                if (rowData is DtsPipelinePerformance && this.ParentPerformanceTab != null)
                {
                    Rectangle rect = this.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                    this._ContextMenuDataFlowTaskID = (rowData as DtsPipelinePerformance).ID;
                    this.contextMenuBreakdown.Show(this, new Point(e.X, rect.Y + e.Y));
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
