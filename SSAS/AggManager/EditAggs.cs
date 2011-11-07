/*============================================================================
  File:    EditAggs.cs

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
        bool boolInExpandOrCollapse = false;
        private Color nonMaterializedColor = Color.SteelBlue;
        private Color parentChildAttributeColor = Color.SlateGray;
        private Color belowGranularityColor = Color.LightSlateGray;

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
            aggDes = mg.AggregationDesigns.GetByName(strAggDesign);
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

            DataGridTextBoxColumn nameColumnStyle2 = new DataGridTextBoxColumn();
            nameColumnStyle2.MappingName = "Type";
            nameColumnStyle2.HeaderText = "Type";
            nameColumnStyle2.Width = 50;
            myGridStyle.GridColumnStyles.Add(nameColumnStyle2);

            DataGridTextBoxColumn nameColumnStyle1 = new DataGridTextBoxColumn();
            nameColumnStyle1.MappingName = "Aggregations";
            nameColumnStyle1.HeaderText = "Aggregation Definition";
            nameColumnStyle1.Width = iWidth1;
            myGridStyle.GridColumnStyles.Add(nameColumnStyle1);



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
                this.DialogResult = DialogResult.OK;
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
            string sInvalidChars = BIDSHelper.SsasCharacters.Invalid_Name_Characters;

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
            double minSize = 0;
            bool bAggContainsAllGranularityAttributes = true;
            try
            {
                EstimatedAggSize oEstSize = GetEstimatedSize(agg, mg1);
                size = oEstSize.size;
                minSize = oEstSize.minSize;
                bAggContainsAllGranularityAttributes = oEstSize.bAggContainsAllGranularityAttributes;
            }
            catch { }
            finally
            {
                if (size != 0)
                {
                    if (minSize != 0 && minSize < size && !bAggContainsAllGranularityAttributes)
                    {
                        lblEstimatedSize.Text = agg.Name + " Estimated Size: " + (minSize * 100).ToString("#0.00") + "% to " + (size * 100).ToString("#0.00") + "% of fact data";
                        lblEstimatedSize.ForeColor = (size > .3333 ? Color.Red : Color.Black);
                    }
                    else
                    {
                        lblEstimatedSize.Text = agg.Name + " Estimated Size: " + (size * 100).ToString("#0.00") + "% of fact data";
                        lblEstimatedSize.ForeColor = (size > .3333 ? Color.Red : Color.Black);
                    }
                }
                else
                {
                    lblEstimatedSize.Text = agg.Name + " Estimated Size: Unknown (please update EstimatedRows on measure group)";
                    lblEstimatedSize.ForeColor = Color.Black;
                }
            }
        }



        public class EstimatedAggSize
        {
            public double size = 0;
            public double minSize = 0;
            public bool bAggContainsAllGranularityAttributes = true;
            public Aggregation agg;
        }

        /// <summary>
        /// Returns the estimated size of this aggregation.
        /// </summary>
        /// <param name="agg"></param>
        /// <returns></returns>
        public static EstimatedAggSize GetEstimatedSize(Aggregation agg)
        {
            return GetEstimatedSize(agg, agg.ParentMeasureGroup);
        }

        /// <summary>
        /// Returns the estimated size of this aggregation. Use this signature which takes in the MeasureGroup when the agg is not attached to a ParentMeasureGroup.
        /// </summary>
        /// <param name="agg"></param>
        /// <param name="mg1"></param>
        /// <returns></returns>
        public static EstimatedAggSize GetEstimatedSize(Aggregation agg, MeasureGroup mg1)
        {
            double size = 0;
            double minSize = 0;
            bool bAggContainsAllGranularityAttributes = true;

            AggregationAttribute aggAttr;
            AggregationDimension aggDim;
            double dblAggCardinality = 1;
            long iNumSurrogateKeysInAgg = 0;
            int iMeasureGroupDimensionsCount = 0;
            long lngMaxAggDimensionCardinality = 0;

            foreach (MeasureGroupDimension mgDim in mg1.Dimensions)
            {
                long iDimGranularityCardinality = 0;
                double dblDimAggCardinality = 1;
                bool bDimAggCardinalityFound = false;

                if (!(mgDim is RegularMeasureGroupDimension)) continue; //m2m dimensions apparently aren't stored in the agg since they're calculated at runtime

                iMeasureGroupDimensionsCount++; //don't count m2m dimensions

                MeasureGroupAttribute granularity = null;
                RegularMeasureGroupDimension regMgDim = (RegularMeasureGroupDimension)mgDim;
                foreach (MeasureGroupAttribute mgDimAttr in regMgDim.Attributes)
                {
                    if (mgDimAttr.Type == MeasureGroupAttributeType.Granularity)
                    {
                        iDimGranularityCardinality = mgDimAttr.Attribute.EstimatedCount;
                        granularity = mgDimAttr;
                        break;
                    }
                }

                aggDim = agg.Dimensions.Find(mgDim.CubeDimensionID);

                if (aggDim == null || granularity == null || aggDim.Attributes.Find(granularity.AttributeID) == null)
                    bAggContainsAllGranularityAttributes = false;

                if (aggDim != null)
                {
                    foreach (CubeAttribute cubeAttr in mgDim.CubeDimension.Attributes)
                    {
                        aggAttr = aggDim.Attributes.Find(cubeAttr.AttributeID);
                        if (aggAttr != null)
                        {
                            if (!CanReachAttributeFromChildInAgg(aggAttr, mgDim.Dimension.KeyAttribute, false)) //redundant attributes don't increase the cardinality of the attribute
                            {
                                dblDimAggCardinality *= (cubeAttr.Attribute.EstimatedCount == 0 ? 1 : cubeAttr.Attribute.EstimatedCount);
                            }
                            bDimAggCardinalityFound = true;
                            iNumSurrogateKeysInAgg++; //apparently every key, even redundant keys, get stored in the agg
                        }
                    }
                }
                if (dblDimAggCardinality > iDimGranularityCardinality)
                {
                    //shouldn't be more than granularity cardinality because auto-exists prevents that
                    dblDimAggCardinality = (iDimGranularityCardinality == 0 ? 1 : iDimGranularityCardinality);
                }
                if (bDimAggCardinalityFound)
                {
                    dblAggCardinality *= dblDimAggCardinality;
                    if (lngMaxAggDimensionCardinality < dblAggCardinality) lngMaxAggDimensionCardinality = (long)dblDimAggCardinality;
                }
            }
            if (mg1.EstimatedRows != 0 && dblAggCardinality != 0)
            {
                long iMeasureBytes = 0;
                foreach (Microsoft.AnalysisServices.Measure m in mg1.Measures)
                {
                    if (m.DataType == MeasureDataType.Inherited)
                    {
                        if (m.Source.DataSize > 0)
                            iMeasureBytes += m.Source.DataSize;
                        else if (m.Source.DataType == System.Data.OleDb.OleDbType.Integer)
                            iMeasureBytes += 4;
                        else if (m.Source.DataType == System.Data.OleDb.OleDbType.SmallInt)
                            iMeasureBytes += 2;
                        else if (m.Source.DataType == System.Data.OleDb.OleDbType.TinyInt)
                            iMeasureBytes += 1;
                        else
                            iMeasureBytes += 8;
                    }
                    else
                    {
                        if (m.DataType == MeasureDataType.Integer)
                            iMeasureBytes += 4;
                        else if (m.DataType == MeasureDataType.SmallInt)
                            iMeasureBytes += 2;
                        else if (m.DataType == MeasureDataType.TinyInt)
                            iMeasureBytes += 1;
                        else
                            iMeasureBytes += 8;
                    }
                }

                //the size of each row is 4 bytes for each surrogate key plus the size of measures
                long lngFactTableRowSize = (iMeasureGroupDimensionsCount * 4 + iMeasureBytes);
                long lngAggRowSize = (iNumSurrogateKeysInAgg * 4 + iMeasureBytes);

                if (dblAggCardinality > mg1.EstimatedRows) //this is not possible in the data
                {
                    dblAggCardinality = mg1.EstimatedRows;
                }

                //multiply the estimated rows by the size of each row
                size = ((double)(dblAggCardinality * lngAggRowSize)) / ((double)(mg1.EstimatedRows * lngFactTableRowSize));
                //purposefully don't prevent size from being over 1 because an agg can be larger than the fact table if it has more dimension attribute keys than the fact table

                if (lngMaxAggDimensionCardinality > mg1.EstimatedRows) //this is not possible in the data
                {
                    lngMaxAggDimensionCardinality = mg1.EstimatedRows;
                }

                //calculate the min size (best case scenario when there is lots of sparsity in fact table) so you can present a range to the user and give the user an idea of the uncertainty
                minSize = ((double)(lngMaxAggDimensionCardinality * lngAggRowSize)) / ((double)(mg1.EstimatedRows * lngFactTableRowSize));
            }

            EstimatedAggSize ret = new EstimatedAggSize();
            ret.minSize = minSize;
            ret.size = size;
            ret.bAggContainsAllGranularityAttributes = bAggContainsAllGranularityAttributes;
            ret.agg = agg;
            return ret;
        }

        public static string GetEstimatedAggSizeRange(EstimatedAggSize estimate)
        {
            if (estimate.size == 0)
            {
                return null;
            }
            else if (estimate.minSize != 0 && estimate.minSize < estimate.size && !estimate.bAggContainsAllGranularityAttributes)
            {
                return (estimate.minSize * 100).ToString("#0.00") + "% to " + (estimate.size * 100).ToString("#0.00") + "%";
            }
            else
            {
                return (estimate.size * 100).ToString("#0.00") + "%";
            }
        }

        private static bool CanReachAttributeFromChildInAgg(AggregationAttribute attr, DimensionAttribute current, bool bChildIsInAgg)
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
                    if (mgDim is ManyToManyMeasureGroupDimension)
                        parentNode.StateImageIndex = 3;
                    else
                        parentNode.StateImageIndex = 2;

                    foreach (CubeAttribute cubeDimAttr in mgDim.CubeDimension.Attributes)
                    {
                        TreeNode childNode = parentNode.Nodes.Add(cubeDimAttr.AttributeID, cubeDimAttr.Attribute.Name);
                        if (!cubeDimAttr.AttributeHierarchyEnabled)
                        {
                            childNode.NodeFont = new Font(treeViewAggregation.Font, FontStyle.Italic);
                            childNode.ForeColor = Color.Gray;
                        }
                        else if (mgDim is ReferenceMeasureGroupDimension)
                        {
                            ReferenceMeasureGroupDimension refDim = (ReferenceMeasureGroupDimension)mgDim;
                            if (refDim.Materialization == ReferenceDimensionMaterialization.Indirect)
                            {
                                childNode.ForeColor = Color.Red;
                            }
                        }
                        else if (cubeDimAttr.Attribute.Usage == AttributeUsage.Parent)
                        {
                            childNode.ForeColor = Color.Red;
                        }
                    }
                }
            }
            else
            {
                foreach (MeasureGroupDimension mgDim in mg1.Dimensions)
                {
                    TreeNode parentNode = treeViewAggregation.Nodes.Add(mgDim.CubeDimensionID, mgDim.CubeDimension.Name);
                    if (mgDim is ManyToManyMeasureGroupDimension)
                        parentNode.StateImageIndex = 3;
                    else
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
            bool bIsAtOrAboveGranularity = ValidateAggs.IsAtOrAboveGranularity(cubeDimAttr.Attribute, mgDim);
            if (!bIsAtOrAboveGranularity)
            {
                node.ForeColor = belowGranularityColor;
            }
            else if (mgDim is ReferenceMeasureGroupDimension)
            {
                ReferenceMeasureGroupDimension refDim = (ReferenceMeasureGroupDimension)mgDim;
                if (refDim.Materialization == ReferenceDimensionMaterialization.Indirect)
                {
                    node.ForeColor = nonMaterializedColor;
                }
            }
            else if (cubeDimAttr.Attribute.Usage == AttributeUsage.Parent)
            {
                node.ForeColor = parentChildAttributeColor;
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

            //Add Expand/Collapse Events after everything else is loaded 
            //this.treeViewAggregation.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAggregation_AfterExpandOrCollapse);
            this.treeViewAggregation.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewAggregation_ExpandOrCollapse);
            this.treeViewAggregation.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewAggregation_ExpandOrCollapse);
            //this.treeViewAggregation.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAggregation_AfterExpandOrCollapse);
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

                txtSummary.Text = GetCheckedNodeText(treeViewAggregation.Nodes);
            }
            catch
            { }
            sychContr = SynchControls.Unknown;

        }

        private string GetCheckedNodeText(TreeNodeCollection nodes)
        {
            string text = "";
            foreach (TreeNode tn in nodes)
            {
                if (tn.Checked)
                {
                    int iCntMatchingName = 0;
                    foreach (MeasureGroupDimension mgd in mg1.Dimensions)
                    {
                        foreach (CubeAttribute ca in mgd.CubeDimension.Attributes)
                        {
                            if (ca.AttributeHierarchyEnabled && ca.Attribute.AttributeHierarchyEnabled && string.Compare(ca.Attribute.Name, tn.Text, true) == 0)
                            {
                                iCntMatchingName++;
                            }
                        }
                    }
                    if (iCntMatchingName > 1)
                    {
                        //put the cube dimension name on the end to disambiguate
                        TreeNode parentNode = tn;
                        while (parentNode.Parent != null) //loop until you hit the dimension
                            parentNode = parentNode.Parent;
                        text += tn.Text + " (" + parentNode.Text + ")\r\n";
                    }
                    else
                    {
                        text += tn.Text + "\r\n";
                    }
                }
                if (tn.Nodes.Count > 0) 
                {
                    text += GetCheckedNodeText(tn.Nodes);
                }
            }
            return text;
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

            DataRow dr = GetCurrentDataGridRow();
            SetEstimatedSize(GetAggregationFromString(dr[0].ToString(), dr[1].ToString()));
            txtSummary.Text = GetCheckedNodeText(treeViewAggregation.Nodes);

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
                if (!boolInExpandOrCollapse)
                {
                    MouseEventArgs me = (MouseEventArgs)e;
                    TreeNode node = treeViewAggregation.GetNodeAt(me.Location);
                    if (node.StateImageIndex >= 2) //this node is a dimension, so ignore clicks
                        return;
                    if (node.NodeFont != null && node.NodeFont.Italic && !node.Checked)
                    {
                        MessageBox.Show("The cube dimension attribute " + node.Text + " is marked AttributeHierarchyEnabled=false");
                    }
                    else if (node.ForeColor == nonMaterializedColor && !node.Checked) //show this warning unless they're unchecking it
                    {
                        MessageBox.Show("This dimension is related through a non-materialized reference relationship\n\nCreating aggregations on such a relationship is not valid is it can produce incorrect figures.");
                    }
                    else if (node.ForeColor == parentChildAttributeColor && !node.Checked) //show this warning unless they're unchecking it
                    {
                        MessageBox.Show("This attribute is a parent-child attribute. Aggregations are not allowed on it.");
                    }
                    else if (node.ForeColor == belowGranularityColor && !node.Checked) //show this warning unless they're unchecking it
                    {
                        MessageBox.Show("This attribute is below granularity. Aggregations are not allowed on it.");
                    }
                    else
                    {
                        bool bFlipCheck = true;

                        TreeNode dimensionNode = node.Parent;
                        while (dimensionNode.Parent != null)
                            dimensionNode = dimensionNode.Parent;

                        MeasureGroupDimension mgDim = mg1.Dimensions.Find(dimensionNode.Name);
                        if (!node.Checked && mgDim is ManyToManyMeasureGroupDimension)
                        {
                            //check that all the joining dimensions are in the agg, if not, offer to do that
                            //also suggest not including the m2m dimension in the agg
                            DataRow currentDR = GetCurrentDataGridRow();

                            String strAgg = currentDR[1].ToString();
                            String strAggName = currentDR[0].ToString();
                            Aggregation agg = GetAggregationFromString(strAggName, strAgg);

                            ManyToManyMeasureGroupDimension m2mDim = (ManyToManyMeasureGroupDimension)mgDim;
                            MeasureGroup intermediateMG = m2mDim.MeasureGroup;
                            List<MeasureGroupAttribute> missing = new List<MeasureGroupAttribute>();
                            foreach (MeasureGroupDimension commonDim in intermediateMG.Dimensions)
                            {
                                if (!mgDim.Parent.Dimensions.Contains(commonDim.CubeDimensionID)) continue; //this isn't a shared dimension
                                MeasureGroupDimension dataMeasureGroupDim = mgDim.Parent.Dimensions[commonDim.CubeDimensionID];
                                if (dataMeasureGroupDim is ManyToManyMeasureGroupDimension) continue; //this shared dimension is m2m on the data measure group so don't include it

                                RegularMeasureGroupDimension regCommonDim = commonDim as RegularMeasureGroupDimension;
                                if (commonDim.CubeDimensionID != m2mDim.CubeDimensionID || regCommonDim == null)
                                {
                                    //this is a common dimension and the granularity attribute on the intermediate measure group needs to be in the agg
                                    bool bFoundGranularityAgg = false;
                                    MeasureGroupAttribute mga = ValidateAggs.GetGranularityAttribute(regCommonDim);
                                    AggregationDimension aggCommonDim = agg.Dimensions.Find(commonDim.CubeDimensionID);
                                    if (aggCommonDim != null)
                                    {
                                        if (aggCommonDim.Attributes.Find(mga.AttributeID) != null)
                                        {
                                            bFoundGranularityAgg = true;
                                        }
                                    }
                                    if (!bFoundGranularityAgg && mga != null)
                                    {
                                        missing.Add(mga);
                                    }
                                }
                            }
                            string sWarning = "This aggregation will not be used when querying many-to-many dimension [" + m2mDim.CubeDimension.Name + "] unless it also contains ";
                            for (int i = 0; i < missing.Count; i++)
                            {
                                MeasureGroupAttribute mga = missing[i];
                                if (i > 0) sWarning += " and ";
                                sWarning += "[" + mga.Parent.CubeDimension.Name + "].[" + mga.Attribute.Name + "]";
                            }

                            if (missing.Count == 0)
                                sWarning = "";
                            else
                                sWarning += ". ";
                            sWarning += "The many-to-many dimension [" + m2mDim.CubeDimension.Name + "] itself should not be included in the aggregation to workaround a bug.";
                            sWarning += "\r\n\r\nWould you like BIDS Helper to fix this for you?";

                            if (MessageBox.Show(sWarning, "BIDS Helper Agg Manager Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                foreach (MeasureGroupAttribute mga in missing)
                                {
                                    TreeNode missingMGAdimensionNode = treeViewAggregation.Nodes[mga.Parent.CubeDimensionID];
                                    TreeNode missingNode = FindChildNode(missingMGAdimensionNode, mga.AttributeID);
                                    boolHandleClick = true;
                                    missingNode.Checked = true;
                                    boolHandleClick = false;
                                }
                                bFlipCheck = false;
                            }
                        }

                        if (bFlipCheck)
                        {
                            boolHandleClick = true;
                            node.Checked = !node.Checked;
                            boolHandleClick = false;
                        }

                        DataRow dr = GetCurrentDataGridRow();
                        SetEstimatedSize(GetAggregationFromString(dr[0].ToString(), dr[1].ToString()));
                        boolInExpandOrCollapse = false;

                        txtSummary.Text = GetCheckedNodeText(treeViewAggregation.Nodes);
                    }
                }
            }
            //HACK: Due to the way the custom checked treeview is implemented we have to unset the following 
            //      flag to indicate that we are finished with Expanding/Collapsing and not actually clicking on
            //      the node.
            //      This is to address the following bug http://www.codeplex.com/bidshelper/WorkItem/View.aspx?WorkItemId=15858
            boolInExpandOrCollapse = false;
        }

        private void buttonValidate_Click(object sender, EventArgs e)
        {
            //the latest version of aggs is just stored in a grid, so throw this into a new temporary agg design
            AggregationDesign tempAggDesign = mg1.AggregationDesigns.Add();
            try
            {
                DataView myDataView = (DataView)dataGrid1.DataSource;
                foreach (DataRow dr in myDataView.Table.Select(null, myDataView.Sort))
                {
                    String strAgg = dr[1].ToString();
                    String strAggName = dr[0].ToString();
                    Aggregation agg = GetAggregationFromString(strAggName, strAgg);
                    tempAggDesign.Aggregations.Add(agg);
                }

                ValidateAggs.Validate(tempAggDesign, aggDes.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                mg1.AggregationDesigns.Remove(tempAggDesign); //remove the temporary agg design
            }
        }


        //HACK: Due to the way the custom checked treeview is implemented we have to set the following 
        //      flag to indicate that we are only Expanding/Collapsing and not actually clicking on
        //      the node.
        //      This is to address the following bug http://www.codeplex.com/bidshelper/WorkItem/View.aspx?WorkItemId=15858
        private void treeViewAggregation_ExpandOrCollapse(object sender, TreeViewCancelEventArgs e)
        {
            boolInExpandOrCollapse = true;
        }




        private void buttonFindSimilar_Click(object sender, EventArgs e)
        {

            //the latest version of aggs is just stored in a grid, so throw this into a new temporary agg design
            AggregationDesign tempAggDesign = mg1.AggregationDesigns.Add();
            try
            {
                DataView myDataView = (DataView)dataGrid1.DataSource;
                foreach (DataRow dr in myDataView.Table.Select(null, myDataView.Sort))
                {
                    String strAgg = dr[1].ToString();
                    String strAggName = dr[0].ToString();
                    Aggregation agg = GetAggregationFromString(strAggName, strAgg);
                    tempAggDesign.Aggregations.Add(agg);
                }

                bool bUseCounts = false;
                if (MessageBox.Show("Do you want to consider estimated member counts?\r\n\r\nClicking Yes will exclude similar aggregations with vastly different cardinalities.", "Search Similar Aggregations - Consider Estimated Member Counts?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    bUseCounts = true;
                }

                SearchSimilarAggs.ShowAggsSimilaritiesReport(mg1, aggDes.Name, bUseCounts);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                mg1.AggregationDesigns.Remove(tempAggDesign); //remove the temporary agg design
            }


        }

        private void chkVerbose_CheckChanged(object sender, EventArgs e)
        {
            splitDetails.Panel2Collapsed = !chkVerbose.Checked;
        }


    }

}