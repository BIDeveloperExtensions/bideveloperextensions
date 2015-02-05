using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Varigence.Flow.FlowFramework.Validation;
using Varigence.Languages.Biml;
using Varigence.Languages.Biml.Platform;
using System.Xml;

namespace BIDSHelper.SSIS.Biml
{
    public class BimlExpandPlugin : BimlFeaturePluginBase
    {
        public BimlExpandPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        #region Standard Property Overrides
        public override string ShortName
        {
            get { return "BimlExpandPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override System.Drawing.Icon CustomMenuIcon
        {
            get { return BIDSHelper.Resources.Common.Biml; }
        }

        public override string ButtonText
        {
            get { return "Generate SSIS Packages"; }
        }

        public override string ToolTip
        {
            get { return "Expand BimlScript file into one or more SSIS packages in your project"; }
        }

        public override string MenuName
        {
            get { return "Item"; }
        }
        #endregion

        public override bool DisplayCommand(UIHierarchyItem item)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            foreach (object selected in ((System.Array)solExplorer.SelectedItems))
            {
                UIHierarchyItem hierItem = (UIHierarchyItem)selected;
                ProjectItem projectItem = hierItem.Object as ProjectItem;
                if (projectItem == null || !projectItem.Name.ToLower().EndsWith(".biml"))
                {
                    return false;
                }
            }

            return (((System.Array)solExplorer.SelectedItems).Length > 0);
        }

        public override void Exec()
        {
            if (!BimlUtility.CheckRequiredFrameworkVersion())
            {
                return;
            }

            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            var emittableFilePaths = new List<string>();
            Project containingProject = null;
            string projectDirectory = null;
            bool sameProject = false;
            foreach (object selected in ((System.Array)solExplorer.SelectedItems))
            {
                UIHierarchyItem hierItem = (UIHierarchyItem)selected;
                ProjectItem projectItem = hierItem.Object as ProjectItem;
                if (projectItem != null && projectItem.Name.ToLower().EndsWith(".biml"))
                {
                    if (projectItem.Document != null && !projectItem.Document.Saved)
                    {
                        projectItem.Document.Save(null);
                    }

                    Project newContainingProject = projectItem.ContainingProject;
                    sameProject = containingProject == null || containingProject == newContainingProject;
                    containingProject = projectItem.ContainingProject;
                    projectDirectory = Path.GetDirectoryName(containingProject.FullName);
                    string filePath = Path.Combine(projectDirectory, projectItem.Name);
                    emittableFilePaths.Add(filePath);
                }
            }

            if (sameProject && containingProject != null && projectDirectory != null)
            {
                try
                {
                    Expand(emittableFilePaths, containingProject, projectDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Expand(List<string> bimlScriptPaths, Project project, string projectDirectory)
        {
            var tempTargetDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(tempTargetDirectory);

#if KATMAI
                SsisVersion ssisVersion = BimlUtility.GetSsisVersion2008Variant();
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Biml.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2008, ssisVersion, SsasVersion.Ssas2008, SsisDeploymentModel.Package);
#elif DENALI
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Biml.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2008, SsisVersion.Ssis2012, SsasVersion.Ssas2008, DeployPackagesPlugin.IsLegacyDeploymentMode(project) ? SsisDeploymentModel.Package : SsisDeploymentModel.Project);
#elif SQL2014
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Biml.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2008, SsisVersion.Ssis2014, SsasVersion.Ssas2008, DeployPackagesPlugin.IsLegacyDeploymentMode(project) ? SsisDeploymentModel.Package : SsisDeploymentModel.Project);
#else
                ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Biml.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2005, SsisVersion.Ssis2005, SsasVersion.Ssas2005, SsisDeploymentModel.Package);
#endif

                if (validationReporter.HasErrors)
                {
                    var form = new BimlValidationListForm(validationReporter, true);
                    form.ShowDialog();
                }
                else
                {
                    if (validationReporter.HasErrors || validationReporter.HasWarnings)
                    {
                        var form = new BimlValidationListForm(validationReporter, true);
                        form.ShowDialog();
                    }

                    List<string> newProjectFiles = new List<string>();
                    string[] newPackageFiles = Directory.GetFiles(tempTargetDirectory, "*.dtsx", SearchOption.AllDirectories);
                    newProjectFiles.AddRange(newPackageFiles);

#if (!YUKON && !KATMAI)
                    //IF DENALI or later...
                    // Read packages AND project connection managers
                    string[] newConnFiles = Directory.GetFiles(tempTargetDirectory, "*.conmgr", SearchOption.AllDirectories);
                    newProjectFiles.AddRange(newConnFiles);
#endif
                    var safePackageFilePaths = new List<string>();
                    var conflictingPackageFilePaths = new List<string>();
                    foreach (var tempFilePath in newProjectFiles)
                    {
                        string tempFileName = Path.GetFileName(tempFilePath);
                        string projectItemFileName = Path.Combine(projectDirectory, tempFileName);
                        if (File.Exists(projectItemFileName))
                        {
                            conflictingPackageFilePaths.Add(tempFilePath);
                        }
                        else
                        {
                            safePackageFilePaths.Add(tempFilePath);
                        }
                    }

                    if (conflictingPackageFilePaths.Count > 0)
                    {
                        var dialog = new MultipleSelectionConfirmationDialog(conflictingPackageFilePaths, projectDirectory, safePackageFilePaths.Count);
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            foreach (var filePath in dialog.SelectedFilePaths)
                            {
                                safePackageFilePaths.Add(filePath);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

#if (DENALI || SQL2014)
                    /*
                     * Make sure that the package correctly references the Project connection manager, if used
                     */
                    List<ProjectConnectionManagerInfo> prjConnInfoList = new List<ProjectConnectionManagerInfo>();

                    // STEP 1 - Store all existing Project Connection Managers
                    foreach (ProjectItem item in project.ProjectItems)
                    {
                        if (item.Name.EndsWith(".conmgr"))
                        {
                            string fileFullPath = item.FileNames[0];

                            XmlReader r = new XmlTextReader(fileFullPath);
                            XmlDocument doc = new XmlDocument();
                            doc.Load(r);

                            //XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(doc.NameTable);
                            //xmlnsManager.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");

                            XmlElement node = doc.DocumentElement;

                            string prefix = node.GetPrefixOfNamespace("www.microsoft.com/SqlServer/Dts");

                            XmlAttribute xaObjectName = node.Attributes[prefix + ":ObjectName"];
                            XmlAttribute xaDTSID = node.Attributes[prefix + ":DTSID"];

                            if (xaObjectName == null) throw new ApplicationException("ObjectName attribute cannot found.");
                            if (xaDTSID == null) throw new ApplicationException("DTSID attribute cannot found.");

                            prjConnInfoList.Add(new ProjectConnectionManagerInfo(fileFullPath, xaObjectName.Value, xaDTSID.Value));

                            r.Close();
                        }
                    }

                    // STEP 2 - For all the Connection Managers that have to be inserted in the solution,
                    //          if a connection manager with the same name alread exists, use the existing GUID
                    //          to avoid corrupting the package that will be inserted
                    foreach (var tempFilePath in safePackageFilePaths)
                    {
                        if (tempFilePath.EndsWith(".conmgr"))
                        {
                            string fileFullPath = tempFilePath;

                            XmlReader r = new XmlTextReader(fileFullPath);
                            XmlDocument doc = new XmlDocument();
                            doc.Load(r);

                            //XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(doc.NameTable);
                            //xmlnsManager.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");

                            XmlElement node = doc.DocumentElement;

                            string prefix = node.GetPrefixOfNamespace("www.microsoft.com/SqlServer/Dts");

                            bool saveRequired = false;
                            ProjectConnectionManagerInfo pcmi = prjConnInfoList.Find(x => System.IO.Path.GetFileName(x.FileFullPath) == System.IO.Path.GetFileName(fileFullPath));
                            if (pcmi != null)
                            {
                                XmlAttribute xaDTSID = node.Attributes[prefix + ":DTSID"];
                                if (xaDTSID == null) throw new ApplicationException("DTSID attribute cannot found.");

                                xaDTSID.Value = pcmi.DTSID;
                                saveRequired = true;
                            }
                            else
                            {
                                XmlAttribute xaObjectName = node.Attributes[prefix + ":ObjectName"];
                                XmlAttribute xaDTSID = node.Attributes[prefix + ":DTSID"];

                                if (xaObjectName == null) throw new ApplicationException("ObjectName attribute cannot found.");
                                if (xaDTSID == null) throw new ApplicationException("DTSID attribute cannot found.");

                                prjConnInfoList.Add(new ProjectConnectionManagerInfo(fileFullPath, xaObjectName.Value, xaDTSID.Value));
                            }

                            r.Close();

                            if (saveRequired) doc.Save(fileFullPath);
                        }
                    }

                    // STEP 3 - For Each NEW package make sure that the Project Connection Manager
                    //          points to the correct GUID
                    foreach (var tempFilePath in safePackageFilePaths)
                    {
                        if (tempFilePath.EndsWith(".dtsx"))
                        {
                            string fileFullPath = tempFilePath;

                            XmlReader r = new XmlTextReader(fileFullPath);
                            XmlDocument doc = new XmlDocument();
                            doc.Load(r);

                            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(doc.NameTable);
                            xmlnsManager.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");

                            XmlElement root = doc.DocumentElement;

                            XmlNodeList nodes = root.SelectNodes("//connection", xmlnsManager);
                            foreach (XmlNode n in nodes)
                            {
                                string refID = n.Attributes["connectionManagerRefId"].Value;
                                ProjectConnectionManagerInfo pcmi = prjConnInfoList.Find(x => "Package.ConnectionManagers[" + x.ObjectName + "]" == refID);
                                if (pcmi == null)
                                {
                                    pcmi = prjConnInfoList.Find(x => "Project.ConnectionManagers[" + x.ObjectName + "]" == refID);
                                }

                                if (pcmi != null)
                                {
                                    // If a local connection manager does NOT exists, then point to the project connection manager
                                    XmlNode node = root.SelectSingleNode("/DTS:Executable/DTS:ConnectionManagers/DTS:ConnectionManager[@DTS:refId=\"" + pcmi.ObjectName + "\"]", xmlnsManager);
                                    if (node == null)
                                    {
                                        n.Attributes["connectionManagerID"].Value = pcmi.DTSID + ":external";
                                        n.Attributes["connectionManagerRefId"].Value = "Project.ConnectionManagers[" + pcmi.ObjectName + "]";
                                    }
                                }
                            }

                            r.Close();

                            doc.Save(fileFullPath);
                        }
                    }
#endif

                    // Add files to VS Project
                    foreach (var tempFilePath in safePackageFilePaths)
                    {
                        string projectItemFilePath = Path.Combine(projectDirectory, Path.GetFileName(tempFilePath));
                        File.Copy(tempFilePath, projectItemFilePath, true);
                        project.ProjectItems.AddFromFile(projectItemFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempTargetDirectory))
                    {
                        Directory.Delete(tempTargetDirectory);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}