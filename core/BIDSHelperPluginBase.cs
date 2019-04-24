extern alias asAlias;
extern alias rsAlias;
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
using System.Collections.Generic;
using System.Windows.Forms;

namespace BIDSHelper.Core
{

    public abstract class BIDSHelperPluginBase : IBIDSHelperPlugin
    {
        private const string BASE_NAME = "BIDSHelperPackage.";
        protected const string DefaultMessageBoxCaption = "BIDS Helper";
        private const string DefaultUrlFormat = "https://bideveloperextensions.github.io/features/{0}/";
        private bool isEnabled;
        private bool isEnabledCached = false;
        public static readonly Guid CommandSet = new Guid("bd8ea5c7-1cc4-490b-a7b8-8484dc5532e7");
        private IVsWindowFrame m_windowFrame = null;
        private UserControl m_userControl;
        private OleMenuCommand m_menuItem = null;
        private Guid m_ProjectKind = Guid.Empty;

        #region Constructor
        public BIDSHelperPluginBase(BIDSHelperPackage package)
        {
            Extensions = new List<string>();
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

        //public virtual bool DisplayCommand(UIHierarchyItem item) { return false; }



        /// <summary>
        /// This virtual method gives you a chance to override the displaying of a menu command
        /// to cater for any special pre-requisite of the command
        /// </summary>
        public virtual bool ShouldDisplayCommand() { return true; }

        public abstract void Exec();
        #endregion

        #region Properties
        protected List<string> Extensions { get; private set; }

        public enumIDEMode IdeMode { get { return package.IdeMode; } }

        public VsIntegration.StatusBar StatusBar { get; private set; }

        public Type AssociatedObjectType { get; private set; }

        #endregion

        //=================================================================================

        #region Dynamic Command Visibility
        internal bool DisplayCommandInternal(FileInfo file)
        {
            if (file == null) return false;
            return Extensions.Contains(file.Extension.ToLower());
        }

        internal bool DisplayCommandInternal()
        {
            if (Enabled)
            {

                if (Extensions.Count > 0)
                {
                    var f = GetSelectedFile();
                    return DisplayCommandInternal(f);
                }
                else if (m_ProjectKind != Guid.Empty)
                {
                    return DisplayCommandInternal(m_ProjectKind);
                }
                else if (AssociatedObjectType != null)
                {
                    return DisplayCommandInternal(AssociatedObjectType);
                }
                else {
                    package.Log.Verbose("Calling virtual ShouldDisplayCommand to see if we should be displaying this command " + this.GetType().Name);
                    try
                    {
                        return ShouldDisplayCommand();
                    }
                    catch (Exception ex)
                    {
                        this.package.Log.Exception("Error in base class " + this.GetType().Name + " ShouldDisplayCommand", ex);
                        return false;
                    }
                }
            }
            return false;
        }

        internal bool DisplayCommandInternal(Guid projectKind)
        {
            try
            {
                // TODO - do I need to add a ProjectKind overload to CreateContextMenu
                //if (ToolWindowVisible) return true;
                if (this.ApplicationObject.Solution == null) return false;
                foreach (EnvDTE.Project p in this.ApplicationObject.Solution.Projects)
                {
                    if (p.Kind == projectKind.ToString("B")) return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                this.package.Log.Exception("Error in base class " + this.GetType().Name + " DisplayCommandInternal(guid)", ex);
                return false;
            }
        }

        internal bool DisplayCommandInternal(Type objectType)
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                return hierItem.Object is ProjectItem && ((ProjectItem)hierItem.Object).Object != null && (((ProjectItem)hierItem.Object).Object.GetType() == objectType);
            }
            catch (Exception ex)
            {
                this.package.Log.Exception("Error in base class " + this.GetType().Name + " DisplayCommandInternal(type)", ex);
                return false;
            }
        }
        #endregion

        #region Options Page Properties
        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        /// <remarks>The feature category is used to organise features into SSIS, SSAS, SSRS or Common.</remarks>
        public abstract BIDSFeatureCategories FeatureCategory { get; }

