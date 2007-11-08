using System;
using Microsoft.VisualStudio.CommandBars;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Reflection;
//using BIDSHelper

namespace BIDSHelper
{
    public enum enumIDEMode
    {
        Design = 1,
        Debug = 2,
        Run = 3
    }

    public class Connect : IDTExtensibility2, IDTCommandTarget
    {



        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private DebuggerEvents _debuggerEvents;
        private enumIDEMode _ideMode = enumIDEMode.Design;

        private static System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase> addins = new System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase>();


        public const string REGISTRY_BASE_PATH = "SOFTWARE\\BIDS Helper";

        ///<summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        public string RegistryBasePath
        {
            get { return REGISTRY_BASE_PATH; }
        }

        public string PluginRegistryPath(Type t)
        {
            return RegistryBasePath + "\\" + t.Name;
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

                _applicationObject.StatusBar.Text = "Loading BIDSHelper (" + this.GetType().Assembly.GetName().Version.ToString() + ")...";

                _debuggerEvents = _applicationObject.Events.DebuggerEvents;
                _debuggerEvents.OnEnterBreakMode += new _dispDebuggerEvents_OnEnterBreakModeEventHandler(_debuggerEvents_OnEnterBreakMode);
                _debuggerEvents.OnEnterDesignMode += new _dispDebuggerEvents_OnEnterDesignModeEventHandler(_debuggerEvents_OnEnterDesignMode);
                _debuggerEvents.OnEnterRunMode += new _dispDebuggerEvents_OnEnterRunModeEventHandler(_debuggerEvents_OnEnterRunMode);

                foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (typeof(BIDSHelperPluginBase).IsAssignableFrom(t)
                        && (!object.ReferenceEquals(t, typeof(BIDSHelperPluginBase)))
                        && (!t.IsAbstract))
                    {
                        BIDSHelperPluginBase ext;
                        System.Type[] @params = { typeof(Connect), typeof(DTE2), typeof(AddIn) };
                        System.Reflection.ConstructorInfo con;

                        con = t.GetConstructor(@params);
                        ext = (BIDSHelperPluginBase)con.Invoke(new object[] { this, _applicationObject, _addInInstance });
                        //ext.AddinCore = this;
                        addins.Add(ext.CommandName, ext);

                        //if (ext is IWindowActivatedPlugin)// && ext.Enabled )
                        //{
                        //    ((IWindowActivatedPlugin)ext).HookWindowActivation();
                        //}
                    }
                }

                //switch (connectMode)
                //{
                //    case ext_ConnectMode.ext_cm_Startup:
                //    case ext_ConnectMode.ext_cm_AfterStartup:


                //        foreach (BIDSHelperPluginBase iExt in addins.Values)
                //        {
                //            //Create a Command with name SolnExplContextMenuVB and then add it to the "Item" menubar for the SolutionExplorer
                //            iExt.AddCommand();
                //        }

                //        break;
                //}
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Problem loading BIDS Helper: " + ex.Message);
            }
            finally
            {
                _applicationObject.StatusBar.Clear();
            }
        }

        void _debuggerEvents_OnEnterRunMode(dbgEventReason Reason)
        {
            _ideMode = enumIDEMode.Run;
            dettachWindowEvents();
        }

        void _debuggerEvents_OnEnterDesignMode(dbgEventReason Reason)
        {
            _ideMode = enumIDEMode.Design;
            attachWindowEvents();
        }

        void _debuggerEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            _ideMode = enumIDEMode.Debug;
            dettachWindowEvents();
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
                    try
                    {
                        if (iExt.Enabled)
                            { iExt.OnDisable(); }
                    }
                    catch 
                    { //ignore any errors - we are pulling down the add-in anyway
                    }
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

        private void attachWindowEvents()
        {
            foreach (BIDSHelperPluginBase plugIn in addins.Values)
            {
                if (plugIn is IWindowActivatedPlugin)
                {
                    //TODO
                    ((IWindowActivatedPlugin)plugIn).HookWindowActivation();
                }
            }
        }

        private void dettachWindowEvents()
        {
            foreach (BIDSHelperPluginBase plugIn in addins.Values)
            {
                if (plugIn is IWindowActivatedPlugin)
                {
                    //TODO
                    ((IWindowActivatedPlugin)plugIn).UnHookWindowActivation();
                }
            }
        }

        public enumIDEMode IdeMode
        {
            get { return _ideMode; }
        }

        public static System.Collections.Generic.Dictionary<string, BIDSHelperPluginBase> Plugins
        {
            get { return addins; }
        }
    }
}