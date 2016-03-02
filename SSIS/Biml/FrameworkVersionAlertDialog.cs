using System;
using System.Windows.Forms;

namespace BIDSHelper.SSIS.Biml
{
    public partial class FrameworkVersionAlertDialog : Form
    {
        public FrameworkVersionAlertDialog()
        {
            InitializeComponent();
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(BIDSHelper.Resources.Common.BimlFrameworkVersionAlert);
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.microsoft.com/downloads/en/details.aspx?FamilyId=333325fd-ae52-4e35-b531-508d977d32a6");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.microsoft.com/downloads/en/details.aspx?FamilyId=333325fd-ae52-4e35-b531-508d977d32a6");
        }
    }
}
