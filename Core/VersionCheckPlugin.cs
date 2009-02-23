using System;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
//using System.Runtime.InteropServices;

namespace BIDSHelper
{
    public class VersionCheckPlugin : BIDSHelperPluginBase
    {

#if KATMAI
        private static string CURRENT_VERSION_URL = "https://bidshelper.svn.codeplex.com/svn/SetupScript/SQL2008CurrentReleaseVersion.xml";
        private const string REGISTRY_LAST_VERSION_CHECK_SETTING_NAME = "LastVersionCheck2008";
        private const string REGISTRY_DISMISSED_VERSION_SETTING_NAME = "DismissedVersion2008";
#else
        private static string CURRENT_VERSION_URL = "https://bidshelper.svn.codeplex.com/svn/SetupScript/SQL2005CurrentReleaseVersion.xml";
        private const string REGISTRY_LAST_VERSION_CHECK_SETTING_NAME = "LastVersionCheck2005";
        private const string REGISTRY_DISMISSED_VERSION_SETTING_NAME = "DismissedVersion2005";
#endif

        public static string BIDS_HELPER_RELEASE_URL = "http://www.codeplex.com/bidshelper/Release/ProjectReleases.aspx";
        private const int CHECK_EVERY_DAYS = 7;
        private const int CHECK_SECONDS_AFTER_STARTUP = 60;

        private BackgroundWorker worker = new BackgroundWorker();
        private Core.VersionCheckNotificationForm versionCheckForm;

        public static VersionCheckPlugin VersionCheckPluginInstance;

        public VersionCheckPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
            VersionCheckPluginInstance = this;

            if (this.Enabled && LastVersionCheck.AddDays(CHECK_EVERY_DAYS) < DateTime.Today)
            {
                //create this form on the main thread
                versionCheckForm = new BIDSHelper.Core.VersionCheckNotificationForm(this);
                versionCheckForm.Show(); //will hide itself

                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerAsync();
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(CHECK_SECONDS_AFTER_STARTUP * 1000); //give BIDS a little time to get started up so we don't impede work people are doing with this version check
            CheckVersion();
        }

        public void CheckVersion()
        {
            try
            {
                if (!VersionIsLatest(LocalVersion, ServerVersion) && ServerVersion != DismissedVersion)
                {
                    versionCheckForm.ShowNotification();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public DateTime LastVersionCheck
        {
            get
            {
                DateTime dtReturnVal = DateTime.MinValue;
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(this.PluginRegistryPath);
                if (rk != null)
                {
                    DateTime.TryParse((string)rk.GetValue(REGISTRY_LAST_VERSION_CHECK_SETTING_NAME, DateTime.MinValue.ToShortDateString()), out dtReturnVal);
                    rk.Close();
                }
                return dtReturnVal;
            }
            set
            {
                string path = Connect.REGISTRY_BASE_PATH + "\\" + this.ShortName;
                RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(path, true);
                if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(path);
                settingKey.SetValue(REGISTRY_LAST_VERSION_CHECK_SETTING_NAME, value, RegistryValueKind.String);
                settingKey.Close();
            }
        }

        public string DismissedVersion
        {
            get
            {
                string sReturnVal = string.Empty;
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(this.PluginRegistryPath);
                if (rk != null)
                {
                    sReturnVal = (string)rk.GetValue(REGISTRY_DISMISSED_VERSION_SETTING_NAME, string.Empty);
                    rk.Close();
                }
                return sReturnVal;
            }
            set
            {
                string path = Connect.REGISTRY_BASE_PATH + "\\" + this.ShortName;
                RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(path, true);
                if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(path);
                settingKey.SetValue(REGISTRY_DISMISSED_VERSION_SETTING_NAME, value, RegistryValueKind.String);
                settingKey.Close();
            }
        }

        public string LocalVersion
        {
            get
            {
                return this.GetType().Assembly.GetName().Version.ToString();
            }
        }

        private string _serverVersion;
        public string ServerVersion
        {
            get
            {
                if (_serverVersion != null)
                    return _serverVersion;

                System.Net.WebClient http = new System.Net.WebClient();
                MemoryStream ms = new MemoryStream(http.DownloadData(new System.Uri(CURRENT_VERSION_URL)));
                XmlReader reader = XmlReader.Create(ms);
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                _serverVersion = doc.DocumentElement.SelectSingleNode("Version").InnerText;
                ms.Close();
                reader.Close();
                return _serverVersion;
            }
        }

        public static bool VersionIsLatest(string sLocalVersion, string sServerVersion)
        {
            string[] arrLocalVersion = sLocalVersion.Split('.');
            string[] arrServerVersion = sServerVersion.Split('.');

            for (int i = 0; i < Math.Max(arrLocalVersion.Length, arrServerVersion.Length); i++)
            {
                int iLocal = 0;
                if (arrLocalVersion.Length > i) iLocal = int.Parse(arrLocalVersion[i]);
                int iServer = 0;
                if (arrServerVersion.Length > i) iServer = int.Parse(arrServerVersion[i]);
                if (iLocal < iServer)
                {
                    return false;
                }
                else if (iLocal > iServer)
                {
                    return true;
                }
            }

            return true;
        }

        public static void OpenBidsHelperReleasePageInBrowser()
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = "iexplore.exe";
                process.StartInfo.Arguments = BIDS_HELPER_RELEASE_URL;
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public override string ShortName
        {
            get { return "VersionCheckPlugin"; }
        }

        public override string FriendlyName
        {
            get { return "BIDS Helper Version Notification"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return FriendlyName; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }


        public override string MenuName
        {
            get { return string.Empty; }
        }

        public override bool DisplayCommand(UIHierarchyItem item)
        {
            return false;
        }

        public override void Exec()
        {
        }

    }

}

