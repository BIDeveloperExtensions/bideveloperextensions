using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper.SSAS
{
    public partial class MeasureGroupHealthCheckForm : Form
    {
        public MeasureGroupHealthCheckForm()
        {
            InitializeComponent();
            this.Icon = BIDSHelper.Resources.Common.BIDSHelper;
        }

        private void MeasureGroupHealthCheckForm_Load(object sender, EventArgs e)
        {
            FormatGrid();
        }

        private void FormatGrid()
        {
            //this doesn't work... ends up middle aligning the col headers I wanted right aligned... bug in DataGridView maybe
            //foreach (DataGridViewColumn col in this.dataGridView1.Columns)
            //{
            //    col.HeaderCell.Style.Alignment = col.DefaultCellStyle.Alignment;
            //}
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                MeasureGroupHealthCheckPlugin.MeasureHealthCheckResult item = (MeasureGroupHealthCheckPlugin.MeasureHealthCheckResult)row.DataBoundItem;
                if (item.CurrentDataType != item.DataType)
                {
                    Color newColor = Color.Blue;
                    if (item.CurrentDataType.max < item.DataType.max)
                    {
                        newColor = Color.Red;
                    }
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.ForeColor = newColor;
                    }
                }
            }
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (this.dataGridView1.CurrentCell.ColumnIndex == this.DataType.Index)
            {
                //limit this dropdown to just the valid data types for this measure
                BindingSource bindingSource = this.dataGridView1.DataSource as BindingSource;
                MeasureGroupHealthCheckPlugin.MeasureHealthCheckResult item = bindingSource.Current as MeasureGroupHealthCheckPlugin.MeasureHealthCheckResult;
                DataGridViewComboBoxEditingControl comboBox = e.Control as DataGridViewComboBoxEditingControl;
                comboBox.DataSource = item.PossibleDataTypes;
                comboBox.SelectedValue = item.DataType;

                //not necessary to capture this event and commit a change before they leave the cell because we decided not to change cell colors or anything
                //comboBox.SelectionChangeCommitted -= this.comboBox_SelectionChangeCommitted;
                //comboBox.SelectionChangeCommitted += this.comboBox_SelectionChangeCommitted;
            }
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            Form legend = new Form();
            legend.Icon = BIDSHelper.Resources.Common.BIDSHelper;
            legend.Text = "Measure Group Health Check: Legend";
            legend.MaximizeBox = false;
            legend.MinimizeBox = false;

            TextBox text = new TextBox();
            text.Location = new System.Drawing.Point(2, 2);
            text.Size = new System.Drawing.Size(500, 380);
            text.Multiline = true;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Columns:");
            sb.AppendLine("Measure - The measure name");
            sb.AppendLine("Aggregate - The aggregate function for that measure");
            sb.AppendLine("Total - The aggregated value across all rows");
            sb.AppendLine("Min - The minimum value from a single row");
            sb.AppendLine("Max - The maximum value from a single row");
            sb.AppendLine("Decimals? - Are there any values that have decimals? More than 4 decimals?");
            sb.AppendLine("DSV - The datatype for the column in the data source view");
            sb.AppendLine("Current - The current measure datatype");
            sb.AppendLine("New - The recommended new measure datatype");
            sb.AppendLine();
            sb.AppendLine("Datatypes:");
            foreach (MeasureGroupHealthCheckPlugin.MeasureDataTypeOption dataType in MeasureGroupHealthCheckPlugin.dataTypeOptions)
            {
                sb.Append(dataType.dataType.ToString()).Append("/").Append(dataType.type.Name);
                sb.Append(" (").Append(dataType.displayMin).Append(" to ").Append(dataType.displayMax).Append(") ").AppendLine(dataType.limitations);
            }
            text.Text = sb.ToString();
            text.ReadOnly = true;
            text.Select(0, 0);
            text.BorderStyle = BorderStyle.None;
            text.PerformLayout();

            legend.Controls.Add(text);
            legend.Width = text.Width + 4;
            legend.Height = text.Height + 4;
            legend.MinimumSize = legend.Size;
            legend.MaximumSize = legend.Size;
            legend.SizeGripStyle = SizeGripStyle.Hide;
            legend.ShowInTaskbar = false;
            legend.StartPosition = FormStartPosition.CenterParent;
            legend.FormBorderStyle = FormBorderStyle.Fixed3D;
            legend.PerformLayout();
            legend.ShowDialog();
            legend.Dispose();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            helpButton_Click(sender, e);
        }

        //void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        //{
        //    this.dataGridView1.EndEdit();
        //    FormatGrid();
        //    dataGridView1_EditingControlShowing(sender, e);
        //}

    }
}