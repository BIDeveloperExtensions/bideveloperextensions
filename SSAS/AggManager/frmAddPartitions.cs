/*============================================================================
  File:    frmAddPartitions.cs

  Summary: Contains the form to add, delete, and change aggregations

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
using Microsoft.AnalysisServices;


namespace AggManager
{
    public partial class AddPartitionsForm : Form
    {
        private MeasureGroup mg1;
        private string strAggDes;

        public AddPartitionsForm()
        {
            InitializeComponent();
        }
        public void Init(string strAggDesign, MeasureGroup mg)
        {
            int i = 0;
            mg1 = mg;

            foreach (Partition part in mg1.Partitions)
            {
                listBox1.Items.Add(part.Name);

                if (part.AggregationDesign != null)
                    if (part.AggregationDesign.Name == strAggDesign )
                        listBox1.SelectedIndices.Add(i);
                i++;
            }

            strAggDes = strAggDesign;
            labelAggDesign.Text = labelAggDesign.Text +" " + strAggDesign;

        }


        /// <summary>
        /// For every selected partition sets the AggregationDesignID property 
        /// to a current aggregation design.
        /// Meaning current aggregation desing applies to partitions selected.
        /// </summary>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            int i = 0;

            AggregationDesign aggD = mg1.AggregationDesigns.GetByName(strAggDes);

            foreach (Partition part in mg1.Partitions)
            {
                if (listBox1.SelectedIndices.Contains(i))
                    mg1.Partitions[i].AggregationDesignID = aggD.ID;
                else
                    if (mg1.Partitions[i].AggregationDesignID == aggD.ID)
                        mg1.Partitions[i].AggregationDesignID = null;
                i++;

            }

            foreach (int index in listBox1.SelectedIndices)
            {
                mg1.Partitions[index].AggregationDesignID = aggD.ID;
                i++;
            }
            this.Close();

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}