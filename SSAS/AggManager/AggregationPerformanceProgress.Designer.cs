namespace AggManager
{
    partial class AggregationPerformanceProgress
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AggregationPerformanceProgress));
            this.chkTestAgg = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkTestNoAggs = new System.Windows.Forms.CheckBox();
            this.chkWithoutIndividualAggs = new System.Windows.Forms.CheckBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblAggDesign = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblCube = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblMeasureGroup = new System.Windows.Forms.Label();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.lblServer = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkTestAgg
            // 
            this.chkTestAgg.AutoSize = true;
            this.chkTestAgg.Checked = true;
            this.chkTestAgg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTestAgg.Location = new System.Drawing.Point(15, 30);
            this.chkTestAgg.Name = "chkTestAgg";
            this.chkTestAgg.Size = new System.Drawing.Size(244, 17);
            this.chkTestAgg.TabIndex = 15;
            this.chkTestAgg.Text = "Query performance with that aggregation (fast)";
            this.chkTestAgg.UseVisualStyleBackColor = true;
            this.chkTestAgg.CheckedChanged += new System.EventHandler(this.chkTestAgg_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(245, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "For each aggregation, test the following...";
            // 
            // chkTestNoAggs
            // 
            this.chkTestNoAggs.AutoSize = true;
            this.chkTestNoAggs.Checked = true;
            this.chkTestNoAggs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTestNoAggs.Location = new System.Drawing.Point(15, 50);
            this.chkTestNoAggs.Name = "chkTestNoAggs";
            this.chkTestNoAggs.Size = new System.Drawing.Size(247, 17);
            this.chkTestNoAggs.TabIndex = 17;
            this.chkTestNoAggs.Text = "Query performance with no aggregations (slow)";
            this.chkTestNoAggs.UseVisualStyleBackColor = true;
            // 
            // chkWithoutIndividualAggs
            // 
            this.chkWithoutIndividualAggs.AutoSize = true;
            this.chkWithoutIndividualAggs.Location = new System.Drawing.Point(15, 71);
            this.chkWithoutIndividualAggs.Name = "chkWithoutIndividualAggs";
            this.chkWithoutIndividualAggs.Size = new System.Drawing.Size(274, 17);
            this.chkWithoutIndividualAggs.TabIndex = 18;
            this.chkWithoutIndividualAggs.Text = "Query performance with some aggregations (slowest)";
            this.chkWithoutIndividualAggs.UseVisualStyleBackColor = true;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(15, 94);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 25);
            this.btnRun.TabIndex = 20;
            this.btnRun.Text = "Run Tests";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblAggDesign);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.lblCube);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.lblStatus);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.lblMeasureGroup);
            this.panel2.Controls.Add(this.lblDatabase);
            this.panel2.Controls.Add(this.lblServer);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.progressBar1);
            this.panel2.Enabled = false;
            this.panel2.Location = new System.Drawing.Point(0, 125);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(462, 166);
            this.panel2.TabIndex = 21;
            // 
            // lblAggDesign
            // 
            this.lblAggDesign.AutoSize = true;
            this.lblAggDesign.Location = new System.Drawing.Point(109, 85);
            this.lblAggDesign.Name = "lblAggDesign";
            this.lblAggDesign.Size = new System.Drawing.Size(65, 13);
            this.lblAggDesign.TabIndex = 40;
            this.lblAggDesign.Text = "AggDesign1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(12, 85);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 13);
            this.label7.TabIndex = 39;
            this.label7.Text = "Agg Design:";
            // 
            // lblCube
            // 
            this.lblCube.AutoSize = true;
            this.lblCube.Location = new System.Drawing.Point(109, 51);
            this.lblCube.Name = "lblCube";
            this.lblCube.Size = new System.Drawing.Size(90, 13);
            this.lblCube.TabIndex = 38;
            this.lblCube.Text = "Adventure Works";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(12, 51);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(40, 13);
            this.label10.TabIndex = 36;
            this.label10.Text = "Cube:";
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(109, 102);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(336, 28);
            this.lblStatus.TabIndex = 34;
            this.lblStatus.Text = "Step 1 of 10";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 32;
            this.label5.Text = "Status:";
            // 
            // lblMeasureGroup
            // 
            this.lblMeasureGroup.AutoSize = true;
            this.lblMeasureGroup.Location = new System.Drawing.Point(109, 68);
            this.lblMeasureGroup.Name = "lblMeasureGroup";
            this.lblMeasureGroup.Size = new System.Drawing.Size(72, 13);
            this.lblMeasureGroup.TabIndex = 30;
            this.lblMeasureGroup.Text = "Internet Sales";
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(109, 34);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(81, 13);
            this.lblDatabase.TabIndex = 28;
            this.lblDatabase.Text = "DatabaseName";
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(109, 16);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(66, 13);
            this.lblServer.TabIndex = 25;
            this.lblServer.Text = "ServerName";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 13);
            this.label3.TabIndex = 24;
            this.label3.Text = "Measure Group:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "Database:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Server:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(15, 135);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(430, 18);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 18;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Enabled = false;
            this.buttonCancel.Location = new System.Drawing.Point(108, 94);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(149, 25);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "Cancel and Show Report";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // AggregationPerformanceProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 291);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.chkWithoutIndividualAggs);
            this.Controls.Add(this.chkTestNoAggs);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chkTestAgg);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AggregationPerformanceProgress";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Aggregation Performance Test Progress";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AggregationPerformanceProgress_FormClosing);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkTestAgg;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkTestNoAggs;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblCube;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblMeasureGroup;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label lblAggDesign;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.CheckBox chkWithoutIndividualAggs;

    }
}