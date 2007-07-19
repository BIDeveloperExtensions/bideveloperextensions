namespace BIDSHelper
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ObjectType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Expression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EditorBtn = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ObjectType,
            this.ObjectName,
            this.Property,
            this.Expression,
            this.EditorBtn});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(447, 179);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // ObjectType
            // 
            this.ObjectType.Frozen = true;
            this.ObjectType.HeaderText = "Object Type";
            this.ObjectType.Name = "ObjectType";
            this.ObjectType.ReadOnly = true;
            this.ObjectType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ObjectType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
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
            // 
            // Expression
            // 
            this.Expression.Frozen = true;
            this.Expression.HeaderText = "Expression";
            this.Expression.Name = "Expression";
            // 
            // EditorBtn
            // 
            this.EditorBtn.HeaderText = "";
            this.EditorBtn.Name = "EditorBtn";
            this.EditorBtn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.EditorBtn.Width = 20;
            // 
            // ExpressionListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Name = "ExpressionListControl";
            this.Size = new System.Drawing.Size(447, 179);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewComboBoxColumn ObjectType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Property;
        private System.Windows.Forms.DataGridViewTextBoxColumn Expression;
        private System.Windows.Forms.DataGridViewButtonColumn EditorBtn;
    }
}
