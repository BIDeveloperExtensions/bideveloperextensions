using System;
using Microsoft.VisualStudio.CommandBars;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Reflection;

namespace BIDSHelper
{
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {


        private DTE2 _applicationObject;
        private AddIn _addInInstance;

        private System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase> addins = new System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase>();


        public const string REGISTRY_BASE_PATH = "SOFTWARE\\BIDS Helper";

        ///<summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        ///<summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        ///<param name='application'>Root object of the host application.</param>
        ///<param name='connectMode'>Describes how the Add-in is being loaded.</param>
        ///<param name='addInInst'>Object representing this Add-in.</param>
        ///<param name='custom'>Array containing custom parameters</param>
        ///<remarks></remarks>
        void IDTExtensibility2.OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            try
            {
                _applicationObject = (DTE2)application;
                _addInInstance = (AddIn)addInInst;


                foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (typeof(BIDSHelperPluginBase).IsAssignableFrom(t) && (!object.ReferenceEquals(t, typeof(BIDSHelperPluginBase))))
                    {
                        BIDSHelperPluginBase ext;
                        System.Type[] @params = { typeof(DTE2), typeof(AddIn) };
                        System.Reflection.ConstructorInfo con;

                        con = t.GetConstructor(@params);
                        ext = (BIDSHelperPluginBase)con.Invoke(new object[] { _applicationObject, _addInInstance });

                        addins.Add(ext.FullName, ext);
                    }
                }

                switch (connectMode)
                {
                    case ext_ConnectMode.ext_cm_Startup:
                    case ext_ConnectMode.ext_cm_AfterStartup:


                        foreach (BIDSHelperPluginBase iExt in addins.Values)
                        {
                            //Create a Command with name SolnExplContextMenuVB and then add it to the "Item" menubar for the SolutionExplorer
                            iExt.AddCommand();
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Problem loading BIDS Helper: " + ex.Message);
            }
        }

        ///<summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        ///<param name='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        ///<param name='custom'>Array of parameters that are host application specific.</param>
        ///<remarks></remarks>
        void IDTExtensibility2.OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            try
            {
                foreach (BIDSHelperPluginBase iExt in addins.Values)
                {
                    iExt.DeleteCommand();
                }
            }

            catch //(Exception ex) 
            {
                //MsgBox(ex.ToString)
            }

        }

        ///<summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification that the collection of Add-ins has changed.</summary>
        ///<param name='custom'>Array of parameters that are host application specific.</param>
        ///<remarks></remarks>
        void IDTExtensibility2.OnAddInsUpdate(ref Array custom)
        {
        }

        ///<summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        ///<param name='custom'>Array of parameters that are host application specific.</param>
        ///<remarks></remarks>
        void IDTExtensibility2.OnStartupComplete(ref Array custom)
        {
        }

        ///<summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        ///<param name='custom'>Array of parameters that are host application specific.</param>
        ///<remarks></remarks>
        void IDTExtensibility2.OnBeginShutdown(ref Array custom)
        {
        }


        void EnvDTE.IDTCommandTarget.Exec(string CmdName, EnvDTE.vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut, ref bool Handled)
        {
            try
            {
                Handled = false;
                if (ExecuteOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
                {
                    BIDSHelperPluginBase iExt = addins[CmdName];
                    Handled = true;
                    iExt.Exec();
                }
            }
            catch { }
        }

        void EnvDTE.IDTCommandTarget.QueryStatus(string CmdName, EnvDTE.vsCommandStatusTextWanted NeededText, ref EnvDTE.vsCommandStatus StatusOption, ref object CommandText)
        {
            try
            {
                if (NeededText == EnvDTE.vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
                {
                    //Dynamically enable & disable the command. If the selected file name is File1.cs, then make the command visible.
                    UIHierarchyItem item = GetSelectedProjectItem(_applicationObject);
                    StatusOption = addins[CmdName].QueryStatus(item);
                }
            }
            catch { }
        }

        private UIHierarchyItem GetSelectedProjectItem(DTE2 appObj)
        {
            try
            {
                UIHierarchyItem item;
                UIHierarchy UIH = _applicationObject.ToolWindows.SolutionExplorer;
                item = (UIHierarchyItem)((System.Array)UIH.SelectedItems).GetValue(0);
                return item;
            }
            catch
            {
                return null;
            }
        }

    }
}