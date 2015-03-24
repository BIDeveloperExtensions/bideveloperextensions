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

#if DENALI || SQL2014
                string sBIDSName = "SSDTBI";
#else
                string sBIDSName = "BIDS";
#endif

                if (Connect.AddInLoadException != null)
                {
                    this.lblBidsHelperLoadException.Text = "BIDS Helper encountered an error when Visual Studio started:\r\n" + Connect.AddInLoadException.Message + "\r\n" + Connect.AddInLoadException.StackTrace;

                    ReflectionTypeLoadException ex = Connect.AddInLoadException as ReflectionTypeLoadException;
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
                    this.lblSqlVersion.Text = sBIDSName + " " + GetFriendlySqlVersion() + " for Visual Studio " + GetFriendlyVisualStudioVersion() + " was detected";
                }
                catch
                {
                    //if there's an exception it's because we couldn't find SSDTBI or BIDS installed in this Visual Studio version
                    try
                    {
                        this.lblSqlVersion.Text = sBIDSName + " for Visual Studio " + GetFriendlyVisualStudioVersion() + " was NOT detected. BIDS Helper disabled.";
                        this.lblSqlVersion.ForeColor = System.Drawing.Color.Red;
                        if (Connect.AddInLoadException != null && Connect.AddInLoadException is System.Reflection.ReflectionTypeLoadException)
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

                // First check we have a valid instance, the add-in may be disabled.
                if (VersionCheckPlugin.VersionCheckPluginInstance == null)
                {
                    bool bConnected = false;
                    try
                    {
                        foreach (EnvDTE.AddIn addin in Connect.Application.AddIns)
                        {
                            if (addin.ProgID.ToLower() == "BIDSHelper.Connect".ToLower())
                            {
                                if (addin.Connected)
                                {
                                    bConnected = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }

                    if (bConnected)
                    {
                        // Display disabled information and exit
                        if (Connect.AddInLoadException == null)
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


                try
                {
                    VersionCheckPlugin.VersionCheckPluginInstance.LastVersionCheck = DateTime.Today;
                    if (!VersionCheckPlugin.VersionIsLatest(VersionCheckPlugin.LocalVersion, VersionCheckPlugin.VersionCheckPluginInstance.ServerVersion))
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

        private string GetFriendlyVisualStudioVersion()
        {
            if (Connect.Application.Version.StartsWith("8.")) //YUKON runs here
                return "2005";
            else if (Connect.Application.Version.StartsWith("9.")) //KATMAI runs here
                return "2008";
            else if (Connect.Application.Version.StartsWith("10.")) //DENALI runs here
                return "2010";
            else if (Connect.Application.Version.StartsWith("11.")) //DENALI runs here
                return "2012";
            else if (Connect.Application.Version.StartsWith("12.")) //SQL2014 runs here
                return "2013";
            else
                return Connect.Application.Version; //todo in future
        }

        private string GetFriendlySqlVersion()
        {
            Assembly assemb = System.Reflection.Assembly.Load("Microsoft.DataWarehouse"); //get a sample assembly that's installed with BIDS and use that to detect if BIDS is installed
            string sVersion = assemb.GetName().Version.ToString();

            try
            {
                //if it's a SQL2008 R2 release, you need to get the informational version attribute
                //SQL2005 didn't have this attribute
                AssemblyInformationalVersionAttribute attributeVersion = (AssemblyInformationalVersionAttribute)AssemblyTitleAttribute.GetCustomAttribute(assemb, typeof(AssemblyInformationalVersionAttribute));
                if (attributeVersion != null) sVersion = attributeVersion.InformationalVersion;
            }
            catch { }

            if (sVersion.StartsWith("9."))
                return "2005";
            else if (sVersion.StartsWith("10.5"))
                return "2008 R2";
            else if (sVersion.StartsWith("10."))
                return "2008";
            else if (sVersion.StartsWith("11."))
                return "2012";
            else if (sVersion.StartsWith("12."))
                return "2014";
            else
                return sVersion; //todo in future post DENALI and SQL2014
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