        public abstract string ToolTip { get; }

        /// <summary>
        /// Gets the short name, the unique internal plug-in name
        /// </summary>
        /// <value>The short name.</value>
        /// <remarks>The short name uniquely identiofies the plug-in within BIDS Helper. It is used to derive the full name, which is unique within all Visual Studio commands.</remarks>
        public virtual string ShortName { get { return this.GetType().Name; } }

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
        //public abstract string ButtonText
        //{
        //    get;
        //}

        /// <summary>
        /// Gets the feature name as displayed in the enabled features list, previously known as the friendly name.
        /// </summary>
        /// <value>The feature name.</value>
        /// <remarks>
        ///     If not overridden then the <see cref="ButtonText"/> will be used instead.
        ///     The feature name is the default page title used for by the HelpUrl.
        ///     Using a friendly name accross multiple plug-ins allows you to group commands (each a plug-in) together. The BIML Package Generator feature includes 4 commandfs/plug-ins, Add New File, Expand, Validate and Help.
        /// </remarks>
        public abstract string FeatureName { get; }

        #endregion

        #region Enable / Disable
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



        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        #endregion



        /// <summary>
        /// Gets the CodePlex help page URL.
        /// </summary>
        /// <param name="wikiTitle">The wiki page title.</param>
        /// <returns>The full help page URL.</returns>
        /// <remarks>Used by default implementation of HelpUrl, as well as being 
        /// available for derived classes that need to override that property.</remarks>
        protected string GetCodePlexHelpUrl(string wikiTitle)
        {
            return string.Format(CultureInfo.InvariantCulture, DefaultUrlFormat, wikiTitle.Replace(" ",""));
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
                System.Windows.MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, DefaultMessageBoxCaption);
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

        //public static string StaticPluginRegistryPath
        //{
        //    get
        //    {
        //        return BIDSHelperPackage.PluginRegistryPath(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType);
        //    }
        //}

        /// <summary>
        /// Gets the fully qualified name of the plug-in.
        /// </summary>
        /// <value>The full name.</value>
        /// <remarks>The full name is built from the short name, and is used to unqiuely identify a plug-in, e.g. BIDSHelper.Connect.MyCleverPlugin.</remarks>
        public string FullName
        {
            get { return BASE_NAME + this.GetType().Name; } //this.ShortName; }
        }






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
        /// <summary>
        /// This method hooks up the click of the associated menu item in the 
        /// BIDSHelperPackage.vsct file with the Exec method in this plugin.
        /// When only passing a CommandId you need to override ShouldDisplayCommand
        /// if you need dynamic visibility for the menu command.
        /// </summary>
        /// <param name="commandId">The ordinal of this commandID has to match the id in the BIDSHelperPackage.vsct file</param>
        public void CreateContextMenu(CommandList commandId)
        {
            CreateMenu(CommandSet, (int)commandId);
        }

        /// <summary>
        /// This method hooks up the click of the associated menu item in the 
        /// BIDSHelperPackage.vsct file with the Exec method in this plugin.
        /// </summary>
        /// <param name="commandId">The ordinal of this commandID has to match the id in the BIDSHelperPackage.vsct file</param>
        /// <param name="extension">This menu item will only display for files with this extension (eg ".cube")</param>
        public void CreateContextMenu(CommandList commandId, string extension)
        {
            if (!extension.StartsWith(".")) throw new ArgumentException("the extension argument to CreateContextMenu must start with a period (.)"); 
            Extensions.Add(extension);
            CreateMenu(CommandSet, (int)commandId);
        }

        /// <summary>
        /// This method hooks up the click of the associated menu item in the 
        /// BIDSHelperPackage.vsct file with the Exec method in this plugin.
        /// </summary>
        /// <param name="commandId">The ordinal of this commandID has to match the id in the BIDSHelperPackage.vsct file</param>
        /// <param name="extension">This menu item will only display if projects matching the guid from BIDSHelperProjectKinds is in the current solution</param>
        public void CreateContextMenu(CommandList commandId, Guid projectKind)
        {
            m_ProjectKind = projectKind;
            CreateMenu(CommandSet, (int)commandId);
        }

