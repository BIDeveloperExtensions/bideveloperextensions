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
/*
 * This file has been incorporated into BIDSHelper. 
 *    http://www.codeplex.com/BIDSHelper
 * and may have been altered from the orginal version which was released 
 * as a Microsoft sample.
 * 
 * The official version can be found on the sample website here: 
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
           string[] inDimNames,
           string[] inDimIDs,
           bool inAddNew,
           String inAggDesName)
        {
            bool IsOnlineMode = false;
            Cube selectedCube = projItem.Object as Cube;
            string sTargetDatabase = "";
            if ((selectedCube != null) && (selectedCube.ParentServer != null))
            {
                // if we are in Online mode there will be a parent server
                server = selectedCube.ParentServer;
                sTargetDatabase = selectedCube.Parent.Name;
                IsOnlineMode = true;
            }
            else
            {
                // if we are in Project mode we will use the server name from 
                // the deployment settings
                DeploymentSettings deploySet = new DeploymentSettings(projItem);
                server = new Server();
                server.Connect(deploySet.TargetServer);
                sTargetDatabase = deploySet.TargetDatabase;
            }

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
                                   sTargetDatabase + "." +
                                   mg1.Parent.ID + "." +
                                   mg1.ID + "'" +
                                   " and duration >= 100";


            textBoxSQLConnectionString.Text = server.ServerProperties.Find("Log\\QueryLog\\QueryLogConnectionString").Value.ToString(); ;
            textBoxSQLConnectionString.Text = MakeConnectionStringRemote(server.Name, textBoxSQLConnectionString.Text);

            if (!addNew)
            {
                textBoxNewAggDesign.Text = inAggDesName;
                textBoxNewAggDesign.Enabled = false;
            }
            txtServerNote.Text = string.Format("Note: the QueryLog details have been taken from the '{0}' server,\n" +
             "which is the one currently configured as the deployment target.", server.Name);
            txtServerNote.Visible = !IsOnlineMode; // hide the note if we are in online mode.
        }

        /// <summary>
        /// If the query log connection string refers to the server using localhost, (local), or "." then it should be changed so that AggManager can connect from another server.
        /// </summary>
        /// <param name="ServerName"></param>
        /// <param name="ConnectionString"></param>
        /// <returns></returns>
        private static string MakeConnectionStringRemote(string ServerName, string ConnectionString)
        {
            string NewConnectionString = "";
            foreach (string part in ConnectionString.Split(new char[] { ';' }))
            {
                if (NewConnectionString.Length > 0) NewConnectionString += ";";
                if (part.ToLower() == "data source=(local)"
                    || part.ToLower() == "data source=."
                    || part.ToLower() == "data source=localhost")
                {
                    NewConnectionString += "Data Source=" + ServerName;
                }
                else
                {
                    NewConnectionString += part;
                }
            }
            return NewConnectionString;
        }

        private void buttonConnectToSQL_Click(object sender, EventArgs e)
        {
            string strConnection;
            try
            {
                this.Cursor = Cursors.WaitCursor;

                strConnection = textBoxSQLConnectionString.Text;
                if (strConnection == "")
                {
                    Exception ex = new ApplicationException("Query Log connection string is empty");
                    throw ex;
                }

                sqlConnection1 = new SqlConnection(strConnection.Substring(strConnection.IndexOf(";")));
                sqlConnection1.Open();
                sqlDataAdapter1 = new SqlDataAdapter(textBoxSQLQuery.Text, sqlConnection1);

                DataSet ds = new DataSet();
                sqlDataAdapter1.Fill(ds, textBoxSQLQuery.Text);
                DataView source = ds.Tables[textBoxSQLQuery.Text].DefaultView;

                dataGrid1.DataSource = source;

                DataGridTableStyle myGridStyle = new DataGridTableStyle();
                myGridStyle.MappingName = source.Table.TableName;

                DataGridTextBoxColumn datasetColumnStyle = new DataGridTextBoxColumn();
                datasetColumnStyle.MappingName = "dataset";
                datasetColumnStyle.HeaderText = "dataset";
                datasetColumnStyle.Width = dataGrid1.Width - 45;
                myGridStyle.GridColumnStyles.Add(datasetColumnStyle);

                dataGrid1.TableStyles.Clear();
                dataGrid1.TableStyles.Add(myGridStyle);

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
            try
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
                    aggDes = mg1.AggregationDesigns.GetByName(aggDesName);

                int i = 0;
                foreach (DataRow dRow in myDataView.Table.Rows)
                {
                    //Skip over deleted rows
                    if (dRow.RowState.ToString() != "Deleted")
                        AddAggregationToAggDesign(
                            aggDes,
                            dRow.ItemArray[intOrdinal].ToString(),
                            i++,
                            textBoxAggregationPrefix.Text);

                }

                //MessageBox.Show("Aggregation Design '" + aggDes.Name + "' updated with " + aggDes.Aggregations.Count.ToString() + " aggregations.");
                this.Close();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message)) MessageBox.Show("Error saving: " + ex.Message);
            }
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
            int originalAggNum = aggNum;
            try
            {
                Aggregation agg;
                string aggName = strAggPrefix + aggNum.ToString();

                while (aggDesign.Aggregations.Find(aggName) != null)
                {
                    aggName = strAggPrefix + (++aggNum).ToString();
                }


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

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving aggregation #" + (originalAggNum + 1) + ": " + ex.Message);
                throw new Exception(""); //blank exception means not to report again
            }
        }

        private void checkBoxConnction_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnction.Checked == false)
                textBoxSQLConnectionString.Enabled = true;
            else
                textBoxSQLConnectionString.Enabled = false;
        }

    }

}