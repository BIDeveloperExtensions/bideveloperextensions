using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace BIDSHelper.SSIS.DesignPracticeScanner
{

    //TODO: Update this to work with 2005
#if KATMAI
    class DataFlowPerformancePractices : DesignPractice
    {
        internal const string ManagedComponentWrapper = "{bf01d463-7089-41ee-8f05-0a6dc17ce633}";
        private ComponentInfos _componentInfos;

        public DataFlowPerformancePractices()
        {
            base.Name = "Data Flow Performance";
            base.Description = "Checks for a number of possible performance hindrances in the data flow";
        }

        public override void Check(Package package, EnvDTE.ProjectItem projectItem)
        {
            Results.Clear();
            CacheComponentInfo();

            List<TaskHost> pipelines = GetControlFlowObjects<MainPipe>(package);

            foreach (TaskHost pipe in pipelines)
            {
                ProcessDataFlow((MainPipe)pipe.InnerObject, pipe);
            } 
        }

        private static List<TaskHost> GetControlFlowObjects<T>(DtsContainer container)
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

        private void ProcessDataFlow(MainPipe mainPipe, TaskHost taskHost)
        {
            int sourceCount = 0;
            foreach (IDTSComponentMetaData100 result in mainPipe.ComponentMetaDataCollection)
            {
                string key = result.ComponentClassID;
                if (result.ComponentClassID == ManagedComponentWrapper)
                {
                    key = result.CustomPropertyCollection["UserComponentTypeName"].Value.ToString();
                }
                if (_componentInfos[key].ComponentType == DTSPipelineComponentType.SourceAdapter)
                {
                    sourceCount++;
                }
            }

            int asyncCount = 0;
            foreach (IDTSPath100 path in mainPipe.PathCollection)
            {
                if (path.StartPoint.SynchronousInputID == 0)
                {
                    asyncCount++;
                }
            }
            asyncCount = asyncCount - sourceCount;
            if ((asyncCount) > 0)
            {
                Results.Add(new Result(false, string.Format("There are {0} asynchronous outputs in the {1} data flow. Too many asynchronous outputs can adversely impact performance.", asyncCount, taskHost.Name), ResultSeverity.Normal));
            }
        }

        private void CacheComponentInfo()
        {
            if (_componentInfos != null)
            {
                return;
            }
            _componentInfos = new ComponentInfos();

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

        private class ComponentInfos : Dictionary<string, ComponentInfo>
        { }

        private class ComponentInfo
        {
            public ComponentInfo(PipelineComponentInfo componentInfo)
            {
                ComponentType = componentInfo.ComponentType;
                ID = componentInfo.ID;
                Name = componentInfo.Name;
                CreationName = componentInfo.CreationName;
            }

            public DTSPipelineComponentType ComponentType { get; set; }
            public string ID { get; set; }
            public string Name { get; set; }
            public string CreationName { get; set; }
        }


    }
#endif
}
