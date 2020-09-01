//------------------------------------------------------------------------------
// <copyright file="BidsHelperPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Reflection;
using BIDSHelper.Core;
using BIDSHelper.Core.VsIntegration;
using System.Linq;
using Microsoft.VisualStudio;
using BIDSHelper.Core.Logger;
using Task = System.Threading.Tasks.Task;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace mscoree
{
    [CompilerGenerated]
    [Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [TypeIdentifier]
    [ComImport]
    [CLSCompliant(false)]
    public interface ICorRuntimeHost
    {
        void _VtblGap1_11();

        void EnumDomains(out IntPtr enumHandle);

        void NextDomain([In] IntPtr enumHandle, [MarshalAs(UnmanagedType.IUnknown)] out object appDomain);

        void CloseEnum([In] IntPtr enumHandle);
    }
}

namespace BIDSHelper
{

    public enum enumIDEMode
    {
        Design = 1,
        Debug = 2,
        Run = 3
    }

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82", PackageAutoLoadFlags.BackgroundLoad)]
    //[ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", VersionInfo.Version, IconResourceID = 400)] // Info on this package for Help/About
    [Guid(BIDSHelperPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(BIDSHelperOptionsFeatures), "BIDS Helper", "Features", 0, 0, true)]
    [ProvideOptionPage(typeof(BIDSHelperPreferencesDialogPage), "BIDS Helper", "Preferences", 0, 0, true)]
    [ProvideOptionPage(typeof(BIDSHelperOptionsVersion), "BIDS Helper", "Version", 0, 0, true)]
    public sealed class BIDSHelperPackage : AsyncPackage, IVsDebuggerEvents
    {
        /// <summary>
        /// BidsHelperPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "d3474f10-475f-4a9d-84f6-85bc892ad3b6";
        public const string REGISTRY_BASE_PATH = "SOFTWARE\\BIDS Helper";


        private static System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase> plugins = new System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase>();
        private enumIDEMode _ideMode = enumIDEMode.Design;

        /// <summary>
        /// Initializes a new instance of the <see cref="BidsHelperPackage"/> class.
        /// </summary>
        public BIDSHelperPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }


        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        //protected override void Initialize()
        {
            // runs in the background thread and doesn't affect the responsiveness of the UI thread.
            //await Task.Delay(100);
            
            // Switches to the UI thread in order to consume some services used in command initialization
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);



            OriginalInitialize();

        }

        private void OriginalInitialize()
        {
            bool bQuitting = false;
            base.Initialize();

#if DEBUG
            Log = new Core.Logger.OutputLogger(this);
            Log.LogLevel = LogLevels.Debug;
#else
            Log = new Core.Logger.NullLogger();
            Log.LogLevel = LogLevels.Warning;
#endif
            Log.Debug("BIDSHelper Package Initialize Starting");


            //var bidsHelperPath = new System.IO.FileInfo(typeof(BIDSHelperPackage).Assembly.Location);

//#pragma warning disable 0618 //ignore the fact this is an obsolete method
//            AppDomain.CurrentDomain.AppendPrivatePath(bidsHelperPath.DirectoryName);
//            AppDomain.CurrentDomain.SetupInformation.ApplicationBase = bidsHelperPath.DirectoryName;
//#pragma warning restore 0618

            //(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath + ";" ?? "") + 
            /*
             * VS2019
             * Microsoft Reporting Services Projects (by Microsoft) v2.5.6 717ad572-c4b7-435c-c166-c2969777f718 
Microsoft Analysis Services Projects (by Microsoft) v2.8.11 04a86fc2-dbd5-4222-848e-911638e487fe 

            VS2017
            Microsoft Reporting Services Projects (by Microsoft) v2.5.6 717ad572-c4b7-435c-c166-c2969777f718 
Microsoft Integration Services Projects (by Microsoft) v2.1 D1B09713-C12E-43CC-9EF4-6562298285AB 
Microsoft Analysis Services Projects (by Microsoft) v2.8.11 04a86fc2-dbd5-4222-848e-911638e487fe 

             */
            try
            {
                System.IServiceProvider serviceProvider = this as System.IServiceProvider;
                //Microsoft.VisualStudio.ExtensionManager.IVsExtensionManager em =
                //   (Microsoft.VisualStudio.ExtensionManager.IVsExtensionManager)serviceProvider.GetService(
                //        typeof(Microsoft.VisualStudio.ExtensionManager.IVsExtensionManager));
                var em1 =
                   serviceProvider.GetService(
                        typeof(Microsoft.VisualStudio.ExtensionManager.SVsExtensionManager));
                Microsoft.VisualStudio.ExtensionManager.IVsExtensionManager em = em1 as Microsoft.VisualStudio.ExtensionManager.IVsExtensionManager;
                string result = "";
                foreach (Microsoft.VisualStudio.ExtensionManager.IInstalledExtension i in em.GetInstalledExtensions())
                {
                    try
                    {
                        Microsoft.VisualStudio.ExtensionManager.IExtensionHeader h = i.Header;
                        if (h.Name == "Microsoft Reporting Services Projects" || string.Compare(h.Identifier, "717ad572-c4b7-435c-c166-c2969777f718", true) == 0)
                        {
                            SSRSExtensionVersion = h.Version;
                            SSRSExtensionInstallPath = i.InstallPath;
                            Log.Debug("SSRS extension v" + h.Version + " is installed at " + i.InstallPath);
                        }
                        else if (h.Name == "Microsoft Integration Services Projects" || string.Compare(h.Identifier, "D1B09713-C12E-43CC-9EF4-6562298285AB", true) == 0 //2.2 (VS2017)
                            || h.Name == "SQL Server Integration Services Projects" || string.Compare(h.Identifier, "851E7A09-7B2B-4F06-A15D-BABFCB26B970", true) == 0 //3.0 (VS2019)
                            )
                        {
                            SSISExtensionVersion = h.Version;
                            SSISExtensionInstallPath = i.InstallPath;
                            Log.Debug("SSIS extension v" + h.Version + " is installed at " + i.InstallPath);
                        }
                        else if (h.Name == "Microsoft Analysis Services Projects" || string.Compare(h.Identifier, "04a86fc2-dbd5-4222-848e-911638e487fe", true) == 0)
                        {
                            SSASExtensionVersion = h.Version;
                            SSASExtensionInstallPath = i.InstallPath;
                            Log.Debug("SSAS extension v" + h.Version + " is installed at " + i.InstallPath);
                        }
                        else if (h.Name == "Microsoft BI Shared Components for Visual Studio" || string.Compare(h.Identifier, "BAB64743-DA65-4501-B3A3-A73171C73D77", true) == 0)
                        {
                            BISharedExtensionInstallPath = i.InstallPath;
                            Log.Debug("BI Shared extension v" + h.Version + " is installed at " + i.InstallPath);
                        }
                        result += h.Name + " (by " + h.Author + ") v" + h.Version + " " + h.Identifier + " " + h.MoreInfoUrl + " " + i.InstallPath + System.Environment.NewLine;
                    }
                    catch (Exception ex){ Log.Debug($"Error iterating other extensions: {ex.Message}"); }
                }
                Log.Debug(result);
            }
            catch (Exception ex) {
                Log.Debug($"Error getting extension manager: {ex.Message}");
            }


            string sAddInTypeName = string.Empty;
            try
            {
#if !DENALI
                //given the version numbers seem to be changing frequently, try this approach to increment version numbers of references
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);




                try
                {
                    MulticastDelegate handler = (MulticastDelegate)AppDomain.CurrentDomain.GetType().InvokeMember("_AssemblyResolve", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, AppDomain.CurrentDomain, null);
                    System.Collections.Generic.List<object> invocationList = new System.Collections.Generic.List<object>(handler.GetInvocationList());
                    int cnt = invocationList.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        System.ResolveEventHandler info = (System.ResolveEventHandler)invocationList[i];
                        Log.Debug(info.Method.ToString() + " - " + info.Method.DeclaringType.ToString());
                        if (info.Method.DeclaringType.FullName.StartsWith("Microsoft.DataWarehouse.")) //remove this event handler. We will call it from our AssemblyResolve code
                        {
                            Log.Debug("removed this AssemblyResolve event handler from the list and will call in our AssemblyResolve event handler");
                            invocationList.RemoveAt(i);
                            _microsoftEventHandlersToIgnoreErrors.Add(info);
                            cnt--;
                            i--;
                        }
                    }

                    typeof(MulticastDelegate).InvokeMember("_invocationList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetField, null, handler, new object[] { invocationList.ToArray() });
                    typeof(MulticastDelegate).InvokeMember("_invocationCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetField, null, handler, new object[] { new IntPtr(invocationList.Count) });
                }
                catch
                {

                }
#endif

                StatusBar = new Core.VsIntegration.StatusBar(this);
                StatusBar.Text = "Loading BIDSHelper (" + this.GetType().Assembly.GetName().Version.ToString() + ")...";
                VsShell = (IVsShell)this.GetService(typeof(SVsShell));
                DTE2 = this.GetService(typeof(Microsoft.VisualStudio.Shell.Interop.SDTE)) as EnvDTE80.DTE2;

                DebuggerService.AdviseDebuggerEvents(this, out debugEventCookie);

                //if (SwitchVsixManifest())
                //{
                //    bQuitting = true;
                //    RestartVisualStudio();
                //    return;
                //}

                System.Collections.Generic.List<Exception> pluginExceptions = new System.Collections.Generic.List<Exception>();
                Type[] types = null;
                try
                {
                    types = Assembly.GetExecutingAssembly().GetTypes();
                }
                catch (ReflectionTypeLoadException loadEx)
                {
                    types = loadEx.Types; //if some types can't be loaded (possibly because SSIS SSDT isn't installed, just SSAS?) then proceed with the types that work
                    //pluginExceptions.Add(loadEx); //in testing it appears that this does return the complete list of types so there's no need to display this exception to the user, I don't think since we will individually load plugins below and log exceptions
                    //Log.Exception("Problem loading BIDS Helper types list", loadEx);
                    //Log.Error(FormatLoaderException(loadEx));
                }
                Log.Debug($"Found {types.Length} types");

                //can be used for debugging which types couldn't be loaded above
                //for (int i = 0; i < types.Length; i++)
                //{
                //    if (types[i] == null)
                //        Log.Error("types[" + i + "] is null");
                //    else
                //        Log.Debug("types[" + i + "]\t" + types[i].FullName);
                //}

                foreach (Type t in types)
                {
                    if (//typeof(IBIDSHelperPlugin).IsAssignableFrom(t.GetType())
                        t != null
                        && t.GetInterfaces().Contains(typeof(IBIDSHelperPlugin))
                        && (!object.ReferenceEquals(t, typeof(IBIDSHelperPlugin)))
                        && (!t.IsAbstract))
                    {
                        sAddInTypeName = t.Name;

                        //new... only load SSIS to test this works
                        //load any not marked
                        var categoryAttribute = t.GetCustomAttributes(typeof(FeatureCategory), true).FirstOrDefault() as FeatureCategory;
                        if (categoryAttribute != null)
                        {
                            if (SSISExtensionVersion == null && categoryAttribute.Category == BIDSFeatureCategories.SSIS)
                            {
                                Log.Verbose(string.Format("Skipping Plugin: {0}", sAddInTypeName));
                                continue;
                            }
                            else if (SSASExtensionVersion == null && (categoryAttribute.Category == BIDSFeatureCategories.SSASMulti || categoryAttribute.Category == BIDSFeatureCategories.SSASTabular))
                            {
                                Log.Verbose(string.Format("Skipping Plugin: {0}", sAddInTypeName));
                                continue;
                            }
                            else if (SSRSExtensionVersion == null && categoryAttribute.Category == BIDSFeatureCategories.SSRS)
                            {
                                Log.Verbose(string.Format("Skipping Plugin: {0}", sAddInTypeName));
                                continue;
                            }
                        }
                        else
                        {
                            Log.Verbose(string.Format("Warning: Plugin FeatureCategory not set: {0}", sAddInTypeName));
                        }


                        Log.Verbose(string.Format("Loading Plugin: {0}", sAddInTypeName));

                        BIDSHelperPluginBase feature;
                        Type[] @params = { typeof(BIDSHelperPackage) };
                        System.Reflection.ConstructorInfo con;

                        con = t.GetConstructor(@params);

                        if (con == null)
                        {
                            System.Windows.Forms.MessageBox.Show("Problem loading type " + t.Name + ". No constructor found.");
                            continue;
                        }

                        try
                        {
                            feature = (BIDSHelperPluginBase)con.Invoke(new object[] { this });
                            Plugins.Add(feature.FullName, feature);
                        }
                        catch (Exception ex)
                        {
                            pluginExceptions.Add(new Exception("BIDS Helper plugin constructor failed on " + sAddInTypeName + ": " + ex.Message + "\r\n" + ex.StackTrace, ex));
                            Log.Exception("BIDS Helper plugin constructor failed on " + sAddInTypeName, ex);
                        }
                    }
                }

                if (pluginExceptions.Count > 0)
                {
                    string sException = "";
                    foreach (Exception pluginEx in pluginExceptions)
                    {
                        sException += FormatLoaderException(pluginEx);
                    }
                    AddInLoadException = new Exception(sException);
                }

#if DENALI
                //handle assembly reference problems when the compiled reference doesn't exist in that version of Visual Studio
                //doesn't appear to be needed for VS2013
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);
#endif

            }
            catch (Exception ex)
            {
                //don't show a popup anymore since this exception is viewable in the Version dialog in the Tools menu
                if (string.IsNullOrEmpty(sAddInTypeName))
                {
                    AddInLoadException = ex;
                    Log.Exception("Problem loading BIDS Helper", ex);
                    //System.Windows.Forms.MessageBox.Show("Problem loading BIDS Helper: " + ex.Message + "\r\n" + ex.StackTrace);
                }
                else
                {
                    AddInLoadException = new Exception("Problem loading BIDS Helper. Problem type was " + sAddInTypeName + ": " + ex.Message + "\r\n" + ex.StackTrace, ex);
                    Log.Exception("Problem loading BIDS Helper. Problem type was " + sAddInTypeName , ex);
                    //System.Windows.Forms.MessageBox.Show("Problem loading BIDS Helper. Problem type was " + sAddInTypeName + ": " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            finally
            {
                if (!bQuitting)
                    StatusBar.Clear();
            }

        }

        public static string FormatLoaderException(Exception pluginEx)
        {
            string sException = "";
            sException += string.Format("BIDS Helper encountered an error when Visual Studio started:\r\n{0}\r\n{1}"
                , pluginEx.Message
                , pluginEx.StackTrace);

            Exception innerEx = pluginEx.InnerException;
            while (innerEx != null)
            {
                sException += string.Format("\r\nInner exception:\r\n{0}\r\n{1}"
                , innerEx.Message
                , innerEx.StackTrace);
                innerEx = innerEx.InnerException;
            }

            ReflectionTypeLoadException ex = pluginEx as ReflectionTypeLoadException;
            if (ex == null) ex = pluginEx.InnerException as ReflectionTypeLoadException;
            if (ex != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine();
                    sb.AppendLine(exSub.GetType().FullName);
                    sb.AppendLine(exSub.Message);
                    sb.AppendLine(exSub.StackTrace);
                    System.IO.FileNotFoundException exFileNotFound = exSub as System.IO.FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                        sb.AppendLine(string.Format("Source: {0}", exFileNotFound.Source));
                        sb.AppendLine(string.Format("TargetSite: {0}", exFileNotFound.TargetSite));
                    }
                    sb.AppendLine();
                }
                sException += sb.ToString();
            }
            return sException;
        }

