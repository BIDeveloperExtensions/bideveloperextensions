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
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtFreeSpaceFactor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBoxExpressionEditor = new System.Windows.Forms.GroupBox();
            this.buttonResultFontSample = new System.Windows.Forms.Button();
            this.buttonExpressionFontSample = new System.Windows.Forms.Button();
            this.buttonResultFont = new System.Windows.Forms.Button();
            this.buttonExpressionFont = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.fontDialog = new System.Windows.Forms.FontDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBoxExpressionEditor.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSmartDiffCustom
            // 
            this.txtSmartDiffCustom.Location = new System.Drawing.Point(12, 66);
            this.txtSmartDiffCustom.Margin = new System.Windows.Forms.Padding(4);
            this.txtSmartDiffCustom.Name = "txtSmartDiffCustom";
            this.txtSmartDiffCustom.Size = new System.Drawing.Size(455, 22);
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
            this.groupBox1.Location = new System.Drawing.Point(3, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(485, 127);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Smart Diff";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.label3.Location = new System.Drawing.Point(11, 95);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(297, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Use a ? as the file parameters. Example: diff.exe -old ? -new ?";
            // 
            // radSmartDiffCustom
            // 
            this.radSmartDiffCustom.AutoSize = true;
            this.radSmartDiffCustom.Location = new System.Drawing.Point(12, 44);
            this.radSmartDiffCustom.Margin = new System.Windows.Forms.Padding(4);
            this.radSmartDiffCustom.Name = "radSmartDiffCustom";
            this.radSmartDiffCustom.Size = new System.Drawing.Size(261, 20);
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
            this.label2.Location = new System.Drawing.Point(344, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Default Diff Viewers";
            // 
            // lblVSS
            // 
            this.lblVSS.AutoSize = true;
            this.lblVSS.Location = new System.Drawing.Point(344, 47);
            this.lblVSS.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblVSS.Name = "lblVSS";
            this.lblVSS.Size = new System.Drawing.Size(115, 16);
            this.lblVSS.TabIndex = 8;
            this.lblVSS.Text = "VSS: Not Installed";
            // 
            // radSmartDiffDefault
            // 
            this.radSmartDiffDefault.AutoSize = true;
            this.radSmartDiffDefault.Location = new System.Drawing.Point(12, 23);
            this.radSmartDiffDefault.Margin = new System.Windows.Forms.Padding(4);
            this.radSmartDiffDefault.Name = "radSmartDiffDefault";
            this.radSmartDiffDefault.Size = new System.Drawing.Size(162, 20);
            this.radSmartDiffDefault.TabIndex = 7;
            this.radSmartDiffDefault.TabStop = true;
            this.radSmartDiffDefault.Text = "Use Default Diff Viewer";
            this.radSmartDiffDefault.UseVisualStyleBackColor = true;
            this.radSmartDiffDefault.CheckedChanged += new System.EventHandler(this.radSmartDiffDefault_CheckedChanged);
            // 
            // lblTFS
            // 
            this.lblTFS.AutoSize = true;
            this.lblTFS.Location = new System.Drawing.Point(344, 31);
            this.lblTFS.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTFS.Name = "lblTFS";
            this.lblTFS.Size = new System.Drawing.Size(90, 16);
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
            this.groupBox2.Location = new System.Drawing.Point(4, 289);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(485, 105);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "SSIS Expression and Configuration Highlighter";
            // 
            // linkResetExpressionColors
            // 
            this.linkResetExpressionColors.AutoSize = true;
            this.linkResetExpressionColors.Location = new System.Drawing.Point(284, 44);
            this.linkResetExpressionColors.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkResetExpressionColors.Name = "linkResetExpressionColors";
            this.linkResetExpressionColors.Size = new System.Drawing.Size(145, 16);
            this.linkResetExpressionColors.TabIndex = 6;
            this.linkResetExpressionColors.TabStop = true;
            this.linkResetExpressionColors.Text = "Reset Colors to Default";
            this.linkResetExpressionColors.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkResetExpressionColors_LinkClicked);
            // 
            // btnConfigurationColor
            // 
            this.btnConfigurationColor.Location = new System.Drawing.Point(208, 58);
            this.btnConfigurationColor.Margin = new System.Windows.Forms.Padding(4);
            this.btnConfigurationColor.Name = "btnConfigurationColor";
            this.btnConfigurationColor.Size = new System.Drawing.Size(32, 26);
            this.btnConfigurationColor.TabIndex = 5;
            this.btnConfigurationColor.UseVisualStyleBackColor = true;
            this.btnConfigurationColor.Click += new System.EventHandler(this.btnConfigurationColor_Click);
            // 
            // btnExpressionColor
            // 
            this.btnExpressionColor.Location = new System.Drawing.Point(208, 27);
            this.btnExpressionColor.Margin = new System.Windows.Forms.Padding(4);
            this.btnExpressionColor.Name = "btnExpressionColor";
            this.btnExpressionColor.Size = new System.Drawing.Size(32, 26);
            this.btnExpressionColor.TabIndex = 4;
            this.btnExpressionColor.UseVisualStyleBackColor = true;
            this.btnExpressionColor.Click += new System.EventHandler(this.btnExpressionColor_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(9, 63);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(191, 26);
            this.label4.TabIndex = 3;
            this.label4.Text = "Configuration Highlight Color:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(-3, 32);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Expression Highlight Color:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // colorDialog
            // 
            this.colorDialog.AnyColor = true;
            this.colorDialog.SolidColorOnly = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.txtFreeSpaceFactor);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(3, 150);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(485, 119);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "SSAS Measure Group Health Check";
            // 
            // label7
            // 
            this.label7.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.label7.Location = new System.Drawing.Point(15, 57);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(452, 59);
            this.label7.TabIndex = 3;
            this.label7.Text = "If the measure currently contains 1 million rows, a free space factor of 20 means" +
    " that Measure Group Health Check will pick a datatype that will hold a value of " +
    "20 million.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.label6.Location = new System.Drawing.Point(204, 28);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 16);
            this.label6.TabIndex = 2;
            this.label6.Text = "default 20";
            // 
            // txtFreeSpaceFactor
            // 
            this.txtFreeSpaceFactor.Location = new System.Drawing.Point(153, 25);
            this.txtFreeSpaceFactor.Margin = new System.Windows.Forms.Padding(4);
            this.txtFreeSpaceFactor.Name = "txtFreeSpaceFactor";
            this.txtFreeSpaceFactor.Size = new System.Drawing.Size(45, 22);
            this.txtFreeSpaceFactor.TabIndex = 1;
            this.txtFreeSpaceFactor.Text = "20";
            this.txtFreeSpaceFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtFreeSpaceFactor.Leave += new System.EventHandler(this.txtFreeSpaceFactor_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 28);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 16);
            this.label5.TabIndex = 0;
            this.label5.Text = "Free Space Factor:";
            // 
            // groupBoxExpressionEditor
            // 
            this.groupBoxExpressionEditor.Controls.Add(this.buttonResultFontSample);
            this.groupBoxExpressionEditor.Controls.Add(this.buttonExpressionFontSample);
            this.groupBoxExpressionEditor.Controls.Add(this.buttonResultFont);
            this.groupBoxExpressionEditor.Controls.Add(this.buttonExpressionFont);
            this.groupBoxExpressionEditor.Controls.Add(this.label9);
            this.groupBoxExpressionEditor.Controls.Add(this.label8);
            this.groupBoxExpressionEditor.Location = new System.Drawing.Point(4, 421);
            this.groupBoxExpressionEditor.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxExpressionEditor.Name = "groupBoxExpressionEditor";
            this.groupBoxExpressionEditor.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxExpressionEditor.Size = new System.Drawing.Size(485, 129);
            this.groupBoxExpressionEditor.TabIndex = 8;
            this.groupBoxExpressionEditor.TabStop = false;
            this.groupBoxExpressionEditor.Text = "SSIS Expression Editor";
            // 
            // buttonResultFontSample
            // 
            this.buttonResultFontSample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResultFontSample.BackColor = System.Drawing.SystemColors.Window;
            this.buttonResultFontSample.ForeColor = System.Drawing.SystemColors.WindowText;
            this.buttonResultFontSample.Location = new System.Drawing.Point(122, 64);
            this.buttonResultFontSample.Name = "buttonResultFontSample";
            this.buttonResultFontSample.Size = new System.Drawing.Size(311, 36);
            this.buttonResultFontSample.TabIndex = 5;
            this.buttonResultFontSample.Text = "Result sample";
            this.buttonResultFontSample.UseVisualStyleBackColor = false;
            this.buttonResultFontSample.Click += new System.EventHandler(this.buttonResultFont_Click);
            // 
            // buttonExpressionFontSample
            // 
            this.buttonExpressionFontSample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExpressionFontSample.BackColor = System.Drawing.SystemColors.Window;
            this.buttonExpressionFontSample.ForeColor = System.Drawing.SystemColors.WindowText;
            this.buttonExpressionFontSample.Location = new System.Drawing.Point(122, 22);
            this.buttonExpressionFontSample.Name = "buttonExpressionFontSample";
            this.buttonExpressionFontSample.Size = new System.Drawing.Size(311, 36);
            this.buttonExpressionFontSample.TabIndex = 4;
            this.buttonExpressionFontSample.Text = "Expression sample";
            this.buttonExpressionFontSample.UseVisualStyleBackColor = false;
            this.buttonExpressionFontSample.Click += new System.EventHandler(this.buttonExpressionFont_Click);
            // 
            // buttonResultFont
            // 
            this.buttonResultFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResultFont.Location = new System.Drawing.Point(430, 64);
            this.buttonResultFont.Name = "buttonResultFont";
            this.buttonResultFont.Size = new System.Drawing.Size(36, 36);
            this.buttonResultFont.TabIndex = 3;
            this.buttonResultFont.Text = "...";
            this.buttonResultFont.UseVisualStyleBackColor = true;
            this.buttonResultFont.Click += new System.EventHandler(this.buttonResultFont_Click);
            // 
            // buttonExpressionFont
            // 
            this.buttonExpressionFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExpressionFont.Location = new System.Drawing.Point(430, 22);
            this.buttonExpressionFont.Name = "buttonExpressionFont";
            this.buttonExpressionFont.Size = new System.Drawing.Size(36, 36);
            this.buttonExpressionFont.TabIndex = 2;
            this.buttonExpressionFont.Text = "...";
            this.buttonExpressionFont.UseVisualStyleBackColor = true;
            this.buttonExpressionFont.Click += new System.EventHandler(this.buttonExpressionFont_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(14, 74);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(73, 16);
            this.label9.TabIndex = 1;
            this.label9.Text = "Result font:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(14, 33);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(102, 16);
            this.label8.TabIndex = 0;
            this.label8.Text = "Expression font:";
            // 
            // fontDialog
            // 
            this.fontDialog.Color = System.Drawing.SystemColors.WindowText;
            this.fontDialog.ShowColor = true;
            // 
            // BIDSHelperPreferencesPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBoxExpressionEditor);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BIDSHelperPreferencesPage";
            this.Size = new System.Drawing.Size(540, 588);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBoxExpressionEditor.ResumeLayout(false);
            this.groupBoxExpressionEditor.PerformLayout();
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
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Button btnExpressionColor;
        private System.Windows.Forms.Button btnConfigurationColor;
        private System.Windows.Forms.LinkLabel linkResetExpressionColors;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFreeSpaceFactor;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBoxExpressionEditor;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonResultFont;
        private System.Windows.Forms.Button buttonExpressionFont;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button buttonResultFontSample;
        private System.Windows.Forms.Button buttonExpressionFontSample;
        private System.Windows.Forms.FontDialog fontDialog;
    }
}
