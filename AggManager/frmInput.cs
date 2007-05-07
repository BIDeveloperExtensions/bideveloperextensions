/*============================================================================
  File:    frmInput.cs

  Summary: Contains the form for entering name of the new aggregation design to be created

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
    public partial class InputForm : Form
    {
        private MeasureGroup mg1;

        public InputForm()
        {
            InitializeComponent();
        }
        public void Init(MeasureGroup mg)
        {
            mg1 = mg;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (mg1.AggregationDesigns.Find(textBoxNewAggDesign.Text) != null)
                {
                    MessageBox.Show("Aggregation design: " + textBoxNewAggDesign.Text + " already exists");
                    return;
                }
                mg1.AggregationDesigns.Add(textBoxNewAggDesign.Text);
                this.Close();
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

    }
}