/*============================================================================
  File:    frmTraceTable.cs

  Summary: Contains the form to connect to a SQL table containing trace data
============================================================================*/
/*
 * This file created new for BIDSHelper. 
 *    http://www.codeplex.com/BIDSHelper
 * 
 * It is not part of the official Agg Manager version: 
 * http://www.codeplex.com/MSFTASProdSamples                                   
 *                                                                             
 ============================================================================*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace AggManager
{
    public partial class TraceTable : Form
    {
        public string ConnectionString;
        public string Table;

        private SqlConnection conn = new SqlConnection();

        public TraceTable()
        {
            InitializeComponent();
        }

        private void TraceTable_Load(object sender, EventArgs e)
        {
            this.comboAuthentication.SelectedIndex = 0;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(textServer.Text))
                    MessageBox.Show("Must enter a server name");
                else if (string.IsNullOrEmpty(comboDatabase.Text))
                    MessageBox.Show("Must enter a database name");
                else if (string.IsNullOrEmpty(comboSchema.Text))
                    MessageBox.Show("Must enter a schema name");
                else if (string.IsNullOrEmpty(comboTable.Text))
                    MessageBox.Show("Must enter a table name");
                else
                {
                    Table = "[" + comboDatabase.Text + "].[" + comboSchema.Text + "].[" + comboTable.Text + "]";
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textServer_TextChanged(object sender, EventArgs e)
        {
            RefreshConnectionString();
        }


        private void textUsername_TextChanged(object sender, EventArgs e)
        {
            RefreshConnectionString();
        }

        private void textPassword_TextChanged(object sender, EventArgs e)
        {
            RefreshConnectionString();
        }

        private void comboAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshConnectionString();
            if (comboAuthentication.SelectedIndex == 0)
            {
                textUsername.Enabled = false;
                textPassword.Enabled = false;
                labelUsername.Enabled = false;
                labelPassword.Enabled = false;
            }
            else
            {
                textUsername.Enabled = true;
                textPassword.Enabled = true;
                labelUsername.Enabled = true;
                labelPassword.Enabled = true;
            }
        }

        private void RefreshConnectionString()
        {
            try
            {
                string sConnectionString = "Data Source=" + textServer.Text + ";";
                if (comboAuthentication.SelectedIndex == 0)
                    sConnectionString += "Integrated Security=SSPI;";
                else
                    sConnectionString += "Uid=" + textUsername.Text + ";Pwd=" + textPassword.Text + ";";
                if (!string.IsNullOrEmpty(comboDatabase.Text))
                    sConnectionString += "Initial Catalog=" + comboDatabase.Text;
                if (sConnectionString != ConnectionString)
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    ConnectionString = sConnectionString;
                    if (conn == null)
                        conn = new SqlConnection();
                    conn.ConnectionString = ConnectionString;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboDatabase_DropDown(object sender, EventArgs e)
        {
            try
            {
                comboDatabase.Items.Clear();
                if (conn.State != ConnectionState.Open) conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select name from master..sysdatabases where dbid not in (2,3) order by name";
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    comboDatabase.Items.Add(reader["name"]);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshConnectionString();
            RefreshSchemas();
        }

        private void RefreshSchemas()
        {
            try
            {
                comboSchema.Items.Clear();
                if (conn.State != ConnectionState.Open) conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = @"select name
                    ,IsDefault = cast(case when (select default_schema_name from sys.database_principals p where p.name = user_name() and p.default_schema_name = s.name) is not null then 1 else 0 end as bit)
                    from sys.schemas s
                    order by name";
                SqlDataReader reader = cmd.ExecuteReader();
                int iSelectedIndex = -1;
                while (reader.Read())
                {
                    int i = comboSchema.Items.Add(reader["name"]);
                    if (Convert.ToBoolean(reader["IsDefault"])) iSelectedIndex = i;
                }
                reader.Close();
                if (iSelectedIndex != -1) comboSchema.SelectedIndex = iSelectedIndex;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboSchema_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                comboTable.Items.Clear();
                if (conn.State != ConnectionState.Open) conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select st.name from sys.tables st, sys.schemas ss where st.name not like '#%%' and st.type='U' and ss.schema_id=st.schema_id and not st.is_ms_shipped=1 and ss.name=@Schema order by st.name";
                cmd.Parameters.Add("@Schema", SqlDbType.VarChar).Value = comboSchema.Text;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    comboTable.Items.Add(reader["name"]);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}