        /// <summary>
        /// This method hooks up the click of the associated menu item in the 
        /// BIDSHelperPackage.vsct file with the Exec method in this plugin.
        /// </summary>
        /// <param name="commandId">The ordinal of this commandID has to match the id in the BIDSHelperPackage.vsct file</param>
        /// <param name="extension">This menu item will only display for files that match one of the entries in this array of extensions (eg ".cube")</param>
        public void CreateContextMenu(CommandList commandId, string[] extensions)
        {
            Extensions.AddRange(extensions);
            CreateMenu(CommandSet, (int)commandId);
        }

        /// <summary>
        /// This method hooks up the click of the associated menu item in the 
        /// BIDSHelperPackage.vsct file with the Exec method in this plugin.
        /// </summary>
        /// <param name="commandId">The ordinal of this commandID has to match the id in the BIDSHelperPackage.vsct file</param>
        /// <param name="extension">This menu item will only display for files where the associated object is of this type</param>
        public void CreateContextMenu(CommandList commandId, Type associatedObjectType)
        {
            AssociatedObjectType = associatedObjectType;
            CreateMenu(CommandSet, (int)commandId);
        }

        internal void CreateMenu(Guid commandSet, int commandId)
        {
            OleMenuCommandService commandService = this.MenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(commandSet, commandId);
                //var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);

                // AND REPLACE IT WITH A DIFFERENT TYPE
                m_menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                m_menuItem.BeforeQueryStatus += OnMenuBeforeQueryStatus;

                commandService.AddCommand(m_menuItem);
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

                //var selectedFileInfo = GetSelectedFile();

                //// then check if the file is named '.cube'
                //bool showMenu = DisplayCommand(selectedFileInfo);

                bool showMenu = DisplayCommandInternal();

                // if not leave the menu hidden
                if (!showMenu) return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                this.Exec();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        #endregion

        #region ToolWindow Methods

        protected bool Checked { get; set; }

        protected bool ToolWindowVisible
        {
            get {
                if (m_windowFrame == null) return false;
                return m_windowFrame.IsVisible() != 0;
            }
            set {
                if (m_windowFrame == null) return; // TODO - show we throw and error or even create the window here??
                if (value) ShowToolWindow();
                else HideToolWindow();
            }
        }

        private void HideToolWindow() {
            if (m_windowFrame == null) return;
            m_windowFrame.Hide();
            m_menuItem.Checked = false;
        }

        private void ShowToolWindow()
        {
            
            if (m_windowFrame == null) 
            {
                //TODO - throw error here??
                return;
            }
            m_windowFrame.Show();
            m_menuItem.Checked = true;
        }

        public void SetToolWindowIcon(int icon)
        {
            // TODO - set tool window icon
            //m_windowFrame.SetProperty(__VSFPROPID.VSFPROPID_BitmapResource,  )
        }

        protected UserControl ToolWindowUserControl { get { return m_userControl; } }

        protected void  CreateToolWindow(string caption, Guid guid, Type controlType)
        {
            const int TOOL_WINDOW_INSTANCE_ID = 0; // Single-instance toolwindow

            IVsUIShell uiShell;
            //Guid toolWindowPersistenceGuid;
            Guid guidNull = Guid.Empty;
            int[] position = new int[1];
            int result;
            IVsWindowFrame windowFrame = null;
            try {
                uiShell = (IVsUIShell)package.ServiceProvider.GetService(typeof(SVsUIShell));

                //toolWindowPersistenceGuid = new Guid(guid);

                m_userControl = (UserControl)Activator.CreateInstance(controlType);

                //m_windowFrame.SetProperty(__VSFPROPID.VSFPROPID_BitmapResource, )

                // TODO: Initialize m_userControl if required adding a method like:
                //    internal void Initialize(VSPackageToolWindowPackage package)
                // and pass this instance of the package:
                //    m_userControl.Initialize(this);

                result = uiShell.CreateToolWindow((uint)__VSCREATETOOLWIN.CTW_fInitNew,
                      TOOL_WINDOW_INSTANCE_ID, m_userControl, ref guidNull, ref guid,
                      ref guidNull, null, caption, position, out windowFrame);

                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(result);

                m_windowFrame = windowFrame;
            }
            catch (Exception ex)
            {
                this.package.Log.Exception("Error Creating ToolWindow", ex);
            }
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

        #region Solution Explorer Helpers
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
        protected Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt GetSelectedProjectReference()
        {
            return GetSelectedProjectReference(false);   
        }
        //protected asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt 
        protected EnvDTE.Project GetSelectedProjectReferenceAS()
        {
            try
            {
                return GetSelectedProjectReferenceAS(false);
            }
            catch (Exception ex)
            {
                this.package.Log.Exception("Error in " + this.GetType().Name + " GetSelectedProjectReferenceAS", ex);
                return null;
            }
        }
        //protected rsAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt 
        protected EnvDTE.Project GetSelectedProjectReferenceRS()
        {
            try
            {
                return GetSelectedProjectReferenceRS(false);
            }
            catch (Exception ex)
            {
                this.package.Log.Exception("Error in " + this.GetType().Name + " GetSelectedProjectReferenceAS", ex);
                return null;
            }
        }

        protected Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt GetSelectedProjectReference(bool onlyIfFileNotSelected)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
            Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt proj = hierItem.Object as Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;

            // if a project is in a folder the UIHierarchy object appears to return a project 
            // wrapped in a ProjectItem so we need to unwrap it to get to the project.
            if (proj == null && hierItem.Object is ProjectItem)
            {
                ProjectItem pi = (ProjectItem)hierItem.Object;
                if ((pi.Object as Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt) != null)
                {
                    proj = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)pi.Object;
                }
                else if (pi is Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)
                {
                    proj = pi as Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
                }
                else if (!onlyIfFileNotSelected)
                {
                    proj = pi.ContainingProject as Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
                }
                
            }
            return proj;
        }
        //protected asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt 
        protected EnvDTE.Project GetSelectedProjectReferenceAS(bool onlyIfFileNotSelected)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
            asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt proj = hierItem.Object as asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;

