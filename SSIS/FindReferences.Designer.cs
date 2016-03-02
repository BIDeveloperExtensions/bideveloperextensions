namespace BIDSHelper.SSIS
{
    partial class FindReferences
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.expressionGrid = new System.Windows.Forms.DataGridView();
            this.ContainerID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Expression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EditorColumn = new Core.DataGridViewButtonDisableColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.expressionGrid)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // expressionGrid
            // 
            this.expressionGrid.AllowUserToAddRows = false;
            this.expressionGrid.AllowUserToDeleteRows = false;
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
            this.expressionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.expressionGrid.Location = new System.Drawing.Point(0, 0);
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
            this.expressionGrid.Size = new System.Drawing.Size(863, 390);
            this.expressionGrid.TabIndex = 1;
            this.expressionGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.expressionGrid_CellPainting);
            // 
            // ContainerID
            // 
            this.ContainerID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ContainerID.Frozen = true;
            this.ContainerID.HeaderText = "ContainerID";
            this.ContainerID.Name = "ContainerID";
            this.ContainerID.ReadOnly = true;
            this.ContainerID.Width = 5;
            // 
            // ObjectID
            // 
            this.ObjectID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ObjectID.Frozen = true;
            this.ObjectID.HeaderText = "Object ID";
            this.ObjectID.Name = "ObjectID";
            this.ObjectID.ReadOnly = true;
            this.ObjectID.Width = 5;
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
            this.ObjectPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ObjectPath.HeaderText = "Path";
            this.ObjectPath.Name = "ObjectPath";
            this.ObjectPath.ReadOnly = true;
            // 
            // ObjectName
            // 
            this.ObjectName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ObjectName.HeaderText = "Object Name";
            this.ObjectName.Name = "ObjectName";
            this.ObjectName.ReadOnly = true;
            this.ObjectName.Width = 5;
            // 
            // Property
            // 
            this.Property.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Property.HeaderText = "Property";
            this.Property.Name = "Property";
            this.Property.ReadOnly = true;
            // 
            // Expression
            // 
            this.Expression.HeaderText = "Value";
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
            // panel1
            // 
            this.panel1.Controls.Add(this.expressionGrid);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(863, 390);
            this.panel1.TabIndex = 2;
            // 
            // FindReferences
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 414);
            this.Controls.Add(this.panel1);
            this.Name = "FindReferences";
            this.Text = "Find variable references";
            ((System.ComponentModel.ISupportInitialize)(this.expressionGrid)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView expressionGrid;
        private Core.DataGridViewButtonDisableColumn EditorColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Expression;
        private System.Windows.Forms.DataGridViewTextBoxColumn Property;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ContainerID;
        private System.Windows.Forms.Panel panel1;
    }
}