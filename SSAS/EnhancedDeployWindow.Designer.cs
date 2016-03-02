namespace BIDSHelper.SSAS
{
    partial class EnhancedDeployWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnhancedDeployWindow));
            this.grpPartitions = new System.Windows.Forms.GroupBox();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.grpRoles = new System.Windows.Forms.GroupBox();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tspProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.tspLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDeploy = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.lblServer = new System.Windows.Forms.Label();
            this.grpPartitions.SuspendLayout();
            this.grpRoles.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpPartitions
            // 
            this.grpPartitions.Controls.Add(this.radioButton3);
            this.grpPartitions.Controls.Add(this.radioButton2);
            this.grpPartitions.Controls.Add(this.radioButton1);
            this.grpPartitions.Location = new System.Drawing.Point(12, 88);
            this.grpPartitions.Name = "grpPartitions";
            this.grpPartitions.Size = new System.Drawing.Size(354, 89);
            this.grpPartitions.TabIndex = 0;
            this.grpPartitions.TabStop = false;
            this.grpPartitions.Tag = "0";
            this.grpPartitions.Text = "Partitions";
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(9, 65);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(288, 17);
            this.radioButton3.TabIndex = 6;
            this.radioButton3.Tag = "2";
            this.radioButton3.Text = "Deploy partitions, overwriting the partitions on the server";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(9, 42);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(328, 17);
            this.radioButton2.TabIndex = 5;
            this.radioButton2.Tag = "1";
            this.radioButton2.Text = "Deploy partitions, but keep partitions that only exist on the server";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(9, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(144, 17);
            this.radioButton1.TabIndex = 4;
            this.radioButton1.TabStop = true;
            this.radioButton1.Tag = "0";
            this.radioButton1.Text = "Do NOT deploy partitions";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // grpRoles
            // 
            this.grpRoles.Controls.Add(this.radioButton6);
            this.grpRoles.Controls.Add(this.radioButton5);
            this.grpRoles.Controls.Add(this.radioButton4);
            this.grpRoles.Location = new System.Drawing.Point(12, 195);
            this.grpRoles.Name = "grpRoles";
            this.grpRoles.Size = new System.Drawing.Size(354, 94);
            this.grpRoles.TabIndex = 1;
            this.grpRoles.TabStop = false;
            this.grpRoles.Tag = "0";
            this.grpRoles.Text = "Roles";
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(6, 65);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(248, 17);
            this.radioButton6.TabIndex = 5;
            this.radioButton6.Tag = "1";
            this.radioButton6.Text = "Deploy roles, overwriting the roles on the server";
            this.radioButton6.UseVisualStyleBackColor = true;
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(6, 42);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(288, 17);
            this.radioButton5.TabIndex = 4;
            this.radioButton5.Tag = "1";
            this.radioButton5.Text = "Deploy roles, but keep roles that only exist on the server";
            this.radioButton5.UseVisualStyleBackColor = true;
            this.radioButton5.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Checked = true;
            this.radioButton4.Location = new System.Drawing.Point(6, 19);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(124, 17);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.TabStop = true;
            this.radioButton4.Tag = "0";
            this.radioButton4.Text = "Do NOT deploy roles";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(314, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Select the desired deployment behaviour  from the options below:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tspProgress,
            this.tspLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 341);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(376, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tspProgress
            // 
            this.tspProgress.Name = "tspProgress";
            this.tspProgress.Size = new System.Drawing.Size(100, 16);
            this.tspProgress.Visible = false;
            // 
            // tspLabel
            // 
            this.tspLabel.Name = "tspLabel";
            this.tspLabel.Size = new System.Drawing.Size(39, 17);
            this.tspLabel.Text = "Ready";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(291, 306);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnDeploy
            // 
            this.btnDeploy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeploy.Location = new System.Drawing.Point(210, 306);
            this.btnDeploy.Name = "btnDeploy";
            this.btnDeploy.Size = new System.Drawing.Size(75, 23);
            this.btnDeploy.TabIndex = 8;
            this.btnDeploy.Text = "Deploy";
            this.btnDeploy.UseVisualStyleBackColor = true;
            this.btnDeploy.Click += new System.EventHandler(this.btnDeploy_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Target Server:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Target Database:";
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(109, 30);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(65, 13);
            this.lblDatabase.TabIndex = 11;
            this.lblDatabase.Text = "<Database>";
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(109, 13);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(50, 13);
            this.lblServer.TabIndex = 12;
            this.lblServer.Text = "<Server>";
            // 
            // EnhancedDeployWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 363);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnDeploy);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.grpRoles);
            this.Controls.Add(this.grpPartitions);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EnhancedDeployWindow";
            this.Text = "SSAS Enhanced Deploy (BIDS Helper)";
            this.grpPartitions.ResumeLayout(false);
            this.grpPartitions.PerformLayout();
            this.grpRoles.ResumeLayout(false);
            this.grpRoles.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpPartitions;
        private System.Windows.Forms.GroupBox grpRoles;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar tspProgress;
        private System.Windows.Forms.ToolStripStatusLabel tspLabel;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDeploy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.RadioButton radioButton6;
    }
}