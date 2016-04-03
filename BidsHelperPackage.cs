//------------------------------------------------------------------------------
// <copyright file="BidsHelperPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
//using System.ComponentModel.Design;
//using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
//using Microsoft.VisualStudio;
//using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
//using Microsoft.Win32;
using System.Reflection;
using BIDSHelper.Core;
using BidsHelper.Core;
using BIDSHelper.Core.VsIntegration;
//using BidsHelper.Core;

namespace BIDSHelper
{
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
    //[ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
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

        private static System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase> addins = new System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase>();
        
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


            string sAddInTypeName = string.Empty;
            try
            {
                //TODO - fix these commented lines

                //_applicationObject = (DTE2)application;
                //_addInInstance = (AddIn)addInInst;

                //_applicationObject.StatusBar.Text = "Loading BIDSHelper (" + this.GetType().Assembly.GetName().Version.ToString() + ")...";
                
                StatusBar = new Core.VsIntegration.StatusBar(this);
                //_debuggerEvents = _applicationObject.Events.DebuggerEvents;
                //_debuggerEvents.OnEnterBreakMode += new _dispDebuggerEvents_OnEnterBreakModeEventHandler(_debuggerEvents_OnEnterBreakMode);
                //_debuggerEvents.OnEnterDesignMode += new _dispDebuggerEvents_OnEnterDesignModeEventHandler(_debuggerEvents_OnEnterDesignMode);
                //_debuggerEvents.OnEnterRunMode += new _dispDebuggerEvents_OnEnterRunModeEventHandler(_debuggerEvents_OnEnterRunMode);
                DebuggerService.AdviseDebuggerEvents(this, out debugEventCookie);
                

                foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (typeof(BIDSHelperPluginBase).IsAssignableFrom(t)
                        && (!object.ReferenceEquals(t, typeof(BIDSHelperPluginBase)))
                        && (!t.IsAbstract))
                    {
                        sAddInTypeName = t.Name;
                        
                        BIDSHelperPluginBase feature;
                        Type[] @params = { typeof(Package)} ;
                        //System.Reflection.ConstructorInfo con;
                        //con = t.GetConstructor(@params);
                        var initMethod = t.GetMethod("Initialize",BindingFlags.Static | BindingFlags.Public);

                        if (initMethod == null)
                        {
                            System.Windows.Forms.MessageBox.Show("Problem loading type " + t.Name + ". No constructor found.");
                            continue;
                        }

                        var instanceProp = t.GetProperty("Instance");
                        initMethod.Invoke(null, new object[] { this });
                        feature = (BIDSHelperPluginBase)instanceProp.GetValue(null, null);

                        Plugins.Add(feature.FeatureName, feature);
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
                    //System.Windows.Forms.MessageBox.Show("Problem loading BIDS Helper: " + ex.Message + "\r\n" + ex.StackTrace);
                }
                else
                {
                    AddInLoadException = new Exception("Problem loading BIDS Helper. Problem type was " + sAddInTypeName + ": " + ex.Message + "\r\n" + ex.StackTrace, ex);
                    //System.Windows.Forms.MessageBox.Show("Problem loading BIDS Helper. Problem type was " + sAddInTypeName + ": " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            finally
            {
                //TODO - fix these commented lines
                //    _applicationObject.StatusBar.Clear();
            }




//            DeployMdxScript.Initialize(this);

        }

        public int OnModeChange(DBGMODE dbgmodeNew)
        {
            throw new NotImplementedException();
        }



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
            get { return addins; }
        }

        public StatusBar StatusBar { get; private set; }

        #endregion
    }
}
