namespace BIDSHelper
{
    partial class VisualizeAttributeLatticeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VisualizeAttributeLatticeForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllDimensionsToFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dimensionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previousAltLeftArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextAltRightArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.layoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.layoutAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.layoutBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.layoutCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyMultilevelRelationshipsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.legendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.openReportWithAllDimensionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.dimensionsToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(546, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem,
            this.saveAllDimensionsToFolderToolStripMenuItem,
            this.openReportWithAllDimensionsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // saveAllDimensionsToFolderToolStripMenuItem
            // 
            this.saveAllDimensionsToFolderToolStripMenuItem.Name = "saveAllDimensionsToFolderToolStripMenuItem";
            this.saveAllDimensionsToFolderToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.saveAllDimensionsToFolderToolStripMenuItem.Text = "Save All Dimensions To Folder...";
            this.saveAllDimensionsToFolderToolStripMenuItem.Click += new System.EventHandler(this.saveAllDimensionsToFolderToolStripMenuItem_Click);
            // 
            // dimensionsToolStripMenuItem
            // 
            this.dimensionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.previousAltLeftArrowToolStripMenuItem,
            this.nextAltRightArrowToolStripMenuItem,
            this.toolStripSeparator1});
            this.dimensionsToolStripMenuItem.Name = "dimensionsToolStripMenuItem";
            this.dimensionsToolStripMenuItem.Size = new System.Drawing.Size(72, 20);
            this.dimensionsToolStripMenuItem.Text = "Dimensions";
            // 
            // previousAltLeftArrowToolStripMenuItem
            // 
            this.previousAltLeftArrowToolStripMenuItem.Name = "previousAltLeftArrowToolStripMenuItem";
            this.previousAltLeftArrowToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.previousAltLeftArrowToolStripMenuItem.Text = "Previous (Alt-Left Arrow)";
            this.previousAltLeftArrowToolStripMenuItem.Click += new System.EventHandler(this.previousAltLeftArrowToolStripMenuItem_Click);
            // 
            // nextAltRightArrowToolStripMenuItem
            // 
            this.nextAltRightArrowToolStripMenuItem.Name = "nextAltRightArrowToolStripMenuItem";
            this.nextAltRightArrowToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.nextAltRightArrowToolStripMenuItem.Text = "Next (Alt-Right Arrow)";
            this.nextAltRightArrowToolStripMenuItem.Click += new System.EventHandler(this.nextAltRightArrowToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(202, 6);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.layoutToolStripMenuItem,
            this.showOnlyMultilevelRelationshipsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // layoutToolStripMenuItem
            // 
            this.layoutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.layoutAToolStripMenuItem,
            this.layoutBToolStripMenuItem,
            this.layoutCToolStripMenuItem});
            this.layoutToolStripMenuItem.Name = "layoutToolStripMenuItem";
            this.layoutToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.layoutToolStripMenuItem.Text = "Layout Method";
            this.layoutToolStripMenuItem.Visible = false;
            // 
            // layoutAToolStripMenuItem
            // 
            this.layoutAToolStripMenuItem.Checked = true;
            this.layoutAToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.layoutAToolStripMenuItem.Name = "layoutAToolStripMenuItem";
            this.layoutAToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.layoutAToolStripMenuItem.Text = "Layout A (Usually the Best)";
            this.layoutAToolStripMenuItem.Click += new System.EventHandler(this.layoutAToolStripMenuItem_Click);
            // 
            // layoutBToolStripMenuItem
            // 
            this.layoutBToolStripMenuItem.Name = "layoutBToolStripMenuItem";
            this.layoutBToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.layoutBToolStripMenuItem.Text = "Layout B";
            this.layoutBToolStripMenuItem.Click += new System.EventHandler(this.layoutBToolStripMenuItem_Click);
            // 
            // layoutCToolStripMenuItem
            // 
            this.layoutCToolStripMenuItem.Name = "layoutCToolStripMenuItem";
            this.layoutCToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.layoutCToolStripMenuItem.Text = "Layout C";
            this.layoutCToolStripMenuItem.Click += new System.EventHandler(this.layoutCToolStripMenuItem_Click);
            // 
            // showOnlyMultilevelRelationshipsToolStripMenuItem
            // 
            this.showOnlyMultilevelRelationshipsToolStripMenuItem.Name = "showOnlyMultilevelRelationshipsToolStripMenuItem";
            this.showOnlyMultilevelRelationshipsToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.showOnlyMultilevelRelationshipsToolStripMenuItem.Text = "Show Only Multi-level Relationships";
            this.showOnlyMultilevelRelationshipsToolStripMenuItem.Click += new System.EventHandler(this.showOnlyMultilevelRelationshipsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.legendToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // legendToolStripMenuItem
            // 
            this.legendToolStripMenuItem.Name = "legendToolStripMenuItem";
            this.legendToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.legendToolStripMenuItem.Text = "Legend";
            this.legendToolStripMenuItem.Click += new System.EventHandler(this.legendToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(546, 419);
            this.panel1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(-2, -2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(534, 405);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // openReportWithAllDimensionsToolStripMenuItem
            // 
            this.openReportWithAllDimensionsToolStripMenuItem.Name = "openReportWithAllDimensionsToolStripMenuItem";
            this.openReportWithAllDimensionsToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.openReportWithAllDimensionsToolStripMenuItem.Text = "Open Report With All Dimensions...";
            this.openReportWithAllDimensionsToolStripMenuItem.Click += new System.EventHandler(this.openReportWithAllDimensionsToolStripMenuItem_Click);
            // 
            // VisualizeAttributeLatticeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 443);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "VisualizeAttributeLatticeForm";
            this.Text = "VisualizeAttributeLatticeForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.VisualizeAttributeLatticeForm_FormClosed);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VisualizeAttributeLatticeForm_KeyUp);
            this.Load += new System.EventHandler(this.VisualizeAttributeLatticeForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem dimensionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem previousAltLeftArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextAltRightArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem layoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem layoutAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOnlyMultilevelRelationshipsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem layoutBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem layoutCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem legendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAllDimensionsToFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openReportWithAllDimensionsToolStripMenuItem;
    }
}