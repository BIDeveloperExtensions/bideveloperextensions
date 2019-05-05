namespace BIDSHelper
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Xml;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Win32;
    using Core;

    [FeatureCategory(BIDSFeatureCategories.General)]
    public class VersionCheckPlugin : BIDSHelperPluginBase, IDisposable
    {
//#if SQL2016
//        private static string CURRENT_VERSION_URL = "https://bidshelper.svn.codeplex.com/svn/SetupScript/SQL2014CurrentReleaseVersion.xml"; //TODO
//        private const string REGISTRY_LAST_VERSION_CHECK_SETTING_NAME = "LastVersionCheck2014";
//        private const string REGISTRY_DISMISSED_VERSION_SETTING_NAME = "DismissedVersion2014";
//#elif SQL2014
//        private static string CURRENT_VERSION_URL = "https://bidshelper.svn.codeplex.com/svn/SetupScript/SQL2014CurrentReleaseVersion.xml";
//        private const string REGISTRY_LAST_VERSION_CHECK_SETTING_NAME = "LastVersionCheck2014";
//        private const string REGISTRY_DISMISSED_VERSION_SETTING_NAME = "DismissedVersion2014";
//#elif DENALI
//        private static string CURRENT_VERSION_URL = "https://bidshelper.svn.codeplex.com/svn/SetupScript/SQL2012CurrentReleaseVersion.xml";
//        private const string REGISTRY_LAST_VERSION_CHECK_SETTING_NAME = "LastVersionCheck2012";
//        private const string REGISTRY_DISMISSED_VERSION_SETTING_NAME = "DismissedVersion2012";
//#else
//        private static string CURRENT_VERSION_URL = "https://bidshelper.svn.codeplex.com/svn/SetupScript/SQL2005CurrentReleaseVersion.xml";
//        private const string REGISTRY_LAST_VERSION_CHECK_SETTING_NAME = "LastVersionCheck2005";
//        private const string REGISTRY_DISMISSED_VERSION_SETTING_NAME = "DismissedVersion2005";
//#endif

//        public static string BIDS_HELPER_RELEASE_URL = "https://bidshelper.codeplex.com/releases";
//        private const int CHECK_EVERY_DAYS = 7;
//        private const int CHECK_SECONDS_AFTER_STARTUP = 60;

        private BackgroundWorker worker = new BackgroundWorker();
        //private Core.VersionCheckNotificationForm versionCheckForm;

        public static VersionCheckPlugin Instance { get; private set; }

        /// <summary>
        /// The latest version from CodePlex. Use a class field to prevent repeat calls, this acts as a cache.
        /// </summary>
        //private string serverVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionCheckPlugin"/> class.
        /// </summary>
        /// <param name="con">The connect object.</param>
        /// <param name="appObject">The application object.</param>
        /// <param name="addinInstance">The add-in instance.</param>
        public VersionCheckPlugin(BIDSHelperPackage package) : base(package)
        {
            Instance = this;

            //if (this.Enabled && LastVersionCheck.AddDays(CHECK_EVERY_DAYS) < DateTime.Today)
            //{
            //    //create this form on the main thread
            //    versionCheckForm = new BIDSHelper.Core.VersionCheckNotificationForm(this);
            //    versionCheckForm.Show(); //will hide itself

            //    worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            //    worker.RunWorkerAsync();
            //}
        }

        //private void worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    System.Threading.Thread.Sleep(CHECK_SECONDS_AFTER_STARTUP * 1000); //give BIDS a little time to get started up so we don't impede work people are doing with this version check
        //    CheckVersion();
        //}

        //public void CheckVersion()
        //{
        //    try
        //    {
        //        if (!VersionIsLatest(LocalVersion, ServerVersion) && ServerVersion != DismissedVersion)
        //        {
        //            versionCheckForm.ShowNotification();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
        //    }
        //}

        //public DateTime LastVersionCheck
        //{
        //    get
        //    {
        //        DateTime dtReturnVal = DateTime.MinValue;
        //        RegistryKey rk = Registry.CurrentUser.OpenSubKey(this.PluginRegistryPath);
        //        if (rk != null)
        //        {
        //            DateTime.TryParse((string)rk.GetValue(REGISTRY_LAST_VERSION_CHECK_SETTING_NAME, DateTime.MinValue.ToShortDateString()), out dtReturnVal);
        //            rk.Close();
        //        }

        //        return dtReturnVal;
        //    }
        //    set
        //    {
        //        string path = BIDSHelperPackage.REGISTRY_BASE_PATH + "\\" + this.ShortName;
        //        RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(path, true);
        //        if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(path);
        //        settingKey.SetValue(REGISTRY_LAST_VERSION_CHECK_SETTING_NAME, value, RegistryValueKind.String);
        //        settingKey.Close();
        //    }
        //}

        //public string DismissedVersion
        //{
        //    get
        //    {
        //        string sReturnVal = string.Empty;
        //        RegistryKey rk = Registry.CurrentUser.OpenSubKey(this.PluginRegistryPath);
        //        if (rk != null)
        //        {
        //            sReturnVal = (string)rk.GetValue(REGISTRY_DISMISSED_VERSION_SETTING_NAME, string.Empty);
        //            rk.Close();
        //        }

        //        return sReturnVal;
        //    }
        //    set
        //    {
        //        string path = BIDSHelperPackage.REGISTRY_BASE_PATH + "\\" + this.ShortName;
        //        RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(path, true);
        //        if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(path);
        //        settingKey.SetValue(REGISTRY_DISMISSED_VERSION_SETTING_NAME, value, RegistryValueKind.String);
        //        settingKey.Close();
        //    }
        //}

        public static string LocalVersion
        {
            get
            {
                return typeof(VersionCheckPlugin).Assembly.GetName().Version.ToString();
            }
        }

        //public string ServerVersion
        //{
        //    get
        //    {
        //        if (this.serverVersion != null)
        //        {
        //            return this.serverVersion;
        //        }

        //        System.Net.WebClient http = new System.Net.WebClient();
        //        //http.Proxy = System.Net.WebProxy.GetDefaultProxy(); //works but is deprecated
        //        http.Proxy = System.Net.WebRequest.GetSystemWebProxy(); //inherits the Internet Explorer proxy settings. Should help this version check work behind a proxy server.
        //        MemoryStream ms;
        //        try
        //        {
        //            ms = new MemoryStream(http.DownloadData(new Uri(CURRENT_VERSION_URL)));
        //        }
        //        catch (System.Net.WebException wex)
        //        {
        //            if (wex.Status == System.Net.WebExceptionStatus.ProtocolError) // this catches a 407 - Proxy authentication required error
        //            {
        //                // try again, but using the current user's windows credentials
        //                http.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
        //                ms = new MemoryStream(http.DownloadData(new Uri(CURRENT_VERSION_URL)));
        //            }
        //            else
        //            { 
        //                throw; 
        //            }
        //        }
        //        XmlReader reader = XmlReader.Create(ms);
        //        XmlDocument doc = new XmlDocument();
        //        doc.Load(reader);
        //        this.serverVersion = doc.DocumentElement.SelectSingleNode("Version").InnerText;
        //        ms.Close();
        //        reader.Close();

        //        return this.serverVersion;
        //    }
        //}

        //public static bool VersionIsLatest(string sLocalVersion, string sServerVersion)
        //{
        //    string[] arrLocalVersion = sLocalVersion.Split('.');
        //    string[] arrServerVersion = sServerVersion.Split('.');

        //    for (int i = 0; i < Math.Max(arrLocalVersion.Length, arrServerVersion.Length); i++)
        //    {
        //        int iLocal = 0;
        //        if (arrLocalVersion.Length > i) iLocal = int.Parse(arrLocalVersion[i]);
        //        int iServer = 0;
        //        if (arrServerVersion.Length > i) iServer = int.Parse(arrServerVersion[i]);
        //        if (iLocal < iServer)
        //        {
        //            return false;
        //        }
        //        else if (iLocal > iServer)
        //        {
        //            return true;
        //        }
        //    }

        //    return true;
        //}

        //public static void OpenBidsHelperReleasePageInBrowser()
        //{
        //    OpenUrl(BIDS_HELPER_RELEASE_URL);
        //}

        public override string ShortName
        {
            get { return "VersionCheckPlugin"; }
        }

        //public override int Bitmap
        //{
        //    get { return 0; }
        //}

        public override string FeatureName
        {
            get { return "BIDS Helper Version Notification (deprecated)"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>s
        /// <value>The help page Url.</value>
        public override string HelpUrl
        {
            get { return base.GetCodePlexHelpUrl("Version Notification"); }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.General; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Get notified when a new BIDS Helper version is released. A balloon and icon will appear in the system tray, which will link to the download page for the new release."; }
        }


        public override void Exec()
        {
            // Nothing required
        }

        public override void OnDisable()
        {
            base.OnDisable();
            this.Dispose();
        }

        public void Dispose()
        {
            if (worker != null) worker.Dispose();
            //if (versionCheckForm != null) versionCheckForm.Dispose();

        }
    }

}

