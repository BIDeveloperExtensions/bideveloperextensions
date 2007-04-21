/*============================================================================
  File:    frmPartitionAggs.cs

  Summary: Contains the form to display information about aggregation sizes

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
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.AdomdClient;

namespace AggManager
{
    public partial class PartitionsAggsForm : Form
    {
        private MeasureGroup mg1;
        private Partition part1;
        private AggregationDesign aggDes;
        DataSet partitionDetails;

        public void Init(MeasureGroup mg, 
            string strParition)
        {
            mg1 = mg;
            part1 = mg.Partitions[strParition];
            aggDes = part1.AggregationDesign;
            this.Text = " Aggregation sizes for partition " + strParition;

            lblSize.Text = part1.EstimatedRows.ToString() + " records" ;
            lablPartName.Text = strParition;

            //--------------------------------------------------------------------------------
            // Open ADOMD connection to the server and issue DISCOVER_PARTITION_STAT request to get aggregation sizes
            //--------------------------------------------------------------------------------
            AdomdConnection adomdConnection = new AdomdConnection("Data Source=" + mg.ParentServer.Name);
            adomdConnection.Open();
            partitionDetails = adomdConnection.GetSchemaDataSet(AdomdSchemaGuid.PartitionStat, new object[] { mg1.Parent.Parent.Name, mg1.Parent.Name, mg1.Name, mg1.Partitions[0].Name });

            DataColumn colItem1 = new DataColumn("Percentage", Type.GetType("System.String"));
            partitionDetails.Tables[0].Columns.Add(colItem1);

            AddGridStyle();

            dataGrid1.DataSource = partitionDetails.Tables[0];

            double ratio = 0; 
            foreach (DataRow row in partitionDetails.Tables[0].Rows)
            {
                ratio = 100.0 * ((long)row["AGGREGATION_SIZE"] / (double)part1.EstimatedRows );
                row["Percentage"] =  ratio.ToString( "#0.00") + "%";
            }

            CurrencyManager cm = (CurrencyManager)this.BindingContext[dataGrid1.DataSource, dataGrid1.DataMember];
            ((DataView)cm.List).AllowNew = false;

        }
        public PartitionsAggsForm()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Would you like to save the aggregation design : " + aggDes.Name + "? Note, aggregations not found in the list will be deleted from the aggregation design", "Save Message", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;

            bool boolAggFound = false;

            for (int i = 0; aggDes.Aggregations.Count > i ; i++ )
            {
                boolAggFound = false;

                foreach (DataRow dRow in partitionDetails.Tables[0].Rows)
                    if (dRow["AGGREGATION_NAME"].ToString() == aggDes.Aggregations[i].Name) boolAggFound = true;

                if (!boolAggFound)
                {
                    aggDes.Aggregations.Remove(aggDes.Aggregations[i].Name);
                    i--;    
                }
            }

            MessageBox.Show("Aggregation desing :" + aggDes.Name + "  has been updated with " + aggDes.Aggregations.Count.ToString() + " aggregations ");


            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void AddGridStyle()
        {

            int iWidth0 = 300;
            Graphics Graphics = dataGrid1.CreateGraphics();

            if (partitionDetails.Tables[0].Rows.Count > 0)
            {
                int iColWidth = (int)(Graphics.MeasureString
                    (partitionDetails.Tables[0].Rows[0].ItemArray[0].ToString(),
                    dataGrid1.Font).Width);
                iWidth0 = (int)System.Math.Max(iWidth0, iColWidth);
            }

            DataGridTableStyle myGridStyle = new DataGridTableStyle();
            myGridStyle.MappingName = "rowsettable";

            DataGridTextBoxColumn nameColumnStyle = new DataGridTextBoxColumn();
            nameColumnStyle.MappingName = "AGGREGATION_NAME";
            nameColumnStyle.HeaderText = "Aggregation Name";
            nameColumnStyle.Width = iWidth0 + 10;
            myGridStyle.GridColumnStyles.Add(nameColumnStyle);

            DataGridTextBoxColumn nameColumnStyle1 = new DataGridTextBoxColumn();
            nameColumnStyle1.MappingName = "AGGREGATION_SIZE";
            nameColumnStyle1.HeaderText = "Records";
            nameColumnStyle1.Width = 70;
            myGridStyle.GridColumnStyles.Add(nameColumnStyle1);

            DataGridTextBoxColumn nameColumnStyle2 = new DataGridTextBoxColumn();
            nameColumnStyle2.MappingName = "Percentage";
            nameColumnStyle2.HeaderText = "Percentage";
            nameColumnStyle2.Width = 100;
            myGridStyle.GridColumnStyles.Add(nameColumnStyle2);

            dataGrid1.TableStyles.Add(myGridStyle);

        }
    }
}