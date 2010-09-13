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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExpressionListControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.expressionGrid = new System.Windows.Forms.DataGridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.ContainerID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.objectID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Expression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EditorBtn = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.expressionGrid)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // expressionGrid
            // 
            this.expressionGrid.AllowUserToAddRows = false;
            this.expressionGrid.AllowUserToDeleteRows = false;
            this.expressionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.expressionGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.expressionGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ContainerID,
            this.objectID,
            this.ObjectType,
            this.ObjectPath,
            this.ObjectName,
            this.Property,
            this.Expression,
            this.EditorBtn});
            this.expressionGrid.Location = new System.Drawing.Point(3, 28);
            this.expressionGrid.MultiSelect = false;
            this.expressionGrid.Name = "expressionGrid";
            this.expressionGrid.ReadOnly = true;
            this.expressionGrid.Size = new System.Drawing.Size(601, 298);
            this.expressionGrid.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRefresh,
            this.toolStripProgressBar1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(607, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "expressionTools";
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
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 22);
            this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.toolStripProgressBar1.Visible = false;
            // 
            // ContainerID
            // 
            this.ContainerID.Frozen = true;
            this.ContainerID.HeaderText = "ContainerID";
            this.ContainerID.Name = "ContainerID";
            this.ContainerID.ReadOnly = true;
            // 
            // objectID
            // 
            this.objectID.Frozen = true;
            this.objectID.HeaderText = "Object ID";
            this.objectID.Name = "objectID";
            this.objectID.ReadOnly = true;
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
            // EditorBtn
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.Format = "...";
            dataGridViewCellStyle1.NullValue = "...";
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            this.EditorBtn.DefaultCellStyle = dataGridViewCellStyle1;
            this.EditorBtn.HeaderText = "";
            this.EditorBtn.Name = "EditorBtn";
            this.EditorBtn.ReadOnly = true;
            this.EditorBtn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.EditorBtn.Text = "...";
            this.EditorBtn.ToolTipText = "Edit Expression";
            this.EditorBtn.Width = 20;
            // 
            // ExpressionListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.expressionGrid);
            this.Name = "ExpressionListControl";
            this.Size = new System.Drawing.Size(607, 329);
            ((System.ComponentModel.ISupportInitialize)(this.expressionGrid)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView expressionGrid;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ContainerID;
        private System.Windows.Forms.DataGridViewTextBoxColumn objectID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Property;
        private System.Windows.Forms.DataGridViewTextBoxColumn Expression;
        private System.Windows.Forms.DataGridViewButtonColumn EditorBtn;
    }
}
