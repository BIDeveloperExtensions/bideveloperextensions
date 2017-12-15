namespace BIDSHelper.Core
{
    using BIDSHelper.SSIS;
    using System;
    using System.Windows.Forms;

    public partial class BIDSHelperPreferencesPage : UserControl
    {
        public BIDSHelperPreferencesPage()
        {
            InitializeComponent();
        }

        public BIDSHelperPreferencesDialogPage OptionsPage
        {
            get;
            set;
        }

        public void Initialize()
        {
            this.Width = 395; //392
            this.Height = 290;

            // Smart Diff
            lblTFS.Text = "Visual Studio Built-in";
            lblVSS.Text = string.Empty;

            if (string.IsNullOrEmpty(SmartDiffPlugin.CustomDiffViewer))
            {
                radSmartDiffDefault.Checked = true;
                txtSmartDiffCustom.Text = "\"C:\\Program Files (x86)\\Microsoft Visual Studio 9.0\\Common7\\IDE\\diffmerge.exe\" ? ? /ignoreeol /ignorespace";
            }
            else
            {
                radSmartDiffCustom.Checked = true;
                txtSmartDiffCustom.Text = SmartDiffPlugin.CustomDiffViewer;
            }

            FixSmartDiffCustomEnabled();

            // Expression & Configuration highlighter
            btnExpressionColor.BackColor = ExpressionHighlighterPlugin.ExpressionColor;
            btnConfigurationColor.BackColor = ExpressionHighlighterPlugin.ConfigurationColor;

            // Measure group free space
            txtFreeSpaceFactor.Text = MeasureGroupHealthCheckPlugin.FreeSpaceFactor.ToString();

            // Expression editor, font and colours
            buttonExpressionFontSample.Font = ExpressionListPlugin.ExpressionFont;
            buttonExpressionFontSample.ForeColor = ExpressionListPlugin.ExpressionColor;
            buttonResultFontSample.Font = ExpressionListPlugin.ResultFont;
            buttonResultFontSample.ForeColor = ExpressionListPlugin.ResultColor;
        }

        public void Apply()
        {
            try
            {
                // Smart Diff
                if (radSmartDiffCustom.Checked)
                {
                    SmartDiffPlugin.CustomDiffViewer = txtSmartDiffCustom.Text;
                }
                else
                {
                    SmartDiffPlugin.CustomDiffViewer = null;
                }

                // Expression & Configuration highlighter
                ExpressionHighlighterPlugin.ExpressionColor = btnExpressionColor.BackColor;
                ExpressionHighlighterPlugin.ConfigurationColor = btnConfigurationColor.BackColor;

                // Measure group free space
                int iFreeSpaceFactor;
                if (int.TryParse(txtFreeSpaceFactor.Text, out iFreeSpaceFactor))
                {
                    MeasureGroupHealthCheckPlugin.FreeSpaceFactor = iFreeSpaceFactor;
                }

                // Expression editor, font and colours
                ExpressionListPlugin.ExpressionFont = buttonExpressionFontSample.Font;
                ExpressionListPlugin.ExpressionColor = buttonExpressionFontSample.ForeColor;
                ExpressionListPlugin.ResultFont = buttonResultFontSample.Font;
                ExpressionListPlugin.ResultColor = buttonResultFontSample.ForeColor;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
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
            colorDialog.Color = btnExpressionColor.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                btnExpressionColor.BackColor = colorDialog.Color;
            }
        }

        private void btnConfigurationColor_Click(object sender, EventArgs e)
        {
            colorDialog.Color = btnConfigurationColor.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                btnConfigurationColor.BackColor = colorDialog.Color;
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

        private void buttonExpressionFont_Click(object sender, EventArgs e)
        {
            fontDialog.Font = buttonExpressionFontSample.Font;
            fontDialog.Color = buttonExpressionFontSample.ForeColor;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                buttonExpressionFontSample.Font = fontDialog.Font;
                buttonExpressionFontSample.ForeColor = fontDialog.Color;
            }
        }

        private void buttonResultFont_Click(object sender, EventArgs e)
        {
            fontDialog.Font = buttonResultFontSample.Font;
            fontDialog.Color = buttonResultFontSample.ForeColor;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                buttonResultFontSample.Font = fontDialog.Font;
                buttonResultFontSample.ForeColor = fontDialog.Color;
            }

        }
    }

}
