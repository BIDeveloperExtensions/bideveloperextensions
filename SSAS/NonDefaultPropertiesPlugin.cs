extern alias asAlias;
using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using System.Reflection;
using Microsoft.Win32;
using System.ComponentModel.Design;
using System.ComponentModel;
using Microsoft.DataWarehouse.Design;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.DataWarehouse.Controls;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using IDTSConnectionManagerDatabaseParametersXX = Microsoft.SqlServer.Dts.Runtime.Wrapper.IDTSConnectionManagerDatabaseParameters100;
using IDTSComponentMetaDataXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData100;
using IDTSCustomPropertyXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomProperty100;
using IDTSObjectXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSObject100;
using IDTSOutputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput100;
using BIDSHelper.Core;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.General)]
    public class NonDefaultPropertiesPlugin : BIDSHelperPluginBase
    {
        private List<NonDefaultProperty> listNonDefaultProperties;
        private string DatabaseName;
        private bool SSASProject = true;
        private string PackagePathPrefix;
        private Package packageDefault;
        private Dictionary<string, DtsObject> dictCachedDtsObjects;
        ProjectItem piCurrent = null;


        #region Standard Plugin Overrides
        public NonDefaultPropertiesPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.NonDefaultPropertiesReportId);
        }

        public override string ShortName
        {
            get { return "NonDefaultProperties"; }
        }

        //public override int Bitmap
        //{
        //    get { return 2035; }
        //}

        //public override string ButtonText
        //{
        //    get { return "Non-Default Properties Report..."; }
        //}

        public override string FeatureName
        {
            get { return "Non-Default Properties Report"; }
        }

        //public override string MenuName
        //{
        //    get { return "Project,Item,Solution"; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        
        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.General; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Provides a report of properties which have been changed from their defaults. The report can be run for Analysis Services or for Integration Services."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ShouldDisplayCommand()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;
                var proj = GetSelectedProjectReference();
                EnvDTE.Project projAS = null;
                if (BIDSHelperPackage.SSASExtensionVersion != null) projAS = GetSelectedProjectReferenceAS();

                if (proj != null)
                {
                    if (proj.Object == null || proj.Object.GetType().FullName != "Microsoft.AnalysisServices.Database") return false; //this should be a reference to Microsoft.AnalysisServices.AppLocal.dll
                    //Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)proj;
                    return (proj.Kind == BIDSProjectKinds.SSAS || proj.Kind == BIDSProjectKinds.SSIS);
                }
                else if (projAS != null)
                {
                    if (projAS.Object == null || projAS.Object.GetType().FullName != "Microsoft.AnalysisServices.Database") return false;
                    //asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)projAS;
                    return (projAS.Kind == BIDSProjectKinds.SSAS || projAS.Kind == BIDSProjectKinds.SSIS);
                }
                else if (solution != null)
                {
                    foreach (EnvDTE.Project p in solution.Projects)
                    {
                        if (p.Kind != BIDSProjectKinds.SSIS) return false;
                    }
                    return (solution.Projects.Count > 0);
                }
                else if (hierItem.Object is ProjectItem)
                {
                    string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                    return (sFileName.EndsWith(".dtsx"));
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.package.Log.Exception("Error in NonDefaultPropertiesPlugin ShouldDisplayCommand", ex);
                this.package.Log.Error(ex.Message + "\r\n" + ex.StackTrace);
            }
            return false;
        }
        #endregion

        private bool ExecAS(EnvDTE.Project p)
        {
            asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExtAS = p as asAlias::Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
            if (projExtAS != null && projExtAS.Kind == BIDSProjectKinds.SSAS)
            {
                Database db = (Database)p.Object;
                ScanAnalysisServicesProperties(db);
                return true;
            }
            return false;
        }

        public override void Exec()
        {
            piCurrent = null;
            try
            {
                listNonDefaultProperties = new List<NonDefaultProperty>();

                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;
                EnvDTE.Project p = GetSelectedProjectReference();
                if (p == null && BIDSHelperPackage.SSASExtensionVersion != null) p = GetSelectedProjectReferenceAS();
                if (p != null)
                {
                    Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = p as Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt;
                    if (BIDSHelperPackage.SSASExtensionVersion != null && ExecAS(p))
                    {
                        //ExecAS has already done all there is to do
                    }
                    else
                    {
                        this.DatabaseName = "Project: " + p.Name;
                        this.SSASProject = false;

                        try
                        {
                            using (WaitCursor cursor1 = new WaitCursor())
                            {
                                ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationGeneral);
                                OrchestrateSSISProjectScan(p, hierItem);
                            }
                        }
                        finally
                        {
                            try
                            {
                                ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationGeneral);
                                ApplicationObject.StatusBar.Progress(false, "", 1, 1);
                            }
                            catch { }
                        }
                    }
                }
                else if (solution != null)
                {
                    this.DatabaseName = "Solution: " + System.IO.Path.GetFileNameWithoutExtension(solution.FullName);
                    try
                    {
                        this.DatabaseName = "Solution: " + solution.Properties.Item("Name").Value;
                    }
                    catch { }

                    this.SSASProject = false;

                    try
                    {
                        using (WaitCursor cursor1 = new WaitCursor())
                        {
                            ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationGeneral);
                            OrchestrateSSISSolutionScan(solution);
                        }
                    }
                    finally
                    {
                        try
                        {
                            ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationGeneral);
                            ApplicationObject.StatusBar.Progress(false, "", 1, 1);
                        }
                        catch { }
                    }
                }
                else
                {
                    OrchestrateSSISPackageScan(hierItem);
                }

                //clear the cache
                if (BIDSHelperPackage.SSISExtensionVersion != null)
                {
                    ClearSSISCacheObjects();
                }

                if (listNonDefaultProperties.Count == 0)
                {
                    MessageBox.Show("No properties set to non-default values were found.", "BIDS Helper - Non-Default Properties Report");
                    return;
                }

                //pop up the form to let the user exclude properties from showing on the report
                List<string> listExcludedProperties = new List<string>(this.ExcludedProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                BIDSHelper.SSAS.NonDefaultPropertiesSelectionForm selector = new BIDSHelper.SSAS.NonDefaultPropertiesSelectionForm();
                foreach (NonDefaultProperty prop in listNonDefaultProperties)
                {
                    if (!selector.listProperties.Items.Contains(prop.PropertyName))
                    {
                        bool bChecked = !listExcludedProperties.Contains(prop.PropertyName);
                        selector.listProperties.Items.Add(prop.PropertyName, bChecked);
                    }
                }

                DialogResult selectorResult = selector.ShowDialog();
                if (selectorResult == DialogResult.OK)
                {
                    //remove the the report rows they unchecked
                    for (int i = 0; i < listNonDefaultProperties.Count; i++)
                    {
                        if (!selector.listProperties.CheckedItems.Contains(listNonDefaultProperties[i].PropertyName))
                        {
                            listNonDefaultProperties.RemoveAt(i--);
                        }
                    }

                    //save their prefs... keep previously prefs which haven't been changes (because an excluded property may not show up in the possible properties list each time you run the report)
                    foreach (object item in selector.listProperties.Items)
                    {
                        if (!selector.listProperties.CheckedItems.Contains(item)) //if excluded, then add to the excluded list
                        {
                            if (!listExcludedProperties.Contains(item.ToString()))
                                listExcludedProperties.Add(item.ToString());
                        }
                        else //if included, then remove from the excluded list
                        {
                            if (listExcludedProperties.Contains(item.ToString()))
                                listExcludedProperties.Remove(item.ToString());
                        }
                    }
                    this.ExcludedProperties = string.Join(",", listExcludedProperties.ToArray());

                    ReportViewerForm frm = new ReportViewerForm();
                    frm.ReportBindingSource.DataSource = this.listNonDefaultProperties;
                    frm.Report = "SSAS.NonDefaultProperties.rdlc";
                    Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                    reportDataSource1.Name = "BIDSHelper_NonDefaultProperty";
                    reportDataSource1.Value = frm.ReportBindingSource;
                    frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                    frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = frm.Report;

                    frm.Caption = "Non-Default Properties Report";
                    frm.WindowState = FormWindowState.Maximized;
                    frm.Show();
                }
            }
            catch (System.Exception ex)
            {
                if (ex.GetType().Name == "DtsRuntimeException")
                    HandleDtsRuntimeException(ex);
                else
                    MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error" + (piCurrent == null ? string.Empty : " scanning " + piCurrent.Name));
            }
        }

        private void HandleDtsRuntimeException(Exception exGeneric)
        {
            DtsRuntimeException ex = (DtsRuntimeException)exGeneric;
            if (ex.ErrorCode == -1073659849L)
            {
                MessageBox.Show((piCurrent == null ? "This package" : piCurrent.Name) + " has a package password. Please open the package designer, specify the password when the dialog prompts you, then rerun the Non-Default Properties report.\r\n\r\nDetailed error was:\r\n" + ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Password Not Specified");
            }
            else
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error" + (piCurrent == null ? string.Empty : " scanning " + piCurrent.Name));
            }
        }

        private void OrchestrateSSISPackageScan(UIHierarchyItem hierItem)
        {
            ProjectItem pi = (ProjectItem)hierItem.Object;
            piCurrent = pi;
            Package package = GetPackageFromIntegrationServicesProjectItem(pi);

            this.DatabaseName = "Package: " + package.Name;
            this.SSASProject = false;
            this.PackagePathPrefix = string.Empty;

            ScanIntegrationServicesProperties(package);
        }

        private void OrchestrateSSISProjectScan(EnvDTE.Project p, UIHierarchyItem hierItem)
        {
            SSIS.PackageHelper.SetTargetServerVersion(p);
            int iProgress = 0;
            Microsoft.SqlServer.Dts.Runtime.Application app = SSIS.PackageHelper.Application; //sets the proper TargetServerVersion
            foreach (ProjectItem pi in p.ProjectItems)
            {
                if (hierItem.Name != null && hierItem.Name.ToLower().EndsWith(".dtsx") && hierItem.Name != pi.Name) continue;
                ApplicationObject.StatusBar.Progress(true, "Scanning package " + pi.Name, iProgress++, p.ProjectItems.Count);
                string sFileName = pi.Name.ToLower();
                if (!sFileName.EndsWith(".dtsx")) continue;
                piCurrent = pi;
                this.PackagePathPrefix = pi.Name;
                Package package = GetPackageFromIntegrationServicesProjectItem(pi);
                ScanIntegrationServicesProperties(package);
            }
        }


        private void OrchestrateSSISSolutionScan(SolutionClass solution)
        {
            foreach (EnvDTE.Project proj in solution.Projects)
            {
                SSIS.PackageHelper.SetTargetServerVersion(proj);
                Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)proj;
                if (projExt.Kind == BIDSProjectKinds.SSIS)
                {
                    int iProgress = 0;
                    Microsoft.SqlServer.Dts.Runtime.Application app = SSIS.PackageHelper.Application; //sets the proper TargetServerVersion;
                    foreach (ProjectItem pi in proj.ProjectItems)
                    {
                        ApplicationObject.StatusBar.Progress(true, "Scanning project " + proj.Name + " package " + pi.Name, iProgress++, proj.ProjectItems.Count);
                        string sFileName = pi.Name.ToLower();
                        if (!sFileName.EndsWith(".dtsx")) continue;
                        piCurrent = pi;
                        this.PackagePathPrefix = proj.Name + "\\" + pi.Name;
                        Package package = GetPackageFromIntegrationServicesProjectItem(pi);
                        ScanIntegrationServicesProperties(package);
                    }
                }
            }
        }

        private void ClearSSISCacheObjects()
        {
            this.packageDefault = null;
            if (this.dictCachedDtsObjects != null)
                this.dictCachedDtsObjects.Clear();
        }

        /// <summary>
        /// If the package designer is already open, then use the package object on it which may have in-memory modifications.
        /// If the package designer is not open, then just load the package from disk.
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        private Package GetPackageFromIntegrationServicesProjectItem(ProjectItem pi)
        {
            bool bIsOpen = pi.get_IsOpen(BIDSViewKinds.Designer);

            if (bIsOpen)
            {
                Window w = pi.Open(BIDSViewKinds.Designer); //opens the designer
                w.Activate();

                IDesignerHost designer = w.Object as IDesignerHost;
                if (designer == null) return null;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                return win.PropertiesLinkComponent as Package;
            }
            else
            {
                Microsoft.SqlServer.Dts.Runtime.Application app = SSIS.PackageHelper.Application; //sets the proper TargetServerVersion
                return app.LoadPackage(pi.get_FileNames(0), null);
            }
        }

        #region SSIS Scanning
        private void ScanIntegrationServicesProperties(Package package)
        {
            RecurseExecutables(package);

            foreach (ConnectionManager conn in package.Connections)
            {
                ScanIntegrationServicesExecutableForPropertiesWithNonDefaultValue(conn, this.PackagePathPrefix + ((IDTSPackagePath)conn).GetPackagePath());
            }
        }

        private void RecurseExecutables(IDTSSequence parentExecutable)
        {
            Package package = GetPackageFromContainer((DtsContainer)parentExecutable);
            ScanIntegrationServicesExecutableForPropertiesWithNonDefaultValue((Executable)parentExecutable, this.PackagePathPrefix + ((IDTSPackagePath)parentExecutable).GetPackagePath());
            if (parentExecutable is EventsProvider)
            {
                foreach (DtsEventHandler eh in (parentExecutable as EventsProvider).EventHandlers)
                {
                    RecurseExecutables(eh);
                }
            }
            foreach (Executable e in parentExecutable.Executables)
            {
                if (e is IDTSSequence)
                {
                    RecurseExecutables((IDTSSequence)e);
                }
                else
                {
                    ScanIntegrationServicesExecutableForPropertiesWithNonDefaultValue(e, this.PackagePathPrefix + ((IDTSPackagePath)e).GetPackagePath());

                    if (e is EventsProvider)
                    {
                        foreach (DtsEventHandler eh in (e as EventsProvider).EventHandlers)
                        {
                            RecurseExecutables(eh);
                        }
                    }
                }
            }
        }

        private Package GetPackageFromContainer(DtsContainer container)
        {
            while (!(container is Package))
            {
                container = container.Parent;
            }
            return (Package)container;
        }

        private void ScanIntegrationServicesExecutableForPropertiesWithNonDefaultValue(DtsObject o, string FriendlyPath)
        {
            if (o == null) return;

            if (packageDefault == null)
            {
                packageDefault = new Package();
            }
            if (dictCachedDtsObjects == null)
            {
                dictCachedDtsObjects = new Dictionary<string, DtsObject>();
            }

            DtsObject defaultObject;
            if (o is Package)
            {
                defaultObject = (Package)packageDefault;
            }
            else if (o is IDTSName)
            {
                if (dictCachedDtsObjects.ContainsKey(((IDTSName)o).CreationName))
                {
                    defaultObject = dictCachedDtsObjects[((IDTSName)o).CreationName];
                }
                else if (o is DtsEventHandler)
                {
                    defaultObject = (DtsObject)((Package)packageDefault).EventHandlers.Add(((IDTSName)o).CreationName);
                    dictCachedDtsObjects.Add(((IDTSName)o).CreationName, defaultObject);
                }
                else if (o is ConnectionManager)
                {
                    defaultObject = (DtsObject)((Package)packageDefault).Connections.Add(((IDTSName)o).CreationName);
                    dictCachedDtsObjects.Add(((IDTSName)o).CreationName, defaultObject);
                }
                else
                {
                    defaultObject = ((Package)packageDefault).Executables.Add(((IDTSName)o).CreationName);
                    dictCachedDtsObjects.Add(((IDTSName)o).CreationName, defaultObject);
                }
            }
            else
            {
                throw new Exception("Object " + o.GetType().FullName + " does not implement IDTSName.");
            }

            PropertyInfo[] properties = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;

                //SSIS objects don't have a DefaultValueAttribute, which is wy we have to create a new control flow object (defaultObject) above and compare properties from this object to that object

                object[] attrs = prop.GetCustomAttributes(typeof(System.ComponentModel.BrowsableAttribute), true);
                System.ComponentModel.BrowsableAttribute browsableAttr = (System.ComponentModel.BrowsableAttribute)(attrs.Length > 0 ? attrs[0] : null);
                if (browsableAttr != null && !browsableAttr.Browsable) continue; //don't show attributes marked not browsable

                attrs = prop.GetCustomAttributes(typeof(System.ComponentModel.ReadOnlyAttribute), true);
                System.ComponentModel.ReadOnlyAttribute readOnlyAttr = (System.ComponentModel.ReadOnlyAttribute)(attrs.Length > 0 ? attrs[0] : null);
                if (readOnlyAttr != null && readOnlyAttr.IsReadOnly) continue; //don't show attributes marked read only

                if (prop.PropertyType.Namespace != "System" && !prop.PropertyType.IsPrimitive && !prop.PropertyType.IsValueType && !prop.PropertyType.IsEnum) continue;
                if (prop.PropertyType == typeof(DateTime)) continue;
                if (prop.PropertyType == typeof(string)) continue;
                if (prop.Name == "VersionBuild") continue;
                if (prop.Name == "VersionMajor") continue;
                if (prop.Name == "VersionMinor") continue;
                if (prop.Name == "PackageType") continue;

                object value = prop.GetValue(o, null);
                object defaultValue = prop.GetValue(defaultObject, null);
                if (defaultValue != null && !defaultValue.Equals(value))
                {
                    string sValue = (value == null ? string.Empty : value.ToString());
                    this.listNonDefaultProperties.Add(new NonDefaultProperty(this.DatabaseName, FriendlyPath, prop.Name, defaultValue.ToString(), sValue));
                }
            }

            if (o is IDTSObjectHost)
            {
                IDTSObjectHost host = (IDTSObjectHost)o;
                if (host.InnerObject is Microsoft.SqlServer.Dts.Pipeline.Wrapper.MainPipe)
                    properties = typeof(Microsoft.SqlServer.Dts.Pipeline.Wrapper.MainPipe).GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                else if (host.InnerObject is IDTSConnectionManagerDatabaseParametersXX)
                    properties = typeof(IDTSConnectionManagerDatabaseParametersXX).GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                else //probably won't turn up any properties because reflection on a COM object Type doesn't work
                    properties = host.InnerObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);

                foreach (PropertyInfo prop in properties)
                {
                    if (!prop.CanWrite || !prop.CanRead) continue;

                    object[] attrs = prop.GetCustomAttributes(typeof(System.ComponentModel.BrowsableAttribute), true);
                    System.ComponentModel.BrowsableAttribute browsableAttr = (System.ComponentModel.BrowsableAttribute)(attrs.Length > 0 ? attrs[0] : null);
                    if (browsableAttr != null && !browsableAttr.Browsable) continue; //don't show attributes marked not browsable

                    attrs = prop.GetCustomAttributes(typeof(System.ComponentModel.ReadOnlyAttribute), true);
                    System.ComponentModel.ReadOnlyAttribute readOnlyAttr = (System.ComponentModel.ReadOnlyAttribute)(attrs.Length > 0 ? attrs[0] : null);
                    if (readOnlyAttr != null && readOnlyAttr.IsReadOnly) continue; //don't show attributes marked read only

                    if (prop.PropertyType.Namespace != "System" && !prop.PropertyType.IsPrimitive && !prop.PropertyType.IsValueType && !prop.PropertyType.IsEnum) continue;
                    if (prop.PropertyType == typeof(DateTime)) continue;
                    if (prop.PropertyType == typeof(string)) continue;
                    if (prop.Name == "VersionBuild") continue;
                    if (prop.Name == "VersionMajor") continue;
                    if (prop.Name == "VersionMinor") continue;
                    if (prop.Name == "PackageType") continue;
                    if (prop.Name.StartsWith("IDTS")) continue;

                    object value;
                    object defaultValue;
                    if (host.InnerObject is Microsoft.SqlServer.Dts.Pipeline.Wrapper.MainPipe)
                    {
                        try
                        {
                            value = host.InnerObject.GetType().InvokeMember(prop.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, host.InnerObject, null);
                        }
                        catch
                        {
                            continue;
                        }
                        try
                        {
                            defaultValue = ((IDTSObjectHost)defaultObject).InnerObject.GetType().InvokeMember(prop.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, ((IDTSObjectHost)defaultObject).InnerObject, null);
                        }
                        catch
                        {
                            defaultValue = null;
                        }
                    }
                    else
                    {
                        value = prop.GetValue(host.InnerObject, null);
                        defaultValue = prop.GetValue(((IDTSObjectHost)defaultObject).InnerObject, null);
                    }
                    if (defaultValue != null && !defaultValue.Equals(value))
                    {
                        string sValue = (value == null ? string.Empty : value.ToString());
                        this.listNonDefaultProperties.Add(new NonDefaultProperty(this.DatabaseName, FriendlyPath, prop.Name, defaultValue.ToString(), sValue));
                    }
                }

                //scan data flow transforms
                if (host.InnerObject is Microsoft.SqlServer.Dts.Pipeline.Wrapper.MainPipe)
                {
                    Microsoft.SqlServer.Dts.Pipeline.Wrapper.MainPipe pipe = (Microsoft.SqlServer.Dts.Pipeline.Wrapper.MainPipe)host.InnerObject;
                    Microsoft.SqlServer.Dts.Pipeline.Wrapper.MainPipe defaultPipe = (Microsoft.SqlServer.Dts.Pipeline.Wrapper.MainPipe)((IDTSObjectHost)defaultObject).InnerObject;
                    foreach (IDTSComponentMetaDataXX transform in pipe.ComponentMetaDataCollection)
                    {
                        IDTSComponentMetaDataXX defaultTransform = defaultPipe.ComponentMetaDataCollection.New();
                        defaultTransform.ComponentClassID = transform.ComponentClassID;
                        CManagedComponentWrapper defaultInst = defaultTransform.Instantiate();
                        try
                        {
                            defaultInst.ProvideComponentProperties();
                        }
                        catch
                        {
                            continue; //if there's a corrupt package (or if you don't have the component installed on your laptop?) then this might fail... so just move on
                        }

                        if (!transform.ValidateExternalMetadata) //this property isn't in the CustomPropertyCollection, so we have to check it manually
                        {
                            this.listNonDefaultProperties.Add(new NonDefaultProperty(this.DatabaseName, FriendlyPath + "\\" + transform.Name, "ValidateExternalMetadata", "True", "False"));
                        }
                        foreach (IDTSOutputXX output in transform.OutputCollection) //check for error row dispositions
                        {
                            if (output.ErrorRowDisposition == DTSRowDisposition.RD_IgnoreFailure)
                            {
                                this.listNonDefaultProperties.Add(new NonDefaultProperty(this.DatabaseName, FriendlyPath + "\\" + transform.Name + "\\" + output.Name, "ErrorRowDisposition", "FailComponent", "IgnoreFailure"));
                            }
                            if (output.TruncationRowDisposition == DTSRowDisposition.RD_IgnoreFailure)
                            {
                                this.listNonDefaultProperties.Add(new NonDefaultProperty(this.DatabaseName, FriendlyPath + "\\" + transform.Name + "\\" + output.Name, "TruncationRowDisposition", "FailComponent", "IgnoreFailure"));
                            }
                        }

                        Microsoft.DataTransformationServices.Design.PipelinePropertiesWrapper propWrapper = new Microsoft.DataTransformationServices.Design.PipelinePropertiesWrapper(transform, transform, 0);

                        foreach (IDTSCustomPropertyXX prop in transform.CustomPropertyCollection)
                        {
                            System.ComponentModel.PropertyDescriptor propDesc = (System.ComponentModel.PropertyDescriptor)propWrapper.GetType().InvokeMember("CreateCustomPropertyPropertyDescriptor", BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance, null, propWrapper, new object[] { prop });
                            if (propDesc == null) continue;
                            if (propDesc.IsReadOnly) continue;
                            if (!propDesc.IsBrowsable) continue;
                            if (prop.Value is string) continue;
                            if (prop.Value is DateTime) continue;
                            IDTSCustomPropertyXX defaultProp;
                            try
                            {
                                defaultProp = defaultTransform.CustomPropertyCollection[prop.Name];
                            }
                            catch
                            {
                                if (prop.Name == "PreCompile" && bool.Equals(prop.Value, false)) //this property doesn't show up in the new script component we created to determine defaults, so we have to check it manually
                                {
                                    this.listNonDefaultProperties.Add(new NonDefaultProperty(this.DatabaseName, FriendlyPath + "\\" + transform.Name, prop.Name, "True", "False"));
                                }
                                continue;
                            }

                            System.ComponentModel.ITypeDescriptorContext context = new PipelinePropertyContext(transform, propDesc);
                            string sValue = propDesc.Converter.ConvertToString(context, prop.Value); //gets nice text descriptions for enums on component properties
                            string sDefaultValue = propDesc.Converter.ConvertToString(context, defaultProp.Value); //gets nice text descriptions for enums on component properties
                            if (sValue == sDefaultValue) continue;

                            this.listNonDefaultProperties.Add(new NonDefaultProperty(this.DatabaseName, FriendlyPath + "\\" + transform.Name, prop.Name, sDefaultValue, sValue));
                        }

                        defaultPipe.ComponentMetaDataCollection.RemoveObjectByID(defaultTransform.ID);
                    }
                }
            }
        }
        #endregion

        #region SSAS Scanning
        private void ScanAnalysisServicesProperties(Database db)
        {
            this.DatabaseName = "Database: " + db.Name;
            this.SSASProject = true;

            ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(db);
            foreach (DataSource ds in db.DataSources)
            {
                ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(ds);
            }
            foreach (Cube c in db.Cubes)
            {
                ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(c);
                ScanAnalysisServicesPropertyForNonDefaultValue(c, "DefaultMeasure");
                ScanAnalysisServicesPropertyForNonDefaultValue(c, "StorageLocation");
                foreach (MeasureGroup mg in c.MeasureGroups)
                {
                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(mg);
                    ScanAnalysisServicesPropertyForNonDefaultValue(mg, "StorageLocation");
                    foreach (Measure m in mg.Measures)
                    {
                        ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(m);
                        ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(m.Source, ((IModelComponent)m).FriendlyPath + " > Source");
                        ScanAnalysisServicesPropertyForNonDefaultValue(m, "MeasureExpression");
                    }
                    foreach (Partition part in mg.Partitions)
                    {
                        ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(part);
                        ScanAnalysisServicesPropertyForNonDefaultValue(part, "StorageLocation");
                        ScanAnalysisServicesPropertyForNonDefaultValue(part, "Slice");
                    }
                    foreach (MeasureGroupDimension mgd in mg.Dimensions)
                    {
                        ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(mgd);
                        if (mgd is ManyToManyMeasureGroupDimension)
                        {
                            ScanAnalysisServicesPropertyForNonDefaultValue(mgd, "DirectSlice");
                        }
                        else if (mgd is RegularMeasureGroupDimension)
                        {
                            foreach (MeasureGroupAttribute mga in (mgd as RegularMeasureGroupDimension).Attributes)
                            {
                                foreach (DataItem di in mga.KeyColumns)
                                {
                                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(di, ((IModelComponent)mga).FriendlyPath + " > KeyColumn '" + di.ToString() + "'");
                                }
                                //don't actually scan the MeasureGroupAttribute... nothing of interest in there
                            }
                        }
                    }
                }
                foreach (CubeDimension cd in c.Dimensions)
                {
                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(cd);
                    foreach (CubeAttribute ca in cd.Attributes)
                    {
                        ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(ca);
                    }
                    foreach (CubeHierarchy ch in cd.Hierarchies)
                    {
                        ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(ch);
                    }
                }
            }

            foreach (Dimension d in db.Dimensions)
            {
                ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(d);
                ScanAnalysisServicesPropertyForNonDefaultValue(d, "AttributeAllMemberName");
                ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(d.Source, ((IModelComponent)d).FriendlyPath + " > Source");
                foreach (DimensionAttribute da in d.Attributes)
                {
                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(da);
                    foreach (DataItem di in da.KeyColumns)
                    {
                        ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(di, ((IModelComponent)da).FriendlyPath + " > KeyColumn '" + di.ToString() + "'");
                    }
                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(da.NameColumn, ((IModelComponent)da).FriendlyPath + " > NameColumn");
                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(da.ValueColumn, ((IModelComponent)da).FriendlyPath + " > ValueColumn");
                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(da.CustomRollupColumn, ((IModelComponent)da).FriendlyPath + " > CustomRollupColumn");
                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(da.CustomRollupPropertiesColumn, ((IModelComponent)da).FriendlyPath + " > CustomRollupPropertiesColumn");
                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(da.UnaryOperatorColumn, ((IModelComponent)da).FriendlyPath + " > UnaryOperatorColumn");
                    foreach (AttributeRelationship r in da.AttributeRelationships)
                    {
                        ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(r);
                    }
                }
                foreach (Hierarchy h in d.Hierarchies)
                {
                    ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(h);
                    ScanAnalysisServicesPropertyForNonDefaultValue(h, "AllMemberName");
                }
            }
        }

        private void ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(object o)
        {
            ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(o, ((IModelComponent)o).FriendlyPath);
        }


        private void ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(object o, string FriendlyPath)
        {
            if (o == null) return;

            PropertyInfo[] properties = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;

                object[] attrs = prop.GetCustomAttributes(typeof(System.ComponentModel.DefaultValueAttribute), true);
                System.ComponentModel.DefaultValueAttribute defaultAttr = (System.ComponentModel.DefaultValueAttribute)(attrs.Length > 0 ? attrs[0] : null);
                if (defaultAttr == null) continue; //only show properties with defaults

                attrs = prop.GetCustomAttributes(typeof(System.ComponentModel.BrowsableAttribute), true);
                System.ComponentModel.BrowsableAttribute browsableAttr = (System.ComponentModel.BrowsableAttribute)(attrs.Length > 0 ? attrs[0] : null);
                if (browsableAttr != null && !browsableAttr.Browsable) continue; //don't show attributes marked not browsable

                attrs = prop.GetCustomAttributes(typeof(System.ComponentModel.ReadOnlyAttribute), true);
                System.ComponentModel.ReadOnlyAttribute readOnlyAttr = (System.ComponentModel.ReadOnlyAttribute)(attrs.Length > 0 ? attrs[0] : null);
                if (readOnlyAttr != null && readOnlyAttr.IsReadOnly) continue; //don't show attributes marked read only

                if (prop.PropertyType.Namespace != "System" && !prop.PropertyType.IsPrimitive && !prop.PropertyType.IsValueType && !prop.PropertyType.IsEnum)
                {
                    object v = prop.GetValue(o, null);
                    if (v != null)
                        ScanAnalysisServicesObjectForPropertiesWithNonDefaultValue(v, FriendlyPath + " > " + prop.Name);
                    continue;
                }

                object value = prop.GetValue(o, null);
                if (defaultAttr.Value != null && !defaultAttr.Value.Equals(value))
                {
                    string sValue = (value == null ? string.Empty : value.ToString());
                    this.listNonDefaultProperties.Add(new NonDefaultProperty(this.DatabaseName, FriendlyPath, prop.Name, defaultAttr.Value.ToString(), sValue));
                }
            }
        }

        /// <summary>
        /// Object properties which are not decorated with DefaultValueAttribute must be manually scanned using this signature. 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="PropertyName"></param>
        private void ScanAnalysisServicesPropertyForNonDefaultValue(object o, string PropertyName)
        {
            if (o == null) return;

            string FriendlyPath;
            if (o is IModelComponent)
                FriendlyPath = ((IModelComponent)o).FriendlyPath;
            else
                throw new Exception("Object was type " + o.GetType().FullName + " which is not an IModelComponent object.");

            PropertyInfo prop = o.GetType().GetProperty(PropertyName);

            if (prop == null) throw new Exception("Couldn't find property " + PropertyName + " in object type " + o.GetType().FullName);

            object value = prop.GetValue(o, null);
            string sValue = (value == null ? string.Empty : value.ToString());
            if (sValue != string.Empty)
            {
                this.listNonDefaultProperties.Add(new NonDefaultProperty(this.DatabaseName, FriendlyPath, prop.Name, string.Empty, sValue));
            }
        }
        #endregion

        #region ExcludedProperties
        private string ExcludedPropertiesRegistryKey
        {
            get
            {
                return (SSASProject ? "SSAS_ExcludedProperties" : "SSIS_ExcludedProperties");
            }
        }

        /// <summary>
        /// Get or save to the registry the list of unchecked properties which the user does not want to see in the report
        /// </summary>
        private string ExcludedProperties
        {
            get
            {
                RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PluginRegistryPath);
                string sExcludedProps = (string)regKey.GetValue(ExcludedPropertiesRegistryKey, string.Empty);
                regKey.Close();
                return sExcludedProps;
            }

            set
            {
                RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PluginRegistryPath);
                regKey.SetValue(ExcludedPropertiesRegistryKey, value, RegistryValueKind.String);
                regKey.Close();
            }
        }
        #endregion

        #region Object for Report Designer
        public class NonDefaultProperty
        {
            public NonDefaultProperty(string DatabaseName, string FriendlyPath, string PropertyName, string DefaultValue, string Value)
            {
                mDatabaseName = DatabaseName;
                mFriendlyPath = FriendlyPath;
                mPropertyName = PropertyName;
                mDefaultValue = DefaultValue;
                mValue = Value;
            }

            private Guid mGuid = Guid.NewGuid();
            public string GUID
            {
                get { return mGuid.ToString(); }
            }

            private string mDatabaseName;
            public string DatabaseName
            {
                get { return mDatabaseName; }
            }

            private string mFriendlyPath;
            public string FriendlyPath
            {
                get { return mFriendlyPath; }
            }

            private string mPropertyName;
            public string PropertyName
            {
                get { return mPropertyName; }
            }

            private string mDefaultValue;
            public string DefaultValue
            {
                get { return mDefaultValue; }
            }

            private string mValue;
            public string Value
            {
                get { return mValue; }
            }
        }
        #endregion

        #region Object Used for Putting Text Descriptions on pipeline component properties
        public class PipelinePropertyContext : ITypeDescriptorContext
        {
            // Fields
            private object _Instance;
            private PropertyDescriptor _PropertyDescriptor;

            // Methods
            public PipelinePropertyContext(object Instance, PropertyDescriptor PropertyDescriptor)
            {
                this._Instance = Instance;
                this._PropertyDescriptor = PropertyDescriptor;
            }

            void ITypeDescriptorContext.OnComponentChanged()
            {
            }

            bool ITypeDescriptorContext.OnComponentChanging()
            {
                return true;
            }

            object IServiceProvider.GetService(Type serviceType)
            {
                return null;
            }

            IContainer ITypeDescriptorContext.Container
            {
                get
                {
                    return null;
                }
            }

            object ITypeDescriptorContext.Instance
            {
                get
                {
                    return this._Instance;
                }
            }

            PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
            {
                get
                {
                    return this._PropertyDescriptor;
                }
            }
        }
        #endregion

    }
}
