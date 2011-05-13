using Microsoft.Win32;
using Varigence.Languages.Biml.Platform;

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

        public static SsisVersion GetSsisVersion2008Variant()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\100\DTS\Setup");
            if (rk != null)
            {
                var version = rk.GetValue("Version") as string;
                if (version != null && version.StartsWith("10.5"))
                {
                    return SsisVersion.Ssis2008R2;
                }
            }

            return SsisVersion.Ssis2008;
        }
    }
}
