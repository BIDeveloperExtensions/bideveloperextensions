namespace BIDSHelper.Core
{
    using System;
    using System.Windows.Forms;
    using EnvDTE;
    using System.Reflection;
    using System.Globalization;

    /// <summary>
    /// Version Check dialog for BIDS Helper, as seen under the Visual Studio menu Tools -> Options.
    /// </summary>
    public partial class BIDSHelperOptionsVersionCheckPage : UserControl, EnvDTE.IDTToolsOptionsPage
    {
        /// <summary>
        /// Standard caption for message boxes shown by this options page.
        /// </summary>
        private static string DefaultMessageBoxCaption = "BIDS Helper Version Check Options";

        /// <summary>
        /// Initializes a new instance of the <see cref="BIDSHelperOptionsVersionCheckPage"/> class.
        /// </summary>
        public BIDSHelperOptionsVersionCheckPage()
        {
            InitializeComponent();
        }

        #region IDTToolsOptionsPage Members
        //Property can be accessed through the object model with code such as the following macro:
        //    Sub ToolsOptionsPageProperties()
        //        MsgBox(DTE.Properties("My Category", "My Subcategory - Visual C#").Item("MyProperty").Value)
        //        DTE.Properties("My Category", "My Subcategory - Visual C#").Item("MyProperty").Value = False
        //        MsgBox(DTE.Properties("My Category", "My Subcategory - Visual C#").Item("MyProperty").Value)
        //    End Sub
        void IDTToolsOptionsPage.GetProperties(ref object PropertiesObject)
        {
            throw new NotImplementedException();
        }

        void IDTToolsOptionsPage.OnAfterCreated(DTE DTEObject)
        {
            // Nothing required
        }

        void IDTToolsOptionsPage.OnCancel()
        {
            // Nothing required
        }

        void IDTToolsOptionsPage.OnHelp()
        {
            // Launch plug-in help
            OpenUrl(VersionCheckPlugin.VersionCheckPluginInstance.HelpUrl);
        }

        void IDTToolsOptionsPage.OnOK()
        {
            // Nothing required
        }
        #endregion

        private void BIDSHelperOptionsVersionCheckPage_Load(object sender, EventArgs e)
        {
            try
            {
                // Get title from assembly info, e.g. "BIDS Helper for SQL 2008"
                Assembly assembly = this.GetType().Assembly;
                AssemblyTitleAttribute attribute = (AssemblyTitleAttribute)AssemblyTitleAttribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
                this.lblTitle.Text = attribute.Title;

                // First check we have a valid instance, the add-in may be disabled.
                if (VersionCheckPlugin.VersionCheckPluginInstance == null)
                {
                    // Display disabled information and exit
                    lblLocalVersion.Text = "[Add-in Disabled]";
                    lblServerVersion.Text = "The BIDS Helper Add-in is not currently enabled.";
                    linkNewVersion.Visible = false;
                    return;
                }

                // Set current version
                this.lblLocalVersion.Text = VersionCheckPlugin.VersionCheckPluginInstance.LocalVersion;
#if DEBUG
                this.lblLocalVersion.Text += string.Format(CultureInfo.InvariantCulture, " (Debug {0})", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
#endif

                try
                {
                    VersionCheckPlugin.VersionCheckPluginInstance.LastVersionCheck = DateTime.Today;
                    if (!VersionCheckPlugin.VersionIsLatest(VersionCheckPlugin.VersionCheckPluginInstance.LocalVersion, VersionCheckPlugin.VersionCheckPluginInstance.ServerVersion))
                    {
                        lblServerVersion.Text = "Version " + VersionCheckPlugin.VersionCheckPluginInstance.ServerVersion + " is available...";
                        lblServerVersion.Visible = true;
                        linkNewVersion.Visible = true;
                    }
                    else
                    {
                        lblServerVersion.Text = "BIDS Helper is up to date.";
                        lblServerVersion.Visible = true;
                        linkNewVersion.Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    lblServerVersion.Text = "Unable to retrieve current available BIDS Helper version from Codeplex: " + ex.Message + "\r\n" + ex.StackTrace;
                    linkNewVersion.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, DefaultMessageBoxCaption);
            }
        }

        /// <summary>
        /// Handles the LinkClicked event of the linkLabelCodePlexUrl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
        private void linkLabelCodePlexUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl(linkLabelCodePlexUrl.Text);
        }

        /// <summary>
        /// Handles the LinkClicked event of the linkNewVersion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
        private void linkNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl(VersionCheckPlugin.BIDS_HELPER_RELEASE_URL);
        }

        /// <summary>
        /// Opens a URL.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = "iexplore.exe";
                process.StartInfo.Arguments = url;
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, DefaultMessageBoxCaption);
            }
        }
    }
}
