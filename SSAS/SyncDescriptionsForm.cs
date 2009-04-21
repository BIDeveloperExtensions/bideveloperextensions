using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper.SSAS
{
    public partial class SyncDescriptionsForm : Form
    {
        public SyncDescriptionsForm()
        {
            InitializeComponent();
        }

        private void SyncDescriptionsForm_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < cmbDescriptionProperty.Items.Count; i++)
            {
                if (string.Compare(cmbDescriptionProperty.GetItemText(cmbDescriptionProperty.Items[i]), "Description", true) == 0 || string.Compare(cmbDescriptionProperty.GetItemText(cmbDescriptionProperty.Items[i]), "MS_Description", true) == 0)
                {
                    cmbDescriptionProperty.SelectedIndex = i;
                    HideOtherProperties();
                }
            }
        }

        private void HideOtherProperties()
        {
            for (int i = 0; i < cmbDescriptionProperty.Items.Count && i < listOtherProperties.Items.Count; i++)
            {
                if (cmbDescriptionProperty.GetItemText(cmbDescriptionProperty.SelectedItem) == listOtherProperties.GetItemText(listOtherProperties.Items[i]))
                {
                    listOtherProperties.SetItemChecked(i, false);
                }
            }
        }

        private void cmbDescriptionProperty_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideOtherProperties();
        }

        private void listOtherProperties_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideOtherProperties();
        }

        private void listOtherProperties_Click(object sender, EventArgs e)
        {
            HideOtherProperties();
        }

    }
}
