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
using BidsHelper.Core;
using BIDSHelper.Core.VsIntegration;
using System.Linq;
using Microsoft.VisualStudio;
using BIDSHelper.Core.Logger;

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
    [ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]
    //[ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", VersionInfo.Version, IconResourceID = 400)] // Info on this package for Help/About
    [Guid(BIDSHelperPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(BIDSHelperOptionsFeatures), "BIDS Helper", "Features", 0, 0, true)]
    //[ProvideOptionPage(typeof(BIDSHelperOptionsPreferences), "BIDS Helper", "Preferences", 0, 0, true)]
    [ProvideOptionPage(typeof(BIDSHelperOptionsVersion), "BIDS Helper", "Version", 0, 0, true)]
    public sealed class BIDSHelperPackage : Package, IVsDebuggerEvents
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
        protected override void Initialize()
        {
            base.Initialize();

#if DEBUG
            Log = new Core.Logger.OutputLogger(this);
            Log.LogLevel = LogLevels.Verbose;
#else
            Logger = new Core.Logger.NullLogger();
#endif
            Log.Info("BIDSHelper Package Initialize Starting");
            string sAddInTypeName = string.Empty;
            try
            {
                StatusBar = new Core.VsIntegration.StatusBar(this);
                StatusBar.Text = "Loading BIDSHelper (" + this.GetType().Assembly.GetName().Version.ToString() + ")...";
                VsShell = (IVsShell)this.GetService(typeof(SVsShell));
                DTE2 = this.GetService(typeof(Microsoft.VisualStudio.Shell.Interop.SDTE)) as EnvDTE80.DTE2;

                DebuggerService.AdviseDebuggerEvents(this, out debugEventCookie);

                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (//typeof(IBIDSHelperPlugin).IsAssignableFrom(t.GetType())
                        t.GetInterfaces().Contains(typeof(IBIDSHelperPlugin))
                        && (!object.ReferenceEquals(t, typeof(IBIDSHelperPlugin)))
                        && (!t.IsAbstract))
                    {
                        sAddInTypeName = t.Name;
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
                        feature = (BIDSHelperPluginBase)con.Invoke(new object[] { this });
                        Plugins.Add(feature.FullName, feature);
                    }
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
                StatusBar.Clear();
            }

        }


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
            outputWindow = base.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;

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
