namespace AggManager
{
    partial class EditAggs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditAggs));
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.treeViewAggregation = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.checkBoxRelationships = new System.Windows.Forms.CheckBox();
            this.buttonOptimizeAgg = new System.Windows.Forms.Button();
            this.buttonEliminateDupe = new System.Windows.Forms.Button();
            this.lblEstimatedSize = new System.Windows.Forms.Label();
            this.buttonValidate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGrid1
            // 
            this.dataGrid1.AllowSorting = false;
            this.dataGrid1.AlternatingBackColor = System.Drawing.Color.LightGray;
            this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid1.BackColor = System.Drawing.Color.DarkGray;
            this.dataGrid1.CaptionBackColor = System.Drawing.Color.White;
            this.dataGrid1.CaptionFont = new System.Drawing.Font("Verdana", 10F);
            this.dataGrid1.CaptionForeColor = System.Drawing.Color.Navy;
            this.dataGrid1.CaptionVisible = false;
            this.dataGrid1.DataMember = "";
            this.dataGrid1.ForeColor = System.Drawing.Color.Black;
            this.dataGrid1.GridLineColor = System.Drawing.Color.Black;
            this.dataGrid1.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
            this.dataGrid1.HeaderBackColor = System.Drawing.Color.Silver;
            this.dataGrid1.HeaderForeColor = System.Drawing.Color.Black;
            this.dataGrid1.LinkColor = System.Drawing.Color.Navy;
            this.dataGrid1.Location = new System.Drawing.Point(2, 2);
            this.dataGrid1.Margin = new System.Windows.Forms.Padding(2);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.ParentRowsBackColor = System.Drawing.Color.White;
            this.dataGrid1.ParentRowsForeColor = System.Drawing.Color.Black;
            this.dataGrid1.PreferredColumnWidth = 400;
            this.dataGrid1.SelectionBackColor = System.Drawing.Color.Navy;
            this.dataGrid1.SelectionForeColor = System.Drawing.Color.White;
            this.dataGrid1.Size = new System.Drawing.Size(456, 404);
            this.dataGrid1.TabIndex = 2;
            this.dataGrid1.CurrentCellChanged += new System.EventHandler(this.dataGrid1_CurrentCellChanged);
            this.dataGrid1.Click += new System.EventHandler(this.dataGrid1_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(550, 451);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(108, 31);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(438, 451);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(108, 31);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // treeViewAggregation
            // 
            this.treeViewAggregation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewAggregation.Location = new System.Drawing.Point(-2, 0);
            this.treeViewAggregation.Margin = new System.Windows.Forms.Padding(2);
            this.treeViewAggregation.Name = "treeViewAggregation";
            this.treeViewAggregation.Size = new System.Drawing.Size(203, 407);
            this.treeViewAggregation.StateImageList = this.imageList1;
            this.treeViewAggregation.TabIndex = 5;
            this.treeViewAggregation.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAggregation_AfterCheck);
            this.treeViewAggregation.Click += new System.EventHandler(this.treeViewAggregation_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Unchecked.ico");
            this.imageList1.Images.SetKeyName(1, "Checked.ico");
            this.imageList1.Images.SetKeyName(2, "Dimension.ico");
            this.imageList1.Images.SetKeyName(3, "m2mDimension.ico");
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 32);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGrid1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.treeViewAggregation);
            this.splitContainer1.Size = new System.Drawing.Size(664, 406);
            this.splitContainer1.SplitterDistance = 461;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 6;
            // 
            // checkBoxRelationships
            // 
            this.checkBoxRelationships.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxRelationships.AutoSize = true;
            this.checkBoxRelationships.Location = new System.Drawing.Point(497, 11);
            this.checkBoxRelationships.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxRelationships.Name = "checkBoxRelationships";
            this.checkBoxRelationships.Size = new System.Drawing.Size(161, 17);
            this.checkBoxRelationships.TabIndex = 7;
            this.checkBoxRelationships.Text = "Show Attribute Relationships";
            this.checkBoxRelationships.UseVisualStyleBackColor = true;
            this.checkBoxRelationships.CheckedChanged += new System.EventHandler(this.checkBoxRelationships_CheckedChanged);
            // 
            // buttonOptimizeAgg
            // 
            this.buttonOptimizeAgg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOptimizeAgg.Location = new System.Drawing.Point(11, 442);
            this.buttonOptimizeAgg.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOptimizeAgg.Name = "buttonOptimizeAgg";
            this.buttonOptimizeAgg.Size = new System.Drawing.Size(108, 37);
            this.buttonOptimizeAgg.TabIndex = 8;
            this.buttonOptimizeAgg.Text = "Eliminate Redundancy";
            this.buttonOptimizeAgg.UseVisualStyleBackColor = true;
            this.buttonOptimizeAgg.Click += new System.EventHandler(this.buttonOptimizeAgg_Click);
            // 
            // buttonEliminateDupe
            // 
            this.buttonEliminateDupe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonEliminateDupe.Location = new System.Drawing.Point(123, 442);
            this.buttonEliminateDupe.Margin = new System.Windows.Forms.Padding(2);
            this.buttonEliminateDupe.Name = "buttonEliminateDupe";
            this.buttonEliminateDupe.Size = new System.Drawing.Size(108, 37);
            this.buttonEliminateDupe.TabIndex = 9;
            this.buttonEliminateDupe.Text = "Elliminate Duplicates";
            this.buttonEliminateDupe.UseVisualStyleBackColor = true;
            this.buttonEliminateDupe.Click += new System.EventHandler(this.buttonEliminateDupe_Click);
            // 
            // lblEstimatedSize
            // 
            this.lblEstimatedSize.AutoSize = true;
            this.lblEstimatedSize.Location = new System.Drawing.Point(5, 14);
            this.lblEstimatedSize.Name = "lblEstimatedSize";
            this.lblEstimatedSize.Size = new System.Drawing.Size(142, 13);
            this.lblEstimatedSize.TabIndex = 10;
            this.lblEstimatedSize.Text = "Estimated Aggregation Size: ";
            // 
            // buttonValidate
            // 
            this.buttonValidate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonValidate.Location = new System.Drawing.Point(235, 442);
            this.buttonValidate.Margin = new System.Windows.Forms.Padding(2);
            this.buttonValidate.Name = "buttonValidate";
            this.buttonValidate.Size = new System.Drawing.Size(108, 37);
            this.buttonValidate.TabIndex = 11;
            this.buttonValidate.Text = "Validate Aggregations";
            this.buttonValidate.UseVisualStyleBackColor = true;
            this.buttonValidate.Click += new System.EventHandler(this.buttonValidate_Click);
            // 
            // EditAggs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 493);
            this.Controls.Add(this.buttonValidate);
            this.Controls.Add(this.lblEstimatedSize);
            this.Controls.Add(this.buttonEliminateDupe);
            this.Controls.Add(this.buttonOptimizeAgg);
            this.Controls.Add(this.checkBoxRelationships);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(677, 520);
            this.Name = "EditAggs";
            this.Text = "Edit Aggregations";
            this.Load += new System.EventHandler(this.EditAggs_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGrid dataGrid1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TreeView treeViewAggregation;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox checkBoxRelationships;
        private System.Windows.Forms.Button buttonOptimizeAgg;
        private System.Windows.Forms.Button buttonEliminateDupe;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label lblEstimatedSize;
        private System.Windows.Forms.Button buttonValidate;

    }
}