using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using EnvDTE;

namespace BIDSHelper.SSIS.DesignPracticeScanner
{
    public partial class DesignWarningsOptionsPage : UserControl, EnvDTE.IDTToolsOptionsPage
    {
        public DesignWarningsOptionsPage()
        {
            InitializeComponent();
        }


        //public static bool GetValueFromRegistry()
        //{
        //    Microsoft.Win32.RegistryKey registryKey;
        //    int registryValue;

        //    registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\8.0", false);
        //    registryValue = (int)registryKey.GetValue("MyPropertyCS", 0);
        //    if (registryValue == 0)
        //        return false;
        //    return true;
        //}

        //public static void SetValueToRegistry(bool value)
        //{
        //    Microsoft.Win32.RegistryKey registryKey;
        //    int registryValue;

        //    registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\8.0", true);
        //    if (value)
        //        registryValue = 1;
        //    else
        //        registryValue = 0;

        //    registryKey.SetValue("MyPropertyCS", registryValue, Microsoft.Win32.RegistryValueKind.DWord);
        //}


        #region IDTToolsOptionsPage Members

        //Property can be accessed through the object model with code such as the following macro:
        //    Sub ToolsOptionsPageProperties()
        //        MsgBox(DTE.Properties("My Category", "My Subcategory - Visual C#").Item("MyProperty").Value)
        //        DTE.Properties("My Category", "My Subcategory - Visual C#").Item("MyProperty").Value = False
        //        MsgBox(DTE.Properties("My Category", "My Subcategory - Visual C#").Item("MyProperty").Value)
        //    End Sub
        void IDTToolsOptionsPage.GetProperties(ref object PropertiesObject)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        void IDTToolsOptionsPage.OnAfterCreated(DTE DTEObject)
        {
            lstPlugins.Items.Clear();
            foreach (DesignPractice dbRef in DesignPracticesPlugin.DesignPractices)
            {
                lstPlugins.Items.Add(dbRef , dbRef.Enabled);
            }
            //throw new Exception("The method or operation is not implemented.");
            if (lstPlugins.Items.Count > 0)
            { 
                lstPlugins.Visible = true;
                lblCurrentlyDisabled.Visible = false;
            }
            else
            { 
                lblCurrentlyDisabled.Visible = true;
                lstPlugins.Visible = false;
            }
        }

        void IDTToolsOptionsPage.OnCancel()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IDTToolsOptionsPage.OnHelp()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IDTToolsOptionsPage.OnOK()
        {
            DesignPractice dp = null;
            //foreach (object itm in lstPlugins.Items) //(BIDSHelperPluginReference puRef in Connect.Plugins.Values)
            for (int i = 0;i<lstPlugins.Items.Count;i++)
            {
                dp = (DesignPractice)lstPlugins.Items[i];
                if (dp.Enabled != lstPlugins.GetItemChecked(i))
                {
                    dp.Enabled = lstPlugins.GetItemChecked(i);
                }
            }
        }

        #endregion

        private void BIDSHelperOptionsPage_Load(object sender, EventArgs e)
        {

        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }


    }
}