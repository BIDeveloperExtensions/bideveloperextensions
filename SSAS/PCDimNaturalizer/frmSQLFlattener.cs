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

namespace PCDimNaturalizer
{
    partial class frmSQLFlattener : Form
    {
        public Microsoft.AnalysisServices.DataSourceView dsv = null;
        public int MinLevels = 0;
        public bool AddAllAttributesNatural = false, AddAllAttributesPC = true;
        public List<string> AttributesNatural = new List<string>(), AttributesPC = new List<string>();
        public Microsoft.DataWarehouse.Design.DataSourceConnection DataSourceConnection = null;

        private frmSQLFlattenerOptions Options;
        public OleDbConnection Conn;
        public DataRow[] Columns;

        public frmSQLFlattener()
        {
            InitializeComponent();
        }

        private void btnNaturalize_Click(object sender, EventArgs e)
        {
            if (VerifyColumns())
            {
                this.Enabled = false;
                Program.Progress = new frmProgress();
                Program.Progress.ShowDialog(this);
                this.Close();
            }
            else
                MessageBox.Show("Error accessing database server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool VerifyServer()
        {
            /*
            if (Options == null) return false;
            if (Conn == null || Conn.DataSource != txtServer.Text.Trim())
            {
                Conn = new OleDbConnection("Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Data Source=" + txtServer.Text.Trim());
                try
                {
                    Conn.Open();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
             */
                return true;
        }

        private bool VerifyColumns()
        {
            if (VerifyServer())
            {
                if (Columns == null || Columns.Length == 0 || 
                    "[" + Columns[0].ItemArray[Columns[0].Table.Columns.IndexOf("TABLE_SCHEMA")].ToString() + "].[" +
                    Columns[0].ItemArray[Columns[0].Table.Columns.IndexOf("TABLE_NAME")] + "]" 
                    != cmbTable.Text)
                try
                {
                    if (Conn.Database != cmbDatabase.Text.Trim())
                        Conn.ChangeDatabase(cmbDatabase.Text.Trim());
                    int iDotLoc = cmbTable.Text.Trim().IndexOf('.');
                    DataTable dt = Conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                        new object[] { null, 
                        cmbTable.Text.Trim().Substring(0, iDotLoc), 
                        cmbTable.Text.Trim().Substring(iDotLoc + 1, cmbTable.Text.Length - iDotLoc - 1), 
                        null });
                    Columns = dt.Select();
                    for (int i = 0; i < AttributesPC.Count; i++)
                        if (!Columns[0].Table.Columns.Contains(AttributesPC[i]))
                        {
                            AttributesPC.RemoveAt(i);
                            i--;
                        }
                    for (int i = 0; i < AttributesNatural.Count; i++)
                        if (!Columns[0].Table.Columns.Contains(AttributesNatural[i]))
                        {
                            AttributesNatural.RemoveAt(i);
                            i--;
                        }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                else
                    return true;
            }
            else
                return false;
        }

        private void cmbDatabase_Enter(object sender, EventArgs e)
        {
            if (VerifyServer())
            {
                DataTable dt = Conn.GetOleDbSchemaTable(OleDbSchemaGuid.Catalogs, null);
                DataRow[] dr = dt.Select();
                cmbDatabase.Items.Clear();
                for (int i = 0; i < dr.Length; i++)
                    cmbDatabase.Items.Add(dr[i].ItemArray[0]);
            }
            else
                MessageBox.Show("Error accessing database server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void cmbTable_Enter(object sender, EventArgs e)
        {
            if (VerifyServer())
            {
                try
                {
                    if (Conn.Database != cmbDatabase.Text.Trim())
                        Conn.ChangeDatabase(cmbDatabase.Text.Trim());
                    DataTable dt = Conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                    DataRow[] dr = dt.Select();
                    cmbTable.Items.Clear();
                    for (int i = 0; i < dr.Length; i++)
                        cmbTable.Items.Add("[" + dr[i].ItemArray[1] + "].[" + dr[i].ItemArray[2] + "]");
                    dt = Conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "VIEW" });
                    dr = dt.Select();
                    for (int i = 0; i < dr.Length; i++)
                        cmbTable.Items.Add("[" + dr[i].ItemArray[1] + "].[" + dr[i].ItemArray[2] + "]");
                    cmbTable.Sorted = true;
                    cmbID.Enabled = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Error accessing database server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Error accessing database server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool IsOleDbTypeValidForIDCol(OleDbType ot)
        {
            if (ot == OleDbType.Integer
                || ot == OleDbType.BigInt
                || ot == OleDbType.SmallInt
                || ot == OleDbType.TinyInt
                || ot == OleDbType.Guid) return true;
            else return false;
        }

        public void cmbID_Enter(object sender, EventArgs e)
        {
            if (VerifyColumns())
            {
                cmbID.Items.Clear();
                ArrayList al = new ArrayList();
                for (int i = 0; i < Columns.Length; i++)
                    if (cmbPID.Text.Trim() != Columns[i].ItemArray[Columns[i].Table.Columns.IndexOf("COLUMN_NAME")].ToString().Trim()
                        && IsOleDbTypeValidForIDCol((OleDbType)Columns[i].ItemArray[Columns[i].Table.Columns.IndexOf("DATA_TYPE")]))
                        cmbID.Items.Add(Columns[i].ItemArray[Columns[i].Table.Columns.IndexOf("COLUMN_NAME")].ToString());
                cmbID.Sorted = true;
            }
            else
                MessageBox.Show("Error accessing database server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void cmbPID_Enter(object sender, EventArgs e)
        {
            if (VerifyColumns())
            {
                cmbPID.Items.Clear();
                ArrayList al = new ArrayList();
                for (int i = 0; i < Columns.Length; i++)
                    if (cmbID.Text.Trim() != Columns[i].ItemArray[Columns[i].Table.Columns.IndexOf("COLUMN_NAME")].ToString().Trim()
                        && IsOleDbTypeValidForIDCol((OleDbType)Columns[i].ItemArray[Columns[i].Table.Columns.IndexOf("DATA_TYPE")]))
                        cmbPID.Items.Add(Columns[i].ItemArray[Columns[i].Table.Columns.IndexOf("COLUMN_NAME")].ToString());
                cmbPID.Sorted = true;
            }
            else
                MessageBox.Show("Error accessing database server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void frmSQLFlattener_Load(object sender, EventArgs e)
        {
            cmbDatabase.SelectedIndex = 0;
            cmbTable.SelectedIndex = 0;
            cmbID.SelectedIndex = 0;
            cmbPID.SelectedIndex = 0;
            Options = new frmSQLFlattenerOptions();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            try
            {
                if (VerifyServer())
                {
                    if (Conn.Database != cmbDatabase.Text.Trim())
                        Conn.ChangeDatabase(cmbDatabase.Text.Trim());
                    int iDotLoc = cmbTable.Text.Trim().IndexOf('.');
                    DataTable dt = Conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                       new object[] {  null, 
                                    cmbTable.Text.Trim().Substring(0, iDotLoc), 
                                    cmbTable.Text.Trim().Substring(iDotLoc + 1, cmbTable.Text.Length - iDotLoc - 1), 
                                    null });
                    DataRow[] dr = dt.Select();
                    Options.lbPCAttrCols.Items.Clear();
                    Options.lbNaturalAttrCols.Items.Clear();
                    for (int i = 0; i < dr.Length; i++)
                        if (cmbPID.Text.Trim() != dr[i].ItemArray[dr[i].Table.Columns.IndexOf("COLUMN_NAME")].ToString().Trim()
                            && cmbID.Text.Trim() != dr[i].ItemArray[dr[i].Table.Columns.IndexOf("COLUMN_NAME")].ToString().Trim())
                        {
                            string col = dr[i].ItemArray[dr[i].Table.Columns.IndexOf("COLUMN_NAME")].ToString();
                            Options.lbPCAttrCols.Items.Add(col, AttributesPC.Contains(col));
                            Options.lbNaturalAttrCols.Items.Add(col, AttributesNatural.Contains(col));
                        }

                    Options.tabControl1.Enabled = true;
                }
            }
            catch (Exception) 
            {
                Options.tabControl1.Enabled = false;
            }
            Options.ShowDialog();
        }
    }
}