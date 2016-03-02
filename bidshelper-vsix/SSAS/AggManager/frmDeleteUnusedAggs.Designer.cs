namespace AggManager
{
    partial class DeleteUnusedAggs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteUnusedAggs));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblDatabaseName = new System.Windows.Forms.Label();
            this.lblServer = new System.Windows.Forms.Label();
            this.radioTraceTypeLive = new System.Windows.Forms.RadioButton();
            this.radioTraceTypeSQL = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSQLConnection = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.grpProgress = new System.Windows.Forms.GroupBox();
            this.lblTraceDuration = new System.Windows.Forms.Label();
            this.lblAggHits = new System.Windows.Forms.Label();
            this.lblQueries = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.treeViewAggregation = new System.Windows.Forms.TreeView();
            this.imgListMetadata = new System.Windows.Forms.ImageList(this.components);
            this.lblUnusedAggregationsToDelete = new System.Windows.Forms.Label();
            this.grpProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(442, 508);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(94, 31);
            this.buttonOK.TabIndex = 38;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(540, 508);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(85, 31);
            this.buttonCancel.TabIndex = 37;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 13);
            this.label1.TabIndex = 45;
            this.label1.Text = "Analysis Services Server:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(17, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(168, 13);
            this.label2.TabIndex = 46;
            this.label2.Text = "Analysis Services Database:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDatabaseName
            // 
            this.lblDatabaseName.AutoSize = true;
            this.lblDatabaseName.Location = new System.Drawing.Point(189, 31);
            this.lblDatabaseName.Name = "lblDatabaseName";
            this.lblDatabaseName.Size = new System.Drawing.Size(81, 13);
            this.lblDatabaseName.TabIndex = 48;
            this.lblDatabaseName.Text = "DatabaseName";
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(189, 9);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(66, 13);
            this.lblServer.TabIndex = 47;
            this.lblServer.Text = "ServerName";
            // 
            // radioTraceTypeLive
            // 
            this.radioTraceTypeLive.AutoSize = true;
            this.radioTraceTypeLive.Checked = true;
            this.radioTraceTypeLive.Location = new System.Drawing.Point(192, 53);
            this.radioTraceTypeLive.Name = "radioTraceTypeLive";
            this.radioTraceTypeLive.Size = new System.Drawing.Size(153, 17);
            this.radioTraceTypeLive.TabIndex = 49;
            this.radioTraceTypeLive.TabStop = true;
            this.radioTraceTypeLive.Text = "New In-Memory Live Trace";
            this.radioTraceTypeLive.UseVisualStyleBackColor = true;
            this.radioTraceTypeLive.CheckedChanged += new System.EventHandler(this.radioTraceTypeLive_CheckedChanged);
            // 
            // radioTraceTypeSQL
            // 
            this.radioTraceTypeSQL.AutoSize = true;
            this.radioTraceTypeSQL.Location = new System.Drawing.Point(192, 72);
            this.radioTraceTypeSQL.Name = "radioTraceTypeSQL";
            this.radioTraceTypeSQL.Size = new System.Drawing.Size(192, 17);
            this.radioTraceTypeSQL.TabIndex = 50;
            this.radioTraceTypeSQL.Text = "Existing Trace Saved to SQL Table";
            this.radioTraceTypeSQL.UseVisualStyleBackColor = true;
            this.radioTraceTypeSQL.CheckedChanged += new System.EventHandler(this.radioTraceTypeSQL_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(109, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 51;
            this.label3.Text = "Trace Type:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnSQLConnection
            // 
            this.btnSQLConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSQLConnection.Location = new System.Drawing.Point(384, 69);
            this.btnSQLConnection.Margin = new System.Windows.Forms.Padding(0);
            this.btnSQLConnection.Name = "btnSQLConnection";
            this.btnSQLConnection.Size = new System.Drawing.Size(26, 20);
            this.btnSQLConnection.TabIndex = 52;
            this.btnSQLConnection.Text = "...";
            this.btnSQLConnection.UseVisualStyleBackColor = true;
            this.btnSQLConnection.Visible = false;
            this.btnSQLConnection.Click += new System.EventHandler(this.btnSQLConnection_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(192, 96);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(87, 23);
            this.btnExecute.TabIndex = 53;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // grpProgress
            // 
            this.grpProgress.Controls.Add(this.lblTraceDuration);
            this.grpProgress.Controls.Add(this.lblAggHits);
            this.grpProgress.Controls.Add(this.lblQueries);
            this.grpProgress.Controls.Add(this.label6);
            this.grpProgress.Controls.Add(this.label5);
            this.grpProgress.Controls.Add(this.label4);
            this.grpProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpProgress.Location = new System.Drawing.Point(457, 9);
            this.grpProgress.Name = "grpProgress";
            this.grpProgress.Size = new System.Drawing.Size(168, 110);
            this.grpProgress.TabIndex = 54;
            this.grpProgress.TabStop = false;
            this.grpProgress.Text = "Trace Progress:";
            this.grpProgress.Visible = false;
            // 
            // lblTraceDuration
            // 
            this.lblTraceDuration.AutoSize = true;
            this.lblTraceDuration.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTraceDuration.Location = new System.Drawing.Point(103, 67);
            this.lblTraceDuration.Name = "lblTraceDuration";
            this.lblTraceDuration.Size = new System.Drawing.Size(43, 13);
            this.lblTraceDuration.TabIndex = 5;
            this.lblTraceDuration.Text = "1:03:44";
            // 
            // lblAggHits
            // 
            this.lblAggHits.AutoSize = true;
            this.lblAggHits.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAggHits.Location = new System.Drawing.Point(103, 48);
            this.lblAggHits.Name = "lblAggHits";
            this.lblAggHits.Size = new System.Drawing.Size(34, 13);
            this.lblAggHits.TabIndex = 4;
            this.lblAggHits.Text = "9,999";
            // 
            // lblQueries
            // 
            this.lblQueries.AutoSize = true;
            this.lblQueries.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQueries.Location = new System.Drawing.Point(103, 29);
            this.lblQueries.Name = "lblQueries";
            this.lblQueries.Size = new System.Drawing.Size(34, 13);
            this.lblQueries.TabIndex = 3;
            this.lblQueries.Text = "9,999";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(21, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Trace Duration:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(14, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Aggregation Hits:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(29, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "MDX Queries:";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // treeViewAggregation
            // 
            this.treeViewAggregation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewAggregation.CheckBoxes = true;
            this.treeViewAggregation.ImageIndex = 0;
            this.treeViewAggregation.ImageList = this.imgListMetadata;
            this.treeViewAggregation.Location = new System.Drawing.Point(11, 157);
            this.treeViewAggregation.Margin = new System.Windows.Forms.Padding(2);
            this.treeViewAggregation.Name = "treeViewAggregation";
            this.treeViewAggregation.SelectedImageIndex = 0;
            this.treeViewAggregation.Size = new System.Drawing.Size(614, 343);
            this.treeViewAggregation.TabIndex = 55;
            this.treeViewAggregation.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAggregation_AfterCheck);
            // 
            // imgListMetadata
            // 
            this.imgListMetadata.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListMetadata.ImageStream")));
            this.imgListMetadata.TransparentColor = System.Drawing.Color.Transparent;
            this.imgListMetadata.Images.SetKeyName(0, "MeasureGroup.ico");
            this.imgListMetadata.Images.SetKeyName(1, "AggDesign.ico");
            this.imgListMetadata.Images.SetKeyName(2, "Aggregation.ico");
            // 
            // lblUnusedAggregationsToDelete
            // 
            this.lblUnusedAggregationsToDelete.AutoSize = true;
            this.lblUnusedAggregationsToDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnusedAggregationsToDelete.Location = new System.Drawing.Point(12, 142);
            this.lblUnusedAggregationsToDelete.Name = "lblUnusedAggregationsToDelete";
            this.lblUnusedAggregationsToDelete.Size = new System.Drawing.Size(188, 13);
            this.lblUnusedAggregationsToDelete.TabIndex = 56;
            this.lblUnusedAggregationsToDelete.Text = "Unused Aggregations to Delete:";
            // 
            // DeleteUnusedAggs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 553);
            this.Controls.Add(this.lblUnusedAggregationsToDelete);
            this.Controls.Add(this.treeViewAggregation);
            this.Controls.Add(this.grpProgress);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.btnSQLConnection);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.radioTraceTypeSQL);
            this.Controls.Add(this.radioTraceTypeLive);
            this.Controls.Add(this.lblDatabaseName);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeleteUnusedAggs";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BIDS Helper - Delete Unused Aggregations";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DeleteUnusedAggs_FormClosed);
            this.grpProgress.ResumeLayout(false);
            this.grpProgress.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblDatabaseName;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.RadioButton radioTraceTypeLive;
        private System.Windows.Forms.RadioButton radioTraceTypeSQL;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSQLConnection;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.GroupBox grpProgress;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTraceDuration;
        private System.Windows.Forms.Label lblAggHits;
        private System.Windows.Forms.Label lblQueries;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblUnusedAggregationsToDelete;
        private System.Windows.Forms.ImageList imgListMetadata;
        public System.Windows.Forms.TreeView treeViewAggregation;
    }
}