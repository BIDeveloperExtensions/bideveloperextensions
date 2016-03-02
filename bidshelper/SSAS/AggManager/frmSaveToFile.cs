/*============================================================================
  File:    frmSaveToFile.cs

  Summary: Contains the form to specify a name of file for saving Measure group with new/changed 
  aggregation design

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
using System.Xml;

namespace AggManager 
{
    public partial class SaveToFileForm : Form
    {
        private MeasureGroup mg1;

        public SaveToFileForm()
        {
            InitializeComponent();
        }

        public void Init(MeasureGroup mg)
        {
            mg1 = mg;
            textBoxFile.Text = mg.Name + textBoxFile.Text;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

            XmlTextWriter xmlwrite = new XmlTextWriter(textBoxFile.Text, System.Text.Encoding.UTF8);
            xmlwrite.Formatting = Formatting.Indented;
            xmlwrite.Indentation = 2;

            Scripter.WriteAlter(xmlwrite, mg1, true, true);
            xmlwrite.Close();
            MessageBox.Show( "MeasureGroup definition scripted to the :" + textBoxFile.Text + " \n");
            
            this.Close();
        }


        private void buttonCancel_Click_1(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}