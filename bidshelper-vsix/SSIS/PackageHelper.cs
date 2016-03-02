namespace BIDSHelper.SSIS
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
    using Microsoft.SqlServer.Dts.Runtime;

    internal class PackageHelper
    {
        /// <summary>
        /// All managed components in the data flow share the same wrapper, identified by this GUID.
        /// The specific type of managed component is identified by the UserComponentTypeName 
        /// custom property of the component.
        /// </summary>
        public const string ManagedComponentWrapper = "{33D831DE-5DCF-48F0-B431-4D327B9E785D}";//{bf01d463-7089-41ee-8f05-0a6dc17ce633}";
        

        // Script Component ID
        public const string ScriptComponentID = "Microsoft.SqlServer.Dts.Pipeline.ScriptComponentHost, Microsoft.SqlServer.TxScript, Version=12.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

        public const string PackageCreationName = "Package";
        public const string EventHandlerCreationName = "EventHandler";
        public const string ConnectionCreationName = "Connection";
        public const string SequenceCreationName = "Sequence";
        public const string ForLoopCreationName = "ForLoop";
        public const string ForEachLoopCreationName = "ForEachLoop";

        /// <summary>
        /// Private field for ComponentInfos property
        /// </summary>
        private static ComponentInfos componentInfos = new ComponentInfos();

        /// <summary>
        /// Private field for ComponentInfos property
        /// </summary>
        private static ComponentInfos controlInfos = new ComponentInfos();

        private static bool componentInitialised = false;
        private static object componentLock = new object();

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
                            Application application = new Application();
                            PipelineComponentInfos pipelineComponentInfos = application.PipelineComponentInfos;

                            foreach (PipelineComponentInfo pipelineComponentInfo in pipelineComponentInfos)
                            {
                                if (pipelineComponentInfo.ID == ManagedComponentWrapper)
                                {
                                    componentInfos.Add(pipelineComponentInfo.CreationName, new ComponentInfo(pipelineComponentInfo));
                                }
                                else
                                {
                                    ////if (pipelineComponentInfo.ID == ScriptComponentID)
                                    ////{
                                    ////    // For the script component on SQL 2014, PipelineComponentInfo shows an ID of Microsoft.SqlServer.Dts.Pipeline.ScriptComponentHost, Microsoft.SqlServer.TxScript, Version=12.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
                                    ////    // When enumerating components in the pipeline the ComponentClassID is the GUID, not the creation name
                                    ////    // COM CLSID vs COM ProgID vs assembly strong name, they all get mixed up sometimes.
                                    ////    componentInfos.Add("{33D831DE-5DCF-48F0-B431-4D327B9E785D}", new ComponentInfo(pipelineComponentInfo));
                                    ////}
                                    componentInfos.Add(pipelineComponentInfo.ID, new ComponentInfo(pipelineComponentInfo));
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
                    Application application = new Application();
                    TaskInfos taskInfos = application.TaskInfos;

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
                    controlInfos.Add(PackageCreationName, new ComponentInfo(BIDSHelper.Resources.Common.Package));
                    controlInfos.Add(EventHandlerCreationName, new ComponentInfo(BIDSHelper.Resources.Versioned.Event));
                    controlInfos.Add(SequenceCreationName, new ComponentInfo(BIDSHelper.Resources.Versioned.Sequence));
                    controlInfos.Add(ForLoopCreationName, new ComponentInfo(BIDSHelper.Resources.Versioned.ForLoop));
                    controlInfos.Add(ForEachLoopCreationName, new ComponentInfo(BIDSHelper.Resources.Versioned.ForEachLoop));

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

            string typeName = container.GetType().Name;

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

#if KATMAI || DENALI || SQL2014
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
#endif
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
