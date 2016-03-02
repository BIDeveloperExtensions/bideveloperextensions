/*============================================================================
  File:    frmExportTable.cs

  Summary: Contains the form to connect to a SQL table containing trace data
============================================================================*/
/*
 * This file created new for BIDSHelper. 
 *    http://www.codeplex.com/BIDSHelper
 * 
 * It is not part of the official Agg Manager version: 
 * http://www.codeplex.com/MSFTASProdSamples                                   
 *                                                                             
 * Export to SQL Table code thanks to Leandro Tubia
 ============================================================================*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AnalysisServices;


namespace AggManager
{
    public partial class ExportTable : Form
    {
        public string ConnectionString;
        public string Table;
        public AggregationDesign aggrDesign;

        private SqlConnection conn = new SqlConnection();

        public ExportTable()
        {
            InitializeComponent();
        }

        public void Init(AggregationDesign aggrD)
        {
            this.aggrDesign  = aggrD;
        }

        private void ExportTable_Load(object sender, EventArgs e)
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

                    bool bExistsTable = ExistsTable();
                    bool bDropTable = false;

                    if (bExistsTable)
                    {
                        DialogResult res = MessageBox.Show("Table " + Table + " already exists.\r\n\r\nClick Yes to overwrite it.\r\nClick No to append to it.\r\nClick Cancel to abort.", "Export Aggregations", MessageBoxButtons.YesNoCancel);

                        if (res == DialogResult.Cancel) return;
                        if (res == DialogResult.Yes) bDropTable = true;
                    }
                    SaveAggregations(bDropTable, !bExistsTable || bDropTable);
                    MessageBox.Show("Aggregation design [" + aggrDesign.Name + "] successfully saved in table " + Table);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private Boolean ExistsTable()
        {
            string strSQL;
            Boolean bExists;
            if (conn.State != ConnectionState.Open) conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            strSQL = "select name from " + comboDatabase.Text + "..sysobjects where type = 'U' And name = @name";
            cmd.CommandText = strSQL;
            cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = comboTable.Text;
            SqlDataReader reader = cmd.ExecuteReader();
            bExists = reader.HasRows;
            reader.Close();
            return bExists;
        }

        private void SaveAggregations(bool bDropTable, bool bCreateTable)
        {
            String strSQL;
            if (conn.State != ConnectionState.Open) conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;

            SqlTransaction tran = conn.BeginTransaction();
            cmd.Transaction = tran;
            try
            {
                // Drop table if exists
                if (bDropTable)
                {
                    strSQL = "Drop table " + Table;
                    cmd.CommandText = strSQL;
                    cmd.ExecuteNonQuery();
                    bCreateTable = true;
                }

                if (bCreateTable)
                {
                    // Create table
                    strSQL = "Create Table " + Table + " ( CubeName varchar(255), MeasureGroupName varchar(255), AggregationDesignName varchar(255), AggregationName varchar(255), Dataset varchar(4000))";
                    cmd.CommandText = strSQL;
                    cmd.ExecuteNonQuery();
                }

                foreach (Aggregation agg in aggrDesign.Aggregations)
                {
                    strSQL = "Insert into " + Table + " (CubeName, MeasureGroupName, AggregationDesignName, AggregationName, Dataset) Values ('" + agg.ParentCube.Name.Replace("'", "''") + "','" + agg.ParentMeasureGroup.Name.Replace("'", "''") + "','" + agg.Parent.Name.Replace("'", "''") + "','" + agg.Name.Replace("'", "''") + "','" + ConvertAggToSting(agg) + "')";
                    cmd.CommandText = strSQL;
                    cmd.ExecuteNonQuery();
                }
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }

        }

        private string ConvertAggToSting(Aggregation agg)
        {
            string outStr = "";
            AggregationAttribute aggAttr;
            AggregationDimension aggDim;
            MeasureGroup mg1 = agg.ParentMeasureGroup;

            foreach (MeasureGroupDimension mgDim in mg1.Dimensions)
            {
                aggDim = agg.Dimensions.Find(mgDim.CubeDimensionID);
                if (aggDim == null)
                {
                    foreach (CubeAttribute cubeDimAttr in mgDim.CubeDimension.Attributes)
                        outStr = outStr + "0";
                }
                else
                {
                    foreach (CubeAttribute cubeDimAttr in mgDim.CubeDimension.Attributes)
                    {
                        aggAttr = aggDim.Attributes.Find(cubeDimAttr.AttributeID);
                        if (aggAttr == null)
                            outStr = outStr + "0";
                        else
                            outStr = outStr + "1";
                    }
                }

                outStr = outStr + ",";
            }
            return outStr.Substring(0, outStr.Length - 1);
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