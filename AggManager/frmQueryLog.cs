/*============================================================================
  File:    frmQueryLog.cs

  Summary: Contains the form to add aggregations based on informaion in the Query Log

           Part of Aggregation Manager 

  Date:    January 2007
------------------------------------------------------------------------------
  This file is part of the Microsoft SQL Server Code Samples.

  Copyright (C) Microsoft Corporation.  All rights reserved.

  This source code is intended only as a supplement to Microsoft
  Development Tools and/or on-line documentation.  See these other
  materials for detailed information regarding Microsoft code samples.

  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.
============================================================================*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AnalysisServices;
using System.Data.SqlClient;

namespace AggManager
{
    public partial class QueryLogForm : Form
    {
        Server server;
        string strSQLQuery;
        MeasureGroup mg1;
        string[,] dimAttributes;
        string[] dimNames;
        string[] dimIDs;
        bool addNew;
        string aggDesName;

        public QueryLogForm()
        {
            InitializeComponent();
        }

         public void Init(
            EnvDTE.ProjectItem projItem,
            MeasureGroup mg, 
            string[,] inDimAttributes,  
            string[] inDimNames ,
            string[] inDimIDs,
            bool inAddNew,
            String inAggDesName )

        {
            DeploymentSettings deploySet = new DeploymentSettings(projItem);

            server = new Server();
            server.Connect(deploySet.TargetServer);
            mg1 = mg;
            dimAttributes = inDimAttributes;
            dimNames = inDimNames;
            dimIDs = inDimIDs;
            //Variable defining whether form is invoked to create new aggregation design or add aggregations to exising one.
            addNew = inAddNew;
            aggDesName = inAggDesName;

            strSQLQuery = textBoxSQLQuery.Text;
            textBoxSQLQuery.Text = textBoxSQLQuery.Text +
                                   " Where MSOLAP_Database = '" +
                                   mg1.ParentDatabase.Name +
                                   "' and  MSOLAP_ObjectPath = '" +
                                   server.Name + "." +
                                   mg1.ParentDatabase.ID + "." +
                                   mg1.Parent.ID + "." +
                                   mg1.ID + "'";


            textBoxSQLConnectionString.Text = server.ServerProperties.Find("Log\\QueryLog\\QueryLogConnectionString").Value.ToString(); ;

            if (!addNew)
            {
                textBoxNewAggDesign.Text = inAggDesName;
                textBoxNewAggDesign.Enabled = false;
            }

        }

        private void buttonConnectToSQL_Click(object sender, EventArgs e)
        {
            string strConncection;
            try
                {
                this.Cursor = Cursors.WaitCursor;

                if (checkBoxConnction.Checked)
                {
                    strConncection = server.ServerProperties.Find("Log\\QueryLog\\QueryLogConnectionString").Value.ToString(); ;
                    if (strConncection == "")
                    {
                        Exception ex = new ApplicationException("Query Log connection string is empty");
                        throw ex;
                    }
                }
                else
                {
                    strConncection = textBoxSQLConnectionString.Text;
                }

                sqlConnection1 = new SqlConnection(strConncection.Substring(strConncection.IndexOf(";")));
                sqlConnection1.Open();
                sqlDataAdapter1 = new SqlDataAdapter(textBoxSQLQuery.Text, sqlConnection1);

                DataSet ds = new DataSet();
                sqlDataAdapter1.Fill(ds, textBoxSQLQuery.Text);
                DataView source = ds.Tables[textBoxSQLQuery.Text].DefaultView;

                dataGrid1.DataSource = source;
                this.Cursor = Cursors.Default;

            }
            catch (Exception ex)
            {
                //Show exception if cannot connect to SQL Server
                this.Cursor = Cursors.Default;
                sqlDataAdapter1.Dispose();
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DataView myDataView;
            myDataView = (DataView)dataGrid1.DataSource;

            int intOrdinal;
            intOrdinal = myDataView.Table.Columns["Dataset"].Ordinal;

            AggregationDesign aggDes;
            if (addNew)
            {
                if (mg1.AggregationDesigns.Find(textBoxNewAggDesign.Text) != null)
                {
                    MessageBox.Show("Aggregation design: " + textBoxNewAggDesign.Text + " already exists");
                    return;
                }
                aggDes = mg1.AggregationDesigns.Add(textBoxNewAggDesign.Text);

            }
            else
                aggDes = mg1.AggregationDesigns[aggDesName];

            int i = 0;
            foreach (DataRow dRow in myDataView.Table.Rows)
            {
                //Skip over deleted rows
                if (dRow.RowState.ToString() != "Deleted")
                    AddAggregationToAggDesign(
                        aggDes,
                        dRow.ItemArray[intOrdinal].ToString(),
                        i++,
                        textBoxNewAggDesign.Text);

            }

            MessageBox.Show("Aggregation Design '" + aggDes.Name + "' updated with " + aggDes.Aggregations.Count.ToString() + " aggregations.");
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            textBoxSQLConnectionString.Text = strSQLQuery;
            this.Close();
        }


        /// <summary>
        /// Helper function receives a string in format like 0001000,001001 
        /// translates string into aggregation and adds it to aggegation design 
        /// </summary>
        Boolean AddAggregationToAggDesign(AggregationDesign aggDesign, string instr, int aggNum, string strAggPrefix )
        {

            Aggregation agg;
            string aggName = strAggPrefix + aggNum.ToString();
            agg = aggDesign.Aggregations.Find(aggName);

            if (agg != null)
                aggName = strAggPrefix + aggDesign.Aggregations.Count.ToString();


            agg = aggDesign.Aggregations.Add(aggName, aggName);

            string a1;
            int dimNum = 0;
            int attrNum = 0;
            bool newDim = true;

            for (int i = 0; i < instr.Length; i++)
            {
                a1 = instr[i].ToString();
                switch (a1)
                {
                    case ",":
                        dimNum++;
                        attrNum = -1;
                        newDim = true;
                        break;
                    case "0":
                        break;
                    case "1":

                        if (newDim)
                        {
                            agg.Dimensions.Add(dimIDs[dimNum]);
                            newDim = false;
                        }
                        agg.Dimensions[dimIDs[dimNum]].Attributes.Add(dimAttributes[dimNum, attrNum]);

                        break;
                    default:
                        break;
                }
                attrNum++;
            }

            if (agg.Dimensions.Count == 0)
            {
                aggDesign.Aggregations.Remove(agg);
                return false;
            }
            return true;

        }

        private void checkBoxConnction_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnction.Checked == false)
                textBoxSQLConnectionString.Enabled = true;
            else
                textBoxSQLConnectionString.Enabled = false;
        }

        private void QueryLogForm_Load(object sender, EventArgs e)
        {

        }

    }

}