//        private bool SwitchVsixManifest()
//        {
//#if SQL2019
//            string sVersion = VersionInfo.SqlServerVersion.ToString();
//            if (sVersion.StartsWith("14.")) //this BI Dev Extensions DLL is for SQL 2019 but you have SSDT for SQL2017 installed
//            {
//                string sFolder = System.IO.Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
//                string sManifestPath = sFolder + "\\extension.vsixmanifest";
//                string sBackupManifestPath = sFolder + "\\extension2019.vsixmanifest";
//                string sOtherManifestPath = sFolder + "\\extension2017.vsixmanifest";

//                string sPkgdef2019Path = sFolder + "\\BidsHelper2019.pkgdef";
//                string sPkgdef2019BackupPath = sFolder + "\\BidsHelper2019.pkgdef.bak";
//                string sPkgdef2017Path = sFolder + "\\BidsHelper2017.pkgdef";
//                string sPkgdef2017BackupPath = sFolder + "\\BidsHelper2017.pkgdef.bak";

//                string sDll2017Path = sFolder + "\\SQL2017\\BidsHelper2017.dll";

//                if (System.IO.File.Exists(sOtherManifestPath) && System.IO.File.Exists(sPkgdef2017BackupPath) && System.IO.File.Exists(sPkgdef2019Path))
//                {
//                    //backup the current SQL2019 manifest
//                    System.IO.File.Copy(sManifestPath, sBackupManifestPath, true);

