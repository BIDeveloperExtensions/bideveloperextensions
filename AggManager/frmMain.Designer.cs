namespace AggManager
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imgListMetadata = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStripMG = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmdAddAggregationDesign = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdAddFromQueryLog = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAggDes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmdEditAggDes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdDeleteAggDes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdAddAggregationsFromQueryLogToExisting = new System.Windows.Forms.ToolStripMenuItem();
            this.addPartitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listBoxReport = new System.Windows.Forms.ListBox();
            this.contextMenuStripSave = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSave = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveToFile = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripPartitionAggs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.aggregationSizesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.contextMenuStripMG.SuspendLayout();
            this.contextMenuStripAggDes.SuspendLayout();
            this.contextMenuStripSave.SuspendLayout();
            this.contextMenuStripPartitionAggs.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imgListMetadata;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Margin = new System.Windows.Forms.Padding(2);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.Size = new System.Drawing.Size(453, 476);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterExpand);
            // 
            // imgListMetadata
            // 
            this.imgListMetadata.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListMetadata.ImageStream")));
            this.imgListMetadata.TransparentColor = System.Drawing.Color.Transparent;
            this.imgListMetadata.Images.SetKeyName(0, "Connected.ico");
            this.imgListMetadata.Images.SetKeyName(1, "Disconnected.ico");
            this.imgListMetadata.Images.SetKeyName(2, "Folder.ico");
            this.imgListMetadata.Images.SetKeyName(3, "Database.ico");
            this.imgListMetadata.Images.SetKeyName(4, "Cube.ico");
            this.imgListMetadata.Images.SetKeyName(5, "MeasureGroup.ico");
            this.imgListMetadata.Images.SetKeyName(6, "Partition.ico");
            this.imgListMetadata.Images.SetKeyName(7, "AggDesign.ico");
            // 
            // contextMenuStripMG
            // 
            this.contextMenuStripMG.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdAddAggregationDesign,
            this.cmdAddFromQueryLog});
            this.contextMenuStripMG.Name = "contextMenuStripMG";
            this.contextMenuStripMG.Size = new System.Drawing.Size(226, 48);
            // 
            // cmdAddAggregationDesign
            // 
            this.cmdAddAggregationDesign.Name = "cmdAddAggregationDesign";
            this.cmdAddAggregationDesign.Size = new System.Drawing.Size(225, 22);
            this.cmdAddAggregationDesign.Text = "Add Empty";
            this.cmdAddAggregationDesign.Click += new System.EventHandler(this.cmdAddAggregationDesignToolStripMenuItem_Click);
            // 
            // cmdAddFromQueryLog
            // 
            this.cmdAddFromQueryLog.Name = "cmdAddFromQueryLog";
            this.cmdAddFromQueryLog.Size = new System.Drawing.Size(225, 22);
            this.cmdAddFromQueryLog.Text = "Add from Query Log";
            this.cmdAddFromQueryLog.Click += new System.EventHandler(this.cmdAddFromQueryLog_Click);
            // 
            // contextMenuStripAggDes
            // 
            this.contextMenuStripAggDes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdEditAggDes,
            this.cmdDeleteAggDes,
            this.cmdAddAggregationsFromQueryLogToExisting,
            this.addPartitionsToolStripMenuItem});
            this.contextMenuStripAggDes.Name = "contextMenuStripAggDes";
            this.contextMenuStripAggDes.Size = new System.Drawing.Size(310, 92);
            // 
            // cmdEditAggDes
            // 
            this.cmdEditAggDes.Name = "cmdEditAggDes";
            this.cmdEditAggDes.Size = new System.Drawing.Size(309, 22);
            this.cmdEditAggDes.Text = "Edit";
            this.cmdEditAggDes.Click += new System.EventHandler(this.cmdEditAggDes_Click);
            // 
            // cmdDeleteAggDes
            // 
            this.cmdDeleteAggDes.Name = "cmdDeleteAggDes";
            this.cmdDeleteAggDes.Size = new System.Drawing.Size(309, 22);
            this.cmdDeleteAggDes.Text = "Delete";
            this.cmdDeleteAggDes.Click += new System.EventHandler(this.cmdDeleteAggDes_Click);
            // 
            // cmdAddAggregationsFromQueryLogToExisting
            // 
            this.cmdAddAggregationsFromQueryLogToExisting.Name = "cmdAddAggregationsFromQueryLogToExisting";
            this.cmdAddAggregationsFromQueryLogToExisting.Size = new System.Drawing.Size(309, 22);
            this.cmdAddAggregationsFromQueryLogToExisting.Text = "Add Aggregations from QueryLog";
            this.cmdAddAggregationsFromQueryLogToExisting.Click += new System.EventHandler(this.cmdAddAggregationsFromQueryLogToExisting_Click);
            // 
            // addPartitionsToolStripMenuItem
            // 
            this.addPartitionsToolStripMenuItem.Name = "addPartitionsToolStripMenuItem";
            this.addPartitionsToolStripMenuItem.Size = new System.Drawing.Size(309, 22);
            this.addPartitionsToolStripMenuItem.Text = "Change Partitions";
            this.addPartitionsToolStripMenuItem.Click += new System.EventHandler(this.addPartitionsToolStripMenuItem_Click);
            // 
            // listBoxReport
            // 
            this.listBoxReport.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBoxReport.FormattingEnabled = true;
            this.listBoxReport.Location = new System.Drawing.Point(0, 0);
            this.listBoxReport.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxReport.Name = "listBoxReport";
            this.listBoxReport.Size = new System.Drawing.Size(201, 485);
            this.listBoxReport.TabIndex = 8;
            // 
            // contextMenuStripSave
            // 
            this.contextMenuStripSave.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSave,
            this.toolStripMenuItemSaveToFile});
            this.contextMenuStripSave.Name = "contextMenuStripMG";
            this.contextMenuStripSave.Size = new System.Drawing.Size(84, 48);
            // 
            // toolStripMenuItemSave
            // 
            this.toolStripMenuItemSave.Name = "toolStripMenuItemSave";
            this.toolStripMenuItemSave.Size = new System.Drawing.Size(83, 22);
            // 
            // toolStripMenuItemSaveToFile
            // 
            this.toolStripMenuItemSaveToFile.Name = "toolStripMenuItemSaveToFile";
            this.toolStripMenuItemSaveToFile.Size = new System.Drawing.Size(83, 22);
            // 
            // contextMenuStripPartitionAggs
            // 
            this.contextMenuStripPartitionAggs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aggregationSizesToolStripMenuItem});
            this.contextMenuStripPartitionAggs.Name = "contextMenuStripPartitionAggs";
            this.contextMenuStripPartitionAggs.Size = new System.Drawing.Size(261, 26);
            // 
            // aggregationSizesToolStripMenuItem
            // 
            this.aggregationSizesToolStripMenuItem.Name = "aggregationSizesToolStripMenuItem";
            this.aggregationSizesToolStripMenuItem.Size = new System.Drawing.Size(260, 22);
            this.aggregationSizesToolStripMenuItem.Text = "Physical Aggregation Sizes";
            this.aggregationSizesToolStripMenuItem.Click += new System.EventHandler(this.aggregationSizesToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(8, 8);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            this.splitContainer1.Panel1MinSize = 225;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listBoxReport);
            this.splitContainer1.Size = new System.Drawing.Size(658, 476);
            this.splitContainer1.SplitterDistance = 453;
            this.splitContainer1.TabIndex = 9;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(510, 490);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(591, 490);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 525);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Aggregation Manager";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.contextMenuStripMG.ResumeLayout(false);
            this.contextMenuStripAggDes.ResumeLayout(false);
            this.contextMenuStripSave.ResumeLayout(false);
            this.contextMenuStripPartitionAggs.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMG;
        private System.Windows.Forms.ToolStripMenuItem cmdAddAggregationDesign;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAggDes;
        private System.Windows.Forms.ToolStripMenuItem cmdEditAggDes;
        private System.Windows.Forms.ToolStripMenuItem cmdDeleteAggDes;
        private System.Windows.Forms.ToolStripMenuItem cmdAddFromQueryLog;
        private System.Windows.Forms.ToolStripMenuItem cmdAddAggregationsFromQueryLogToExisting;
        private System.Windows.Forms.ListBox listBoxReport;
        private System.Windows.Forms.ToolStripMenuItem addPartitionsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSave;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSave;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveToFile;
        private System.Windows.Forms.ImageList imgListMetadata;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPartitionAggs;
        private System.Windows.Forms.ToolStripMenuItem aggregationSizesToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}