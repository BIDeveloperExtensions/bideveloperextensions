using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;

namespace BIDSHelper.SSIS.Biml
{
    internal static class BimlUtility
    {
        public static bool CheckRequiredFrameworkVersion()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5");
            if (rk == null || rk.GetValue("Install") == null)
            {
                var dialog = new FrameworkVersionAlertDialog();
                dialog.ShowDialog();
                return false;
            }

            return true;
        }
    }
}