//                    //copy SQL2017 manifest over the current manifest
//                    System.IO.File.Copy(sOtherManifestPath, sManifestPath, true);

//                    if (System.IO.File.Exists(sPkgdef2017Path))
//                        System.IO.File.Delete(sPkgdef2017BackupPath);
//                    else
//                        System.IO.File.Move(sPkgdef2017BackupPath, sPkgdef2017Path);

//                    if (System.IO.File.Exists(sPkgdef2019BackupPath))
//                        System.IO.File.Delete(sPkgdef2019Path);
//                    else
//                        System.IO.File.Move(sPkgdef2019Path, sPkgdef2019BackupPath);



//                    //VS2017 seems to use the registry after the first run to denote which DLL to launch
//                    //the 15.0* hive is special in that it is actually "C:\Users\<user>\AppData\Local\Microsoft\VisualStudio\15.0_31028247\privateregistry.bin"
//                    //if you want to view this in regedit then close all VS2017 and go to HKEY_LOCAL_MACHINE... File... Load Hive... choose that privateregistry.bin
//                    //remember to click on the new hive folder and do File... Unload Hive before trying to open VS2017
//                    Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\15.0_Config\Packages\{" + PackageGuidString + "}", true);
//                    if (regKey != null)
//                    {
//                        regKey.SetValue("CodeBase", sDll2017Path, Microsoft.Win32.RegistryValueKind.String);
//                        regKey.Close();
//                    }

