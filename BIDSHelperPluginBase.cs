using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Windows.Forms;

namespace BIDSHelper
{
    public abstract class BIDSHelperPluginBase
    {


        private const string BASE_NAME = "BIDSHelper.Connect.";
        private Command pluginCmd;
        private CommandBar pluginCmdBar;
        private DTE2 appObj;
        private AddIn addIn;
        private const int UNKNOWN_CMD_ID = -1;

        #region "Constructors"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appObject"></param>
        /// <param name="addinInstance"></param>
        public BIDSHelperPluginBase(DTE2 appObject, AddIn addinInstance)
        {
            appObj = appObject;
            addIn = addinInstance;
        }

        /// <summary>
        /// 
        /// </summary>
        public BIDSHelperPluginBase()
        {

        }
        #endregion

        #region "Helper Functions"
        /// <summary>
        /// 
        /// </summary>
        public void AddCommand(string commandBarName, int commandPosition)
        {
            try
            {
                Command cmdTmp;
                CommandBars cmdBars;
                cmdBars = (CommandBars)appObj.CommandBars;
                // Check any old versions of the command are not still hanging around
                try
                {
                    cmdTmp = appObj.Commands.Item(this.FullName, UNKNOWN_CMD_ID);
                    cmdTmp.Delete();
                }
                catch { }

                // this is an empty array for passing into the AddNamedCommand method
                object[] contextUIGUIDs = null;

                cmdTmp = appObj.Commands.AddNamedCommand(
                            this.addIn,
                            this.ShortName,
                            this.ButtonText,
                            this.ToolTip,
                            true,
                            this.Bitmap,
                            ref contextUIGUIDs,
                            (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled);

                pluginCmdBar = cmdBars["Item"];
                if (pluginCmdBar == null)
                {
                    System.Windows.Forms.MessageBox.Show("Cannot get the Item menubar");
                }
                else
                {
                    pluginCmd = cmdTmp;
                    pluginCmd.AddControl(pluginCmdBar, 1);
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show("Problem registering " + this.FullName + " command: " + e.Message);
            }

        }

        /// <summary>
        /// By Default the AddCommand method ads the command to the item commandbar at position 0
        /// </summary>
        public void AddCommand()
        {
            AddCommand("Item", 0);
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
                //\\ Enabled for .cube files
                return (vsCommandStatus)vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported;
            }
            else
            {
                //\\ disabled
                return (vsCommandStatus)vsCommandStatus.vsCommandStatusUnsupported | vsCommandStatus.vsCommandStatusInvisible;
            }
        }

        #endregion

        # region "Public Properties"
        /// <summary>
        /// 
        /// </summary>
        public string FullName
        {
            get { return BASE_NAME + this.ShortName; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DTE2 ApplicationObject
        {
            get { return appObj; }
        }

        /// <summary>
        /// 
        /// </summary>
        public AddIn AddInInstance
        {
            get { return addIn; }
        }
        #endregion

        #region "methods that must be overridden"
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="item"></param>
        ///// <returns></returns>
        //public abstract EnvDTE.vsCommandStatus QueryStatus(EnvDTE.UIHierarchyItem item);

        /// <summary>
        /// 
        /// </summary>
        public abstract string ShortName
        {
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public abstract string ButtonText
        {
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public abstract string ToolTip
        {
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public abstract int Bitmap
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void Exec();

        public abstract bool DisplayCommand(UIHierarchyItem item);

        #endregion
    }
}