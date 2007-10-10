using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using System;
using Microsoft.Win32;
using MSDDS;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace BIDSHelper
{
    public class ExpressionHighlighterPlugin : BIDSHelperWindowActivatedPluginBase
    {
        //private WindowEvents windowEvents;
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.List<string> windowHandlesFixedForExpressionHighlighter = new System.Collections.Generic.List<string>();
        private System.Collections.Generic.List<string> windowHandlesInProgressStatus = new System.Collections.Generic.List<string>();
        private static System.Drawing.Color expressionColor = System.Drawing.Color.Magenta;
        private static System.Drawing.Color configurationColor = System.Drawing.Color.FromArgb(17, 200, 255);

        EditorWindow win = null;
        //System.ComponentModel.BackgroundWorker processPackage = null;

        public ExpressionHighlighterPlugin(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
        }

        public override bool ShouldHookWindowCreated
        {
            get { return false; }
        }


        //TODO: need to find a way to pick up changes to the package more quickly than just the WindowActivated event
        //The DtsPackageView object seems to have the appropriate methods, but it's internal to the Microsoft.DataTransformationServices.Design assembly.
        //void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        public override void OnWindowActivated(Window GotFocus, Window lostFocus)
        {
            IDTSSequence container = null;
            MainPipe pipe = null;
            TaskHost taskHost = null;
            string sHandle = String.Empty;
            List<string> warnings = new List<string>();

            try
            {
                if (GotFocus == null) return;
                if (GotFocus.DTE.Mode == vsIDEMode.vsIDEModeDebug) return;
                IDesignerHost designer = GotFocus.Object as IDesignerHost;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if (!(pi.Name.ToLower().EndsWith(".dtsx"))) return;
                //EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                Control viewControl = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
                DdsDiagramHostControl diagram = null;
                ListView lvwConnMgrs = null;

                IntPtr ptr = win.Handle;
                sHandle = ptr.ToInt64().ToString();

                if (!windowHandlesFixedForExpressionHighlighter.Contains(sHandle))
                {
                    windowHandlesFixedForExpressionHighlighter.Add(sHandle);
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);

                    IComponentChangeService configurationsChangeService = (IComponentChangeService)designer;
                    configurationsChangeService.ComponentChanging += new ComponentChangingEventHandler(configurationsChangeService_ComponentChanging);
                }

                if (windowHandlesInProgressStatus.Contains(sHandle))
                {
                    return;
                }
                windowHandlesInProgressStatus.Add(sHandle);

                if (win.SelectedIndex == 0) //Control Flow
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["ddsDiagramHostControl1"];
                    lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                    container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                }
                else if (win.SelectedIndex == 1) //Data Flow
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel2"].Controls["pipelineDetailsControl"].Controls["PipelineTaskView"];
                    taskHost = (TaskHost)diagram.ComponentDiagram.RootComponent;
                    pipe = (MainPipe)taskHost.InnerObject;
                    container = (IDTSSequence)taskHost.Parent;
                    lvwConnMgrs = (ListView)viewControl.Controls["dataFlowsTrayTabControl"].Controls["dataFlowConnectionsTabPage"].Controls["dataFlowConnectionsListView"];
                }
                else if (win.SelectedIndex == 2) //Event Handlers
                {
                    diagram = (DdsDiagramHostControl)viewControl.Controls["panel1"].Controls["panelDiagramHost"].Controls["EventHandlerView"];
                    lvwConnMgrs = (ListView)viewControl.Controls["controlFlowTrayTabControl"].Controls["controlFlowConnectionsTabPage"].Controls["controlFlowConnectionsListView"];
                    container = (IDTSSequence)diagram.ComponentDiagram.RootComponent;
                }
                else
                {
                    return;
                }

                //processPackage.RunWorkerAsync(container);



                Microsoft.DataWarehouse.Interfaces.IConfigurationSettings settings = (Microsoft.DataWarehouse.Interfaces.IConfigurationSettings)((System.IServiceProvider)pi.ContainingProject).GetService(typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings));
                bool bOfflineMode = (bool)settings.GetSetting("OfflineMode");

                string sVisualStudioRelativePath = this.ApplicationObject.FullName.Substring(0, this.ApplicationObject.FullName.LastIndexOf('\\') + 1);
                Package package = GetPackageFromContainer((DtsContainer)container);
                List<PackageConfigurationSetting> listConfigs = new List<PackageConfigurationSetting>();
                List<string> listConfigPaths = new List<string>();
                if (package.EnableConfigurations)
                {
                    foreach (Microsoft.SqlServer.Dts.Runtime.Configuration c in package.Configurations)
                    {
                        try
                        {
                            PackageConfigurationSetting[] configs = PackageConfigurationLoader.GetPackageConfigurationSettings(c, package, sVisualStudioRelativePath, bOfflineMode);
                            listConfigs.AddRange(configs);
                            foreach (PackageConfigurationSetting config in configs)
                            {
                                listConfigPaths.Add(config.Path);
                            }
                        }
                        catch (Exception ex)
                        {
                            warnings.Add("BIDS Helper was unable to load package configuration " + c.Name + ". Objects controlled by this configuration will not be highlighted. Error: " + ex.Message);
                        }
                    }
                }




                Type managedshapebasetype = GetPrivateType(typeof(Microsoft.DataTransformationServices.Design.ColumnInfo), "Microsoft.DataTransformationServices.Design.ManagedShapeBase");
                if (managedshapebasetype == null) return;

                foreach (MSDDS.IDdsDiagramObject o in diagram.DDS.Objects)
                {
                    if (o.Type == DdsLayoutObjectType.dlotShape)
                    {
                        //TODO: any way of looking at the task metadata and determining that it hasn't changed since the last time we searched it for expressions?
                        bool bHasExpression = false;
                        bool bHasConfiguration = false;
                        MSDDS.IDdsExtendedProperty prop = o.IDdsExtendedProperties.Item("LogicalObject");
                        if (prop == null) continue;
                        string sObjectGuid = prop.Value.ToString();


                        if (pipe == null) //Not a data flow
                        {
                            try
                            {
                                Executable executable = FindExecutable(container, sObjectGuid);

                                if (executable is IDTSPropertiesProvider)
                                {
                                    bHasExpression = HasExpression(executable, listConfigPaths, out bHasConfiguration);
                                }
                            }
                            catch
                            {
                                continue;
                            }

                        }
                        else
                        {
                            IDTSComponentMetaData90 transform = pipe.ComponentMetaDataCollection.GetObjectByID(int.Parse(sObjectGuid.Substring(sObjectGuid.LastIndexOf("/") + 1)));
                            bHasExpression = HasExpression(taskHost, transform.Name, listConfigPaths, out bHasConfiguration);
                        }

                        object managedShape = managedshapebasetype.InvokeMember("GetManagedShape", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static, null, null, new object[] { o });
                        if (managedShape != null)
                        {
                            System.Drawing.Bitmap icon = (System.Drawing.Bitmap)managedshapebasetype.InvokeMember("Icon", getflags | System.Reflection.BindingFlags.Public, null, managedShape, null);
                            if (!bHasExpression && !bHasConfiguration && icon.Tag != null)
                            {
                                //reset the icon because this one doesn't have an expression anymore
                                System.Reflection.BindingFlags setflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                                managedshapebasetype.InvokeMember("Icon", setflags, null, managedShape, new object[] { icon.Tag });
                                icon.Tag = null;
                            }
                            else if ((bHasExpression || bHasConfiguration))
                            {
                                //save what the icon looked like originally so we can go back if they remove the expression
                                icon.Tag = icon.Clone();

                                //now update the icon to note this one has an expression
                                if (bHasExpression && !bHasConfiguration)
                                    ModifyIcon(icon, expressionColor);
                                else if (bHasConfiguration && !bHasExpression)
                                    ModifyIcon(icon, configurationColor);
                                else
                                    ModifyIcon(icon, expressionColor, configurationColor);
                            }
                        }
                    }
                }
                HighlighConnectionManagers(package, lvwConnMgrs, listConfigPaths);

                return;

                //TODO: does the above code run too slow? should it be put in a BackgroundWorker thread? will that cause threads to step on each other?

            }
            catch (Exception ex)
            {
                warnings.Add("BIDS Helper had trouble highlighting expressions and package configurations. Error: " + ex.Message);
            }
            finally
            {
                if (windowHandlesInProgressStatus.Contains(sHandle))
                {
                    try
                    {
                        AddWarningsToVSErrorList(GotFocus, warnings.ToArray());
                    }
                    catch { }
                    windowHandlesInProgressStatus.Remove(sHandle);
                }
            }
        }

        void configurationsChangeService_ComponentChanging(object sender, ComponentChangingEventArgs e)
        {
            try
            {
                if (e.Component is Package && e.Member == null) //capture when the package configuration editor window is closed
                {
                    IDesignerHost designer = (IDesignerHost)sender;
                    foreach (Window win in this.ApplicationObject.Windows)
                    {
                        if (win.Object == designer)
                        {
                            OnWindowActivated(win, null);
                            return;
                        }
                    }
                }
            }
            catch { }
        }

        private void HighlighConnectionManagers(Package package, ListView lvwConnMgrs, List<string> listConfigPaths)
        {
            foreach (ListViewItem lviConn in lvwConnMgrs.Items)
            {
                ConnectionManager conn = FindConnectionManager(package, lviConn.Text);

                bool bHasConfiguration = false;
                bool bHasExpression = HasExpression(conn, listConfigPaths, out bHasConfiguration);

                System.Drawing.Bitmap icon = (System.Drawing.Bitmap)lviConn.ImageList.Images[lviConn.ImageIndex];

                if (!bHasExpression && !bHasConfiguration && icon.Tag != null)
                {
                    lviConn.ImageIndex = (int)icon.Tag;
                    icon.Tag = null;
                }
                else if ((bHasExpression || bHasConfiguration))
                {
                    System.Drawing.Bitmap newicon = new System.Drawing.Bitmap(lviConn.ImageList.Images[lviConn.ImageIndex]);
                    newicon.Tag = lviConn.ImageIndex; //save the old index
                    if (bHasExpression && !bHasConfiguration)
                        ModifyIcon(newicon, expressionColor);
                    else if (bHasConfiguration && !bHasExpression)
                        ModifyIcon(newicon, configurationColor);
                    else
                        ModifyIcon(newicon, expressionColor, configurationColor);
                    lviConn.ImageList.Images.Add(newicon);
                    lviConn.ImageIndex = lviConn.ImageList.Images.Count - 1;
                }
            }
        }

        private Package GetPackageFromContainer(DtsContainer container)
        {
            while (! (container is Package))
            {
                container = container.Parent;
            }
            return (Package)container;
        }

        private static void ModifyIcon(System.Drawing.Bitmap icon, System.Drawing.Color color)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 8 - i; j > -1; j--)
                {
                    icon.SetPixel(i, j, color);
                }
            }
        }

        private static void ModifyIcon(System.Drawing.Bitmap icon, System.Drawing.Color color1, System.Drawing.Color color2)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 8 - i; j > -1; j--)
                {
                    if (i <= j)
                        icon.SetPixel(i, j, color1);
                    else
                        icon.SetPixel(i, j, color2);
                }
            }
        }

        //Determine if the task has an expression
        private bool HasExpression(Executable executable, System.Collections.Generic.List<string> listConfigPaths, out bool HasConfiguration)
        {
            IDTSPropertiesProvider task = (IDTSPropertiesProvider)executable;
            bool returnValue = false;
            HasConfiguration = false;

            foreach (DtsProperty p in task.Properties)
            {
                try
                {
                    if (!string.IsNullOrEmpty(task.GetExpression(p.Name)))
                    {
                        returnValue = true;
                        break;
                    }
                }
                catch { }
            }

            //check for package configurations separately so you can break out of the expensive expressions search as soon as you find one
            foreach (DtsProperty p in task.Properties)
            {
                string sPackagePath = p.GetPackagePath(task);
                if (listConfigPaths.Contains(sPackagePath))
                {
                    HasConfiguration = true;
                    break;
                }
            }
            return returnValue;
        }

        private bool HasExpression(ConnectionManager connectionManager, List<string> listConfigPaths, out bool HasConfiguration)
        {
            IDTSPropertiesProvider dtsObject = (IDTSPropertiesProvider)connectionManager;
            bool returnValue = false;
            HasConfiguration = false;

            foreach (DtsProperty p in dtsObject.Properties)
            {
                try
                {
                    if (!string.IsNullOrEmpty(dtsObject.GetExpression(p.Name)))
                    {
                        returnValue = true;
                        break;
                    }
                }
                catch { }
            }

            //check for package configurations separately so you can break out of the expensive expressions search as soon as you find one
            foreach (DtsProperty p in dtsObject.Properties)
            {
                string sPackagePath = p.GetPackagePath(dtsObject);
                if (listConfigPaths.Contains(sPackagePath))
                {
                    HasConfiguration = true;
                    break;
                }
            }
            return returnValue;
        }

        private bool HasExpression(TaskHost taskHost, string transformName, List<string> listConfigPaths, out bool HasConfiguration)
        {
            IDTSPropertiesProvider dtsObject = (IDTSPropertiesProvider)taskHost;
            bool returnValue = false;
            HasConfiguration = false;
            transformName = "[" + transformName + "]";

            foreach (DtsProperty p in dtsObject.Properties)
            {
                if (p.Name.StartsWith(transformName))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(dtsObject.GetExpression(p.Name)))
                        {
                            returnValue = true;
                            break;
                        }
                    }
                    catch { }
                }
            }

            //check for package configurations separately so you can break out of the expensive expressions search as soon as you find one
            foreach (DtsProperty p in dtsObject.Properties)
            {
                if (p.Name.StartsWith(transformName))
                {
                    string sPackagePath = p.GetPackagePath(dtsObject);
                    if (listConfigPaths.Contains(sPackagePath))
                    {
                        HasConfiguration = true;
                        break;
                    }
                }
            }
            return returnValue;
        }

        //recursively looks in executables to find executable with the specified GUID
        Executable FindExecutable(IDTSSequence parentExecutable, string sObjectGuid)
        {
            Executable matchingExecutable = null;

            if (parentExecutable.Executables.Contains(sObjectGuid))
            {
                matchingExecutable = parentExecutable.Executables[sObjectGuid];
            }
            else
            {
                foreach (Executable e in parentExecutable.Executables)
                {
                    if (e is IDTSSequence)
                    {
                        matchingExecutable = FindExecutable((IDTSSequence)e, sObjectGuid);
                    }
                }
            }
            return matchingExecutable;
        }

        ConnectionManager FindConnectionManager(Package package, string connectionManagerName)
        {
            ConnectionManager matchingConnectionManager = null;

            if (package.Connections.Contains(connectionManagerName))
            {
                matchingConnectionManager = package.Connections[connectionManagerName];
            }

            return matchingConnectionManager;
        }

        private void AddWarningsToVSErrorList(Window window, string[] warnings)
        {
            ErrorList errorList = this.ApplicationObject.ToolWindows.ErrorList;
            Window2 errorWin2 = (Window2)(errorList.Parent);
            if (!errorWin2.Visible)
            {
                this.ApplicationObject.ExecuteCommand("View.ErrorList", " ");
            }
            IDesignerHost designer = (IDesignerHost)window.Object;
            win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
            ITaskListService service = designer.GetService(typeof(ITaskListService)) as ITaskListService;

            //remove old task items from this document and BIDS Helper class
            foreach (ITaskItem ti in service.GetTaskItems())
            {
                ICustomTaskItem task = ti as ICustomTaskItem;
                if (task != null && task.CustomInfo == this && task.Document == window.ProjectItem.get_FileNames(0))
                {
                    service.Remove(ti);
                }
            }

            foreach (string s in warnings)
            {
                ICustomTaskItem item = (ICustomTaskItem)service.CreateTaskItem(TaskItemType.Custom, s);
                item.Category = TaskItemCategory.Misc;
                item.Appearance = TaskItemAppearance.Squiggle;
                item.Priority = TaskItemPriority.Normal;
                item.Document = window.ProjectItem.get_FileNames(0);
                item.CustomInfo = this;
                service.Add(item);
            }
        }

        public static Type GetPrivateType(Type publicTypeInSameAssembly, string FullName)
        {
            foreach (Type t in System.Reflection.Assembly.GetAssembly(publicTypeInSameAssembly).GetTypes())
            {
                if (t.FullName == FullName)
                {
                    return t;
                }
            }
            return null;
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            OnActiveViewChanged();
        }

        public override string ShortName
        {
            get { return "ExpressionHighlighterPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "Expression Highlighter"; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }

        public override string MenuName
        {
            get { return ""; } //no need to have a menu command
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            return false; //TODO: decide whether to have a menu option where you can turn on/off this feature like the ShowExtraProperties feature
        }

        public override void Exec()
        {
        }

        //public override void Dispose()
        //{
        //    base.Dispose();
        //    win.ActiveViewChanged -= win_ActiveViewChanged;
        //}
    }


    class PackageConfigurationLoader
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
                string[] settings = c.ConfigurationString.Split(new string[] { "\";" }, StringSplitOptions.None);
                string sConnectionManagerName = settings[0].Substring(1);
                string sTableName = settings[1].Substring(1);
                string sFilter = settings[2].Substring(1);

                ConnectionManager cm = p.Connections[sConnectionManagerName];
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