//                    System.Windows.Forms.MessageBox.Show("You have SSDT for SQL Server " + VersionInfo.SqlServerFriendlyVersion + " installed. Please restart Visual Studio so BI Developer Extensions can reconfigure itself to work properly with that version of SSDT.", "BIDS Helper");
//                    return true;
//                }
//                else
//                {
//                    throw new Exception("You have SSDT for SQL Server " + VersionInfo.SqlServerFriendlyVersion + " installed but we couldn't find BIDS Helper 2017 files!");
//                }
//            }
//#elif SQL2017 && VS2017
//            string sVersion = VersionInfo.SqlServerVersion.ToString();
//            if (sVersion.StartsWith("15.")) //this BI Dev Extensions DLL is for SQL 2017 but you have SSDT for SQL2019 installed
//            {
//                string sFolder = System.IO.Directory.GetParent(System.IO.Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName).FullName;
//                string sManifestPath = sFolder + "\\extension.vsixmanifest";
//                string sBackupManifestPath = sFolder + "\\extension2017.vsixmanifest";
//                string sOtherManifestPath = sFolder + "\\extension2019.vsixmanifest";

//                string sPkgdef2019Path = sFolder + "\\BidsHelper2019.pkgdef";
//                string sPkgdef2019BackupPath = sFolder + "\\BidsHelper2019.pkgdef.bak";
//                string sPkgdef2017Path = sFolder + "\\BidsHelper2017.pkgdef";
//                string sPkgdef2017BackupPath = sFolder + "\\BidsHelper2017.pkgdef.bak";

//                string sDll2019Path = sFolder + "\\BidsHelper2019.dll";

//                if (System.IO.File.Exists(sOtherManifestPath) && System.IO.File.Exists(sPkgdef2019BackupPath) && System.IO.File.Exists(sPkgdef2017Path))
//                {
//                    //backup the current SQL2017 manifest
//                    System.IO.File.Copy(sManifestPath, sBackupManifestPath, true);

//                    //copy SQL2019 manifest over the current manifest
//                    System.IO.File.Copy(sOtherManifestPath, sManifestPath, true);

//                    if (System.IO.File.Exists(sPkgdef2019Path))
//                        System.IO.File.Delete(sPkgdef2019BackupPath);
//                    else
//                        System.IO.File.Move(sPkgdef2019BackupPath, sPkgdef2019Path);

//                    if (System.IO.File.Exists(sPkgdef2017BackupPath))
//                        System.IO.File.Delete(sPkgdef2017Path);
//                    else
//                        System.IO.File.Move(sPkgdef2017Path, sPkgdef2017BackupPath);

