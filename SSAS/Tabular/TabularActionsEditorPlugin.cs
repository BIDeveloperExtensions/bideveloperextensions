#if !DENALI && !SQL2014
extern alias localAdomdClient;
#endif

using System;
using EnvDTE;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using BIDSHelper.Core;
//using AdomdLocal = localAdomdClient.Microsoft.AnalysisServices.AdomdClient;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASTabular)]
    public class TabularActionsEditorPlugin : BIDSHelperPluginBase, ITabularOnPreBuildAnnotationCheck
    {
        private Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox;
        private Cube cube;

        #region Standard Plugin Overrides
        public TabularActionsEditorPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.TabularActionsEditorId);
        }

        public override string ShortName
        {
            get { return "TabularActionsEditor"; }
        }

        //public override int Bitmap
        //{
        //    get { return 144; }
        //}

        public override string FeatureName
        {
            get { return "Tabular Actions Editor"; }
        }
        
        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        
        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASTabular; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Provides a UI for editing actions such as drillthrough actions or report actions in Tabular models."; }
        }

        #endregion

        public override bool ShouldDisplayCommand()
        {
            var selectedFile = GetSelectedFile();
            if (selectedFile != null && selectedFile.Extension == ".bim")
            {
#if !DENALI && !SQL2014
                var sb = TabularHelpers.GetTabularSandboxFromBimFile(this, false);
                if (sb == null) return false;
                return !sb.IsTabularMetadata;
#else
                return true;
#endif
            }
            return false; 
        }

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));

                sandbox = TabularHelpers.GetTabularSandboxFromBimFile(this, true);
                ExecSandbox(sandbox);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }


        private void ExecSandbox(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandboxParam)
        {
            try
            {


#if DENALI || SQL2014
                var sb = sandboxParam;
                var conn = sandboxParam.AdomdConnection;
#elif SQL2017
                var sb = (Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)sandboxParam.Impl;
                var localConn = sandboxParam.AdomdConnection;
                var conn = new localAdomdClient.Microsoft.AnalysisServices.AdomdClient.AdomdConnection(localConn.ConnectionString);
#else
                var sb = (Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)sandboxParam.Impl;
                var localConn = sandboxParam.AdomdConnection;
                var conn = new Microsoft.AnalysisServices.AdomdClient.AdomdConnection(localConn.ConnectionString);
#endif

                if (sb == null) throw new Exception("Can't get Sandbox!");
                cube = sb.Cube;
                if (cube == null) throw new Exception("The workspace database cube doesn't exist.");


                SSAS.TabularActionsEditorForm form = new SSAS.TabularActionsEditorForm(cube, conn);
                if (form.ShowDialog() == DialogResult.OK)
                {
#if DENALI || SQL2014
                    Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.AMOCode code;
#else
                    Microsoft.AnalysisServices.BackEnd.AMOCode code;
#endif
                    code = delegate
                        {
                            using (Microsoft.AnalysisServices.BackEnd.SandboxTransaction tran = sandbox.CreateTransaction())
                            {
                                foreach (Perspective p in cube.Perspectives)
                                {
                                    p.Actions.Clear();
                                }
                                cube.Actions.Clear();
                                foreach (Microsoft.AnalysisServices.Action action in form.Actions())
                                {
                                    cube.Actions.Add(action);
                                    foreach (Perspective p in cube.Perspectives)
                                    {
                                        if (form.ActionInPerspective(action.ID, p.ID))
                                        {
                                            p.Actions.Add(action.ID);
                                        }
                                    }
                                }

                                TabularHelpers.SaveXmlAnnotation(cube, SSAS.TabularActionsEditorForm.ACTION_ANNOTATION, form.Annotation);

                                cube.Update(UpdateOptions.ExpandFull);
                                tran.GetType().InvokeMember("Commit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, tran, null); //The .Commit() function used to return a list of strings, but in the latest set of code it is a void method which leads to "method not found" errors
                                //tran.Commit();
                            }
                        };
#if DENALI || SQL2014
                    sandbox.ExecuteAMOCode(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationType.Update, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
#else
                    sandboxParam.ExecuteEngineCode(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationType.Update, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
#endif
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        public static SSAS.TabularActionsAnnotation GetAnnotation(Cube cube)
        {
            SSAS.TabularActionsAnnotation annotation = new SSAS.TabularActionsAnnotation();
            if (cube.Annotations.Contains(SSAS.TabularActionsEditorForm.ACTION_ANNOTATION))
            {
                string xml = TabularHelpers.GetAnnotationXml(cube, SSAS.TabularActionsEditorForm.ACTION_ANNOTATION);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SSAS.TabularActionsAnnotation));
                annotation = (SSAS.TabularActionsAnnotation)serializer.Deserialize(new System.IO.StringReader(xml));
            }
            return annotation;
        }

#region ITabularOnPreBuildAnnotationCheck
        public TabularOnPreBuildAnnotationCheckPriority TabularOnPreBuildAnnotationCheckPriority
        {
            get
            {
                return TabularOnPreBuildAnnotationCheckPriority.RegularPriority;
            }
        }

        public string GetPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
#if DENALI || SQL2014
            cube = sandbox.Cube;
#else
            cube = ((Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)sandbox.Impl).Cube;
#endif
            SSAS.TabularActionsAnnotation annotation = GetAnnotation(cube);

            bool bContainsPerspectiveListAnnotation = false;
            foreach (Microsoft.AnalysisServices.Action action in cube.Actions)
            {
                SSAS.TabularAction actionAnnotation = annotation.Find(action.ID);
                if (actionAnnotation == null) continue;

                //see if this action is assigned to perspectives
                if (actionAnnotation.Perspectives != null && actionAnnotation.Perspectives.Length > 0)
                {
                    bContainsPerspectiveListAnnotation = true;
                }
            }

            long lngPerspectiveActionsCount = 0;
            foreach (Perspective p in cube.Perspectives)
            {
                lngPerspectiveActionsCount += p.Actions.Count;
            }

            //note: this logic is also duplicated in the constructor of TabularActionsEditorForm since we will just rely on it to fix the actions
            if (bContainsPerspectiveListAnnotation && lngPerspectiveActionsCount == 0 && cube.Perspectives.Count > 0)
                return "Click OK for BIDS Helper to restore the assignments of actions to perspectives. The Tabular Actions Editor form will open. Then click Yes then OK.";
            else
                return null;
        }

        public void FixPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            //open the actions form and let it fix actions
            ExecSandbox(sandbox);
        }
#endregion
        
        public class DrillthroughColumn
        {
            public DrillthroughColumn()
            {
            }

            private string _cubeDimension;
            public string CubeDimension
            {
                get { return _cubeDimension; }
                set { _cubeDimension = value; }
            }

            private string _attribute;
            public string Attribute
            {
                get { return _attribute; }
                set { _attribute = value; }
            }
        }
    }
}
