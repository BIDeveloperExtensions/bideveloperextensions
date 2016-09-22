using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BIDSHelper.Core
{
    public class VersionInfo
    {
        // BIDS Helper Assembly & VSIX Version
        // N.B. Manually update the manifest file, if you change this - See source.extension.vsixmanifest
        public const string Version = "2.0.0.0";

        private static readonly object lockResource = new object();
        private static Version visualStudioVersion;
        private static Version sqlServerVersion;

        // Adapted from http://stackoverflow.com/questions/11082436/detect-the-visual-studio-version-inside-a-vspackage
        public static Version VisualStudioVersion
        {
            get
            {
                lock (lockResource)
                {
                    if (visualStudioVersion == null)
                    {
                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msenv.dll");

                        if (File.Exists(path))
                        {
                            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);

                            string verName = fvi.ProductVersion;

                            for (int i = 0; i < verName.Length; i++)
                            {
                                if (!char.IsDigit(verName, i) && verName[i] != '.')
                                {
                                    verName = verName.Substring(0, i);
                                    break;
                                }
                            }

                            visualStudioVersion = new Version(verName);
                        }
                        else
                        {
                            visualStudioVersion = new Version(0, 0); // Not running inside Visual Studio!
                        }
                    }
                }

                return visualStudioVersion;
            }
        }

        public static string VisualStudioFriendlyVersion
        {
            get
            {
                switch (VisualStudioVersion.Major)
                {
                    case 8:
                        return "2005";
                    case 9:
                        return "2008";
                    case 10:
                        return "2010";
                    case 11:
                        return "2012";
                    case 12:
                        return "2013";
                    case 14:
                        return "2015";
                    case 15:
                        return "\"15\"";    // Unofficial
                    default:
                        return string.Format("(VS Unknown {0})", VisualStudioVersion.ToString());
                }
            }
        }

        public static Version SqlServerVersion
        {
            get
            {
                lock (lockResource)
                {
                    if (sqlServerVersion == null)
                    {
                        // Get a sample assembly that's installed with BIDS and use that to detect if BIDS is installed
                        Assembly assembly = Assembly.Load("Microsoft.DataWarehouse"); 

                        AssemblyInformationalVersionAttribute attributeVersion = (AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute));
                        if (attributeVersion != null)
                        {
                            sqlServerVersion = new Version(attributeVersion.InformationalVersion);
                        }
                        else
                        {
                            sqlServerVersion = assembly.GetName().Version;
                        }
                    }

                    return sqlServerVersion;
                }
            }
        }

        public static string SqlServerFriendlyVersion
        {
            get
            {
                switch (VisualStudioVersion.Major)
                {
                    case 9:
                        return "2005";
                    case 10:
                        if (VisualStudioVersion.Minor == 5)
                        {
                            return "2008 R2";
                        }
                        else
                        {
                            return "2008";
                        }
                    case 11:
                        return "2012";
                    case 12:
                        return "2014";
                    case 13:
                        return "2016";
                    default:
                        return string.Format("(SQL Unknown {0})", VisualStudioVersion.ToString());
                }
            }
        }

    }
}
