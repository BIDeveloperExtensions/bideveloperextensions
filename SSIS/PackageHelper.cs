using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper.SSIS
{
    class PackageHelper
    {
        //Magic number - All managed components in the data flow share the same wrapper, identified by this GUID.
        //The specific type of managed component is identified by the UserComponentTypeName custom property of the component.
        public const string ManagedComponentWrapper = "{bf01d463-7089-41ee-8f05-0a6dc17ce633}";

        private static ComponentInfos _componentInfos = new ComponentInfos();

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


        public static ComponentInfos ComponentInfos
        {
            get
            {
                if (_componentInfos.Count == 0)
                {
                    Application application = new Application();
                    PipelineComponentInfos pipelineComponentInfos = application.PipelineComponentInfos;

                    foreach (PipelineComponentInfo pipelineComponentInfo in pipelineComponentInfos)
                    {
                        if (pipelineComponentInfo.ID == ManagedComponentWrapper)
                        {
                            _componentInfos.Add(pipelineComponentInfo.CreationName, new ComponentInfo(pipelineComponentInfo));
                        }
                        else
                        {
                            _componentInfos.Add(pipelineComponentInfo.ID, new ComponentInfo(pipelineComponentInfo));
                        }
                    }
                }
                return _componentInfos;
            }
        }


#if KATMAI
        public static string GetComponentKey(IDTSComponentMetaData100 component)
        {
            string key = component.ComponentClassID;
            if (component.ComponentClassID == ManagedComponentWrapper)
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
                        //This is the source component.
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

    public class ComponentInfo
    {
        public ComponentInfo(PipelineComponentInfo componentInfo)
        {
            ComponentType = componentInfo.ComponentType;
            ID = componentInfo.ID;
            Name = componentInfo.Name;
            CreationName = componentInfo.CreationName;
        }

        private DTSPipelineComponentType _ComponentType;
        public DTSPipelineComponentType ComponentType
        {
            get { return _ComponentType; }
            set { _ComponentType = value; }
        }

        private string _ID;
        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _CreationName;
        public string CreationName
        {
            get { return _CreationName; }
            set { _CreationName = value; }
        }
    }




}
