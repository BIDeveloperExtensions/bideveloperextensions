namespace BIDSHelper.SSIS
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
    using Microsoft.SqlServer.Dts.Runtime;
    using System.Reflection;
    internal class PackageHelper
    {
        public const string PackageCreationName = "Package";
        public const string EventHandlerCreationName = "EventHandler";
        public const string ConnectionCreationName = "Connection";
        public const string SequenceCreationName = "Sequence";
        public const string ForLoopCreationName = "ForLoop";
        public const string ForEachLoopCreationName = "ForEachLoop";

        /// <summary>
        /// Private field for the ComponentInfos property
        /// </summary>
        private static ComponentInfos componentInfos = new ComponentInfos();

        /// <summary>
        /// Private field for the ComponentInfos property
        /// </summary>
        private static ComponentInfos controlInfos = new ComponentInfos();

        /// <summary>
        /// Indicates if the ComponentInfos propertry has been initialied or not, prevents repeating the expensive component search
        /// </summary>
        private static bool componentInitialised = false;

        /// <summary>
        /// Object used for locking
        /// </summary>
        private static object componentLock = new object();

        /// <summary>
        /// All managed components in the data flow share the same wrapper, identified by this GUID.
        /// The specific type of managed component is identified by the UserComponentTypeName custom property of the component.
        /// The GUID is documented in the class Syntax section - https://technet.microsoft.com/en-gb/library/microsoft.sqlserver.dts.pipeline.wrapper.cmanagedcomponentwrapperclass(v=sql.105).aspx
        /// With newer versions, disassemble Microsoft.SqlServer.DTSPipelineWrap to find the GUID attribute on Microsoft.SqlServer.Dts.Pipeline.Wrapper.CManagedComponentWrapperClass
        /// </summary>
#if SQL2019
        public const string ManagedComponentWrapper = "{7CDF593F-DE06-4ABD-B356-7976EF7AC8E0}";
#elif SQL2017
        public const string ManagedComponentWrapper = "{8DC69D45-2AD5-40C6-AAEC-25722F92D6FC}";
#elif SQL2016
        public const string ManagedComponentWrapper = "{4F885D04-B578-47B7-94A0-DE9C7DA25EE2}";
#elif SQL2014
        public const string ManagedComponentWrapper = "{33D831DE-5DCF-48F0-B431-4D327B9E785D}";
#elif DENALI
        public const string ManagedComponentWrapper = "{2E42D45B-F83C-400F-8D77-61DDE6A7DF29}";
#endif

        public static List<TaskHost> GetControlFlowObjects<T>(DtsContainer container)
        {
            List<TaskHost> returnItems = new List<TaskHost>();
            if (container is EventsProvider)
            {
                EventsProvider ep = (EventsProvider)container;

                foreach (DtsEventHandler eh in ep.EventHandlers)
                {
                    returnItems.AddRange(GetControlFlowObjects<T>(eh));
                }
            }

            IDTSSequence sequence = (IDTSSequence)container;

            foreach (Executable exec in sequence.Executables)
            {
                if (exec is IDTSSequence)
                {
                    returnItems.AddRange(GetControlFlowObjects<T>((DtsContainer)exec));
                }
                else if (exec is TaskHost)
                {
                    TaskHost th = (TaskHost)exec;
                    if (th.InnerObject is T)
                    {
                        returnItems.Add(th);
                    }
                }
            }

            return returnItems;
        }

        private static Application application;

        internal static Application Application
        {
            get
            {
                if (application == null)
                {
                    application = new Application();
#if DENALI || SQL2014
                    // Do nothing
#else
                    // SQL2016 or above, set the version
                    if (targetServerVersion == 0) throw new Exception("TargetServerVersion not set. This function cannot proceed.");
                    application.TargetServerVersion = (DTSTargetServerVersion)targetServerVersion;
#endif
                }

                return application;
            }
        }

        /// <summary>
        /// Private field for the TargetServerVersion property
        /// </summary>
        private static SsisTargetServerVersion targetServerVersion;


        public static SsisTargetServerVersion TargetServerVersion
        {
            get { return targetServerVersion; }
            set
            {
                if (targetServerVersion == value)
                {
                    return;
                }

                componentInfos.Clear();
                controlInfos.Clear();
                application = null;
                targetServerVersion = value;
            }
        }

        /// <summary>
        /// Gets the cached collection of Pipeline ComponentInfo objects.
        /// </summary>
        /// <value>The ComponentInfos collection.</value>
        public static ComponentInfos ComponentInfos
        {
            get
            {
                lock (componentLock)
                {
                    if (!componentInitialised)
                    {
                        if (componentInfos.Count == 0)
                        {

                            PipelineComponentInfos pipelineComponentInfos = Application.PipelineComponentInfos;

                            foreach (PipelineComponentInfo pipelineComponentInfo in pipelineComponentInfos)
                            {
                                if (pipelineComponentInfo.ID == ManagedComponentWrapper)
                                {
                                    componentInfos.Add(pipelineComponentInfo.CreationName, new ComponentInfo(pipelineComponentInfo));
                                }
                                else
                                {
                                    componentInfos.Add(pipelineComponentInfo.CreationName, new ComponentInfo(pipelineComponentInfo));

                                    // Add both the creation name and the component GUID to ensure we get a match
                                    if (pipelineComponentInfo.CreationName != pipelineComponentInfo.ID)
                                    {
                                        componentInfos.Add(pipelineComponentInfo.ID, new ComponentInfo(pipelineComponentInfo));
                                    }                                    
                                }
                            }
                        }

                        componentInitialised = true;
                    }
                }

                return componentInfos;
            }
        }

        /// <summary>
        /// Gets the cached collection of Control Flow ComponentInfo objects.
        /// </summary>
        /// <value>The ComponentInfos collection.</value>
        public static ComponentInfos ControlFlowInfos
        {
            get
            {
                if (controlInfos.Count == 0)
                {
                    TaskInfos taskInfos = Application.TaskInfos;

                    foreach (TaskInfo taskInfo in taskInfos)
                    {
                        ComponentInfo info = new ComponentInfo(taskInfo);

                        controlInfos.Add(taskInfo.CreationName, info);

                        // Tasks can be created using the creation name or 
                        // ID, so need both when they differ
                        if (taskInfo.CreationName != taskInfo.ID)
                        {
                            controlInfos.Add(taskInfo.ID, info);                            
                        }
                    }

                    // Special containers, see GetCreationName usage
                    // TODO:  Consider switching to Microsoft.DataTransformationServices.Design.SharedIcons instead of supplying our own, but is that available in all SQL versions, or just 2014?
                    controlInfos.Add(PackageCreationName, new ComponentInfo(BIDSHelper.Resources.Common.Package));
                    controlInfos.Add(EventHandlerCreationName, new ComponentInfo(BIDSHelper.Resources.Versioned.Event));
                    controlInfos.Add(SequenceCreationName, new ComponentInfo(BIDSHelper.Resources.Versioned.Sequence));
                    controlInfos.Add("STOCK:" + SequenceCreationName.ToUpper(), new ComponentInfo(BIDSHelper.Resources.Versioned.Sequence));
                    controlInfos.Add(ForLoopCreationName, new ComponentInfo(BIDSHelper.Resources.Versioned.ForLoop));
                    controlInfos.Add("STOCK:" + ForLoopCreationName.ToUpper(), new ComponentInfo(BIDSHelper.Resources.Versioned.ForLoop));
                    controlInfos.Add(ForEachLoopCreationName, new ComponentInfo(BIDSHelper.Resources.Versioned.ForEachLoop));
                    controlInfos.Add("STOCK:" + ForEachLoopCreationName.ToUpper(), new ComponentInfo(BIDSHelper.Resources.Versioned.ForEachLoop));

                    // Connections - Cannot get them as with components - Attribute pattern is broken, only used by third
                    // parties. The Connection toolbox doesn't use it so MS haven't attributed their connections. 
                    // We will use a default local resource icon for all connections. 
                    // TODO: Investigate getting proper icon i.e. Microsoft.DataTransformationServices.Graphics.SMTP_connection.ico 
                    controlInfos.Add(ConnectionCreationName, new ComponentInfo(BIDSHelper.Resources.Common.Connection));
                }

                return controlInfos;
            }
        }

        /// <summary>
        /// Get a Type from a TypeCode
        /// </summary>
        /// <remarks>
        /// SSIS does not support the type UInt16 for variables, since this is actually used to store Char types.
        /// </remarks>
        /// <param name="typeCode">TypeCode to lookup Type</param>
        /// <returns>Type matching TypeCode supplied.</returns>
        public static Type GetTypeFromTypeCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return typeof(bool);
                case TypeCode.Byte:
                    return typeof(byte);
                case TypeCode.Char:
                    return typeof(byte);
                case TypeCode.DateTime:
                    return typeof(DateTime);
                case TypeCode.DBNull:
                    return typeof(DBNull);
                case TypeCode.Decimal:
                    return typeof(decimal);
                case TypeCode.Double:
                    return typeof(double);
                case TypeCode.Empty:
                    return null;
                case TypeCode.Int16:
                    return typeof(short);
                case TypeCode.Int32:
                    return typeof(int);
                case TypeCode.Int64:
                    return typeof(long);
                case TypeCode.Object:
                    return typeof(object);
                case TypeCode.SByte:
                    return typeof(sbyte);
                case TypeCode.Single:
                    return typeof(float);
                case TypeCode.String:
                    return typeof(string);
                case TypeCode.UInt16:
                    return typeof(char); // Assign a char, get a UInt16 with SSIS variables
                case TypeCode.UInt32:
                    return typeof(uint);
                case TypeCode.UInt64:
                    return typeof(ulong);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the unique container key, based on the creation name.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The container key</returns>
        public static string GetContainerKey(DtsContainer container)
        {
            string containerKey = container.CreationName;

            if (container is Package)
            {
                containerKey = PackageHelper.PackageCreationName;
            }
            else if (container is DtsEventHandler)
            {
                containerKey = PackageHelper.EventHandlerCreationName;
            }
            else if (container is Sequence)
            {
                containerKey = PackageHelper.SequenceCreationName;
            }
            else if (container is ForLoop)
            {
                containerKey = PackageHelper.ForLoopCreationName;
            }
            else if (container is ForEachLoop)
            {
                containerKey = PackageHelper.ForEachLoopCreationName;
            }

            return containerKey;
        }

        /////// <summary>
        /////// Gets a friendly type name for the task. For managed tasks the type name will work, but for native code tasks we need to avoid "__ComObject".
        /////// </summary>
        /////// <param name="container">The container.</param>
        /////// <returns>The container key</returns>
        ////public static string GetTaskFriendlyName(TaskHost taskHost)
        ////{
        ////    string containerKey = container.CreationName;

        ////    if (container is Package)
        ////    {
        ////        containerKey = PackageHelper.PackageCreationName;
        ////    }
        ////    else if (container is DtsEventHandler)
        ////    {
        ////        containerKey = PackageHelper.EventHandlerCreationName;
        ////    }
        ////    else if (container is Sequence)
        ////    {
        ////        containerKey = PackageHelper.SequenceCreationName;
        ////    }
        ////    else if (container is ForLoop)
        ////    {
        ////        containerKey = PackageHelper.ForLoopCreationName;
        ////    }
        ////    else if (container is ForEachLoop)
        ////    {
        ////        containerKey = PackageHelper.ForEachLoopCreationName;
        ////    }

        ////    return containerKey;
        ////}


        public static string GetComponentKey(IDTSComponentMetaData100 component)
        {
            string key = component.ComponentClassID;
            if (component.ComponentClassID.Equals(ManagedComponentWrapper, StringComparison.OrdinalIgnoreCase))
            {
                key = component.CustomPropertyCollection["UserComponentTypeName"].Value.ToString();
            }
            return key;
        }

        public static IDTSComponentMetaData100 TraceInputToSource(MainPipe mainPipe, IDTSComponentMetaData100 component)
        {
            foreach (IDTSPath100 path in mainPipe.PathCollection)
            {
                if (path.EndPoint.Component.IdentificationString == component.IdentificationString)
                {
                    if (path.StartPoint.SynchronousInputID == 0)
                    {
                        // This is the source component.
                        return path.StartPoint.Component;
                    }
                    else
                    {
                        return TraceInputToSource(mainPipe, path.StartPoint.Component);
                    }
                }
            }
            return null;

        }

        internal static void SetTargetServerVersion(Package package)
        {
            // Get target version of the package, and set on PackageHelper to ensure any ComponentInfos is for the correct info.
            PackageHelper.TargetServerVersion = SSISHelpers.GetTargetServerVersion(package);
        }

        internal static void SetTargetServerVersion(EnvDTE.Project project)
        {
            // Get target version of the package, and set on PackageHelper to ensure any ComponentInfos is for the correct info.
            PackageHelper.TargetServerVersion = SSISHelpers.GetTargetServerVersion(project);
        }

        /// <summary>
        /// Get property value from an object via reflection.
        /// </summary>
        /// <param name="target">The object which hosts the property to get the value of.</param>
        /// <param name="propertyName">The name of the property to get the value of.</param>
        /// <returns>The value of the named property from the target object.</returns>
        internal static object GetPropertyValue(object target, string propertyName)
        {
            if (target == null || propertyName == null) return null;
            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            object result = property.GetValue(target, null);
            return result;
        }
    }

    public enum SourceAccessMode : int
    {
        AM_OPENROWSET = 0,
        AM_OPENROWSET_VARIABLE = 1,
        AM_SQLCOMMAND = 2,
        AM_SQLCOMMAND_VARIABLE = 3
    }

    public class ComponentInfos : Dictionary<string, ComponentInfo>
    { }
}
