/*============================================================================
  File:    EditAgggs.cs

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
using Microsoft.AnalysisServices.AdomdClient;


namespace AggManager
{
    public partial class EditAggs : Form
    {
        private MeasureGroup mg1;
        private AggregationDesign aggDes;
        string[,] dimAttributes;
        string[] dimNames;
        string[] dimIDs;
        bool boolHandleClick = false;
        bool boolIsRigid = false;

        private Color nonMaterializedColor = Color.SteelBlue;

        enum SynchControls 
        { 
            SynchTreeToGrid = 1,
            SynchGridToTreeView = 2,
            Unknown = 3
        }

        SynchControls sychContr = SynchControls.Unknown;

        public EditAggs()
        {
            InitializeComponent();
        }

        public void Init(string strAggDesign, 
            MeasureGroup mg, 
            string[,] inDimAttributes,  
            string[] inDimNames ,
            string[] inDimIDs)
        {
            this.Text = this.Text + " Aggregation Design: " + strAggDesign; 
            mg1 = mg;
            aggDes = mg.AggregationDesigns[strAggDesign];
            dimAttributes = inDimAttributes;
            dimNames = inDimNames;
            dimIDs = inDimIDs;

            DataTable myTable = new DataTable("Aggregations");

            DataColumn colItem = new DataColumn("Name", Type.GetType("System.String"));
            myTable.Columns.Add(colItem);
            colItem = new DataColumn("Aggregations", Type.GetType("System.String"));
            myTable.Columns.Add(colItem);
            colItem = new DataColumn("Type", Type.GetType("System.String"));
            myTable.Columns.Add(colItem);

            DataView myDataView = new DataView(myTable);
            dataGrid1.DataSource = myDataView;

            DataRow NewRow;
            foreach (Aggregation agg in aggDes.Aggregations)
            {
                NewRow = myTable.NewRow();
                NewRow["Aggregations"] = ConvertAggToSting(agg);
                NewRow["Name"] = agg.Name;
                myTable.Rows.Add(NewRow);
           }

           AddGridStyle();

           PopulateTreeView();
           checkBoxRelationships.Checked = true;  
           
           int i = 0;
           foreach (DataRow dRow in myDataView.Table.Rows)
           {
               dataGrid1.CurrentRowIndex = i;
               dataGrid1_Click(null, null);
               i++;
           }

           myDataView.AllowNew = false;
           
        }


        /// <summary>
        /// Setting data grid column captions and width
        /// </summary>
        private void AddGridStyle()
        {
            DataView myDataView = (DataView)dataGrid1.DataSource;

            int iWidth0 = 100;
            int iWidth1 = 400;
            Graphics Graphics = dataGrid1.CreateGraphics();

            if (myDataView.Table.Rows.Count > 0)
			{
				int iColWidth = (int)(Graphics.MeasureString
                    (myDataView.Table.Rows[0].ItemArray[0].ToString(),
					dataGrid1.Font).Width);
				iWidth0 = (int)System.Math.Max(iWidth0, iColWidth);

                iColWidth = (int)(Graphics.MeasureString
                    (myDataView.Table.Rows[0].ItemArray[1].ToString(),
                    dataGrid1.Font).Width);
                iWidth1 = (int)System.Math.Max(iWidth1, iColWidth);
			}
            
            DataGridTableStyle myGridStyle = new DataGridTableStyle();
            myGridStyle.MappingName = "Aggregations";

            DataGridTextBoxColumn nameColumnStyle = new DataGridTextBoxColumn();
            nameColumnStyle.MappingName = "Name";
            nameColumnStyle.HeaderText = "Name";
            nameColumnStyle.Width = iWidth0;
            myGridStyle.GridColumnStyles.Add(nameColumnStyle);

            DataGridTextBoxColumn nameColumnStyle1 = new DataGridTextBoxColumn();
            nameColumnStyle1.MappingName = "Aggregations";
            nameColumnStyle1.HeaderText = "Aggregation Definition";
            nameColumnStyle1.Width = iWidth1;
            myGridStyle.GridColumnStyles.Add(nameColumnStyle1);

            DataGridTextBoxColumn nameColumnStyle2 = new DataGridTextBoxColumn();
            nameColumnStyle2.MappingName = "Type";
            nameColumnStyle2.HeaderText = "Type";
            nameColumnStyle2.Width = 50;
            myGridStyle.GridColumnStyles.Add(nameColumnStyle2);

            dataGrid1.TableStyles.Add(myGridStyle);
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                DataView myDataView;
                int i = 0;
                string strRow;
                string strAggName;

                if (!AggNamesAreValid()) return;

                this.Cursor = Cursors.WaitCursor;

                aggDes.Aggregations.Clear();

                myDataView = (DataView)dataGrid1.DataSource;

                foreach (DataRow dRow in myDataView.Table.Rows)
                {
                    if (dRow.RowState.ToString() != "Deleted")
                    {
                        strAggName = dRow.ItemArray[0].ToString();
                        strRow = dRow.ItemArray[1].ToString();
                        i++;
                        if (AddAggregationToAggDesign(
                            aggDes,
                            strRow,
                            strAggName) == false)
                            i--; // No aggregation has been added

                    }

                }

                this.Cursor = Cursors.Default;
                //MessageBox.Show("Aggregation design: " + aggDes.Name + "  has been updated with " + i.ToString() + " aggregations");
                this.Close();
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                if (!String.IsNullOrEmpty(ex.Message)) MessageBox.Show("Error: " + ex.Message);
            }
        }

        private bool AggNamesAreValid()
        {
            DataView myDataView = (DataView)dataGrid1.DataSource;
            string strAggName;
            List<string> aggNames = new List<string>();
            string sInvalidChars = ".,;'`:/\\*|?\"&%$!+=()[]{}<>";

            foreach (DataRow dRow in myDataView.Table.Rows)
            {
                if (dRow.RowState != DataRowState.Deleted)
                {
                    strAggName = dRow.ItemArray[0].ToString();
                    if (strAggName.IndexOfAny(sInvalidChars.ToCharArray()) > 0)
                    {
                        MessageBox.Show(strAggName + " cannot contain any of the following characters: " + sInvalidChars);
                        return false;
                    }
                    if (aggNames.Contains(strAggName))
                    {
                        MessageBox.Show(strAggName + " is a duplicate aggregation name.");
                        return false;
                    }
                    aggNames.Add(strAggName);
                }
            }
            return true;
        }

        /// <summary>
        /// Helper function takes aggregation as input and returns string representation of aggregation
        /// </summary>

        private string ConvertAggToSting(Aggregation agg)
        {
            string outStr = "";
            AggregationAttribute aggAttr;
            AggregationDimension aggDim;

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

        private void SetEstimatedSize(Aggregation agg)
        {
            double size = 0;
            try
            {
                AggregationAttribute aggAttr;
                AggregationDimension aggDim;
                long iAggCardinality = 1;

                foreach (MeasureGroupDimension mgDim in mg1.Dimensions)
                {
                    long iDimGranularityCardinality = 0;
                    long iDimAggCardinality = 1;
                    bool bDimAggCardinalityFound = false;

                    if (!(mgDim is RegularMeasureGroupDimension)) continue; //not sure how to handle m2m dimensions

                    RegularMeasureGroupDimension regMgDim = (RegularMeasureGroupDimension)mgDim;
                    aggDim = agg.Dimensions.Find(mgDim.CubeDimensionID);
                    foreach (MeasureGroupAttribute mgDimAttr in regMgDim.Attributes)
                    {
                        if (mgDimAttr.Type == MeasureGroupAttributeType.Granularity)
                        {
                            iDimGranularityCardinality = mgDimAttr.Attribute.EstimatedCount;
                            break;
                        }
                    }
                    if (aggDim != null)
                    {
                        foreach (CubeAttribute cubeAttr in mgDim.CubeDimension.Attributes)
                        {
                            aggAttr = aggDim.Attributes.Find(cubeAttr.AttributeID);
                            if (aggAttr != null && !CanReachAttributeFromChildInAgg(aggAttr,mgDim.Dimension.KeyAttribute, false))
                            {
                                iDimAggCardinality *= cubeAttr.Attribute.EstimatedCount;
                                bDimAggCardinalityFound = true;
                            }
                        }
                    }
                    if (iDimAggCardinality > iDimGranularityCardinality)
                    {
                        //shouldn't be more than granularity cardinality because auto-exists prevents that
                        iDimAggCardinality = iDimGranularityCardinality;
                    }
                    if (bDimAggCardinalityFound)
                    {
                        iAggCardinality *= iDimAggCardinality;
                    }
                }
                if (mg1.EstimatedRows != 0 || iAggCardinality != 0)
                {
                    size = ((double)iAggCardinality / (double)mg1.EstimatedRows);
                    if (size > 1) size = 1;
                }
            }
            catch { }
            finally
            {
                if (size != 0)
                {
                    lblEstimatedSize.Text = agg.Name + " Estimated Size: " + (size*100).ToString("#0.00") + "% of fact data";
                    lblEstimatedSize.ForeColor = (size > .3333 ? Color.Red : Color.Black);
                }
                else
                {
                    lblEstimatedSize.Text = agg.Name + " Estimated Size: Unknown";
                    lblEstimatedSize.ForeColor = Color.Black;
                }
            }
        }

        private bool CanReachAttributeFromChildInAgg(AggregationAttribute attr, DimensionAttribute current, bool bChildIsInAgg)
        {
            bChildIsInAgg = bChildIsInAgg || attr.Parent.Attributes.Contains(current.ID);
            foreach (AttributeRelationship r in current.AttributeRelationships)
            {
                if (r.AttributeID == attr.AttributeID && bChildIsInAgg)
                {
                    return true;
                }
                else
                {
                    if (CanReachAttributeFromChildInAgg(attr, r.Attribute, bChildIsInAgg)) return true;
                }
            }
            return false;
        }

        Boolean AddAggregationToAggDesign(AggregationDesign aggDesign, string instr, string aggName)
        {
            try
            {
                Aggregation agg;
                agg = aggDesign.Aggregations.Find(aggName);

                if (agg != null)
                    aggName = "Aggregation " + aggDesign.Aggregations.Count.ToString();

                if (aggName == "")
                    aggName = "Aggregation " + aggDesign.Aggregations.Count.ToString();

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
                MessageBox.Show("Problem saving " + aggName + ": " + ex.Message);
                throw new Exception(""); //blank exception means not to report it
            }
        }

        private Aggregation GetAggregationFromString(string aggregationName, string instr)
        {
            Aggregation agg = new Aggregation();
            agg.Name = aggregationName;
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
            return agg;
        }

        /// <summary>
        /// Called to populate tree view representing dimension attributes
        /// </summary>
        public void PopulateTreeView()
        {
            treeViewAggregation.SuspendLayout();
            treeViewAggregation.Nodes.Clear();

            if (!checkBoxRelationships.Checked)
            {
                foreach (MeasureGroupDimension mgDim in mg1.Dimensions)
                {
                    TreeNode parentNode = treeViewAggregation.Nodes.Add(mgDim.CubeDimensionID, mgDim.CubeDimension.Name);
                    parentNode.StateImageIndex = 2;
                    foreach (CubeAttribute cubeDimAttr in mgDim.CubeDimension.Attributes)
                    {
                        TreeNode childNode = parentNode.Nodes.Add(cubeDimAttr.AttributeID, cubeDimAttr.Attribute.Name);
                        if (!cubeDimAttr.AttributeHierarchyEnabled)
                        {
                            childNode.NodeFont = new Font(treeViewAggregation.Font, FontStyle.Italic);
                            childNode.ForeColor = Color.Gray;
                        }
                        if (mgDim is ReferenceMeasureGroupDimension)
                        {
                            ReferenceMeasureGroupDimension refDim = (ReferenceMeasureGroupDimension)mgDim;
                            if (refDim.Materialization == ReferenceDimensionMaterialization.Indirect)
                            {
                                childNode.ForeColor = Color.Red;
                            }
                        }
                    }

                }
            }
            else
            {
                foreach (MeasureGroupDimension mgDim in mg1.Dimensions)
                {
                    TreeNode parentNode = treeViewAggregation.Nodes.Add(mgDim.CubeDimensionID, mgDim.CubeDimension.Name);
                    parentNode.StateImageIndex = 2;
                    foreach (CubeAttribute cubeDimAttr in mgDim.CubeDimension.Attributes)
                        if (cubeDimAttr.Attribute.Usage == AttributeUsage.Key)
                        {
                            parentNode = parentNode.Nodes.Add(cubeDimAttr.AttributeID, cubeDimAttr.Attribute.Name);
                            AddTreeViewNodeChildren(parentNode, cubeDimAttr,mgDim);
                        }
                }
            }
            treeViewAggregation.ExpandAll();
            treeViewAggregation.Nodes[0].EnsureVisible(); //scroll to top
            treeViewAggregation.ResumeLayout(false);
        }


        /// <summary>
        /// Recursive function. Adds nodes to the tree view control.
        /// Adds nodes accourding to attribute relationships
        /// </summary>

        private void AddTreeViewNodeChildren(TreeNode node, CubeAttribute cubeDimAttr , MeasureGroupDimension mgDim)
        {
            if (mgDim is ReferenceMeasureGroupDimension)
            {
                ReferenceMeasureGroupDimension refDim = (ReferenceMeasureGroupDimension)mgDim;
                if (refDim.Materialization == ReferenceDimensionMaterialization.Indirect)
                {
                    node.ForeColor = nonMaterializedColor;
                }
            }
            foreach (AttributeRelationship attRel in cubeDimAttr.Attribute.AttributeRelationships)
            {
                CubeAttribute childAttr = cubeDimAttr.Parent.Attributes.Find(attRel.AttributeID);
                if (childAttr == null) break; 
                TreeNode childNode = node.Nodes.Add(childAttr.AttributeID, childAttr.Attribute.Name);
                if (!childAttr.AttributeHierarchyEnabled)
                {
                    childNode.NodeFont = new Font(treeViewAggregation.Font, FontStyle.Italic);
                    childNode.ForeColor = Color.Gray;
                }

                childNode.Tag = attRel;
                AddTreeViewNodeChildren( childNode, childAttr,mgDim);
            }
        }
        
        private void EditAggs_Load(object sender, EventArgs e)
        {
            //Add menu item allowing for adding new aggregations to agg design
            MenuItem item1 = new MenuItem("Add Aggregation", new EventHandler(AddAggregationHandler));
            ContextMenu menu = new ContextMenu(new MenuItem[] { item1});
            this.dataGrid1.ContextMenu = menu;

            dataGrid1_Click(sender, e);

        }

        void AddAggregationHandler(object sender, EventArgs e)
        {
            DataView myDataView;
            myDataView = (DataView)dataGrid1.DataSource;

            String strAgg = "";

            int i = 0, j = 0;
            while (dimNames[i] != null)
            {
                j = 0;
                while (dimAttributes[i, j] != null)
                {
                    strAgg = strAgg + "0";
                    j++;
                }
                i++;
                strAgg = strAgg + ",";
            }
            strAgg = strAgg.Remove(strAgg.Length - 1);

            DataRow NewRow = myDataView.Table.NewRow();
            NewRow["Aggregations"] = strAgg;
            NewRow["Name"] = "Aggregation " + myDataView.Table.Rows.Count.ToString();

            myDataView.Table.Rows.Add(NewRow);
            dataGrid1.CurrentRowIndex = NewRow.Table.Rows.IndexOf(NewRow);
            dataGrid1_Click(null, null);

        }

        /// <summary>
        /// Helps select the correct data source row even when grid is sorted
        /// </summary>
        /// <returns></returns>
        private DataRow GetCurrentDataGridRow()
        {
            DataView myDataView = (DataView)dataGrid1.DataSource;
            return myDataView.Table.Select(null, myDataView.Sort)[dataGrid1.CurrentRowIndex];
        }

        /// <summary>
        /// Helps synchronizing aggregation definition in current row data grid control to
        /// aggregation definition presented as checked boxes for appropriate attributes in 
        /// tree view control
        /// </summary>
        private void dataGrid1_Click(object sender, EventArgs e)
        {
            if (sychContr == SynchControls.SynchGridToTreeView)
                return;

            try
            {
                DataRow dr = GetCurrentDataGridRow();
                String strAgg = dr[1].ToString();
                String strAggName = dr[0].ToString();

                if (checkBoxRelationships.Checked)
                {
                    sychContr = SynchControls.SynchGridToTreeView;
                   if (CheckOffTreeView(strAgg))
                        dr[2] = "Rigid";
                    else
                        dr[2] = "Flexible";

                }
                else
                {
                    int i = 0;
                    foreach (TreeNode parentNode in treeViewAggregation.Nodes)
                    {
                        foreach (TreeNode childNode in parentNode.Nodes)
                        {
                            if (strAgg[i].ToString() == "1")
                            {
                                sychContr = SynchControls.SynchGridToTreeView;
                                childNode.Checked = true;
                                childNode.BackColor = dataGrid1.HeaderBackColor;
                            }
                            else 
                            {
                                sychContr = SynchControls.SynchGridToTreeView;
                                childNode.Checked = false;
                                childNode.BackColor = treeViewAggregation.BackColor;
                            }
                            i++;
                        }
                        i++;
                    }
                }
                SetEstimatedSize(GetAggregationFromString(strAggName, strAgg));
            }
            catch
            { }
            sychContr = SynchControls.Unknown;
        }   

        private void dataGrid1_CurrentCellChanged(object sender, EventArgs e)
        {
            dataGrid1_Click(sender, e);
        }



        /// <summary>
        /// Main function called when user checks/uncheckes dimension attribute.
        /// If attribute checked it needs to be added to the string representation of an aggregations
        /// </summary>
        private void treeViewAggregation_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // If happen to synch Tree to grid, we should quit
            if (sychContr == SynchControls.SynchTreeToGrid)
                return;

            if (e.Action == TreeViewAction.Unknown && !boolHandleClick ) return;
            if (e.Node.Parent == null) 
                return;

            DataRow dr = GetCurrentDataGridRow();
            String strAgg = dr[1].ToString();

            int i = 0;
            int intAttrCount = 0;
            TreeNode parent = e.Node.Parent;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            if (parent == null) MessageBox.Show("Cannot find parent");
            string parentName = parent.Name;


            while (treeViewAggregation.Nodes[i].Name != parentName)
            {
                intAttrCount++;
                intAttrCount += mg1.Dimensions[treeViewAggregation.Nodes[i].Name].CubeDimension.Attributes.Count;
                i++;
                
            }

            if (checkBoxRelationships.Checked)
            {
                //Find a place of an attribute in the attr string
                int j = 0;
                foreach (CubeAttribute cubeDimAttr in mg1.Dimensions[treeViewAggregation.Nodes[i].Name].CubeDimension.Attributes)
                {
                    if (cubeDimAttr.AttributeID == e.Node.Name)
                        intAttrCount += j;

                    j++;
                }
            }
            else
            {
                intAttrCount += e.Node.Index;
            }


            if (e.Node.Checked)
            {
                strAgg = strAgg.Insert(intAttrCount, "1");
                strAgg = strAgg.Remove(intAttrCount + 1, 1);


                System.Drawing.Color ii = dataGrid1.HeaderBackColor;
                e.Node.BackColor = ii;
            }
            else
            {
                strAgg = strAgg.Insert(intAttrCount, "0");
                strAgg = strAgg.Remove(intAttrCount + 1, 1);
                e.Node.BackColor = treeViewAggregation.BackColor;
            }
            dr[1] = strAgg;

            if (sychContr == SynchControls.SynchTreeToGrid)
                dataGrid1_Click(null, null);
            sychContr = SynchControls.Unknown;
        }

        private void checkBoxRelationships_CheckedChanged(object sender, EventArgs e)
        {
            PopulateTreeView();
            dataGrid1_Click(sender, e);
        }


        /// <summary>
        /// Takes aggregation string as an input and interates over nodes in tree view control
        /// if attribute participates in aggregation, function will check the check box for 
        /// the attribute
        /// </summary>
        private bool CheckOffTreeView(string strAgg)
        {
            string a1; 
            int dimNum = 0;
            int attrNum = 0;
            bool newDim = true;

            boolIsRigid = true;

            TreeNode dimNode = treeViewAggregation.Nodes[0];
            TreeNode attrNode; 
            for (int i = 0; i < strAgg.Length; i++)
            {
                a1 = strAgg[i].ToString();
                switch (a1)
                {
                    case ",":
                        dimNum++;
                        attrNum = -1;
                        newDim = true;
                        dimNode = treeViewAggregation.Nodes[dimNum];
                        break;
                    case "0":
                        attrNode = FindChildNode(dimNode, dimAttributes[dimNum, attrNum]);
                        if (attrNode != null)
                        {
                            attrNode.Checked = false;
                            attrNode.BackColor = treeViewAggregation.BackColor;
                        }
                        break;
                    case "1":

                        if (newDim) newDim = false;
                        attrNode = FindChildNodeAndRelationships(dimNode, dimAttributes[dimNum, attrNum]);
                        if (attrNode != null)
                        {
                            attrNode.Checked = true;
                            attrNode.BackColor = dataGrid1.HeaderBackColor;
                        }
                        break;
                    default:
                        break;
                }
                attrNum++;
            }

           treeViewAggregation.ExpandAll();
           return boolIsRigid;
        }

        private TreeNode FindChildNodeAndRelationships(TreeNode node, string strNodeID)
        {
            TreeNode returnNode;
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Name == strNodeID)
                {
                    if (childNode.Tag != null) 
                        if (((AttributeRelationship)childNode.Tag).RelationshipType == RelationshipType.Flexible)
                            boolIsRigid = false;
                    return childNode; 
                }
                returnNode = FindChildNodeAndRelationships(childNode, strNodeID);
                if (returnNode != null)
                {
                    if (node.Tag != null)
                        if (((AttributeRelationship)node.Tag).RelationshipType == RelationshipType.Flexible)
                            boolIsRigid = false;
                    return returnNode;
                }
            }
            return null;
        }

        private TreeNode FindChildNode(TreeNode node, string strNodeID)
        {
            TreeNode returnNode;
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Name == strNodeID)
                    return childNode;
                returnNode = FindChildNode(childNode, strNodeID);
                if (returnNode != null)
                    return returnNode;
            }
            return null;
        }


        /// <summary>
        /// Goes over every aggregation in the aggregation design and eliminates 
        /// redundant attributes from every aggregation
        /// Redundant is an attribute in aggregation that in the attribute relationship chain appears on top
        /// of another attribute.
        /// For example State attribute should not be selected if City attribute appears in the aggregation.
        /// </summary>
        private void buttonOptimizeAgg_Click(object sender, EventArgs e)
        {
            if (checkBoxRelationships.Checked == false)
            {
                MessageBox.Show("Please switch to relationships view");
                return;
            }
            this.Cursor = Cursors.WaitCursor;


            DataView myDataView;
            myDataView = (DataView)dataGrid1.DataSource;

            boolHandleClick = true;
            int i = 0;
            foreach( DataRow dataGridRow in myDataView.Table.Rows)
            {
                dataGrid1.CurrentRowIndex = i;
                dataGrid1_Click(null,null);

                foreach (TreeNode node in treeViewAggregation.Nodes)
                    OptimizeNode(node, false);
                i++;
            }
            boolHandleClick = false;

            this.Cursor = Cursors.Default;
        }

        private void OptimizeNode(TreeNode node, bool removeChecks)
        {
            bool localRemoveChecks = removeChecks;
            foreach (TreeNode childNode in node.Nodes)
            {
                localRemoveChecks = removeChecks;
                if (childNode.Checked == true)
                    if (removeChecks) childNode.Checked = false;
                    else localRemoveChecks = true;

                OptimizeNode(childNode, localRemoveChecks);
            }
        }


        private void buttonEliminateDupe_Click(object sender, EventArgs e)
        {
            DataView myDataView;
            myDataView = (DataView)dataGrid1.DataSource;
            int i = 0;
            List<string> uniqueAggs = new List<string>();
            while (i < myDataView.Table.Rows.Count)
            {
                if (uniqueAggs.Contains(myDataView.Table.Rows[i].ItemArray[1].ToString()))
                {
                    myDataView.Table.Rows.Remove(myDataView.Table.Rows[i]);
                }
                else
                {
                    uniqueAggs.Add(myDataView.Table.Rows[i].ItemArray[1].ToString());
                    i++;
                }
            }
        }

        private void treeViewAggregation_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs)
            {
                MouseEventArgs me = (MouseEventArgs)e;
                TreeNode node = treeViewAggregation.GetNodeAt(me.Location);
                if (node.StateImageIndex == 2) //this node is a dimension, so ignore clicks
                    return;
                if (node.NodeFont != null && node.NodeFont.Italic && !node.Checked)
                {
                    MessageBox.Show("The cube dimension attribute " + node.Text + " is marked AttributeHierarchyEnabled=false");
                }
                else if (node.ForeColor == nonMaterializedColor)
                {
                    MessageBox.Show("This dimension is related through a non-materialized reference relationship\n\nCreating aggregations on such a relationship is not valid is it can produce incorrect figures.");
                }
                else
                {
                    boolHandleClick = true;
                    node.Checked = !node.Checked;
                    boolHandleClick = false;
                    DataRow dr = GetCurrentDataGridRow();
                    SetEstimatedSize(GetAggregationFromString(dr[0].ToString(), dr[1].ToString()));
                }
            }
        }

    }

}