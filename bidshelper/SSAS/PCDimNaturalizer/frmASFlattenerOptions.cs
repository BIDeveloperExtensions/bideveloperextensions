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
    partial class frmASFlattenerOptions : Form
    {
        public frmASFlattenerOptions()
        {
            InitializeComponent();
        }

        private void trkActionLevel_Scroll(object sender, EventArgs e)
        {
            lblDescription.SuspendLayout();
            lblDescription.Text = "- Create a new SQL view for the naturalized dimension.";

            if (trkActionLevel.Value == 1)
                label1.Font = new Font(label1.Font, FontStyle.Bold);
            else
                label1.Font = new Font(label1.Font, FontStyle.Regular);
            if (trkActionLevel.Value == 2)
                label2.Font = new Font(label2.Font, FontStyle.Bold);
            else
                label2.Font = new Font(label2.Font, FontStyle.Regular);
            if (trkActionLevel.Value == 3)
                label3.Font = new Font(label3.Font, FontStyle.Bold);
            else
                label3.Font = new Font(label3.Font, FontStyle.Regular);
            if (trkActionLevel.Value == 4)
                label4.Font = new Font(label4.Font, FontStyle.Bold);
            else
                label4.Font = new Font(label4.Font, FontStyle.Regular);
            if (trkActionLevel.Value == 5)
                label5.Font = new Font(label5.Font, FontStyle.Bold);
            else
                label5.Font = new Font(label5.Font, FontStyle.Regular);

            if (trkActionLevel.Value >= 2)
                lblDescription.Text += "\r\n- Add new naturalized SQL view to Data Source View and create relationships parallel to original dimension.";
            if (trkActionLevel.Value >= 3)
                lblDescription.Text += "\r\n- Create a new dimension parallel to the original parent child dimension.";
            if (trkActionLevel.Value >= 4)
                lblDescription.Text += "\r\n- Add the naturalized dimension to the cube(s) of which the original dimension is a member and set dimension usage.";
            if (trkActionLevel.Value >= 5)
            {
                if (Program.ASFlattener.dim.ParentServer != null)
                {
                    lblDescription.Text += "\r\n- Process Full on the naturalized dimension and cube(s) associated with it.";
                }
                else
                {
                    MessageBox.Show("BIDS Helper only supports the Process action if working in online mode.", "BIDS Helper - Naturalizer Parent-Child Dimension");
                    trkActionLevel.Value = 4;
                    trkActionLevel_Scroll(null, null);
                }
            }

            lblDescription.ResumeLayout();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Program.ASFlattener.MinLevels = (int)numMinLevels.Value;
            Program.ASFlattener.ActionLevel = trkActionLevel.Value;
            Program.ASFlattener.AddAllNonPCHierarchies = chkAllHierarchies.CheckState == CheckState.Checked;
            Program.ASFlattener.AddAllPCAttributes = chkAllAttributes.CheckState == CheckState.Checked;
            Program.ASFlattener.NonPCHierarchiesToInclude.Clear();
            for (int i = 0; i < lbHierarchies.CheckedItems.Count; i++)
                Program.ASFlattener.NonPCHierarchiesToInclude.Add(lbHierarchies.CheckedItems[i].ToString());
            Program.ASFlattener.PCAttributesToInclude.Clear();
            for (int i = 0; i < lbAttributes.CheckedItems.Count; i++)
                Program.ASFlattener.PCAttributesToInclude.Add(lbAttributes.CheckedItems[i].ToString());
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkAllHierarchies_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAllHierarchies.CheckState != CheckState.Indeterminate)
            {
                lbHierarchies.ItemCheck -= lbHierarchies_ItemCheck;
                chkAllHierarchies.ThreeState = false;
                for (int i = 0; i < lbHierarchies.Items.Count; i++)
                    lbHierarchies.SetItemChecked(i, chkAllHierarchies.Checked);
                lbHierarchies.ItemCheck += lbHierarchies_ItemCheck;
            }
        }

        private void chkAllAttributes_CheckedChanged(object sender, EventArgs e)
        {            
            if (chkAllAttributes.CheckState != CheckState.Indeterminate)
            {
                lbAttributes.ItemCheck -= lbAttributes_ItemCheck;
                chkAllAttributes.ThreeState = false;
                for (int i = 0; i < lbAttributes.Items.Count; i++)
                    lbAttributes.SetItemChecked(i, chkAllAttributes.Checked);
                lbAttributes.ItemCheck += lbAttributes_ItemCheck;
            }
        }

        private List<string> AnyAttsOrHiersDependOnAtt(DimensionAttribute attr)
        {
            List<string> dependants = new List<string>();
            foreach (Hierarchy hier in Program.ASFlattener.dim.Hierarchies)
                foreach (Level lvl in hier.Levels)
                    if (lvl.SourceAttribute.Name == attr.Name)
                        dependants.Add(hier.Name);
            for (int i = 0; i < lbAttributes.Items.Count; i++)
                if (ASPCDimNaturalizer.IsAttributeRelated((Program.ASFlattener.dim.Attributes.FindByName((string)lbAttributes.Items[i])), attr))
                    dependants.Add((string)lbAttributes.Items[i]);
            return dependants;
        }

        private void lbAttributes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                lbAttributes.ItemCheck -= lbAttributes_ItemCheck;
                bool Announced = false;
                string attCheckedName = lbAttributes.Items[e.Index].ToString();
                if (e.NewValue != CheckState.Checked)
                {
                    foreach (string attName in AnyAttsOrHiersDependOnAtt(Program.ASFlattener.dim.Attributes.FindByName((string)lbAttributes.Items[e.Index])))
                    {
                        if (lbAttributes.CheckedItems.Contains(attName))
                        {
                            if (!Announced && lbAttributes.Visible) MessageBox.Show("One or more attribute is related to the dimension key indirectly by way of this one, so all related attributes will also be unselected.", "Indirect Key Attribute Relationship Detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Announced = true;
                        }
                        lbAttributes.SetItemChecked(lbAttributes.FindStringExact(attName), false);
                    }
                }
                else
                    if (!Program.ASFlattener.dim.KeyAttribute.AttributeRelationships.ContainsName(attCheckedName))
                    {
                        List<DimensionAttribute> atList = ASPCDimNaturalizer.GetAttrRelOwnerChainToKey(Program.ASFlattener.dim.Attributes.GetByName(attCheckedName));
                        foreach (DimensionAttribute attr in atList)
                            if (lbAttributes.Items.Contains(attr.Name) && !lbAttributes.CheckedItems.Contains(attr.Name) && attCheckedName != attr.Name)
                            {
                                if (!Announced && lbAttributes.Visible) MessageBox.Show("The selected attribute is related only indirectly to the key attribute for the dimension, so all its related attributes will also be selected.", "Indirect Key Attribute Relationship Detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                if (lbAttributes.Visible) Announced = true;
                                lbAttributes.SetItemChecked(lbAttributes.FindStringExact(attr.Name), true);
                            }
                    }
                lbAttributes.ItemCheck += lbAttributes_ItemCheck;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        private void lbHierarchies_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                lbHierarchies.ItemCheck -= lbHierarchies_ItemCheck;
                bool Announced = false;
                string attCheckedName = ((ctlFancyCheckedListBoxItem)lbHierarchies.Items[e.Index]).Text;

                if (!((ctlFancyCheckedListBoxItem)lbHierarchies.Items[e.Index]).Bold)
                {
                    if (e.NewValue != CheckState.Checked)
                    {
                        foreach (string attName in AnyAttsOrHiersDependOnAtt(Program.ASFlattener.dim.Attributes.FindByName(((ctlFancyCheckedListBoxItem)lbHierarchies.Items[e.Index]).Text)))
                        {
                            if (lbHierarchies.CheckedItems.Contains(attName))
                            {
                                if (!Announced && lbHierarchies.Visible) MessageBox.Show("One or more attributes are related to the dimension key indirectly by way of this one, so all related attributes will also be unselected.", "Indirect Key Attribute Relationship Detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Announced = true;
                            }
                            lbHierarchies.SetItemChecked(lbHierarchies.FindStringExact(attName), false);
                        }
                    }
                    else
                        if (!Program.ASFlattener.dim.KeyAttribute.AttributeRelationships.ContainsName(attCheckedName))
                        {
                            List<DimensionAttribute> atList = ASPCDimNaturalizer.GetAttrRelOwnerChainToKey(Program.ASFlattener.dim.Attributes.GetByName(attCheckedName));
                            foreach (DimensionAttribute attr in atList)
                                if (lbHierarchies.Items.Contains(attr.Name) && !lbHierarchies.CheckedItems.Contains(attr.Name) && attCheckedName != attr.Name)
                                {
                                    if (!Announced && lbHierarchies.Visible) MessageBox.Show("The selected attributes are related only indirectly to the key attribute for the dimension, so all its related attributes will also be selected.", "Indirect Key Attribute Relationship Detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    if (lbHierarchies.Visible) Announced = true;
                                    lbHierarchies.SetItemChecked(lbHierarchies.FindStringExact(attr.Name), true);
                                }
                        }
                }
                else
                {
                    if (e.NewValue == CheckState.Checked)
                        foreach (Level lvl in Program.ASFlattener.dim.Hierarchies.FindByName(((ctlFancyCheckedListBoxItem)lbHierarchies.Items[e.Index]).Text).Levels)
                            if (lbHierarchies.Items.Contains(lvl.SourceAttribute.Name) && !lbHierarchies.CheckedItems.Contains(lvl.SourceAttribute.Name))
                            {
                                if (!Announced && lbHierarchies.Visible) MessageBox.Show("The selected hierarchy contains attributes that were not selected, so all of them will also be selected.", "Unselected User Hierarchy Attributes Detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                if (lbHierarchies.Visible) Announced = true;
                                lbHierarchies.SetItemChecked(lbHierarchies.FindStringExact(lvl.SourceAttribute.Name), true);
                            }

                }
                lbHierarchies.ItemCheck += lbHierarchies_ItemCheck;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        private void lbAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkAllAttributes.ThreeState = false;
            if (lbAttributes.CheckedIndices.Count == lbAttributes.Items.Count) chkAllAttributes.CheckState = CheckState.Checked;
            else if (lbAttributes.CheckedIndices.Count == 0) chkAllAttributes.CheckState = CheckState.Unchecked;
            else
            {
                chkAllAttributes.ThreeState = true;
                chkAllAttributes.CheckState = CheckState.Indeterminate;
            }
        }

        private void lbHierarchies_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkAllHierarchies.ThreeState = false;
            if (lbHierarchies.CheckedIndices.Count == lbHierarchies.Items.Count) chkAllHierarchies.CheckState = CheckState.Checked;
            else if (lbHierarchies.CheckedIndices.Count == 0) chkAllHierarchies.CheckState = CheckState.Unchecked;
            else
            {
                chkAllHierarchies.ThreeState = true;
                chkAllHierarchies.CheckState = CheckState.Indeterminate;
            }
        }

        private void frmASFlattenerOptions_Load(object sender, EventArgs e)
        {
            this.richTextBox1.Rtf = @"{\rtf1\ansi (User hierarchies in \b bold\b0 .)}";
        }
    }
}
