namespace PCDimNaturalizer
{
    partial class frmASFlattenerOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmASFlattenerOptions));
            this.trkActionLevel = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblOptionAction = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numMinLevels = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tbHierarchies = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.chkAllHierarchies = new System.Windows.Forms.CheckBox();
            this.tbAttributes = new System.Windows.Forms.TabPage();
            this.lbAttributes = new System.Windows.Forms.CheckedListBox();
            this.chkAllAttributes = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbHierarchies = new ctlFancyCheckedListBox();
            ((System.ComponentModel.ISupportInitialize)(this.trkActionLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinLevels)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tbHierarchies.SuspendLayout();
            this.tbAttributes.SuspendLayout();
            this.SuspendLayout();
            // 
            // trkActionLevel
            // 
            this.trkActionLevel.LargeChange = 1;
            this.trkActionLevel.Location = new System.Drawing.Point(107, 85);
            this.trkActionLevel.Maximum = 5;
            this.trkActionLevel.Minimum = 1;
            this.trkActionLevel.Name = "trkActionLevel";
            this.trkActionLevel.Size = new System.Drawing.Size(476, 42);
            this.trkActionLevel.TabIndex = 0;
            this.trkActionLevel.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trkActionLevel.Value = 4;
            this.trkActionLevel.ValueChanged += new System.EventHandler(this.trkActionLevel_Scroll);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(549, 132);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Process";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(415, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Add to cube(s)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(167, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Add to data source view";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(73, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Create SQL view";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(73, 170);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(552, 52);
            this.lblDescription.TabIndex = 5;
            this.lblDescription.Text = resources.GetString("lblDescription.Text");
            // 
            // lblOptionAction
            // 
            this.lblOptionAction.AutoSize = true;
            this.lblOptionAction.Location = new System.Drawing.Point(42, 25);
            this.lblOptionAction.Name = "lblOptionAction";
            this.lblOptionAction.Size = new System.Drawing.Size(162, 13);
            this.lblOptionAction.TabIndex = 6;
            this.lblOptionAction.Text = "Action level for naturalization:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(269, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(158, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Create naturalized dimension";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(42, 254);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Minimum Levels:";
            // 
            // numMinLevels
            // 
            this.numMinLevels.Location = new System.Drawing.Point(139, 250);
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
            this.label7.Location = new System.Drawing.Point(73, 285);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(575, 39);
            this.label7.TabIndex = 18;
            this.label7.Text = resources.GetString("label7.Text");
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tbHierarchies);
            this.tabControl1.Controls.Add(this.tbAttributes);
            this.tabControl1.Location = new System.Drawing.Point(45, 383);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(603, 238);
            this.tabControl1.TabIndex = 19;
            // 
            // tbHierarchies
            // 
            this.tbHierarchies.BackColor = System.Drawing.SystemColors.Menu;
            this.tbHierarchies.Controls.Add(this.richTextBox1);
            this.tbHierarchies.Controls.Add(this.lbHierarchies);
            this.tbHierarchies.Controls.Add(this.chkAllHierarchies);
            this.tbHierarchies.Location = new System.Drawing.Point(4, 22);
            this.tbHierarchies.Name = "tbHierarchies";
            this.tbHierarchies.Padding = new System.Windows.Forms.Padding(3);
            this.tbHierarchies.Size = new System.Drawing.Size(595, 212);
            this.tbHierarchies.TabIndex = 0;
            this.tbHierarchies.Text = "Hierarchies";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Menu;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(433, 16);
            this.richTextBox1.Multiline = false;
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.Size = new System.Drawing.Size(145, 17);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // chkAllHierarchies
            // 
            this.chkAllHierarchies.AutoSize = true;
            this.chkAllHierarchies.Checked = true;
            this.chkAllHierarchies.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllHierarchies.Location = new System.Drawing.Point(15, 15);
            this.chkAllHierarchies.Name = "chkAllHierarchies";
            this.chkAllHierarchies.Size = new System.Drawing.Size(422, 17);
            this.chkAllHierarchies.TabIndex = 0;
            this.chkAllHierarchies.Text = "Include all non-parent child user and attribute hierarchies in new dimension.";
            this.chkAllHierarchies.UseVisualStyleBackColor = true;
            this.chkAllHierarchies.CheckedChanged += new System.EventHandler(this.chkAllHierarchies_CheckedChanged);
            // 
            // tbAttributes
            // 
            this.tbAttributes.BackColor = System.Drawing.SystemColors.Menu;
            this.tbAttributes.Controls.Add(this.lbAttributes);
            this.tbAttributes.Controls.Add(this.chkAllAttributes);
            this.tbAttributes.Location = new System.Drawing.Point(4, 22);
            this.tbAttributes.Name = "tbAttributes";
            this.tbAttributes.Padding = new System.Windows.Forms.Padding(3);
            this.tbAttributes.Size = new System.Drawing.Size(595, 212);
            this.tbAttributes.TabIndex = 1;
            this.tbAttributes.Text = "Attributes";
            // 
            // lbAttributes
            // 
            this.lbAttributes.CheckOnClick = true;
            this.lbAttributes.ColumnWidth = 280;
            this.lbAttributes.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAttributes.FormattingEnabled = true;
            this.lbAttributes.Location = new System.Drawing.Point(17, 39);
            this.lbAttributes.MultiColumn = true;
            this.lbAttributes.Name = "lbAttributes";
            this.lbAttributes.Size = new System.Drawing.Size(561, 157);
            this.lbAttributes.Sorted = true;
            this.lbAttributes.TabIndex = 2;
            this.lbAttributes.SelectedIndexChanged += new System.EventHandler(this.lbAttributes_SelectedIndexChanged);
            this.lbAttributes.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lbAttributes_ItemCheck);
            // 
            // chkAllAttributes
            // 
            this.chkAllAttributes.AutoSize = true;
            this.chkAllAttributes.Checked = true;
            this.chkAllAttributes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllAttributes.Location = new System.Drawing.Point(15, 15);
            this.chkAllAttributes.Name = "chkAllAttributes";
            this.chkAllAttributes.Size = new System.Drawing.Size(247, 17);
            this.chkAllAttributes.TabIndex = 0;
            this.chkAllAttributes.Text = "Include all parent child hierarchy attributes";
            this.chkAllAttributes.UseVisualStyleBackColor = true;
            this.chkAllAttributes.CheckedChanged += new System.EventHandler(this.chkAllAttributes_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(42, 353);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(140, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "Hierachies and Attributes:";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(450, 635);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(96, 30);
            this.btnOK.TabIndex = 21;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(552, 635);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(96, 30);
            this.btnCancel.TabIndex = 22;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lbHierarchies
            // 
            this.lbHierarchies.CheckOnClick = true;
            this.lbHierarchies.ColumnWidth = 280;
            this.lbHierarchies.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbHierarchies.FormattingEnabled = true;
            this.lbHierarchies.Location = new System.Drawing.Point(17, 39);
            this.lbHierarchies.MultiColumn = true;
            this.lbHierarchies.Name = "lbHierarchies";
            this.lbHierarchies.Size = new System.Drawing.Size(561, 157);
            this.lbHierarchies.Sorted = true;
            this.lbHierarchies.TabIndex = 1;
            this.lbHierarchies.SelectedIndexChanged += new System.EventHandler(this.lbHierarchies_SelectedIndexChanged);
            this.lbHierarchies.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lbHierarchies_ItemCheck);
            // 
            // frmASFlattenerOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(691, 682);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numMinLevels);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblOptionAction);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.trkActionLevel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmASFlattenerOptions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Parent Child Dimension Naturalizer Options";
            this.Load += new System.EventHandler(this.frmASFlattenerOptions_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trkActionLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinLevels)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tbHierarchies.ResumeLayout(false);
            this.tbHierarchies.PerformLayout();
            this.tbAttributes.ResumeLayout(false);
            this.tbAttributes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblOptionAction;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.NumericUpDown numMinLevels;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage tbHierarchies;
        private System.Windows.Forms.TabPage tbAttributes;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.CheckBox chkAllAttributes;
        public ctlFancyCheckedListBox lbHierarchies;
        public System.Windows.Forms.CheckedListBox lbAttributes;
        public System.Windows.Forms.TrackBar trkActionLevel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        public System.Windows.Forms.CheckBox chkAllHierarchies;


    }
}