//                    //VS2017 seems to use the registry after the first run to denote which DLL to launch
//                    //the 15.0* hive is special in that it is actually "C:\Users\<user>\AppData\Local\Microsoft\VisualStudio\15.0_31028247\privateregistry.bin"
//                    //if you want to view this in regedit then close all VS2017 and go to HKEY_LOCAL_MACHINE... File... Load Hive... choose that privateregistry.bin
//                    //remember to click on the new hive folder and do File... Unload Hive before trying to open VS2017
//                    Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\15.0_Config\Packages\{" + PackageGuidString + "}", true);
//                    if (regKey != null)
//                    {
//                        regKey.SetValue("CodeBase", sDll2019Path, Microsoft.Win32.RegistryValueKind.String);
//                        regKey.Close();
//                    }

//                    System.Windows.Forms.MessageBox.Show("You have SSDT for SQL Server " + VersionInfo.SqlServerFriendlyVersion + " installed. Please restart Visual Studio so BI Developer Extensions can reconfigure itself to work properly with that version of SSDT.", "BIDS Helper");
//                    return true;
//                }
//                else
//                {
//                    throw new Exception("You have SSDT for SQL Server " + VersionInfo.SqlServerFriendlyVersion + " installed but we couldn't find BIDS Helper 2019 files!");
//                }
//            }
//#elif SQL2017 && !VS2017
//            string sVersion = VersionInfo.SqlServerVersion.ToString();
//            if (sVersion.StartsWith("13.")) //this DLL is for SQL 2017 but you have SSDT for SQL2016 installed
//            {
//                string sFolder = System.IO.Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
//                string sManifestPath = sFolder + "\\extension.vsixmanifest";
//                string sBackupManifestPath = sFolder + "\\extension2017.vsixmanifest";
//                string sOtherManifestPath = sFolder + "\\extension2016.vsixmanifest";

//                string sPkgdef2017Path = sFolder + "\\BidsHelper2017.pkgdef";
//                string sPkgdef2017BackupPath = sFolder + "\\BidsHelper2017.pkgdef.bak";
//                string sPkgdef2016Path = sFolder + "\\BidsHelper2016.pkgdef";
//                string sPkgdef2016BackupPath = sFolder + "\\BidsHelper2016.pkgdef.bak";

//                string sDll2016Path = sFolder + "\\SQL2016\\BidsHelper2016.dll";

//                if (System.IO.File.Exists(sOtherManifestPath) && System.IO.File.Exists(sPkgdef2016BackupPath) && System.IO.File.Exists(sPkgdef2017Path))
//                {
//                    //backup the current SQL2017 manifest
//                    System.IO.File.Copy(sManifestPath, sBackupManifestPath, true);

//                    //copy SQL2016 manifest over the current manifest
//                    System.IO.File.Copy(sOtherManifestPath, sManifestPath, true);

//                    if (System.IO.File.Exists(sPkgdef2016Path))
//                        System.IO.File.Delete(sPkgdef2016BackupPath);
//                    else
//                        System.IO.File.Move(sPkgdef2016BackupPath, sPkgdef2016Path);

//                    if (System.IO.File.Exists(sPkgdef2017BackupPath))
//                        System.IO.File.Delete(sPkgdef2017Path);
//                    else
//                        System.IO.File.Move(sPkgdef2017Path, sPkgdef2017BackupPath);

//                    //it looks like some earlier versions of VS2015 use the registry while newer versions of VS2015 (like Update 3) just use the vsixmanifest and pkgdef files?
//                    Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\14.0_Config\Packages\{" + PackageGuidString +"}", true);
//                    if (regKey != null) 
//                    {
//                        regKey.SetValue("CodeBase", sDll2016Path, Microsoft.Win32.RegistryValueKind.String);
//                        regKey.Close();
//                    }

//                    System.Windows.Forms.MessageBox.Show("You have SSDT for SQL Server " + VersionInfo.SqlServerFriendlyVersion + " installed. Please restart Visual Studio so BI Developer Extensions can reconfigure itself to work properly with that version of SSDT.", "BIDS Helper");
//                    return true;
//                }
//                else
//                {
//                    throw new Exception("You have SSDT for SQL Server " + VersionInfo.SqlServerFriendlyVersion + " installed but we couldn't find BIDS Helper 2016 files!");
//                }
//            }
//#elif SQL2016 && !VS2017
//            string sVersion = VersionInfo.SqlServerVersion.ToString();
//            if (sVersion.StartsWith("14.")) //this DLL is for SQL 2016 but you have SSDT for SQL2017 installed
//            {
//                string sFolder = System.IO.Directory.GetParent(System.IO.Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName).FullName;
//                string sManifestPath = sFolder + "\\extension.vsixmanifest";
//                string sBackupManifestPath = sFolder + "\\extension2016.vsixmanifest";
//                string sOtherManifestPath = sFolder + "\\extension2017.vsixmanifest";

//                string sPkgdef2017Path = sFolder + "\\BidsHelper2017.pkgdef";
//                string sPkgdef2017BackupPath = sFolder + "\\BidsHelper2017.pkgdef.bak";
//                string sPkgdef2016Path = sFolder + "\\BidsHelper2016.pkgdef";
//                string sPkgdef2016BackupPath = sFolder + "\\BidsHelper2016.pkgdef.bak";

//                string sDll2017Path = sFolder + "\\BidsHelper2017.dll";

