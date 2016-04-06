namespace BIDSHelper.Core
{
    partial class BIDSHelperPreferencesPage
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
            this.txtSmartDiffCustom = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.radSmartDiffCustom = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.lblVSS = new System.Windows.Forms.Label();
            this.radSmartDiffDefault = new System.Windows.Forms.RadioButton();
            this.lblTFS = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.linkResetExpressionColors = new System.Windows.Forms.LinkLabel();
            this.btnConfigurationColor = new System.Windows.Forms.Button();
            this.btnExpressionColor = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtFreeSpaceFactor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSmartDiffCustom
            // 
            this.txtSmartDiffCustom.Location = new System.Drawing.Point(9, 54);
            this.txtSmartDiffCustom.Name = "txtSmartDiffCustom";
            this.txtSmartDiffCustom.Size = new System.Drawing.Size(342, 20);
            this.txtSmartDiffCustom.TabIndex = 4;
            this.txtSmartDiffCustom.Text = "\"C:\\Program Files\\Microsoft Visual Studio 9.0\\Common7\\IDE\\diffmerge.exe\" ? ? /ign" +
                "oreeol /ignorespace";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.radSmartDiffCustom);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblVSS);
            this.groupBox1.Controls.Add(this.radSmartDiffDefault);
            this.groupBox1.Controls.Add(this.lblTFS);
            this.groupBox1.Controls.Add(this.txtSmartDiffCustom);
            this.groupBox1.Location = new System.Drawing.Point(2, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(364, 103);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Smart Diff";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.label3.Location = new System.Drawing.Point(8, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(297, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Use a ? as the file parameters. Example: diff.exe -old ? -new ?";
            // 
            // radSmartDiffCustom
            // 
            this.radSmartDiffCustom.AutoSize = true;
            this.radSmartDiffCustom.Location = new System.Drawing.Point(9, 36);
            this.radSmartDiffCustom.Name = "radSmartDiffCustom";
            this.radSmartDiffCustom.Size = new System.Drawing.Size(212, 17);
            this.radSmartDiffCustom.TabIndex = 10;
            this.radSmartDiffCustom.TabStop = true;
            this.radSmartDiffCustom.Text = "Use Custom Diff Viewer Command Line:";
            this.radSmartDiffCustom.UseVisualStyleBackColor = true;
            this.radSmartDiffCustom.CheckedChanged += new System.EventHandler(this.radSmartDiffCustom_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(258, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Default Diff Viewers";
            // 
            // lblVSS
            // 
            this.lblVSS.AutoSize = true;
            this.lblVSS.Location = new System.Drawing.Point(258, 38);
            this.lblVSS.Name = "lblVSS";
            this.lblVSS.Size = new System.Drawing.Size(93, 13);
            this.lblVSS.TabIndex = 8;
            this.lblVSS.Text = "VSS: Not Installed";
            // 
            // radSmartDiffDefault
            // 
            this.radSmartDiffDefault.AutoSize = true;
            this.radSmartDiffDefault.Location = new System.Drawing.Point(9, 19);
            this.radSmartDiffDefault.Name = "radSmartDiffDefault";
            this.radSmartDiffDefault.Size = new System.Drawing.Size(135, 17);
            this.radSmartDiffDefault.TabIndex = 7;
            this.radSmartDiffDefault.TabStop = true;
            this.radSmartDiffDefault.Text = "Use Default Diff Viewer";
            this.radSmartDiffDefault.UseVisualStyleBackColor = true;
            this.radSmartDiffDefault.CheckedChanged += new System.EventHandler(this.radSmartDiffDefault_CheckedChanged);
            // 
            // lblTFS
            // 
            this.lblTFS.AutoSize = true;
            this.lblTFS.Location = new System.Drawing.Point(258, 25);
            this.lblTFS.Name = "lblTFS";
            this.lblTFS.Size = new System.Drawing.Size(72, 13);
            this.lblTFS.TabIndex = 6;
            this.lblTFS.Text = "TFS: Installed";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.linkResetExpressionColors);
            this.groupBox2.Controls.Add(this.btnConfigurationColor);
            this.groupBox2.Controls.Add(this.btnExpressionColor);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(3, 119);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(364, 85);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Expression and Configuration Highlighter";
            // 
            // linkResetExpressionColors
            // 
            this.linkResetExpressionColors.AutoSize = true;
            this.linkResetExpressionColors.Location = new System.Drawing.Point(213, 36);
            this.linkResetExpressionColors.Name = "linkResetExpressionColors";
            this.linkResetExpressionColors.Size = new System.Drawing.Size(116, 13);
            this.linkResetExpressionColors.TabIndex = 6;
            this.linkResetExpressionColors.TabStop = true;
            this.linkResetExpressionColors.Text = "Reset Colors to Default";
            this.linkResetExpressionColors.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkResetExpressionColors_LinkClicked);
            // 
            // btnConfigurationColor
            // 
            this.btnConfigurationColor.Location = new System.Drawing.Point(156, 47);
            this.btnConfigurationColor.Name = "btnConfigurationColor";
            this.btnConfigurationColor.Size = new System.Drawing.Size(24, 21);
            this.btnConfigurationColor.TabIndex = 5;
            this.btnConfigurationColor.UseVisualStyleBackColor = true;
            this.btnConfigurationColor.Click += new System.EventHandler(this.btnConfigurationColor_Click);
            // 
            // btnExpressionColor
            // 
            this.btnExpressionColor.Location = new System.Drawing.Point(156, 22);
            this.btnExpressionColor.Name = "btnExpressionColor";
            this.btnExpressionColor.Size = new System.Drawing.Size(24, 21);
            this.btnExpressionColor.TabIndex = 4;
            this.btnExpressionColor.UseVisualStyleBackColor = true;
            this.btnExpressionColor.Click += new System.EventHandler(this.btnExpressionColor_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(7, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(143, 21);
            this.label4.TabIndex = 3;
            this.label4.Text = "Configuration Highlight Color:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Expression Highlight Color:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // colorDialog1
            // 
            this.colorDialog1.AnyColor = true;
            this.colorDialog1.SolidColorOnly = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.txtFreeSpaceFactor);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(3, 223);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(364, 97);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Measure Group Health Check";
            // 
            // label7
            // 
            this.label7.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.label7.Location = new System.Drawing.Point(11, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(339, 48);
            this.label7.TabIndex = 3;
            this.label7.Text = "If the measure currently contains 1 million rows, a free space factor of 20 means" +
                " that Measure Group Health Check will pick a datatype that will hold a value of " +
                "20 million.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.label6.Location = new System.Drawing.Point(153, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "default 20";
            // 
            // txtFreeSpaceFactor
            // 
            this.txtFreeSpaceFactor.Location = new System.Drawing.Point(115, 20);
            this.txtFreeSpaceFactor.Name = "txtFreeSpaceFactor";
            this.txtFreeSpaceFactor.Size = new System.Drawing.Size(35, 20);
            this.txtFreeSpaceFactor.TabIndex = 1;
            this.txtFreeSpaceFactor.Text = "20";
            this.txtFreeSpaceFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtFreeSpaceFactor.Leave += new System.EventHandler(this.txtFreeSpaceFactor_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Free Space Factor:";
            // 
            // BIDSHelperPreferencesPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "BIDSHelperPreferencesPage";
            this.Size = new System.Drawing.Size(405, 478);
            this.Load += new System.EventHandler(this.BIDSHelperPreferencesPage_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtSmartDiffCustom;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblTFS;
        private System.Windows.Forms.RadioButton radSmartDiffDefault;
        private System.Windows.Forms.Label lblVSS;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton radSmartDiffCustom;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button btnExpressionColor;
        private System.Windows.Forms.Button btnConfigurationColor;
        private System.Windows.Forms.LinkLabel linkResetExpressionColors;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFreeSpaceFactor;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
    }
}
