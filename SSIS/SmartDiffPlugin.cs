using System;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using System.Xml.Xsl;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using BIDSHelper.Core;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.General)]
    public class SmartDiffPlugin : BIDSHelperPluginBase
    {
        private static string _VisualStudioRegistryPath;
        private static System.Collections.Generic.Dictionary<string, string> _dictPasswords = new System.Collections.Generic.Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        private const string REGISTRY_CUSTOM_DIFF_VIEWER_SETTING_NAME = "CustomDiffViewer";

        public SmartDiffPlugin(BIDSHelperPackage package)
            : base(package)
        {
            // TODO - should we get this property without using the ApplicationObject
            _VisualStudioRegistryPath = this.ApplicationObject.RegistryRoot;
            CreateContextMenu(CommandList.SmartDiffId, BI_FILE_EXTENSIONS);
        }

        public override string ShortName
        {
            get { return "SmartDiff"; }
        }

        //public override int Bitmap
        //{
        //    get { return 1836; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Smart Diff..."; }
        //}

        public override string FeatureName
        {
            get { return "Smart Diff"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Compare differences between two versions of a file, including those in source control. Works across the BI stack including Packages, Cubes, Dimensions, Data Sources and Reports."; }
        }


        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.General; }
        }

        private string[] DTS_FILE_EXTENSIONS = { ".dtsx" };
        private string[] SSAS_FILE_EXTENSIONS = { ".dim", ".cube", ".dmm", ".dsv", ".bim" };
        private string[] SSRS_FILE_EXTENSIONS = { ".rdl", ".rdlc" };

        private string[] BI_FILE_EXTENSIONS = { ".dtsx", ".dim", ".cube", ".dmm", ".dsv", ".bim", ".rdl", ".rdlc" };


        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        //public override bool DisplayCommand(UIHierarchyItem item)
        //{
        //    try
        //    {
        //        UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
        //        if (((System.Array)solExplorer.SelectedItems).Length != 1)
        //            return false;

        //        UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
        //        string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
        //        foreach (string extension in DTS_FILE_EXTENSIONS)
        //        {
        //            if (sFileName.EndsWith(extension))
        //                return true;
        //        }
        //        foreach (string extension in SSAS_FILE_EXTENSIONS)
        //        {
        //            if (sFileName.EndsWith(extension))
        //                return true;
        //        }
        //        foreach (string extension in SSRS_FILE_EXTENSIONS)
        //        {
        //            if (sFileName.EndsWith(extension))
        //                return true;
        //        }
        //        return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public static string PROVIDER_NAME_SOURCESAFE = "MSSCCI:Microsoft Visual SourceSafe";
        public static string PROVIDER_NAME_TFS = "{4CA58AB2-18FA-4F8D-95D4-32DDF27D184C}";
        public static string PROVIDER_NAME_GIT = "Git"; //made up this provider name for integration with the previous flow

        public override void Exec()
        {
            try
            {
                ExecInternal();
            }
            catch (Exception ex)
            {
                string sError = "";
                Exception exLoop = ex;
                while (exLoop != null)
                {
                    sError += exLoop.Message + "\r\n";
                    exLoop = exLoop.InnerException;
                }
                sError += ex.StackTrace;
                MessageBox.Show(sError, "BIDS Helper Smart Diff Error");
            }
        }

       

        private void ExecInternal()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;

#if !(YUKON || KATMAI || DENALI || SQL2014)
                if (projItem.Name.ToLower().EndsWith(".bim"))
                {
                    var sandboxWrapper = new BIDSHelper.SSAS.DataModelingSandboxWrapper(this);
                    if (sandboxWrapper.IsTabularMetadata)
                    {
                        string compatibility = sandboxWrapper.DatabaseCompatibilityLevel.ToString();
                        System.Windows.Forms.MessageBox.Show("BIDS Helper Smart Diff is not supported for " + compatibility + " compatibility level models yet.", "BIDS Helper Smart Diff");
                        return;
                    }
                }
