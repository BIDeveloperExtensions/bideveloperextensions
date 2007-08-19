using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace BIDSHelper.Core
{
    class BIDSHelperPluginReference
    {
        public BIDSHelperPluginReference(Type t)
        {
            pluginType = t;
        }

        private Type pluginType = null;
        public Type PluginType
        {
            get { return pluginType; }
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
            }
        }

        private string PluginRegistryPath()
        {
            return Connect.REGISTRY_BASE_PATH + "\\" + pluginType.Name;
        }

        private void SetEnabled(bool isEnabled)
        {
            RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PluginRegistryPath());
            regKey.SetValue("Enabled",isEnabled);
        }

        private bool GetEnabled()
        {
            RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PluginRegistryPath());
            return (bool)regKey.GetValue("Enabled",true);
        }
    }
}
