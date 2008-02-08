namespace BIDSHelper.SSAS
{
    partial class DataTypeDiscrepancyCheckForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.SaveChange = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dimensionNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AggregateFunction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HasDecimals = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dsvColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Total = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Min = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Max = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dsvDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.newDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.newLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBindingSource)).BeginInit();
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
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SaveChange,
            this.dimensionNameDataGridViewTextBoxColumn,
            this.AggregateFunction,
            this.HasDecimals,
            this.dsvColumnName,
            this.Total,
            this.Min,
            this.Max,
            this.dsvDataType,
            this.newDataType,
            this.newLength});
            this.dataGridView1.DataSource = this.gridBindingSource;
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridView1.Location = new System.Drawing.Point(13, 13);
            this.dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle12;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.Size = new System.Drawing.Size(991, 328);
            this.dataGridView1.TabIndex = 0;
            // 
            // SaveChange
            // 
            this.SaveChange.DataPropertyName = "SaveChange";
            this.SaveChange.HeaderText = "Save?";
            this.SaveChange.MinimumWidth = 40;
            this.SaveChange.Name = "SaveChange";
            this.SaveChange.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.SaveChange.Width = 45;
            // 
            // dimensionNameDataGridViewTextBoxColumn
            // 
            this.dimensionNameDataGridViewTextBoxColumn.DataPropertyName = "DimensionName";
            this.dimensionNameDataGridViewTextBoxColumn.HeaderText = "Dimension";
            this.dimensionNameDataGridViewTextBoxColumn.Name = "dimensionNameDataGridViewTextBoxColumn";
            this.dimensionNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.dimensionNameDataGridViewTextBoxColumn.Width = 130;
            // 
            // AggregateFunction
            // 
            this.AggregateFunction.DataPropertyName = "AttributeName";
            this.AggregateFunction.HeaderText = "Attribute";
            this.AggregateFunction.Name = "AggregateFunction";
            this.AggregateFunction.ReadOnly = true;
            this.AggregateFunction.Width = 130;
            // 
            // HasDecimals
            // 
            this.HasDecimals.DataPropertyName = "ColumnTypeName";
            this.HasDecimals.HeaderText = "Column Type";
            this.HasDecimals.Name = "HasDecimals";
            this.HasDecimals.ReadOnly = true;
            this.HasDecimals.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.HasDecimals.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.HasDecimals.Width = 75;
            // 
            // dsvColumnName
            // 
            this.dsvColumnName.DataPropertyName = "dsvColumnName";
            this.dsvColumnName.HeaderText = "DSV Column";
            this.dsvColumnName.Name = "dsvColumnName";
            this.dsvColumnName.ReadOnly = true;
            this.dsvColumnName.Width = 113;
            // 
            // Total
            // 
            this.Total.DataPropertyName = "dsvDataTypeName";
            this.Total.HeaderText = "DSV Data Type";
            this.Total.Name = "Total";
            this.Total.ReadOnly = true;
            this.Total.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Total.Width = 90;
            // 
            // Min
            // 
            this.Min.DataPropertyName = "dsvDataTypeLength";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Min.DefaultCellStyle = dataGridViewCellStyle8;
            this.Min.HeaderText = "DSV Length";
            this.Min.Name = "Min";
            this.Min.ReadOnly = true;
            this.Min.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Min.Width = 72;
            // 
            // Max
            // 
            this.Max.DataPropertyName = "OldDataTypeName";
            this.Max.HeaderText = "Old Data Type";
            this.Max.Name = "Max";
            this.Max.ReadOnly = true;
            this.Max.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Max.Width = 85;
            // 
            // dsvDataType
            // 
            this.dsvDataType.DataPropertyName = "OldDataTypeLength";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.dsvDataType.DefaultCellStyle = dataGridViewCellStyle9;
            this.dsvDataType.HeaderText = "Old Length";
            this.dsvDataType.Name = "dsvDataType";
            this.dsvDataType.ReadOnly = true;
            this.dsvDataType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dsvDataType.Width = 67;
            // 
            // newDataType
            // 
            this.newDataType.DataPropertyName = "NewDataType";
            this.newDataType.HeaderText = "New Data Type";
            this.newDataType.Name = "newDataType";
            this.newDataType.ReadOnly = true;
            this.newDataType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.newDataType.Width = 90;
            // 
            // newLength
            // 
            this.newLength.DataPropertyName = "NewDataTypeLength";
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.newLength.DefaultCellStyle = dataGridViewCellStyle10;
            this.newLength.HeaderText = "New Length";
            this.newLength.Name = "newLength";
            this.newLength.ReadOnly = true;
            this.newLength.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.newLength.Width = 75;
            // 
            // gridBindingSource
            // 
            this.gridBindingSource.DataSource = typeof(BIDSHelper.DataTypeDiscrepancyCheckPlugin.DataTypeDiscrepancy);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(928, 347);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okButton.Location = new System.Drawing.Point(847, 347);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 352);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(352, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Editing of Dimension Usage tab may be required after making this change";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.BackColor = System.Drawing.Color.Red;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(13, 347);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(18, 18);
            this.panel1.TabIndex = 7;
            // 
            // DataTypeDiscrepancyCheckForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(1016, 385);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.dataGridView1);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(900, 200);
            this.Name = "DataTypeDiscrepancyCheckForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dimension Data Type Discrepancy Check";
            this.Load += new System.EventHandler(this.DataTypeDiscrepancyCheckForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        public System.Windows.Forms.BindingSource gridBindingSource;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.DataGridViewCheckBoxColumn SaveChange;
        private System.Windows.Forms.DataGridViewTextBoxColumn dimensionNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn AggregateFunction;
        private System.Windows.Forms.DataGridViewTextBoxColumn HasDecimals;
        private System.Windows.Forms.DataGridViewTextBoxColumn dsvColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Total;
        private System.Windows.Forms.DataGridViewTextBoxColumn Min;
        private System.Windows.Forms.DataGridViewTextBoxColumn Max;
        private System.Windows.Forms.DataGridViewTextBoxColumn dsvDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn newDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn newLength;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
    }
}