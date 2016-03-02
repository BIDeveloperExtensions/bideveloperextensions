using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;

namespace PCDimNaturalizer
{
    partial class frmSQLFlattenerOptions : Form
    {
        public frmSQLFlattenerOptions()
        {
            InitializeComponent();
        }

        private void frmSQLFlattenerOptions_Load(object sender, EventArgs e)
        {
            chkAllAttributesPC.Checked = Program.SQLFlattener.AddAllAttributesPC;
            if (chkAllAttributesPC.Checked) lbPCAttrCols.Enabled = false;
            chkAllAttributesNatural.Checked = Program.SQLFlattener.AddAllAttributesNatural;
            if (chkAllAttributesNatural.Checked) lbNaturalAttrCols.Enabled = false;
            tabControl1.SelectedIndex = 0;

            numMinLevels.Value = Program.SQLFlattener.MinLevels;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Program.SQLFlattener.MinLevels = (int)numMinLevels.Value;
            Program.SQLFlattener.AddAllAttributesNatural = chkAllAttributesNatural.Checked;
            Program.SQLFlattener.AddAllAttributesPC = chkAllAttributesPC.Checked;
            if (Program.SQLFlattener.AttributesNatural != null)
            {
                Program.SQLFlattener.AttributesNatural.Clear();
                for (int i = 0; i < lbNaturalAttrCols.Items.Count; i++)
                    if (lbNaturalAttrCols.GetItemChecked(i) || chkAllAttributesNatural.Checked) Program.SQLFlattener.AttributesNatural.Add(lbNaturalAttrCols.Items[i].ToString());
            }
            if (Program.SQLFlattener.AttributesPC != null)
            {
                Program.SQLFlattener.AttributesPC.Clear();
                for (int i = 0; i < lbPCAttrCols.CheckedItems.Count; i++)
                    Program.SQLFlattener.AttributesPC.Add(lbPCAttrCols.CheckedItems[i].ToString());
            }           
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkAllHierarchies_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAllAttributesNatural.Checked) lbNaturalAttrCols.Enabled = false; else lbNaturalAttrCols.Enabled = true;
        }

        private void chkAllAttributes_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAllAttributesPC.Checked) lbPCAttrCols.Enabled = false; else lbPCAttrCols.Enabled = true;
        }

        private void lbAttributes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
        }
    }
}
