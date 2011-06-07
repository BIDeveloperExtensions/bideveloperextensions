using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("BIDS Helper")]
[assembly: AssemblyDescription("Provides additional useful features to the BI Development Studio")]
[assembly: AssemblyCompany("http://bidshelper.codeplex.com/")]
[assembly: AssemblyCopyright("Copyright ? 2010 BIDS Helper")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("1.5.0.0")]
[assembly: AssemblyFileVersion("1.5.0.0")]

[assembly: ComVisible(true)]
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]

#if KATMAI
[assembly: AssemblyTitle("BIDS Helper for SQL Server 2008")]
#else
[assembly: AssemblyTitle("BIDS Helper for SQL Server 2005")]
#endif

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
