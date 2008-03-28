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
    public class DtsStatisticsGrid : DtsGrid
    {
        public DtsStatisticsGrid()
        {
            InitializeComponent();
        }

        public DtsStatisticsGrid(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        private void SetupComponent()
        {
            this.AllowUserToResizeColumns = true;
            this.ColumnHeadersVisible = true;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            System.Windows.Forms.DataGridViewTextBoxColumn durationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            durationDataGridViewTextBoxColumn.HeaderText = "Duration";
            durationDataGridViewTextBoxColumn.DataPropertyName = "TotalSeconds";
            durationDataGridViewTextBoxColumn.Name = "Duration";
            durationDataGridViewTextBoxColumn.ReadOnly = true;
            durationDataGridViewTextBoxColumn.Width = 70;
            if (!this.Columns.Contains(durationDataGridViewTextBoxColumn.Name))
                this.Columns.Add(durationDataGridViewTextBoxColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn inRowSecDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            inRowSecDataGridViewTextBoxColumn.HeaderText = "Inbound Rows/Sec";
            inRowSecDataGridViewTextBoxColumn.DataPropertyName = "InboundRowsSec";
            inRowSecDataGridViewTextBoxColumn.Name = "InRowsSec";
            inRowSecDataGridViewTextBoxColumn.ReadOnly = true;
            inRowSecDataGridViewTextBoxColumn.Width = 120;
            inRowSecDataGridViewTextBoxColumn.DefaultCellStyle.Format = "F2";
            if (!this.Columns.Contains(inRowSecDataGridViewTextBoxColumn.Name))
                this.Columns.Add(inRowSecDataGridViewTextBoxColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn outRowSecDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            outRowSecDataGridViewTextBoxColumn.HeaderText = "Outbound Rows/Sec";
            outRowSecDataGridViewTextBoxColumn.DataPropertyName = "OutboundRowsSec";
            outRowSecDataGridViewTextBoxColumn.Name = "OutRowsSec";
            outRowSecDataGridViewTextBoxColumn.ReadOnly = true;
            outRowSecDataGridViewTextBoxColumn.Width = 120;
            outRowSecDataGridViewTextBoxColumn.DefaultCellStyle.Format = "F2";
            if (!this.Columns.Contains(outRowSecDataGridViewTextBoxColumn.Name))
                this.Columns.Add(outRowSecDataGridViewTextBoxColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn bufferRowCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            bufferRowCountDataGridViewTextBoxColumn.HeaderText = "Rows Per Buffer";
            bufferRowCountDataGridViewTextBoxColumn.DataPropertyName = "BufferRowCount";
            bufferRowCountDataGridViewTextBoxColumn.Name = "BufferRowCount";
            bufferRowCountDataGridViewTextBoxColumn.ReadOnly = true;
            bufferRowCountDataGridViewTextBoxColumn.Width = 100;
            bufferRowCountDataGridViewTextBoxColumn.DefaultCellStyle.Format = "F0";
            if (!this.Columns.Contains(bufferRowCountDataGridViewTextBoxColumn.Name))
                this.Columns.Add(bufferRowCountDataGridViewTextBoxColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn bytesPerRowDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            bytesPerRowDataGridViewTextBoxColumn.HeaderText = "Est. Bytes Per Row";
            bytesPerRowDataGridViewTextBoxColumn.DataPropertyName = "BufferEstimatedBytesPerRow";
            bytesPerRowDataGridViewTextBoxColumn.Name = "BytesPerRow";
            bytesPerRowDataGridViewTextBoxColumn.ReadOnly = true;
            bytesPerRowDataGridViewTextBoxColumn.Width = 110;
            bytesPerRowDataGridViewTextBoxColumn.DefaultCellStyle.Format = "F0";
            if (!this.Columns.Contains(bytesPerRowDataGridViewTextBoxColumn.Name))
                this.Columns.Add(bytesPerRowDataGridViewTextBoxColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn inKbDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            inKbDataGridViewTextBoxColumn.HeaderText = "Inbound Kb/Sec";
            inKbDataGridViewTextBoxColumn.DataPropertyName = "InboundKbSec";
            inKbDataGridViewTextBoxColumn.Name = "InboundKbSec";
            inKbDataGridViewTextBoxColumn.ReadOnly = true;
            inKbDataGridViewTextBoxColumn.Width = 100;
            inKbDataGridViewTextBoxColumn.DefaultCellStyle.Format = "F2";
            if (!this.Columns.Contains(inKbDataGridViewTextBoxColumn.Name))
                this.Columns.Add(inKbDataGridViewTextBoxColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn outKbDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            outKbDataGridViewTextBoxColumn.HeaderText = "Outbound Kb/Sec";
            outKbDataGridViewTextBoxColumn.DataPropertyName = "OutboundKbSec";
            outKbDataGridViewTextBoxColumn.Name = "OutboundKbSec";
            outKbDataGridViewTextBoxColumn.ReadOnly = true;
            outKbDataGridViewTextBoxColumn.Width = 100;
            outKbDataGridViewTextBoxColumn.DefaultCellStyle.Format = "F2";
            if (!this.Columns.Contains(outKbDataGridViewTextBoxColumn.Name))
                this.Columns.Add(outKbDataGridViewTextBoxColumn);
        }

        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            base.OnDataBindingComplete(e);
            SetupComponent();
        }

    }
}
