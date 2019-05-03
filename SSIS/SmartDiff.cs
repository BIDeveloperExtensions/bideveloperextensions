using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper.SSIS
{
    public partial class SmartDiff : Form
    {
        public string DefaultSourceSafePath;
        public string DefaultWindowsPath;
        public string SourceSafeIniDirectory;
        public string SourceControlProvider;

        public SmartDiff()
        {
            InitializeComponent();
            this.Icon = BIDSHelper.Resources.Common.BIDSHelper;
        }

        private void SmartDiff_Load(object sender, EventArgs e)
        {
            this.txtCompare.Text = DefaultSourceSafePath;
            this.txtTo.Text = DefaultWindowsPath;
        }

        private bool bContextButton1 = true;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                bContextButton1 = true;
                if (!string.IsNullOrEmpty(SourceControlProvider))
                    this.browseContextMenuStrip1.Show(this, new Point(button1.Left, button1.Bottom));
                else
                    windowsFolderToolStripMenuItem_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                bContextButton1 = false;
                if (!string.IsNullOrEmpty(SourceControlProvider))
                    this.browseContextMenuStrip1.Show(this, new Point(button2.Left, button2.Bottom));
                else
                    windowsFolderToolStripMenuItem_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void sourceSafeFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form versionForm = new Form();
                versionForm.Icon = BIDSHelper.Resources.Common.BIDSHelper;
                versionForm.Text = "Choose Version...";
                versionForm.MaximizeBox = false;
                versionForm.MinimizeBox = false;
                versionForm.SizeGripStyle = SizeGripStyle.Show;
                versionForm.ShowInTaskbar = false;
                versionForm.Width = 500;
                versionForm.Height = 120;
                versionForm.MaximumSize = new Size(1600, versionForm.Height);
                versionForm.MinimumSize = new Size(300, versionForm.Height);
                versionForm.StartPosition = FormStartPosition.CenterParent;
                ComboBox combo = new ComboBox();
                combo.DropDownStyle = ComboBoxStyle.DropDownList;
                combo.Width = versionForm.Width - 40;
                combo.Left = 20;
                combo.Top = 20;
                combo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                versionForm.Controls.Add(combo);

                string sSourceSafePath = (bContextButton1 ? txtCompare.Text : txtTo.Text);
                string sVersion = "";
                if (sSourceSafePath.StartsWith("$/"))
                {
                    if (sSourceSafePath.Contains(":"))
                    {
                        sVersion = sSourceSafePath.Substring(sSourceSafePath.IndexOf(':') + 1);
                        sSourceSafePath = sSourceSafePath.Substring(0, sSourceSafePath.IndexOf(':'));
                    }
                }
                else
                {
                    sSourceSafePath = DefaultSourceSafePath;
                }

                if (string.IsNullOrEmpty(sSourceSafePath))
                {
                    MessageBox.Show("Source control path does not exist.");
                    return;
                }

                bool bFoundVersion = false;
                foreach (string option in SmartDiffPlugin.GetSourceControlVersions(SourceSafeIniDirectory, sSourceSafePath, SourceControlProvider))
                {
                    int i = combo.Items.Add(option);
                    if (option.StartsWith(sVersion + " "))
                    {
                        combo.SelectedIndex = i;
                        bFoundVersion = true;
                    }
                }
                if (!bFoundVersion && combo.Items.Count > 0) combo.SelectedIndex = 0;

                Button ok = new Button();
                ok.Text = "OK";
                ok.Width = 80;
                ok.Left = combo.Right - ok.Width;
                ok.Top = combo.Bottom + 15;
                ok.Anchor = AnchorStyles.Right;
                ok.Click += new EventHandler(versionFormOK_Click);
                versionForm.Controls.Add(ok);

                versionForm.AcceptButton = ok;
                DialogResult res = versionForm.ShowDialog(this);
                if (res != DialogResult.OK) return;


                string sVersionSuffix = "";
                if (combo.SelectedIndex > 0)
                {
                        sVersion = combo.Items[combo.SelectedIndex].ToString().Substring(0, combo.Items[combo.SelectedIndex].ToString().IndexOf(' '));
                        sVersionSuffix = ":" + sVersion;
                }

                if (bContextButton1)
                {
                    txtCompare.Text = sSourceSafePath + sVersionSuffix;
                }
                else
                {
                    txtTo.Text = sSourceSafePath + sVersionSuffix;
                }
            }
            catch (Exception ex)
            {
                string sError = "";
                Exception exLoop = ex;
                while (exLoop != null)
                {
                    sError += exLoop.Message + "\r\n";
                    exLoop = exLoop.InnerException;
                }
                sError += ex.StackTrace;
                MessageBox.Show(sError, "BIDS Helper Smart Diff Error");
            }
        }

        private void versionFormOK_Click(object sender, EventArgs e)
        {
            try
            {
                Button b = (Button)sender;
                Form form = (Form)b.Parent;
                form.DialogResult = DialogResult.OK;
                form.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void windowsFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Choose File...";
                dlg.Filter = "All files (*.*)|*.*";
                dlg.CheckFileExists = true;
                dlg.InitialDirectory = System.IO.Directory.GetParent(DefaultWindowsPath).FullName;
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                DefaultWindowsPath = dlg.FileName;
                if (bContextButton1)
                {
                    txtCompare.Text = dlg.FileName;
                }
                else
                {
                    txtTo.Text = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}