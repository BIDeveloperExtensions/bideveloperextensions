using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper.Core
{
    public partial class VersionCheckNotificationForm : Form
    {
        private VersionCheckPlugin _versionCheckPlugin;
        private EnvDTE.DTEEvents _events;
        private System.Threading.Mutex _mtx = new System.Threading.Mutex(false, "BIDS Helper Version Notification");

        public VersionCheckNotificationForm(VersionCheckPlugin versionCheckPlugin)
        {
            _versionCheckPlugin = versionCheckPlugin;
            InitializeComponent();
            notifyIcon1.Icon = BIDSHelper.Resources.Common.BIDSHelper;
            _events = versionCheckPlugin.ApplicationObject.Events.DTEEvents;
            _events.OnBeginShutdown += new EnvDTE._dispDTEEvents_OnBeginShutdownEventHandler(DTEEvents_OnBeginShutdown);
        }

        private void DTEEvents_OnBeginShutdown()
        {
            try
            {
                notifyIcon1.Visible = false;
            }
            catch { }
            try
            {
                _mtx.ReleaseMutex();
            }
            catch { }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            VersionCheckPlugin.OpenBidsHelperReleasePageInBrowser();
        }

        private void upgradeNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VersionCheckPlugin.OpenBidsHelperReleasePageInBrowser();
        }

        private void remindMeLaterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _versionCheckPlugin.LastVersionCheck = DateTime.Today;
                _mtx.ReleaseMutex();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void dismissToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _versionCheckPlugin.DismissedVersion = _versionCheckPlugin.ServerVersion;
                _mtx.ReleaseMutex();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void VersionCheckNotificationForm_Load(object sender, EventArgs e)
        {
            try
            {
                this.Visible = false;
            }
            catch { }
        }

        public void ShowNotification()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    //important to show the notification on the main thread of BIDS
                    this.BeginInvoke(new MethodInvoker(delegate() { ShowNotification(); }));
                }
                else
                {
                    if (_mtx.WaitOne(0, false)) //only returns true if no other BIDS Helper notification is open
                    {
                        notifyIcon1.BalloonTipText = "You are currently running BIDS Helper version " + _versionCheckPlugin.LocalVersion + "\r\nThe latest version of BIDS Helper available is version " + _versionCheckPlugin.ServerVersion + "\r\nClick to download the latest version.";
                        notifyIcon1.Visible = true;
                        notifyIcon1.ShowBalloonTip(int.MaxValue);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem showing BIDS Helper version notification: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void VersionCheckNotificationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            DTEEvents_OnBeginShutdown();
        }


        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            VersionCheckPlugin.OpenBidsHelperReleasePageInBrowser();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                VersionCheckPlugin.OpenBidsHelperReleasePageInBrowser();
        }
    }
}
