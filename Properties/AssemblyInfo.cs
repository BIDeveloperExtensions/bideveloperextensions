using BIDSHelper.Core;
using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("BIDS Helper")]
[assembly: AssemblyDescription("Provides additional useful features to SQL Server Data Tools (formerly known as BI Development Studio)")]
[assembly: AssemblyCompany("http://bidshelper.codeplex.com/")]
[assembly: AssemblyCopyright("Copyright © 2016 BIDS Helper")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion(VersionInfo.Version)]
[assembly: AssemblyFileVersion(VersionInfo.Version)]

[assembly: ComVisible(true)]
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]

#if SQL2017
[assembly: AssemblyTitle("BIDS Helper for SQL Server 2017")]
#elif SQL2016
[assembly: AssemblyTitle("BIDS Helper for SQL Server 2016")]
#elif SQL2014
[assembly: AssemblyTitle("BIDS Helper for SQL Server 2014")]
#elif DENALI
[assembly: AssemblyTitle("BIDS Helper for SQL Server 2012")]
#else
Unknown SQL Sever version. Add a new clause for AssemblyTitle
#endif

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
