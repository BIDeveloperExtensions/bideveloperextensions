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
using Microsoft.AnalysisServices.Common;
using System.Linq;
using System.Linq.Expressions;

namespace BIDSHelper
{
    public class TabularHideMemberIfPlugin : BIDSHelperWindowActivatedPluginBase, ITabularOnPreBuildAnnotationCheck
    {
        public const string HIDEMEMBERIF_ANNOTATION = "BIDS_Helper_Tabular_HideMemberIf_Backups";

        #region Standard Plugin Overrides
        public TabularHideMemberIfPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "TabularHideMemberIf"; }
        }

        public override int Bitmap
        {
            get { return 144; }
        }

        public override string ButtonText
        {
            get { return "Tabular HideMemberIf..."; }
        }

        public override string FeatureName
        {
            get { return "Tabular HideMemberIf"; }
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
            get { return "Provides a UI for setting the HideMemberIf property of a level in a hierarchy for Tabular models."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            return false;
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

                SandboxEditor editor = TabularHelpers.GetTabularSandboxEditorFromBimFile(hierItem, false);
                if (editor == null) return;
                Microsoft.AnalysisServices.Common.DiagramDisplay diagramDisplay = (Microsoft.AnalysisServices.Common.DiagramDisplay)editor.GetType().InvokeMember("GetCurrentDiagramDisplay", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic, null, editor, new object[] { });
                Microsoft.AnalysisServices.Common.ERDiagram diagram = null;
                if (diagramDisplay != null)
                {
                    diagram = diagramDisplay.Diagram as Microsoft.AnalysisServices.Common.ERDiagram;
                }
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

            foreach (IDiagramAction action in diagram.Actions)
            {
                if (action is ERDiagramActionHideMemberIf) return; //if this context menu is already part of the diagram, then we're done
            }

            IDiagramTag tagHierarchyLevel = (IDiagramTag)diagram.GetType().InvokeMember("tagHierarchyLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, diagram, null);

            ERDiagramActionHideMemberIf levels = new ERDiagramActionHideMemberIf(diagram, this);
            levels.Text = "Set HideMemberIf...";
            levels.DisplayIndex = 0x19f;
            levels.Key = new DiagramObjectKey(@"Actions\{0}", new object[] { "HideMemberIf" });
            levels.AvailableRule = delegate(IEnumerable<IEnumerable<IDiagramTag>> tagSets)
            {
                return tagSets.All<IEnumerable<IDiagramTag>>(tagSet => tagSet.Contains<IDiagramTag>(tagHierarchyLevel));
            };
            diagram.GetType().InvokeMember("InitializeViewStates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod, null, diagram, new object[] { levels });
            diagram.Actions.Add(levels);

            //_hookedERDiagrams.Add(diagram);
        }

        internal HideIfValue GetHideMemberIf(IEnumerable<Tuple<string, string, string>> hierarchyLevels)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
            Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = TabularHelpers.GetTabularSandboxFromBimFile(hierItem, false);
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

        internal void SetHideMemberIf(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox, IEnumerable<Tuple<string, string, string>> hierarchyLevels, List<HideIfValue> newValues)
        {
            try
            {
                Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.AMOCode code = delegate
                    {
                        Microsoft.AnalysisServices.BackEnd.SandboxTransactionProperties properties = new Microsoft.AnalysisServices.BackEnd.SandboxTransactionProperties();
                        properties.RecalcBehavior = Microsoft.AnalysisServices.BackEnd.TransactionRecalcBehavior.AlwaysRecalc;
                        List<Dimension> dims = new List<Dimension>();
                        using (Microsoft.AnalysisServices.BackEnd.SandboxTransaction tran = sandbox.CreateTransaction(properties))
                        {
                            if (!TabularHelpers.EnsureDataSourceCredentials(sandbox))
                            {
                                MessageBox.Show("Cancelling apply of HideMemberIf because data source credentials were not entered.", "BIDS Helper Tabular HideMemberIf - Cancelled!");
                                tran.RollbackAndContinue();
                                return;
                            }

                            SSAS.TabularHideMemberIfAnnotation annotation = GetAnnotation(sandbox);

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

                            tran.Commit();
                        }

                    };
                sandbox.ExecuteAMOCode(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationType.Update, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }

        }

        private SSAS.TabularHideMemberIfAnnotation GetAnnotation(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            if (sandbox.Database.Annotations.Contains(HIDEMEMBERIF_ANNOTATION))
            {
                string xml = sandbox.Database.Annotations[HIDEMEMBERIF_ANNOTATION].Value.OuterXml;
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


        private Level[] GetProblemLevels(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            List<Level> levels = new List<Level>();
            SSAS.TabularHideMemberIfAnnotation annotation = GetAnnotation(sandbox);
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

        internal class ERDiagramActionHideMemberIf : SSAS.Tabular.ERDiagramActionBase, IDiagramActionBasic, IDiagramAction, IDiagramObject, System.ComponentModel.INotifyPropertyChanged, INotifyCollectionPropertyChanged
        {
            private TabularHideMemberIfPlugin _plugin;
            public ERDiagramActionHideMemberIf(ERDiagram diagramInput, TabularHideMemberIfPlugin plugin)
                : base(diagramInput)
            {
                _plugin = plugin;
                this.Icon = DiagramIcon.Hierarchy;
            }

            public override void Cancel(IDiagramActionInstance actionInstance)
            {
            }

            public override IShowMessageRequest Confirm(IDiagramActionInstance actionInstance)
            {
                return null;
            }

            public override void Consider(IDiagramActionInstance actionInstance)
            {
            }

            private Form form;
            public override DiagramActionResult Do(IDiagramActionInstance actionInstance)
            {
                try
                {
                    IEnumerable<Tuple<string, string, string>> hierarchyLevels = this.SortHierarchyLevels(actionInstance.Targets.OfType<IDiagramNode>());

                    UIHierarchy solExplorer = _plugin.ApplicationObject.ToolWindows.SolutionExplorer;
                    UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                    Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = TabularHelpers.GetTabularSandboxFromBimFile(hierItem, false);
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
                return new DiagramActionResult(null, (IDiagramObject)null);
            }

            void okButton_Click(object sender, EventArgs e)
            {
                form.DialogResult = DialogResult.OK;
                form.Close();
            }

            private IEnumerable<Tuple<string, string, string>> SortHierarchyLevels(IEnumerable<IDiagramNode> hierarchyLevelNodes)
            {
                List<Tuple<string, string, string>> list = new List<Tuple<string, string, string>>();
                foreach (IDiagramNode node in hierarchyLevelNodes)
                {
                    list.Add(new Tuple<string, string, string>(DiagramNode.GetTopAncestor(node).Text, node.ParentNode.Text, node.Text));
                }
                return list;
            }

            
        }

    
    }
}
