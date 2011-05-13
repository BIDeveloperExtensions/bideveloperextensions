namespace BIDSHelper.SSIS
{
    partial class ExpressionListControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExpressionListControl));
            this.expressionGrid = new System.Windows.Forms.DataGridView();
            this.ContainerID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Expression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EditorColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.expressionGrid)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // expressionGrid
            // 
            this.expressionGrid.AllowUserToAddRows = false;
            this.expressionGrid.AllowUserToDeleteRows = false;
            this.expressionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.expressionGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.expressionGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.expressionGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ContainerID,
            this.ObjectID,
            this.ObjectType,
            this.ObjectPath,
            this.ObjectName,
            this.Property,
            this.Expression,
            this.EditorColumn});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.expressionGrid.DefaultCellStyle = dataGridViewCellStyle3;
            this.expressionGrid.Location = new System.Drawing.Point(3, 28);
            this.expressionGrid.MultiSelect = false;
            this.expressionGrid.Name = "expressionGrid";
            this.expressionGrid.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.expressionGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.expressionGrid.RowHeadersVisible = false;
            this.expressionGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.expressionGrid.Size = new System.Drawing.Size(601, 298);
            this.expressionGrid.TabIndex = 0;
            this.expressionGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.expressionGrid_CellPainting);
            // 
            // ContainerID
            // 
            this.ContainerID.Frozen = true;
            this.ContainerID.HeaderText = "ContainerID";
            this.ContainerID.Name = "ContainerID";
            this.ContainerID.ReadOnly = true;
            // 
            // ObjectID
            // 
            this.ObjectID.Frozen = true;
            this.ObjectID.HeaderText = "Object ID";
            this.ObjectID.Name = "ObjectID";
            this.ObjectID.ReadOnly = true;
            // 
            // ObjectType
            // 
            this.ObjectType.Frozen = true;
            this.ObjectType.HeaderText = "Object Type";
            this.ObjectType.Name = "ObjectType";
            this.ObjectType.ReadOnly = true;
            this.ObjectType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // ObjectPath
            // 
            this.ObjectPath.Frozen = true;
            this.ObjectPath.HeaderText = "Path";
            this.ObjectPath.Name = "ObjectPath";
            this.ObjectPath.ReadOnly = true;
            this.ObjectPath.Width = 200;
            // 
            // ObjectName
            // 
            this.ObjectName.Frozen = true;
            this.ObjectName.HeaderText = "Object Name";
            this.ObjectName.Name = "ObjectName";
            this.ObjectName.ReadOnly = true;
            // 
            // Property
            // 
            this.Property.Frozen = true;
            this.Property.HeaderText = "Property";
            this.Property.Name = "Property";
            this.Property.ReadOnly = true;
            this.Property.Width = 160;
            // 
            // Expression
            // 
            this.Expression.Frozen = true;
            this.Expression.HeaderText = "Expression";
            this.Expression.Name = "Expression";
            this.Expression.ReadOnly = true;
            this.Expression.Width = 200;
            // 
            // EditorColumn
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.Format = "...";
            dataGridViewCellStyle2.NullValue = "...";
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            this.EditorColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.EditorColumn.HeaderText = "";
            this.EditorColumn.Name = "EditorColumn";
            this.EditorColumn.ReadOnly = true;
            this.EditorColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.EditorColumn.Text = "...";
            this.EditorColumn.ToolTipText = "Edit Expression";
            this.EditorColumn.Width = 20;
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRefresh,
            this.toolStripProgressBar});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(607, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "expressionTools";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(65, 22);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(100, 22);
            this.toolStripProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.toolStripProgressBar.Visible = false;
            // 
            // ExpressionListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.expressionGrid);
            this.Name = "ExpressionListControl";
            this.Size = new System.Drawing.Size(607, 329);
            ((System.ComponentModel.ISupportInitialize)(this.expressionGrid)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView expressionGrid;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.DataGridViewButtonColumn EditorColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ContainerID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Property;
        private System.Windows.Forms.DataGridViewTextBoxColumn Expression;
    }
}
