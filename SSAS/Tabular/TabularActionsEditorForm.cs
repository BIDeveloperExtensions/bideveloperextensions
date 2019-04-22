#if SQL2017
extern alias localAdomdClient;
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;

#if SQL2017
using localAdomdClient.Microsoft.AnalysisServices.AdomdClient;
#else
using Microsoft.AnalysisServices.AdomdClient;
#endif


namespace BIDSHelper.SSAS
{
    public partial class TabularActionsEditorForm : Form
    {
        private Microsoft.AnalysisServices.Cube cube;
        private AdomdConnection conn;
        private List<TabularActionsEditorPlugin.DrillthroughColumn> _listDrillthroughColumns;
        private List<ReportParameter> _listReportParameters;
        private List<Microsoft.AnalysisServices.Action> _listActionClones;
        private Dictionary<string, string[]> _dictActionPerspectives = new Dictionary<string, string[]>();
        private Microsoft.AnalysisServices.Action _currentAction;
        private bool _skipEvents = false;
        public const string ACTION_ANNOTATION = "BIDS_Helper_Tabular_Actions_Backups";
        private TabularActionsAnnotation annotation;
        private Control[] arrEnabledControls;

        public Microsoft.AnalysisServices.Action[] Actions()
        {
            annotation = new TabularActionsAnnotation();

            List<Microsoft.AnalysisServices.Action> list = new List<Microsoft.AnalysisServices.Action>();
            List<TabularAction> listAnnotationActions = new List<TabularAction>();
            foreach (Microsoft.AnalysisServices.Action action in _listActionClones)
            {
                bool bAlreadyAdded = false;
                var daction = (Microsoft.AnalysisServices.Action)action;
                MeasureGroup mg = null;
                foreach (MeasureGroup mg2 in cube.MeasureGroups)
                {
                    if (daction.Target == "MeasureGroupMeasures(\"" + mg2.Name + "\")")
                    {
                        mg = mg2;
                        break;
                    }
                }
                if (mg != null) //if this is a drillthrough action targeting a whole measure group, MeasureGroupMeasures doesn't actually return the calculated measures in a measure group, so we have to take this one drillthrough action and clone it for each measure under the covers
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    AdomdRestrictionCollection restrictions = new AdomdRestrictionCollection();
                    restrictions.Add(new AdomdRestriction("CUBE_NAME", cube.Name));
                    restrictions.Add(new AdomdRestriction("MEASUREGROUP_NAME", mg.Name));
                    DataSet dataset = conn.GetSchemaDataSet("MDSCHEMA_MEASURES", restrictions);
                    int i = 0;
                    foreach (DataRow r in dataset.Tables[0].Rows)
                    {
                        TabularAction actionAnnotation = new TabularAction();

                        var newaction = (Microsoft.AnalysisServices.Action)daction.Clone();
                        string sSuffix = " " + (i++);
                        newaction.Target = Convert.ToString(r["MEASURE_UNIQUE_NAME"]);

                        if (i == 1)
                        {
                            actionAnnotation.IsMasterClone = true;
                        }
                        else
                        {
                            newaction.ID = daction.ID + sSuffix;
                            newaction.Name = daction.Name + sSuffix;
                            _dictActionPerspectives.Add(newaction.ID, _dictActionPerspectives[action.ID]);
                        }

                        actionAnnotation.ID = newaction.ID;
                        actionAnnotation.Perspectives = _dictActionPerspectives[action.ID];
                        actionAnnotation.OriginalTarget = daction.Target;

                        listAnnotationActions.Add(actionAnnotation);
                        list.Add(newaction);
                        bAlreadyAdded = true;
                    }
                }

                if (!bAlreadyAdded)
                {
                    list.Add(action);

                    TabularAction actionAnnotation = new TabularAction();
                    actionAnnotation.ID = action.ID;
                    actionAnnotation.Perspectives = _dictActionPerspectives[action.ID];
                    listAnnotationActions.Add(actionAnnotation);
                }
            }
            annotation.TabularActions = listAnnotationActions.ToArray();
            return list.ToArray();
        }

