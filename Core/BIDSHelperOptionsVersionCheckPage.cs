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
    public partial class BIDSHelperOptionsVersionCheckPage : UserControl, EnvDTE.IDTToolsOptionsPage
    {
        public BIDSHelperOptionsVersionCheckPage()
        {
            InitializeComponent();
        }


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
            //lstPlugins.Items.Clear();
            //foreach (BIDSHelperPluginBase puRef in Connect.Plugins.Values)
            //{
            //    lstPlugins.Items.Add(puRef , puRef.Enabled);
            //}
            ////throw new Exception("The method or operation is not implemented.");
            //if (lstPlugins.Items.Count > 0)
            //{ 
            //    lstPlugins.Visible = true;
            //    lblCurrentlyDisabled.Visible = false;
            //}
            //else
            //{ 
            //    lblCurrentlyDisabled.Visible = true;
            //    lstPlugins.Visible = false;
            //}
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
            //BIDSHelperPluginBase pu = null;
            ////foreach (object itm in lstPlugins.Items) //(BIDSHelperPluginReference puRef in Connect.Plugins.Values)
            //for (int i = 0;i<lstPlugins.Items.Count;i++)
            //{
            //    pu = (BIDSHelperPluginBase)lstPlugins.Items[i];
            //    if (pu.Enabled != lstPlugins.GetItemChecked(i))
            //    {
            //        pu.Enabled = lstPlugins.GetItemChecked(i);
            //    }
            //}
        }

        #endregion

        private void BIDSHelperOptionsVersionCheckPage_Load(object sender, EventArgs e)
        {
            try
            {
                #if KATMAI
                lblTitle.Text = "BIDS Helper for SQL 2008";
                #else
                lblTitle.Text = "BIDS Helper for SQL 2005";
                #endif

                lblLocalVersion.Text = VersionCheckPlugin.VersionCheckPluginInstance.LocalVersion;

                try
                {
                    VersionCheckPlugin.VersionCheckPluginInstance.LastVersionCheck = DateTime.Today;
                    if (!VersionCheckPlugin.VersionIsLatest(VersionCheckPlugin.VersionCheckPluginInstance.LocalVersion, VersionCheckPlugin.VersionCheckPluginInstance.ServerVersion))
                    {
                        lblServerVersion.Text = "Version " + VersionCheckPlugin.VersionCheckPluginInstance.ServerVersion + " is available...";
                        lblServerVersion.Visible = true;
                        linkNewVersion.Visible = true;
                    }
                    else
                    {
                        lblServerVersion.Visible = false;
                        linkNewVersion.Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    lblServerVersion.Text = "Unable to retrieve current available BIDS Helper version from Codeplex: " + ex.Message + "\r\n" + ex.StackTrace;
                    linkNewVersion.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = "iexplore.exe";
                process.StartInfo.Arguments = linkLabel1.Text;
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void linkNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = "iexplore.exe";
                process.StartInfo.Arguments = VersionCheckPlugin.BIDS_HELPER_RELEASE_URL;
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void BIDSHelperOptionsVersionCheckPage_ControlAdded(object sender, ControlEventArgs e)
        {

        }

        private void BIDSHelperOptionsVersionCheckPage_Paint(object sender, PaintEventArgs e)
        {

        }


    }

}
