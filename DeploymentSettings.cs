using System;
using System.Xml;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.IO;

// =========================================================================================
// Class      : DeploymentSettings.vb
// Author     : Darren Gosbell
// Date       : 18 Dec 2006
// Description: This class determines the users currently deployment settings.
//              This class is a bit of a hack, it reads the deployment server settings from the 
//              file system as this cannot currently be determined from the extensibility objects
//
// =========================================================================================

/// <summary>
/// 
/// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectItm"></param>
	public DeploymentSettings(EnvDTE.ProjectItem projectItm)
	{
		// (ByVal projectFile As String, ByVal currentConfig As String)
		//// A default target deployment server can be set at the user level
		//// under tools options in visual studio.
		SetDefaultTargetServer();
		//// The default database name is the project name if it is not overriden
		//// by the user settings
		SetDefaultDatabaseName(projectItm);

		string projectFile = projectItm.ContainingProject.FullName;
		string currentConfig = projectItm.ContainingProject.DTE.Solution.SolutionBuild.ActiveConfiguration.Name;
		ReadUserFile(projectFile, currentConfig);

	}

    /// <summary>
    /// 
    /// </summary>
	public string TargetServer {
		get { return mTargetServer; }
	}

    /// <summary>
    /// 
    /// </summary>
	public string TargetDatabase {
		get { return mTargetDatabase; }
	}

	private void ReadUserFile(string projectFile, string currentConfig)
	{
		string userFile = projectFile + ".user";
		if (System.IO.File.Exists(userFile))
		{
			XmlReader xmlRdr = XmlReader.Create(new System.IO.FileStream(userFile, System.IO.FileMode.Open));
			try {
				string configName = xmlRdr.NameTable.Add("Name");
				string targetServer = xmlRdr.NameTable.Add("TargetServer");
				string targetDb = xmlRdr.NameTable.Add("TargetDatabase");

				while (xmlRdr.Read()) {
					if (object.ReferenceEquals(xmlRdr.Name, configName) && xmlRdr.NodeType == XmlNodeType.Element)
					{
						if (string.Compare(xmlRdr.ReadElementContentAsString(), currentConfig, true) == 0)
						{


							while (xmlRdr.Read()) {
								    if (xmlRdr.Name == targetServer)
                                    {
										    mTargetServer = xmlRdr.ReadElementContentAsString();
								    }
                                    else if (xmlRdr.Name == targetDb)
                                    {
										    mTargetDatabase = xmlRdr.ReadElementContentAsString();
								    }
								
							}
							//\\ Exit the outer while
							break; // TODO: might not be correct. Was : Exit While

						}
					}
				}
			}
			finally {
				//// make sure the reader is always closed
				xmlRdr.Close();
			}
		}
	}

	private void SetDefaultTargetServer()
	{
		Microsoft.Win32.RegistryKey regKey;
		regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\VisualStudio\\8.0\\Packages\\{4a0c6509-bf90-43da-abee-0aba3a8527f1}\\Settings\\Analysis Services Project");
		string targetSvr = (string)regKey.GetValue("DefaultTargetServer");
		if (((targetSvr != null)) && (targetSvr.Length > 0))
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
