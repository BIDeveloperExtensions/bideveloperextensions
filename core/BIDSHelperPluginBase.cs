using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project;


namespace BIDSHelper.Core
{

    public abstract class BIDSHelperPluginBase:IBIDSHelperPlugin
    {
        private const string BASE_NAME = "BIDSHelperPackage.";
        private const string DefaultMessageBoxCaption = "BIDS Helper";
        private const string DefaultUrlFormat = "http://bidshelper.codeplex.com/wikipage?title={0}";
        private bool isEnabled;
        private bool isEnabledCached = false;
        public static readonly Guid CommandSet = new Guid("bd8ea5c7-1cc4-490b-a7b8-8484dc5532e7");

        #region Constructor
        public BIDSHelperPluginBase(BIDSHelperPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            this.package = (BIDSHelperPackage)package;
            StatusBar = package.StatusBar;
            if (Enabled)
            {
                OnEnable();
            }
        }
        #endregion


        #region Virtual/Abstract Methods
        //=================================================================================

        public virtual bool DisplayCommand(UIHierarchyItem item) { return false; }

        public virtual bool DisplayCommand(FileInfo file)
        { return string.Compare( file.Extension ,Extension, true) == 0 ;
        }

        public bool DisplayCommand() {
            var f = GetSelectedFile();
            return DisplayCommand(f);
        }
        public abstract void Exec();
        #endregion

        protected string Extension { get; set; }

        //=================================================================================

        #region Options Page Properties
        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        /// <remarks>The feature category is used to organise features into SSIS, SSAS, SSRS or Common.</remarks>
        public abstract BIDSFeatureCategories FeatureCategory
        {
            get;
        }

        public abstract string ToolTip
        {
            get;
        }

        /// <summary>
        /// Gets the short name, the unique internal plug-in name
        /// </summary>
        /// <value>The short name.</value>
        /// <remarks>The short name uniquely identiofies the plug-in within BIDS Helper. It is used to derive the full name, which is unique within all Visual Studio commands.</remarks>
        public abstract string ShortName
        {
            get;
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The feature description.</value>
        /// <remarks>If not overridden then the <see cref="ToolTip"/> will be used instead. Multiple plug-ins can form one feature. The description of teh last plug-in to be enumerated will take precedence, consider using a base class to tie a feature together e.g. <see cref="BIDSHelper.SSIS.Biml.BimlFeaturePluginBase"/>.</remarks>
        public virtual string FeatureDescription
        {
            get { return this.ToolTip; }
        }

        /// <summary>
        /// Gets the Url of the online help page for this plug-in.
        /// </summary>
        /// <value>The help page Url.</value>
        /// <remarks>If no help is appropriate return null.</remarks>
        public virtual string HelpUrl
        {
            // Default implementation of Help Url using FriendlyName. 
            // Override this property if you need a different value
            get { return this.GetCodePlexHelpUrl(this.FeatureName); }
        }

        /// <summary>
        /// Gets the button or command text, as displayed on the menu button.
        /// </summary>
        /// <value>The button text.</value>
        /// <remarks>This is the first level of friendly naming.</remarks>
        public abstract string ButtonText
        {
            get;
        }
        /// <summary>
        /// Gets the feature name as displayed in the enabled features list, previously known as the friendly name.
        /// </summary>
        /// <value>The feature name.</value>
        /// <remarks>
        ///     If not overridden then the <see cref="ButtonText"/> will be used instead.
        ///     The feature name is the default page title used for by the HelpUrl.
        ///     Using a friendly name accross multiple plug-ins allows you to group commands (each a plug-in) together. The BIML Package Generator feature includes 4 commandfs/plug-ins, Add New File, Expand, Validate and Help.
        /// </remarks>
        public virtual string FeatureName
        {
            get { return this.ButtonText; }
        }

        #endregion

        public bool Enabled
        {
            get
            {
                if (!isEnabledCached)
                {
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(this.PluginRegistryPath);
                    isEnabled = ((int)regKey.GetValue("Enabled", 1) == 1) ? true : false;
                    regKey.Close();
                    isEnabledCached = true;
                }
                return isEnabled;
            }

            set
            {
                // if the setting is being changed
                if (value != Enabled)
                {

                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(this.PluginRegistryPath);
                    isEnabled = value;

                    if (isEnabled)
                    {
                        // the default state is enabled so we can remove the Enabled key
                        regKey.DeleteValue("Enabled");
                        regKey.Close();
                        // TODO - is this needed ??
                        OnEnable();
                    }
                    else
                    {
                        // set the enabled property to 0
                        regKey.SetValue("Enabled", isEnabled, RegistryValueKind.DWord);
                        regKey.Close();
                        // TODO - is this needed ??
                        OnDisable();
                    }

                }
            }
        }

        public enumIDEMode IdeMode
        {
            get
            {
                return package.IdeMode;
            }
        }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }




        /// <summary>
        /// Gets the CodePlex help page URL.
        /// </summary>
        /// <param name="wikiTitle">The wiki page title.</param>
        /// <returns>The full help page URL.</returns>
        /// <remarks>Used by default implementation of HelpUrl, as well as being 
        /// available for derived classes that need to override that property.</remarks>
        protected string GetCodePlexHelpUrl(string wikiTitle)
        {
            return string.Format(CultureInfo.InvariantCulture, DefaultUrlFormat, wikiTitle);
        }


        
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance. 
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.FeatureName;
        }

        /// <summary>
        /// Opens a URL, using Internet Explorer
        /// </summary>
        /// <param name="url">The URL to open.</param>
        protected static void OpenUrl(string url)
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

