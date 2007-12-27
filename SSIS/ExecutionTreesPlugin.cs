using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Text;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using System;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.DataTransformationServices.Project;
using System.Runtime.InteropServices;
using Microsoft.DataTransformationServices.Project.DebugEngine;

namespace BIDSHelper
{
    public class ExecutionTreesPlugin : BIDSHelperPluginBase
    {
        public ExecutionTreesPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "ExecutionTreesPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "Execution Trees"; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }

        public override bool DisplayCommand(UIHierarchyItem item)
        {
            return false; //TODO: just temporary to disable this from showing up in checked in version

            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            if (((System.Array)solExplorer.SelectedItems).Length != 1)
                return false;

            UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
            string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
            return (sFileName.EndsWith(".dtsx"));
        }


        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                ProjectItem pi = (ProjectItem)hierItem.Object;

                Window w = pi.Open("{7651A702-06E5-11D1-8EBD-00A0C90F26EA}"); //opens the designer, I guess?
                w.Activate();

                IDesignerHost designer = w.Object as IDesignerHost;
                if (designer == null) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                Package package = win.PropertiesLinkComponent as Package;
                if (package == null) return;

                Microsoft.DataTransformationServices.Design.DtsBasePackageDesigner rootDesigner = typeof(EditorWindow).InvokeMember("designer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.GetField, null, win, null) as Microsoft.DataTransformationServices.Design.DtsBasePackageDesigner;
                string packagePassword = null;
                if (rootDesigner != null)
                {
                    packagePassword = rootDesigner.GetPackagePassword();
                }

                Microsoft.DataWarehouse.Interfaces.IConfigurationSettings settings = (Microsoft.DataWarehouse.Interfaces.IConfigurationSettings)((System.IServiceProvider)pi.ContainingProject).GetService(typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings));
                Microsoft.DataWarehouse.Project.DataWarehouseProjectManager projectManager = (Microsoft.DataWarehouse.Project.DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);

                //the following code comes from Microsoft.DataTransformationServices.Project.DtsPackagesFolderProjectFeature.ExecuteTaskOrPackage
                Microsoft.DataWarehouse.Project.DataWarehouseProjectConfiguration currentConfiguration = (Microsoft.DataWarehouse.Project.DataWarehouseProjectConfiguration)projectManager.ConfigurationManager.CurrentConfiguration;
                if (currentConfiguration == null) return;
                IOutputWindow standardOutputWindow = (projectManager.GetService(typeof(IOutputWindowFactory)) as IOutputWindowFactory).GetStandardOutputWindow(StandardOutputWindow.Debug);
                standardOutputWindow.Clear();
                pi.Document.Save(null);

                Type typeDebugger = ExpressionHighlighterPlugin.GetPrivateType(typeof(Microsoft.DataTransformationServices.Project.DataTransformationsConfiguration), "Microsoft.DataTransformationServices.Project.DataTransformationsPackageDebugger");
                System.Reflection.ConstructorInfo constructor = typeDebugger.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly, null, new Type[] { typeof(ProjectItem), typeof(string) }, null);
                object debugger = constructor.Invoke(new object[] { (ProjectItem)pi, (string)null });

                //the following code mimics Microsoft.DataTransformationServices.Project.DataTransformationsPackageDebugger.ValidateAndRunDebugger so we can override what dtsx file it runs off of
                Microsoft.VisualStudio.Shell.Interop.IVsDebugger vsDebuggerSvc = projectManager.GetService(typeof(Microsoft.VisualStudio.Shell.Interop.IVsDebugger)) as Microsoft.VisualStudio.Shell.Interop.IVsDebugger;
                Microsoft.DataWarehouse.VsIntegration.Shell.Service.IDesignerDebuggingServiceImpl debuggingService = projectManager.GetService(typeof(Microsoft.DataWarehouse.Interfaces.Debugger.IDesignerDebuggingService)) as Microsoft.DataWarehouse.VsIntegration.Shell.Service.IDesignerDebuggingServiceImpl;
                debuggingService.SetDebuggee(package);
                Microsoft.DataWarehouse.Interfaces.IDesignerToolWindowService toolWindowService = projectManager.GetService(typeof(Microsoft.DataWarehouse.Interfaces.IDesignerToolWindowService)) as Microsoft.DataWarehouse.Interfaces.IDesignerToolWindowService;

