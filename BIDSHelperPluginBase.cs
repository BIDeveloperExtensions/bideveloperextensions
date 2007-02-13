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
        private CommandBarPopup toolsCommandBarPopup;
        private DTE2 appObj;
        private AddIn addIn;
        private const int UNKNOWN_CMD_ID = -1;

        #region "Constructors"
        public BIDSHelperPluginBase(DTE2 appObject, AddIn addinInstance)
        {
            appObj = appObject;
            addIn = addinInstance;
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

                pluginCmdBar = cmdBars[this.MenuName];
                if (pluginCmdBar == null)
                {
                    System.Windows.Forms.MessageBox.Show("Cannot get the " + this.MenuName + " menubar");
                }
                else
                {
                    pluginCmd = cmdTmp;

                    if (this.MenuName == "Tools")
                    {
                        if (toolsCommandBarPopup == null)
                        {
                            toolsCommandBarPopup = (CommandBarPopup)pluginCmdBar.Controls.Add(MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing, 1, true);
                            toolsCommandBarPopup.CommandBar.Name = "BIDSHelperToolsCommandBarPopup";
                            toolsCommandBarPopup.Caption = "BIDS Helper";
                        }
                        pluginCmd.AddControl(toolsCommandBarPopup.CommandBar, 1);
                        toolsCommandBarPopup.Visible = true;
                    }
                    else
                    {
                        if (!ShouldPositionAtEnd)
                        {
                            pluginCmd.AddControl(pluginCmdBar, 1);
                        }
                        else
                        {
                            pluginCmd.AddControl(pluginCmdBar, pluginCmdBar.Controls.Count - 1);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show("Problem registering " + this.FullName + " command: " + e.Message);
            }

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
                    toolsCommandBarPopup.Delete(true);
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

        public abstract bool ShouldPositionAtEnd
        {
            get;
        }

        public abstract string MenuName
        {
            get;
        }

        public abstract bool Checked
        {
            get;
        }

        public abstract void Exec();

        public abstract bool DisplayCommand(UIHierarchyItem item);

        #endregion
    }
}