            // if a project is in a folder the UIHierarchy object appears to return a project 
            // wrapped in a ProjectItem so we need to unwrap it to get to the project.
            if (proj == null && hierItem.Object is ProjectItem)
            {
                ProjectItem pi = (ProjectItem)hierItem.Object;
                if ((pi.Object as asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt) != null)
                {
                    proj = (asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)pi.Object;
                }
                else if (pi is asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)
                {
                    proj = pi as asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
                }
                else if (!onlyIfFileNotSelected)
                {
                    proj = pi.ContainingProject as asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
                }

            }
            return proj;
        }
        protected rsAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt GetSelectedProjectReferenceRS(bool onlyIfFileNotSelected)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
            rsAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt proj = hierItem.Object as rsAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;

            // if a project is in a folder the UIHierarchy object appears to return a project 
            // wrapped in a ProjectItem so we need to unwrap it to get to the project.
            if (proj == null && hierItem.Object is ProjectItem)
            {
                ProjectItem pi = (ProjectItem)hierItem.Object;
                if ((pi.Object as rsAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt) != null)
                {
                    proj = (rsAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)pi.Object;
                }
                else if (pi is rsAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)
                {
                    proj = pi as rsAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
                }
                else if (!onlyIfFileNotSelected)
                {
                    proj = pi.ContainingProject as rsAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
                }

            }
            return proj;
        }

        public FileInfo GetSelectedFile()
        {
            IVsHierarchy hierarchy;
            uint itemId;
            string itemFullPath;
            if (!IsSingleProjectItemSelection(out hierarchy, out itemId)) return null;
            ((IVsProject)hierarchy).GetMkDocument(itemId, out itemFullPath);

            var i = GetSelectedProjectItem();

            return new FileInfo(itemFullPath);
        }
        #endregion

        #region Static Methods

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
        #endregion
    }
}
