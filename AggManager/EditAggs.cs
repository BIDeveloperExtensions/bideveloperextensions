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
        bool boolOptimizeAgg = false;
        bool boolIsRigid = false;
        
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
            this.Text = this.Text + " Aggregation Desing :" + strAggDesign; 
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

            DataView myDataView;
            int i = 0;
            string strRow;
            string strAggName;

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

            MessageBox.Show( "Aggregation desing :" + aggDes.Name + "  has been updated with " + i.ToString() + " aggregations ");

            this.Cursor = Cursors.Default;
            this.Close();
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

        Boolean AddAggregationToAggDesign(AggregationDesign aggDesign, string instr, string aggName)
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

            if (agg.Dimensions.Count == 0)
            {
                aggDesign.Aggregations.Remove(agg);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Called to populate tree view representing dimension attributes
        /// </summary>
        public void PopulateTreeView()
        {
            treeViewAggregation.Nodes.Clear();

            if (!checkBoxRelationships.Checked )
            {
                foreach (MeasureGroupDimension mgDim in mg1.Dimensions)
                {
                    TreeNode parentNode = treeViewAggregation.Nodes.Add(mgDim.CubeDimensionID, mgDim.CubeDimensionID);
                    foreach (CubeAttribute cubeDimAttr in mgDim.CubeDimension.Attributes)
                        parentNode.Nodes.Add(cubeDimAttr.AttributeID, cubeDimAttr.AttributeID);

                }
            }
            else
            {
                foreach (MeasureGroupDimension mgDim in mg1.Dimensions)
                {
                    TreeNode parentNode = treeViewAggregation.Nodes.Add(mgDim.CubeDimensionID, mgDim.CubeDimensionID);

                    foreach (CubeAttribute cubeDimAttr in mgDim.CubeDimension.Attributes)
                        if (cubeDimAttr.Attribute.Usage == AttributeUsage.Key)
                        {
                            parentNode = parentNode.Nodes.Add(cubeDimAttr.AttributeID, cubeDimAttr.AttributeID);
                            AddTreeViewNodeChildren(parentNode, cubeDimAttr);
                        }
                }
            }
            treeViewAggregation.ExpandAll();
        }


        /// <summary>
        /// Recursive function. Adds nodes to the tree view control.
        /// Adds nodes accourding to attribute relationships
        /// </summary>

        private void AddTreeViewNodeChildren(TreeNode node, CubeAttribute cubeDimAttr )
        {
            foreach (AttributeRelationship attRel in cubeDimAttr.Attribute.AttributeRelationships)
            {
                CubeAttribute childAttr = cubeDimAttr.Parent.Attributes.Find(attRel.AttributeID);
                if (childAttr == null) break; 
                TreeNode childNode = node.Nodes.Add(childAttr.AttributeID, childAttr.AttributeID);
                childNode.Tag = attRel;
                AddTreeViewNodeChildren( childNode, childAttr);
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
                int intGridOrdinal = dataGrid1.CurrentRowIndex;
                DataView myDataView;
                myDataView = (DataView)dataGrid1.DataSource;

                String strAgg = myDataView.Table.Rows[intGridOrdinal].ItemArray[1].ToString();

                if (checkBoxRelationships.Checked)
                {
                    sychContr = SynchControls.SynchGridToTreeView;
                   if (CheckOffTreeView(strAgg))
                        myDataView.Table.Rows[intGridOrdinal][2] = "Rigid";
                    else
                        myDataView.Table.Rows[intGridOrdinal][2] = "Flexible";

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

            if (e.Action == TreeViewAction.Unknown && !boolOptimizeAgg ) return;
            if (e.Node.Parent == null) 
                return;

            int intGridOrdinal = dataGrid1.CurrentRowIndex;
            DataView myDataView;
            myDataView = (DataView)dataGrid1.DataSource;

            String strAgg = myDataView.Table.Rows[intGridOrdinal].ItemArray[1].ToString();

            int i = 0;
            int intAttrCount = 0;
            int slashIndex = 0;
            string parentName = "";
            slashIndex = e.Node.FullPath.IndexOf("\\");
            if ( slashIndex > 0 ) parentName = e.Node.FullPath.Substring( 0, slashIndex);
            else MessageBox.Show ( "Cannot find parent");

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
            myDataView.Table.Rows[intGridOrdinal][1] = strAgg;

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

            boolOptimizeAgg = true;
            int i = 0;
            foreach( DataRow dataGridRow in myDataView.Table.Rows)
            {
                dataGrid1.CurrentRowIndex = i;
                dataGrid1_Click(null,null);

                foreach (TreeNode node in treeViewAggregation.Nodes)
                    OptimizeNode(node, false);
                i++;
            }
            boolOptimizeAgg = false;

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
            myDataView.Sort = "Aggregations";
            int i = 0;
            try
            {

                while (i < myDataView.Table.Rows.Count - 1)
                {
                    while (myDataView.Table.Rows[i].ItemArray[1].ToString() == myDataView.Table.Rows[i + 1].ItemArray[1].ToString())
                        myDataView.Table.Rows.Remove(myDataView.Table.Rows[i + 1]);
                    i++;
                }
            }
            catch
            { }
            myDataView.Sort = "";
        }

    }

}