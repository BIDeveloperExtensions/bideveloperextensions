namespace BIDSHelper.SSIS.DesignPracticeScanner
{
    partial class DesignWarningsOptionsPage
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblCurrentlyDisabled = new System.Windows.Forms.Label();
            this.lstPlugins = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(1, 3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(94, 13);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Enabled Warnings";
            this.lblTitle.Click += new System.EventHandler(this.lblTitle_Click);
            // 
            // lblCurrentlyDisabled
            // 
            this.lblCurrentlyDisabled.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentlyDisabled.AutoSize = true;
            this.lblCurrentlyDisabled.Location = new System.Drawing.Point(136, 103);
            this.lblCurrentlyDisabled.Name = "lblCurrentlyDisabled";
            this.lblCurrentlyDisabled.Size = new System.Drawing.Size(233, 13);
            this.lblCurrentlyDisabled.TabIndex = 2;
            this.lblCurrentlyDisabled.Text = "The BIDS Helper Add-in is not currently enabled";
            // 
            // lstPlugins
            // 
            this.lstPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstPlugins.FormattingEnabled = true;
            this.lstPlugins.Location = new System.Drawing.Point(3, 20);
            this.lstPlugins.Name = "lstPlugins";
            this.lstPlugins.Size = new System.Drawing.Size(394, 259);
            this.lstPlugins.Sorted = true;
            this.lstPlugins.TabIndex = 3;
            this.lstPlugins.Visible = false;
            // 
            // DesignWarningsOptionsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstPlugins);
            this.Controls.Add(this.lblCurrentlyDisabled);
            this.Controls.Add(this.lblTitle);
            this.Name = "DesignWarningsOptionsPage";
            this.Size = new System.Drawing.Size(422, 314);
            this.Load += new System.EventHandler(this.BIDSHelperOptionsPage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblCurrentlyDisabled;
        private System.Windows.Forms.CheckedListBox lstPlugins;
    }
}