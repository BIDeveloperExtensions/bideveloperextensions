namespace BIDSHelper.SSIS
{
    extern alias sharedDataWarehouseInterfaces;
    extern alias asDataWarehouseInterfaces;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Windows.Forms;
    using EnvDTE;
    using Microsoft.DataWarehouse.Controls;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.SqlServer.IntegrationServices.Designer.ConnectionManagers;

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

            bool bOfflineMode = pi.ContainingProject.GetOfflineMode();

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


        public static System.Drawing.Color expressionColor = ExpressionHighlighterPlugin.ExpressionColor;
        public static System.Drawing.Color configurationColor = ExpressionHighlighterPlugin.ConfigurationColor;
        
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


        private static Dictionary<string, System.Windows.Media.Imaging.BitmapSource> _cacheBitmapSource = new Dictionary<string, System.Windows.Media.Imaging.BitmapSource>();
        public static System.Windows.Media.Imaging.BitmapSource GetBitmapSource(System.Drawing.Color color)
        {
            return GetBitmapSource(color, color);
        }
        public static System.Windows.Media.Imaging.BitmapSource GetBitmapSource(System.Drawing.Color color1, System.Drawing.Color color2)
        {
            string sColorKey = color1.ToArgb().ToString() + "_" + color2.ToArgb().ToString();
            if (_cacheBitmapSource.ContainsKey(sColorKey))
            {
                return _cacheBitmapSource[sColorKey];
            }

            int width = 32;
            int height = 32;
            int stride=width*4 + (width %4);
            byte[] bits=new byte[height*stride];

            for (int i = 0; i < width; i++)
            {
                for (int j = height - 1 - i; j > -1; j--)
                {
                    if (i <= j)
                        setpixel(ref bits, i, j, stride, color1);
                    else
                        setpixel(ref bits, i, j, stride, color2);
                }
            }

            System.Windows.Media.Imaging.BitmapSource bs = System.Windows.Media.Imaging.BitmapSource.Create(  width, height,  300,  300,  System.Windows.Media.PixelFormats.Pbgra32,  null,  bits,  stride);
            _cacheBitmapSource.Add(sColorKey, bs);
            return bs;
        }

        private static void setpixel(ref byte[] bits, int x, int y, int stride, System.Drawing.Color c)
        {
            bits[x * 4 + y * stride] = c.B;
            bits[x * 4 + y * stride + 1] = c.G;
            bits[x * 4 + y * stride + 2] = c.R;
            bits[x * 4 + y * stride + 3] = c.A;
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
        public Microsoft.SqlServer.Graph.Model.ModelElement controlFlowTaskModelElement;

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

            this.controlFlowTaskModelElement.Dispatcher.BeginInvoke //enables this code to run on the application thread or something like that and avoids errors
                (System.Windows.Threading.DispatcherPriority.Normal,
                (Action)(() =>
                {
                    System.Windows.FrameworkElement fe = (System.Windows.FrameworkElement)this.controlFlowTaskModelElement.View;
                    if (fe != null) //the sequence container for the Package object itself has a null view. TODO is how to add a configuration highlight to the background of the package itself... this seems to do the trick but there's more formatting work to do: fe = (System.Windows.FrameworkElement)(((Microsoft.SqlServer.IntegrationServices.Designer.Model.SequenceModelElement)(controlFlowTaskModelElement)).GraphModelElement).GraphControl
                    {
                        System.Diagnostics.Debug.WriteLine("ExpressionHighlighter - adding task adorner " + controlFlowTaskModelElement.Name);
                        Adorners.BIDSHelperConfigurationAdorner adorner = new Adorners.BIDSHelperConfigurationAdorner(fe, typeof(Microsoft.SqlServer.Graph.Model.ModelElement));
                        adorner.UpdateAdorner(false, status.bHasConfiguration); //only highlight configurations
                    }
                }
                ));


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

                // Check the for each loop has been configured, else it won't have an enumerator yet
                if (forEachEnumerator == null)
                    return returnValue;


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
        public TaskHost taskHost = null;
        public string transformName = null;
        public string transformUniqueID = null;

        public Microsoft.SqlServer.Graph.Model.ModelElement dataFlowTransformModelElement;


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


            this.dataFlowTransformModelElement.Dispatcher.BeginInvoke //enables this code to run on the application thread or something like that and avoids errors
                (System.Windows.Threading.DispatcherPriority.Normal,
                (Action)(() =>
                {
                    System.Diagnostics.Debug.WriteLine("ExpressionHighlighter - adding transform adorner " + transformName);
                    System.Windows.FrameworkElement fe = (System.Windows.FrameworkElement)this.dataFlowTransformModelElement.View;
                    if (fe != null) //the sequence container for the Package object itself has a null view. TODO is how to add a configuration highlight to the background of the package itself
                    {
                        Adorners.BIDSHelperConfigurationAdorner adorner = new Adorners.BIDSHelperConfigurationAdorner(fe, typeof(Microsoft.SqlServer.Graph.Model.ModelElement));
                        adorner.UpdateAdorner(status.bHasExpression, status.bHasConfiguration);
                    }
                }
                ));


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
        public List<System.Windows.FrameworkElement> listConnectionLVIs = new List<System.Windows.FrameworkElement>();

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
            if (Rescan || !cacheConnectionManagers.ContainsKey(connection)) //note this is not allowing highlighting of connections on event handlers tab since the event handlers tab is not loaded when you first load the package... TODO... consider changing the cacheConnectionManagers object to track which tabs have been highlighted before
            {
                System.Diagnostics.Debug.WriteLine("scanning connection manager for expressions & configurations");
                bHasExpression = HasExpression(connection, listConfigPaths, out bHasConfiguration); //also figures out whether it has configurations
                if (cacheConnectionManagers.ContainsKey(connection))
                    cacheConnectionManagers[connection] = new CachedHighlightStatus(this.package, bHasExpression, bHasConfiguration);
                else
                {
                    lock (cacheConnectionManagers)
                        cacheConnectionManagers.Add(connection, new CachedHighlightStatus(this.package, bHasExpression, bHasConfiguration));
                }
            }
            else
            {
                CachedHighlightStatus ccm = cacheConnectionManagers[connection];
                bHasExpression = ccm.bHasExpression;
                bHasConfiguration = ccm.bHasConfiguration;
                System.Diagnostics.Debug.WriteLine("scanning connection manager for expressions & configurations from cache");
            }

            lock (listConnectionLVIs)
            {
                foreach (System.Windows.FrameworkElement fe in listConnectionLVIs)
                {
                    if (fe != null)
                    {
                        fe.Dispatcher.BeginInvoke //enables this code to run on the application thread or something like that and avoids errors
                            (System.Windows.Threading.DispatcherPriority.Normal,
                            (Action)(() =>
                            {
                                System.Diagnostics.Debug.WriteLine("ExpressionHighlighter - adding connection adorner " + connection.Name);
                                Adorners.BIDSHelperConfigurationAdorner adorner = new Adorners.BIDSHelperConfigurationAdorner(fe, typeof(ConnectionManagerModelElement));
                                adorner.UpdateAdorner(false, bHasConfiguration); //only highlight configurations
                            }
                            ));
                    }
                }

            }

            //the lvwConnMgrs_DrawItem event will take care of painting the connection managers 90% of the time
            //unfortunately we had to use the lvwConnMgrs_DrawItem to catch all the appropriate times we needed to refix the icon
        }

        private static bool HasExpression(ConnectionManager connectionManager, List<string> listConfigPaths, out bool HasConfiguration)
        {
            IDTSPropertiesProvider dtsObject = (IDTSPropertiesProvider)connectionManager;
            bool returnValue = false;
            HasConfiguration = false;

#if !DENALI && !SQL2014 //connection managers already have built-in expression highlighting in Denali, so don't run this code in Denali
#endif

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



    /// <summary>
    /// A to-do for rerunning BuildToDos after the paste operation has finished
    /// </summary>
    public class RequeueToDo : HighlightingToDo
    {
        public RequeueToDo()
        {
            _mostRecentReQueue = DateTime.Now;
        }
        public Window GotFocus;
        public DtsObject oIncrementalObject;
        public int? iIncrementalTransformID;
        public Microsoft.DataWarehouse.Design.EditorWindow editorWin;
        public ExpressionHighlighterPlugin plugin;
        private static DateTime _mostRecentReQueue = DateTime.MinValue;

        public override void Highlight()
        {
            if (editorWin.InvokeRequired)
            {
                if (DateTime.Now.AddSeconds(-2) < _mostRecentReQueue)
                {
                    System.Diagnostics.Debug.WriteLine("RequeueToDo.Highlight sleeping");
                    System.Threading.Thread.Sleep(1000); //give it some time for the paste to complete
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("RequeueToDo.Highlight skipped sleeping");
                }

                System.Diagnostics.Debug.WriteLine("BeginInvoke on background thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
                IAsyncResult r = editorWin.BeginInvoke(new MethodInvoker(delegate() { Highlight(); })); //use Invoke, not BeginInvoke which does it asynchronously
                r.AsyncWaitHandle.WaitOne();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("calling BuildToDos on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
                plugin.BuildToDos(GotFocus, oIncrementalObject, iIncrementalTransformID);
            }
        }
    }
}
