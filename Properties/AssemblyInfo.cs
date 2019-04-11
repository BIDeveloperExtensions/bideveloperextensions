using BIDSHelper.Core;
using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("BI Developer Extensions")]
[assembly: AssemblyDescription("BI Developer Extensions (formerly BIDS Helper) provides additional useful features to SQL Server Data Tools")]
[assembly: AssemblyCompany("https://bideveloperextensions.github.io")]
[assembly: AssemblyCopyright("Copyright © 2019 BI Developer Extensions")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion(VersionInfo.Version)]
[assembly: AssemblyFileVersion(VersionInfo.Version)]

[assembly: ComVisible(true)]
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]

#if SQL2019
[assembly: AssemblyTitle("BI Developer Extensions for SQL Server 2019")]
#elif SQL2017
[assembly: AssemblyTitle("BI Developer Extensions for SQL Server 2017")]
#elif SQL2016
[assembly: AssemblyTitle("BI Developer Extensions for SQL Server 2016")]
#elif SQL2014
[assembly: AssemblyTitle("BI Developer Extensions for SQL Server 2014")]
#elif DENALI
[assembly: AssemblyTitle("BI Developer Extensions for SQL Server 2012")]
#else
[assembly: AssemblyTitle("BI Developer Extensions for Unknown SQL Server version. Add a new clause for AssemblyTitle")]
#endif

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