//                if (System.IO.File.Exists(sOtherManifestPath) && System.IO.File.Exists(sPkgdef2017BackupPath) && System.IO.File.Exists(sPkgdef2016Path))
//                {
//                    //backup the current SQL2016 manifest
//                    System.IO.File.Copy(sManifestPath, sBackupManifestPath, true);

//                    //copy SQL2017 manifest over the current manifest
//                    System.IO.File.Copy(sOtherManifestPath, sManifestPath, true);

//                    if (System.IO.File.Exists(sPkgdef2017Path))
//                        System.IO.File.Delete(sPkgdef2017BackupPath);
//                    else
//                        System.IO.File.Move(sPkgdef2017BackupPath, sPkgdef2017Path);

//                    if (System.IO.File.Exists(sPkgdef2016BackupPath))
//                        System.IO.File.Delete(sPkgdef2016Path);
//                    else
//                        System.IO.File.Move(sPkgdef2016Path, sPkgdef2016BackupPath);

//                    //it looks like some earlier versions of VS2015 use the registry while newer versions of VS2015 (like Update 3) just use the vsixmanifest and pkgdef files?
//                    Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\14.0_Config\Packages\{" + PackageGuidString +"}", true);
//                    if (regKey != null)
//                    {
//                        regKey.SetValue("CodeBase", sDll2017Path, Microsoft.Win32.RegistryValueKind.String);
//                        regKey.Close();
//                    }

//                    System.Windows.Forms.MessageBox.Show("You have SSDT for SQL Server " + VersionInfo.SqlServerFriendlyVersion + " installed. Please restart Visual Studio so BI Developer Extensions can reconfigure itself to work properly with that version of SSDT.", "BIDS Helper");
//                    return true;
//                }
//                else
//                {
//                    throw new Exception("You have SSDT for SQL Server " + VersionInfo.SqlServerFriendlyVersion + " installed but we couldn't find BIDS Helper 2017 files!");
//                }
//            }
//#endif
//            return false;

//        }

        //private void RestartVisualStudio()
        //{
        //    System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        //    System.Diagnostics.Process newProcess = new System.Diagnostics.Process();
        //    newProcess.StartInfo = new System.Diagnostics.ProcessStartInfo {
        //        FileName = currentProcess.MainModule.FileName,
        //        ErrorDialog = true,
        //        UseShellExecute = true,
        //        Arguments = DTE2.CommandLineArguments
        //    };
        //    newProcess.Start();

        //    EnvDTE.Command command = DTE2.Commands.Item("File.Exit", -1);

        //    if ((command != null) && command.IsAvailable)
        //    {
        //        DTE2.ExecuteCommand("File.Exit", "");
        //    }
        //    else
        //    {
        //        DTE2.Quit();
        //    }

        //}


