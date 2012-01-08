namespace BIDSHelper.SSIS.DesignPracticeScanner
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
    using Microsoft.SqlServer.Dts.Runtime;

    //TODO: Update this to work with 2005
#if KATMAI || DENALI
    class DataFlowAsynchPathsPractice : DesignPractice
    {
        //private ComponentInfos _componentInfos;

        public DataFlowAsynchPathsPractice(string registryPath)
            : base(registryPath)
        {
            base.Name = "Data Flow Asynchronous Paths";
            base.Description = "Checks for asynchronous paths in the data flow";
        }

        public override void Check(Package package, EnvDTE.ProjectItem projectItem)
        {
            Results.Clear();
            //CacheComponentInfo();

            List<TaskHost> pipelines = PackageHelper.GetControlFlowObjects<MainPipe>(package);

            foreach (TaskHost pipe in pipelines)
            {
                ProcessDataFlow((MainPipe)pipe.InnerObject, pipe);
            }
        }

        private void ProcessDataFlow(MainPipe mainPipe, TaskHost taskHost)
        {
            int asyncCount = 0;
            foreach (IDTSPath100 path in mainPipe.PathCollection)
            {
                string key = PackageHelper.GetComponentKey(path.StartPoint.Component);
                if (path.StartPoint.SynchronousInputID != 0)
                {
                    continue;
                }
                if (PackageHelper.ComponentInfos[key].ComponentType != DTSPipelineComponentType.SourceAdapter)
                {
                    asyncCount++;
                }
            }
            //asyncCount = asyncCount - sourceCount;
            if ((asyncCount) > 0)
            {
                Results.Add(new Result(false, string.Format("There are {0} asynchronous outputs in the {1} data flow. Too many asynchronous outputs can adversely impact performance.", asyncCount, taskHost.Name), ResultSeverity.Normal));
            }
        }
    }
#endif

    class DataFlowCountPractice : DesignPractice
    {
        public DataFlowCountPractice(string registryPath)
            : base(registryPath)
        {
            base.Name = "Data Flow Count";
            base.Description = "Checks for the number of data flows in the package";
        }

        public override void Check(Package package, EnvDTE.ProjectItem projectItem)
        {
            Results.Clear();
            List<TaskHost> pipelines = PackageHelper.GetControlFlowObjects<MainPipe>(package);

            if (pipelines.Count > 1)
            {
                Results.Add(new Result(false, string.Format("There are {0} data flows in the package. For simplicity, encapsulation, and to faciltate team development, consider using only one data flow per package.", pipelines.Count), ResultSeverity.Low));
            }

        }

    }

#if KATMAI || DENALI
    class DataFlowSortPractice : DesignPractice
    {
        public DataFlowSortPractice(string registryPath)
            : base(registryPath)
        {
            base.Name = "Data Flow Sort Transformations";
            base.Description = "Checks for the number of sorts in the data flows, and whether they could be performed in a database";
        }

        public override void Check(Package package, EnvDTE.ProjectItem projectItem)
        {
            Results.Clear();
            List<TaskHost> pipelines = PackageHelper.GetControlFlowObjects<MainPipe>(package);
            foreach (TaskHost host in pipelines)
            {
                ProcessDataFlow((MainPipe)host.InnerObject, host);
            }

        }

        private void ProcessDataFlow(MainPipe mainPipe, TaskHost taskHost)
        {
            int sortCount = 0;
            foreach (IDTSComponentMetaData100 comp in mainPipe.ComponentMetaDataCollection)
            {
                string key = PackageHelper.GetComponentKey(comp);

                if (PackageHelper.ComponentInfos[key].CreationName == "DTSTransform.Sort.2")
                {
                    sortCount++;
                    //Trace the input
                    IDTSComponentMetaData100 sourceComp = PackageHelper.TraceInputToSource(mainPipe, comp);


                    if (sourceComp != null)
                    {
                        key = PackageHelper.GetComponentKey(sourceComp);
                        if (PackageHelper.ComponentInfos[key].Name == "OLE DB Source" ||
                            PackageHelper.ComponentInfos[key].Name == "ADO NET Source")
                        {
                            Results.Add(new Result(false, string.Format("The {0} Sort transformation is operating on data provided from the {1} source. Rather than using the Sort transformation, which is fully blocking, the sorting should be performed using a WHERE clause in the source's SQL, and the IsSorted and SortKey properties should be set appropriately. Reference: http://msdn.microsoft.com/en-us/library/ms137653(SQL.90).aspx", comp.Name, sourceComp.Name), ResultSeverity.Normal));
                        }
                    }
                }
            }
            if (sortCount > 2)
            {
                Results.Add(new Result(false, String.Format("There are {0} Sort transfomations in the {1} data flow. A large number of Sorts can slow down data flow performance. Consider staging the data to a relational database and sorting it there.", sortCount, taskHost.Name), ResultSeverity.Normal));
            }
        }
    }
#endif

#if KATMAI || DENALI
    class AccessModePractice : DesignPractice
    {
        public AccessModePractice(string registryPath)
            : base(registryPath)
        {
            base.Name = "Access Mode";
            base.Description = "Validates that sources and Lookup transformations are not set to use the 'Table or View' access mode, as it can be slower than specifying a SQL Statement";
        }

        public override void Check(Package package, EnvDTE.ProjectItem projectItem)
        {
            Results.Clear();
            List<TaskHost> pipelines = PackageHelper.GetControlFlowObjects<MainPipe>(package);
            foreach (TaskHost host in pipelines)
            {
                ProcessDataFlow((MainPipe)host.InnerObject, host);
            }

        }

        private void ProcessDataFlow(MainPipe mainPipe, TaskHost taskHost)
        {
            foreach (IDTSComponentMetaData100 comp in mainPipe.ComponentMetaDataCollection)
            {
                string key = PackageHelper.GetComponentKey(comp);

                if (PackageHelper.ComponentInfos[key].Name == "OLE DB Source" ||
                    PackageHelper.ComponentInfos[key].Name == "ADO NET Source" ||
                    PackageHelper.ComponentInfos[key].Name == "Lookup")
                {
                    IDTSCustomProperty100 prop = comp.CustomPropertyCollection["AccessMode"];

                    if (prop != null && prop.Value is int)
                    {
                        switch ((SourceAccessMode)prop.Value)
                        {
                            case SourceAccessMode.AM_OPENROWSET:
                            case SourceAccessMode.AM_OPENROWSET_VARIABLE:
                                Results.Add(new Result(false, String.Format("Change the {0} component to use a SQL Command access mode, as this performs better than the OpenRowset access mode.", comp.Name), ResultSeverity.Normal));
                                break;
                        }
                    }
                }
            }
        }
    }
#endif
}


