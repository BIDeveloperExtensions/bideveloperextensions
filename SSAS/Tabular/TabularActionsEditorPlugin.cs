using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;

namespace BIDSHelper
{
    public class TabularActionsEditorPlugin : BIDSHelperPluginBase, ITabularOnPreBuildAnnotationCheck
    {
        private Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox;
        private Cube cube;

        #region Standard Plugin Overrides
        public TabularActionsEditorPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "TabularActionsEditor"; }
        }

        public override int Bitmap
        {
            get { return 144; }
        }

        public override string ButtonText
        {
            get { return "Tabular Actions Editor..."; }
        }

        public override string FeatureName
        {
            get { return "Tabular Actions Editor"; }
        }

        public override string MenuName
        {
            get { return "Item"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        public override bool ShouldPositionAtEnd
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSAS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Provides a UI for editing actions such as drillthrough actions or report actions in Tabular models."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                if (!(hierItem.Object is ProjectItem)) return false;
                string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                return (sFileName.EndsWith(".bim"));
            }
            catch
            {
            }
            return false;
        }
        #endregion

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));

                sandbox = TabularHelpers.GetTabularSandboxFromBimFile(hierItem, true);
                if (sandbox == null) throw new Exception("Can't get Sandbox!");
                cube = sandbox.Cube;

                SSAS.TabularActionsEditorForm form = new SSAS.TabularActionsEditorForm(cube, sandbox.AdomdConnection);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.AMOCode code = delegate
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
                                tran.Commit();
                            }
                        };
                    sandbox.ExecuteAMOCode(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationType.Update, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
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
                string xml = cube.Annotations[SSAS.TabularActionsEditorForm.ACTION_ANNOTATION].Value.OuterXml;
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SSAS.TabularActionsAnnotation));
                annotation = (SSAS.TabularActionsAnnotation)serializer.Deserialize(new System.IO.StringReader(xml));
            }
            return annotation;
        }

        #region ITabularOnPreBuildAnnotationCheck
        public string GetPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            cube = sandbox.Cube;
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
            Exec();
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
