using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper.SSAS.Tabular
{
    public partial class TabularAnnotationWorkaroundForm : Form
    {
        public TabularAnnotationWorkaroundForm(string sFileName)
        {
            InitializeComponent();
            label2.Text += sFileName;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://connect.microsoft.com/SQLServer/feedback/details/776444/tabular-model-error-during-opening-bim-after-sp1-readelementcontentas-methods-cannot-be-called-on-an-element-that-has-child-elements");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
