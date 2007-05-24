using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper
{
    public partial class ReportViewerForm : Form
    {
        public ReportViewerForm()
        {
            InitializeComponent();
        }

        public string Report;
        public string Caption;

        private void PrinterFriendlyDimensionUsage_Load(object sender, EventArgs e)
        {
            this.ReportViewerControl.ProcessingMode = Microsoft.Reporting.WinForms.ProcessingMode.Local;
            this.ReportViewerControl.LocalReport.ReportEmbeddedResource = "BIDSHelper." + Report;
            //TODO - set the form caption, either via a property or by parsing the report name
            this.Text = Caption;
            this.ReportViewerControl.RefreshReport();
        }


    }
}