                typeDebugger.InvokeMember("debuggingService", System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance, null, debugger, new object[] { debuggingService });
                typeDebugger.InvokeMember("toolWindowService", System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance, null, debugger, new object[] { toolWindowService });
                typeDebugger.InvokeMember("vsDebuggerSvc", System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance, null, debugger, new object[] { vsDebuggerSvc });

                vsDebuggerSvc.AdviseDebugEventCallback(debugger);
                string sModifiedPackagePath = @"C:\projects\test ssis\Copy of Package18.dtsx"; //TODO: modify the dtsx file to add custom logging and then write to a temp file
                LaunchVsDebugger(vsDebuggerSvc, (DataTransformationsProjectConfigurationOptions)projectManager.ConfigurationManager.CurrentConfiguration.Options, sModifiedPackagePath, packagePassword);

                System.Diagnostics.Debug.WriteLine("done starting debugging");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + " " + ex.StackTrace);
            }
        }

        //from Microsoft.DataTransformationServices.Project.DataTransformationsPackageDebugger
        private void LaunchVsDebugger(IVsDebugger iVsDebugger, Microsoft.DataTransformationServices.Project.DataTransformationsProjectConfigurationOptions options, string sModifiedPackagePath, string packagePassword)
        {
            VsDebugTargetInfo info = new VsDebugTargetInfo();
            info.cbSize = (uint)Marshal.SizeOf(typeof(VsDebugTargetInfo));
            info.dlo = DEBUG_LAUNCH_OPERATION.DLO_Custom;
            info.bstrMdmRegisteredName = "DTS";
            info.bstrExe = sModifiedPackagePath;
            info.bstrArg = null;
            info.clsidCustom = typeof(Microsoft.DataTransformationServices.Project.DebugEngine.DebugEngine).GUID;
            info.grfLaunch = 1;
            info.dwClsidCount = 1;
            info.fSendStdoutToOutputWindow = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = AllocHGlobalForStructure(typeof(Guid), typeof(Microsoft.DataTransformationServices.Project.DebugEngine.DebugEngine).GUID);
                info.pClsidList = ptr;
                zero = AllocHGlobalForStructure(typeof(VsDebugTargetInfo), info);

                System.Reflection.BindingFlags setstaticfield = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField;
                typeof(DebugEngine).InvokeMember("LastException", setstaticfield, null, null, new object[] { (Exception)null });
                typeof(DebugEngine).InvokeMember("DebugEngineCreated", setstaticfield, null, null, new object[] { false });
                typeof(DebugEngine).InvokeMember("InteractiveMode", setstaticfield, null, null, new object[] { options.InteractiveMode });
                typeof(DebugEngine).InvokeMember("RunInOptimizedMode", setstaticfield, null, null, new object[] { options.RunInOptimizedMode });
                typeof(DebugEngine).InvokeMember("Run64BitRuntime", setstaticfield, null, null, new object[] { options.Run64BitRuntime });
                typeof(DebugEngine).InvokeMember("PackagePassword", setstaticfield, null, null, new object[] { packagePassword });
                int hr = iVsDebugger.LaunchDebugTargets(1, zero);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

            }
            finally
            {
                FreeStructureHGlobal(typeof(Guid), ptr);
                FreeStructureHGlobal(typeof(VsDebugTargetInfo), zero);
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        internal static extern void ZeroMemory(IntPtr handle, IntPtr length);

        private static IntPtr AllocHGlobalForStructure(Type structType, object obj)
        {
            int cb = Marshal.SizeOf(structType);
            IntPtr handle = Marshal.AllocHGlobal(cb);
            ZeroMemory(handle, (IntPtr)cb);
            Marshal.StructureToPtr(obj, handle, false);
            return handle;
        }

        private static void FreeStructureHGlobal(Type structType, IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.DestroyStructure(ptr, structType);
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}