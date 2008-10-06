// Copyright (c) Microsoft Corporation.  All rights reserved. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using Microsoft.AnalysisServices;


namespace PCDimNaturalizer
{
    partial class frmASFlattener : Form
    {
        private frmASFlattenerOptions Options;

        public Microsoft.AnalysisServices.Server srv = new Server();
        public Microsoft.AnalysisServices.Database db = null;
        public Microsoft.AnalysisServices.Dimension dim = null;
        public Microsoft.DataWarehouse.Design.DataSourceConnection DataSourceConnection = null;

        public frmASFlattener()
        {
            InitializeComponent();
            
        }

        private void btnNaturalize_Click(object sender, EventArgs e)
        {
            try
            {
                if (VerifySelections())
                {
                    string ConnStr = dim.DataSource.ConnectionString;
                    this.Enabled = false;
                    Program.Progress = new frmProgress();
                    Program.Progress.ShowDialog(this); // The Progress form actually launches the naturalizer...
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                MessageBox.Show("Error accessing database server.\r\nVerify the selected server, database and dimension are correct.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool VerifyServer()
        {
            try
            {
                if (Options == null) return false;
                if (!srv.Connected || srv.ConnectionInfo.Server != txtServer.Text.Trim())
                {
                    if (srv.Connected)
                        srv.Disconnect();
                    srv.Connect("Integrated Security=SSPI;Persist Security Info=False;Data Source=" + txtServer.Text.Trim());
                }
                return true;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        public bool VerifySelections()
        {
            try
            {
                if (VerifyServer())
                {
                    if (srv.Databases.ContainsName(cmbDatabase.Text))
                        db = srv.Databases.GetByName(cmbDatabase.Text);
                    else 
                        return false;
                    if (db == null || !db.Dimensions.ContainsName(cmbDimension.Text))
                        return false;
                    dim = db.Dimensions.GetByName(cmbDimension.Text);
                    for (int i = 0; i < PCAttributesToInclude.Count; i++)
                        if (!dim.Attributes.ContainsName(PCAttributesToInclude[i]))
                        {
                            PCAttributesToInclude.RemoveAt(i);
                            i--;
                        }
                    for (int i = 0; i < NonPCHierarchiesToInclude.Count; i++)
                        if (!dim.Attributes.ContainsName(NonPCHierarchiesToInclude[i]))
                        {
                            NonPCHierarchiesToInclude.RemoveAt(i);
                            i--;
                        }
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void cmbDimension_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                VerifySelections();
            }
            catch (Exception)
            {
                MessageBox.Show("Error accessing database server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbDimension_Enter(object sender, EventArgs e)
        {
            if (cmbDatabase.Text.Trim() != "" || txtServer.Text.Trim() != "")
            {
                try
                {
                    if (VerifyServer())
                    {
                        db = srv.Databases.GetByName(cmbDatabase.Text);
                        cmbDimension.Items.Clear();
                        for (int i = 0; i < db.Dimensions.Count; i++)
                            if (db.Dimensions[i].IsParentChild) cmbDimension.Items.Add(db.Dimensions[i].Name);
                        cmbDimension.Sorted = true;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error accessing database server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void cmbDatabase_Enter(object sender, EventArgs e)
        {
            if (txtServer.Text.Trim() != "")
            {
                try
                {
                    if (VerifyServer())
                    {
                        cmbDatabase.Items.Clear();
                        for (int i = 0; i < srv.Databases.Count; i++)
                            cmbDatabase.Items.Add(srv.Databases[i].Name);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error accessing database server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void frmASFlattener_Load(object sender, EventArgs e)
        {
            cmbDatabase.SelectedIndex = 0;
            cmbDimension.SelectedIndex = 0;
            Options = new frmASFlattenerOptions();
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {

            Options.lbHierarchies.Items.Clear();
            Options.lbAttributes.Items.Clear();
            if (VerifySelections())
                if (dim != null)
                {
                    foreach (Hierarchy hier in dim.Hierarchies)
                        Options.lbHierarchies.Items.Add(new ctlFancyCheckedListBoxItem(hier.Name, true));
                    foreach (DimensionAttribute attr in dim.Attributes)
                    {
                        if (attr.Usage == AttributeUsage.Regular)
                        {
                            if (ASPCDimNaturalizer.IsAttributeRelated(attr, dim.KeyAttribute))
                                Options.lbAttributes.Items.Add(attr.Name);
                            Options.lbHierarchies.Items.Add(new ctlFancyCheckedListBoxItem(attr.Name, false));
                        }
                    }
                }

            for (int i = 0; i < Options.lbAttributes.Items.Count; i++)
                if (PCAttributesToInclude.Contains((string)Options.lbAttributes.Items[i]) || AddAllPCAttributes)
                    Options.lbAttributes.SetItemChecked(i, true);
            for (int i = 0; i < Options.lbHierarchies.Items.Count; i++)
                if (NonPCHierarchiesToInclude.Contains(((ctlFancyCheckedListBoxItem)Options.lbHierarchies.Items[i]).Text) || AddAllNonPCHierarchies)
                    Options.lbHierarchies.SetItemChecked(i, true);
            Options.tabControl1.SelectedIndex = 0;
            Options.numMinLevels.Value = MinLevels;
            Options.trkActionLevel.Value = ActionLevel;
            Options.ShowDialog();
        }

        // Various options, set to defaults initially
        public int MinLevels = 0;
        public int ActionLevel = 3;
        public List<string> NonPCHierarchiesToInclude = new List<string>(), PCAttributesToInclude = new List<string>();
        public bool AddAllNonPCHierarchies = true, AddAllPCAttributes = true;

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
   }
}