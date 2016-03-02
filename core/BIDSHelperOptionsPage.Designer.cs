namespace BIDSHelper.Core
{
    partial class BIDSHelperOptionsPage
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
            this.propertyGridFeatures = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(1, 3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(90, 13);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Enabled Features";
            // 
            // lblCurrentlyDisabled
            // 
            this.lblCurrentlyDisabled.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCurrentlyDisabled.Location = new System.Drawing.Point(0, 0);
            this.lblCurrentlyDisabled.Name = "lblCurrentlyDisabled";
            this.lblCurrentlyDisabled.Size = new System.Drawing.Size(394, 290);
            this.lblCurrentlyDisabled.TabIndex = 2;
            this.lblCurrentlyDisabled.Text = "The BIDS Helper Add-in is not currently enabled";
            this.lblCurrentlyDisabled.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // propertyGridFeatures
            // 
            this.propertyGridFeatures.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridFeatures.Location = new System.Drawing.Point(0, 19);
            this.propertyGridFeatures.Name = "propertyGridFeatures";
            this.propertyGridFeatures.Size = new System.Drawing.Size(394, 268);
            this.propertyGridFeatures.TabIndex = 4;
            this.propertyGridFeatures.ToolbarVisible = false;
            // 
            // BIDSHelperOptionsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.propertyGridFeatures);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblCurrentlyDisabled);
            this.Name = "BIDSHelperOptionsPage";
            this.Size = new System.Drawing.Size(394, 290);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblCurrentlyDisabled;
        private System.Windows.Forms.PropertyGrid propertyGridFeatures;
    }
}