        public static string BaseName
        {
            get { return BASE_NAME; }
        }

        public string CommandName
        {
            get { return BASE_NAME + this.GetType().Name; }
        }

        public string PluginRegistryPath
        {
            get { return BIDSHelperPackage.PluginRegistryPath(this.GetType()); }
        }

        /// <summary>
        /// Gets the fully qualified name of the plug-in.
        /// </summary>
        /// <value>The full name.</value>
        /// <remarks>The full name is built from the short name, and is used to unqiuely identify a plug-in, e.g. BIDSHelper.Connect.MyCleverPlugin.</remarks>
        public string FullName
        {
            get { return BASE_NAME + this.GetType().Name; } //this.ShortName; }
        }


        public VsIntegration.StatusBar StatusBar { get; private set;}

        

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        internal readonly BIDSHelperPackage package;

        #region Service References
        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        internal IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }


        internal OleMenuCommandService MenuCommandService
        {
            get
            {
                return this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            }
        }

        protected IVsShell VSShellService { get { return package.VsShell; } }

        protected DTE2 DTE2Service { get { return package.DTE2; } }
        
        #endregion

        protected string RegistryRoot
        {
            get {
                object regRoot;
                VSShellService.GetProperty((int)__VSSPROPID.VSSPROPID_VirtualRegistryRoot, out regRoot);
                return (string)regRoot;
                }
        }

        #region Menu Methods
        public void CreateMenu(Guid commandSet, int commandId)
        {
            OleMenuCommandService commandService = this.MenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(commandSet, commandId);
                //var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);

                // AND REPLACE IT WITH A DIFFERENT TYPE
                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += OnMenuBeforeQueryStatus;

                commandService.AddCommand(menuItem);
            }
        }

        protected virtual void OnMenuBeforeQueryStatus(object sender, EventArgs e)
        {

            // get the menu that fired the event
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                // leave the menu invisible and disabled if the current feature is not enabled
                if (!Enabled) return;

                var selectedFileInfo = GetSelectedFile();

                // then check if the file is named '.cube'
                bool showMenu = DisplayCommand(selectedFileInfo);

                // if not leave the menu hidden
                if (!showMenu) return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            this.Exec();
        }
#endregion

        internal DTE2 ApplicationObject
        {
            get { return  Package.GetGlobalService(typeof(DTE)) as DTE2; }
        }

        internal WindowEvents GetWindowEvents()
        {
            return package.DTE2.Events.WindowEvents;
        }


        public static bool IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out uint itemid)
        {
            hierarchy = null;
            itemid = VSConstants.VSITEMID_NIL;
            int hr = VSConstants.S_OK;

            var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (monitorSelection == null || solution == null)
            {
                return false;
            }

            IVsMultiItemSelect multiItemSelect = null;
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainerPtr = IntPtr.Zero;

            try
            {
                hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);

                if (ErrorHandler.Failed(hr) || hierarchyPtr == IntPtr.Zero || itemid == VSConstants.VSITEMID_NIL)
                {
                    // there is no selection
                    return false;
                }

                // multiple items are selected
                if (multiItemSelect != null) return false;

                // there is a hierarchy root node selected, thus it is not a single item inside a project

                if (itemid == VSConstants.VSITEMID_ROOT) return false;

                var tmp = Marshal.GetObjectForIUnknown(hierarchyPtr);
            
                var tmp2 = (FileProjectHierarchy)tmp;

                hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
                if (hierarchy == null) return false;

                Guid guidProjectID = Guid.Empty;

                if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out guidProjectID)))
                {
                    return false; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid
                }

                // if we got this far then there is a single project item selected
                return true;
            }
            finally
            {
                if (selectionContainerPtr != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainerPtr);
                }

                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
            }
        }

        public static IVsHierarchy GetSelectedProjectItem()
        {
            IVsHierarchy hierarchy = null;
            uint itemid = VSConstants.VSITEMID_NIL;
            int hr = VSConstants.S_OK;

            var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (monitorSelection == null || solution == null)
            {
                return null;
            }

            IVsMultiItemSelect multiItemSelect = null;
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainerPtr = IntPtr.Zero;

            try
            {
                hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);

                if (ErrorHandler.Failed(hr) || hierarchyPtr == IntPtr.Zero || itemid == VSConstants.VSITEMID_NIL)
                {
                    // there is no selection
                    return null;
                }

                // multiple items are selected
                if (multiItemSelect != null) return null;

                // there is a hierarchy root node selected, thus it is not a single item inside a project

                if (itemid == VSConstants.VSITEMID_ROOT) return null;

                hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
                if (hierarchy == null) return null;

                Guid guidProjectID = Guid.Empty;

                if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out guidProjectID)))
                {
                    return null; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid
                }

                // if we got this far then there is a single project item selected
                return hierarchy;
            }
            finally
            {
                if (selectionContainerPtr != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainerPtr);
                }

                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
            }
        }
        public FileInfo GetSelectedFile()
        {
            IVsHierarchy hierarchy;
            uint itemId;
            string itemFullPath;
            if (!IsSingleProjectItemSelection(out hierarchy, out itemId)) return null;
            ((IVsProject)hierarchy).GetMkDocument(itemId, out itemFullPath);
            
            return new FileInfo(itemFullPath);
        }

        public static Type GetPrivateType(Type publicTypeInSameAssembly, string FullName)
        {
            foreach (Type t in System.Reflection.Assembly.GetAssembly(publicTypeInSameAssembly).GetTypes())
            {
                if (t.FullName == FullName)
                {
                    return t;
                }
            }
            return null;
        }
    }
}
