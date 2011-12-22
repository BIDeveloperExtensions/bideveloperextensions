/*============================================================================
  File:    frmMain.cs

  Summary: Contains main form of sample application

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
using System.Globalization;
using System.Reflection;

using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AnalysisServices;

namespace AggManager
{
    public partial class MainForm : Form
    {

        //Server server;
        private static string TagForLoadingNodes = "LOADING";
        //private static string TagDatabases = "Databases";
        //private static string TagCubes = "Cubes";
        private static string TagMeasureGroups = "Measure Groups";
        private static string TagAggdesigns = "Aggregation Designs";
        private static string TagNoAggdesign = "No Aggregation Design";

        public const int ImgListMetadataConnectedIndex = 0;
        public const int ImgListMetadataDisconnectedIndex = 1;
        public const int ImgListMetadataFolderIndex = 2;
        public const int ImgListMetadataDatabaseIndex = 3;
        public const int ImgListMetadataCubeIndex = 4;
        public const int ImgListMetadataMeasureGroupIndex = 5;
        public const int ImgListMetadataPartitionIndex = 6;
        public const int ImgListMetadataAggDesIndex = 7;

        public const string MODIFIED_SUFFIX = "-****-modified";

        private bool mIsDirty = false;
        private EnvDTE.ProjectItem mProjItem;

        string[,] dimAttributes;
        string[] dimNames;
        string[] dimIDs;
        MeasureGroup mgCurrent;
        private Cube realCube;
        private Database cloneDB;

        public MainForm()
        {
            InitializeComponent();

        }

        public MainForm(Cube selectedCube, EnvDTE.ProjectItem projectItm)
        {
            this.realCube = selectedCube;

            //easiest way to allow changes in this window to be made so they can still be rolled back by the cancel button so they don't show up in BIDS
            this.cloneDB = selectedCube.Parent.Clone();

            InitializeComponent();
            mProjItem = projectItm;
            TreeNode nd;
            nd = CreateStaticNode(treeView1.Nodes, selectedCube.Name, cloneDB.Cubes[selectedCube.ID], ImgListMetadataCubeIndex);
            nd.Expand();
            nd = CreateNode(nd.Nodes, TagMeasureGroups, TagMeasureGroups, ImgListMetadataFolderIndex);
            nd.Expand();
        }

        public bool IsDirty
        {
             get { return mIsDirty;}
             private set {mIsDirty = value;}

        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    Application.Run(new MainForm());
        //}

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private static TreeNode CreateNode(TreeNodeCollection nodes, string text, object obj, int imageIndex)
        {
            TreeNode node = nodes.Add(text, text);
            node.ImageIndex = imageIndex;
            node.SelectedImageIndex = imageIndex;
            node.Tag = obj;
            node.Nodes.Add("... loading ...").Tag = TagForLoadingNodes;
            return node;
        }
        private static TreeNode CreateStaticNode(TreeNodeCollection nodes, string text, object obj, int imageIndex)
        {
            TreeNode node = nodes.Add(text, text );
            node.ImageIndex = imageIndex;
            node.SelectedImageIndex = imageIndex;
            node.Tag = obj;
            return node;
        }

   
        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            //--------------------------------------------------------------------------------
            // Some validations.
            //--------------------------------------------------------------------------------
            if (null == e)
            {
                return;
            }

            TreeNode node = e.Node;
            if (null == node)
            {
                return;
            }

            //--------------------------------------------------------------------------------
            // If the TreeNode being expanded has the information already loaded, we'll do nothing.
            //
            // REMINDER: we are using a special child TreeNode with Tag = "LOADING" to mark
            //           TreeNodes that don't have their children loaded.
            //--------------------------------------------------------------------------------
            TreeNodeCollection nodes = node.Nodes;
            if ((nodes.Count != 1)
                || (nodes[0].Tag != (object)TagForLoadingNodes))
            {
                return;
            }
            nodes.Clear();
            TreeNode nd;
            switch ((string)node.Tag)
            {
                //case "Databases":
                //    this.Cursor = Cursors.WaitCursor;
                //    foreach (Database db in server.Databases)
                //    {
                //        nd = CreateStaticNode(node.Nodes, db.Name, db, ImgListMetadataDatabaseIndex);
                //        CreateNode(nd.Nodes, TagCubes, TagCubes,ImgListMetadataFolderIndex );
                //    }
                //    this.Cursor = Cursors.Default;
                //    break;
                //case "Cubes":
                //    Database db1 = (Database)node.Parent.Tag;
                //    this.Cursor = Cursors.WaitCursor;

                //    foreach (Cube cb in db1.Cubes)
                //    {
                //        nd = CreateStaticNode(node.Nodes, cb.Name, cb, ImgListMetadataCubeIndex);
                //        CreateNode(nd.Nodes, TagMeasureGroups, TagMeasureGroups, ImgListMetadataFolderIndex);
                //    }
                //    this.Cursor = Cursors.Default;
                //    break;
                case "Measure Groups":
                    Cube cb1 = (Cube)node.Parent.Tag;

                    this.Cursor = Cursors.WaitCursor;
                    foreach (MeasureGroup mg in cb1.MeasureGroups)
                    {
                        nd = CreateStaticNode(node.Nodes, mg.Name, mg, ImgListMetadataMeasureGroupIndex);
                        if (mg.IsLinked)
                            nd.ForeColor = System.Drawing.Color.Gray;
                        else
                            CreateNode(nd.Nodes, TagAggdesigns, TagAggdesigns, ImgListMetadataFolderIndex);
                    }
                    this.Cursor = Cursors.Default;
                    break;
                case "Aggregation Designs":
                    MeasureGroup mg1 = (MeasureGroup)node.Parent.Tag;
                    TreeNode ndNoAgg = CreateStaticNode(node.Nodes, TagNoAggdesign, TagNoAggdesign, ImgListMetadataFolderIndex);

                    this.Cursor = Cursors.WaitCursor;

                    foreach (AggregationDesign aggdes in mg1.AggregationDesigns)
                    {
                        CreateStaticNode(node.Nodes, aggdes.Name, aggdes.Name, ImgListMetadataAggDesIndex);
                    }

                    foreach (Partition pt in mg1.Partitions)
                    {
                        if (pt.AggregationDesign == null)
                            CreateStaticNode(ndNoAgg.Nodes, pt.Name, pt, ImgListMetadataPartitionIndex);
                        else
                            CreateStaticNode(node.Nodes[node.Nodes.IndexOfKey(pt.AggregationDesign.Name)].Nodes, pt.Name, pt.Name,  ImgListMetadataPartitionIndex);
                    }
                    this.Cursor = Cursors.Default;
                    break;

                default:
                    break;
            }


        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listBoxReport.Items.Clear();

            TreeNode nd = e.Node;
            if (nd.Parent == null) return;

            try
            {

                switch ((string)nd.Parent.Tag)
                {
                    case "Databases":
                        break;
                    case "Cubes":
                        break;
                    case "Measure Groups":
                        break;
                    case "Aggregation Designs":
                        UpdateAggCountInListBox(nd);
                       break;

                    default:
                        break;
                }
            }
            catch
            {
            }

        }

        private void UpdateAggCountInListBox(TreeNode nd)
        {
            listBoxReport.Items.Clear();
            if (nd.Parent != null && nd.Parent.Parent != null && nd.Parent.Parent.Tag != null && nd.Parent.Parent.Tag is MeasureGroup)
            {
                listBoxReport.Items.Add("Aggregation count");
                AggregationDesign aggdes = ((MeasureGroup)nd.Parent.Parent.Tag).AggregationDesigns.GetByName((string)nd.Tag);
                listBoxReport.Items.Add(aggdes.Aggregations.Count);
            }
        }

        private void cmdAddAggregationDesignToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //Add new Aggregation Design
                AggManager.InputForm form1 = new AggManager.InputForm();

                if (treeView1.SelectedNode.Text == TagNoAggdesign)
                    form1.Init((MeasureGroup)treeView1.SelectedNode.Parent.Parent.Tag);
                else
                    form1.Init((MeasureGroup)treeView1.SelectedNode.Parent.Tag);

                form1.ShowDialog(this);


                //Refresh nodes tree
                if (treeView1.SelectedNode.Text == TagNoAggdesign)
                    treeView1.SelectedNode = treeView1.SelectedNode.Parent.Parent;
                else
                    treeView1.SelectedNode = treeView1.SelectedNode.Parent;

                treeView1.SelectedNode.Nodes.RemoveAt(0);
                CreateNode(treeView1.SelectedNode.Nodes, TagAggdesigns, TagAggdesigns, ImgListMetadataFolderIndex);
                treeView1.SelectedNode.Nodes[0].Expand();

                if (!treeView1.SelectedNode.Text.EndsWith(MODIFIED_SUFFIX))
                    treeView1.SelectedNode.Text = treeView1.SelectedNode.Text + MODIFIED_SUFFIX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }


        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                //force node to be selected
                treeView1.SelectedNode = e.Node;


                Point pt = treeView1.PointToClient(Control.MousePosition);
                try
                {
                    //if (e.Node.Text.EndsWith(MODIFIED_SUFFIX))
                    //    contextMenuStripSave.Show(treeView1, pt);

                    if (e.Node.Tag is Cube)
                        contextMenuStripCube.Show(treeView1, pt); 
                    else if ((string)e.Node.Tag == "Aggregation Designs" || (string)e.Node.Tag == TagNoAggdesign)
                        contextMenuStripMG.Show(treeView1, pt);
                    else if ((string)e.Node.Parent.Tag == "Aggregation Designs")
                        contextMenuStripAggDes.Show(treeView1, pt);

                    if (((string)e.Node.Parent.Parent.Tag == "Aggregation Designs") &&
                        ((string)e.Node.Parent.Tag != TagNoAggdesign))
                        contextMenuStripPartitionAggs.Show(treeView1, pt);

                }
                catch { }
            }
        }

        private void cmdEditAggDes_Click(object sender, EventArgs e)
        {
            try
            {
                AggManager.EditAggs form1 = new AggManager.EditAggs();
                TreeNode node;
                node = treeView1.SelectedNode;

                FillCollections((MeasureGroup)node.Parent.Parent.Tag);

                form1.Init(node.Name, (MeasureGroup)node.Parent.Parent.Tag, dimAttributes, dimNames, dimIDs);
                if (form1.ShowDialog(this) != DialogResult.OK) return;

                UpdateAggCountInListBox(treeView1.SelectedNode);
                if (!treeView1.SelectedNode.Parent.Parent.Text.EndsWith(MODIFIED_SUFFIX))
                    treeView1.SelectedNode.Parent.Parent.Text = treeView1.SelectedNode.Parent.Parent.Text + MODIFIED_SUFFIX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void FillCollections( MeasureGroup mg1)
        {

            if (mg1 == mgCurrent) return;

            mgCurrent = mg1;

            dimAttributes = new string[100, 300];
            dimNames = new string[100];
            dimIDs = new string[100];

            int dimI = 0;
            int attI = 0;

            foreach (MeasureGroupDimension mgDim in mg1.Dimensions)
            {
                dimNames[dimI] = mgDim.CubeDimension.Name;
                dimIDs[dimI] = mgDim.CubeDimensionID;
                attI = 0;

                foreach (CubeAttribute cubeDimAttr in mgDim.CubeDimension.Attributes)
                {
                        dimAttributes[dimI, attI] = cubeDimAttr.AttributeID;
                        attI++;
                }
                dimI++;
            }
        }

        private void cmdAddFromQueryLog_Click(object sender, EventArgs e)
        {
            try
            {
                AggManager.QueryLogForm form1 = new AggManager.QueryLogForm();
                TreeNode node;


                if (treeView1.SelectedNode.Text == TagNoAggdesign)
                    node = treeView1.SelectedNode.Parent;
                else
                    node = treeView1.SelectedNode;

                FillCollections((MeasureGroup)node.Parent.Tag);
                form1.Init(mProjItem, (MeasureGroup)node.Parent.Tag, dimAttributes, dimNames, dimIDs, true, null);

                form1.ShowDialog(this);


                //Refresh the tree 
                treeView1.SelectedNode = node.Parent;
                treeView1.SelectedNode.Nodes.RemoveAt(0);
                CreateNode(treeView1.SelectedNode.Nodes, TagAggdesigns, TagAggdesigns, ImgListMetadataFolderIndex);

                treeView1.SelectedNode.Nodes[0].Expand();
                if (!treeView1.SelectedNode.Text.EndsWith(MODIFIED_SUFFIX))
                    treeView1.SelectedNode.Text = treeView1.SelectedNode.Text + MODIFIED_SUFFIX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void cmdAddAggregationsFromQueryLogToExisting_Click(object sender, EventArgs e)
        {
            try
            {
                AggManager.QueryLogForm form1 = new AggManager.QueryLogForm();


                FillCollections((MeasureGroup)treeView1.SelectedNode.Parent.Parent.Tag);
                form1.Init(mProjItem, (MeasureGroup)treeView1.SelectedNode.Parent.Parent.Tag, dimAttributes, dimNames, dimIDs, false, treeView1.SelectedNode.Text);

                form1.ShowDialog(this);

                UpdateAggCountInListBox(treeView1.SelectedNode);
                if (!treeView1.SelectedNode.Parent.Parent.Text.EndsWith(MODIFIED_SUFFIX))
                    treeView1.SelectedNode.Parent.Parent.Text = treeView1.SelectedNode.Parent.Parent.Text + MODIFIED_SUFFIX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void cmdDeleteAggDes_Click(object sender, EventArgs e)
        {
            try
            {
                TreeNode nd = treeView1.SelectedNode;

                if (MessageBox.Show("Would you like to delete Aggregation design:" + nd.Text + "?", "Delete Aggregation design", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;

                //Delete aggregation design from the measure group
                MeasureGroup mg = (MeasureGroup)nd.Parent.Parent.Tag;
                AggregationDesign aggD = mg.AggregationDesigns.GetByName(nd.Text);

                foreach (Partition pt in mg.Partitions)
                {

                    if (pt.AggregationDesignID == aggD.ID)
                        pt.AggregationDesignID = null;
                }

                mg.AggregationDesigns.Remove(aggD.ID);

                //Remove agg design from the tree
                treeView1.SelectedNode = treeView1.SelectedNode.Parent.Parent;
                treeView1.SelectedNode.Nodes.RemoveAt(0);
                CreateNode(treeView1.SelectedNode.Nodes, TagAggdesigns, TagAggdesigns, ImgListMetadataFolderIndex);

                treeView1.SelectedNode.Nodes[0].Expand();
                if (!treeView1.SelectedNode.Text.EndsWith(MODIFIED_SUFFIX))
                    treeView1.SelectedNode.Text = treeView1.SelectedNode.Text + MODIFIED_SUFFIX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }


        private void addPartitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AggManager.AddPartitionsForm form1 = new AggManager.AddPartitionsForm();
                TreeNode node;

                node = treeView1.SelectedNode;

                form1.Init(node.Name, (MeasureGroup)node.Parent.Parent.Tag);
                form1.ShowDialog(this);

                //Refresh the tree 
                treeView1.SelectedNode = treeView1.SelectedNode.Parent.Parent;
                treeView1.SelectedNode.Nodes.RemoveAt(0);
                CreateNode(treeView1.SelectedNode.Nodes, TagAggdesigns, TagAggdesigns, ImgListMetadataFolderIndex);

                treeView1.SelectedNode.Nodes[0].Expand();

                if (!treeView1.SelectedNode.Text.EndsWith(MODIFIED_SUFFIX))
                    treeView1.SelectedNode.Text = treeView1.SelectedNode.Text + MODIFIED_SUFFIX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /*
        private void toolStripMenuItemSave_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to save measure group: " + ((MeasureGroup)treeView1.SelectedNode.Tag).Name+ "?", "Save Message", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;
   
            this.Cursor = Cursors.WaitCursor;
            ((MeasureGroup)treeView1.SelectedNode.Tag).Update(UpdateOptions.ExpandFull);
            this.Cursor = Cursors.Default;

            MessageBox.Show( "Measure Group " + ((MeasureGroup)treeView1.SelectedNode.Tag).Name+ " has been saved to the server" );
            treeView1.SelectedNode.Text = treeView1.SelectedNode.Text.Remove(treeView1.SelectedNode.Text.IndexOf("-****-modified") - 1);
        }
        */

        /*
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            AggManager.SaveToFileForm form1 = new AggManager.SaveToFileForm();

            form1.Init((MeasureGroup)treeView1.SelectedNode.Tag);

            form1.ShowDialog(this);
            treeView1.SelectedNode.Text = treeView1.SelectedNode.Text.Remove(treeView1.SelectedNode.Text.IndexOf(MODIFIED_SUFFIX) - 1);

        }
        */

        private void aggregationSizesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AggManager.PartitionsAggsForm form1 = new AggManager.PartitionsAggsForm();

                form1.Init((MeasureGroup)treeView1.SelectedNode.Parent.Parent.Parent.Tag, treeView1.SelectedNode.Text, mProjItem);

                form1.ShowDialog(this);

                if (!treeView1.SelectedNode.Parent.Parent.Parent.Text.EndsWith(MODIFIED_SUFFIX))
                    treeView1.SelectedNode.Parent.Parent.Parent.Text = treeView1.SelectedNode.Parent.Parent.Parent.Text + MODIFIED_SUFFIX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                saveModifiedMeasureGroups();
                this.Close();
                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.IsDirty)
            {
                if (MessageBox.Show("Are you sure you want to cancel without saving changes?", "Save Changes", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Close();
                    this.Dispose();
                }
            }
            else
            {
                this.Close();
                this.Dispose();
            }

        }

        private void saveModifiedMeasureGroups()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                foreach (TreeNode nd in treeView1.Nodes[0].Nodes[0].Nodes)
                {
                    if ((nd.Tag is MeasureGroup) && (nd.Text.EndsWith(MODIFIED_SUFFIX)))
                    {
                        MeasureGroup cloneMG = ((MeasureGroup)nd.Tag);
                        MeasureGroup realMG = realCube.MeasureGroups[cloneMG.ID];

                        //1. Update existing agg designs and add new ones
                        foreach (AggregationDesign aggDesign in cloneMG.AggregationDesigns)
                        {
                            AggregationDesign realAgg = realMG.AggregationDesigns.Find(aggDesign.ID);
                            if (realAgg == null)
                                realAgg = realMG.AggregationDesigns.Add(aggDesign.Name, aggDesign.ID);
                            aggDesign.CopyTo(realAgg);
                        }

                        //2. fix AggregationDesignID on partitions
                        foreach (Partition part in cloneMG.Partitions)
                        {
                            realMG.Partitions[part.ID].AggregationDesignID = part.AggregationDesignID;
                        }

                        //3. remove deleted agg designs... do this last so no partition will be invalid
                        for (int i = 0; i < realMG.AggregationDesigns.Count; i++)
                        {
                            AggregationDesign aggDesign = realMG.AggregationDesigns[i];
                            if (cloneMG.AggregationDesigns.Find(aggDesign.ID) == null)
                            {
                                realMG.AggregationDesigns.RemoveAt(i);
                                i--;
                            }
                        }
                        //no need to run an Update statement from within BIDS
                    }
                }
                this.IsDirty = false;
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during save: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void listBoxReport_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                cloneDB.Dispose();
                cloneDB = null;
            }
            catch { }
        }

        //clicked on this menu option from the Aggregation Designs node under a measure group
        private void deleteUnusedAggregationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MeasureGroup mg = (MeasureGroup)treeView1.SelectedNode.Parent.Tag;
                PopupDeleteUnusedAggsForm(mg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        //clicked on this menu option from the Cube node
        private void deleteUnusedAggregationsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                PopupDeleteUnusedAggsForm(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void PopupDeleteUnusedAggsForm(MeasureGroup mg)
        {
            MeasureGroup cloneMG = null;
            if (mg != null)
                cloneMG = this.cloneDB.Cubes[mg.Parent.ID].MeasureGroups[mg.ID];
            AggManager.DeleteUnusedAggs form1 = new AggManager.DeleteUnusedAggs();
            form1.Init(mProjItem, cloneDB, cloneMG);
            if (form1.ShowDialog(this) == DialogResult.OK)
            {
                foreach (TreeNode node in form1.treeViewAggregation.Nodes)
                    DeleteAggsAndRecurse(node);
            }
        }

        private void DeleteAggsAndRecurse(TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
            {
                if (child.Checked && child.Tag is Aggregation)
                {
                    Aggregation agg = child.Tag as Aggregation;
                    foreach (TreeNode childNode in treeView1.Nodes)
                        MarkMeasureGroupAsModified(childNode, agg.Parent.Parent);
                    agg.Parent.Aggregations.Remove(agg);
                }
                DeleteAggsAndRecurse(child);
            }
        }

        private void MarkMeasureGroupAsModified(TreeNode node, MeasureGroup mg)
        {
            if (node.Tag is MeasureGroup && ((MeasureGroup)node.Tag).ID == mg.ID)
            {
                if (!node.Text.EndsWith(MODIFIED_SUFFIX))
                    node.Text = node.Text + MODIFIED_SUFFIX;
                return;
            }
            foreach (TreeNode child in node.Nodes)
            {
                MarkMeasureGroupAsModified(child, mg);
            }
        }

        private void validateAggregationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateAggs.Validate(cloneDB.Cubes[realCube.ID]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void printerFriendlyAggregationsReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PrinterFriendlyAggs.ShowAggsReport(cloneDB.Cubes[realCube.ID]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        
        private void searchSimilarAggregationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to consider estimated member counts?\r\n\r\nClicking Yes will exclude similar aggregations with vastly different cardinalities.", "Search Similar Aggregations - Consider Estimated Member Counts?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                { SearchSimilarAggs.ShowAggsSimilaritiesReport((cloneDB.Cubes[realCube.ID]), true); }
                else
                { SearchSimilarAggs.ShowAggsSimilaritiesReport((cloneDB.Cubes[realCube.ID]), false); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void searchSimilarAggregationsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to consider estimated member counts?\r\n\r\nClicking Yes will exclude similar aggregations with vastly different cardinalities.", "Search Similar Aggregations - Consider Estimated Member Counts?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                { SearchSimilarAggs.ShowAggsSimilaritiesReport(((MeasureGroup)treeView1.SelectedNode.Parent.Parent.Tag), treeView1.SelectedNode.Tag.ToString(), true); }
                else
                { SearchSimilarAggs.ShowAggsSimilaritiesReport(((MeasureGroup)treeView1.SelectedNode.Parent.Parent.Tag), treeView1.SelectedNode.Tag.ToString(), false); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        //clicked on this menu option from the Aggregation Designs node under a measure group
        private void exportToASQLTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AggregationDesign aggr = ((MeasureGroup)treeView1.SelectedNode.Parent.Parent.Tag).AggregationDesigns.GetByName(treeView1.SelectedNode.Tag.ToString());
                PopupExportTableForm(aggr);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }


        private void PopupExportTableForm(AggregationDesign aggrD)
        {
            //MeasureGroup cloneMG = null;
            //if (mg != null)
            //    cloneMG = this.cloneDB.Cubes[mg.Parent.ID].MeasureGroups[mg.ID];
            AggManager.ExportTable form1 = new AggManager.ExportTable();
            form1.Init(aggrD);

            DialogResult res = form1.ShowDialog(this);
            if (res != DialogResult.OK) return;

        }

        private void testAggregationPerformanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Server s = new Server();
            try
            {
                string serverName = "";
                string databaseName = "";
                if ((realCube != null) && (realCube.ParentServer != null))
                {
                    // if we are in Online mode there will be a parent server
                    serverName = realCube.ParentServer.Name;
                    databaseName = realCube.Parent.Name;
                }
                else
                {
                    // if we are in Project mode we will use the server name from 
                    // the deployment settings
                    DeploymentSettings deploySet = new DeploymentSettings(mProjItem);
                    serverName = deploySet.TargetServer;
                    databaseName = deploySet.TargetDatabase; //use the target database instead of selectedCube.Parent.Name because selectedCube.Parent.Name only reflects the last place it was deployed to, and we want the user to be able to use the deployment settings to control which deployed server/database to check against
                }

                s.Connect("Data Source=" + serverName);

                Database db = s.Databases.FindByName(databaseName);
                if (db == null)
                {
                    MessageBox.Show("Database " + databaseName + " isn't deployed to server " + serverName + ".");
                    return;
                }

                Cube cube = db.Cubes.Find(realCube.ID);
                if (cube == null)
                {
                    MessageBox.Show("Cube " + realCube.Name + " isn't deployed to database " + databaseName + " on server " + serverName + ".");
                    return;
                }

                AggregationPerformanceProgress progressForm = new AggregationPerformanceProgress();
                progressForm.Init(cube);
                progressForm.ShowDialog(this);

                if (progressForm.Results.Count > 0)
                {
                    OpenAggPerfReport(progressForm.Results, progressForm.MissingResults, progressForm.chkWithoutIndividualAggs.Checked);
                }
                else if (progressForm.Started)
                {
                    if (string.IsNullOrEmpty(progressForm.Errors))
                    {
                        MessageBox.Show("No processed aggregations found in cube " + cube.Name + " on database " + cube.Parent.Name + " on server " + cube.ParentServer.Name + ".");
                    }
                    else
                    {
                        MessageBox.Show(progressForm.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                try
                {
                    s.Disconnect();
                }
                catch { }
            }
        }

        private void testAggregationPerformanceToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            Server s = new Server();
            try
            {
                AggregationDesign aggD = ((MeasureGroup)treeView1.SelectedNode.Parent.Parent.Tag).AggregationDesigns.GetByName(treeView1.SelectedNode.Tag.ToString());
                if (aggD.Parent.IsLinked)
                {
                    MessageBox.Show("This measure group is linked.");
                    return;
                }

                string serverName = "";
                string databaseName = "";
                if (aggD.ParentServer != null)
                {
                    // if we are in Online mode there will be a parent server
                    serverName = aggD.ParentServer.Name;
                    databaseName = aggD.ParentDatabase.Name;
                }
                else
                {
                    // if we are in Project mode we will use the server name from 
                    // the deployment settings
                    DeploymentSettings deploySet = new DeploymentSettings(mProjItem);
                    serverName = deploySet.TargetServer;
                    databaseName = deploySet.TargetDatabase; //use the target database instead of selectedCube.Parent.Name because selectedCube.Parent.Name only reflects the last place it was deployed to, and we want the user to be able to use the deployment settings to control which deployed server/database to check against
                }

                s.Connect("Data Source=" + serverName);

                Database db = s.Databases.FindByName(databaseName);
                if (db == null)
                {
                    MessageBox.Show("Database " + databaseName + " isn't deployed to server " + serverName + ".");
                    return;
                }

                Cube cube = db.Cubes.Find(realCube.ID);
                if (cube == null)
                {
                    MessageBox.Show("Cube " + realCube.Name + " isn't deployed to database " + databaseName + " on server " + serverName + ".");
                    return;
                }

                MeasureGroup liveMG = cube.MeasureGroups.Find(aggD.Parent.ID);
                if (liveMG == null)
                {
                    MessageBox.Show("Measure group " + aggD.Parent.Name + " in cube " + realCube.Name + " isn't deployed to database " + databaseName + " on server " + serverName + ".");
                    return;
                }

                AggregationDesign liveAggD = liveMG.AggregationDesigns.Find(aggD.ID);
                if (liveMG == null)
                {
                    MessageBox.Show("Agg design " + aggD.Name + " in measure group " + aggD.Parent.Name + " in cube " + realCube.Name + " isn't deployed to database " + databaseName + " on server " + serverName + ".");
                    return;
                }

                AggregationPerformanceProgress progressForm = new AggregationPerformanceProgress();
                progressForm.Init(liveAggD);
                progressForm.ShowDialog(this);

                if (progressForm.Results.Count > 0)
                {
                    OpenAggPerfReport(progressForm.Results, progressForm.MissingResults, progressForm.chkWithoutIndividualAggs.Checked);
                }
                else if (progressForm.Started)
                {
                    if (string.IsNullOrEmpty(progressForm.Errors))
                    {
                        MessageBox.Show("No processed aggregations found in agg design " + liveAggD.Name + " in measure group " + liveMG.Name + " in cube " + liveMG.Parent.Name + " on database " + liveMG.ParentDatabase.Name + " on server " + liveMG.ParentServer.Name + ".");
                    }
                    else
                    {
                        MessageBox.Show(progressForm.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                try
                {
                    s.Disconnect();
                }
                catch { }
            }
        }

        private void OpenAggPerfReport(List<AggregationPerformanceTester.AggregationPerformance> listPerf, List<AggregationPerformanceTester.MissingAggregationPerformance> missingPerf, bool showMissingAggs)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate() { OpenAggPerfReport(listPerf, missingPerf, showMissingAggs); }));
            }
            else
            {
                if (listPerf == null) return;

                BIDSHelper.ReportViewerForm frm = new BIDSHelper.ReportViewerForm();
                frm.ReportBindingSource.DataSource = listPerf;
                frm.Report = "SSAS.AggManager.AggregationPerformance.rdlc";

                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "AggManager_AggregationPerformance";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);

                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource2 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource2.Name = "AggManager_MissingAggregationPerformance";
                reportDataSource2.Value = missingPerf;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource2);

                frm.Parameters.Add(new Microsoft.Reporting.WinForms.ReportParameter("ShowMissingAggs", showMissingAggs.ToString()));

                frm.Caption = "Aggregation Performance Report";
                frm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                frm.Show(this);
            }
        }
    }
}