using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BIDSHelper
{
    public abstract class BIDSHelperPluginBase: IDisposable
    {


        private const string BASE_NAME = "BIDSHelper.Connect.";
        private Command pluginCmd;
        static CommandBarPopup toolsCommandBarPopup;
        private DTE2 appObj;
        private AddIn addIn;
        private const int UNKNOWN_CMD_ID = -1;
        private Connect addinCore;
        private bool isEnabled;
        private bool isEnabledCached = false;

        #region "Constructors"
        public BIDSHelperPluginBase(Connect con, DTE2 appObject, AddIn addinInstance)
        {
            addinCore = con;
            appObj = appObject;
            addIn = addinInstance;
            if (Enabled)
            {
                OnEnable();
            }
        }

        public string CommandName
        {
            get { return BASE_NAME + this.GetType().Name; }
        }

        public static string BaseName
        {
            get   { return BASE_NAME; }
        }

        public virtual void OnEnable()
        {
            AddCommand();
        }

        public virtual void OnDisable()
        {
            DeleteCommand();
        }

        
        public bool Enabled
        {
            get {
                if (!isEnabledCached)
                {
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PluginRegistryPath);
                    isEnabled = ((int)regKey.GetValue("Enabled", 1) == 1) ? true : false;
                    regKey.Close();
                    isEnabledCached = true;
                }
                return isEnabled;
            }

            set {
                // if the setting is being changed
                if (value != Enabled)
                {

                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PluginRegistryPath);
                    isEnabled = value;

                    if (isEnabled)
                    {
                        // the default state is enabled so we can remove the Enabled key
                        regKey.DeleteValue("Enabled");
                        regKey.Close();
                        OnEnable();
                    }
                    else
                    {
                        // set the enabled property to 0
                        regKey.SetValue("Enabled", isEnabled, RegistryValueKind.DWord);
                        regKey.Close();
                        OnDisable();
                    }
                    
                }
            }
        }

        public Connect AddinCore
        {
            set { addinCore = value; }
            get { return addinCore; }
        }

        public enumIDEMode IdeMode
        {
            get { 
                return addinCore.IdeMode; 
            }
        }

        public BIDSHelperPluginBase()
        {

        }
        #endregion

        #region "Helper Functions"


        public void AddCommand()
        {
            try
            {
                if (this.MenuName == "") return;

                Command cmdTmp;
                CommandBars cmdBars;
                cmdBars = (CommandBars)appObj.CommandBars;
                // Check any old versions of the command are not still hanging around
                try
                {
                    cmdTmp = appObj.Commands.Item(this.CommandName, UNKNOWN_CMD_ID);
                    cmdTmp.Delete();
                }
                catch { }

                // this is an empty array for passing into the AddNamedCommand method
                object[] contextUIGUIDs = null;
                
                cmdTmp = appObj.Commands.AddNamedCommand(
                            this.addIn, 
                            this.GetType().Name,
                            this.ButtonText,
                            this.ToolTip,
                            true,
                            this.Bitmap,
                            ref contextUIGUIDs,
                            (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled);

                foreach (string sMenuName in this.MenuName.Split(','))
                {
                    CommandBar pluginCmdBar = cmdBars[sMenuName];
                    if (pluginCmdBar == null)
                    {
                        System.Windows.Forms.MessageBox.Show("Cannot get the " + this.MenuName + " menubar");
                    }
                    else
                    {
                        pluginCmd = cmdTmp;

                        CommandBarButton btn;
                        if (sMenuName == "Tools")
                        {
                            if (toolsCommandBarPopup == null)
                            {
                                toolsCommandBarPopup = (CommandBarPopup)pluginCmdBar.Controls.Add(MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing, 1, true);
                                toolsCommandBarPopup.CommandBar.Name = "BIDSHelperToolsCommandBarPopup";
                                toolsCommandBarPopup.Caption = "BIDS Helper";
                            }
                            btn = pluginCmd.AddControl(toolsCommandBarPopup.CommandBar, 1) as CommandBarButton;
                            SetCustomIcon(btn);
                            toolsCommandBarPopup.Visible = true;
                        }
                        else if (AddCommandToMultipleMenus)
                        {
                            foreach (CommandBar bar in (CommandBars)(appObj.CommandBars))
                            {
                                if (bar.Name == sMenuName)
                                {
                                    if (!ShouldPositionAtEnd)
                                    {
                                        btn = pluginCmd.AddControl(bar, 1) as CommandBarButton;
                                    }
                                    else
                                    {
                                        btn = pluginCmd.AddControl(bar, bar.Controls.Count - 1) as CommandBarButton;
                                    }
                                    SetCustomIcon(btn);
                                }
                            }
                        }
                        else
                        {
                            if (!ShouldPositionAtEnd)
                            {
                                btn = pluginCmd.AddControl(pluginCmdBar, 1) as CommandBarButton;
                            }
                            else
                            {
                                btn = pluginCmd.AddControl(pluginCmdBar, pluginCmdBar.Controls.Count - 1) as CommandBarButton;
                            }
                            SetCustomIcon(btn);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show("Problem registering " + this.FullName + " command: " + e.Message);
            }

        }

        private void SetCustomIcon(CommandBarButton btn)
        {
            if (btn != null && this.CustomMenuIcon != null)
            {
                System.Drawing.Bitmap bmp = this.CustomMenuIcon.ToBitmap();
                btn.Picture = (stdole.StdPicture)ImageToPictureDispConverter.GetIPictureDispFromImage(bmp);
                btn.Mask = (stdole.StdPicture)ImageToPictureDispConverter.GetMaskIPictureDispFromBitmap(bmp);
                bmp.Dispose();
            }
        }

        public WindowEvents GetWindowEvents()
        {
            return appObj.Events.get_WindowEvents(null);
        }

        /// <summary>
        /// Deletes the plugin command
        /// </summary>
        public void DeleteCommand()
        {
            try
            {
                if ((pluginCmd != null))
                {
                    pluginCmd.Delete();
                }
                if (toolsCommandBarPopup != null)
                {
                    if (toolsCommandBarPopup.Controls.Count == 0)
                    {
                        toolsCommandBarPopup.Delete(true);
                    }
                }
            }
            catch
            {
                // we are exiting here so we just swallow any exception, because most likely VS.Net is shutting down too.
            }
        }

        public EnvDTE.vsCommandStatus QueryStatus(UIHierarchyItem item)
        {
            //Dynamically enable & disable the command. If the selected file name is File1.cs, then make the command visible.
            if (this.DisplayCommand(item))
            {
                if (this.Checked) //enabled and checked
                    return (vsCommandStatus)vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusLatched;
                else //enabled and unchecked
                    return (vsCommandStatus)vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported;
            }
            else
            {
                //\\ disabled
                return (vsCommandStatus)vsCommandStatus.vsCommandStatusUnsupported | vsCommandStatus.vsCommandStatusInvisible;
            }
        }

        public string PluginRegistryPath
        {
            get { return addinCore.PluginRegistryPath(this.GetType()); }
        }

        #endregion

        # region "Public Properties"
        public string FullName
        {
            get { return BASE_NAME + this.ShortName; }
        }

        public DTE2 ApplicationObject
        {
            get { return appObj; }
        }

        public AddIn AddInInstance
        {
            get { return addIn; }
        }

        #endregion

        #region "methods that must be overridden"

        public abstract string ShortName
        {
            get;
        }

        public abstract string ButtonText
        {
            get;
        }

        public abstract string ToolTip
        {
            get;
        }

        public abstract int Bitmap
        {
            get;
        }

        public virtual void Dispose()
        {
            this.DeleteCommand();
        }

        /*public abstract bool ShouldPositionAtEnd
        {
            get;
        }*/

        /*public abstract string MenuName
        {
            get;
        }*/

        /*public abstract bool Checked
        {
            get;
        }*/

        public abstract void Exec();

        public abstract bool DisplayCommand(UIHierarchyItem item);

        #endregion

        #region "virtual methods/properties
        public override string ToString()
        {
            return FriendlyName;
        }

        public virtual bool ShouldPositionAtEnd
        {
            get { return false; }
        }

        public virtual string MenuName
        {
            get { return "Item"; }
        }

        public virtual bool Checked
        {
            get { return false; }
        }

        public virtual string FriendlyName
        {
            get { return this.ButtonText; }
        }

        public virtual System.Drawing.Icon CustomMenuIcon
        {
            get { return null; }
        }

        /// <summary>
        /// If there are multiple menus with the same MenuName, this setting controls whether this command is added to all of them or just the first
        /// </summary>
        public virtual bool AddCommandToMultipleMenus
        {
            get { return false; }
        }

        #endregion
    }

}
