using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace BIDSHelper
{
    public class SmartDiffPlugin : BIDSHelperPluginBase
    {
        public SmartDiffPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "SmartDiff"; }
        }

        public override int Bitmap
        {
            get { return 1836; }
        }

        public override string ButtonText
        {
            get { return "Smart Diff..."; }
        }

        public override string ToolTip
        {
            get { return ""; } //not seen anywhere
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                return (sFileName.EndsWith(".dtsx"));
            }
            catch
            {
                return false;
            }
        }

        public static string PROVIDER_NAME_SOURCESAFE = "MSSCCI:Microsoft Visual SourceSafe";
        public static string PROVIDER_NAME_TFS = "{4CA58AB2-18FA-4F8D-95D4-32DDF27D184C}";

        //TODO: SSAS and SSRS filetypes
        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
                ProjectItem projItem = (ProjectItem)hierItem.Object;

                if (projItem.Document != null && !projItem.Document.Saved)
                {
                    if (MessageBox.Show("This command compares the disk version of a file. Do you want to save your changes to disk before proceeding?", "Save Changes?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        projItem.Save("");
                }

                SourceControl2 oSourceControl = ((SourceControl2)this.ApplicationObject.SourceControl);
                string sProvider = "";
                string sSourceControlServerName = "";
                string sServerBinding = "";
                string sProject = "";
                string sDefaultSourceSafePath = "";
                if (oSourceControl != null)
                {
                    SourceControlBindings bindings = oSourceControl.GetBindings(projItem.ContainingProject.FileName);
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
                            sDefaultSourceSafePath = sProject + "/" + projItem.Name;
                        }
                    }
                }

                SSIS.SmartDiff form = new BIDSHelper.SSIS.SmartDiff();
                form.SourceControlProvider = sProvider;
                form.DefaultWindowsPath = projItem.get_FileNames(0);
                if (sProvider == PROVIDER_NAME_SOURCESAFE || sProvider == PROVIDER_NAME_TFS)
                {
                    form.DefaultSourceSafePath = sDefaultSourceSafePath;
                    form.SourceSafeIniDirectory = sSourceControlServerName;
                }
                DialogResult res = form.ShowDialog();
                if (res != DialogResult.OK) return;

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
                        sNewFileName += " (server)";
                    }
                    else
                    {
                        System.IO.File.Copy(form.txtTo.Text, sNewFile, true);
                        sNewFileName += " (local)";
                    }

                    PrepXmlForDiff(sOldFile, BIDSHelper.Properties.Resources.SmartDiffDtsx);
                    PrepXmlForDiff(sNewFile, BIDSHelper.Properties.Resources.SmartDiffDtsx);

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
                MessageBox.Show(ex.Message);
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
            throw new Exception("Invalid provider");
        }


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

            string sUsername = GetVSSUsername();
            string sPassword = "";

            System.Reflection.BindingFlags getpropflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance;
            System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
            System.Reflection.Assembly vssAssembly = System.Reflection.Assembly.Load(VSS_ASSEMBLY_FULL_NAME);
            object db = vssAssembly.CreateInstance("Microsoft.VisualStudio.SourceSafe.Interop.VSSDatabaseClass");
            db.GetType().InvokeMember("Open", getmethodflags, null, db, new object[] { sIniDirectory + "\\srcsafe.ini", sUsername, sPassword });

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
                    throw new Exception("Cannot open that path in SourceSafe. Error was: " + ex.Message);
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

            string sUsername = GetVSSUsername();
            string sPassword = "";

            System.Reflection.BindingFlags getpropflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance;
            System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
            System.Reflection.Assembly vssAssembly = System.Reflection.Assembly.Load(VSS_ASSEMBLY_FULL_NAME);
            object db = vssAssembly.CreateInstance("Microsoft.VisualStudio.SourceSafe.Interop.VSSDatabaseClass");
            db.GetType().InvokeMember("Open", getmethodflags, null, db, new object[] { sIniDirectory + "\\srcsafe.ini", sUsername, sPassword });
            try
            {
                object item;
                try
                {
                    item = db.GetType().InvokeMember("VSSItem", getpropflags, null, db, new object[] { sSourceSafePath, false });
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot open that path in SourceSafe. Error was: " + ex.Message);
                }
                object versions = item.GetType().InvokeMember("Versions", getpropflags, null, item, new object[] { 0 });
                System.Collections.IEnumerator enumerator = (System.Collections.IEnumerator)versions.GetType().InvokeMember("GetEnumerator", getmethodflags, null, versions, null);
                while (enumerator.MoveNext())
                {
                    object version = enumerator.Current;
                    int iVersionNumber = (int)version.GetType().InvokeMember("VersionNumber", getpropflags, null, version, null);
                    DateTime dtVersionDate = (DateTime)version.GetType().InvokeMember("Date", getpropflags, null, version, null);
                    string sVersionUsername = (string)version.GetType().InvokeMember("Username", getpropflags, null, version, null);
                    list.Add(iVersionNumber + "  -  " + dtVersionDate.ToString() + "  -  " + sVersionUsername);
                }

                return list.ToArray();
            }
            finally
            {
                db.GetType().InvokeMember("Close", getmethodflags, null, db, null);
            }
        }

        //the following didn't work: System.Threading.Thread.CurrentPrincipal.Identity.Name
        //so we get it from the registry
        private static string GetVSSUsername()
        {
            string sUsername = "";
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\8.0\SourceControl\UserNames");
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
            Type typeFactory = tfsAssembly.GetType("Microsoft.TeamFoundation.Client.TeamFoundationServerFactory");
            System.IServiceProvider server = (System.IServiceProvider)typeFactory.InvokeMember("GetServer", getmethodflags | System.Reflection.BindingFlags.Static, null, null, new object[] { sServer });
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
            Type typeFactory = tfsAssembly.GetType("Microsoft.TeamFoundation.Client.TeamFoundationServerFactory");
            System.IServiceProvider server = (System.IServiceProvider)typeFactory.InvokeMember("GetServer", getmethodflags | System.Reflection.BindingFlags.Static, null, null, new object[] { sServer });
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

        private void PrepXmlForDiff(string sFilename, string sXSL)
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
            settings.NewLineOnAttributes = true;
            XmlWriter wr = XmlWriter.Create(sFilename, settings);
            doc.Save(wr);
            wr.Close();
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

        private void ShowDiff(string oldFile, string newFile, bool bIgnoreCase, bool bIgnoreEOL, bool bIgnoreWhiteSpace, string sOldFileName, string sNewFileName)
        {
            int fFlags = 0;
            if (bIgnoreCase) fFlags |= 1;
            if (bIgnoreEOL) fFlags |= 4;
            if (bIgnoreWhiteSpace) fFlags |= 2;

            MSDiff_ShowDiffUI(IntPtr.Zero, oldFile, newFile, fFlags, sOldFileName, sNewFileName);
        }

        [DllImport("msdiff", CharSet = CharSet.Unicode)]
        private static extern void MSDiff_ShowDiffUI(IntPtr hwndParent, string pszFileName1, string pszFileName2, int fFlags, string pszOrigNameFile1, string pszOrigNameFile2);
        
    }


}