        public bool ActionInPerspective(string ActionID, string PerspectiveID)
        {
            if (_dictActionPerspectives.ContainsKey(ActionID))
            {
                foreach (string sPerspectiveID in _dictActionPerspectives[ActionID])
                {
                    if (sPerspectiveID == PerspectiveID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public TabularActionsAnnotation Annotation
        {
            get { return annotation; }
        }

        public TabularActionsEditorForm(Microsoft.AnalysisServices.Cube cube, AdomdConnection conn)
        {
            InitializeComponent();
            this.MinimumSize = this.Size;
            this.Icon = BIDSHelper.Resources.Common.BIDSHelper;
            this.cube = cube;
            this.conn = conn;
            arrEnabledControls = new Control[] { this.btnDelete, this.btnAdd, this.cmbAction, this.okButton, this.cancelButton, this.linkHelp };

            _listDrillthroughColumns = new List<TabularActionsEditorPlugin.DrillthroughColumn>();
            this.drillthroughColumnBindingSource.DataSource = _listDrillthroughColumns;

            _listReportParameters = new List<ReportParameter>();
            this.reportParameterBindingSource.DataSource = _listReportParameters;

            List<string> list = new List<string>();
            List<string> listAttributes = new List<string>();
            list.Add(string.Empty);
            listAttributes.Add(string.Empty);
            foreach (Microsoft.AnalysisServices.CubeDimension cd in cube.Dimensions)
            {
                list.Add(cd.Name);
                foreach (DimensionAttribute a in cd.Dimension.Attributes)
                {
                    listAttributes.Add(a.Name);
                }
            }
            DrillthroughDataGridCubeDimension.Items.AddRange(list.ToArray());
            DrillthroughDataGridAttribute.Items.AddRange(listAttributes.ToArray());

            long lngPerspectiveActionsCount = 0;
            listPerspectives.Items.Clear();
            foreach (Perspective p in cube.Perspectives)
            {
                listPerspectives.Items.Add(p.Name);
                lngPerspectiveActionsCount += p.Actions.Count;
            }

            this.cmbActionType.Items.AddRange(Enum.GetNames(typeof(ActionType)));

            this.cmbTargetType.Items.AddRange(Enum.GetNames(typeof(ActionTargetType)));
            this.cmbTargetType.Items.Remove(ActionTargetType.Set.ToString());

            this.cmbInvocation.Items.AddRange(Enum.GetNames(typeof(ActionInvocation)));

            TabularActionsAnnotation annotation = BIDSHelper.TabularActionsEditorPlugin.GetAnnotation(cube);

            bool bContainsPerspectiveListAnnotation = false;
            _listActionClones = new List<Microsoft.AnalysisServices.Action>();
            foreach (Microsoft.AnalysisServices.Action action in cube.Actions)
            {
                TabularAction actionAnnotation = annotation.Find(action.ID);
                if (actionAnnotation == null) actionAnnotation = new TabularAction();

                if (!string.IsNullOrEmpty(actionAnnotation.OriginalTarget)
                && !actionAnnotation.IsMasterClone)
                {
                    continue;
                }

                Microsoft.AnalysisServices.Action clone = action.Clone();
                _listActionClones.Add(clone);
                if (!string.IsNullOrEmpty(actionAnnotation.OriginalTarget))
                {
                    clone.Target = actionAnnotation.OriginalTarget;
                }

                List<string> lPerspectives = new List<string>();
                foreach (Perspective perspective in cube.Perspectives)
                {
                    if (perspective.Actions.Contains(action.ID))
                    {
                        lPerspectives.Add(perspective.Name);
                    }
                }
                if (!_dictActionPerspectives.ContainsKey(action.ID))
                    _dictActionPerspectives.Add(action.ID, lPerspectives.ToArray());
                else
                    _dictActionPerspectives[action.ID] = lPerspectives.ToArray();

                //see if this action is assigned to perspectives
                if (actionAnnotation.Perspectives != null && actionAnnotation.Perspectives.Length > 0)
                {
                    bContainsPerspectiveListAnnotation = true;
                }
            }

            if (bContainsPerspectiveListAnnotation && lngPerspectiveActionsCount == 0 && cube.Perspectives.Count > 0)
            {
                //we have a backup of the perspectives list, and no actions are assigned to perspectives currently
                if (MessageBox.Show("No actions are currently included in any perspectives, but BIDS Helper did retain a backup of the perspective assignments from the last actions editing session. Changes made to perspectives may have caused action assignments to be lost. Restoring action perspective assignments may be possible except when a perspective has been renamed.\r\n\r\nWould you like BIDS Helper to attempt restore the action perspective assignments now?", "BIDS Helper Tabular Actions Editor", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    foreach (Microsoft.AnalysisServices.Action action in _listActionClones)
                    {
                        TabularAction actionAnnotation = annotation.Find(action.ID);
                        if (actionAnnotation == null) actionAnnotation = new TabularAction();

                        if (actionAnnotation.Perspectives != null && actionAnnotation.Perspectives.Length > 0)
                        {
                            List<string> lPerspectives = new List<string>();
                            foreach (string sPerspectiveID in actionAnnotation.Perspectives)
                            {
                                if (cube.Perspectives.Contains(sPerspectiveID))
                                {
                                    lPerspectives.Add(cube.Perspectives[sPerspectiveID].Name);
                                }
                            }
                            _dictActionPerspectives[action.ID] = lPerspectives.ToArray();
                        }
                    }
                }
            }

            cmbAction.Items.AddRange(_listActionClones.ToArray());
            cmbAction.DisplayMember = "Name";
            cmbAction.ValueMember = "ID";
            cmbAction.ResumeLayout();
            if (_listActionClones.Count > 0)
                cmbAction.SelectedIndex = 0;
            else
                DisableControls(true, arrEnabledControls);

        }

        private void DisableControls(bool bDisable, Control[] arrExceptControls)
        {
            cmbActionType.SelectedItem = ActionType.Url.ToString();
            List<Control> listExceptControls = new List<Control>(arrExceptControls);
            foreach (Control c in this.Controls)
            {
                if (!listExceptControls.Contains(c))
                    c.Enabled = !bDisable;
            }
        }

        private void TabularActionsEditorForm_Load(object sender, EventArgs e)
        {
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (this.dataGridViewDrillthroughColumns.CurrentCell.ColumnIndex == this.DrillthroughDataGridAttribute.Index)
            {
                //limit to just columns in this table and columns that haven't been used already
                BindingSource bindingSource = this.dataGridViewDrillthroughColumns.DataSource as BindingSource;
                TabularActionsEditorPlugin.DrillthroughColumn item = bindingSource.Current as TabularActionsEditorPlugin.DrillthroughColumn;
                DataGridViewComboBoxEditingControl comboBox = e.Control as DataGridViewComboBoxEditingControl;
                List<string> list = new List<string>();
                Microsoft.AnalysisServices.CubeDimension cd = cube.Dimensions.FindByName(item.CubeDimension);
                if (cd == null)
                {
                    comboBox.DataSource = new string[] { };
                }
                else
                {
                    list.Add(string.Empty);
                    foreach (Microsoft.AnalysisServices.DimensionAttribute a in cd.Dimension.Attributes)
                    {
                        if (a.Type != AttributeType.RowNumber) //RowNumber was not supposed to return in drillthrough commands but was at RTM due to a bug
                        {
                            list.Add(a.Name);
                        }
                    }

                    foreach (TabularActionsEditorPlugin.DrillthroughColumn otherItem in _listDrillthroughColumns)
                    {
                        if (otherItem != item && !string.IsNullOrEmpty(otherItem.Attribute))
                        {
                            if (otherItem.CubeDimension == item.CubeDimension)
                            {
                                if (list.Contains(otherItem.Attribute))
                                    list.Remove(otherItem.Attribute);
                            }
                        }
                    }

                    comboBox.Items.Clear();
                    comboBox.Items.AddRange(list.ToArray());

                    if (!string.IsNullOrEmpty(item.Attribute))
                        comboBox.SelectedIndex = list.IndexOf(item.Attribute);
                }
            }
            else if (this.dataGridViewDrillthroughColumns.CurrentCell.ColumnIndex == this.DrillthroughDataGridCubeDimension.Index)
            {
                MeasureGroup mg = null;
                foreach (MeasureGroup mg2 in cube.MeasureGroups)
                {
                    if (Convert.ToString(cmbTarget.SelectedItem) == "MeasureGroupMeasures(\"" + mg2.Name + "\")")
                    {
                        mg = mg2;
                        break;
                    }
                }

                BindingSource bindingSource = this.dataGridViewDrillthroughColumns.DataSource as BindingSource;
                TabularActionsEditorPlugin.DrillthroughColumn item = bindingSource.Current as TabularActionsEditorPlugin.DrillthroughColumn;
                DataGridViewComboBoxEditingControl comboBox = e.Control as DataGridViewComboBoxEditingControl;

                if (mg != null)
                {
                    List<string> relatedTables = GetRelatedTables(cube.Parent.Dimensions.GetByName(mg.Name), mg);
                    relatedTables.Add(mg.Name);
                    relatedTables.Sort();
                    comboBox.Items.Clear();
                    comboBox.Items.AddRange(relatedTables.ToArray());

                    if (!string.IsNullOrEmpty(item.CubeDimension))
                        comboBox.SelectedIndex = relatedTables.IndexOf(item.CubeDimension);
                }
                else
                {
                    comboBox.DataSource = new string[] { };
                }
            }
        }

        private static List<string> GetRelatedTables(Microsoft.AnalysisServices.Dimension dMG, MeasureGroup mgOuter)
        {
            List<string> list = new List<string>();
            foreach (Relationship relOuter in dMG.Relationships)
            {
                bool bFound = false;
                MeasureGroup mgFrom = dMG.Parent.Cubes[0].MeasureGroups[relOuter.FromRelationshipEnd.DimensionID];
                Microsoft.AnalysisServices.Dimension dTo = dMG.Parent.Dimensions[relOuter.ToRelationshipEnd.DimensionID];
                CubeDimension dToCube = dMG.Parent.Cubes[0].Dimensions[relOuter.ToRelationshipEnd.DimensionID];
                foreach (MeasureGroupDimension mgdOuter in mgFrom.Dimensions)
                {
                    ReferenceMeasureGroupDimension rmgdOuter = mgdOuter as ReferenceMeasureGroupDimension;
                    if (rmgdOuter != null && rmgdOuter.Materialization == ReferenceDimensionMaterialization.Regular && rmgdOuter.RelationshipID == relOuter.ID)
                    {
                        //active relationships have a materialized reference relationship
                        bFound = true;
                        break;
                    }
                }
                if (!bFound)
                {
                    continue; //don't show inactive relationships 
                }

                list.Add(dToCube.Name);
                list.AddRange(GetRelatedTables(dTo, mgOuter));
            }

            return list;
        }

        private void btnDrillthroughColumnMoveUp_Click(object sender, EventArgs e)
        {
            int iCurrentRow = this.dataGridViewDrillthroughColumns.CurrentRow.Index;
            if (iCurrentRow > 0 && iCurrentRow != this.dataGridViewDrillthroughColumns.NewRowIndex)
            {
                this.dataGridViewDrillthroughColumns.EndEdit();
                TabularActionsEditorPlugin.DrillthroughColumn current = _listDrillthroughColumns[iCurrentRow];
                _listDrillthroughColumns.Remove(current);
                _listDrillthroughColumns.Insert(iCurrentRow - 1, current);
                this.dataGridViewDrillthroughColumns.Refresh();
                this.dataGridViewDrillthroughColumns.CurrentCell = this.dataGridViewDrillthroughColumns.Rows[iCurrentRow - 1].Cells[0];
            }
        }

        private void btnDrillthroughColumnMoveDown_Click(object sender, EventArgs e)
        {
            int iCurrentRow = this.dataGridViewDrillthroughColumns.CurrentRow.Index;
            if (iCurrentRow < _listDrillthroughColumns.Count - 1)
            {
                this.dataGridViewDrillthroughColumns.EndEdit();
                TabularActionsEditorPlugin.DrillthroughColumn current = _listDrillthroughColumns[iCurrentRow];
                _listDrillthroughColumns.Remove(current);
                _listDrillthroughColumns.Insert(iCurrentRow + 1, current);
                this.dataGridViewDrillthroughColumns.Refresh();
                this.dataGridViewDrillthroughColumns.CurrentCell = this.dataGridViewDrillthroughColumns.Rows[iCurrentRow + 1].Cells[0];
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            StandardAction action = new StandardAction();
            action.Name = "New Action";
            action.ID = Guid.NewGuid().ToString();
            action.Type = ActionType.Url;
            action.Invocation = ActionInvocation.Interactive;
            _listActionClones.Add(action);
            cmbAction.Items.Add(action);
            cmbAction.SelectedItem = _listActionClones[_listActionClones.Count - 1];

            string sNameError = string.Empty;
            if (!IsValidActionName(action.Name, out sNameError))
            {
                for (int i = 2; i < 1000000; i++)
                {
                    if (IsValidActionName(action.Name + " " + i, out sNameError))
                    {
                        action.Name = action.Name + " " + i;
                        txtName.Text = action.Name;
                        break;
                    }
                }
            }
            
            List<string> lPerspectives = new List<string>();
            foreach (Perspective p in cube.Perspectives)
            {
                lPerspectives.Add(p.Name);
            }
            if (!_dictActionPerspectives.ContainsKey(action.ID))
                _dictActionPerspectives.Add(action.ID, lPerspectives.ToArray());
            else
                _dictActionPerspectives[action.ID] = lPerspectives.ToArray();
            DisableControls(false, arrEnabledControls);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            SaveAction();

            Microsoft.AnalysisServices.Action action = _currentAction.Clone();
            string sName = action.Name + " Copy";
            if (sName.Length > 95)
                sName = sName.Substring(0, 95);
            action.Name = sName;
            action.ID = Guid.NewGuid().ToString();
            _listActionClones.Add(action);

            if (!_dictActionPerspectives.ContainsKey(action.ID))
                _dictActionPerspectives.Add(action.ID, _dictActionPerspectives[_currentAction.ID]);
            else
                _dictActionPerspectives[action.ID] = _dictActionPerspectives[_currentAction.ID];
            
            cmbAction.Items.Add(action);
            cmbAction.SelectedItem = _listActionClones[_listActionClones.Count - 1];

            string sNameError = string.Empty;
            if (!IsValidActionName(action.Name, out sNameError))
            {
                for (int i = 2; i < 1000000; i++)
                {
                    if (IsValidActionName(action.Name + " " + i, out sNameError))
                    {
                        action.Name = action.Name + " " + i;
                        txtName.Text = action.Name;
                        break;
                    }
                }
            }

            DisableControls(false, arrEnabledControls);

            FillScreen();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (cmbAction.SelectedIndex < 0) return;
            _listActionClones.RemoveAt(cmbAction.SelectedIndex);
            cmbAction.Items.RemoveAt(cmbAction.SelectedIndex);
            if (_listActionClones.Count > 0)
                cmbAction.SelectedIndex = 0;
            else
            {
                DisableControls(true, arrEnabledControls);
                _currentAction = null;
            }
        }

        private void cmbAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_skipEvents) return;
            SaveAction(); //save old action
            FillScreen();
        }

        private void SaveAction()
        {
            if (_currentAction == null || cmbAction.SelectedIndex == -1) return;
            if (cmbActionType.SelectedIndex >= 0)
                _currentAction.Type = (ActionType)Enum.Parse(typeof(ActionType), cmbActionType.Text);

            if (_currentAction.Type == ActionType.DrillThrough && !(_currentAction is DrillThroughAction))
            {
                int iIndex = _listActionClones.IndexOf(_currentAction);
                string sID = _currentAction.ID;
                _currentAction = new DrillThroughAction();
                _currentAction.ID = sID;
                _currentAction.Type = ActionType.DrillThrough;
                _listActionClones[iIndex] = _currentAction;
            }
            else if (_currentAction.Type == ActionType.Report && !(_currentAction is ReportAction))
            {
                int iIndex = _listActionClones.IndexOf(_currentAction);
                string sID = _currentAction.ID;
                _currentAction = new ReportAction();
                _currentAction.ID = sID;
                _currentAction.Type = ActionType.Report;
                _listActionClones[iIndex] = _currentAction;
            }
            else if (_currentAction.Type != ActionType.Report && _currentAction.Type != ActionType.DrillThrough && !(_currentAction is StandardAction))
            {
                int iIndex = _listActionClones.IndexOf(_currentAction);
                string sID = _currentAction.ID;
                ActionType type = _currentAction.Type;
                _currentAction = new StandardAction();
                _currentAction.ID = sID;
                _currentAction.Type = type;
                _listActionClones[iIndex] = _currentAction;
            }

            _currentAction.Name = TabularHelpers.CleanNameOfInvalidChars(txtName.Text);
            _currentAction.Caption = txtCaption.Text;
            _currentAction.CaptionIsMdx = chkCaptionIsMdx.Checked;
            _currentAction.Description = txtDescription.Text;
            _currentAction.Condition = txtCondition.Text;

            if (cmbTargetType.SelectedIndex >= 0)
                _currentAction.TargetType = (ActionTargetType)Enum.Parse(typeof(ActionTargetType), cmbTargetType.Text);

            _currentAction.Target = cmbTarget.Text;

            if (cmbInvocation.SelectedIndex >= 0)
                _currentAction.Invocation = (ActionInvocation)Enum.Parse(typeof(ActionInvocation), cmbInvocation.Text);

            List<string> lPerspectiveIDs = new List<string>();
            List<string> lPerspective = new List<string>();
            foreach (string sPerspective in listPerspectives.CheckedItems)
            {
                lPerspective.Add(sPerspective);
                lPerspectiveIDs.Add(cube.Perspectives.GetByName(sPerspective).ID);
            }
            _dictActionPerspectives[_currentAction.ID] = lPerspective.ToArray();

            if (_currentAction.Type == ActionType.DrillThrough)
            {
                DrillThroughAction action = (DrillThroughAction)_currentAction;
                action.Columns.Clear();
                dataGridViewDrillthroughColumns.EndEdit();
                foreach (TabularActionsEditorPlugin.DrillthroughColumn col in _listDrillthroughColumns)
                {
                    if (!string.IsNullOrEmpty(col.CubeDimension) && !string.IsNullOrEmpty(col.Attribute))
                    {
                        CubeDimension cd = cube.Dimensions.FindByName(col.CubeDimension);
                        if (cd != null)
                        {
                            DimensionAttribute da = cd.Dimension.Attributes.FindByName(col.Attribute); //if a column doesn't exist anymore in the model, don't throw an error, just skip it... TODO: give some warning or red indication or error?
                            if (da != null)
                            {
                                action.Columns.Add(new CubeAttributeBinding(cube.ID, cd.ID, da.ID, AttributeBindingType.Key)); //6/20/2012 - using AttributeBindingType.Key so that numbers come back as numbers in drillthrough
                            }
                        }
                    }
                }

                action.Default = (cmbDefault.Text == "True");
                long lngMaxRows;
                action.MaximumRows = (string.IsNullOrWhiteSpace(txtMaxRows.Text) || !long.TryParse(txtMaxRows.Text, out lngMaxRows) ? -1 : lngMaxRows);
            }
            else if (_currentAction.Type == ActionType.Report)
            {
                ReportAction action = (ReportAction)_currentAction;

                action.ReportFormatParameters.Clear();
                action.ReportFormatParameters.Add("rs:Command", "Render");

                action.ReportParameters.Clear();
                dataGridViewReportParameters.EndEdit();
                foreach (ReportParameter rp in _listReportParameters)
                {
                    if (!string.IsNullOrEmpty(rp.Name) && !string.IsNullOrEmpty(rp.Value))
                    {
                        action.ReportParameters.Add(rp);
                    }
                }

                action.ReportServer = txtReportServer.Text;
                action.Path = txtMaxRows.Text; //multipurpose field
            }
            else
            {
                StandardAction action = (StandardAction)_currentAction;
                action.Expression = txtExpression.Text;
            }

        }

        private void FillScreen()
        {
            if (cmbAction.SelectedIndex >= 0)
            {
                _currentAction = _listActionClones[cmbAction.SelectedIndex];
                txtName.Text = _currentAction.Name;
                txtCaption.Text = _currentAction.Caption;
                chkCaptionIsMdx.Checked = _currentAction.CaptionIsMdx;
                txtDescription.Text = NormalizeLineBreaks(_currentAction.Description);
                txtCondition.Text = NormalizeLineBreaks(_currentAction.Condition);
                cmbActionType.SelectedItem = _currentAction.Type.ToString();
                cmbTargetType.SelectedItem = _currentAction.TargetType.ToString();

                if (_currentAction.Target != null && !cmbTarget.Items.Contains(_currentAction.Target))
                    cmbTarget.Items.Add(_currentAction.Target);
                cmbTarget.SelectedItem = _currentAction.Target;
                cmbTarget_SelectedIndexChanged(null, null);

                cmbInvocation.SelectedItem = _currentAction.Invocation.ToString();

                foreach (Perspective perspective in cube.Perspectives)
                {
                    for (int i = 0; i < listPerspectives.Items.Count; i++)
                    {
                        if ((string)listPerspectives.Items[i] == perspective.Name)
                        {
                            bool bChecked = false;
                            if (_dictActionPerspectives.ContainsKey(_currentAction.ID))
                                bChecked = (new List<string>(_dictActionPerspectives[_currentAction.ID])).Contains(perspective.Name);
                            listPerspectives.SetItemChecked(i, bChecked);
                        }
                    }
                }

                if (_currentAction is StandardAction)
                {
                    StandardAction action = (StandardAction)_currentAction;
                    txtExpression.Text = NormalizeLineBreaks(action.Expression);
                }
                else
                {
                    txtExpression.Text = string.Empty;
                }

                if (_currentAction is DrillThroughAction)
                {
                    DrillThroughAction action = (DrillThroughAction)_currentAction;
                    _listDrillthroughColumns = new List<TabularActionsEditorPlugin.DrillthroughColumn>();
                    foreach (Microsoft.AnalysisServices.Binding binding in action.Columns)
                    {
                        CubeAttributeBinding cubeAttributeBinding = binding as CubeAttributeBinding;
                        if (cubeAttributeBinding == null) continue;
                        CubeDimension cd = cube.Dimensions.Find(cubeAttributeBinding.CubeDimensionID);
                        if (cd == null) continue;
                        DimensionAttribute a = cd.Dimension.Attributes.Find(cubeAttributeBinding.AttributeID);
                        if (a == null) continue;
                        TabularActionsEditorPlugin.DrillthroughColumn col = new TabularActionsEditorPlugin.DrillthroughColumn();
                        col.CubeDimension = cd.Name;
                        col.Attribute = a.Name;
                        _listDrillthroughColumns.Add(col);
                    }
                    this.drillthroughColumnBindingSource.DataSource = _listDrillthroughColumns;
                    dataGridViewDrillthroughColumns.DataSource = drillthroughColumnBindingSource;
                    dataGridViewDrillthroughColumns.Refresh();

                    cmbDefault.SelectedItem = (action.Default ? "True" : "False");
                    txtMaxRows.Text = (action.MaximumRows <= 0 ? string.Empty : action.MaximumRows.ToString());
                }
                else
                {
                    _listDrillthroughColumns = new List<TabularActionsEditorPlugin.DrillthroughColumn>();
                    this.drillthroughColumnBindingSource.DataSource = _listDrillthroughColumns;
                    dataGridViewDrillthroughColumns.DataSource = drillthroughColumnBindingSource;
                    dataGridViewDrillthroughColumns.Refresh();
                }

                if (_currentAction is ReportAction)
                {
                    ReportAction ra = (ReportAction)_currentAction;
                    _listReportParameters = new List<ReportParameter>();

                    foreach (ReportParameter rp in ra.ReportFormatParameters)
                    {
                        if (string.Compare(rp.Name, "rs:Command", true) != 0) //rs:Command=Render must be put in ReportFormatParameters or else the action won't work... probably a bug in SSAS... so assume it and don't show it in the UI
                        {
                            _listReportParameters.Add(rp);
                        }
                    }
                    foreach (ReportParameter rp in ra.ReportParameters)
                    {
                        _listReportParameters.Add(rp);
                    }

                    reportParameterBindingSource.DataSource = _listReportParameters;
                    dataGridViewReportParameters.DataSource = reportParameterBindingSource;
                    dataGridViewReportParameters.Refresh();

                    txtReportServer.Text = ra.ReportServer;
                    txtMaxRows.Text = ra.Path; //using this as ReportPath also
                }
                else
                {
                    _listReportParameters = new List<ReportParameter>();
                    reportParameterBindingSource.DataSource = _listReportParameters;
                    dataGridViewReportParameters.DataSource = reportParameterBindingSource;
                    dataGridViewReportParameters.Refresh();

                    txtReportServer.Text = string.Empty;

                    if (!(_currentAction is DrillThroughAction))
                    {
                        txtMaxRows.Text = string.Empty; //using this as ReportPath also
                    }
                }

            }
            else
            {
                txtName.Text = string.Empty;
                txtCaption.Text = string.Empty;
                chkCaptionIsMdx.Checked = false;
                txtDescription.Text = string.Empty;
                cmbActionType.SelectedItem = -1;
                cmbTargetType.SelectedItem = -1;
                cmbTarget.SelectedIndex = -1;
                cmbDefault.SelectedIndex = -1;
                txtMaxRows.Text = string.Empty;

                _listDrillthroughColumns = new List<TabularActionsEditorPlugin.DrillthroughColumn>();
                drillthroughColumnBindingSource.DataSource = _listDrillthroughColumns;
                dataGridViewDrillthroughColumns.DataSource = drillthroughColumnBindingSource;
                dataGridViewDrillthroughColumns.Refresh();

                _listReportParameters = new List<ReportParameter>();
                reportParameterBindingSource.DataSource = _listReportParameters;
                dataGridViewReportParameters.DataSource = reportParameterBindingSource;
                dataGridViewReportParameters.Refresh();
            }
        }

        private string NormalizeLineBreaks(string text)
        {
            if (text != null)
            {
                return text.Replace("\r\n", "\r").Replace("\n", "\r").Replace("\r", "\r\n");
            }
            else
            {
                return null;
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbAction.SelectedIndex < 0) return;
                string sNameErrorReason = string.Empty;
                string sName = TabularHelpers.CleanNameOfInvalidChars(txtName.Text);
                bool bIsValidName = IsValidActionName(sName, out sNameErrorReason);
                if (bIsValidName) //change the name of the current action if the name is valid... if not, the Leave event will trap this
                {
                    _currentAction.Name = sName;
                    _skipEvents = true;
                    cmbAction.Items[cmbAction.SelectedIndex] = _currentAction;
                    _skipEvents = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void txtName_Leave(object sender, EventArgs e)
        {
            string sNameErrorReason = string.Empty;
            string sName = TabularHelpers.CleanNameOfInvalidChars(txtName.Text);
            bool bIsValidName = IsValidActionName(sName, out sNameErrorReason);
            if (!bIsValidName)
            {
                txtName.Focus();
                MessageBox.Show("The action name \"" + txtName.Text + "\" is not valid for the following reason:\r\n\r\n" + sNameErrorReason);
            }
        }

        private bool IsValidActionName(string sName, out string sError)
        {
            string sNameErrorReason = string.Empty;
            Cube c = new Cube();
            bool bIsValidName = c.Actions.IsValidName(sName, out sError);
            if (!bIsValidName) return false;

            foreach (object o in cmbAction.Items)
            {
                Microsoft.AnalysisServices.Action a = (Microsoft.AnalysisServices.Action)o;
                if (a.ID != _currentAction.ID && string.Compare(a.Name, sName, true) == 0)
                {
                    sError = "There is already another action with the name " + sName;
                    return false;
                }
            }
            return true;
        }


        private void cmbActionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbActionType.Text == ActionType.DrillThrough.ToString())
            {
                dataGridViewDrillthroughColumns.Visible = true;
                dataGridViewReportParameters.Visible = false;
                btnDrillthroughColumnMoveUp.Visible = true;
                btnDrillthroughColumnMoveDown.Visible = true;
                txtExpression.Visible = false;
                lblChangingLabel.Text = "Drillthrough Columns:";
                lblDefault.Visible = true;
                lblDefault.Text = "Default:";
                lblMaxRows.Visible = true;
                lblMaxRows.Text = "Maximum Rows:";
                cmbDefault.Visible = true;
                txtMaxRows.Text = string.Empty;
                txtMaxRows.Visible = true;
                cmbTargetType.SelectedItem = ActionTargetType.Cells.ToString();
                txtReportServer.Visible = false;
                btnTestMDX.Top = this.lblCondition.Bottom + 2;
            }
            else if (cmbActionType.Text == ActionType.Report.ToString())
            {
                dataGridViewDrillthroughColumns.Visible = false;
                dataGridViewReportParameters.Visible = true;
                btnDrillthroughColumnMoveUp.Visible = false;
                btnDrillthroughColumnMoveDown.Visible = false;
                txtExpression.Visible = false;
                lblChangingLabel.Text = "Report Parameters:";
                lblDefault.Visible = true;
                lblDefault.Text = "Report Server:";
                lblMaxRows.Visible = true;
                lblMaxRows.Text = "Report Path:";
                cmbDefault.Visible = false;
                txtMaxRows.Text = string.Empty;
                txtMaxRows.Visible = true;
                txtReportServer.Visible = true;
                btnTestMDX.Top = btnDrillthroughDefault.Top;

                _listReportParameters = new List<ReportParameter>();
                _listReportParameters.Add(new ReportParameter("rs:Format", "\"HTML5\""));
                reportParameterBindingSource.DataSource = _listReportParameters;
                dataGridViewReportParameters.DataSource = reportParameterBindingSource;
                dataGridViewReportParameters.Refresh();
            }
            else
            {
                dataGridViewDrillthroughColumns.Visible = false;
                dataGridViewReportParameters.Visible = false;
                btnDrillthroughColumnMoveUp.Visible = false;
                btnDrillthroughColumnMoveDown.Visible = false;
                txtExpression.Visible = true;
                lblChangingLabel.Text = "Expression:";
                lblDefault.Visible = false;
                lblMaxRows.Visible = false;
                cmbDefault.Visible = false;
                txtMaxRows.Visible = false;
                txtReportServer.Visible = false;
                btnTestMDX.Top = btnDrillthroughDefault.Top;
            }
        }

        private void cmbTargetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTargetType targetType = (ActionTargetType)Enum.Parse(typeof(ActionTargetType), cmbTargetType.Text);
            if (targetType == ActionTargetType.AttributeMembers)
            {
                cmbTarget.Items.Clear();
                foreach (CubeDimension cd in cube.Dimensions)
                {
                    foreach (DimensionAttribute a in cd.Dimension.Attributes)
                    {
                        cmbTarget.Items.Add(string.Format("[" + cd.Name + "].[" + a.Name + "]"));
                    }
                }
            }
            else if (targetType == ActionTargetType.Cells)
            {
                cmbTarget.Items.Clear();
                cmbTarget.Items.Add(string.Empty);
                foreach (MeasureGroup mg in cube.MeasureGroups)
                {
                    cmbTarget.Items.Add("MeasureGroupMeasures(\"" + mg.Name + "\")");
                }
            }
            else if (targetType == ActionTargetType.Cube)
            {
                cmbTarget.Items.Clear();
                cmbTarget.Items.Add("[CURRENTCUBE]");
                cmbTarget.SelectedIndex = 0;
            }
            else if (targetType == ActionTargetType.DimensionMembers)
            {
                cmbTarget.Items.Clear();
                foreach (CubeDimension cd in cube.Dimensions)
                {
                    cmbTarget.Items.Add(string.Format("[" + cd.Name + "]"));
                }
            }
            else if (targetType == ActionTargetType.Hierarchy || targetType == ActionTargetType.HierarchyMembers)
            {
                cmbTarget.Items.Clear();
                foreach (CubeDimension cd in cube.Dimensions)
                {
                    foreach (DimensionAttribute a in cd.Dimension.Attributes)
                    {
                        cmbTarget.Items.Add(string.Format("[" + cd.Name + "].[" + a.Name + "]"));
                    }
                    foreach (Microsoft.AnalysisServices.Hierarchy a in cd.Dimension.Hierarchies)
                    {
                        cmbTarget.Items.Add(string.Format("[" + cd.Name + "].[" + a.Name + "]"));
                    }
                }
            }
            else if (targetType == ActionTargetType.Level || targetType == ActionTargetType.LevelMembers)
            {
                cmbTarget.Items.Clear();
                cmbTarget.Items.Add(string.Format("[Measures].[MeasuresLevel]"));
                foreach (CubeDimension cd in cube.Dimensions)
                {
                    foreach (Microsoft.AnalysisServices.Hierarchy a in cd.Dimension.Hierarchies)
                    {
                        foreach (Microsoft.AnalysisServices.Level l in a.Levels)
                        {
                            cmbTarget.Items.Add(string.Format("[" + cd.Name + "].[" + a.Name + "].[" + l.Name + "]"));
                        }
                    }
                }
            }
            //todo: investigate what ActionTargetType.Set does. Doesn't appear to be in the multidimensional UI, and tabular models don't yet support sets
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://bideveloperextensions.github.io/features/TabularActionsEditor/");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                string sNameErrorReason = string.Empty;
                string sName = TabularHelpers.CleanNameOfInvalidChars(txtName.Text);
                bool bIsValidName = IsValidActionName(sName, out sNameErrorReason);
                if (bIsValidName)
                {
                    SaveAction();
                }
                else
                {
                    txtName_Leave(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void contextMenuStripDrillColumns_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem == contextMenuDelete)
                {
                    if (this.dataGridViewDrillthroughColumns.Visible)
                    {
                        this.dataGridViewDrillthroughColumns.Rows.Remove(this.dataGridViewDrillthroughColumns.CurrentRow);
                    }
                    else if (this.dataGridViewReportParameters.Visible)
                    {
                        this.dataGridViewReportParameters.Rows.Remove(this.dataGridViewReportParameters.CurrentRow);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridViewReportParameters_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    dataGridViewReportParameters.CurrentCell = dataGridViewReportParameters.Rows[e.RowIndex].Cells[e.ColumnIndex];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridViewDrillthroughColumns_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    dataGridViewDrillthroughColumns.CurrentCell = dataGridViewDrillthroughColumns.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    dataGridViewDrillthroughColumns.BeginEdit(true);
                    DataGridViewComboBoxEditingControl combo = dataGridViewDrillthroughColumns.EditingControl as DataGridViewComboBoxEditingControl;
                    if (combo != null)
                        combo.DroppedDown = true;
                }
                else if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    dataGridViewDrillthroughColumns.CurrentCell = dataGridViewDrillthroughColumns.Rows[e.RowIndex].Cells[e.ColumnIndex];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TabularActionsEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == System.Windows.Forms.DialogResult.Cancel) return;
            string sNameErrorReason = string.Empty;
            string sName = TabularHelpers.CleanNameOfInvalidChars(txtName.Text);
            bool bIsValidName = IsValidActionName(sName, out sNameErrorReason);
            if (!bIsValidName)
            {
                e.Cancel = true;
                MessageBox.Show(sNameErrorReason, "BIDS Helper Tabular Actions Editor");
            }
        }

        private void btnDrillthroughDefault_Click(object sender, EventArgs e)
        {
            SaveAction();

            _listDrillthroughColumns = new List<TabularActionsEditorPlugin.DrillthroughColumn>();

            MeasureGroup mg = null;
            foreach (MeasureGroup mg2 in cube.MeasureGroups)
            {
                if (_currentAction.Target == "MeasureGroupMeasures(\"" + mg2.Name + "\")")
                {
                    mg = mg2;
                    break;
                }
            }

            if (mg != null)
            {
                List<string> relatedTables = GetRelatedTables(cube.Parent.Dimensions.GetByName(mg.Name), mg);
                relatedTables.Add(mg.Name);
                relatedTables.Sort();

                foreach (string sTable in relatedTables)
                {
                    CubeDimension cd = cube.Dimensions.GetByName(sTable);
                    if (cd == null) continue;

                    foreach (Microsoft.AnalysisServices.DimensionAttribute a in cd.Dimension.Attributes)
                    {
                        if (a.Type != AttributeType.RowNumber) //RowNumber was not supposed to return in drillthrough commands but was at RTM due to a bug
                        {
                            TabularActionsEditorPlugin.DrillthroughColumn col = new TabularActionsEditorPlugin.DrillthroughColumn();
                            col.CubeDimension = cd.Name;
                            col.Attribute = a.Name;
                            _listDrillthroughColumns.Add(col);
                        }
                    }
                }

                this.dataGridViewDrillthroughColumns.EndEdit();
                this.drillthroughColumnBindingSource.DataSource = _listDrillthroughColumns;
                dataGridViewDrillthroughColumns.DataSource = drillthroughColumnBindingSource;
                dataGridViewDrillthroughColumns.Refresh();
            }
            else
            {
                return;
            }
        }

        private void cmbTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbActionType.Text == ActionType.DrillThrough.ToString())
            {
                if (Convert.ToString(cmbTarget.SelectedItem).StartsWith("MeasureGroupMeasures("))
                {
                    btnDrillthroughDefault.Visible = true;
                    return;
                }
            }
            btnDrillthroughDefault.Visible = false;
        }

        private void btnTestMDX_Click(object sender, EventArgs e)
        {
            try
            {
                SaveAction();

                AdomdCommand cmd = new AdomdCommand();
                cmd.CommandTimeout = 0;
                cmd.Connection = conn;
                if (conn.State != ConnectionState.Open) conn.Open();

                Dictionary<string, string> dictQueries = new Dictionary<string, string>();

                if (_currentAction.CaptionIsMdx)
                {
                    dictQueries.Add("Caption MDX", "WITH MEMBER [Measures].[Caption MDX] as " + _currentAction.Caption + "\r\nSELECT [Measures].[Caption MDX] on COLUMNS");
                }
                if (!string.IsNullOrWhiteSpace(_currentAction.Condition))
                {
                    dictQueries.Add("Condition MDX", "WITH MEMBER [Measures].[Condition MDX] as " + _currentAction.Condition + "\r\nSELECT [Measures].[Condition MDX] on COLUMNS");
                }
                if (txtExpression.Visible && !string.IsNullOrWhiteSpace(txtExpression.Text))
                {
                    dictQueries.Add("Expression MDX", "WITH MEMBER [Measures].[Expression MDX] as " + txtExpression.Text + "\r\nSELECT [Measures].[Expression MDX] on COLUMNS");
                }
                if (_currentAction is ReportAction)
                {
                    ReportAction reportAction = (ReportAction)_currentAction;
                    for (int i = 0; i < reportAction.ReportParameters.Count; i++)
                    {
                        ReportParameter rp = reportAction.ReportParameters[i];
                        dictQueries.Add("Report Parameter " + i + " MDX (" + rp.Name + ")", "WITH MEMBER [Measures].[Report Parameter " + i + " MDX] as " + rp.Value + "\r\nSELECT [Measures].[Report Parameter " + i + " MDX] on COLUMNS");
                    }
                }

                StringBuilder sbResult = new StringBuilder();
                sbResult.AppendLine("Each MDX expression in this action is tested to make sure it works. Ensure each expression evaluates to a string except for the Condition which should evaluate to a boolean.\r\n");
                foreach (string sKey in dictQueries.Keys)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(dictQueries[sKey]);
                    sb.AppendLine("FROM [" + cube.Name + "]");
                    if (_currentAction.TargetType == ActionTargetType.AttributeMembers || _currentAction.TargetType == ActionTargetType.HierarchyMembers || _currentAction.TargetType == ActionTargetType.Hierarchy)
                    {
                        sb.AppendLine("WHERE (" + _currentAction.Target + ".[All].FirstChild)");
                    }
                    else if (_currentAction.TargetType == ActionTargetType.Level || _currentAction.TargetType == ActionTargetType.LevelMembers)
                    {
                        sb.AppendLine("WHERE (Head(" + _currentAction.Target + ",1))");
                    }

                    cmd.CommandText = sb.ToString();
                    try
                    {
                        CellSet cs = cmd.ExecuteCellSet();
                        sbResult.AppendLine(sKey + ": " + Convert.ToString(cs.Cells[0].Value));
                    }
                    catch (Exception exMDX)
                    {
                        sbResult.AppendLine(sKey + ": ERROR! " + exMDX.Message);
                    }
                    sbResult.AppendLine();
                }
                if (dictQueries.Count == 0)
                    sbResult.AppendLine("No MDX expressions used in this action.");
                MessageBox.Show(sbResult.ToString(), "BIDS Helper Tabular Actions Editor - Test MDX Expressions");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}