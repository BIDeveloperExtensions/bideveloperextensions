using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

#region Conditional compile for Yukon vs Katmai
#if KATMAI || DENALI
using IDTSComponentMetaDataXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData100;
using IDTSOutputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput100;
using IDTSInputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput100;
using IDTSPathXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPath100;
using IDTSInputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn100;
using IDTSOutputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn100;
using IDTSPathCollectionXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPathCollection100;
using IDTSVirtualInputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInput100;
using IDTSVirtualInputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInputColumn100;
using IDTSVirtualInputColumnCollectionXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInputColumnCollection100;
#else
using IDTSComponentMetaDataXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData90;
using IDTSOutputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput90;
using IDTSInputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput90;
using IDTSPathXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPath90;
using IDTSInputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn90;
using IDTSOutputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn90;
using IDTSPathCollectionXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPathCollection90;
using IDTSVirtualInputXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInput90;
using IDTSVirtualInputColumnXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInputColumn90;
using IDTSVirtualInputColumnCollectionXX = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInputColumnCollection90;
#endif
#endregion


namespace BIDSHelper.SSIS.PerformanceVisualization
{
    public class DtsPipelineTestDirector
    {
        private string _TempDirectory;
        private string _OriginalPackagePath;
        private string _StartingPointPackagePath;
        private string _LogFilePath = null;
        private string _DataFlowID;
        private Package _PackageToReference;
        private DtsPipelineComponentTest[] _Tests;
        private bool _TestsRunning;
        private int _TestIndex = 0;
        private System.Diagnostics.Process _Process;
        private bool _Failed = false;
        private bool _ExecutionCancelled = false;
        private bool _PreparingTestIterations = true;
        private Application _app;

        private const string RAW_SOURCE_COMPONENT_NAME_PREFIX = "BIDS_Helper_Raw_Source_";

        public PerformanceTab PerformanceTab;
        public string DtexecPath;
        public readonly DateTime StartTime = DateTime.Now;

        public DtsPipelineTestDirector(string OriginalPackagePath, string DataFlowID)
        {
            this._DataFlowID = DataFlowID;
            this._TempDirectory = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString();
            this._OriginalPackagePath = this._TempDirectory + "\\OriginalPackage.dtsx";
            this._StartingPointPackagePath = this._TempDirectory + "\\StartingPointPackage.dtsx";
            this._LogFilePath = this._TempDirectory + "\\BIDS Helper Pipeline Performance SSIS Log.txt";

            System.IO.Directory.CreateDirectory(this._TempDirectory);
            System.IO.File.Copy(OriginalPackagePath, this._OriginalPackagePath);
            System.IO.File.SetAttributes(this._OriginalPackagePath, System.IO.FileAttributes.Normal);

            this._app = new Application();
        }

        //TODO: skip any step which is simply writing one raw file into another raw file

        private string _PackagePassword;
        public string PackagePassword
        {
            set
            {
                this._app.PackagePassword = value;
                _PackagePassword = value;
            }
        }

        
        public bool AreTestsRunning
        {
            get { return this._TestsRunning; }
        }

        public bool Failed
        {
            get { return this._Failed; }
        }

        public string Status
        {
            get
            {
                if (_TestIndex == 0 || !AreTestsRunning)
                {
                    if (_Failed)
                        return "Failed";
                    else if (_ExecutionCancelled)
                        return "Cancelled";
                    else if (_PreparingTestIterations)
                        return "Preparing Test Iterations";
                    else
                        return "Finished";
                }
                else if (_Tests == null)
                {
                    return "Executing Testing Iterations";
                }
                else
                {
                    return "Executing Testing Iteration " + _TestIndex + " of " + _Tests.Length;
                }
            }
        }

        public List<IDtsGridRowData> GetTestsToDisplay()
        {
            //TODO: organize into rivers
            List<IDtsGridRowData> list = new List<IDtsGridRowData>();
            if (this._Tests != null)
            {
                foreach (DtsPipelineComponentTest test in this._Tests)
                {
                    if (test.TestType == DtsPipelineComponentTestType.SourceOnly || test.TestType == DtsPipelineComponentTestType.DestinationOnly || test.TestType == DtsPipelineComponentTestType.UpstreamOnly)
                    {
                        list.Add(test);
                    }
                }
            }
            return list;
        }

