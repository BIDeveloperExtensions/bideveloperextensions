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
    public partial class BIDSHelperOptionsVersionCheckPage : UserControl //, EnvDTE.IDTToolsOptionsPage
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

        //void IDTToolsOptionsPage.OnHelp()
        //{
        //    // Launch plug-in help
        //    OpenUrl(VersionCheckPlugin.VersionCheckPluginInstance.HelpUrl);
        //}


        #endregion
        public void Initialize()
        {
            // TODO - should we run the page load here?
        }

        private void BIDSHelperOptionsVersionCheckPage_Load(object sender, EventArgs e)
        {
            try
            {
                // Get title from assembly info, e.g. "BIDS Helper for SQL 2008"
                Assembly assembly = this.GetType().Assembly;
                AssemblyTitleAttribute attribute = (AssemblyTitleAttribute)AssemblyTitleAttribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
                this.lblTitle.Text = attribute.Title;

                // Conditionally select name, BIDS vs SSDBI - Retained as suspsect BI will be dropped shortly
                string bidsName = "SSDTBI";

                if (BIDSHelperPackage.AddInLoadException != null)
                {
                    this.lblBidsHelperLoadException.Text = string.Format("BIDS Helper encountered an error when Visual Studio started:\r\n{0}\r\n{1}"
                        , BIDSHelperPackage.AddInLoadException.Message
                        , BIDSHelperPackage.AddInLoadException.StackTrace);

                    Exception innerEx = BIDSHelperPackage.AddInLoadException.InnerException;
                    while (innerEx != null)
                    {
                        this.lblBidsHelperLoadException.Text += string.Format("\r\nInner exception:\r\n{0}\r\n{1}"
                        , innerEx.Message
                        , innerEx.StackTrace);
                        innerEx = innerEx.InnerException;
                    }

                    ReflectionTypeLoadException ex = BIDSHelperPackage.AddInLoadException as ReflectionTypeLoadException;
                    if (ex == null) ex = BIDSHelperPackage.AddInLoadException.InnerException as ReflectionTypeLoadException;
                    if (ex == null && BIDSHelperPackage.AddInLoadException.InnerException != null) ex = BIDSHelperPackage.AddInLoadException.InnerException.InnerException as ReflectionTypeLoadException;
                    if (ex != null)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        foreach (Exception exSub in ex.LoaderExceptions)
                        {
                            sb.AppendLine();
                            sb.AppendLine(exSub.Message);
                            System.IO.FileNotFoundException exFileNotFound = exSub as System.IO.FileNotFoundException;
                            if (exFileNotFound != null)
                            {
                                if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                                {
                                    sb.AppendLine("Fusion Log:");
                                    sb.AppendLine(exFileNotFound.FusionLog);
                                }
                            }
                            sb.AppendLine();
                        }
                        this.lblBidsHelperLoadException.Text += sb.ToString();
                    }

                    this.lblBidsHelperLoadException.Visible = true;
                    this.btnCopyError.Visible = true;
                }
                else
                {
                    this.lblBidsHelperLoadException.Visible = false;
                    this.btnCopyError.Visible = false;
                }

                try
                {
                    this.lblSqlVersion.Text = string.Format("{0} {1} ({2}) for Visual Studio {3} was detected", bidsName, VersionInfo.SqlServerFriendlyVersion, VersionInfo.SqlServerVersion, VersionInfo.VisualStudioFriendlyVersion);
                }
                catch
                {
                    //if there's an exception it's because we couldn't find SSDTBI or BIDS installed in this Visual Studio version
                    try
                    {
                        this.lblSqlVersion.Text = bidsName + " for Visual Studio " + VersionInfo.VisualStudioFriendlyVersion + " was NOT detected. BIDS Helper disabled.";
                        this.lblSqlVersion.ForeColor = System.Drawing.Color.Red;
                        if (BIDSHelperPackage.AddInLoadException != null && BIDSHelperPackage.AddInLoadException is System.Reflection.ReflectionTypeLoadException)
                        {
                            //this is the expected exception if SSDTBI isn't installed... if this is the exception, don't show it... otherwise, show the exception
                            this.lblBidsHelperLoadException.Visible = false;
                        }
                    }
                    catch
                    {
                        this.lblSqlVersion.Visible = false;
                    }
                }

                // Set current version
                this.lblLocalVersion.Text = VersionCheckPlugin.LocalVersion;
#if DEBUG
                DateTime buildDateTime = GetBuildDateTime(assembly);
                this.lblLocalVersion.Text += string.Format(CultureInfo.InvariantCulture, " (Debug Build {0:yyyy-MM-dd HH:mm:ss})", buildDateTime);
#endif

                this.lblSqlVersion.Text += string.Format("\r\nSSDT Extensions Installed: SSAS ({0}), SSIS ({1}), SSRS ({2})",
                     (BIDSHelperPackage.SSASExtensionVersion == null ? "N/A" : BIDSHelperPackage.SSASExtensionVersion.ToString()),
                     (BIDSHelperPackage.SSISExtensionVersion == null ? "N/A" : BIDSHelperPackage.SSISExtensionVersion.ToString()),
                     (BIDSHelperPackage.SSRSExtensionVersion == null ? "N/A" : BIDSHelperPackage.SSRSExtensionVersion.ToString()));

                //the current BI Developer Extensions version is compatible with the following versions or higher of SSDT extensions
                Version SSASExpectedVersion = new Version("2.8.15");
                Version SSRSExpectedVersion = new Version("2.5.9");
                Version SSISExpectedVersion = new Version("2.1");

                string sUpgradeSSDTMessage = string.Empty;
                if (BIDSHelperPackage.SSASExtensionVersion != null && BIDSHelperPackage.SSASExtensionVersion < SSASExpectedVersion)
                {
                    if (sUpgradeSSDTMessage != string.Empty) sUpgradeSSDTMessage += ", ";
                    sUpgradeSSDTMessage += "SSAS to " + SSASExpectedVersion;
                }
                if (BIDSHelperPackage.SSRSExtensionVersion != null && BIDSHelperPackage.SSRSExtensionVersion < SSRSExpectedVersion)
                {
                    if (sUpgradeSSDTMessage != string.Empty) sUpgradeSSDTMessage += ", ";
                    sUpgradeSSDTMessage += "SSRS to " + SSRSExpectedVersion;
                }
                if (BIDSHelperPackage.SSISExtensionVersion != null && BIDSHelperPackage.SSISExtensionVersion < SSISExpectedVersion)
                {
                    if (sUpgradeSSDTMessage != string.Empty) sUpgradeSSDTMessage += ", ";
                    sUpgradeSSDTMessage += "SSIS to " + SSISExpectedVersion;
                }
                if (sUpgradeSSDTMessage != string.Empty)
                    this.lblSqlVersion.Text += "\r\n" + "Please upgrade " + sUpgradeSSDTMessage;

                // First check we have a valid instance, the add-in may be disabled.
                if (VersionCheckPlugin.Instance == null)
                {
                    bool bConnected = false;
                    try
                    {
                        // TODO - will this code even run now that we are in an extension if the extension 
                        // is not loaded
                        //foreach (EnvDTE.AddIn addin in Connect.Application.AddIns)
                        //{
                        //    if (addin.ProgID.ToLower() == "BIDSHelper.Connect".ToLower())
                        //    {
                        //        if (addin.Connected)
                        //        {
                                    bConnected = true;
                            //        break;
                            //    }
                            //}
                        //}
                    }
                    catch { }

                    if (bConnected)
                    {
                        // Display disabled information and exit
                        if (BIDSHelperPackage.AddInLoadException == null)
                            lblServerVersion.Text = "The BIDS Helper Add-in is not running because of problems loading!";
                        else
                            lblServerVersion.Visible = false;
                        linkNewVersion.Visible = false;
                    }
                    else
                    {
                        // Display disabled information and exit
                        lblLocalVersion.Text += " [Add-in Disabled]";
                        lblServerVersion.Text = "The BIDS Helper Add-in is not currently enabled.";
                        linkNewVersion.Visible = false;
                    }
                    
                    
                    return; //if we don't have the version check plugin loaded, then stop now and don't check version on server
                }


                lblServerVersion.Visible = false;
                linkNewVersion.Visible = false;
                //try
                //{
                //    //VersionCheckPlugin.Instance.LastVersionCheck = DateTime.Today;
                //    //if (!VersionCheckPlugin.VersionIsLatest(VersionCheckPlugin.LocalVersion, VersionCheckPlugin.Instance.ServerVersion))
                //    //{
                //    //    lblServerVersion.Text = "Version " + VersionCheckPlugin.Instance.ServerVersion + " is available...";
                //    //    lblServerVersion.Visible = true;
                //    //    linkNewVersion.Visible = true;
                //    //}
                //    //else
                //    //{
                //    //    lblServerVersion.Text = "BIDS Helper is up to date.";
                //    //    lblServerVersion.Visible = true;
                //    //    linkNewVersion.Visible = false;
                //    //}
                //}
                //catch (Exception ex)
                //{
                //    lblServerVersion.Text = "Unable to retrieve current available BIDS Helper version from Codeplex: " + ex.Message + "\r\n" + ex.StackTrace;
                //    linkNewVersion.Visible = false;
                //}
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
            //OpenUrl(VersionCheckPlugin.BIDS_HELPER_RELEASE_URL);
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
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, DefaultMessageBoxCaption);
            }
        }

        private void btnCopyError_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.lblBidsHelperLoadException.Text);
        }

