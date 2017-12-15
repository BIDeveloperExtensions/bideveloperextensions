using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;


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


            //prevents a crash or error in VS2015
            PermissionSet permissions = new PermissionSet(PermissionState.Unrestricted); 
            this.ReportViewerControl.LocalReport.SetBasePermissionsForSandboxAppDomain(permissions);

            AssemblyName asm_name = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            this.ReportViewerControl.LocalReport.AddFullTrustModuleInSandboxAppDomain(new StrongName(new StrongNamePublicKeyBlob(asm_name.GetPublicKeyToken()), asm_name.Name, asm_name.Version));


            //TODO - set the form caption, either via a property or by parsing the report name if the caption hasn't been set
            this.Text = Caption;

            if (Parameters.Count > 0)
            {
                this.ReportViewerControl.LocalReport.SetParameters(Parameters);
            }

            this.ReportViewerControl.RefreshReport();
        }




        private void ReportViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //from https://connect.microsoft.com/VisualStudio/feedback/details/522208/wpf-app-with-reportviewer-gets-error-while-unloading-appdomain-exception-on-termination
                this.ReportViewerControl.LocalReport.ReleaseSandboxAppDomain();
            }
            catch { }
        }
        
    }

    
}