namespace BIDSHelper.SSIS
{
    using System.Xml;
    using System.Text;
    using System.ComponentModel.Design;
    using System;
    using Microsoft.SqlServer.Dts.Runtime;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    
    internal class PackageConfigurationLoader
    {
        public static PackageConfigurationSetting[] GetPackageConfigurationSettings(Microsoft.SqlServer.Dts.Runtime.Configuration c, Package p, string sVisualStudioRelativePath, bool bOfflineMode)
        {
            List<PackageConfigurationSetting> list = new List<PackageConfigurationSetting>();
            if (c.ConfigurationType == DTSConfigurationType.ConfigFile || c.ConfigurationType == DTSConfigurationType.IConfigFile)
            {
                string sConfigurationString = c.ConfigurationString;
                if (c.ConfigurationType == DTSConfigurationType.IConfigFile)
                {
                    sConfigurationString = System.Environment.GetEnvironmentVariable(c.ConfigurationString);
                }

                XmlDocument dom = new XmlDocument();
                if (sConfigurationString.Contains("\\"))
                {
                    dom.Load(sConfigurationString);
                }
                else
                {
                    //if it's a relative file path, then try this directory first, and if it's not there, then try the path relative to the dtsx package
                    if (System.IO.File.Exists(sVisualStudioRelativePath + sConfigurationString))
                        dom.Load(sVisualStudioRelativePath + sConfigurationString);
                    else
                        dom.Load(sConfigurationString);
                }
                foreach (XmlNode node in dom.GetElementsByTagName("Configuration"))
                {
                    list.Add(new PackageConfigurationSetting(node.Attributes["Path"].Value, node.SelectSingleNode("ConfiguredValue").InnerText));
                }
            }
            else if (c.ConfigurationType == DTSConfigurationType.SqlServer && !bOfflineMode) //not when in offline mode
            {
                string sConnectionManagerName;
                string sTableName;
                string sFilter;
                Microsoft.DataTransformationServices.Design.DesignUtils.ParseSqlServerConfigurationString(c.ConfigurationString, out sConnectionManagerName, out sTableName, out sFilter);

                ConnectionManager cm = p.Connections[sConnectionManagerName];

#if DENALI
                if (cm.OfflineMode) return list.ToArray();
#endif

                ISessionProperties o = cm.AcquireConnection(null) as ISessionProperties;
                try
                {
                    IDBCreateCommand command = (IDBCreateCommand)o;
                    ICommandText ppCommand;
                    Guid IID_ICommandText = new Guid(0xc733a27, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
                    command.CreateCommand(IntPtr.Zero, ref IID_ICommandText, out ppCommand);
                    Guid DBGUID_DEFAULT = new Guid(0xc8b521fb, 0x5cf3, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
                    ppCommand.SetCommandText(ref DBGUID_DEFAULT, "select ConfiguredValue, PackagePath from " + sTableName + " Where ConfigurationFilter = '" + sFilter.Replace("'", "''") + "'");
                    IntPtr PtrZero = new IntPtr(0);
                    Guid IID_IRowset = new Guid(0xc733a7c, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
                    tagDBPARAMS dbParams = null;
                    int recordsAffected = 0;
                    object executeResult = null;
                    int result = ppCommand.Execute(PtrZero, ref IID_IRowset, dbParams, out recordsAffected, out executeResult);

                    System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                    System.Reflection.BindingFlags getstaticflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static;
                    Type chapterHandleType = ExpressionHighlighterPlugin.GetPrivateType(typeof(System.Data.OleDb.OleDbCommand), "System.Data.OleDb.ChapterHandle");
                    object chapterHandle = chapterHandleType.InvokeMember("DB_NULL_HCHAPTER", getstaticflags, null, null, null);

                    System.Data.OleDb.OleDbDataReader dataReader = null;
                    dataReader = (System.Data.OleDb.OleDbDataReader)typeof(System.Data.OleDb.OleDbDataReader).GetConstructors(getmethodflags)[0].Invoke(new object[] { null, null, 0, System.Data.CommandBehavior.SingleResult });
                    IntPtr intPtrRecordsAffected = new IntPtr(-1);
                    dataReader.GetType().InvokeMember("InitializeIRowset", getmethodflags, null, dataReader, new object[] { executeResult, chapterHandle, intPtrRecordsAffected });
                    dataReader.GetType().InvokeMember("BuildMetaInfo", getmethodflags, null, dataReader, new object[] { });
                    dataReader.GetType().InvokeMember("HasRowsRead", getmethodflags, null, dataReader, new object[] { });
                    executeResult = null;

                    while (dataReader.Read())
                    {
                        list.Add(new PackageConfigurationSetting(dataReader["PackagePath"].ToString(), dataReader["ConfiguredValue"].ToString()));
                    }
                    dataReader.Close();
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
                }
            }
            else if (c.ConfigurationType == DTSConfigurationType.EnvVariable)
            {
                list.Add(new PackageConfigurationSetting(c.PackagePath, System.Environment.GetEnvironmentVariable(c.ConfigurationString)));
            }
            else if (c.ConfigurationType == DTSConfigurationType.ParentVariable || c.ConfigurationType == DTSConfigurationType.IParentVariable)
            {
                list.Add(new PackageConfigurationSetting(c.PackagePath, "")); //can't know value at design time
            }
            else if (c.ConfigurationType == DTSConfigurationType.RegEntry)
            {
                list.Add(new PackageConfigurationSetting(c.PackagePath, (string)Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\" + c.ConfigurationString, "Value", "")));
            }
            else if (c.ConfigurationType == DTSConfigurationType.IRegEntry)
            {
                list.Add(new PackageConfigurationSetting(c.PackagePath, (string)Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\" + System.Environment.GetEnvironmentVariable(c.ConfigurationString), "Value", "")));
            }
            return list.ToArray();
        }
    }

    class PackageConfigurationSetting
    {
        public string Path;
        public string Value;
        public PackageConfigurationSetting(string path, string value)
        {
            Path = path;
            Value = value;
        }
    }

    #region COM Interfaces for OLEDB
    [ComImport, SuppressUnmanagedCodeSecurity, Guid("0C733A85-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISessionProperties
    {
    }

    [ComImport, SuppressUnmanagedCodeSecurity, Guid("0C733A1D-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDBCreateCommand
    {
        int CreateCommand([In] IntPtr pUnkOuter, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out ICommandText ppCommand);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("0C733A27-2A1C-11CE-ADE5-00AA0044773D")]
    internal interface ICommandText
    {
        int Cancel();
        int Execute([In] IntPtr pUnkOuter, [In] ref Guid riid, [In] tagDBPARAMS pDBParams, out int pcRowsAffected, [MarshalAs(UnmanagedType.Interface)] out object ppRowset);
        int GetDBSession();
        int GetCommandText();
        int SetCommandText([In] ref Guid rguidDialect, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal sealed class tagDBPARAMS
    {
        internal IntPtr pData;
        internal int cParamSets;
        internal IntPtr hAccessor;
        internal tagDBPARAMS() { }
    }
    #endregion

}
