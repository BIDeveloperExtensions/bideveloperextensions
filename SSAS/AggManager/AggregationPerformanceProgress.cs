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
    public partial class AggregationPerformanceProgress : Form
    {
        private AggregationDesign aggD;
        private Cube cube;
        private AggregationPerformanceTester tester;
        private List<AggregationPerformanceTester.AggregationPerformance> listPerf = new List<AggregationPerformanceTester.AggregationPerformance>();
        private List<AggregationPerformanceTester.MissingAggregationPerformance> missingPerf = new List<AggregationPerformanceTester.MissingAggregationPerformance>();
        private BackgroundWorker backgroundThread = new BackgroundWorker();
        private string sErrors = "";
        private bool _started = false;

        public AggregationPerformanceProgress()
        {
            InitializeComponent();
        }

        public void Init(AggregationDesign aggD)
        {
            this.aggD = aggD;
            lblServer.Text = aggD.ParentServer.Name;
            lblDatabase.Text = aggD.ParentDatabase.Name;
            lblCube.Text = aggD.ParentCube.Name;
            lblMeasureGroup.Text = aggD.Parent.Name;
            lblAggDesign.Text = aggD.Name;
            lblStatus.Text = "";
        }

        public void Init(Cube cube)
        {
            this.cube = cube;
            lblServer.Text = cube.ParentServer.Name;
            lblDatabase.Text = cube.Parent.Name;
            lblCube.Text = cube.Name;
            lblMeasureGroup.Text = "";
            lblAggDesign.Text = "";
            lblStatus.Text = "";
        }

        public List<AggregationPerformanceTester.AggregationPerformance> Results
        {
            get { return listPerf; }
        }

        public List<AggregationPerformanceTester.MissingAggregationPerformance> MissingResults
        {
            get { return missingPerf; }
        }

        public string Errors
        {
            get { return sErrors; }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (tester != null)
                    tester.Cancel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        void backgroundThread_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (cube != null)
                {
                    foreach (MeasureGroup mg in cube.MeasureGroups)
                    {
                        if (mg.IsLinked) continue;

                        long lngMeasureGroupRowCount = 0;
                        foreach (AggregationDesign aggD in mg.AggregationDesigns)
                        {
                            bool bPartitionUsesAggD = false;
                            foreach (Partition p in mg.Partitions)
                            {
                                if (p.AggregationDesignID == aggD.ID)
                                {
                                    bPartitionUsesAggD = true;
                                    break;
                                }
                            }
                            if (!bPartitionUsesAggD) continue;

                            this.aggD = aggD;
                            tester = new AggregationPerformanceTester(aggD, chkTestAgg.Checked, chkTestNoAggs.Checked, chkWithoutIndividualAggs.Checked);
                            tester.OnProgress += new ProgressChangedEventHandler(tester_OnProgress);
                            tester.StartTest();
                            listPerf.AddRange(tester.Results);
                            missingPerf.AddRange(tester.MissingResults);

                            if (tester.Results.Length > 0)
                            {
                                lngMeasureGroupRowCount += tester.Results[0].PartitionRowCount;
                                foreach (AggregationPerformanceTester.AggregationPerformance aggP in listPerf)
                                {
                                    if (aggP.MeasureGroupName == mg.Name)
                                    {
                                        aggP.MeasureGroupRowCount = lngMeasureGroupRowCount;
                                    }
                                }
                            }

                            sErrors += tester.Errors;
                            if (tester.Cancelled) break;
                        }
                    }
                }
                else
                {
                    tester = new AggregationPerformanceTester(this.aggD, chkTestAgg.Checked, chkTestNoAggs.Checked, chkWithoutIndividualAggs.Checked);
                    tester.OnProgress += new ProgressChangedEventHandler(tester_OnProgress);
                    tester.StartTest();
                    listPerf.AddRange(tester.Results);
                    missingPerf.AddRange(tester.MissingResults);
                    sErrors = tester.Errors;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }

            try
            {
                this.Hide();
            }
            catch { }
        }

        private void tester_OnProgress(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (progressBar1.InvokeRequired)
                {
                    progressBar1.BeginInvoke(new MethodInvoker(delegate() { tester_OnProgress(sender, e); }));
                }
                else
                {
                    int iProgress = e.ProgressPercentage;
                    if (cube != null)
                    {
                        iProgress = (int)(1.0 * iProgress / cube.MeasureGroups.Count + 100 * (cube.MeasureGroups.IndexOf(aggD.Parent) + 1) / cube.MeasureGroups.Count);
                    }
                    progressBar1.Value = Math.Min(iProgress, progressBar1.Maximum);
                    lblStatus.Text = Convert.ToString(e.UserState);
                    lblMeasureGroup.Text = aggD.Parent.Name;
                    lblAggDesign.Text = aggD.Name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void AggregationPerformanceProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (tester != null)
                    tester.Cancel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public bool Started
        {
            get { return _started; }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                if (!chkTestNoAggs.Checked && !chkTestAgg.Checked && !chkWithoutIndividualAggs.Checked)
                {
                    MessageBox.Show("You must select at least one test type.");
                    return;
                }

                _started = true;

                chkTestNoAggs.Enabled = false;
                chkTestAgg.Enabled = false;
                chkWithoutIndividualAggs.Enabled = false;
                btnRun.Enabled = false;
                panel2.Enabled = true;
                buttonCancel.Enabled = true;

                backgroundThread.DoWork += new DoWorkEventHandler(backgroundThread_DoWork);
                backgroundThread.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void chkTestAgg_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkTestAgg.Checked)
            {
                chkTestAgg.Checked = true;
                MessageBox.Show("You must test query performance with each agg each time.");
            }
        }

    }
}