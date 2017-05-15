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
            PermissionSet permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            this.ReportViewerControl.LocalReport.SetBasePermissionsForSandboxAppDomain(permissions);

            AssemblyName asm_name = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            this.ReportViewerControl.LocalReport.AddFullTrustModuleInSandboxAppDomain(new StrongName(new StrongNamePublicKeyBlob(asm_name.GetPublicKeyToken()), asm_name.Name, asm_name.Version));


            //TODO - set the form caption, either via a property or by parsing the report name if the caption hasn't been set
            this.Text = Caption;

            if (Parameters.Count > 0)
            {
                this.ReportViewerControl.LocalReport.SetParameters(Parameters);
            }

            this.ReportViewerControl.ReportError += ReportViewerControl_ReportError;


            //this.ReportViewerControl.LocalReport.ExecuteReportInCurrentAppDomain(AppDomain.CurrentDomain.Evidence);
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

        private void ReportViewerControl_ReportError(object sender, Microsoft.Reporting.WinForms.ReportErrorEventArgs e)
        {
            MessageBox.Show("error!");
        }
    }



    public class RDLCLocalReportProxy : MarshalByRefObject
    {
        //<remark> The LocalReport has confilct with xbap domain 
        //then we create another domain just for load localreport
        static AppDomain rdlcLocalReportDomain;
        private static AppDomain RDLCLocalReportDomain
        {
            get
            {
                if (rdlcLocalReportDomain == null)
                {
                    var setup = new AppDomainSetup();
                    setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    setup.PrivateBinPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    rdlcLocalReportDomain = AppDomain.CreateDomain("BIDSHelperReportSandbox" + Guid.NewGuid().ToString().GetHashCode().ToString("x"), null, setup);
                    //rdlcLocalReportDomain = AppDomainTools.CreateDomain("rdlcLocalReportDomain");
                    rdlcLocalReportDomain.AssemblyResolve += new ResolveEventHandler(rdlcLocalReportDomain_AssemblyResolve);
                }
                return rdlcLocalReportDomain;
            }
        }

        private static System.Reflection.Assembly rdlcLocalReportDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (System.Reflection.Assembly ____ in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (
                        string.Compare(____.GetName().Name, args.Name, true) == 0 ||
                        string.Compare(____.FullName, args.Name, true) == 0
                    )
                    return ____;
            }
            return null;
        }
        public static ReportViewerForm GetReportViewerForm()
        {
            var proxy = (RDLCLocalReportProxy)RDLCLocalReportDomain.CreateInstanceAndUnwrap(typeof(RDLCLocalReportProxy).Assembly.FullName, typeof(RDLCLocalReportProxy).FullName);
            return proxy.GetReportViewerFormPrivate();
        }

        private ReportViewerForm GetReportViewerFormPrivate()
        {
            return new ReportViewerForm();
        }

        //DataSet dsReportData;
        //public void ShowRDLCReport(DataSet dsReportData, string reportFilePath, string mainReportName)//Stream rdlcReportDefstream)
        //{
        //    ReportViewerForm rptViewerForm = RDLCReportViewerFormProvider.Create();// create report viewer in current domain("rdlcLocalReportDomain")
        //    GenerateLocalReport(dsReportData, reportFilePath, mainReportName,
        //        rptViewerForm.ReportViewer.LocalReport);

        //    rptViewerForm.ReportViewer.RefreshReport();
        //    rptViewerForm.Open();//show report viewer in current domain("rdlcLocalReportDomain")

        //}
    }
}