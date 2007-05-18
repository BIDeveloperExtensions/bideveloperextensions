using System;
using System.Xml;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.IO;

// =========================================================================================
// Class      : DeploymentSettings.cs
// Author     : Darren Gosbell
// Date       : 18 Dec 2006
// Description: This class determines the users currently deployment settings.
//              This class is a bit of a hack, it reads the deployment server settings from 
//              private classes
// =========================================================================================


public class DeploymentSettings
{
	//// The name of the target server is figured out in the following order
	//// * defaults to "localhost"
	//// * can be overridden at the user's registry level with a DefaultTargetServer
	////   (set as an option in BIDS)
	//// * can be overridden at the user's project level (set as a project option)
	private string mTargetServer = "localhost";
	//// The name of the target Database is figured out in the following order
	//// * defaults to the name of the project
	//// * can be overridden at the user's project level (set as a project option)
	private string mTargetDatabase = "";


	public DeploymentSettings(EnvDTE.ProjectItem projectItm)
	{
        //// A default target deployment server can be set at the user level
        //// under tools options in visual studio.
        SetDefaultTargetServer();
        //// The default database name is the project name if it is not overriden
        //// by the user settings
        SetDefaultDatabaseName(projectItm);

        Project project = projectItm.ContainingProject;
        System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
        object oService = typeof(System.IServiceProvider).InvokeMember("GetService", flags, null, projectItm.ContainingProject, new object[] { typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings) });
        string sTargetServer = (string)oService.GetType().InvokeMember("GetSetting", flags, null, oService, new object[] { "TargetServer" });
        if (!String.IsNullOrEmpty(sTargetServer)) mTargetServer = sTargetServer;
        string sTargetDatabase = (string)oService.GetType().InvokeMember("GetSetting", flags, null, oService, new object[] { "TargetDatabase" });
        if (!String.IsNullOrEmpty(sTargetDatabase)) mTargetDatabase = sTargetDatabase;
    }

	public string TargetServer {
		get { return mTargetServer; }
	}

	public string TargetDatabase {
		get { return mTargetDatabase; }
	}

    private void SetDefaultTargetServer()
    {
        Microsoft.Win32.RegistryKey regKey;
        regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\VisualStudio\\8.0\\Packages\\{4a0c6509-bf90-43da-abee-0aba3a8527f1}\\Settings\\Analysis Services Project");
        if (regKey == null) return;
        string targetSvr = (string)regKey.GetValue("DefaultTargetServer");
        if (!String.IsNullOrEmpty(targetSvr))
        {
            mTargetServer = targetSvr;
        }
        regKey.Close();

    }

    private void SetDefaultDatabaseName(ProjectItem projectItm)
    {
        mTargetDatabase = projectItm.ContainingProject.Name;
    }
}
