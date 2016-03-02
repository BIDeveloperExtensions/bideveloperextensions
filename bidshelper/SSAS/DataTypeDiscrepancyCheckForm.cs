using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper.SSAS
{
    public partial class DataTypeDiscrepancyCheckForm : Form
    {
        public DataTypeDiscrepancyCheckForm()
        {
            InitializeComponent();
            this.Icon = BIDSHelper.Resources.Common.BIDSHelper;
        }

        private void DataTypeDiscrepancyCheckForm_Load(object sender, EventArgs e)
        {
            FormatGrid();
        }

        private void FormatGrid()
        {
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                DataTypeDiscrepancyCheckPlugin.DataTypeDiscrepancy item = (DataTypeDiscrepancyCheckPlugin.DataTypeDiscrepancy)row.DataBoundItem;
                if (item.MayRequireDimensionUsageChanges)
                {
                    Color newColor = Color.Red;
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.ForeColor = newColor;
                    }
                }
            }
        }
    }
}