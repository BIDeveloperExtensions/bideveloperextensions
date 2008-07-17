using EnvDTE;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Controls;
using System;
using MSDDS;
using Microsoft.SqlServer.Dts.Runtime;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace BIDSHelper
{
    public abstract class HighlightingToDo
    {
        protected class CachedHighlightStatus
        {
            public CachedHighlightStatus(Package package, bool bHasExpression, bool bHasConfiguration)
            {
                this.package = package;
                this.bHasExpression = bHasExpression;
                this.bHasConfiguration = bHasConfiguration;
            }

            public CachedHighlightStatus(Package package, TaskHost taskHost, bool bHasExpression, bool bHasConfiguration)
            {
                this.package = package;
                this.taskHost = taskHost;
                this.bHasExpression = bHasExpression;
                this.bHasConfiguration = bHasConfiguration;
            }

            public Package package;
            public bool bHasExpression;
            public bool bHasConfiguration;

            /// <summary>
            /// The dataflow task for this transform
            /// </summary>
            public TaskHost taskHost = null;
        }

        protected static Type TYPE_MANAGED_BASE_SHAPE = ExpressionHighlighterPlugin.GetPrivateType(typeof(Microsoft.DataTransformationServices.Design.ColumnInfo), "Microsoft.DataTransformationServices.Design.ManagedShapeBase");
        protected static System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        protected static Dictionary<ConnectionManager, CachedHighlightStatus> cacheConnectionManagers = new Dictionary<ConnectionManager, CachedHighlightStatus>();
        protected static Dictionary<Executable, CachedHighlightStatus> cacheTasks = new Dictionary<Executable, CachedHighlightStatus>();
        public static Dictionary<Package, List<string>> cacheConfigPaths = new Dictionary<Package, List<string>>();
        protected static Dictionary<string, CachedHighlightStatus> cacheTransforms = new Dictionary<string, CachedHighlightStatus>();
        public static Dictionary<Package, List<string>> cacheConfigurationWarnings = new Dictionary<Package, List<string>>();

        public static void CachePackageConfigurations(Package package, ProjectItem pi)
        {
            System.Diagnostics.Debug.WriteLine("Caching package configurations");

            List<string> warnings = new List<string>();

            bool bOfflineMode = false;
            try
            {
                Microsoft.DataWarehouse.Interfaces.IConfigurationSettings settings = (Microsoft.DataWarehouse.Interfaces.IConfigurationSettings)((System.IServiceProvider)pi.ContainingProject).GetService(typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings));
                bOfflineMode = (bool)settings.GetSetting("OfflineMode");
            }
            catch { }


            string sVisualStudioRelativePath = pi.DTE.FullName.Substring(0, pi.DTE.FullName.LastIndexOf('\\') + 1);
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
            lock (cacheConfigPaths)
            {
                if (cacheConfigPaths.ContainsKey(package))
                    cacheConfigPaths[package] = listConfigPaths;
                else
                    cacheConfigPaths.Add(package, listConfigPaths);
            }
            lock (cacheConfigurationWarnings)
            {
                if (cacheConfigurationWarnings.ContainsKey(package))
                    cacheConfigurationWarnings[package] = warnings;
                else
                    cacheConfigurationWarnings.Add(package, warnings);
            }
        }

        #region Cache Cleanup
        public static void ClearCache(ConnectionManager conn)
        {
            lock (cacheConnectionManagers)
            {
                if (cacheConnectionManagers.ContainsKey(conn))
                    cacheConnectionManagers.Remove(conn);
            }
        }

        public static void ClearCache(Executable exe)
        {
            lock (cacheTasks)
            {
                if (cacheTasks.ContainsKey(exe))
                    cacheTasks.Remove(exe);
            }

            List<string> transformsToRemove = new List<string>();
            foreach (string transform in cacheTransforms.Keys)
            {
                if (cacheTransforms[transform].taskHost == exe)
                    transformsToRemove.Add(transform);
            }
            lock (cacheTransforms)
            {
                foreach (string transform in transformsToRemove)
                    cacheTransforms.Remove(transform);
            }
        }

        public static void ClearCache(string transform)
        {
            lock (cacheTransforms)
            {
                if (cacheTransforms.ContainsKey(transform))
                    cacheTransforms.Remove(transform);
            }
        }

        public static void ClearCache(Package pkg)
        {
            List<ConnectionManager> connectionsToRemove = new List<ConnectionManager>();
            foreach (ConnectionManager cm in cacheConnectionManagers.Keys)
            {
                if (cacheConnectionManagers[cm].package == pkg)
                    connectionsToRemove.Add(cm);
            }
            lock (cacheConnectionManagers)
            {
                foreach (ConnectionManager cm in connectionsToRemove)
                    cacheConnectionManagers.Remove(cm);
            }

            List<Executable> tasksToRemove = new List<Executable>();
            foreach (Executable exe in cacheTasks.Keys)
            {
                if (cacheTasks[exe].package == pkg)
                    tasksToRemove.Add(exe);
            }
            lock (cacheTasks)
            {
                foreach (Executable exe in tasksToRemove)
                    cacheTasks.Remove(exe);
            }

            List<string> transformsToRemove = new List<string>();
            foreach (string transform in cacheTransforms.Keys)
            {
                if (cacheTransforms[transform].package == pkg)
                    transformsToRemove.Add(transform);
            }
            lock (cacheTransforms)
            {
                foreach (string transform in transformsToRemove)
                    cacheTransforms.Remove(transform);
            }

            lock (cacheConfigPaths)
            {
                if (cacheConfigPaths.ContainsKey(pkg))
                    cacheConfigPaths.Remove(pkg);
            }
        }

        public static void ClearCache()
        {
            lock (cacheConnectionManagers)
                cacheConnectionManagers.Clear();

            lock (cacheTasks)
                cacheTasks.Clear();

            lock (cacheTransforms)
                cacheTransforms.Clear();

            lock (cacheConfigPaths)
                cacheConfigPaths.Clear();
        }

        public static void TrimCache()
        {
            if (cacheConnectionManagers.Count > 1000)
            {
                lock (cacheConnectionManagers)
                {
                    while (cacheConnectionManagers.Count > 800)
                    {
                        ConnectionManager cmToDelete = null;
                        foreach (ConnectionManager cm in cacheConnectionManagers.Keys)
                        {
                            cmToDelete = cm;
                            break;
                        }
                        cacheConnectionManagers.Remove(cmToDelete);
                    }
                }
                System.Diagnostics.Debug.WriteLine("trimmed cacheConnectionManagers");
            }

            if (cacheTasks.Count > 1000)
            {
                lock (cacheTasks)
                {
                    while (cacheTasks.Count > 800)
                    {
                        Executable taskToDelete = null;
                        foreach (Executable task in cacheTasks.Keys)
                        {
                            taskToDelete = task;
                            break;
                        }
                        cacheTasks.Remove(taskToDelete);
                    }
                }
                System.Diagnostics.Debug.WriteLine("trimmed cacheTasks");
            }

            if (cacheTransforms.Count > 1000)
            {
                lock (cacheTransforms)
                {
                    while (cacheTransforms.Count > 800)
                    {
                        string transformToDelete = null;
                        foreach (string transform in cacheTransforms.Keys)
                        {
                            transformToDelete = transform;
                            break;
                        }
                        cacheTransforms.Remove(transformToDelete);
                    }
                }
                System.Diagnostics.Debug.WriteLine("trimmed cacheTransforms");
            }

            //don't trim cacheConfigPaths as it's important it be there
        }
        #endregion

        public Package package = null;
        public bool BackgroundOnly = true;
        public bool Rescan = false;
        public abstract void Highlight();


        public static System.Drawing.Color expressionColor = System.Drawing.Color.Magenta;
        public static System.Drawing.Color configurationColor = System.Drawing.Color.FromArgb(17, 200, 255);
        
        public static void ModifyIcon(System.Drawing.Bitmap icon, System.Drawing.Color color)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 8 - i; j > -1; j--)
                {
                    icon.SetPixel(i, j, color);
                }
            }
        }

        public static void ModifyIcon(System.Drawing.Bitmap icon, System.Drawing.Color color1, System.Drawing.Color color2)
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

        protected void HighlightDdsDiagramObjectIcon(DdsDiagramHostControl diagram, object managedShape, bool bHasExpression, bool bHasConfiguration)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("called HighlightDdsDiagramObjectIcon " + (diagram == null ? "Null, " : "NotNull, ") + (managedShape == null ? "Null, " : "NotNull, ") + bHasExpression + ", " + bHasConfiguration);
                if (managedShape != null)
                {
                    System.Drawing.Bitmap icon = (System.Drawing.Bitmap)TYPE_MANAGED_BASE_SHAPE.InvokeMember("Icon", getflags | System.Reflection.BindingFlags.Public, null, managedShape, null);
                    if (!bHasExpression && !bHasConfiguration && icon.Tag != null)
                    {
                        //reset the icon because this one doesn't have an expression anymore
                        System.Reflection.BindingFlags setflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                        TYPE_MANAGED_BASE_SHAPE.InvokeMember("Icon", setflags, null, managedShape, new object[] { icon.Tag });
                        icon.Tag = null;
                        diagram.Invalidate(true); //TODO: just invalidate object?

                        System.Diagnostics.Debug.WriteLine("un-highlighted object");
                    }
                    else if ((bHasExpression || bHasConfiguration))
                    {
                        //save what the icon looked like originally so we can go back if they remove the expression
                        if (icon.Tag == null)
                            icon.Tag = icon.Clone();

                        //now update the icon to note this one has an expression
                        if (bHasExpression && !bHasConfiguration)
                            ModifyIcon(icon, expressionColor);
                        else if (bHasConfiguration && !bHasExpression)
                            ModifyIcon(icon, configurationColor);
                        else
                            ModifyIcon(icon, expressionColor, configurationColor);
                        diagram.Invalidate(true);
                        System.Diagnostics.Debug.WriteLine("highlighted object");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error highlighting: " + ex.Message + " " + ex.StackTrace);
            }
        }
    }


    /// <summary>
    /// A highlighting to-do for an object on a control flow
    /// </summary>
    public class TaskHighlightingToDo : HighlightingToDo
    {
        public Executable executable = null;
        public IDesignerHost controlFlowDesigner = null;
        public DdsDiagramHostControl controlFlowDiagram = null;

        private object taskManagedShape = null;
        public MSDDS.IDdsDiagramObject controlFlowDiagramTask
        {
            set
            {
                taskManagedShape = TYPE_MANAGED_BASE_SHAPE.InvokeMember("GetManagedShape", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static, null, null, new object[] { value });
            }
        }
        public List<string> transforms = null;
        public override void Highlight()
        {
            List<string> listConfigPaths;
            lock (cacheConfigPaths)
            {
                if (cacheConfigPaths.ContainsKey(package))
                    listConfigPaths = cacheConfigPaths[package];
                else
                    listConfigPaths = new List<string>();
            }

            CachedHighlightStatus status = null;
            if (!cacheTasks.TryGetValue(executable, out status) || Rescan)
            {
                if (AllTransformsAreCached(transforms)) //all transforms should be cached if we're focused on the data flow window such that transforms are foreground tasks and the control flow task is a background task
                {
                    bool bTaskHasExpression = false;
                    bool bTaskHasConfiguration = false;

                    lock (transforms)
                    {
                        foreach (string str in transforms)
                        {
                            CachedHighlightStatus transformStatus = cacheTransforms[str];
                            bTaskHasExpression = bTaskHasExpression || transformStatus.bHasExpression;
                            bTaskHasConfiguration = bTaskHasConfiguration || transformStatus.bHasConfiguration;
                        }
                    }

                    status = new CachedHighlightStatus(package, bTaskHasExpression, bTaskHasConfiguration);
                    System.Diagnostics.Debug.WriteLine("highlighting data flow task from transforms cache");
                }
                else
                {
                    bool bHasConfiguration = false;
                    bool bHasExpression = HasExpression(executable, listConfigPaths, out bHasConfiguration);
                    status = new CachedHighlightStatus(package, bHasExpression, bHasConfiguration);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("highlighting task from cache");
            }

            HighlightDdsDiagramObjectIcon(controlFlowDiagram, taskManagedShape, status.bHasExpression, status.bHasConfiguration);

            if (cacheTasks.ContainsKey(executable))
                cacheTasks[executable] = status;
            else
                cacheTasks.Add(executable, status);
        }

        private bool AllTransformsAreCached(List<string> transforms)
        {
            if (transforms == null)
                return false;

            lock (transforms)
            {
                foreach (string str in transforms)
                {
                    if (!cacheTransforms.ContainsKey(str)) return false;
                }
            }
            return true;
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

            if (executable is ForEachLoop)
            {
                ForEachEnumeratorHost forEachEnumerator = ((ForEachLoop)executable).ForEachEnumerator;

                if (!returnValue)
                {
                    foreach (DtsProperty p in forEachEnumerator.Properties)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(forEachEnumerator.GetExpression(p.Name)))
                            {
                                returnValue = true;
                                break;
                            }
                        }
                        catch { }
                    }
                }

                if (!HasConfiguration)
                {
                    //check for package configurations separately so you can break out of the expensive expressions search as soon as you find one
                    foreach (DtsProperty p in forEachEnumerator.Properties)
                    {
                        string sPackagePath = p.GetPackagePath(forEachEnumerator);
                        if (listConfigPaths.Contains(sPackagePath))
                        {
                            HasConfiguration = true;
                            break;
                        }
                    }
                }
            }

            return returnValue;
        }
    }




    /// <summary>
    /// A highlighting to-do for an object on a data flow
    /// </summary>
    public class TransformHighlightingToDo : HighlightingToDo
    {
        public IDesignerHost dataFlowDesigner = null;
        public DdsDiagramHostControl dataFlowDiagram = null;
        public TaskHost taskHost = null;
        public string transformName = null;
        public string transformUniqueID = null;

        private object transformManagedShape = null;
        public MSDDS.IDdsDiagramObject dataFlowDiagramTask
        {
            set
            {
                transformManagedShape = TYPE_MANAGED_BASE_SHAPE.InvokeMember("GetManagedShape", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static, null, null, new object[] { value });
            }
        }

        public override void Highlight()
        {
            List<string> listConfigPaths;
            lock (cacheConfigPaths)
            {
                if (cacheConfigPaths.ContainsKey(package))
                    listConfigPaths = cacheConfigPaths[package];
                else
                    listConfigPaths = new List<string>();
            }

            CachedHighlightStatus status = null;
            if (!cacheTransforms.TryGetValue(transformUniqueID, out status) || Rescan)
            {
                bool bHasConfiguration = false;
                bool bHasExpression = HasExpression(taskHost, transformName, listConfigPaths, out bHasConfiguration);
                status = new CachedHighlightStatus(package, taskHost, bHasExpression, bHasConfiguration);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("highlighting from cache for: " + transformUniqueID);
            }

            HighlightDdsDiagramObjectIcon(dataFlowDiagram, transformManagedShape, status.bHasExpression, status.bHasConfiguration);
            
            if (cacheTransforms.ContainsKey(transformUniqueID))
                cacheTransforms[transformUniqueID] = status;
            else
                cacheTransforms.Add(transformUniqueID, status);
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
    }

    
    
    
    /// <summary>
    /// A highlighting to-do for a connection manager
    /// </summary>
    public class ConnectionManagerHighlightingToDo : HighlightingToDo
    {
        public ConnectionManager connection = null;
        public List<ListViewItem> listConnectionLVIs = new List<ListViewItem>();

        public override void Highlight()
        {
            List<string> listConfigPaths;
            lock (cacheConfigPaths)
            {
                if (cacheConfigPaths.ContainsKey(package))
                    listConfigPaths = cacheConfigPaths[package];
                else
                    listConfigPaths = new List<string>();
            }

            bool bHasConfiguration = false;
            bool bHasExpression = false;
            if (Rescan || !cacheConnectionManagers.ContainsKey(connection))
            {
                System.Diagnostics.Debug.WriteLine("scanning connection manager for expressions & configurations");
                bHasExpression = HasExpression(connection, listConfigPaths, out bHasConfiguration);
                if (cacheConnectionManagers.ContainsKey(connection))
                    cacheConnectionManagers[connection] = new CachedHighlightStatus(this.package, bHasExpression, bHasConfiguration);
                else
                {
                    lock (cacheConnectionManagers)
                        cacheConnectionManagers.Add(connection, new CachedHighlightStatus(this.package, bHasExpression, bHasConfiguration));
                }
                lock (listConnectionLVIs)
                {
                    foreach (ListViewItem lvi in listConnectionLVIs)
                    {
                        HighlightConnectionManagerLVI(lvi, bHasExpression, bHasConfiguration, true); //ensure the connection manager is invalidated... this only helps the connection manager repaint in a few situations, but it's necessary
                    }
                }
            }
            else
            {
                CachedHighlightStatus ccm = cacheConnectionManagers[connection];
                bHasExpression = ccm.bHasExpression;
                bHasConfiguration = ccm.bHasConfiguration;
                System.Diagnostics.Debug.WriteLine("scanning connection manager for expressions & configurations from cache");
            }

            //the lvwConnMgrs_DrawItem event will take care of painting the connection managers 90% of the time
            //unfortunately we had to use the lvwConnMgrs_DrawItem to catch all the appropriate times we needed to refix the icon
        }

        public static void HighlightConnectionManagerLVI(ListViewItem lviConn)
        {
            ConnectionManager connection = (ConnectionManager)lviConn.GetType().InvokeMember("Component", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, lviConn, null);

            Package package = null;
            List<string> listConfigPaths = new List<string>();
            lock (cacheConfigPaths)
            {
                foreach (Package p in cacheConfigPaths.Keys)
                {
                    if (p.Connections.Contains(connection.ID) && p.Connections[connection.ID] == connection)
                    {
                        package = p;
                        listConfigPaths = cacheConfigPaths[package];
                        break;
                    }
                }
                if (package == null)
                {
                    System.Diagnostics.Debug.WriteLine("HighlightConnectionManager: can't find package for connection!!!");
                    return;
                }
            }

            bool bHasConfiguration = false;
            bool bHasExpression = false;
            if (!cacheConnectionManagers.ContainsKey(connection))
            {
                System.Diagnostics.Debug.WriteLine("HighlightConnectionManager: not cached!!!");
                bHasExpression = HasExpression(connection, listConfigPaths, out bHasConfiguration);
                if (cacheConnectionManagers.ContainsKey(connection))
                    cacheConnectionManagers[connection] = new CachedHighlightStatus(package, bHasExpression, bHasConfiguration);
                else
                {
                    lock (cacheConnectionManagers)
                        cacheConnectionManagers.Add(connection, new CachedHighlightStatus(package, bHasExpression, bHasConfiguration));
                }
            }
            else
            {
                CachedHighlightStatus ccm = cacheConnectionManagers[connection];
                bHasExpression = ccm.bHasExpression;
                bHasConfiguration = ccm.bHasConfiguration;
            }
            HighlightConnectionManagerLVI(lviConn, bHasExpression, bHasConfiguration, false);
        }

        private static void HighlightConnectionManagerLVI(ListViewItem lviConn, bool bHasExpression, bool bHasConfiguration, bool bInvalidate)
        {
            lock (lviConn)
            {
                System.Drawing.Bitmap icon = null;
                if (lviConn.ImageList.Images.Count > lviConn.ImageIndex && lviConn.ListView != null)
                    icon = lviConn.ImageList.Images[lviConn.ImageIndex] as System.Drawing.Bitmap;
                else
                    System.Diagnostics.Debug.WriteLine("couldn't find current connection manager icon");
                if (icon == null)
                    return;


                System.Diagnostics.Debug.WriteLine("connection has " + (bHasExpression || bHasConfiguration ? " an " : " no ") + " expression/configuration");
                System.Diagnostics.Debug.WriteLine("lviConn.Tag: " + (lviConn.Tag == null ? "null" : lviConn.Tag.ToString()));

                if (!bHasExpression && !bHasConfiguration && lviConn.Tag != null)
                {
                    lviConn.ImageIndex = (int)lviConn.Tag;
                    lviConn.Tag = null;
                    if (bInvalidate) lviConn.ListView.Invalidate(lviConn.Bounds, true);
                }
                else if ((bHasExpression || bHasConfiguration))
                {
                    System.Drawing.Image oldimg = lviConn.ImageList.Images[lviConn.ImageIndex];
                    System.Drawing.Bitmap newicon = new System.Drawing.Bitmap(oldimg);
                    if (lviConn.Tag == null)
                        lviConn.Tag = lviConn.ImageIndex; //save the old index
                    System.Diagnostics.Debug.WriteLine("new lviConn.Tag: " + (lviConn.Tag == null ? "null" : lviConn.Tag.ToString()));
                    if (bHasExpression && !bHasConfiguration)
                        ModifyIcon(newicon, expressionColor);
                    else if (bHasConfiguration && !bHasExpression)
                        ModifyIcon(newicon, configurationColor);
                    else
                        ModifyIcon(newicon, expressionColor, configurationColor);
                    lviConn.ImageList.Images.Add(newicon);
                    lviConn.ImageIndex = lviConn.ImageList.Images.Count - 1;
                    lviConn.ImageList.Images[lviConn.ImageIndex].Tag = newicon.Tag;
                    System.Diagnostics.Debug.WriteLine("after assignment lviConn.Tag: " + (lviConn.Tag == null ? "null" : lviConn.Tag.ToString()));
                    if (bInvalidate) lviConn.ListView.Invalidate(lviConn.Bounds, true);
                }
            }
        }

        private static bool HasExpression(ConnectionManager connectionManager, List<string> listConfigPaths, out bool HasConfiguration)
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
    }
}
