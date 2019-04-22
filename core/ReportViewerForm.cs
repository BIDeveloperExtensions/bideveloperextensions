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
            try
            {
                
                this.ReportViewerControl.ProcessingMode = Microsoft.Reporting.WinForms.ProcessingMode.Local;
                this.ReportViewerControl.LocalReport.ReportEmbeddedResource = "BIDSHelper." + Report;


                //prevents a crash or error in VS2015
                PermissionSet permissions = new PermissionSet(PermissionState.Unrestricted);
                this.ReportViewerControl.LocalReport.SetBasePermissionsForSandboxAppDomain(permissions);

                AssemblyName asm_name = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                this.ReportViewerControl.LocalReport.AddFullTrustModuleInSandboxAppDomain(new StrongName(new StrongNamePublicKeyBlob(asm_name.GetPublicKeyToken()), asm_name.Name, asm_name.Version));


                EnsureReportViewerAppDomainSetup();


                //TODO - set the form caption, either via a property or by parsing the report name if the caption hasn't been set
                this.Text = Caption;

                if (Parameters.Count > 0)
                {
                    this.ReportViewerControl.LocalReport.SetParameters(Parameters);
                }

                this.ReportViewerControl.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                MessageBox.Show(BIDSHelperPackage.FormatLoaderException(ex));
            }

        }

        /// <summary>
        /// By default ReportViewer will launch a new AppDomain with the ApplicationBase path set to be the Visual Studio path.
        /// This works when they have the SSAS extension installed which includes ReportViewer.
        /// But when ReportViewer isn't in that directory the report fails. 
        /// This function tweaks some private members of ReportViewer so that the AppDomain spins up with the 
        /// BI Developer Extensions install folder as the ApplicationBase for the new AppDomain. We are now installing
        /// the necessary ReportViewer DLLs in the BI Developer Extensions installer so that these reports work regardless of what extensions are installed.
        /// </summary>
        private void EnsureReportViewerAppDomainSetup()
        {
            try
            {
                var bidsHelperPath = new System.IO.FileInfo(typeof(BIDSHelperPackage).Assembly.Location);

                object processingHost = this.ReportViewerControl.LocalReport.GetType().InvokeMember("m_processingHost", BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, this.ReportViewerControl.LocalReport, null);
                Type typeLocalService = BIDSHelper.Core.BIDSHelperPluginBase.GetPrivateType(processingHost.GetType(), "Microsoft.Reporting.LocalService");
                object reportRuntimeSetupHandler = typeLocalService.InvokeMember("m_reportRuntimeSetupHandler", BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, processingHost, null);
                object m_appDomainPool = reportRuntimeSetupHandler.GetType().InvokeMember("m_appDomainPool", BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Static, null, reportRuntimeSetupHandler, null);
                AppDomainSetup setup = (AppDomainSetup)m_appDomainPool.GetType().InvokeMember("m_setupInfo", BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, m_appDomainPool, null);
                setup.ApplicationBase = bidsHelperPath.DirectoryName + "\\";

                //reportRuntimeSetupHandler.GetType().InvokeMember("EnsureSandboxAppDomainIfNeeded", BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, reportRuntimeSetupHandler, null);

                //object exprHostSandboxAppDomain = reportRuntimeSetupHandler.GetType().InvokeMember("m_exprHostSandboxAppDomain", BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, reportRuntimeSetupHandler, null);
                //AppDomain sandboxAppDomain = (AppDomain)exprHostSandboxAppDomain.GetType().InvokeMember("AppDomain", BindingFlags.FlattenHierarchy | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, exprHostSandboxAppDomain, null);
                //string path = sandboxAppDomain.SetupInformation.PrivateBinPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem in ReportViewerForm.EnsureReportViewerAppDomainSetup: " + ex.Message + "\r\n" + ex.StackTrace);
            }
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