#if SQL2019
extern alias asAlias;
extern alias sharedDataWarehouseInterfaces;
extern alias asDataWarehouseInterfaces;
using asAlias::Microsoft.DataWarehouse.Design;
using asAlias::Microsoft.DataWarehouse.Controls;
using sharedDataWarehouseInterfaces::Microsoft.DataWarehouse.Design;
using asAlias::Microsoft.AnalysisServices.Design;
using asAlias::Microsoft.DataWarehouse.ComponentModel;
#else
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using Microsoft.AnalysisServices.Design;
using Microsoft.DataWarehouse.ComponentModel;
#endif

//using Microsoft.DataWarehouse.Design;
//using Microsoft.DataWarehouse.Controls;

using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.ComponentModel.Design;
using System;
using Microsoft.Win32;
using BIDSHelper.SSAS;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class CalcHelpersPlugin : BIDSHelperWindowActivatedPluginBase
    {
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.Dictionary<string,EditorWindow> windowHandlesFixedForCalcProperties = new System.Collections.Generic.Dictionary<string,EditorWindow>();
        private System.Collections.Generic.Dictionary<string,EditorWindow> windowHandlesFixedDefaultCalcScriptView = new System.Collections.Generic.Dictionary<string,EditorWindow>();
        
        private ToolBarButton newCalcPropButton = null;
        private ToolBarButton newDeployMdxScriptButton = null;

        private const string REGISTRY_EXTENDED_PATH = "CalcHelpersPlugin";
        private const string REGISTRY_SCRIPT_VIEW_SETTING_NAME = "CalcScriptDefaultView";

        public CalcHelpersPlugin(BIDSHelperPackage package)
            : base(package)
        {
        }

        public override void OnDisable()
        {
            base.OnDisable();
            foreach (EditorWindow win in windowHandlesFixedDefaultCalcScriptView.Values)
            {
                //win.ActiveViewChanged -= win_ActiveViewChanged;            
                // get toolbar and remove click handlers

            }

            foreach (EditorWindow win in windowHandlesFixedForCalcProperties.Values)
            {
                win.ActiveViewChanged -= win_ActiveViewChanged;
                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);
                if (toolbar != null)
                {
                    toolbar.Click -= toolbar_Click;
                    toolbar.ButtonClick -= toolbar_ButtonClick;

                    if (newCalcPropButton != null)
                    {
                        if (toolbar.Buttons.ContainsKey(newCalcPropButton.Name))
                            { toolbar.Buttons.RemoveByKey(newCalcPropButton.Name);}

                        // make the standard properties button visible
                        foreach (ToolBarButton b in toolbar.Buttons)
                        {
                            if (b.ToolTipText.StartsWith("Calculation Properties"))
                            {
                                if (b.Tag == null || b.Tag.ToString() != this.FullName + ".CommandProperties")
                                {
                                    b.Visible = true;
                                }
                            }
                        }

                    }

                    if (newDeployMdxScriptButton != null)
                    {
                        if (toolbar.Buttons.ContainsKey(newDeployMdxScriptButton.Name))
                            {toolbar.Buttons.RemoveByKey(newDeployMdxScriptButton.Name);}
                    }
                }

            }

        }

        public override bool ShouldHookWindowCreated
        {
            get
            { return true; }
        }

        void windowEvents_WindowCreated(Window Window)
        {
            OnWindowActivated(Window, null);
        }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                package.Log.Debug("CalcHelpersPlugin OnWindowActivated Fired");
                if (GotFocus == null) return;
                IDesignerHost designer = GotFocus.Object as IDesignerHost;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if ((pi==null) || (!(pi.Object is Cube))) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(IComponentNavigator));
                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);

                IntPtr ptr = win.Handle;
                string sHandle = ptr.ToInt64().ToString();

                if (!windowHandlesFixedForCalcProperties.ContainsKey(sHandle))
                {
                    windowHandlesFixedForCalcProperties.Add(sHandle,win);
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                }

                if (win.SelectedView.MenuItemCommandID.ID == (int)BIDSViewMenuItemCommandID.Calculations)
                //if (win.SelectedView.Caption == "Calculations")
                {

                    int iMicrosoftCalcPropertiesIndex = 0;
                    bool bFlipScriptViewButton = false;
                    foreach (ToolBarButton b in toolbar.Buttons)
                    {
                        MenuCommandToolBarButton tbb = b as MenuCommandToolBarButton;
                        if (tbb != null && tbb.AssociatedCommandID.ID == (int)BIDSToolbarButtonID.CalculationProperties)
                        //if (b.ToolTipText.StartsWith("Calculation Properties"))
                        {
                            if (b.Tag == null || b.Tag.ToString() != this.FullName + ".CommandProperties")
                            {
                                if (!toolbar.Buttons.ContainsKey(this.FullName + ".CommandProperties"))
                                {
                                    //if we haven't created it yet
                                    iMicrosoftCalcPropertiesIndex = toolbar.Buttons.IndexOf(b);
                                    b.Visible = false;

                                    newCalcPropButton = new ToolBarButton();
                                    newCalcPropButton.ToolTipText = "Calculation Properties (BIDS Helper)";
                                    newCalcPropButton.Name = this.FullName + ".CommandProperties";
                                    newCalcPropButton.Tag = newCalcPropButton.Name;

                                    newCalcPropButton.ImageIndex = 12;
                                    newCalcPropButton.Enabled = true;
                                    newCalcPropButton.Style = ToolBarButtonStyle.PushButton;

                                    toolbar.ImageList.Images.Add(BIDSHelper.Resources.Common.DeployMdxScriptIcon);

                                    if (pi.Name.ToLower().EndsWith(".cube")) //only show feature if we're in offline mode
                                    {
                                        // TODO - does not disable if Deploy plugin is disabled after the button has been added
                                        if (BIDSHelperPackage.Plugins[DeployMdxScriptPlugin.BaseName + typeof(DeployMdxScriptPlugin).Name].Enabled)
                                        {
                                            newDeployMdxScriptButton = new ToolBarButton();
                                            newDeployMdxScriptButton.ToolTipText = "Deploy MDX Script (BIDS Helper)";
                                            newDeployMdxScriptButton.Name = this.FullName + ".DeployMdxScript";
                                            newDeployMdxScriptButton.Tag = newDeployMdxScriptButton.Name;
                                            newDeployMdxScriptButton.ImageIndex = toolbar.ImageList.Images.Count - 1;
                                            newDeployMdxScriptButton.Enabled = true;
                                            newDeployMdxScriptButton.Style = ToolBarButtonStyle.PushButton;
                                        }
                                    }

                                    //catch the button clicks of the new buttons we just added
                                    toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);

                                    //catch the mouse clicks... the only way to catch the button click for the Microsoft buttons
                                    toolbar.Click += new EventHandler(toolbar_Click);
                                }
                            }
                        }
                        else if (tbb != null && tbb.AssociatedCommandID.ID == 12854 && ScriptViewDefault && !windowHandlesFixedDefaultCalcScriptView.ContainsKey(sHandle)) 
                        //else if (b.ToolTipText == "Form View" && ScriptViewDefault && !windowHandlesFixedDefaultCalcScriptView.ContainsKey(sHandle)) //12854
                        {
                            Control control = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null);
                            System.Reflection.BindingFlags getfieldflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                            object controlMgr = control.GetType().InvokeMember("calcControlMgr", getfieldflags, null, control, null);
                            System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                            controlMgr.GetType().InvokeMember("ViewScript", getmethodflags, null, controlMgr, new object[] { });
                            bFlipScriptViewButton = true;
                            b.Pushed = false;
                            windowHandlesFixedDefaultCalcScriptView.Add(sHandle,win);
                        }
                        else if (tbb != null && tbb.AssociatedCommandID.ID == (int)BIDSHelper.BIDSToolbarButtonID.ScriptView  && bFlipScriptViewButton) //12853
                        //else if (b.ToolTipText == "Script View" && bFlipScriptViewButton) //12853
                        {
                            b.Pushed = true;
                        }
                    }
                    if (newDeployMdxScriptButton != null && !toolbar.Buttons.Contains(newDeployMdxScriptButton))
                    {
                        toolbar.Buttons.Insert(iMicrosoftCalcPropertiesIndex, newDeployMdxScriptButton);
                    }
                    if (newCalcPropButton != null && !toolbar.Buttons.Contains(newCalcPropButton))
                    {
                        toolbar.Buttons.Insert(iMicrosoftCalcPropertiesIndex, newCalcPropButton);
                    }
                }
            }
            catch (Exception ex){
                package.Log.Error("CalcHelpersPlugin Exception: " + ex.Message);
            }
        }

        private void DeployMdxScript()
        {
            IDesignerHost designer = (IDesignerHost)ApplicationObject.ActiveWindow.Object;
            if (designer == null) return;
            ProjectItem pi = ApplicationObject.ActiveWindow.ProjectItem;
            EditorWindow win = (EditorWindow)designer.GetService(typeof(IComponentNavigator));
            
            if (win.SelectedView.MenuItemCommandID.ID == (int)BIDSViewMenuItemCommandID.Calculations)
            //if (win.SelectedView.Caption == "Calculations")
            {
                DeployMdxScriptPlugin.Instance.DeployScript(pi, this.ApplicationObject);
            }
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
        }

        public bool ScriptViewDefault
        {
            get
            {
                bool bScriptViewDefault = false;
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(this.PluginRegistryPath);
                if (rk != null)
                {   
                    bScriptViewDefault = (1 == (int)rk.GetValue(REGISTRY_SCRIPT_VIEW_SETTING_NAME, 0));
                    rk.Close();
                }
                return bScriptViewDefault;
            }
            set
            {
                RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(this.PluginRegistryPath, true);
                if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(this.PluginRegistryPath);
                settingKey.SetValue(REGISTRY_SCRIPT_VIEW_SETTING_NAME, value, RegistryValueKind.DWord);
                settingKey.Close();
            }
        }

        void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button.Tag != null)
                {
                    string sButtonTag = e.Button.Tag.ToString();
                    if (sButtonTag == this.FullName + ".CommandProperties")
                    {
                        OpenCalcPropertiesDialog();
                    }
                    else if (sButtonTag == this.FullName + ".DeployMdxScript")
                    {
                        DeployMdxScript();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void toolbar_Click(object sender, EventArgs e)
        {
            try
            {
                ToolBar toolbar = (ToolBar)sender;
                System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                int hotItem = (int)typeof(ToolBar).InvokeMember("hotItem", getflags, null, toolbar, null);
                ToolBarButton button = toolbar.Buttons[hotItem];
                //if (button.ToolTipText == "Script View")
                MenuCommandToolBarButton menubutton = button as MenuCommandToolBarButton;
                if (menubutton == null) return;
                if (menubutton.AssociatedCommandID.ID == (int)BIDSToolbarButtonID.ScriptView)
                {
                    ScriptViewDefault = true;
                }
                else if (menubutton.AssociatedCommandID.ID == (int)BIDSToolbarButtonID.FormView)//if (button.ToolTipText == "Form View")
                {
                    ScriptViewDefault = false;
                }
            }
            catch { }
        }

        void OpenCalcPropertiesDialog()
        {
            Form form1 = null; //should be a CalcPropertiesEditorForm which is private
            Cube cube = (Cube)this.ApplicationObject.ActiveWindow.ProjectItem.Object;
            System.IServiceProvider provider = (System.IServiceProvider)this.ApplicationObject.ActiveWindow.ProjectItem.ContainingProject;
            using (WaitCursor cursor1 = new WaitCursor())
            {
                try
                {
                    IUserPromptService oService = (IUserPromptService)provider.GetService(typeof(IUserPromptService));

                    foreach (Type t in System.Reflection.Assembly.GetAssembly(typeof(Scripts)).GetTypes())
                    {
                        if (t.FullName == "Microsoft.AnalysisServices.Design.Calculations.CalcPropertiesEditorForm")
                        {
                            form1 = (Form)t.GetConstructor(new Type[] { typeof(IUserPromptService) }).Invoke(new object[] { oService });
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    asDataWarehouseInterfaces::Microsoft.DataWarehouse.Design.IUserPromptService oService = (asDataWarehouseInterfaces::Microsoft.DataWarehouse.Design.IUserPromptService)provider.GetService(typeof(asDataWarehouseInterfaces::Microsoft.DataWarehouse.Design.IUserPromptService));

                    foreach (Type t in System.Reflection.Assembly.GetAssembly(typeof(Scripts)).GetTypes())
                    {
                        if (t.FullName == "Microsoft.AnalysisServices.Design.Calculations.CalcPropertiesEditorForm")
                        {
                            form1 = (Form)t.GetConstructor(new Type[] { typeof(asDataWarehouseInterfaces::Microsoft.DataWarehouse.Design.IUserPromptService) }).Invoke(new object[] { oService });
                            break;
                        }
                    }
                }
                if (form1 == null) throw new Exception("Couldn't create instance of CalcPropertiesEditorForm");

                object script1 = null; //should be a Microsoft.AnalysisServices.MdxCodeDom.MdxCodeScript object
                try
                {
                    //validate the script because deploying an invalid script makes cube unusable
                    Scripts scripts = new Scripts(cube);
                    System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                    script1 = scripts.GetType().InvokeMember("mdxCodeScript", getflags, null, scripts, null);
                }
                catch (Microsoft.AnalysisServices.Design.ScriptParsingFailed ex)
                {
                    string throwaway = ex.Message; //prevents a warning during compile
                    MessageBox.Show("MDX Script in " + cube.Name + " is not valid.", "Problem Deploying MDX Script");
                    return;
                }

                if (cube.MdxScripts.Count == 0)
                {
                    MessageBox.Show("There is no MDX script defined in this cube yet.");
                    return;
                }

                System.Reflection.BindingFlags getmethodflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                form1.GetType().InvokeMember("Initialize", getmethodflags, null, form1, new object[] { cube.MdxScripts[0], script1, cube, null });

                //now make custom changes to the form
                Button okButton = (Button)form1.Controls.Find("okButton", true)[0];
                Panel panel = (Panel)form1.Controls.Find("gridPanel", true)[0];
                
                Button descButton = new Button();
                descButton.Text = "Edit Description";
                descButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                descButton.Left = panel.Left;
                descButton.Top = okButton.Top;
                descButton.Width += 40;
                descButton.Click += new EventHandler(descButton_Click);
                form1.Controls.Add(descButton);
            }

            if (Microsoft.DataWarehouse.DataWarehouseUtilities.ShowDialog(form1,provider) == DialogResult.OK)
            {
                using (WaitCursor cursor2 = new WaitCursor())
                {
                    System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                    CalculationPropertyCollection collection1 = (CalculationPropertyCollection)form1.GetType().InvokeMember("GetResultProperties", getflags, null, form1, new object[] { });

                    DesignerTransaction transaction1 = null;
                    try
                    {
                        IDesignerHost host1 = (IDesignerHost)ApplicationObject.ActiveWindow.Object;
                        transaction1 = host1.CreateTransaction("BidsHelperCalcPropertiesUndoBatchDesc");
                        IComponentChangeService service1 = (IComponentChangeService)ApplicationObject.ActiveWindow.Object;
                        service1.OnComponentChanging(cube.MdxScripts[0].CalculationProperties, null);
                        cube.MdxScripts[0].CalculationProperties.Clear();
                        for (int num1 = collection1.Count - 1; num1 >= 0; num1--)
                        {
                            CalculationProperty property1 = collection1[num1];
                            collection1.RemoveAt(num1);
                            cube.MdxScripts[0].CalculationProperties.Insert(0, property1);
                        }
                        service1.OnComponentChanged(cube.MdxScripts[0].CalculationProperties, null, null, null);
                    }
                    catch (CheckoutException exception1)
                    {
                        if (transaction1 != null)
                            transaction1.Cancel();
                        if (exception1 != CheckoutException.Canceled)
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        if (transaction1 != null)
                            transaction1.Commit();
                    }
                }
            }
        }

        void descButton_Click(object sender, EventArgs e)
        {
            try
            {
                Button b = (Button)sender;
                Form form = b.FindForm();
                Control grid = (Control)form.Controls.Find("grid", true)[0];
                System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                object selectionMgr = grid.GetType().BaseType.BaseType.InvokeMember("m_selMgr", getflags, null, grid, new object[] { });
                long currentRow = (long)selectionMgr.GetType().InvokeMember("m_curRowIndex", getflags, null, selectionMgr, new object[] { });
                getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                bool singleRowSelected = (bool)selectionMgr.GetType().InvokeMember("SingleRowOrColumnSelectedInMultiSelectionMode", getflags, null, selectionMgr, new object[] { });
                if (!singleRowSelected)
                {
                    MessageBox.Show("Only one description can be edited at a time.");
                    return;
                }
                getflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                CalculationPropertyCollection calcs = (CalculationPropertyCollection)form.GetType().InvokeMember("GetResultProperties", getflags, null, form, new object[] { });
                if (currentRow >= 0 && currentRow < calcs.Count)
                {
                    string sDesc = calcs[(int)currentRow].Description;
                    if (sDesc != null)
                    {
                        //normalize line breaks so they display well in the textbox
                        sDesc = sDesc.Replace("\r\n", "\r").Replace("\n", "\r").Replace("\r", "\r\n");
                    }

                    Form descForm = new Form();
                    descForm.Icon = BIDSHelper.Resources.Common.BIDSHelper;
                    descForm.Text = "BIDS Helper Description Editor";
                    descForm.MaximizeBox = true;
                    descForm.MinimizeBox = false;
                    descForm.Width = 350;
                    descForm.Height = 300;
                    descForm.SizeGripStyle = SizeGripStyle.Show;
                    descForm.MinimumSize = new System.Drawing.Size(descForm.Width, descForm.Height);

                    TextBox textValue = new TextBox();
                    textValue.Text = sDesc;
                    textValue.Top = 10;
                    textValue.Left = 10;
                    textValue.Width = descForm.Width - 30;
                    textValue.ScrollBars = ScrollBars.Vertical;
                    textValue.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                    textValue.Multiline = true;
                    textValue.Height = descForm.Height - 95;
                    descForm.Controls.Add(textValue);

                    Button okButton = new Button();
                    okButton.Text = "OK";
                    okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    okButton.Left = descForm.Right - okButton.Width * 2 - 40;
                    okButton.Top = descForm.Bottom - okButton.Height * 2 - 20;
                    //descForm.AcceptButton = okButton; //don't want enter to cause this window to close because of multiline value textbox
                    okButton.Click += new EventHandler(okButton_Click);
                    descForm.Controls.Add(okButton);

                    Button cancelButton = new Button();
                    cancelButton.Text = "Cancel";
                    cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    cancelButton.Left = okButton.Right + 10;
                    cancelButton.Top = okButton.Top;
                    descForm.CancelButton = cancelButton;
                    descForm.Controls.Add(cancelButton);

                    DialogResult result = descForm.ShowDialog(form);
                    if (result == DialogResult.OK)
                    {
                        calcs[(int)currentRow].Description = textValue.Text;
                    }

                    descForm.Dispose();
                    
                }
                else
                {
                    MessageBox.Show("You have selected an invalid row.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                Form form = (Form)((Button)sender).FindForm();
                form.DialogResult = DialogResult.OK;
                form.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public override string ShortName
        {
            get { return "CalcHelpers"; }
        }

        //public override int Bitmap
        //{
        //    get { return 0; }
        //}

        public override string FeatureName
        {
            get { return "Calculation Helpers"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        //public override string MenuName
        //{
        //    get { return string.Empty; } //no need to have a menu command
        //}

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASMulti; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Extends the Calculations tab of the cube editor, including an enhanced Calculation Properties dialog."; }
        }

        public override void Exec()
        {
        }
    }
}