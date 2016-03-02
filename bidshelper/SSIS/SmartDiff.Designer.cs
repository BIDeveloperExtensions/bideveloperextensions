namespace BIDSHelper.SSIS
{
    partial class SmartDiff
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
            this.txtCompare = new System.Windows.Forms.TextBox();
            this.txtTo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.browseContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.sourceSafeFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.checkIgnoreWhiteSpace = new System.Windows.Forms.CheckBox();
            this.checkIgnoreCase = new System.Windows.Forms.CheckBox();
            this.checkIgnoreEOL = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.browseContextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtCompare
            // 
            this.txtCompare.Location = new System.Drawing.Point(70, 18);
            this.txtCompare.Name = "txtCompare";
            this.txtCompare.Size = new System.Drawing.Size(291, 20);
            this.txtCompare.TabIndex = 0;
            // 
            // txtTo
            // 
            this.txtTo.Location = new System.Drawing.Point(70, 44);
            this.txtTo.Name = "txtTo";
            this.txtTo.Size = new System.Drawing.Size(291, 20);
            this.txtTo.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Compare:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "To:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // browseContextMenuStrip1
            // 
            this.browseContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.windowsFolderToolStripMenuItem,
            this.sourceSafeFolderToolStripMenuItem});
            this.browseContextMenuStrip1.Name = "browseContextMenuStrip1";
            this.browseContextMenuStrip1.ShowImageMargin = false;
            this.browseContextMenuStrip1.Size = new System.Drawing.Size(187, 48);
            // 
            // sourceSafeFolderToolStripMenuItem
            // 
            this.sourceSafeFolderToolStripMenuItem.Name = "sourceSafeFolderToolStripMenuItem";
            this.sourceSafeFolderToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.sourceSafeFolderToolStripMenuItem.Text = "Source Control Versions...";
            this.sourceSafeFolderToolStripMenuItem.Click += new System.EventHandler(this.sourceSafeFolderToolStripMenuItem_Click);
            // 
            // windowsFolderToolStripMenuItem
            // 
            this.windowsFolderToolStripMenuItem.Name = "windowsFolderToolStripMenuItem";
            this.windowsFolderToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.windowsFolderToolStripMenuItem.Text = "Windows...";
            this.windowsFolderToolStripMenuItem.Click += new System.EventHandler(this.windowsFolderToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(367, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Browse...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(367, 43);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Browse...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // checkIgnoreWhiteSpace
            // 
            this.checkIgnoreWhiteSpace.AutoSize = true;
            this.checkIgnoreWhiteSpace.Checked = true;
            this.checkIgnoreWhiteSpace.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkIgnoreWhiteSpace.Location = new System.Drawing.Point(19, 88);
            this.checkIgnoreWhiteSpace.Name = "checkIgnoreWhiteSpace";
            this.checkIgnoreWhiteSpace.Size = new System.Drawing.Size(116, 17);
            this.checkIgnoreWhiteSpace.TabIndex = 7;
            this.checkIgnoreWhiteSpace.Text = "Ignore white space";
            this.checkIgnoreWhiteSpace.UseVisualStyleBackColor = true;
            // 
            // checkIgnoreCase
            // 
            this.checkIgnoreCase.AutoSize = true;
            this.checkIgnoreCase.Location = new System.Drawing.Point(184, 88);
            this.checkIgnoreCase.Name = "checkIgnoreCase";
            this.checkIgnoreCase.Size = new System.Drawing.Size(82, 17);
            this.checkIgnoreCase.TabIndex = 8;
            this.checkIgnoreCase.Text = "Ignore case";
            this.checkIgnoreCase.UseVisualStyleBackColor = true;
            // 
            // checkIgnoreEOL
            // 
            this.checkIgnoreEOL.AutoSize = true;
            this.checkIgnoreEOL.Checked = true;
            this.checkIgnoreEOL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkIgnoreEOL.Location = new System.Drawing.Point(325, 88);
            this.checkIgnoreEOL.Name = "checkIgnoreEOL";
            this.checkIgnoreEOL.Size = new System.Drawing.Size(115, 17);
            this.checkIgnoreEOL.TabIndex = 9;
            this.checkIgnoreEOL.Text = "Ignore line endings";
            this.checkIgnoreEOL.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(286, 141);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(367, 141);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // SmartDiff
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(459, 183);
            this.Controls.Add(this.checkIgnoreEOL);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.checkIgnoreCase);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.checkIgnoreWhiteSpace);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtTo);
            this.Controls.Add(this.txtCompare);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SmartDiff";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BIDS Helper Smart Diff Options";
            this.Load += new System.EventHandler(this.SmartDiff_Load);
            this.browseContextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox txtCompare;
        public System.Windows.Forms.TextBox txtTo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ContextMenuStrip browseContextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem sourceSafeFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowsFolderToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.CheckBox checkIgnoreWhiteSpace;
        public System.Windows.Forms.CheckBox checkIgnoreCase;
        public System.Windows.Forms.CheckBox checkIgnoreEOL;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}