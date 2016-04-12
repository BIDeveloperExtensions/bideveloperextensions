namespace BIDSHelper.SSIS
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.DataTransformationServices.Design;
    using Microsoft.DataWarehouse.Design;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.SqlServer.Management.UI.Grid;
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Forms;

    public partial class ParametersWindowPlugin : BIDSHelperWindowActivatedPluginBase
    {
#if SQL2014
        private const string SSIS_DESIGNER_WINDOW_KIND = "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}";
#elif DENALI
        private const string SSIS_DESIGNER_WINDOW_KIND = "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}";
#endif
        private const BindingFlags getPropertyFlags = BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.DeclaredOnly | BindingFlags.Instance;
        private const BindingFlags getFieldFlags = BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public ParametersWindowPlugin(Connect con, DTE2 appObject, AddIn addinInstance) : base(con, appObject, addinInstance)
        {
        }

        public override bool ShouldHookWindowCreated
        {
            get { return true; }
        }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            if (GotFocus == null)
                return;

            try
            {
                // ObjectKind can throw errors, wrap in try catch
                if (GotFocus.ObjectKind != SSIS_DESIGNER_WINDOW_KIND)
                    return;

                if (GotFocus.DTE.Mode == vsIDEMode.vsIDEModeDebug)
                    return;
            }
            catch
            { }

            try
            {
                if (GotFocus.Object == null)
                    return;

                IDesignerHost designer = GotFocus.Object as IDesignerHost;
                if (designer == null)
                    return;

                ProjectItem pi = GotFocus.ProjectItem;
                if (!(pi.Name.ToLower().EndsWith(".dtsx")))
                {
                    return;
                }

                // We want the DtsPackageView, an EditorWindow, Microsoft.DataTransformationServices.Design.DtsPackageView
                EditorWindow editorWindow = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));

                if (editorWindow.Tag == null)
                {
                    ParametersWindowManager manager = new ParametersWindowManager(editorWindow);
                }
                else
                {
                    // Safety check to see if anyoine else is using the Tag on the DtsPackageView
                    ParametersWindowManager manager = editorWindow.Tag as ParametersWindowManager;
                    if (manager == null)
                    {
                        throw new Exception(string.Format("DtsPackageView tag is unexpected type, {0}", editorWindow.Tag));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, DefaultMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public override string ShortName
        {
            get { return "ParametersWindowPlugin"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "SSIS Parameters Window Extensions"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        public override string MenuName
        {
            get { return string.Empty; } //no need to have a menu command
        }

        /// <summary>
        /// Gets the name of the friendly name of the plug-in.
        /// </summary>
        /// <value>The friendly name.</value>
        /// <remarks>Used for HelpUrl as ButtonText does not match Wiki page.</remarks>
        public override string FeatureName
        {
            get { return "Parameters Window Extensions"; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Extended features for the Parameters window. Find parameter references, and find unused parameters."; }
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


        public override void Exec()
        {
        }

        /// <summary>
        /// Helper class that we bind to each parameters window, to maintain context and handle toolbar button events.
        /// The main ParametersWindowPlugin class won't work, because we can have multiple pakages, designers, and therefore parameter windows, grids and toolbars.
        /// </summary>
        public class ParametersWindowManager
        {
            private ToolBarButton findReferencesButton;
            private ToolBarButton findUnusedButton;            
            private UserControl variablesToolWindowControl;
            private DlgGridControl grid;
            private EditorWindow editorWindow;
            private bool setupComplete;

            public ParametersWindowManager(EditorWindow editorWindow)
            {
                editorWindow.Tag = this;
                editorWindow.ActiveViewChanged += editorWindow_ActiveViewChanged;
            }

            private void editorWindow_ActiveViewChanged(object sender, EventArgs e)
            {
                if (setupComplete)
                    return;

                // Get the EditorWindow, Microsoft.DataTransformationServices.Design.DtsPackageView
                editorWindow = sender as EditorWindow;

                if (editorWindow.Tag != this)
                {
                    Debug.Assert(false);
                }

                EditorWindow.EditorView view = editorWindow.SelectedView;
                if (view.Caption == "Parameters") // Microsoft.DataTransformationServices.Design.SR.PackageParametersViewCaption
                {
                    object viewControl = view.GetType().InvokeMember("viewControl", getFieldFlags, null, view, null);
                    UserControl control = (UserControl)viewControl; // PackageParametersControl
                    variablesToolWindowControl = (VariablesToolWindow)control.Controls[0];

                    SetupControl();
                }
            }

            private void SetupControl()
            {
                // "tableLayoutPanelMain" - "tableLayoutPanelParameter" - "toolBarParameter"
                ToolBar toolbar = (ToolBar)variablesToolWindowControl.Controls[0].Controls[1].Controls[1];

                // If buttons already added, no need to do it again so exit 
                if (this.findReferencesButton != null && toolbar.Buttons.Contains(this.findReferencesButton))
                    return;

                ToolBarButton separator = new ToolBarButton();
                separator.Style = ToolBarButtonStyle.Separator;
                toolbar.Buttons.Add(separator);

                // Find References button
                this.findReferencesButton = new ToolBarButton();
                this.findReferencesButton.Style = ToolBarButtonStyle.PushButton;
                this.findReferencesButton.ToolTipText = "Find Parameter References (BIDS Helper)";
                toolbar.Buttons.Add(this.findReferencesButton);
                toolbar.ImageList.Images.Add(BIDSHelper.Resources.Versioned.VariableFindReferences);
                this.findReferencesButton.ImageIndex = toolbar.ImageList.Images.Count - 1;

                // Find Unused button
                this.findUnusedButton = new ToolBarButton();
                this.findUnusedButton.Style = ToolBarButtonStyle.PushButton;
                this.findUnusedButton.ToolTipText = "Find Parameter References (BIDS Helper)";
                toolbar.Buttons.Add(this.findUnusedButton);
                toolbar.ImageList.Images.Add(BIDSHelper.Resources.Versioned.VariableFindUnused);
                this.findUnusedButton.ImageIndex = toolbar.ImageList.Images.Count - 1;

                toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);
                toolbar.Wrappable = false;

                // "tableLayoutPanelMain" - "tableLayoutPanelParameter" - "parameterGridControl"
                grid = (DlgGridControl)variablesToolWindowControl.Controls[0].Controls[1].Controls[0];
                grid.SelectionChanged += new SelectionChangedEventHandler(grid_SelectionChanged);
                grid.Invalidated += new InvalidateEventHandler(grid_Invalidated);

                SetButtonEnabled();

                setupComplete = true;
            }

            private void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
            {
                if (e.Button == this.findReferencesButton)
                    FindReferencesButtonClick();
                else if (e.Button == this.findUnusedButton)
                    FindUnusedButtonClick();
            }

            private void FindReferencesButtonClick()
            {
                try
                {
                    ComponentDesigner packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().GetProperty("PackageDesigner", BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy | BindingFlags.Instance).GetValue(variablesToolWindowControl, null);
                    if (packageDesigner == null) return;

                    Package package = packageDesigner.Component as Package;
                    if (package == null) return;

                    int selectedRow;
                    int selectedCol;
                    grid.GetSelectedCell(out selectedRow, out selectedCol);

                    if (selectedRow < 0) return;

                    Parameter parameter = GetParameterForRow(selectedRow);
                    if (parameter == null) return;

                    FindReferences dialog = new FindReferences();
                    dialog.Show(package, parameter);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
                }
            }

            private void FindUnusedButtonClick()
            {
                try
                {
                    ComponentDesigner packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().GetProperty("PackageDesigner", BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy | BindingFlags.Instance).GetValue(variablesToolWindowControl, null);
                    if (packageDesigner == null) return;

                    Package package = packageDesigner.Component as Package;
                    if (package == null) return;

                    FindUnusedVariables dialog = new FindUnusedVariables(VariablesDisplayMode.PackageParameters);
                    dialog.Show(package);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
                }
            }

            private Parameter GetParameterForRow(int iRow)
            {
                GridCell cell = grid.GetCellInfo(iRow, 1);
                DtsBaseDesigner varDesigner = (DtsBaseDesigner)cell.Tag;
                Parameter parameter = (Parameter)varDesigner.GetType().InvokeMember("Parameter", BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, varDesigner, null);
                return parameter;
            }

            private void grid_SelectionChanged(object sender, SelectionChangedEventArgs args)
            {
                try
                {
                    SetButtonEnabled();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, DefaultMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            void grid_Invalidated(object sender, InvalidateEventArgs e)
            {
                //CheckButtonIcons();
            }

            private void SetButtonEnabled()
            {
                //TODO: Why don't we use grid.SelectedRows.Length for variables window too?
                bool enabled = (grid.SelectedRows != null && grid.SelectedRows.Length > 0);
                this.findReferencesButton.Enabled = enabled;
            }
        }
    }


}
