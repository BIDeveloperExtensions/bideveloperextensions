using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper.SSAS
{
    public partial class CustomDesignRulesCheckerForm : Form
    {
        public CustomDesignRulesCheckerForm()
        {
            InitializeComponent();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            chkFunctions.CheckAllItems();
            /*
            for (int item = 0; item < chkFunctions.Items.Count; item++)
            {
                chkFunctions.SetItemChecked(item, true);
            }*/
        }

        private void btnSelectNone_Click(object sender, EventArgs e)
        {
            chkFunctions.UncheckAllItems();
            /*for (int item = 0; item < chkFunctions.Items.Count; item++)
            {
                chkFunctions.SetItemChecked(item, false);
            }*/
        }

        public List<string> Functions {
            get
            {
                return (from object itm in chkFunctions.SelectedItems select itm.ToString()).ToList();
            }
            set { 
                chkFunctions.Items.AddRange(value.ToArray()); 
                }
            }
        public CustomDesignRulesPlugin Plugin;

        private void btnClearErrors_Click(object sender, EventArgs e)
        {
            Plugin.ClearErrorList();
        }

        private void CustomDesignRulesCheckerForm_Activated(object sender, EventArgs e)
        {
            chkFunctions.CheckAllItems();
        }

        
 
    }

    public static class AppExtensions
    {
        public static void UncheckAllItems(this System.Windows.Forms.CheckedListBox clb)
        {
            while (clb.CheckedIndices.Count > 0)
                clb.SetItemChecked(clb.CheckedIndices[0], false);
        }
    
        public static void CheckAllItems(this System.Windows.Forms.CheckedListBox clb)
        {
            while (clb.CheckedIndices.Count < clb.Items.Count)
                clb.SetItemChecked(clb.CheckedIndices[0], true);
        }
    }
}
