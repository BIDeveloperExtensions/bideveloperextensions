namespace BIDSHelper.SSAS
{
    partial class M2MMatrixCompressionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.RunQuery = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataMeasureGroupDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.intermediateMeasureGroupDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.originalRecordCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.compressedRecordCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.reductionPercentDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.matrixDimensionRecordCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m2mMatrixCompressionStatBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m2mMatrixCompressionStatBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoGenerateColumns = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RunQuery,
            this.Status,
            this.dataMeasureGroupDataGridViewTextBoxColumn,
            this.intermediateMeasureGroupDataGridViewTextBoxColumn,
            this.originalRecordCountDataGridViewTextBoxColumn,
            this.compressedRecordCountDataGridViewTextBoxColumn,
            this.reductionPercentDataGridViewTextBoxColumn,
            this.matrixDimensionRecordCountDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.m2mMatrixCompressionStatBindingSource;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridView1.Location = new System.Drawing.Point(13, 13);
            this.dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.Size = new System.Drawing.Size(807, 360);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // RunQuery
            // 
            this.RunQuery.DataPropertyName = "RunQuery";
            this.RunQuery.FalseValue = "false";
            this.RunQuery.HeaderText = "";
            this.RunQuery.Name = "RunQuery";
            this.RunQuery.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.RunQuery.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.RunQuery.ToolTipText = "Check the compressed size?";
            this.RunQuery.TrueValue = "true";
            this.RunQuery.Width = 35;
            // 
            // Status
            // 
            this.Status.DataPropertyName = "Status";
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.Width = 80;
            // 
            // dataMeasureGroupDataGridViewTextBoxColumn
            // 
            this.dataMeasureGroupDataGridViewTextBoxColumn.DataPropertyName = "DataMeasureGroupName";
            this.dataMeasureGroupDataGridViewTextBoxColumn.HeaderText = "Data Measure Group";
            this.dataMeasureGroupDataGridViewTextBoxColumn.Name = "dataMeasureGroupDataGridViewTextBoxColumn";
            this.dataMeasureGroupDataGridViewTextBoxColumn.ReadOnly = true;
            this.dataMeasureGroupDataGridViewTextBoxColumn.Width = 170;
            // 
            // intermediateMeasureGroupDataGridViewTextBoxColumn
            // 
            this.intermediateMeasureGroupDataGridViewTextBoxColumn.DataPropertyName = "IntermediateMeasureGroupName";
            this.intermediateMeasureGroupDataGridViewTextBoxColumn.HeaderText = "Intermediate Measure Group";
            this.intermediateMeasureGroupDataGridViewTextBoxColumn.Name = "intermediateMeasureGroupDataGridViewTextBoxColumn";
            this.intermediateMeasureGroupDataGridViewTextBoxColumn.ReadOnly = true;
            this.intermediateMeasureGroupDataGridViewTextBoxColumn.Width = 170;
            // 
            // originalRecordCountDataGridViewTextBoxColumn
            // 
            this.originalRecordCountDataGridViewTextBoxColumn.DataPropertyName = "OriginalRecordCount";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Format = "N0";
            dataGridViewCellStyle2.NullValue = null;
            this.originalRecordCountDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.originalRecordCountDataGridViewTextBoxColumn.HeaderText = "Original";
            this.originalRecordCountDataGridViewTextBoxColumn.Name = "originalRecordCountDataGridViewTextBoxColumn";
            this.originalRecordCountDataGridViewTextBoxColumn.ReadOnly = true;
            this.originalRecordCountDataGridViewTextBoxColumn.Width = 80;
            // 
            // compressedRecordCountDataGridViewTextBoxColumn
            // 
            this.compressedRecordCountDataGridViewTextBoxColumn.DataPropertyName = "CompressedRecordCount";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N0";
            this.compressedRecordCountDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.compressedRecordCountDataGridViewTextBoxColumn.HeaderText = "Compressed";
            this.compressedRecordCountDataGridViewTextBoxColumn.Name = "compressedRecordCountDataGridViewTextBoxColumn";
            this.compressedRecordCountDataGridViewTextBoxColumn.ReadOnly = true;
            this.compressedRecordCountDataGridViewTextBoxColumn.ToolTipText = "Row count after you compress the intermediate measure group";
            this.compressedRecordCountDataGridViewTextBoxColumn.Width = 80;
            // 
            // reductionPercentDataGridViewTextBoxColumn
            // 
            this.reductionPercentDataGridViewTextBoxColumn.DataPropertyName = "ReductionPercent";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.Format = "P1";
            this.reductionPercentDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.reductionPercentDataGridViewTextBoxColumn.HeaderText = "Reduction %";
            this.reductionPercentDataGridViewTextBoxColumn.Name = "reductionPercentDataGridViewTextBoxColumn";
            this.reductionPercentDataGridViewTextBoxColumn.ReadOnly = true;
            this.reductionPercentDataGridViewTextBoxColumn.ToolTipText = "(Original - Compressed) / Original";
            this.reductionPercentDataGridViewTextBoxColumn.Width = 80;
            // 
            // matrixDimensionRecordCountDataGridViewTextBoxColumn
            // 
            this.matrixDimensionRecordCountDataGridViewTextBoxColumn.DataPropertyName = "MatrixDimensionRecordCount";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Format = "N0";
            this.matrixDimensionRecordCountDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle5;
            this.matrixDimensionRecordCountDataGridViewTextBoxColumn.HeaderText = "Dimension";
            this.matrixDimensionRecordCountDataGridViewTextBoxColumn.Name = "matrixDimensionRecordCountDataGridViewTextBoxColumn";
            this.matrixDimensionRecordCountDataGridViewTextBoxColumn.ReadOnly = true;
            this.matrixDimensionRecordCountDataGridViewTextBoxColumn.ToolTipText = "The row count of the new matrix dimension";
            this.matrixDimensionRecordCountDataGridViewTextBoxColumn.Width = 80;
            // 
            // m2mMatrixCompressionStatBindingSource
            // 
            this.m2mMatrixCompressionStatBindingSource.DataSource = typeof(BIDSHelper.M2MMatrixCompressionPlugin.M2MMatrixCompressionStat);
            // 
            // M2MMatrixCompressionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 385);
            this.Controls.Add(this.dataGridView1);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(840, 200);
            this.Name = "M2MMatrixCompressionForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Many to Many Matrix Compression";
            this.Load += new System.EventHandler(this.M2mMatrixCompressionForm_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.M2MMatrixCompressionForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m2mMatrixCompressionStatBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        public System.Windows.Forms.BindingSource m2mMatrixCompressionStatBindingSource;
        private System.Windows.Forms.DataGridViewCheckBoxColumn RunQuery;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataMeasureGroupDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn intermediateMeasureGroupDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn originalRecordCountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn compressedRecordCountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn reductionPercentDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn matrixDimensionRecordCountDataGridViewTextBoxColumn;
    }
}