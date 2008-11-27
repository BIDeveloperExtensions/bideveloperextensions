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
        public List<Microsoft.Reporting.WinForms.ReportParameter> Parameters = new List<Microsoft.Reporting.WinForms.ReportParameter>();

        private void PrinterFriendlyDimensionUsage_Load(object sender, EventArgs e)
        {
            this.ReportViewerControl.ProcessingMode = Microsoft.Reporting.WinForms.ProcessingMode.Local;
            this.ReportViewerControl.LocalReport.ReportEmbeddedResource = "BIDSHelper." + Report;
            //TODO - set the form caption, either via a property or by parsing the report name if the caption hasn't been set
            this.Text = Caption;

            if (Parameters.Count > 0)
            {
                this.ReportViewerControl.LocalReport.SetParameters(Parameters);
            }

            this.ReportViewerControl.RefreshReport();
        }


    }
}