namespace BIDSHelper.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Data;
    using System.Text;
    using System.Windows.Forms;
    using EnvDTE;
    using BIDSHelper.SSIS;
    
    public partial class BIDSHelperPreferencesPage : UserControl, EnvDTE.IDTToolsOptionsPage
    {
        public BIDSHelperPreferencesPage()
        {
            InitializeComponent();
        }


        #region IDTToolsOptionsPage Members


        void IDTToolsOptionsPage.GetProperties(ref object PropertiesObject)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        void IDTToolsOptionsPage.OnAfterCreated(DTE DTEObject)
        {
            this.Width = 395; //392
            this.Height = 290;

            lblTFS.Text = "TFS: " + (SmartDiffPlugin.TFSInstalled ? "Installed" : "Not Installed");
            lblVSS.Text = "VSS: " + (SmartDiffPlugin.VSSInstalled ? "Installed" : "Not Installed");

            if (string.IsNullOrEmpty(SmartDiffPlugin.CustomDiffViewer))
            {
                radSmartDiffDefault.Checked = true;
                txtSmartDiffCustom.Text = "\"C:\\Program Files\\Microsoft Visual Studio 9.0\\Common7\\IDE\\diffmerge.exe\" ? ? /ignoreeol /ignorespace";
            }
            else
            {
                radSmartDiffCustom.Checked = true;
                txtSmartDiffCustom.Text = SmartDiffPlugin.CustomDiffViewer;
            }
            FixSmartDiffCustomEnabled();

            btnExpressionColor.BackColor = ExpressionHighlighterPlugin.ExpressionColor;
            btnConfigurationColor.BackColor = ExpressionHighlighterPlugin.ConfigurationColor;

            txtFreeSpaceFactor.Text = MeasureGroupHealthCheckPlugin.FreeSpaceFactor.ToString();
        }

        void IDTToolsOptionsPage.OnCancel()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IDTToolsOptionsPage.OnHelp()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IDTToolsOptionsPage.OnOK()
        {
            try
            {
                if (radSmartDiffCustom.Checked)
                {
                    SmartDiffPlugin.CustomDiffViewer = txtSmartDiffCustom.Text;
                }
                else
                {
                    SmartDiffPlugin.CustomDiffViewer = null;
                }

                ExpressionHighlighterPlugin.ExpressionColor = btnExpressionColor.BackColor;
                ExpressionHighlighterPlugin.ConfigurationColor = btnConfigurationColor.BackColor;

                int iFreeSpaceFactor;
                if (int.TryParse(txtFreeSpaceFactor.Text, out iFreeSpaceFactor))
                    MeasureGroupHealthCheckPlugin.FreeSpaceFactor = iFreeSpaceFactor;
                //BIDSHelperPluginBase pu = null;
                ////foreach (object itm in lstPlugins.Items) //(BIDSHelperPluginReference puRef in Connect.Plugins.Values)
                //for (int i = 0;i<lstPlugins.Items.Count;i++)
                //{
                //    pu = (BIDSHelperPluginBase)lstPlugins.Items[i];
                //    if (pu.Enabled != lstPlugins.GetItemChecked(i))
                //    {
                //        pu.Enabled = lstPlugins.GetItemChecked(i);
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        #endregion

        private void BIDSHelperPreferencesPage_Load(object sender, EventArgs e)
        {

        }

        private void radSmartDiffDefault_CheckedChanged(object sender, EventArgs e)
        {
            FixSmartDiffCustomEnabled();
        }

        private void FixSmartDiffCustomEnabled()
        {
            txtSmartDiffCustom.Enabled = radSmartDiffCustom.Checked;
        }

        private void radSmartDiffCustom_CheckedChanged(object sender, EventArgs e)
        {
            FixSmartDiffCustomEnabled();
        }

        private void btnExpressionColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = btnExpressionColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnExpressionColor.BackColor = colorDialog1.Color;
            }
        }

        private void btnConfigurationColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = btnConfigurationColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnConfigurationColor.BackColor = colorDialog1.Color;
            }
        }

        private void linkResetExpressionColors_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ExpressionHighlighterPlugin.ExpressionColor = ExpressionHighlighterPlugin.ExpressionColorDefault;
            btnExpressionColor.BackColor = ExpressionHighlighterPlugin.ExpressionColorDefault;

            ExpressionHighlighterPlugin.ConfigurationColor = ExpressionHighlighterPlugin.ConfigurationColorDefault;
            btnConfigurationColor.BackColor = ExpressionHighlighterPlugin.ConfigurationColorDefault;
        }

        private void txtFreeSpaceFactor_Leave(object sender, EventArgs e)
        {
            int i;
            if (!int.TryParse(txtFreeSpaceFactor.Text, out i))
                txtFreeSpaceFactor.Text = MeasureGroupHealthCheckPlugin.FreeSpaceFactor.ToString();
        }
    }

}