#endif

                SourceControl2 oSourceControl = ((SourceControl2)this.ApplicationObject.SourceControl);
                string sProvider = "";
                string sSourceControlServerName = "";
                string sServerBinding = "";
                string sProject = "";
                string sDefaultSourceSafePath = "";
                if (oSourceControl != null)
                {
                    SourceControlBindings bindings = null;
                    try
                    {
                        bindings = oSourceControl.GetBindings(projItem.ContainingProject.FileName);
                    }
                    catch
                    {
                        //now that you can have custom diff viewers via the preferences window
                    }
                    if (bindings != null)
                    {
                        sProvider = bindings.ProviderName;
                        sSourceControlServerName = bindings.ServerName;
                        sServerBinding = bindings.ServerBinding;
                        if (sProvider == PROVIDER_NAME_SOURCESAFE)
                        {
                            if (sServerBinding.IndexOf("\",") <= 0) throw new Exception("Can't find SourceSafe project path.");
                            sProject = sServerBinding.Substring(1, sServerBinding.IndexOf("\",") - 1);
                        }
                        else if (sProvider == PROVIDER_NAME_TFS)
                        {
                            sProject = sServerBinding;
                        }
                        if (oSourceControl.IsItemUnderSCC(projItem.get_FileNames(0)))
                        {
                            if (projItem.get_FileNames(0).ToLower().StartsWith(bindings.LocalBinding.ToLower()))
                            {
                                sDefaultSourceSafePath = sProject + projItem.get_FileNames(0).Substring(bindings.LocalBinding.Length).Replace('\\','/');
                            }
                            else
                            {
                                sDefaultSourceSafePath = sProject + "/" + projItem.Name;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (GitHelper.IsPathInGitRepository(projItem.get_FileNames(0)))
                            {
                                sSourceControlServerName = GitHelper.GetGitRepositoryPath(projItem.get_FileNames(0));
                                sDefaultSourceSafePath = "$/" + GitHelper.GetRelativePath(projItem.get_FileNames(0));
                                sProvider = PROVIDER_NAME_GIT;
                            }
                        }
                        catch (Exception ex)
                        {
                            package.Log.Exception("Could not check whether solution is a Git repository", ex);
                        }
                        
                    }
                }

                if (projItem.Document != null && !projItem.Document.Saved)
                {
                    if (MessageBox.Show("This command compares the disk version of a file. Do you want to save your changes to disk before proceeding?", "Save Changes?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        projItem.Save("");
                }

                SSIS.SmartDiff form = new BIDSHelper.SSIS.SmartDiff();
                form.SourceControlProvider = sProvider;
                form.DefaultWindowsPath = projItem.get_FileNames(0);
                if (sProvider == PROVIDER_NAME_SOURCESAFE || sProvider == PROVIDER_NAME_TFS || sProvider == PROVIDER_NAME_GIT)
                {
                    form.DefaultSourceSafePath = sDefaultSourceSafePath;
                    form.SourceSafeIniDirectory = sSourceControlServerName;
                }
                DialogResult res = form.ShowDialog();
                if (res != DialogResult.OK) return;


                //get the XSLT for this file extension
                string sXslt = null;
                string sProjectItemFileName = projItem.Name.ToLower();
                bool bNewLineOnAttributes = true;
                foreach (string extension in DTS_FILE_EXTENSIONS)
                {
                    if (sProjectItemFileName.EndsWith(extension))
                    {
                        sXslt = BIDSHelper.Resources.Common.SmartDiffDtsx;
                        break;
                    }
                }
                foreach (string extension in SSAS_FILE_EXTENSIONS)
                {
                    if (sProjectItemFileName.EndsWith(extension))
                    {
                        sXslt = BIDSHelper.Resources.Common.SmartDiffSSAS;
                        break;
                    }
                }
                foreach (string extension in SSRS_FILE_EXTENSIONS)
                {
                    if (sProjectItemFileName.EndsWith(extension))
                    {
                        sXslt = BIDSHelper.Resources.Common.SmartDiffSSRS;
                        bNewLineOnAttributes = false;
                        break;
                    }
                }

                string sOldFile = System.IO.Path.GetTempFileName();
                string sNewFile = System.IO.Path.GetTempFileName();

                try
                {
                    string sOldFileName = form.txtCompare.Text;
                    string sNewFileName = form.txtTo.Text;

                    if (form.txtCompare.Text.StartsWith("$/"))
                    {
                        if (sProvider == PROVIDER_NAME_SOURCESAFE)
                            GetSourceSafeFile(sSourceControlServerName, form.txtCompare.Text, sOldFile);
                        else if (sProvider == PROVIDER_NAME_TFS)
                            GetTFSFile(sSourceControlServerName, form.txtCompare.Text, sOldFile);
                        else if (sProvider == PROVIDER_NAME_GIT)
                            GetGitFile(sSourceControlServerName, form.txtCompare.Text, sOldFile);
                        sOldFileName += " (server)";
                    }
                    else
                    {
                        System.IO.File.Copy(form.txtCompare.Text, sOldFile, true);
                        sOldFileName += " (local)";
                    }

                    if (form.txtTo.Text.StartsWith("$/"))
                    {
                        if (sProvider == PROVIDER_NAME_SOURCESAFE)
                            GetSourceSafeFile(sSourceControlServerName, form.txtTo.Text, sNewFile);
                        else if (sProvider == PROVIDER_NAME_TFS)
                            GetTFSFile(sSourceControlServerName, form.txtTo.Text, sNewFile);
                        else if (sProvider == PROVIDER_NAME_GIT)
                            GetGitFile(sSourceControlServerName, form.txtTo.Text, sNewFile);
                        sNewFileName += " (server)";
                    }
                    else
                    {
                        System.IO.File.Copy(form.txtTo.Text, sNewFile, true);
                        sNewFileName += " (local)";
                    }

                    PrepXmlForDiff(sOldFile, sXslt, bNewLineOnAttributes);
                    PrepXmlForDiff(sNewFile, sXslt, bNewLineOnAttributes);

                    ShowDiff(sOldFile, sNewFile, form.checkIgnoreCase.Checked, form.checkIgnoreEOL.Checked, form.checkIgnoreWhiteSpace.Checked, sOldFileName, sNewFileName);
                }
                finally
                {
                    try
                    {
                        System.IO.File.Delete(sOldFile);
                        System.IO.File.Delete(sNewFile);
                    }
                    catch { }
                }

            }
            catch (System.Exception ex)
            {
                string sError = "";
                Exception exLoop = ex;
                while (exLoop != null)
                {
                    sError += exLoop.Message + "\r\n";
                    exLoop = exLoop.InnerException;
                }
                sError += ex.StackTrace;
                MessageBox.Show(sError, "BIDS Helper Smart Diff Error");
            }
        }

        public static string[] GetSourceControlVersions(string sIniDirectory, string sSourceSafePath, string sProvider)
        {
            if (sProvider == PROVIDER_NAME_SOURCESAFE)
            {
                return GetSourceSafeVersions(sIniDirectory, sSourceSafePath);
            }
            else if (sProvider == PROVIDER_NAME_TFS)
            {
                return GetTFSVersions(sIniDirectory, sSourceSafePath);
            }
            else if (sProvider == PROVIDER_NAME_GIT)
            {
                return GitHelper.GetHistoryOfFile(sIniDirectory, sSourceSafePath);
            }
            throw new Exception("Invalid provider");
        }


        #region Git Access Methods
        private static void GetGitFile(string sRepositoryPath, string sGitPath, string sLocalPath)
        {
            string sha = null;
            if (sGitPath.Contains(":"))
            {
                sha = sGitPath.Substring(sGitPath.IndexOf(':') + 1);
                sGitPath = sGitPath.Substring(0, sGitPath.IndexOf(':'));
            }

            bool bResult = GitHelper.GetSpecificVersionOfFile(sRepositoryPath, sGitPath, sLocalPath, sha);
            if (!bResult) throw new Exception("Could not get file from Git");
        }
        #endregion

        #region SourceSafe Access Methods
        //allows late-binding so that you don't have to have SourceSafe installed to compile BIDS Helper
        private static string VSS_ASSEMBLY_FULL_NAME = "Microsoft.VisualStudio.SourceSafe.Interop, Version=5.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        private static void GetSourceSafeFile(string sIniDirectory, string sSourceSafePath, string sLocalPath)
        {
            int iVersion = -1;
            if (sSourceSafePath.Contains(":"))
            {
                iVersion = int.Parse(sSourceSafePath.Substring(sSourceSafePath.IndexOf(':') + 1));
                sSourceSafePath = sSourceSafePath.Substring(0, sSourceSafePath.IndexOf(':'));
            }

            object db = OpenSourceSafeDatabase(sIniDirectory, sSourceSafePath);

            System.Reflection.BindingFlags getpropflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance;
            System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;

            try
            {
                const int VSSFLAG_REPREPLACE = 128;
                const int VSSFLAG_FORCEDIRNO = 16384;
                object item;
                try
                {
                    item = db.GetType().InvokeMember("VSSItem", getpropflags, null, db, new object[] { sSourceSafePath, false });
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot open that path in SourceSafe.", ex);
                }
                if (iVersion < 0) iVersion = (int)item.GetType().InvokeMember("VersionNumber", getpropflags, null, item, null);
                object version = item.GetType().InvokeMember("Version", getpropflags, null, item, new object[] { iVersion });
                version.GetType().InvokeMember("Get", getmethodflags, null, version, new object[] { sLocalPath, VSSFLAG_FORCEDIRNO | VSSFLAG_REPREPLACE });
            }
            finally
            {
                db.GetType().InvokeMember("Close", getmethodflags, null, db, null);
            }
        }

        private static string[] GetSourceSafeVersions(string sIniDirectory, string sSourceSafePath)
        {
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();

            int iVersion = -1;
            if (sSourceSafePath.Contains(":"))
            {
                iVersion = int.Parse(sSourceSafePath.Substring(sSourceSafePath.IndexOf(':') + 1));
                sSourceSafePath = sSourceSafePath.Substring(0, sSourceSafePath.IndexOf(':'));
            }

            object db = OpenSourceSafeDatabase(sIniDirectory, sSourceSafePath);

            System.Reflection.BindingFlags getpropflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance;
            System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;

            try
            {
                object item;
                try
                {
                    item = db.GetType().InvokeMember("VSSItem", getpropflags, null, db, new object[] { sSourceSafePath, false });
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot open that path in SourceSafe to find versions.", ex);
                }
                object versions = item.GetType().InvokeMember("Versions", getpropflags, null, item, new object[] { 0 });
                System.Collections.IEnumerator enumerator = (System.Collections.IEnumerator)versions.GetType().InvokeMember("GetEnumerator", getmethodflags, null, versions, null);
                while (enumerator.MoveNext())
                {
                    object version = enumerator.Current;
                    int iVersionNumber = (int)version.GetType().InvokeMember("VersionNumber", getpropflags, null, version, null);
                    DateTime dtVersionDate = (DateTime)version.GetType().InvokeMember("Date", getpropflags, null, version, null);
                    string sVersionUsername = (string)version.GetType().InvokeMember("Username", getpropflags, null, version, null);
                    string sAction = (string)version.GetType().InvokeMember("Action", getpropflags, null, version, null);
                    if (!sAction.StartsWith("Labeled")) //ignore labels because it throws off the version numbering
                    {
                        list.Add(iVersionNumber + "  -  " + dtVersionDate.ToString() + "  -  " + sVersionUsername);
                    }
                }

                return list.ToArray();
            }
            finally
            {
                db.GetType().InvokeMember("Close", getmethodflags, null, db, null);
            }
        }

        private static object OpenSourceSafeDatabase(string sIniDirectory, string sSourceSafePath)
        {
            string sUsername = GetVSSUsername();
            string sPassword = "";
            if (_dictPasswords.ContainsKey(sUsername))
                sPassword = _dictPasswords[sUsername];

            System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
            System.Reflection.Assembly vssAssembly = System.Reflection.Assembly.Load(VSS_ASSEMBLY_FULL_NAME);
            
            object db = null;
            bool bRetry = true;
            bool bFirstTime = true;
            while (bRetry)
            {
                try
                {
                    db = vssAssembly.CreateInstance("Microsoft.VisualStudio.SourceSafe.Interop.VSSDatabaseClass");
                    db.GetType().InvokeMember("Open", getmethodflags, null, db, new object[] { sIniDirectory + "\\srcsafe.ini", sUsername, sPassword });
                    bRetry = false;
                }
                catch (Exception ex)
                {
                    try
                    {
                        db.GetType().InvokeMember("Close", getmethodflags, null, db, null);
                    }
                    catch { }

                    Form passwordForm = new Form();
                    passwordForm.Icon = BIDSHelper.Resources.Common.BIDSHelper;
                    passwordForm.Text = "SourceSafe Login";
                    passwordForm.MaximizeBox = false;
                    passwordForm.MinimizeBox = false;
                    passwordForm.SizeGripStyle = SizeGripStyle.Hide;
                    passwordForm.ShowInTaskbar = false;
                    passwordForm.Width = 500;
                    passwordForm.Height = 150;
                    passwordForm.MaximumSize = new System.Drawing.Size(1600, passwordForm.Height);
                    passwordForm.MinimumSize = new System.Drawing.Size(300, passwordForm.Height);
                    passwordForm.StartPosition = FormStartPosition.CenterParent;

                    TextBox txtUsername = new TextBox();
                    txtUsername.Width = passwordForm.Width - 100;
                    txtUsername.Left = 80;
                    txtUsername.Top = 20;
                    txtUsername.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                    passwordForm.Controls.Add(txtUsername);
                    txtUsername.Text = sUsername;

                    Label lblUsername = new Label();
                    lblUsername.Text = "Username:";
                    lblUsername.Width = 60;
                    lblUsername.Left = 15;
                    lblUsername.Top = txtUsername.Top;
                    passwordForm.Controls.Add(lblUsername);

                    TextBox txtPassword = new TextBox();
                    txtPassword.Name = "txtPassword";
                    txtPassword.PasswordChar = '*';
                    txtPassword.Width = passwordForm.Width - 100;
                    txtPassword.Left = 80;
                    txtPassword.Top = 50;
                    txtPassword.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                    passwordForm.Controls.Add(txtPassword);
                    txtPassword.Focus();

                    Label lblPassword = new Label();
                    lblPassword.Text = "Password:";
                    lblPassword.Width = 60;
                    lblPassword.Left = 15;
                    lblPassword.Top = txtPassword.Top;
                    passwordForm.Controls.Add(lblPassword);

                    Button ok = new Button();
                    ok.Text = "OK";
                    ok.Width = 80;
                    ok.Left = txtPassword.Right - ok.Width;
                    ok.Top = txtPassword.Bottom + 15;
                    ok.Anchor = AnchorStyles.Right;
                    ok.Click += new EventHandler(passwordFormOK_Click);
                    passwordForm.Controls.Add(ok);

                    if (!bFirstTime && ex.InnerException != null)
                    {
                        Label lblError = new Label();
                        lblError.ForeColor = System.Drawing.Color.Red;
                        lblError.Left = 15;
                        lblError.Width = passwordForm.Width - 45 - ok.Width;
                        lblError.Top = txtPassword.Bottom + 15;
                        lblError.Text = ex.InnerException.Message;
                        passwordForm.Controls.Add(lblError);
                    }
                    bFirstTime = false;

                    passwordForm.AcceptButton = ok;
                    passwordForm.Load += new EventHandler(passwordForm_Load);
                    DialogResult res = passwordForm.ShowDialog();
                    if (res != DialogResult.OK) throw new Exception("Unable to login using username " + sUsername, ex);

                    sUsername = txtUsername.Text;
                    sPassword = txtPassword.Text;
                    _dictPasswords[sUsername] = sPassword;

                    bRetry = true;
                }
            }

            return db;
        }

        static void passwordForm_Load(object sender, EventArgs e)
        {
            try
            {
                Form form = (Form)sender;
                form.Controls["txtPassword"].Select();
            }
            catch { }
        }

        private static void passwordFormOK_Click(object sender, EventArgs e)
        {
            try
            {
                Button b = (Button)sender;
                Form form = (Form)b.Parent;
                form.DialogResult = DialogResult.OK;
                form.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        
        private static string GetVSSUsername()
        {
            string sUsername = "";
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_VisualStudioRegistryPath + @"\SourceControl\UserNames");
            if (rk != null)
            {
                sUsername = (string)rk.GetValue("0", "");
                rk.Close();
            }
            return sUsername;
        }
        #endregion

        #region TFS Access Methods
        private static string TFS_ASSEMBLY_FULL_NAME = "Microsoft.TeamFoundation.Client";
        private static string TFS_VERSION_CONTROL_ASSEMBLY_FULL_NAME = "Microsoft.TeamFoundation.VersionControl.Client";

        private static void GetTFSFile(string sServer, string sPath, string sLocalPath)
        {
            System.Reflection.BindingFlags getpropflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance;
            System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
            System.Reflection.Assembly tfsAssembly = System.Reflection.Assembly.Load(TFS_ASSEMBLY_FULL_NAME);
            System.Reflection.Assembly tfsVersionControlAssembly = System.Reflection.Assembly.Load(TFS_VERSION_CONTROL_ASSEMBLY_FULL_NAME);
            System.IServiceProvider server = null;
#if VS2017
            Type typeFactory = tfsAssembly.GetType("Microsoft.TeamFoundation.Client.TfsTeamProjectCollectionFactory");
            server = (System.IServiceProvider)typeFactory.InvokeMember("GetTeamProjectCollection", getmethodflags | System.Reflection.BindingFlags.Static, null, null, new object[] { new Uri(sServer) });
#else
            Type typeFactory = tfsAssembly.GetType("Microsoft.TeamFoundation.Client.TeamFoundationServerFactory");
            server = (System.IServiceProvider)typeFactory.InvokeMember("GetServer", getmethodflags | System.Reflection.BindingFlags.Static, null, null, new object[] { sServer });
#endif
            object versionControl = server.GetService(tfsVersionControlAssembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer"));

            Type typeVersionSpec = tfsVersionControlAssembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.VersionSpec");
            object versionSpec = null;

            int iVersion = -1;
            if (sPath.Contains(":"))
            {
                iVersion = int.Parse(sPath.Substring(sPath.IndexOf(':') + 1));
                sPath = sPath.Substring(0, sPath.IndexOf(':'));
                versionSpec = typeVersionSpec.InvokeMember("ParseSingleSpec", getmethodflags | System.Reflection.BindingFlags.Static, null, null, new object[] { iVersion.ToString(), null });
            }
            else
            {
                versionSpec = typeVersionSpec.InvokeMember("Latest", getpropflags | System.Reflection.BindingFlags.Static, null, null, null);
            }
            versionControl.GetType().InvokeMember("DownloadFile", getmethodflags, null, versionControl, new object[] { sPath, 0, versionSpec, sLocalPath });
        }

        private static string[] GetTFSVersions(string sServer, string sPath)
        {
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();

            System.Reflection.BindingFlags getpropflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance;
            System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
            System.Reflection.Assembly tfsAssembly = System.Reflection.Assembly.Load(TFS_ASSEMBLY_FULL_NAME);
            System.Reflection.Assembly tfsVersionControlAssembly = System.Reflection.Assembly.Load(TFS_VERSION_CONTROL_ASSEMBLY_FULL_NAME);
            System.IServiceProvider server = null;
#if VS2017
            Type typeFactory = tfsAssembly.GetType("Microsoft.TeamFoundation.Client.TfsTeamProjectCollectionFactory");
            server = (System.IServiceProvider)typeFactory.InvokeMember("GetTeamProjectCollection", getmethodflags | System.Reflection.BindingFlags.Static, null, null, new object[] { new Uri(sServer) });
#else
            Type typeFactory = tfsAssembly.GetType("Microsoft.TeamFoundation.Client.TeamFoundationServerFactory");
            server = (System.IServiceProvider)typeFactory.InvokeMember("GetServer", getmethodflags | System.Reflection.BindingFlags.Static, null, null, new object[] { sServer });
#endif
            object versionControl = server.GetService(tfsVersionControlAssembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer"));

            int iVersion = -1;
            if (sPath.Contains(":"))
            {
                iVersion = int.Parse(sPath.Substring(sPath.IndexOf(':') + 1));
                sPath = sPath.Substring(0, sPath.IndexOf(':'));
            }

            Type typeVersionSpec = tfsVersionControlAssembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.VersionSpec");
            object latest = typeVersionSpec.InvokeMember("Latest", getpropflags | System.Reflection.BindingFlags.Static, null, null, null);

            Type typeRecursionType = tfsVersionControlAssembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.RecursionType");
            object full = Enum.Parse(typeRecursionType, "Full");

            System.Collections.IEnumerable enumerable = (System.Collections.IEnumerable)versionControl.GetType().InvokeMember("QueryHistory", getmethodflags, null, versionControl, new object[] { sPath, latest, 0, full, null, null, null, int.MaxValue, false, false });
            foreach (object version in enumerable)
            {
                int iVersionNumber = (int)version.GetType().InvokeMember("ChangesetId", getpropflags, null, version, null);
                DateTime dtVersionDate = (DateTime)version.GetType().InvokeMember("CreationDate", getpropflags, null, version, null);
                string sVersionUsername = (string)version.GetType().InvokeMember("Committer", getpropflags, null, version, null);
                list.Add(iVersionNumber + "  -  " + dtVersionDate.ToString() + "  -  " + sVersionUsername);
            }
            return list.ToArray();
        }
#endregion

        private void PrepXmlForDiff(string sFilename, string sXSL, bool bNewLineOnAttributes)
        {
            System.IO.File.SetAttributes(sFilename, System.IO.FileAttributes.Normal); //unhide the file so you can overwrite it

            if (!string.IsNullOrEmpty(sXSL))
            {
                TransformXmlFile(sFilename, sXSL);
            }

            //format the XML file for easier visual diff
            XmlDocument doc = new XmlDocument();
            doc.Load(sFilename);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = bNewLineOnAttributes;
            XmlWriter wr = XmlWriter.Create(sFilename, settings);
            doc.Save(wr);
            wr.Close();

            //finally, replace the &#xA; inside attributes with actual line breaks to make the SQL statements on Execute SQL tasks more readable
            StringBuilder sbReplacer = new StringBuilder(System.IO.File.ReadAllText(sFilename));
            sbReplacer.Replace("&#xD;&#xA;", "\r\n");
            sbReplacer.Replace("&#xD;", "\r\n");
            sbReplacer.Replace("&#xA;", "\r\n");
            System.IO.File.WriteAllText(sFilename, sbReplacer.ToString());
        }

        public static void TransformXmlFile(string xmlPath, string sXSL)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.TextWriter writer = new System.IO.StreamWriter(memoryStream, new System.Text.UTF8Encoding());

            System.IO.StringReader xslReader = new System.IO.StringReader(sXSL);
            XmlReader xmlXslReader = XmlReader.Create(xslReader);

            XslCompiledTransform trans = new XslCompiledTransform();
            trans.Load(xmlXslReader);
            trans.Transform(xmlPath, null, writer);

            System.IO.File.WriteAllBytes(xmlPath, memoryStream.GetBuffer()); //can't write out to the input file until after the Transform is done
        }

        //apparently this 11.0 DLL is used in VS2012 and VS2013... the 12.0 DLL didn't have the classes I need
        private static string VS2012_SHELL_INTEROP_ASSEMBLY_FULL_NAME = "Microsoft.VisualStudio.Shell.Interop.11.0, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        private void ShowDiff(string oldFile, string newFile, bool bIgnoreCase, bool bIgnoreEOL, bool bIgnoreWhiteSpace, string sOldFileName, string sNewFileName)
        {
            string sCustomDiffViewer = CustomDiffViewer;
            if (!string.IsNullOrEmpty(sCustomDiffViewer))
            {
                try
                {
                    int iOldFile = sCustomDiffViewer.IndexOf('?');
                    int iNewFile = sCustomDiffViewer.LastIndexOf('?');
                    sCustomDiffViewer = sCustomDiffViewer.Substring(0, iOldFile) + '"' + oldFile + '"' + sCustomDiffViewer.Substring(iOldFile + 1, iNewFile - iOldFile - 1) + '"' + newFile + '"' + sCustomDiffViewer.Substring(iNewFile + 1);
                    Microsoft.VisualBasic.Interaction.Shell(sCustomDiffViewer, Microsoft.VisualBasic.AppWinStyle.MaximizedFocus, true, -1);
                    return;
                }
                catch (Exception ex)
                {
                    throw new Exception("There was a problem using the custom diff viewer: " + sCustomDiffViewer, ex);
                }
            }


            //try the VS2012 built-in diff viewer
            string sVS2012Error = string.Empty;

            if (this.ApplicationObject.Version.CompareTo("11.") == 1 || this.ApplicationObject.Version.CompareTo("12.") == 1)
            {
                try
                {

                    System.Reflection.Assembly vsAssembly = System.Reflection.Assembly.Load(VS2012_SHELL_INTEROP_ASSEMBLY_FULL_NAME);

                    UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                    UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                    ProjectItem projItem = (ProjectItem)hierItem.Object;
                    string sTabName = projItem.Name + " (BIDS Helper Smart Diff)";

                    System.IServiceProvider provider = package.ServiceProvider; //works in all project types even C# projects
                    //if (projItem.ContainingProject is System.IServiceProvider)
                    //{
                    //    provider = (System.IServiceProvider)projItem.ContainingProject;
                    //}
                    //else
                    //{
                    //    provider = TabularHelpers.GetTabularServiceProviderFromBimFile(hierItem, false);
                    //}

                    Type t = vsAssembly.GetType("Microsoft.VisualStudio.Shell.Interop.IVsDifferenceService");
                    object oVsDifferenceService = provider.GetService(vsAssembly.GetType("Microsoft.VisualStudio.Shell.Interop.SVsDifferenceService"));
                    Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame frame = (Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame)t.InvokeMember("OpenComparisonWindow2", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, oVsDifferenceService, new object[] { oldFile, newFile, sTabName, "BIDS Helper Smart Diff", sOldFileName, sNewFileName, "", "", (uint)0 });

                    return;
                }
                catch (Exception exVS2012)
                {
                    sVS2012Error = exVS2012.Message;
                }
            }

            int fFlags = 0;
            string sFlags = "";
            if (bIgnoreCase)
            {
                fFlags |= 1;
                sFlags += " -ic";
            }
            if (bIgnoreEOL)
            {
                fFlags |= 4;
                sFlags += " -ie";
            }
            if (bIgnoreWhiteSpace)
            {
                fFlags |= 2;
                sFlags += " -iw";
            }

            try
            {
                MSDiff_ShowDiffUI(IntPtr.Zero, oldFile, newFile, fFlags, sOldFileName, sNewFileName);
                MSDiff_Cleanup();
            }
            catch (Exception exMSDiff)
            {
                //couldn't use MSDiff (which gets intalled with TFS2010 and below)
                //try VSS EXE
                try
                {
                    string sVSSDir = VSSInstallDir;
                    if (string.IsNullOrEmpty(sVSSDir))
                    {
                        throw new Exception("");
                    }
                    else
                    {
                        Microsoft.VisualBasic.Interaction.Shell("\"" + sVSSDir + "ssexp.exe\" /diff" + sFlags + " \"" + oldFile + "\" \"" + newFile + "\"", Microsoft.VisualBasic.AppWinStyle.MaximizedFocus, true, -1);
                    }
                }
                catch (Exception exVSS)
                {
                    if (!string.IsNullOrEmpty(sVS2012Error)) sVS2012Error = "\r\n\r\nCould not use the built-in Visual Studio diff viewer:\r\n" + sVS2012Error;
                    throw new Exception("Could not start Microsoft Visual SourceSafe 2005 to view diff. " + exVSS.Message + "\r\n\r\nCould not utilize Microsoft Team Foundation Server to view diff. " + exMSDiff.Message + sVS2012Error);
                }
            }
        }

        [DllImport("msdiff", CharSet = CharSet.Unicode)]
        private static extern void MSDiff_ShowDiffUI(IntPtr hwndParent, string pszFileName1, string pszFileName2, int fFlags, string pszOrigNameFile1, string pszOrigNameFile2);

        [DllImport("msdiff")]
        private static extern void MSDiff_Cleanup();

        private static string VSSInstallDir
        {
            get
            {
                Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\8.0\Setup\VS\VSS");
                if (rk != null)
                {
                    string s = (string)rk.GetValue("ProductDir", "");
                    rk.Close();
                    return s;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool VSSInstalled
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(VSSInstallDir))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool TFSInstalled
        {
            get
            {
                try
                {
                    MSDiff_Cleanup(); //test whether we can hit the msdiff DLL
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static string CustomDiffViewer
        {
            get
            {
                string sCustomDiffViewer = null;
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(BIDSHelperPackage.PluginRegistryPath(typeof(SmartDiffPlugin)));
                if (rk != null)
                {
                    sCustomDiffViewer = (string)rk.GetValue(REGISTRY_CUSTOM_DIFF_VIEWER_SETTING_NAME, null);
                    rk.Close();
                }
                return sCustomDiffViewer;
            }
            set
            {
                var regPath = BIDSHelperPackage.PluginRegistryPath(typeof(SmartDiffPlugin));
                RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(regPath, true);
                if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(regPath);
                if (value == null)
                {
                    settingKey.DeleteValue(REGISTRY_CUSTOM_DIFF_VIEWER_SETTING_NAME, false);
                }
                else
                    settingKey.SetValue(REGISTRY_CUSTOM_DIFF_VIEWER_SETTING_NAME, value, RegistryValueKind.String);
                settingKey.Close();
            }
        }
    }
    
}