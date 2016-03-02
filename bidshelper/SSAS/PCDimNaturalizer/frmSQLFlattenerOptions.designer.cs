namespace PCDimNaturalizer
{
    partial class frmSQLFlattenerOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSQLFlattenerOptions));
            this.label6 = new System.Windows.Forms.Label();
            this.numMinLevels = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tbAttributes = new System.Windows.Forms.TabPage();
            this.lbPCAttrCols = new System.Windows.Forms.CheckedListBox();
            this.chkAllAttributesPC = new System.Windows.Forms.CheckBox();
            this.tbHierarchies = new System.Windows.Forms.TabPage();
            this.lbNaturalAttrCols = new System.Windows.Forms.CheckedListBox();
            this.chkAllAttributesNatural = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numMinLevels)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tbAttributes.SuspendLayout();
            this.tbHierarchies.SuspendLayout();
            this.SuspendLayout();
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Minimum Levels:";
            // 
            // numMinLevels
            // 
            this.numMinLevels.Location = new System.Drawing.Point(124, 21);
            this.numMinLevels.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numMinLevels.Name = "numMinLevels";
            this.numMinLevels.Size = new System.Drawing.Size(47, 22);
            this.numMinLevels.TabIndex = 16;
            this.numMinLevels.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(58, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(575, 39);
            this.label7.TabIndex = 18;
            this.label7.Text = resources.GetString("label7.Text");
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tbAttributes);
            this.tabControl1.Controls.Add(this.tbHierarchies);
            this.tabControl1.Location = new System.Drawing.Point(30, 154);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(628, 238);
            this.tabControl1.TabIndex = 19;
            // 
            // tbAttributes
            // 
            this.tbAttributes.BackColor = System.Drawing.SystemColors.Menu;
            this.tbAttributes.Controls.Add(this.lbPCAttrCols);
            this.tbAttributes.Controls.Add(this.chkAllAttributesPC);
            this.tbAttributes.Location = new System.Drawing.Point(4, 22);
            this.tbAttributes.Name = "tbAttributes";
            this.tbAttributes.Padding = new System.Windows.Forms.Padding(3);
            this.tbAttributes.Size = new System.Drawing.Size(620, 212);
            this.tbAttributes.TabIndex = 1;
            this.tbAttributes.Text = "Parent Child Attributes";
            this.tbAttributes.UseVisualStyleBackColor = true;
            // 
            // lbPCAttrCols
            // 
            this.lbPCAttrCols.CheckOnClick = true;
            this.lbPCAttrCols.ColumnWidth = 280;
            this.lbPCAttrCols.Enabled = false;
            this.lbPCAttrCols.FormattingEnabled = true;
            this.lbPCAttrCols.Location = new System.Drawing.Point(17, 39);
            this.lbPCAttrCols.MultiColumn = true;
            this.lbPCAttrCols.Name = "lbPCAttrCols";
            this.lbPCAttrCols.Size = new System.Drawing.Size(561, 157);
            this.lbPCAttrCols.Sorted = true;
            this.lbPCAttrCols.TabIndex = 2;
            this.lbPCAttrCols.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lbAttributes_ItemCheck);
            // 
            // chkAllAttributesPC
            // 
            this.chkAllAttributesPC.AutoSize = true;
            this.chkAllAttributesPC.Checked = true;
            this.chkAllAttributesPC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllAttributesPC.Location = new System.Drawing.Point(15, 15);
            this.chkAllAttributesPC.Name = "chkAllAttributesPC";
            this.chkAllAttributesPC.Size = new System.Drawing.Size(348, 17);
            this.chkAllAttributesPC.TabIndex = 0;
            this.chkAllAttributesPC.Text = "Include all relational columns for use as parent child attributes";
            this.chkAllAttributesPC.UseVisualStyleBackColor = true;
            this.chkAllAttributesPC.CheckedChanged += new System.EventHandler(this.chkAllAttributes_CheckedChanged);
            // 
            // tbHierarchies
            // 
            this.tbHierarchies.BackColor = System.Drawing.SystemColors.Menu;
            this.tbHierarchies.Controls.Add(this.lbNaturalAttrCols);
            this.tbHierarchies.Controls.Add(this.chkAllAttributesNatural);
            this.tbHierarchies.Location = new System.Drawing.Point(4, 22);
            this.tbHierarchies.Name = "tbHierarchies";
            this.tbHierarchies.Padding = new System.Windows.Forms.Padding(3);
            this.tbHierarchies.Size = new System.Drawing.Size(620, 212);
            this.tbHierarchies.TabIndex = 0;
            this.tbHierarchies.Text = "Natural Attributes";
            this.tbHierarchies.UseVisualStyleBackColor = true;
            // 
            // lbNaturalAttrCols
            // 
            this.lbNaturalAttrCols.CheckOnClick = true;
            this.lbNaturalAttrCols.ColumnWidth = 280;
            this.lbNaturalAttrCols.Enabled = false;
            this.lbNaturalAttrCols.FormattingEnabled = true;
            this.lbNaturalAttrCols.Location = new System.Drawing.Point(17, 39);
            this.lbNaturalAttrCols.MultiColumn = true;
            this.lbNaturalAttrCols.Name = "lbNaturalAttrCols";
            this.lbNaturalAttrCols.Size = new System.Drawing.Size(561, 157);
            this.lbNaturalAttrCols.Sorted = true;
            this.lbNaturalAttrCols.TabIndex = 1;
            // 
            // chkAllAttributesNatural
            // 
            this.chkAllAttributesNatural.AutoSize = true;
            this.chkAllAttributesNatural.Checked = true;
            this.chkAllAttributesNatural.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllAttributesNatural.Location = new System.Drawing.Point(15, 15);
            this.chkAllAttributesNatural.Name = "chkAllAttributesNatural";
            this.chkAllAttributesNatural.Size = new System.Drawing.Size(323, 17);
            this.chkAllAttributesNatural.TabIndex = 0;
            this.chkAllAttributesNatural.Text = "Include all relational columns for use as natural attributes";
            this.chkAllAttributesNatural.UseVisualStyleBackColor = true;
            this.chkAllAttributesNatural.CheckedChanged += new System.EventHandler(this.chkAllHierarchies_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(27, 124);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(140, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "Hierachies and Attributes:";
            // 
            // btnOK
            // 
            this.btnOK.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(454, 415);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(96, 30);
            this.btnOK.TabIndex = 21;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(562, 415);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(96, 30);
            this.btnCancel.TabIndex = 22;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmSQLFlattenerOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(691, 457);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numMinLevels);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmSQLFlattenerOptions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Parent Child Dimension Naturalizer Options (SQL Source)";
            this.Load += new System.EventHandler(this.frmSQLFlattenerOptions_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numMinLevels)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tbAttributes.ResumeLayout(false);
            this.tbAttributes.PerformLayout();
            this.tbHierarchies.ResumeLayout(false);
            this.tbHierarchies.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.NumericUpDown numMinLevels;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage tbHierarchies;
        private System.Windows.Forms.TabPage tbAttributes;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.CheckBox chkAllAttributesNatural;
        public System.Windows.Forms.CheckBox chkAllAttributesPC;
        public System.Windows.Forms.CheckedListBox lbNaturalAttrCols;
        public System.Windows.Forms.CheckedListBox lbPCAttrCols;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.TabControl tabControl1;


    }
}