        public void CancelExecution()
        {
            try
            {
                _ExecutionCancelled = true;
                _TestsRunning = false;
                if (_Process != null && !_Process.HasExited)
                {
                    _Process.Kill();
                    _Process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(this.PerformanceTab, "Problem stopping execution:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void ExecuteTests()
        {
            this._Failed = false;
            this._ExecutionCancelled = false;
            this._TestsRunning = false;
            this._TestIndex = 0;

            CreateStartingPointPackage();

            string sNewPackagePath = null;
            Package packageToModify = null;

            this._PackageToReference = _app.LoadPackage(this._StartingPointPackagePath, null);
            Executable exe = FindExecutable(this._PackageToReference, this._DataFlowID);
            if (exe == null || !(exe is TaskHost))
            {
                this._Failed = true;
                System.Windows.Forms.MessageBox.Show(this.PerformanceTab, "Couldn't find data flow task with ID " + this._DataFlowID);
                return;
            }

            TaskHost dataFlowTask = (TaskHost)exe;
            MainPipe pipelineReference = dataFlowTask.InnerObject as MainPipe;
            if (pipelineReference == null)
            {
                this._Failed = true;
                System.Windows.Forms.MessageBox.Show(this.PerformanceTab, "Task ID " + this._DataFlowID + " was not a data flow task");
                return;
            }

            CreateTests(pipelineReference);
            if (_ExecutionCancelled) return;

            this._PreparingTestIterations = false;
            this._TestsRunning = true;

            try
            {
                foreach (DtsPipelineComponentTest test in this._Tests)
                {
                    _TestIndex++;
                    sNewPackagePath = this._TempDirectory + "\\Test" + _TestIndex.ToString() + "_Component" + test.ComponentToTest.ID + "_" + test.TestType.ToString() + ".dtsx";

                    if (_ExecutionCancelled) return;

                    //reload package objects so the prior test's changes don't carry forward
                    packageToModify = _app.LoadPackage(this._StartingPointPackagePath, null);
                    TaskHost taskToModify = (TaskHost)FindExecutable(packageToModify, this._DataFlowID);
                    MainPipe pipeline = (MainPipe)taskToModify.InnerObject;
                    IDTSComponentMetaDataXX component = (IDTSComponentMetaDataXX)pipeline.GetObjectByID(test.ComponentToTest.ID);
                    IDTSComponentMetaDataXX componentReference = (IDTSComponentMetaDataXX)pipelineReference.GetObjectByID(test.ComponentToTest.ID);

                    if (test.TestType == DtsPipelineComponentTestType.SourceOnly)
                    {
                        DeleteAllNonUpstreamComponents(pipeline, component);
                        foreach (IDTSOutputXX output in component.OutputCollection)
                        {
                            if (_ExecutionCancelled) return;
                            if (componentReference.OutputCollection.FindObjectByID(output.ID).IsAttached)
                            {
                                if (HasUnsupportedRawFileColumnDataTypes(componentReference))
                                    HookupRowCountTransform(taskToModify, pipeline, output);
                                else
                                    HookupRawDestination(pipeline, output);
                            }
                        }
                    }
                    else if (test.TestType == DtsPipelineComponentTestType.UpstreamOnly || test.TestType == DtsPipelineComponentTestType.UpstreamOnlyWithoutComponentItself)
                    {
                        ReplaceUpstreamDestinationOutputsWithRawFileSource(pipeline, pipelineReference, component);
                        if (_ExecutionCancelled) return;
                        DeleteAllNonUpstreamComponents(pipeline, component);
                        if (_ExecutionCancelled) return;
                        ReplaceSourcesWithRawFileSource(pipeline, pipelineReference);
                        if (test.TestType == DtsPipelineComponentTestType.UpstreamOnly)
                        {
                            foreach (IDTSOutputXX output in component.OutputCollection)
                            {
                                if (_ExecutionCancelled) return;
                                if (ContainsID(pipelineReference, output.ID) && componentReference.OutputCollection.FindObjectByID(output.ID).IsAttached)
                                {
                                    HookupRowCountTransform(taskToModify, pipeline, output);
                                }
                            }
                        }
                        else
                        {
                            int iInputIndex = 0;
                            while (iInputIndex < component.InputCollection.Count)
                            {
                                if (_ExecutionCancelled) return;
                                IDTSInputXX input = component.InputCollection[iInputIndex];
                                if (input.IsAttached)
                                {
                                    IDTSPathXX path = GetPathForInput(input, pipeline.PathCollection);
                                    IDTSOutputXX output = path.StartPoint;
                                    RemovePath(path, pipeline.PathCollection, false);
                                    HookupRowCountTransform(taskToModify, pipeline, output);
                                }
                                else
                                {
                                    iInputIndex++;
                                }
                            }
                            pipeline.ComponentMetaDataCollection.RemoveObjectByID(component.ID);
                        }
                    }
                    else if (test.TestType == DtsPipelineComponentTestType.DestinationOnlyWithoutComponentItself)
                    {
                        ReplaceUpstreamDestinationOutputsWithRawFileSource(pipeline, pipelineReference, component);
                        if (_ExecutionCancelled) return;
                        DeleteAllNonUpstreamComponents(pipeline, component);
                        if (_ExecutionCancelled) return;
                        ReplaceSourcesWithRawFileSource(pipeline, pipelineReference);
                        if (_ExecutionCancelled) return;

                        IDTSInputXX input = component.InputCollection[0]; //only one input for destinations supported
                        if (input.IsAttached)
                        {
                            IDTSPathXX path = GetPathForInput(input, pipeline.PathCollection);
                            IDTSOutputXX output = path.StartPoint;
                            RemovePath(path, pipeline.PathCollection, false);
                            HookupRawDestination(pipeline, output); //won't ever run this test type if there are unsupported raw file datatypes
                        }
                        pipeline.ComponentMetaDataCollection.RemoveObjectByID(component.ID);
                    }
                    else if (test.TestType == DtsPipelineComponentTestType.DestinationOnly)
                    {
                        bool bHasUnsupportedRawFileColumnDataTypes = HasUnsupportedRawFileColumnDataTypes(component);
                        if (!bHasUnsupportedRawFileColumnDataTypes)
                        {
                            //now use that raw file I just created as a source which goes directly to the component
                            IDTSInputXX input = component.InputCollection[0]; //only one input for destinations supported
                            if (input.IsAttached)
                            {
                                IDTSPathXX path = GetPathForInput(input, pipeline.PathCollection);
                                IDTSOutputXX output = path.StartPoint;
                                IDTSComponentMetaDataXX componentNextInPath = input.Component;
                                RemovePath(path, pipeline.PathCollection, true);
                                HookupRawSource(pipeline, pipelineReference, input, componentNextInPath, GetRawFilePathForOutput(output));
                            }
                        }
                        if (_ExecutionCancelled) return;

                        DeleteAllNonUpstreamComponents(pipeline, component);

                        if (!bHasUnsupportedRawFileColumnDataTypes)
                        {
                            foreach (IDTSOutputXX output in component.OutputCollection)
                            {
                                if (_ExecutionCancelled) return;
                                HookupRawDestination(pipeline, output);
                            }
                        }
                    }
                    if (_ExecutionCancelled) return;
                    AttachRowCountTransformToAllUnattachedOutputs(taskToModify, pipeline);
                    if (_ExecutionCancelled) return;

                    if (System.IO.File.Exists(_LogFilePath)) System.IO.File.Delete(_LogFilePath);

                    _app.SaveToXml(sNewPackagePath, packageToModify, null);

                    DtsPerformanceLogEventParser eventParser = new DtsPerformanceLogEventParser(packageToModify);

                    //setup Process object to call the dtexec EXE
                    _Process = new System.Diagnostics.Process();
                    _Process.StartInfo.UseShellExecute = false;
                    _Process.StartInfo.RedirectStandardError = false;
                    _Process.StartInfo.RedirectStandardOutput = false;
                    _Process.StartInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory(); //inherit the working directory from the current BIDS process (so that relative dtsConfig paths will work)
                    _Process.StartInfo.CreateNoWindow = true;
                    _Process.StartInfo.FileName = "\"" + this.DtexecPath + "\"";
                    _Process.StartInfo.Arguments = "/Rep N /F \"" + sNewPackagePath + "\"";
                    if (!string.IsNullOrEmpty(this._PackagePassword))
                    {
                        _Process.StartInfo.Arguments += " /Decrypt \"" + this._PackagePassword + "\"";
                    }

                    _Process.Start();
                    _Process.WaitForExit();
                    if (_ExecutionCancelled) return;

                    System.Threading.Thread.Sleep(1000); //wait a second in case log events are still flowing

                    DtsTextLogFileLoader logFileLoader = new DtsTextLogFileLoader(_LogFilePath);
                    DtsLogEvent[] events = logFileLoader.GetEvents(true);

                    string sError = "";
                    foreach (DtsLogEvent ee in events)
                    {
                        eventParser.LoadEvent(ee);
                        if (ee.Event == BidsHelperCapturedDtsLogEvent.OnError)
                        {
                            test.IsError = true;
                            sError += ee.SourceName + " - " + ee.Message;
                            break; //first error message is really all we need
                        }
                    }

                    if (test.IsError)
                    {
                        this._TestsRunning = false;
                        this._Failed = true;
                        System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(this.PerformanceTab, "The following error occurred during test iteration " + this._TestIndex + " and package execution has been stopped.\r\n\r\nClick OK to open the temp directory with the test iteration packages to you can troubleshoot (and then delete manually when finished).\r\nClick Cancel to delete the temp directory now.\r\n\r\n" + sError, "BIDS Helper - Pipeline Component Performance Breakdown - Error", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Error);
                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            System.Diagnostics.Process.Start(this._TempDirectory);
                        }
                        else
                        {
                            DeleteTempDirectory();
                        }
                        return;
                    }

                    foreach (IDtsGridRowData row in eventParser.GetAllDtsGanttGridRowDatas())
                    {
                        if (row.UniqueId == this._DataFlowID)
                        {
                            test.TotalSeconds = row.TotalSeconds;
                            break;
                        }
                    }
                    if (_ExecutionCancelled) return;
                }
                DeleteTempDirectory();
            }
            catch (Exception ex)
            {
                //on error, prompt asking them if they would like to save the problem package off somewhere for troubleshooting
                if (packageToModify != null && sNewPackagePath != null)
                {
                    System.Windows.Forms.MessageBox.Show(this.PerformanceTab, "An unexpected error has occurred while executing test iteration " + this._TestIndex + ". The temp directory with the test iteration packages will now open. Please troubleshoot the problem.\r\n\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    try
                    {
                        _app.SaveToXml(sNewPackagePath, packageToModify, null);
                    }
                    catch (Exception ex2)
                    {
                        System.Windows.Forms.MessageBox.Show(this.PerformanceTab, "problem saving package to path " + sNewPackagePath + "\r\n" + ex2.Message + "\r\n" + ex2.StackTrace);
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(this.PerformanceTab, "An unexpected error has occurred while executing test iteration " + this._TestIndex + ". The temp directory with the test iteration packages will now open. Please troubleshoot the problem.\r\n\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
                System.Diagnostics.Process.Start(this._TempDirectory);
            }
            finally
            {
                this._TestsRunning = false;
                if (this._ExecutionCancelled)
                    DeleteTempDirectory();
            }
        }

        private string GetRawFilePathForOutput(IDTSOutputXX output)
        {
            return this._TempDirectory + "\\BIDS_Helper_Raw_Dest_" + output.ID.ToString() + ".raw";
        }

        private void DeleteTempDirectory()
        {
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    System.IO.Directory.Delete(this._TempDirectory, true);
                    break;
                }
                catch
                {
                    //problem deleting... wait half a second then try again
                    System.Threading.Thread.Sleep(500);
                }
            }
            if (System.IO.Directory.Exists(this._TempDirectory))
            {
                System.Windows.Forms.MessageBox.Show(this.PerformanceTab, "Unable to delete temp directory because another process was using it:\r\n" + this._TempDirectory);
            }
        }

        private void CreateTests(MainPipe pipeline)
        {
            List<DtsPipelineComponentTest> tests = new List<DtsPipelineComponentTest>();

            //add all the source-only tests
            foreach (IDTSComponentMetaDataXX component in pipeline.ComponentMetaDataCollection)
            {
                if (_ExecutionCancelled) return;
                if ((component.ObjectType & DTSObjectType.OT_SOURCEADAPTER) == DTSObjectType.OT_SOURCEADAPTER)
                {
                    if (component.InputCollection.Count > 0)
                    {
                        throw new Exception("Source " + component.Name + " has inputs. This is not supported by BIDS Helper.");
                    }

                    DtsPipelineComponentTest test = new DtsPipelineComponentTest();
                    test.TestType = DtsPipelineComponentTestType.SourceOnly;
                    test.ComponentToTest = new DtsPipelineComponent(component.ID, component.Name);
                    tests.Add(test);
                }
            }

            //add all the rest of the (non-source) tests
            {
                List<IDTSComponentMetaDataXX> components = new List<IDTSComponentMetaDataXX>();
                foreach (IDTSComponentMetaDataXX component in pipeline.ComponentMetaDataCollection)
                {
                    if (_ExecutionCancelled) return;
                    if ((component.ObjectType & DTSObjectType.OT_DESTINATIONADAPTER) == DTSObjectType.OT_DESTINATIONADAPTER)
                    {
                        if (component.InputCollection.Count > 1)
                        {
                            throw new Exception("Destination " + component.Name + " has multiple inputs. This is not supported by BIDS Helper.");
                        }
                        components.Add(component);
                    }
                    else if ((component.ObjectType & DTSObjectType.OT_SOURCEADAPTER) != DTSObjectType.OT_SOURCEADAPTER)
                    {
                        components.Add(component);
                    }
                }

                //loop and inspect components to add the test cases in the right order
                //all upstream components must already be added
                //e.g. destinations with error outputs must be added first because they are upstream of other destinations
                while (components.Count > 0)
                {
                    bool bFoundUpstreamComponentNotTested = false;
                    foreach (IDTSComponentMetaDataXX upstream in GetUpstreamComponents(components[0], pipeline.PathCollection))
                    {
                        if (_ExecutionCancelled) return;
                        bool bFoundComponent = false;
                        foreach (DtsPipelineComponentTest othertest in tests)
                        {
                            if (othertest.ComponentToTest.ID == upstream.ID)
                            {
                                bFoundComponent = true;
                                break;
                            }
                        }
                        if (!bFoundComponent)
                        {
                            bFoundUpstreamComponentNotTested = true;
                            break;
                        }
                    }

                    if (bFoundUpstreamComponentNotTested)
                    {
                        //move this component to the end of the list
                        components.Add(components[0]);
                        components.RemoveAt(0);
                        continue;
                    }

                    bool bSkipFindingImmediatelyUpstreamTest = false;
                    DtsPipelineComponentTest test = new DtsPipelineComponentTest();
                    if ((components[0].ObjectType & DTSObjectType.OT_DESTINATIONADAPTER) == DTSObjectType.OT_DESTINATIONADAPTER)
                    {
                        if (!HasUnsupportedRawFileColumnDataTypes(components[0]))
                        {
                            DtsPipelineComponentTest priorTest = new DtsPipelineComponentTest();
                            priorTest.ComponentToTest = new DtsPipelineComponent(components[0].ID, components[0].Name);
                            priorTest.TestType = DtsPipelineComponentTestType.DestinationOnlyWithoutComponentItself;
                            tests.Add(priorTest);
                            bSkipFindingImmediatelyUpstreamTest = true;
                        }

                        test.TestType = DtsPipelineComponentTestType.DestinationOnly;
                        test.ComponentToTest = new DtsPipelineComponent(components[0].ID, components[0].Name);
                    }
                    else
                    {
                        test.TestType = DtsPipelineComponentTestType.UpstreamOnly;
                        test.ComponentToTest = new DtsPipelineComponent(components[0].ID, components[0].Name);
                    }

                    if (!bSkipFindingImmediatelyUpstreamTest)
                    {
                        //count the attached inputs
                        int iAttachedInputs = 0;
                        foreach (IDTSInputXX input in components[0].InputCollection)
                        {
                            if (input.IsAttached)
                            {
                                iAttachedInputs++;
                            }
                        }
                        if (iAttachedInputs > 1)
                        {
                            //this component has multiple inputs, so we need to construct a test that we can subtract from the current test to tell the current test's incremental duration
                            DtsPipelineComponentTest immediateUpstreamTest = new DtsPipelineComponentTest();
                            immediateUpstreamTest.TestType = DtsPipelineComponentTestType.UpstreamOnlyWithoutComponentItself;
                            immediateUpstreamTest.ComponentToTest = new DtsPipelineComponent(components[0].ID, components[0].Name);
                            tests.Add(immediateUpstreamTest);

                            test.ImmediatelyUpstreamTest = immediateUpstreamTest;
                        }

                        //find the immediately upstream test
                        foreach (IDTSInputXX input in components[0].InputCollection)
                        {
                            if (_ExecutionCancelled) return;
                            if (input.IsAttached && test.ImmediatelyUpstreamTest == null)
                            {
                                IDTSPathXX path = GetPathForInput(input, pipeline.PathCollection);
                                if (path == null) continue;
                                foreach (DtsPipelineComponentTest othertest in tests)
                                {
                                    if (othertest.TestType == DtsPipelineComponentTestType.UpstreamOnly && othertest.ComponentToTest.ID == path.StartPoint.Component.ID)
                                    {
                                        test.ImmediatelyUpstreamTest = othertest;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    tests.Add(test);

                    components.RemoveAt(0);
                }
            } //end of adding destination tests

            this._Tests = tests.ToArray();
        }

        /// <summary>
        /// strips out everything but the dataflow task itself
        /// deals with variables in parent containers and adds them to the package
        /// </summary>
        private void CreateStartingPointPackage()
        {
            Package package = _app.LoadPackage(this._OriginalPackagePath, null);

            //kill the layout information so that the package diagram layout won't throw an error when you open the package in BIDS to troubleshoot it
            //this will reset the layout and cause BIDS to autolayout
            while (package.ExtendedProperties.Count > 0)
                package.ExtendedProperties.Remove(0);

            Executable dataFlowEXE = FindExecutable(package, this._DataFlowID);
            if (dataFlowEXE == null || !(dataFlowEXE is TaskHost)) throw new Exception("Couldn't find data flow ID " + this._DataFlowID);
            TaskHost dataFlowTask = (TaskHost)dataFlowEXE;

            List<DtsContainer> parentContainers = new List<DtsContainer>();
            DtsContainer parentContainer = dataFlowTask.Parent;
            while (!(parentContainer is Package))
            {
                parentContainers.Insert(0, parentContainer);
                parentContainer = parentContainer.Parent;
            }

            foreach (DtsContainer parentDtsContainer in parentContainers)
            {
                //IDTSSequence parentContainer = (IDTSSequence)packageToReference.Executables[1];
                //v.GetPackagePath().StartsWith("\\" + objectPath + "Variables[")
                //DtsContainer parentDtsContainer = (DtsContainer)parentContainer;

                foreach (Variable v in parentDtsContainer.Variables)
                {
                    if (!v.SystemVariable)
                    {
                        if (v.GetPackagePath().StartsWith(((IDTSPackagePath)parentDtsContainer).GetPackagePath() + ".Variables["))
                        {
                            Variable newV;
                            if (package.Variables.Contains(v.QualifiedName))
                            {
                                newV = package.Variables[v.QualifiedName];
                                newV.Value = v.Value;
                            }
                            else
                            {
                                newV = package.Variables.Add(v.Name, v.ReadOnly, v.Namespace, v.Value);
                            }
                            newV.EvaluateAsExpression = v.EvaluateAsExpression;
                            newV.Expression = v.Expression;
                            newV.Description = v.Description;
                        }
                    }
                }
            }
            ((IDTSSequence)dataFlowTask.Parent).Executables.Remove(dataFlowEXE);

            while (package.Executables.Count > 0)
                package.Executables.Remove(0);

            package.Executables.Join(dataFlowEXE);

            while (dataFlowTask.EventHandlers.Count > 0)
                dataFlowTask.EventHandlers.Remove(0);

            while (package.EventHandlers.Count > 0)
                package.EventHandlers.Remove(0);

            PerformanceTab.SetupCustomLogging(package, _LogFilePath);

            _app.SaveToXml(this._StartingPointPackagePath, package, null);
        }

        #region Pipeline Helper Functions
        private static bool HasUnsupportedRawFileColumnDataTypes(IDTSComponentMetaDataXX component)
        {
            foreach (IDTSInputXX input in component.InputCollection)
            {
                if (input.IsAttached)
                {
                    foreach (IDTSInputColumnXX col in input.InputColumnCollection)
                    {
                        if (col.DataType == Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_IMAGE || col.DataType == Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NTEXT || col.DataType == Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_TEXT)
                        {
                            return true;
                        }
                    }
                }
            }
            foreach (IDTSOutputXX output in component.OutputCollection)
            {
                if (output.IsAttached)
                {
                    foreach (IDTSOutputColumnXX col in output.OutputColumnCollection)
                    {
                        if (col.DataType == Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_IMAGE || col.DataType == Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NTEXT || col.DataType == Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_TEXT)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //workaround to http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=3161935&SiteID=1
        private static void RemovePath(IDTSPathXX path, IDTSPathCollectionXX paths, bool AllowDeleteStartPoint)
        {
            if (!AllowDeleteStartPoint && path.StartPoint.DeleteOutputOnPathDetached)
            {
                System.Diagnostics.Debug.WriteLine("path going to delete output after detached!");
                path.StartPoint.DeleteOutputOnPathDetached = false;
            }
            bool bSuccess = false;
            try
            {
                paths.RemoveObjectByID(path.ID);
                bSuccess = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Caught error during RemovePath: " + ex.Message);
                if (!ContainsID(paths, path.ID))
                    bSuccess = true;
            }
            if (!bSuccess) throw new Exception("Could not remove path");
        }

        private static IDTSPathXX GetPathForInput(IDTSInputXX input, IDTSPathCollectionXX paths)
        {
            foreach (IDTSPathXX path in paths)
            {
                if (path.EndPoint.ID == input.ID)
                    return path;
            }
            return null;
        }

        private static IDTSComponentMetaDataXX[] GetUpstreamComponents(IDTSComponentMetaDataXX component, IDTSPathCollectionXX paths)
        {
            List<IDTSComponentMetaDataXX> componentsToReturn = new List<IDTSComponentMetaDataXX>();
            
            foreach (IDTSPathXX upstream in paths)
            {
                if (upstream.EndPoint.Component.ID == component.ID)
                {
                    if (!componentsToReturn.Contains(upstream.StartPoint.Component))
                    {
                        componentsToReturn.Add(upstream.StartPoint.Component);
                        componentsToReturn.AddRange(GetUpstreamComponents(upstream.StartPoint.Component, paths));
                    }
                }
            }
            return componentsToReturn.ToArray();
        }

        //to workaround a problem when we're testing the error branch of a flat file source. There must be a main output from such a source, and this puts a rowcount on that output.
        private static void AttachRowCountTransformToAllUnattachedOutputs(TaskHost dataFlowTask, MainPipe pipeline)
        {
            foreach (IDTSComponentMetaDataXX component in pipeline.ComponentMetaDataCollection)
            {
                if (!component.Name.StartsWith("BIDS_Helper_"))
                {
                    foreach (IDTSOutputXX output in component.OutputCollection)
                    {
                        if (!output.IsAttached && !output.IsErrorOut)
                        {
                            HookupRowCountTransform(dataFlowTask, pipeline, output);
                        }
                    }
                }
            }
        }

        private static void HookupRowCountTransform(TaskHost dataFlowTask, MainPipe pipeline, IDTSOutputXX output)
        {
            Variable variable = dataFlowTask.Variables.Add("BIDS_HELPER_ROWCOUNT_" + output.ID, false, "User", (int)0);

            IDTSComponentMetaDataXX transform = pipeline.ComponentMetaDataCollection.New();
            transform.ComponentClassID = "DTSTransform.RowCount";
            CManagedComponentWrapper inst = transform.Instantiate();
            inst.ProvideComponentProperties();
            inst.SetComponentProperty("VariableName", variable.QualifiedName);

            string sOutputName = output.Name;
            int iComponentID = output.Component.ID;

            IDTSPathXX path = pipeline.PathCollection.New();
            path.AttachPathAndPropagateNotifications(output, transform.InputCollection[0]);

            transform.Name = "BIDS_Helper_RowCount_" + path.StartPoint.ID.ToString();
        }

        private void HookupRawDestination(MainPipe pipeline, IDTSOutputXX output)
        {
            IDTSComponentMetaDataXX rawDestComponent = pipeline.ComponentMetaDataCollection.New();
            rawDestComponent.ComponentClassID = "DTSAdapter.RawDestination";
            CManagedComponentWrapper inst = rawDestComponent.Instantiate();
            inst.ProvideComponentProperties();
            inst.SetComponentProperty("FileName", GetRawFilePathForOutput(output));

            IDTSPathXX path = pipeline.PathCollection.New();
            path.AttachPathAndPropagateNotifications(output, rawDestComponent.InputCollection[0]);
            IDTSVirtualInputXX vInput = path.EndPoint.GetVirtualInput();
            foreach (IDTSVirtualInputColumnXX vColumn in vInput.VirtualInputColumnCollection)
            {
                inst.SetUsageType(path.EndPoint.ID, vInput, vColumn.LineageID, DTSUsageType.UT_READONLY);
            }
            rawDestComponent.Name = "BIDS_Helper_Raw_Dest_" + output.ID.ToString();
        }

        private static void HookupRawSource(MainPipe pipeline, MainPipe pipelineToReference, IDTSInputXX input, IDTSComponentMetaDataXX componentNextInPath, string sRawFilePath)
        {
            IDTSComponentMetaDataXX rawSourceComponent = pipeline.ComponentMetaDataCollection.New();
            rawSourceComponent.ComponentClassID = "DTSAdapter.RawSource";
            CManagedComponentWrapper inst = rawSourceComponent.Instantiate();
            inst.ProvideComponentProperties();
            inst.SetComponentProperty("FileName", sRawFilePath);
            try
            {
                inst.AcquireConnections(null);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to locate raw file at the following path which means that the previous test iteration did not run. Is the data flow task disabled?\r\n\r\n" + sRawFilePath, ex);
            }
            inst.ReinitializeMetaData();
            inst.ReleaseConnections();

            IDTSPathXX pathToReference = GetPathForInput(input, pipelineToReference.PathCollection);
            if (pathToReference == null) throw new Exception("Couldn't find path for input " + input.ID);

            if (pathToReference.StartPoint.IsSorted)
            {
                rawSourceComponent.OutputCollection[0].IsSorted = true;
            }

            Dictionary<string, List<IDTSVirtualInputColumnXX>> dictVirtualColumns = new Dictionary<string, List<IDTSVirtualInputColumnXX>>();
            IDTSVirtualInputColumnCollectionXX colCollection = pathToReference.EndPoint.GetVirtualInput().VirtualInputColumnCollection;
            foreach (IDTSVirtualInputColumnXX sourceColToReference in colCollection)
            {
                string sColName = sourceColToReference.Name;
                if (dictVirtualColumns.ContainsKey(sColName))
                {
                    dictVirtualColumns[sColName].Add(sourceColToReference);
                }
                else
                {
                    List<IDTSVirtualInputColumnXX> list = new List<IDTSVirtualInputColumnXX>();
                    list.Add(sourceColToReference);
                    dictVirtualColumns.Add(sColName, list);
                }
            }

            Dictionary<int, int> lineageIdReplacements = new Dictionary<int, int>();
            foreach (IDTSOutputColumnXX outputCol in rawSourceComponent.OutputCollection[0].OutputColumnCollection)
            {
                string sColName = outputCol.Name;
                if (dictVirtualColumns.ContainsKey(sColName))
                {
                    foreach (IDTSVirtualInputColumnXX sourceColToReference in dictVirtualColumns[sColName])
                    {
                        if (!lineageIdReplacements.ContainsKey(sourceColToReference.LineageID))
                        {
                            lineageIdReplacements.Add(sourceColToReference.LineageID, outputCol.LineageID);
                            if (sourceColToReference.SortKeyPosition != 0)
                                outputCol.SortKeyPosition = sourceColToReference.SortKeyPosition;
                        }
                    }
                }
                //foreach (IDTSVirtualInputColumnXX sourceColToReference in colCollection)
                //{
                //    if (sourceColToReference.Name == outputCol.Name && !lineageIdReplacements.ContainsKey(sourceColToReference.LineageID))
                //    {
                //        lineageIdReplacements.Add(sourceColToReference.LineageID, outputCol.LineageID);
                //        if (sourceColToReference.SortKeyPosition != 0)
                //            outputCol.SortKeyPosition = sourceColToReference.SortKeyPosition;
                //    }
                //}
            }

            if (!ContainsID(pipeline, input.ID))
            {
                //look for an unattached input and use it instead
                //it's hopefully a fair assumption that if deleting the path to an input resulted in the input automatically being deleted, then there's nothing special about that input and any other can be used.
                //this is the case with the Union All transform for which this is a workaround
                bool bFoundAlt = false;
                foreach (IDTSInputXX inputAlt in componentNextInPath.InputCollection)
                {
                    if (!inputAlt.IsAttached)
                    {
                        input = inputAlt;
                        bFoundAlt = true;
                        break;
                    }
                }

                if (!bFoundAlt)
                {
                    //create a new input
                    string sNewInputName = "BIDS_Helper_Input_" + input.ID;
                    input = componentNextInPath.InputCollection.New();
                    input.Name = sNewInputName;
                }
            }

            LineageIdReplacer.ReplaceLineageIDs(pipeline, lineageIdReplacements);

            IDTSPathXX path = pipeline.PathCollection.New();
            path.AttachPathAndPropagateNotifications(rawSourceComponent.OutputCollection[0], input);

            rawSourceComponent.Name = RAW_SOURCE_COMPONENT_NAME_PREFIX + pathToReference.StartPoint.ID.ToString();
        }

        private static void DeleteAllNonUpstreamComponents(MainPipe pipeline, IDTSComponentMetaDataXX component)
        {
            List<int> upstreamComponentIDs = new List<int>();
            upstreamComponentIDs.Add(component.ID);
            foreach (IDTSComponentMetaDataXX upstream in GetUpstreamComponents(component, pipeline.PathCollection))
            {
                upstreamComponentIDs.Add(upstream.ID);
            }

            //delete all paths not upstream
            int iPath = 0;
            while (iPath < pipeline.PathCollection.Count)
            {
                IDTSPathXX path = pipeline.PathCollection[iPath];
                if (!upstreamComponentIDs.Contains(path.StartPoint.Component.ID) || !upstreamComponentIDs.Contains(path.EndPoint.Component.ID))
                {
                    RemovePath(path, pipeline.PathCollection, false);
                }
                else
                {
                    iPath++;
                }
            }

            //remove all non-upstream components
            int iComponent = 0;
            while (iComponent < pipeline.ComponentMetaDataCollection.Count)
            {
                IDTSComponentMetaDataXX componentToRemove = pipeline.ComponentMetaDataCollection[iComponent];
                if (!upstreamComponentIDs.Contains(componentToRemove.ID))
                {
                    pipeline.ComponentMetaDataCollection.RemoveObjectByID(componentToRemove.ID);
                }
                else
                {
                    iComponent++;
                }
            }
        }

        private void ReplaceSourcesWithRawFileSource(MainPipe pipeline, MainPipe pipelineToReference)
        {
            List<int> sourceComponents = new List<int>();
            foreach (IDTSComponentMetaDataXX component in pipeline.ComponentMetaDataCollection)
            {
                if ((component.ObjectType & DTSObjectType.OT_SOURCEADAPTER) == DTSObjectType.OT_SOURCEADAPTER)
                {
                    if (!component.Name.StartsWith(RAW_SOURCE_COMPONENT_NAME_PREFIX) && !HasUnsupportedRawFileColumnDataTypes(pipelineToReference.ComponentMetaDataCollection.FindObjectByID(component.ID)))
                        sourceComponents.Add(component.ID);
                }
            }

            foreach (int iComponentID in sourceComponents)
            {
                int iPath = 0;
                while (iPath < pipeline.PathCollection.Count)
                {
                    IDTSPathXX path = pipeline.PathCollection[iPath];
                    if (path.StartPoint.Component.ID == iComponentID && ContainsID(pipelineToReference.PathCollection, path.ID))
                    {
                        IDTSPathXX pathToReference = pipelineToReference.PathCollection.FindObjectByID(path.ID);
                        string sRawFilePath = GetRawFilePathForOutput(path.StartPoint);
                        IDTSInputXX input = path.EndPoint;
                        IDTSComponentMetaDataXX componentNextInPath = input.Component;
                        RemovePath(path, pipeline.PathCollection, true);
                        HookupRawSource(pipeline, pipelineToReference, input, componentNextInPath, sRawFilePath);
                    }
                    else
                    {
                        iPath++;
                    }
                }
                pipeline.ComponentMetaDataCollection.RemoveObjectByID(iComponentID);
            }
        }

        //would expect that PathCollection.FindObjectByID would do this, but it appears to throw an exception if not found
        private static bool ContainsID(IDTSPathCollectionXX paths, int ID)
        {
            foreach (IDTSPathXX path in paths)
            {
                if (path.ID == ID)
                    return true;
            }
            return false;
        }

        private static bool ContainsID(MainPipe pipe, int ID)
        {
            try
            {
                object obj = pipe.GetObjectByID(ID);
                return (obj != null);
            }
            catch
            {
                return false;
            }
        }

        private void ReplaceUpstreamDestinationOutputsWithRawFileSource(MainPipe pipeline, MainPipe pipelineToReference, IDTSComponentMetaDataXX downstreamComponent)
        {
            List<int> destErrorOutputsComponents = new List<int>();
            foreach (IDTSComponentMetaDataXX component in GetUpstreamComponents(downstreamComponent, pipeline.PathCollection))
            {
                if ((component.ObjectType & DTSObjectType.OT_DESTINATIONADAPTER) == DTSObjectType.OT_DESTINATIONADAPTER)
                {
                    foreach (IDTSOutputXX errorOutput in component.OutputCollection)
                    {
                        if (errorOutput.IsAttached && !HasUnsupportedRawFileColumnDataTypes(pipelineToReference.ComponentMetaDataCollection.FindObjectByID(component.ID)))
                        {
                            destErrorOutputsComponents.Add(component.ID);
                        }
                    }
                }
            }

            foreach (int iComponentID in destErrorOutputsComponents)
            {
                int iPath = 0;
                while (iPath < pipeline.PathCollection.Count)
                {
                    IDTSPathXX path = pipeline.PathCollection[iPath];
                    if (path.StartPoint.Component.ID == iComponentID && ContainsID(pipelineToReference.PathCollection, path.ID))
                    {
                        IDTSPathXX pathToReference = pipelineToReference.PathCollection.FindObjectByID(path.ID);
                        string sRawFilePath = GetRawFilePathForOutput(path.StartPoint);
                        IDTSInputXX input = path.EndPoint;
                        IDTSOutputXX output = path.StartPoint;
                        if (output.IsErrorOut && output.IsAttached)
                        {
                            IDTSComponentMetaDataXX componentNextInPath = input.Component;
                            RemovePath(path, pipeline.PathCollection, true);
                            HookupRawSource(pipeline, pipelineToReference, input, componentNextInPath, sRawFilePath);
                        }
                        else
                        {
                            RemovePath(path, pipeline.PathCollection, true);
                        }
                    }
                    else if (path.EndPoint.Component.ID == iComponentID)
                    {
                        RemovePath(path, pipeline.PathCollection, true);
                    }
                    else
                    {
                        iPath++;
                    }
                }
                pipeline.ComponentMetaDataCollection.RemoveObjectByID(iComponentID);
            }
        }

        //recursively looks in executables to find executable with the specified GUID
        private Executable FindExecutable(IDTSSequence parentExecutable, string sObjectGuid)
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
                        if (matchingExecutable != null) return matchingExecutable;
                    }
                }
            }
            return matchingExecutable;
        }
        #endregion

        #region Pipeline Test Helper Classes
        public class DtsPipelineComponentTest : IDtsGridRowData
        {
            public DtsPipelineComponentTestType TestType;
            public DtsPipelineComponent ComponentToTest;
            public DtsPipelineComponentTest ImmediatelyUpstreamTest;

            public string UniqueId
            {
                get { return ComponentToTest.ID.ToString(); }
            }

            public string Name
            {
                get { return ComponentToTest.Name; }
            }

            public long? RowCount
            {
                get { return null; }
            }

            public long? BufferCount
            {
                get { return null; }
            }

            private bool _IsError = false;
            public bool IsError
            {
                get { return _IsError; }
                set { _IsError = value; }
            }

            public List<DateRange> DateRanges
            {
                get { return null; }
            }

            private int? _TotalSeconds;
            public int? TotalSeconds
            {
                get
                {
                    if (ImmediatelyUpstreamTest == null)
                        return _TotalSeconds;
                    else if (_TotalSeconds == null || ImmediatelyUpstreamTest._TotalSeconds == null)
                        return null;
                    else
                        return _TotalSeconds - ImmediatelyUpstreamTest._TotalSeconds;
                }
                set { _TotalSeconds = value; }
            }

            public int? BufferRowCount
            {
                get { return null; }
            }

            public int? BufferEstimatedBytesPerRow
            {
                get { return null; }
            }

            public double? InboundRowsSec
            {
                get { return null; }
            }

            public double? OutboundRowsSec
            {
                get { return null; }
            }

            public double? InboundKbSec
            {
                get { return null; }
            }

            public double? OutboundKbSec
            {
                get { return null; }
            }

            public Type Type
            {
                get { return this.GetType(); }
            }

            private int _Indent;
            public int Indent
            {
                get { return _Indent; }
                set { _Indent = value; }
            }

            public bool HasChildren
            {
                get { return false; }
            }
        }

        public class DtsPipelineComponent
        {
            public DtsPipelineComponent(int ID, string Name)
            {
                this.ID = ID;
                this.Name = Name;
            }
            public int ID;
            public string Name;
        }

        public enum DtsPipelineComponentTestType
        {
            /// <summary>
            /// Testing the source only. Source data is written directly to a raw file destination.
            /// </summary>
            SourceOnly,

            /// <summary>
            /// Testing the destination only. Raw file source is flowed directly to the destination with no transforms.
            /// </summary>
            DestinationOnly,

            /// <summary>
            /// Testing the full pipeline upstream excluding the particular destination being tested.
            /// The input to this particular destination is written to a raw file.
            /// This raw file can then be used in a separate test to isolate destination performance.
            /// </summary>
            DestinationOnlyWithoutComponentItself,

            /// <summary>
            /// Testing the full pipeline upstream through the component being tested.
            /// All outputs of this component lead to rowcount transforms.
            /// A raw file source substituted for the real source components.
            /// </summary>
            UpstreamOnly,

            /// <summary>
            /// Testing the full pipeline upstream excluding the particular component being tested.
            /// This test can be subtracted from the UpstreamOnly test for this component to calculate incremental component duration.
            /// All outputs of this component lead to rowcount transforms.
            /// A raw file source substituted for the real source components.
            /// </summary>
            UpstreamOnlyWithoutComponentItself
        }
        #endregion
    }
}
