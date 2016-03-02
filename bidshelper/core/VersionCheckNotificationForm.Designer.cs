namespace BIDSHelper.Core
{
    partial class VersionCheckNotificationForm
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
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.upgradeNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remindMeLaterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dismissToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = "You are not running the latest version of BIDS Helper.\r\nClick here to download th" +
                "e latest release";
            this.notifyIcon1.BalloonTipTitle = "BIDS Helper Version Notification";
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Text = "A new version of BIDS Helper is available to download";
            this.notifyIcon1.BalloonTipClicked += new System.EventHandler(this.notifyIcon1_BalloonTipClicked);
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.upgradeNowToolStripMenuItem,
            this.remindMeLaterToolStripMenuItem,
            this.dismissToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(273, 70);
            // 
            // upgradeNowToolStripMenuItem
            // 
            this.upgradeNowToolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.upgradeNowToolStripMenuItem.Name = "upgradeNowToolStripMenuItem";
            this.upgradeNowToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.upgradeNowToolStripMenuItem.Text = "Download Latest BIDS Helper Release";
            this.upgradeNowToolStripMenuItem.Click += new System.EventHandler(this.upgradeNowToolStripMenuItem_Click);
            // 
            // remindMeLaterToolStripMenuItem
            // 
            this.remindMeLaterToolStripMenuItem.Name = "remindMeLaterToolStripMenuItem";
            this.remindMeLaterToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.remindMeLaterToolStripMenuItem.Text = "Remind Me Later";
            this.remindMeLaterToolStripMenuItem.Click += new System.EventHandler(this.remindMeLaterToolStripMenuItem_Click);
            // 
            // dismissToolStripMenuItem
            // 
            this.dismissToolStripMenuItem.Name = "dismissToolStripMenuItem";
            this.dismissToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.dismissToolStripMenuItem.Text = "Dismiss Notification";
            this.dismissToolStripMenuItem.Click += new System.EventHandler(this.dismissToolStripMenuItem_Click);
            // 
            // VersionCheckNotificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(268, 45);
            this.Name = "VersionCheckNotificationForm";
            this.ShowInTaskbar = false;
            this.Text = "VersionCheckNotificationForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.VersionCheckNotificationForm_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.VersionCheckNotificationForm_FormClosed);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem upgradeNowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remindMeLaterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dismissToolStripMenuItem;
        public System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}