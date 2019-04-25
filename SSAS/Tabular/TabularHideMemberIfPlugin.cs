using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.Common;
using System.Linq;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASTabular)]
    public class TabularHideMemberIfPlugin : BIDSHelperWindowActivatedPluginBase, ITabularOnPreBuildAnnotationCheck
    {
        public const string HIDEMEMBERIF_ANNOTATION = "BIDS_Helper_Tabular_HideMemberIf_Backups";

        #region Standard Plugin Overrides
        public TabularHideMemberIfPlugin(BIDSHelperPackage package)
            : base(package)
        {
        }

        public override string ShortName
        {
            get { return "TabularHideMemberIf"; }
        }

        //public override int Bitmap
        //{
        //    get { return 144; }
        //}

        public override string FeatureName
        {
            get { return "Tabular HideMemberIf"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
        //}

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
            get { return "Provides a UI for setting the HideMemberIf property of a level in a hierarchy for Tabular models."; }
        }

        public override bool ShouldHookWindowCreated
        {
            get
            {
                return true;
            }
        }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {

            try
            {
                HookBimWindow(GotFocus.ProjectItem);
            }
            catch { }
        }
        #endregion

        private List<SandboxEditor> _hookedSandboxEditors = new List<SandboxEditor>();
        private void HookBimWindow(ProjectItem pi)
        {
            try
            {
                if (pi == null || pi.Name == null) return;
                string sFileName = pi.Name.ToLower();
                if (!sFileName.EndsWith(".bim")) return;

                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));

                SandboxEditor editor = TabularHelpers.GetTabularSandboxEditorFromProjectItem(pi, false);
                if (editor == null) return;
#if !DENALI && !SQL2014
                // if the sandbox is in the new tabular metadata mode (JSON) then exit here
                if (editor.Sandbox.IsTabularMetadata) return;
#endif
                Microsoft.AnalysisServices.Common.ERDiagram diagram = TabularHelpers.GetTabularERDiagramFromSandboxEditor(editor);
                if (diagram != null)
                {
                    SetupContextMenu(diagram);
                }
                else
                {
                    if (!_hookedSandboxEditors.Contains(editor))
                    {
                        editor.DiagramObjectsSelected += new EventHandler<ERDiagramSelectionChangedEventArgs>(editor_DiagramObjectsSelected);
                        _hookedSandboxEditors.Add(editor);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        void editor_DiagramObjectsSelected(object sender, ERDiagramSelectionChangedEventArgs e)
        {
            try
            {
                SetupContextMenu(sender as ERDiagram);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        //private List<ERDiagram> _hookedERDiagrams = new List<ERDiagram>();
        private void SetupContextMenu(ERDiagram diagram)
        {
            if (diagram == null) return;
            //if (_hookedERDiagrams.Contains(diagram)) return;
            
#if DENALI || SQL2014
            IDiagramAction actionHideMemberIf = null;
            foreach (IDiagramAction action in diagram.Actions)
#else
            IViewModelAction actionHideMemberIf = null;
            foreach (IViewModelAction action in diagram.Actions)
#endif
            {
                if (action is ERDiagramActionHideMemberIf) { actionHideMemberIf = action;  break; } 
            }
            if (actionHideMemberIf != null) return; //if this context menu is already part of the diagram, then we're done


            ERDiagramActionHideMemberIf levels = new ERDiagramActionHideMemberIf(diagram, this);
            levels.Text = "Set HideMemberIf...";
            levels.DisplayIndex = 0x19f;
#if DENALI || SQL2014
            IDiagramTag tagHierarchyLevel = (IDiagramTag)diagram.GetType().InvokeMember("tagHierarchyLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, diagram, null);
            levels.Key = new DiagramObjectKey(@"Actions\{0}", new object[] { "HideMemberIf" });
            levels.AvailableRule = delegate(IEnumerable<IEnumerable<IDiagramTag>> tagSets)
            {
                return tagSets.All<IEnumerable<IDiagramTag>>(tagSet => tagSet.Contains<IDiagramTag>(tagHierarchyLevel));
            };
#else
            IViewModelTag tagHierarchyLevel = (IViewModelTag)diagram.GetType().InvokeMember("tagHierarchyLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, diagram, null);
            levels.Key = new ViewModelObjectKey(@"Actions\{0}", new object[] { "HideMemberIf" });
            levels.AvailableRule = delegate (IEnumerable<IEnumerable<IViewModelTag>> tagSets)
            {
                return tagSets.All<IEnumerable<IViewModelTag>>(tagSet => tagSet.Contains<IViewModelTag>(tagHierarchyLevel));
            };
#endif
            diagram.GetType().InvokeMember("InitializeViewStates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod, null, diagram, new object[] { levels });
            diagram.Actions.Add(levels);

            //_hookedERDiagrams.Add(diagram);
        }

        internal HideIfValue GetHideMemberIf(IEnumerable<Tuple<string, string, string>> hierarchyLevels)
        {
#if DENALI || SQL2014
            var sandbox = TabularHelpers.GetTabularSandboxFromActiveWindow(this.package);
#else
            var sb = TabularHelpers.GetTabularSandboxFromActiveWindow(this.package);
            var sandbox = (Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)sb.Impl;
#endif

            if (sandbox != null)
            {
                foreach (Tuple<string, string, string> tuple in hierarchyLevels)
                {
                    Dimension d = sandbox.Database.Dimensions.GetByName(tuple.Item1);
                    Hierarchy h = d.Hierarchies.GetByName(tuple.Item2);
                    Level l = h.Levels.GetByName(tuple.Item3);
                    return l.HideMemberIf;
                }
            }
            throw new Exception("Couldn't find HideMemberIf value.");
        }

        internal void SetHideMemberIf(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandboxParam, IEnumerable<Tuple<string, string, string>> hierarchyLevels, List<HideIfValue> newValues)
        {
            try
            {
#if DENALI || SQL2014
                Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.AMOCode code;
                var sandbox = sandboxParam;
#else
                Microsoft.AnalysisServices.BackEnd.AMOCode code;
                var sandbox = (Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)sandboxParam.Impl;
#endif
                code = delegate
                    {
                        Microsoft.AnalysisServices.BackEnd.SandboxTransactionProperties properties = new Microsoft.AnalysisServices.BackEnd.SandboxTransactionProperties();
                        properties.RecalcBehavior = Microsoft.AnalysisServices.BackEnd.TransactionRecalcBehavior.AlwaysRecalc;
                        List<Dimension> dims = new List<Dimension>();
                        using (Microsoft.AnalysisServices.BackEnd.SandboxTransaction tran = sandboxParam.CreateTransaction(properties))
                        {
                            if (!TabularHelpers.EnsureDataSourceCredentials(sandboxParam))
                            {
                                MessageBox.Show("Cancelling apply of HideMemberIf because data source credentials were not entered.", "BIDS Helper Tabular HideMemberIf - Cancelled!");
                                tran.RollbackAndContinue();
                                return;
                            }

                            SSAS.TabularHideMemberIfAnnotation annotation = GetAnnotation(sandboxParam);

                            foreach (Tuple<string, string, string> tuple in hierarchyLevels)
                            {
                                Dimension d = sandbox.Database.Dimensions.GetByName(tuple.Item1);
                                if (!dims.Contains(d)) dims.Add(d);
                                Hierarchy h = d.Hierarchies.GetByName(tuple.Item2);
                                Level l = h.Levels.GetByName(tuple.Item3);
                                l.HideMemberIf = newValues[0];
                                newValues.RemoveAt(0);

                                annotation.Set(l);
                            }

                            TabularHelpers.SaveXmlAnnotation(sandbox.Database, HIDEMEMBERIF_ANNOTATION, annotation);

                            sandbox.Database.Update(UpdateOptions.ExpandFull);

                            //bug in AS2012 (still not working in RTM CU1) requires ProcessFull to successfully switch from HideMemberIf=Never to NoName
                            foreach (Dimension d in dims)
                            {
                                d.Process(ProcessType.ProcessFull);
                            }

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
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }

        }

        private SSAS.TabularHideMemberIfAnnotation GetAnnotation(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandboxParam)
        {
#if DENALI || SQL2014
            var sandbox = sandboxParam;
#else
            var sandbox = (Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)sandboxParam.Impl;
#endif
            if (sandbox.Database.Annotations.Contains(HIDEMEMBERIF_ANNOTATION))
            {
                string xml = TabularHelpers.GetAnnotationXml(sandbox.Database, HIDEMEMBERIF_ANNOTATION);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(BIDSHelper.SSAS.TabularHideMemberIfAnnotation));
                return (SSAS.TabularHideMemberIfAnnotation)serializer.Deserialize(new System.IO.StringReader(xml));
            }
            else
            {
                return new SSAS.TabularHideMemberIfAnnotation();
            }
        }

        public override void Exec()
        {
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
            string sLevelsWithProblems = string.Empty;
            foreach (Level l in GetProblemLevels(sandbox))
            {
                if (sLevelsWithProblems.Length > 0) sLevelsWithProblems += ", ";
                sLevelsWithProblems += "[" + l.ParentDimension.Name + "].[" + l.Parent.Name + "].[" + l.Name + "]";
            }
            if (sLevelsWithProblems.Length == 0)
                return null;
            else
                return "Click OK for BIDS Helper to restore the HideMemberIf settings on the following levels: " + sLevelsWithProblems;
        }

        public void FixPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            SSAS.TabularHideMemberIfAnnotation annotation = GetAnnotation(sandbox);
            List<Tuple<string, string, string>> levels = new List<Tuple<string, string, string>>();
            List<HideIfValue> values = new List<HideIfValue>();
            foreach (Level l in GetProblemLevels(sandbox))
            {
                SSAS.TabularLevelHideMemberIf levelAnnotation = annotation.Find(l);
                levels.Add(new Tuple<string, string, string>(l.ParentDimension.Name, l.Parent.Name, l.Name));
                values.Add(levelAnnotation.HideMemberIf);
            }
            SetHideMemberIf(sandbox, levels, values);
        }


        private Level[] GetProblemLevels(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandboxParam)
        {
#if DENALI || SQL2014
            var sandbox = sandboxParam;
#else
            var sandbox = (Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)sandboxParam.Impl;
#endif
            List<Level> levels = new List<Level>();
            SSAS.TabularHideMemberIfAnnotation annotation = GetAnnotation(sandboxParam);
            foreach (Dimension d in sandbox.Database.Dimensions)
            {
                foreach (Hierarchy h in d.Hierarchies)
                {
                    foreach (Level l in h.Levels)
                    {
                        SSAS.TabularLevelHideMemberIf levelAnnotation = annotation.Find(l);
                        if (levelAnnotation != null)
                        {
                            if (levelAnnotation.HideMemberIf != l.HideMemberIf)
                            {
                                levels.Add(l);
                            }
                        }
                    }
                }
            }
            return levels.ToArray();
        }
#endregion

#if DENALI || SQL2014
        internal class ERDiagramActionHideMemberIf : SSAS.Tabular.ERDiagramActionBase, IDiagramActionBasic, IDiagramAction, IDiagramObject, System.ComponentModel.INotifyPropertyChanged, INotifyCollectionPropertyChanged
#else
        internal class ERDiagramActionHideMemberIf : SSAS.Tabular.ERDiagramActionBase, IViewModelActionBasic, IViewModelAction, IViewModelObject, System.ComponentModel.INotifyPropertyChanged, INotifyCollectionPropertyChanged
#endif
        {
            private TabularHideMemberIfPlugin _plugin;
            public ERDiagramActionHideMemberIf(ERDiagram diagramInput, TabularHideMemberIfPlugin plugin)
                : base(diagramInput)
            {
                _plugin = plugin;
                this.Icon = DiagramIcon.Hierarchy;
            }
#if DENALI || SQL2014
            public override void Cancel(IDiagramActionInstance actionInstance) { }

            public override IShowMessageRequest Confirm(IDiagramActionInstance actionInstance) { return null; }

            public override void Consider(IDiagramActionInstance actionInstance) { }
#else
            public override void Cancel(IViewModelActionInstance actionInstance) { }

            public override IShowMessageRequest Confirm(IViewModelActionInstance actionInstance) { return null; }

            public override void Consider(IViewModelActionInstance actionInstance) { }
#endif
            private Form form;

#if DENALI || SQL2014
            public override DiagramActionResult Do(IDiagramActionInstance actionInstance)
#else
            public override ViewModelActionResult Do(IViewModelActionInstance actionInstance)
#endif
            {
                try
                {
#if DENALI || SQL2014
                    IEnumerable<Tuple<string, string, string>> hierarchyLevels = this.SortHierarchyLevels(actionInstance.Targets.OfType<IDiagramNode>());
#else
                    IEnumerable<Tuple<string, string, string>> hierarchyLevels = this.SortHierarchyLevels(actionInstance.Targets.OfType<IViewModelNode>());
#endif

                    Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = TabularHelpers.GetTabularSandboxFromActiveWindow(_plugin.package);
                    if (sandbox == null) throw new Exception("Can't get Sandbox!");

                    string sWarning = _plugin.GetPreBuildWarning(sandbox);
                    if (sWarning != null)
                    {
                        if (MessageBox.Show(sWarning, "BIDS Helper Tabular HideMemberIf", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            _plugin.FixPreBuildWarning(sandbox);
                        }
                    }


                    Microsoft.AnalysisServices.HideIfValue currentValue = _plugin.GetHideMemberIf(hierarchyLevels);

                    form = new Form();
                    form.Icon = BIDSHelper.Resources.Common.BIDSHelper;
                    form.Text = "BIDS Helper Tabular HideMemberIf Editor";
                    form.MaximizeBox = true;
                    form.MinimizeBox = false;
                    form.Width = 400;
                    form.Height = 150;
                    form.SizeGripStyle = SizeGripStyle.Hide;
                    form.MinimumSize = new System.Drawing.Size(form.Width, form.Height);

                    Label labelAnnotation = new Label();
                    labelAnnotation.Text = "HideMemberIf:";
                    labelAnnotation.Top = 25;
                    labelAnnotation.Left = 5;
                    labelAnnotation.Width = 80;
                    labelAnnotation.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    labelAnnotation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                    form.Controls.Add(labelAnnotation);

                    ComboBox combo = new ComboBox();
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    combo.Width = form.Width - 40 - labelAnnotation.Width;
                    combo.Left = labelAnnotation.Right + 5;
                    combo.Top = 25;
                    combo.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    form.Controls.Add(combo);
                    combo.Items.AddRange(Enum.GetNames(typeof(HideIfValue)));
                    combo.SelectedIndex = combo.Items.IndexOf(currentValue.ToString());

                    labelAnnotation = new Label();
                    labelAnnotation.Text = "BIDS Helper will ProcessFull this table when you click OK.";
                    labelAnnotation.Top = combo.Bottom + 10;
                    labelAnnotation.Left = 5;
                    labelAnnotation.Width = form.Width;
                    labelAnnotation.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    labelAnnotation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    form.Controls.Add(labelAnnotation);

                    Button okButton = new Button();
                    okButton.Text = "OK";
                    okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    okButton.Left = form.Right - okButton.Width * 2 - 40;
                    okButton.Top = form.Bottom - okButton.Height * 2 - 20;
                    okButton.Click += new EventHandler(okButton_Click);
                    form.Controls.Add(okButton);
                    form.AcceptButton = okButton;

                    Button cancelButton = new Button();
                    cancelButton.Text = "Cancel";
                    cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    cancelButton.Left = okButton.Right + 10;
                    cancelButton.Top = okButton.Top;
                    form.CancelButton = cancelButton;
                    form.Controls.Add(cancelButton);

                    DialogResult result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        //build a list of HideIfValue enums the same length as the list of levels
                        HideIfValue val = (HideIfValue)Enum.Parse(typeof(HideIfValue), combo.SelectedItem.ToString());
                        List<HideIfValue> vals = new List<HideIfValue>();
                        foreach (Tuple<string, string, string> level in hierarchyLevels)
                        {
                            vals.Add(val);
                        }

                        //set the value
                        _plugin.SetHideMemberIf(sandbox, hierarchyLevels, vals);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
                }
#if DENALI || SQL2014
                return new DiagramActionResult(null, (IDiagramObject)null);
#else
                return new ViewModelActionResult(null, (IViewModelObject)null);
#endif

            }

            void okButton_Click(object sender, EventArgs e)
            {
                form.DialogResult = DialogResult.OK;
                form.Close();
            }

#if DENALI || SQL2014
            private IEnumerable<Tuple<string, string, string>> SortHierarchyLevels(IEnumerable<IDiagramNode> hierarchyLevelNodes)
            {
                List<Tuple<string, string, string>> list = new List<Tuple<string, string, string>>();
                foreach (IDiagramNode node in hierarchyLevelNodes)
                {
                    list.Add(new Tuple<string, string, string>(DiagramNode.GetTopAncestor(node).Text, node.ParentNode.Text, node.Text));
                }
                return list;
            }
#else
            private IEnumerable<Tuple<string, string, string>> SortHierarchyLevels(IEnumerable<IViewModelNode> hierarchyLevelNodes)
            {
                List<Tuple<string, string, string>> list = new List<Tuple<string, string, string>>();
                foreach (IViewModelNode node in hierarchyLevelNodes)
                {
                    list.Add(new Tuple<string, string, string>(ViewModelNode.GetTopAncestor(node).Text, node.ParentNode.Text, node.Text));
                }
                return list;
            }
#endif

        }

    
    }
}
