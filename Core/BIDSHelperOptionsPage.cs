using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using EnvDTE;

namespace BIDSHelper.Core
{
    public partial class BIDSHelperOptionsPage : UserControl, EnvDTE.IDTToolsOptionsPage
    {
        public BIDSHelperOptionsPage()
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
            foreach (BIDSHelperPluginBase puRef in Connect.Plugins.Values)
            {
                lstPlugins.Items.Add(puRef , puRef.Enabled);
            }
            //throw new Exception("The method or operation is not implemented.");
            if (lstPlugins.Items.Count > 0)
            { lstPlugins.Visible = true; }
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
            BIDSHelperPluginBase pu = null;
            //foreach (object itm in lstPlugins.Items) //(BIDSHelperPluginReference puRef in Connect.Plugins.Values)
            for (int i = 0;i<lstPlugins.Items.Count;i++)
            {
                pu = (BIDSHelperPluginBase)lstPlugins.Items[i];
                if (pu.Enabled != lstPlugins.GetItemChecked(i))
                {
                    pu.Enabled = lstPlugins.GetItemChecked(i);
                }
            }
        }

        #endregion

        private void BIDSHelperOptionsPage_Load(object sender, EventArgs e)
        {

        }


    }



    //[System.Runtime.InteropServices.ComVisible(true)]
    //[System.Runtime.InteropServices.ClassInterface(System.Runtime.InteropServices.ClassInterfaceType.AutoDual)]
    //public class OptionPageProperties
    //{
    //    public bool MyProperty
    //    {
    //        get
    //        {
    //            return BIDSHelperOptionsPage.GetValueFromRegistry();
    //        }
    //        set
    //        {
    //            BIDSHelperOptionsPage.SetValueToRegistry(value);
    //        }
    //    }
    //}
}
