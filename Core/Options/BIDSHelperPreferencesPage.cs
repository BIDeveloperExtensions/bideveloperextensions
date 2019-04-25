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
            try
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

                if (BIDSHelperPackage.SSISExtensionVersion != null)
                {
                    // Expression & Configuration highlighter
                    btnExpressionColor.BackColor = ExpressionHighlighterPlugin.ExpressionColor;
                    btnConfigurationColor.BackColor = ExpressionHighlighterPlugin.ConfigurationColor;

                    // Expression editor, font and colours
                    buttonExpressionFontSample.Font = ExpressionListPlugin.ExpressionFont;
                    buttonExpressionFontSample.ForeColor = ExpressionListPlugin.ExpressionColor;
                    buttonResultFontSample.Font = ExpressionListPlugin.ResultFont;
                    buttonResultFontSample.ForeColor = ExpressionListPlugin.ResultColor;
                }
                else
                {
                    btnExpressionColor.Enabled = false;
                    btnConfigurationColor.Enabled = false;
                    buttonExpressionFontSample.Enabled = false;
                    buttonResultFontSample.Enabled = false;
                    linkResetExpressionColors.Enabled = false;
                }

                // Measure group free space
                if (BIDSHelperPackage.SSASExtensionVersion != null)
                    txtFreeSpaceFactor.Text = MeasureGroupHealthCheckPlugin.FreeSpaceFactor.ToString();
                else
                    txtFreeSpaceFactor.Enabled = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
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
                if (BIDSHelperPackage.SSISExtensionVersion != null)
                {
                    ExpressionHighlighterPlugin.ExpressionColor = btnExpressionColor.BackColor;
                    ExpressionHighlighterPlugin.ConfigurationColor = btnConfigurationColor.BackColor;

                    // Expression editor, font and colours
                    ExpressionListPlugin.ExpressionFont = buttonExpressionFontSample.Font;
                    ExpressionListPlugin.ExpressionColor = buttonExpressionFontSample.ForeColor;
                    ExpressionListPlugin.ResultFont = buttonResultFontSample.Font;
                    ExpressionListPlugin.ResultColor = buttonResultFontSample.ForeColor;
                }

                // Measure group free space
                int iFreeSpaceFactor;
                if (txtFreeSpaceFactor.Enabled && int.TryParse(txtFreeSpaceFactor.Text, out iFreeSpaceFactor))
                {
                    MeasureGroupHealthCheckPlugin.FreeSpaceFactor = iFreeSpaceFactor;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void radSmartDiffDefault_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                FixSmartDiffCustomEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void FixSmartDiffCustomEnabled()
        {
            txtSmartDiffCustom.Enabled = radSmartDiffCustom.Checked;
        }

        private void radSmartDiffCustom_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                FixSmartDiffCustomEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void btnExpressionColor_Click(object sender, EventArgs e)
        {
            try
            {
                colorDialog.Color = btnExpressionColor.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    btnExpressionColor.BackColor = colorDialog.Color;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void btnConfigurationColor_Click(object sender, EventArgs e)
        {
            try
            {
                colorDialog.Color = btnConfigurationColor.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    btnConfigurationColor.BackColor = colorDialog.Color;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void linkResetExpressionColors_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                ExpressionHighlighterPlugin.ExpressionColor = ExpressionHighlighterPlugin.ExpressionColorDefault;
                btnExpressionColor.BackColor = ExpressionHighlighterPlugin.ExpressionColorDefault;

                ExpressionHighlighterPlugin.ConfigurationColor = ExpressionHighlighterPlugin.ConfigurationColorDefault;
                btnConfigurationColor.BackColor = ExpressionHighlighterPlugin.ConfigurationColorDefault;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void txtFreeSpaceFactor_Leave(object sender, EventArgs e)
        {
            try
            {
                int i;
                if (!int.TryParse(txtFreeSpaceFactor.Text, out i))
                    txtFreeSpaceFactor.Text = MeasureGroupHealthCheckPlugin.FreeSpaceFactor.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void buttonExpressionFont_Click(object sender, EventArgs e)
        {
            try
            {
                fontDialog.Font = buttonExpressionFontSample.Font;
                fontDialog.Color = buttonExpressionFontSample.ForeColor;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    buttonExpressionFontSample.Font = fontDialog.Font;
                    buttonExpressionFontSample.ForeColor = fontDialog.Color;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void buttonResultFont_Click(object sender, EventArgs e)
        {
            try
            {
                fontDialog.Font = buttonResultFontSample.Font;
                fontDialog.Color = buttonResultFontSample.ForeColor;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    buttonResultFontSample.Font = fontDialog.Font;
                    buttonResultFontSample.ForeColor = fontDialog.Color;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }

}
