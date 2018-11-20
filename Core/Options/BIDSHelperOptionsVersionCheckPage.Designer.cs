namespace BIDSHelper.Core
{
    partial class BIDSHelperOptionsVersionCheckPage
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblLocalVersion = new System.Windows.Forms.Label();
            this.linkLabelCodePlexUrl = new System.Windows.Forms.LinkLabel();
            this.linkNewVersion = new System.Windows.Forms.LinkLabel();
            this.lblServerVersion = new System.Windows.Forms.Label();
            this.lblSqlVersion = new System.Windows.Forms.Label();
            this.lblBidsHelperLoadException = new System.Windows.Forms.Label();
            this.btnCopyError = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(1, 3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(77, 13);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "BIDS Helper";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Version:";
            // 
            // lblLocalVersion
            // 
            this.lblLocalVersion.AutoSize = true;
            this.lblLocalVersion.Location = new System.Drawing.Point(45, 23);
            this.lblLocalVersion.Name = "lblLocalVersion";
            this.lblLocalVersion.Size = new System.Drawing.Size(22, 13);
            this.lblLocalVersion.TabIndex = 3;
            this.lblLocalVersion.Text = "1.0";
            // 
            // linkLabelCodePlexUrl
            // 
            this.linkLabelCodePlexUrl.AutoSize = true;
            this.linkLabelCodePlexUrl.Location = new System.Drawing.Point(1, 39);
            this.linkLabelCodePlexUrl.Name = "linkLabelCodePlexUrl";
            this.linkLabelCodePlexUrl.Size = new System.Drawing.Size(155, 13);
            this.linkLabelCodePlexUrl.TabIndex = 5;
            this.linkLabelCodePlexUrl.TabStop = true;
            this.linkLabelCodePlexUrl.Text = "https://bideveloperextensions.github.io";
            this.linkLabelCodePlexUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCodePlexUrl_LinkClicked);
            // 
            // linkNewVersion
            // 
            this.linkNewVersion.AutoSize = true;
            this.linkNewVersion.Location = new System.Drawing.Point(1, 97);
            this.linkNewVersion.Name = "linkNewVersion";
            this.linkNewVersion.Size = new System.Drawing.Size(86, 13);
            this.linkNewVersion.TabIndex = 6;
            this.linkNewVersion.TabStop = true;
            this.linkNewVersion.Text = "Download it now";
            this.linkNewVersion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkNewVersion_LinkClicked);
            // 
            // lblServerVersion
            // 
            this.lblServerVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblServerVersion.Location = new System.Drawing.Point(1, 82);
            this.lblServerVersion.Name = "lblServerVersion";
            this.lblServerVersion.Size = new System.Drawing.Size(377, 178);
            this.lblServerVersion.TabIndex = 7;
            this.lblServerVersion.Text = "Version 1.0.1 is available...";
            // 
            // lblSqlVersion
            // 
            this.lblSqlVersion.AutoSize = true;
            this.lblSqlVersion.Location = new System.Drawing.Point(1, 57);
            this.lblSqlVersion.Name = "lblSqlVersion";
            this.lblSqlVersion.Size = new System.Drawing.Size(224, 13);
            this.lblSqlVersion.TabIndex = 8;
            this.lblSqlVersion.Text = "SSDTBI 2012 for Visual Studio 2012 detected";
            // 
            // lblBidsHelperLoadException
            // 
            this.lblBidsHelperLoadException.Location = new System.Drawing.Point(1, 124);
            this.lblBidsHelperLoadException.Name = "lblBidsHelperLoadException";
            this.lblBidsHelperLoadException.Size = new System.Drawing.Size(377, 136);
            this.lblBidsHelperLoadException.TabIndex = 9;
            this.lblBidsHelperLoadException.Text = "Exception x occurred on load of BIDS Helper\r\nException a\r\nException b";
            // 
            // btnCopyError
            // 
            this.btnCopyError.Location = new System.Drawing.Point(302, 82);
            this.btnCopyError.Name = "btnCopyError";
            this.btnCopyError.Size = new System.Drawing.Size(75, 23);
            this.btnCopyError.TabIndex = 10;
            this.btnCopyError.Text = "Copy Error";
            this.btnCopyError.UseVisualStyleBackColor = true;
            this.btnCopyError.Click += new System.EventHandler(this.btnCopyError_Click);
            // 
            // BIDSHelperOptionsVersionCheckPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCopyError);
            this.Controls.Add(this.lblBidsHelperLoadException);
            this.Controls.Add(this.lblSqlVersion);
            this.Controls.Add(this.linkNewVersion);
            this.Controls.Add(this.linkLabelCodePlexUrl);
            this.Controls.Add(this.lblLocalVersion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblServerVersion);
            this.Name = "BIDSHelperOptionsVersionCheckPage";
            this.Size = new System.Drawing.Size(394, 272);
            this.Load += new System.EventHandler(this.BIDSHelperOptionsVersionCheckPage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblLocalVersion;
        private System.Windows.Forms.LinkLabel linkLabelCodePlexUrl;
        private System.Windows.Forms.LinkLabel linkNewVersion;
        private System.Windows.Forms.Label lblServerVersion;
        private System.Windows.Forms.Label lblSqlVersion;
        private System.Windows.Forms.Label lblBidsHelperLoadException;
        private System.Windows.Forms.Button btnCopyError;
    }
}