#if DEBUG
        // Gets the build date and time (by reading the COFF header)
        // http://stackoverflow.com/questions/1600962/displaying-the-build-date
        // http://msdn.microsoft.com/en-us/library/ms680313
#pragma warning disable 169
#pragma warning disable 649
        struct IMAGE_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;

            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        };
#pragma warning restore 649
#pragma warning restore 169

        private static DateTime GetBuildDateTime(Assembly assembly)
        {
            if (System.IO.File.Exists(assembly.Location))
            {
                var buffer = new byte[Math.Max(System.Runtime.InteropServices.Marshal.SizeOf(typeof(IMAGE_FILE_HEADER)), 4)];
                using (var fileStream = new System.IO.FileStream(assembly.Location, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    fileStream.Position = 0x3C;
                    fileStream.Read(buffer, 0, 4);
                    fileStream.Position = BitConverter.ToUInt32(buffer, 0); // COFF header offset
                    fileStream.Read(buffer, 0, 4); // "PE\0\0"
                    fileStream.Read(buffer, 0, buffer.Length);
                }
                var pinnedBuffer = System.Runtime.InteropServices.GCHandle.Alloc(buffer, System.Runtime.InteropServices.GCHandleType.Pinned);
                try
                {
                    var coffHeader = (IMAGE_FILE_HEADER)System.Runtime.InteropServices.Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(IMAGE_FILE_HEADER));

                    return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1) + new TimeSpan(coffHeader.TimeDateStamp * TimeSpan.TicksPerSecond));
                }
                finally
                {
                    pinnedBuffer.Free();
                }
            }
            return new DateTime();
        }
#endif
    }
}
