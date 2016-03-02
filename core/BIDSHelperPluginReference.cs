using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using Extensibility;
using EnvDTE;
using EnvDTE80;

namespace BIDSHelper.Core
{
    public class BIDSHelperPluginReference
    {

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private Connect _core;

        public BIDSHelperPluginReference(Type t, DTE2 applicationObject, AddIn addInInstance, Connect addinCore)
        {
            pluginType = t;
            _applicationObject = applicationObject;
            _addInInstance = addInInstance;
            _core = addinCore;

        }

        private BIDSHelperPluginBase pi = null;
        public BIDSHelperPluginBase PlugIn
        {
            get { return pi; }
        }

        private Type pluginType = null;
        public Type PluginType
        {
            get { return pluginType; }
        }

        public string PlugInName()
        {
            
            if (pluginType.Name.ToLower().EndsWith("plugin"))
            {
                return pluginType.Name.Substring(0,pluginType.Name.Length -6);
            }
            else
            {
                return pluginType.Name;
            }
        }

        public override string ToString()
        {
            return PlugInName();
        }

        private bool enabledCached = false;
        private bool enabled = true;
        public bool Enabled
        {
            get 
            {
                if (!enabledCached)
                {
                    enabled = GetEnabled();
                    enabledCached = true;
                }
                return enabled; 
            }
            set 
            {
                enabled = value;
                SetEnabled(enabled);
                if (value == true)
                {
                    this.ConstructPlugin();
                    //this.PlugIn.AddCommand();
                    //if (this.PlugIn is IWindowActivatedPlugin)
                    //{
                    //    ((IWindowActivatedPlugin)this.PlugIn).HookWindowActivation();
                    //}
                }
                else
                {
                    //this.PlugIn.DeleteCommand();
                    //if (this.PlugIn is IWindowActivatedPlugin)
                    //{
                    //    ((IWindowActivatedPlugin)this.PlugIn).UnHookWindowActivation();
                    //}
                    this.DisposePlugin();
                }
            }
        }

        private void DisposePlugin()
        {
            pi.DeleteCommand();
            this.PlugIn.DeleteCommand();
            if (this.PlugIn is IWindowActivatedPlugin)
            {
                ((IWindowActivatedPlugin)this.PlugIn).UnHookWindowActivation();
            }
            pi.Dispose();
            pi = null;
        }

        private string PluginRegistryPath()
        {
            return Connect.REGISTRY_BASE_PATH + "\\" + pluginType.Name;
        }

        private void SetEnabled(bool isEnabled)
        {
            RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PluginRegistryPath());
            if (isEnabled)
            {
                regKey.DeleteValue("Enabled");
            }
            else 
            { 
                regKey.SetValue("Enabled", isEnabled, RegistryValueKind.DWord); 
            }
        }

        private bool GetEnabled()
        {
            RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PluginRegistryPath());
            return  ((int)regKey.GetValue("Enabled",1)==1) ?true :false;
            
        }

        public void ConstructPlugin()
        {
            //BIDSHelperPluginBase pi;
            System.Type[] @params = { typeof(DTE2), typeof(AddIn) };
            System.Reflection.ConstructorInfo con;

            con = pluginType.GetConstructor(@params);
            pi = (BIDSHelperPluginBase)con.Invoke(new object[] { _applicationObject, _addInInstance });
            pi.AddinCore = _core;
            //addins.Add(ext.FullName, ext);

            if (pi is IWindowActivatedPlugin)
            {
                ((IWindowActivatedPlugin)pi).HookWindowActivation();
            }
            pi.AddCommand();
        }

    }
}