#if DENALI
        //this isn't necessary in VS2013 apparently
        System.Reflection.Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AssemblyResolve: " + args.Name);
                if (args.Name.StartsWith("Microsoft.AnalysisServices.VSHost,"))
                {
                    //this occurs in SSDTBI from SQL2012 in VS2012... apparently they added a .11 to the end of the assembly name
                    return Assembly.Load("Microsoft.AnalysisServices.VSHost.11, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                }
                else if (args.Name.StartsWith("Microsoft.AnalysisServices.MPFProjectBase,"))
                {
                    //this occurs in SSDTBI from SQL2012 in VS2012... apparently they added a .11 to the end of the assembly name
                    return Assembly.Load("Microsoft.AnalysisServices.MPFProjectBase.11, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Problem during AssemblyResolve in BIDS Helper:\r\n" + ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper");
                return null;
            }
        }
#else
        string _recursiveAssemblyResolveNameToSkip = null;
        System.Collections.Generic.List<string> _assemblyLoadsFailed = new System.Collections.Generic.List<string>();
        System.Collections.Generic.List<System.ResolveEventHandler> _microsoftEventHandlersToIgnoreErrors = new System.Collections.Generic.List<ResolveEventHandler>();
        AppDomain _defaultAppDomain = null;

        private static mscoree.ICorRuntimeHost GetCorRuntimeHost()
        {
            return (mscoree.ICorRuntimeHost)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("CB2F6723-AB3A-11D2-9C40-00C04FA30A3E")));
        }

        public AppDomain GetDefaultAppDomain()
        {
            IntPtr enumHandle = IntPtr.Zero;
            mscoree.ICorRuntimeHost host = GetCorRuntimeHost();
            try
            {
                host.EnumDomains(out enumHandle);

                object domain = null;
                while (true)
                {
                    host.NextDomain(enumHandle, out domain);

                    if (domain == null) break;

                    AppDomain appDomain = (AppDomain)domain;
                    if (appDomain.IsDefaultAppDomain()) 
                        return appDomain;
                }
                return null;
            }
            catch (Exception e)
            {
                Log.Debug("Caught error in Microsoft AssemblyResolve and skipped: " + e.Message);
                return null;
            }
            finally
            {
                host.CloseEnum(enumHandle);
                Marshal.ReleaseComObject(host);
            }
        }

        /// <summary>
        /// Only fires if an assembly fails to load. This gives us a chance to redirect to a DLL that does exist.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        System.Reflection.Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                //we removed the Microsoft AssemblyResolve event handler from the list and are now calling theirs at the top of ours
                //we do this to swallow an error due to a bug in their AssemblyResolve event handler as of May 2020
                foreach (System.ResolveEventHandler handler in _microsoftEventHandlersToIgnoreErrors)
                {
                    try
                    {
                        Assembly a = handler.Invoke(sender, args);
                        if (a != null)
                            return a;
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("Caught error in Microsoft AssemblyResolve and skipped: " + ex.Message);
                    }
                }

                if (_recursiveAssemblyResolveNameToSkip == args.Name)
                    return null; //skip recursion
                Log.Debug("AssemblyResolve: " + args.Name);
                DateTime dtStart = DateTime.Now;
                if (
                    args.Name.StartsWith("Microsoft.AnalysisServices")
                    || args.Name.ToLower().StartsWith("microsoft.sqlserver.")
                    || args.Name.StartsWith("Microsoft.ReportViewer.")
                    || args.Name.StartsWith("Microsoft.DataWarehouse")
                    || args.Name.StartsWith("Microsoft.DataTransformationServices.")
                )
                {
                    var assemblyname = new AssemblyName(args.Name);

                    //first see if this assembly is loaded already... if so, don't scan folders for an assembly... just reuse the previously loaded assembly
                    foreach (Assembly loadedAlready in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (loadedAlready.GetName().Name == assemblyname.Name
                            && loadedAlready.GetName().Version.Major == assemblyname.Version.Major)
                            //&& loadedAlready.GetName().Version.Minor == assemblyname.Version.Minor
                            //&& loadedAlready.GetName().Version.MinorRevision == assemblyname.Version.MinorRevision)
                            return loadedAlready;
                    }

                    //if (_defaultAppDomain == null)
                    //    _defaultAppDomain = GetDefaultAppDomain();
                    //foreach (Assembly loadedAlready in _defaultAppDomain.GetAssemblies())
                    //{
                    //    if (loadedAlready.GetName().Name == assemblyname.Name
                    //        && loadedAlready.GetName().Version.Major == assemblyname.Version.Major
                    //        && loadedAlready.GetName().Version.Minor == assemblyname.Version.Minor
                    //        && loadedAlready.GetName().Version.MinorRevision == assemblyname.Version.MinorRevision)
                    //        return loadedAlready;
                    //}

                    System.Collections.Generic.List<string> pathsToCheck = new System.Collections.Generic.List<string>();
                    var bidsHelperPath = new System.IO.FileInfo(typeof(BIDSHelperPackage).Assembly.Location);
                    pathsToCheck.Add(bidsHelperPath.DirectoryName + "\\");
                    if (SSASExtensionInstallPath != null) pathsToCheck.Add(SSASExtensionInstallPath);
                    if (SSISExtensionInstallPath != null) pathsToCheck.Add(SSISExtensionInstallPath);
                    if (SSRSExtensionInstallPath != null) pathsToCheck.Add(SSRSExtensionInstallPath);
                    if (BISharedExtensionInstallPath != null) pathsToCheck.Add(BISharedExtensionInstallPath);

                    //if (SSISExtensionInstallPath != null)
                    //{
                    //    foreach (string sPath in System.IO.Directory.GetFiles(SSISExtensionInstallPath, assemblyname.Name + ".dll", System.IO.SearchOption.AllDirectories))
                    //    {
                    //        if (!sPath.ToUpper().Contains("\\BISHARED\\")) continue;
                    //        var assembly = Assembly.Load(System.IO.File.ReadAllBytes(sPath));
                    //        if (assembly.GetName().Version.Major == assemblyname.Version.Major) //some SSIS subfolders have multiple versions of the assembly... make sure we match the major version number... it appears there may be an assembly redirect in operation, but this is probably safer
                    //        {
                    //            Log.Debug("AssemblyResolveSuccessSSIS-BIShared: " + args.Name + " to version " + assembly.GetName().Version.ToString() + " at " + sPath + " in " + DateTime.Now.Subtract(dtStart).TotalMilliseconds + "ms");
                    //            return assembly;
                    //        }
                    //    }
                    //}
                    foreach (string extensionfolder in pathsToCheck)
                    {
                        string sPath = extensionfolder + assemblyname.Name + ".dll";
                        if (System.IO.File.Exists(sPath))
                        {
                            var assembly = Assembly.LoadFile(sPath);
                            Log.Debug("AssemblyResolveSuccess: " + args.Name + " to version " + assembly.GetName().Version.ToString() + " at " + sPath + " in " + DateTime.Now.Subtract(dtStart).TotalMilliseconds + "ms");
                            return assembly;
                        }
                    }
                    if (SSISExtensionInstallPath != null)
                    {
                        foreach (string sPath in System.IO.Directory.GetFiles(SSISExtensionInstallPath, assemblyname.Name + ".dll", System.IO.SearchOption.AllDirectories))
                        {
                            //if (sPath.ToUpper().Contains("\\BISHARED\\")) continue;
                            var assembly = Assembly.Load(System.IO.File.ReadAllBytes(sPath));
                            if (assembly.GetName().Version.Major == assemblyname.Version.Major) //some SSIS subfolders have multiple versions of the assembly... make sure we match the major version number... it appears there may be an assembly redirect in operation, but this is probably safer
                            {
                                Log.Debug("AssemblyResolveSuccessSSIS: " + args.Name + " to version " + assembly.GetName().Version.ToString() + " at " + sPath + " in " + DateTime.Now.Subtract(dtStart).TotalMilliseconds + "ms");
                                return assembly;
                            }
                        }
                    }
                    Log.Debug("AssemblyResolveFail: " + args.Name + " in " + DateTime.Now.Subtract(dtStart).TotalMilliseconds + "ms");
                    return null;

                    //Version originalVersion = (Version)assemblyname.Version.Clone();
                    //for (int i = 0; i < 500; i++)
                    //{
                    //    for (int j = 0; j <= (i >= originalVersion.Minor && i <= originalVersion.Minor + 10 ? 5 : 0); j++)
                    //    {
                    //        assemblyname.Version = new Version(originalVersion.Major, i, j, 0);
                    //        string sAssemblyName = assemblyname.ToString();
                    //        if (_assemblyLoadsFailed.Contains(sAssemblyName)) continue;
                    //        try
                    //        {
                    //            _recursiveAssemblyResolveNameToSkip = sAssemblyName;
                    //            var assembly = Assembly.Load(assemblyname);
                    //            System.Diagnostics.Debug.WriteLine("AssemblyResolveSuccess: " + args.Name + " to " + assemblyname.Version + " in " + DateTime.Now.Subtract(dtStart).TotalMilliseconds + "ms");
                    //            return assembly;
                    //        }
                    //        catch
                    //        {
                    //            if (!_assemblyLoadsFailed.Contains(sAssemblyName))
                    //                _assemblyLoadsFailed.Add(sAssemblyName);
                    //            System.Diagnostics.Debug.WriteLine("AssemblyResolveTried: " + args.Name + " to " + assemblyname.Version + " in " + DateTime.Now.Subtract(dtStart).TotalMilliseconds + "ms");
                    //        }
                    //        finally
                    //        {
                    //            _recursiveAssemblyResolveNameToSkip = null;
                    //        }
                    //    }
                    //}
                    //System.Diagnostics.Debug.WriteLine("AssemblyResolveFail: " + args.Name + " in " + DateTime.Now.Subtract(dtStart).TotalMilliseconds + "ms");
                    //return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Problem during AssemblyResolve in BIDS Helper:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                return null;
            }
        }
