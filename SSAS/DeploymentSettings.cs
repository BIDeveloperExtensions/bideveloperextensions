extern alias sharedDataWarehouseInterfaces;
extern alias asDataWarehouseInterfaces;
using System;
using System.Xml;
//using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.IO;
using sharedDataWarehouseInterfaces::Microsoft.DataWarehouse.Interfaces;

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

    private string mTargetCubeName = null;

    public DeploymentSettings(EnvDTE.ProjectItem projectItm)
    {
        PopulateDeploymentSettings(projectItm.ContainingProject);
    }
    public DeploymentSettings(EnvDTE.Project project)
    {
        PopulateDeploymentSettings(project);
    }

    void PopulateDeploymentSettings(Project project)
	{
        //// A default target deployment server can be set at the user level
        //// under tools options in visual studio.
        SetDefaultTargetServer();
        //// The default database name is the project name if it is not overriden
        //// by the user settings
        SetDefaultDatabaseName(project);

        System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
        object oService = null;
        if (project is System.IServiceProvider) //Multidimensional
        {
            oService = project.GetIConfigurationSettings();
            if (oService == null) throw new Exception("Could not GetService IConfigurationSettings in project from " + project.GetType().Assembly.Location);

            string sTargetServer = (string)oService.GetType().InvokeMember("GetSetting", flags, null, oService, new object[] { "TargetServer" });
            if (!String.IsNullOrEmpty(sTargetServer)) mTargetServer = sTargetServer;
            string sTargetDatabase = (string)oService.GetType().InvokeMember("GetSetting", flags, null, oService, new object[] { "TargetDatabase" });
            if (!String.IsNullOrEmpty(sTargetDatabase)) mTargetDatabase = sTargetDatabase;
        }

        else if (project.Object is Microsoft.AnalysisServices.VSHost.Integration.ProjectNode) //Tabular
        {
            //during Visual Studio debug of BIDS Helper on my laptop, this throws an HRESULT: 0x80070057 (E_INVALIDARG) which I haven't been able to fix. But this works fine when not debugging BIDS Helper
            Microsoft.AnalysisServices.VSHost.Integration.ProjectNode projectNode = ((Microsoft.AnalysisServices.VSHost.Integration.ProjectNode)project.Object);
            string sTargetServer = projectNode.GetProjectProperty("DeploymentServerName");
            if (!String.IsNullOrEmpty(sTargetServer)) mTargetServer = sTargetServer;
            string sTargetDatabase = projectNode.GetProjectProperty("DeploymentServerDatabase");
            if (!String.IsNullOrEmpty(sTargetDatabase)) mTargetDatabase = sTargetDatabase;
            string sTargetCubeName = projectNode.GetProjectProperty("DeploymentServerCubeName");
            if (!String.IsNullOrEmpty(sTargetCubeName)) mTargetCubeName = sTargetCubeName;
        }
        else
        {
            throw new Exception("Unable to find SSAS deployment settings. Unexpected project type.");
        }
    }

	public string TargetServer {
		get { return mTargetServer; }
	}

	public string TargetDatabase {
		get { return mTargetDatabase; }
	}

    public string TargetCubeName
    {
        get { return mTargetCubeName; }
    }

    private void SetDefaultTargetServer()
    {
        Microsoft.Win32.RegistryKey regKey;
#if SQL2016 || SQL2017
        regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\14.0\Packages\{4a0c6509-bf90-43da-abee-0aba3a8527f1}\Settings\Analysis Services Project");
#elif SQL2014
        regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\12.0\Packages\{4a0c6509-bf90-43da-abee-0aba3a8527f1}\Settings\Analysis Services Project");
#elif DENALI //TODO: make this dynamic depending on the version of VS so that VS2010 and VS2012 both work
        regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\10.0\Packages\{4a0c6509-bf90-43da-abee-0aba3a8527f1}\Settings\Analysis Services Project");
#else
        Unknown SQL Sever version. Add a new clause for regKey value
#endif
        if (regKey == null) return;
        string targetSvr = (string)regKey.GetValue("DefaultTargetServer");
        if (!String.IsNullOrEmpty(targetSvr))
        {
            mTargetServer = targetSvr;
        }
        regKey.Close();

    }

    private void SetDefaultDatabaseName(Project project)
    {
        mTargetDatabase = project.Name;
    }
}
