namespace BIDSHelper.SSIS
{

    extern alias asAlias;
    extern alias sharedDataWarehouseInterfaces;
    extern alias asDataWarehouseInterfaces;

    using Core;
    using EnvDTE;
    using Microsoft.DataTransformationServices.Design;
    using Microsoft.DataWarehouse.Design;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.SqlServer.Management.UI.Grid;
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Forms;

    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public partial class ParametersWindowPlugin : BIDSHelperWindowActivatedPluginBase
    {

        private const string SSIS_DESIGNER_WINDOW_KIND = "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}";

        internal const BindingFlags getPropertyFlags = BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.DeclaredOnly | BindingFlags.Instance;
        internal const BindingFlags getFieldFlags = BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public ParametersWindowPlugin(BIDSHelperPackage package) : base (package)
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
            {
                // If previous checks failed, the window cannot be what we are looking for, as we know the SSIS window has a valid object kind, and mode.
                return;
            }

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
                object obj = null;
                try
                {
                    obj = designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                } catch { }
                if (obj != null)
                {
                    EditorWindow editorWindow = (EditorWindow)obj;
                    if (editorWindow == null)
                        return;

                    editorWindow.SetTagParametersWindowManager();
                }
                else
                {
                    obj = designer.GetService(typeof(asAlias::Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                    EditorWindow editorWindow = (Microsoft.DataWarehouse.Design.EditorWindow)obj;
                    if (editorWindow == null)
                        return;

                    editorWindow.SetTagParametersWindowManager();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, DefaultMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.package.Log.Exception("ParametersWindowPlugin.OnWindowActivate", ex);
            }
        }

        public override string ShortName
        {
            get { return "ParametersWindowPlugin"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
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


        public override void Exec()
        {
        }
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
        private Microsoft.DataTransformationServices.Controls.DlgGridControl grid;
        private EditorWindow editorWindow;
        private bool setupComplete;

        internal ParametersWindowManager(EditorWindow editorWindow)
        {
            editorWindow.ActiveViewChanged += editorWindow_ActiveViewChanged;
        }

        private void editorWindow_ActiveViewChanged(object sender, EventArgs e)
        {
            if (setupComplete)
                return;

            // Get the EditorWindow, Microsoft.DataTransformationServices.Design.DtsPackageView
            editorWindow = sender as EditorWindow;
            
            if (((EditorWindowTag)editorWindow.Tag).ParametersWindowManager != this)
            {
                Debug.Assert(false);
            }

            EditorWindow.EditorView view = editorWindow.SelectedView;
            if (view != null && view.Caption == "Parameters") // Microsoft.DataTransformationServices.Design.SR.PackageParametersViewCaption
            {
                object viewControl = view.GetType().InvokeMember("viewControl", ParametersWindowPlugin.getFieldFlags, null, view, null);
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
            this.findUnusedButton.ToolTipText = "Find Unused Parameters (BIDS Helper)";
            toolbar.Buttons.Add(this.findUnusedButton);
            toolbar.ImageList.Images.Add(BIDSHelper.Resources.Versioned.VariableFindUnused);
            this.findUnusedButton.ImageIndex = toolbar.ImageList.Images.Count - 1;

            toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);
            toolbar.Wrappable = false;

            // "tableLayoutPanelMain" - "tableLayoutPanelParameter" - "parameterGridControl"
            grid = (Microsoft.DataTransformationServices.Controls.DlgGridControl)variablesToolWindowControl.Controls[0].Controls[1].Controls[0];
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

                FindVariableReferences dialog = new FindVariableReferences();
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
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "Parameters Window Extensions", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    public enum VariablesDisplayMode
    {
        Variables,
        PackageParameters,
        ProjectParameters
    }
}