#endif

        public int OnModeChange(DBGMODE mode)
        {
            mode = mode & ~DBGMODE.DBGMODE_EncMask;

            switch (mode)
            {
                case DBGMODE.DBGMODE_Design:
                    _ideMode = enumIDEMode.Design;
                    break;
                case DBGMODE.DBGMODE_Break:
                    _ideMode = enumIDEMode.Debug;
                    break;
                case DBGMODE.DBGMODE_Run:
                    _ideMode = enumIDEMode.Run;
                    break;
            }
            IdeModeChanged?.Invoke(this, _ideMode);
            return VSConstants.S_OK;
        }


        public event EventHandler<enumIDEMode> IdeModeChanged;

        public static Exception AddInLoadException = null;
        private uint debugEventCookie;
        public static Version SSISExtensionVersion = null;
        public static Version SSASExtensionVersion = null;
        public static Version SSRSExtensionVersion = null;
        public static string SSISExtensionInstallPath = null;
        public static string SSASExtensionInstallPath = null;
        public static string SSRSExtensionInstallPath = null;
        public static string BISharedExtensionInstallPath = null;

        internal System.IServiceProvider ServiceProvider { get { return (System.IServiceProvider)this; } }

        internal IVsDebugger DebuggerService
        { get { return (IVsDebugger)ServiceProvider.GetService(typeof(SVsShellDebugger)); } }

        public static string PluginRegistryPath(Type t)
        {
            return RegistryBasePath + "\\" + t.Name;
        }

        public static string RegistryBasePath
        {
            get { return REGISTRY_BASE_PATH; }
        }

        public static System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase> Plugins
        {
            get { return plugins; }
        }

        public StatusBar StatusBar { get; private set; }
        public EnvDTE80.DTE2 DTE2 { get; private set; }
        public IVsShell VsShell { get; private set; }

        public enumIDEMode IdeMode { get { return _ideMode; } }
#endregion

        public ILog Log { get; private set; }

        public void OutputString(string text)
        {
            const int VISIBLE = 1;
            const int DO_NOT_CLEAR_WITH_SOLUTION = 0;
            Guid guidBidsHelperDebugPane = new Guid("50C4E395-4E87-48BC-9BAC-7C4CD065F6E8");
            Guid guidPane = guidBidsHelperDebugPane;

            IVsOutputWindow outputWindow;
            IVsOutputWindowPane outputWindowPane = null;
            int hr;

            // Get the output window
            if (Microsoft.VisualStudio.Shell.ThreadHelper.CheckAccess())
                outputWindow = base.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            else
            {
                System.Threading.Tasks.Task<object> task = base.GetServiceAsync(typeof(SVsOutputWindow));
                task.Wait();
                outputWindow = task.Result as IVsOutputWindow;
            }

            // The General pane is not created by default. We must force its creation
            //if (guidPane == Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid)
            //{
            hr = outputWindow.CreatePane(guidPane, "BIDS Helper", VISIBLE, DO_NOT_CLEAR_WITH_SOLUTION);
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            //}

            // Get the pane
            hr = outputWindow.GetPane(guidPane, out outputWindowPane);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);

            // Output the text
            if (outputWindowPane != null)
            {
                outputWindowPane.Activate();
                outputWindowPane.OutputString(text);
            }
        }

    }
}
