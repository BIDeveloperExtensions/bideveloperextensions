using System;
using System.Collections.Generic;
using System.Text;
using Extensibility;
using EnvDTE;
using EnvDTE80;
//using Microsoft.VisualStudio.CommandBars;
//using System.Text;
//using System.Windows.Forms;
using Microsoft.AnalysisServices;
//using System.Data;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace BIDSHelper
{

    class PowerShellWindowPlugin : BIDSHelperPluginBase
    {
        private IComponentChangeService changesvc;
        private Database currentDB;

        public PowerShellWindowPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override bool DisplayCommand(UIHierarchyItem item)
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                if (((Project)hierItem.Object).Object is Database)
                {
                        currentDB = (Database)(((Project)hierItem.Object).Object);
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public override void Exec()
        {
            //System.Reflection.Assembly ass = System.Reflection.Assembly.Load(new System.Reflection.AssemblyName("System.Management.Automation"));
            System.Reflection.Assembly ass = System.Reflection.Assembly.Load("System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            //System.Reflection.Assembly ass = System.Reflection.Assembly.LoadWithPartialName("System.Management.Automation");
            if (ass == null)
            {
                MessageBox.Show("PowerShell has not been detected. You need to download and install PowerShell from the Microsoft website in order to use this feature."
                    , "BIDS Helper PowerShell Window"
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Stop);
                return;
            }

            try
            {
                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(true, "Launching Powershell...", 0, 1);

                this.changesvc = (IComponentChangeService)currentDB.Site.GetService(typeof(IComponentChangeService));

                //int iErrorCnt = 0;
                EnvDTE80.Windows2 toolWins;
                EnvDTE.Window toolWin;
                object objTemp = null;
                toolWins = (Windows2)ApplicationObject.Windows;
                toolWin = toolWins.CreateToolWindow2(AddInInstance
                    , typeof(BIDSHelper.SSAS.PowerShellControl).Assembly.Location
                    , typeof(BIDSHelper.SSAS.PowerShellControl).FullName
                    , currentDB.Name + ": PowerShell Window", "{" + typeof(BIDSHelper.SSAS.PowerShellControl).GUID.ToString() + "}"
                    , ref objTemp);

                BIDSHelper.SSAS.PowerShellControl ctrl = (SSAS.PowerShellControl)objTemp;
                ctrl.CurrentDB = currentDB;
                ctrl.ToolWindows = toolWins;
                EnvDTE.Properties prop = this.ApplicationObject.get_Properties("FontsAndColors", "TextEditor");
                ctrl.SetFont((string)prop.Item("FontFamily").Value, (float)Convert.ToDouble(prop.Item("FontSize").Value));

                //setting IsFloating and Linkable to false makes this window tabbed
                toolWin.IsFloating = false;
                toolWin.Linkable = false;
                toolWin.Visible = true;

                this.changesvc.OnComponentChanging(this.currentDB, null);
                this.changesvc.OnComponentChanged(this.currentDB, null, null, null);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

            finally
            {
                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
                ApplicationObject.StatusBar.Progress(false, "Launching Powershell...", 2, 2);
            }
        }

        public override bool ShouldPositionAtEnd
        {
            get
            {
                return true;
            }
        }

        public override string ButtonText
        {
            get { return "Start Powershell"; }
        }

        public override string ToolTip
        {
            get { return "Opens a powershell environment for scripting against the SSAS project"; }
        }

        public override int Bitmap
        {
            get { return 588; }
        }

        public override string ShortName
        {
            get { return "PowerShellWindow"; }
        }

        public override string MenuName
        {
            get
            {
                return "Item,Project";
            }
        }

        public override BIDSFeatureCategories FeatureCategory
        {
            get { throw new NotImplementedException(); }
        }
    }
}
