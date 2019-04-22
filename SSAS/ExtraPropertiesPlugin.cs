#if SQL2019
extern alias asAlias;
using asAlias::Microsoft.DataWarehouse.VsIntegration.Designer;
#else
using Microsoft.DataWarehouse.VsIntegration.Designer;
#endif



using System;
using System.Collections.Generic;
using EnvDTE;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASMulti)]
    public class ExtraPropertiesPlugin : BIDSHelperWindowActivatedPluginBase
    {
        private bool bInEffect = true;
//        private const string REGISTRY_EXTENDED_PATH = "ExtraPropertiesPlugin";
        //private const string REGISTRY_SETTING_NAME = "InEffect";

        public ExtraPropertiesPlugin(BIDSHelperPackage package)
            : base(package)
        {

        }

        public override bool ShouldHookWindowCreated { get { return true; } }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                ConfigureProjectItemExtraProperties(GotFocus.ProjectItem);
            }
            catch { }
        }

        //this method is where any future code changes will need to be made
        //the decisions about what extra properties are show are configured here
        void ConfigureProjectItemExtraProperties(ProjectItem pi)
        {
            if (pi == null) return;
            if (pi.Object is Dimension)
            {
                Microsoft.AnalysisServices.Dimension dim = (Microsoft.AnalysisServices.Dimension)pi.Object;
                System.ComponentModel.TypeDescriptor.Refresh(dim);

                SetAttribute(typeof(Dimension), "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(dim, "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                SetAttribute(typeof(Dimension), "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                SetAttribute(dim, "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);

                SetAttribute(typeof(Hierarchy), "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(typeof(Hierarchy), "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                SetAttribute(typeof(Hierarchy), "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);

                SetAttribute(typeof(DimensionAttribute), "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(typeof(DimensionAttribute), "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                SetAttribute(typeof(DimensionAttribute), "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);

                SetAttribute(typeof(Translation), "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(typeof(Translation), "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
            }
            else if (pi.Object is Cube)
            {
                Microsoft.AnalysisServices.Cube cube = (Microsoft.AnalysisServices.Cube)pi.Object;
                System.ComponentModel.TypeDescriptor.Refresh(cube);
                SetAttribute(typeof(Cube), "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(cube, "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(cube, "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                SetAttribute(typeof(Cube), "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);

                SetAttribute(typeof(MeasureGroup), "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(typeof(MeasureGroup), "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                SetAttribute(typeof(MeasureGroup), "Annotations", new System.ComponentModel.ReadOnlyAttribute(false), true);
                SetAttribute(typeof(MeasureGroup), "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);

                SetAttribute(typeof(Measure), "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(typeof(Measure), "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                SetAttribute(typeof(Measure), "Annotations", new System.ComponentModel.ReadOnlyAttribute(false), true);
                SetAttribute(typeof(Measure), "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);

                foreach (MeasureGroup mg in cube.MeasureGroups)
                {
                    if (mg.IsLinked)
                    {
                        SetAttribute(mg, "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                        SetAttribute(mg, "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                        SetAttribute(mg, "Description", new System.ComponentModel.ReadOnlyAttribute(!bInEffect), true);
                        SetAttribute(mg, "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);

                        foreach (Measure m in mg.Measures)
                        {
                            SetAttribute(m, "Description", new System.ComponentModel.ReadOnlyAttribute(!bInEffect), true);
                            SetAttribute(m, "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                            SetAttribute(m, "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                            SetAttribute(m, "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                            SetAttribute(m, "FormatString", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                        }
                    }

                    foreach (Partition p in mg.Partitions)
                    {
                        SetAttribute(p, "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                        SetAttribute(p, "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                    }
                }

                SetAttribute(typeof(Perspective), "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(typeof(Perspective), "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                SetAttribute(typeof(Perspective), "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);

                SetAttribute(typeof(Translation), "Annotations", new System.ComponentModel.BrowsableAttribute(bInEffect), true);
                SetAttribute(typeof(Translation), "Annotations", new System.ComponentModel.EditorAttribute(typeof(AttributeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
                SetAttribute(typeof(Translation), "Description", new System.ComponentModel.EditorAttribute(typeof(MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)), bInEffect);
            }
        }

        public static void SetAttribute(object o, string property, System.Attribute newattrib, bool on)
        {
            System.ComponentModel.MemberDescriptor memb = null;
            if (o is System.Type)
            {
                memb = (System.ComponentModel.MemberDescriptor)System.ComponentModel.TypeDescriptor.GetProperties((Type)o)[property];
            }
            else
            {
                memb = (System.ComponentModel.MemberDescriptor)System.ComponentModel.TypeDescriptor.GetProperties(o)[property];
            }
            InternalSetAttribute(memb, property, newattrib, on);
        }

        private static void InternalSetAttribute(System.ComponentModel.MemberDescriptor memb, string property, System.Attribute newattrib, bool on)
        {
            System.Attribute oldattrib = memb.Attributes[newattrib.GetType()];
            System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
            System.Reflection.BindingFlags setflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
            System.Attribute[] oldattribs = (System.Attribute[])typeof(System.ComponentModel.MemberDescriptor).InvokeMember("attributes", getflags, null, memb, null);
            if (oldattrib != null)
            {
                if (on)
                {
                    for (int i = 0; i < oldattribs.Length; i++)
                    {
                        if (oldattribs[i].GetType().FullName == newattrib.GetType().FullName)
                        {
                            oldattribs[i] = newattrib;
                            break;
                        }
                    }
                    typeof(System.ComponentModel.MemberDescriptor).InvokeMember("attributeCollection", setflags, null, memb, new object[] { new System.ComponentModel.AttributeCollection(oldattribs) });
                    typeof(System.ComponentModel.MemberDescriptor).InvokeMember("originalAttributes", setflags, null, memb, new object[] { oldattribs });
                    if (newattrib is System.ComponentModel.EditorAttribute)
                    {
                        object[] editors = new object[1];
                        System.ComponentModel.EditorAttribute editor = (System.ComponentModel.EditorAttribute)newattrib;
                        editors[0] = Type.GetType(editor.EditorTypeName).GetConstructors()[0].Invoke(new object[] { });
                        typeof(System.ComponentModel.PropertyDescriptor).InvokeMember("editors", setflags, null, memb, new object[] { editors });
                        typeof(System.ComponentModel.PropertyDescriptor).InvokeMember("editorCount", setflags, null, memb, new object[] { 1 });
                        Type[] editorTypes = new Type[1] { Type.GetType(editor.EditorBaseTypeName) };
                        typeof(System.ComponentModel.PropertyDescriptor).InvokeMember("editorTypes", setflags, null, memb, new object[] { editorTypes });
                    }
                }
                else
                {
                    System.Attribute[] newattribs = new System.Attribute[oldattribs.Length - 1];
                    int i = 0;
                    foreach (System.Attribute a in oldattribs)
                    {
                        if (a.GetType().FullName != newattrib.GetType().FullName)
                        {
                            newattribs[i++] = a;
                        }
                    }
                    typeof(System.ComponentModel.MemberDescriptor).InvokeMember("attributes", setflags, null, memb, new object[] { newattribs });
                    typeof(System.ComponentModel.MemberDescriptor).InvokeMember("originalAttributes", setflags, null, memb, new object[] { newattribs });
                    typeof(System.ComponentModel.MemberDescriptor).InvokeMember("attributeCollection", setflags, null, memb, new object[] { new System.ComponentModel.AttributeCollection(newattribs) });
                }
            }
            else if (on)
            {
                System.Attribute[] newattribs = new System.Attribute[oldattribs.Length + 1];
                int i = 0;
                foreach (System.Attribute a in oldattribs)
                {
                    newattribs[i++] = a;
                }
                newattribs[i++] = newattrib;
                typeof(System.ComponentModel.MemberDescriptor).InvokeMember("attributes", setflags, null, memb, new object[] { newattribs });
                typeof(System.ComponentModel.MemberDescriptor).InvokeMember("originalAttributes", setflags, null, memb, new object[] { newattribs });
                typeof(System.ComponentModel.MemberDescriptor).InvokeMember("attributeCollection", setflags, null, memb, new object[] { new System.ComponentModel.AttributeCollection(newattribs) });
            }
        }

        public override string ShortName
        {
            get { return "ExtraProperties"; }
        }

        //public override int Bitmap
        //{
        //    get { return 0; }
        //}

        public override string FeatureName
        {
            get { return "Show Extra Properties"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }
        

        //public override bool Checked
        //{
        //    get { return bInEffect; }
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
            get { return "Exposes hidden properties on several Analysis Services objects, such as the Annotations property. It also provides a better UI for editing descriptions on Analysis Services objects."; }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            foreach (Project p in this.ApplicationObject.Solution.Projects)
            {
                if (p.ProjectItems != null)
                {
                    foreach (ProjectItem pi in p.ProjectItems)
                    {
                        ConfigureProjectItemExtraProperties(pi);
                    }
                }
            }
        }

        public override void Exec()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        //public override void Exec()
        //{
        //    try
        //    {
        //        bInEffect = !bInEffect;
        //        string path = Connect.REGISTRY_BASE_PATH + "\\" + REGISTRY_EXTENDED_PATH;
        //        RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(path, true);
        //        if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(path);
        //        settingKey.SetValue(REGISTRY_SETTING_NAME, bInEffect, RegistryValueKind.DWord);
        //        settingKey.Close();
        //        foreach (Project p in this.ApplicationObject.Solution.Projects)
        //        {
        //            foreach (ProjectItem pi in p.ProjectItems)
        //            {
        //                ConfigureProjectItemExtraProperties(pi);
        //            }
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        bInEffect = !bInEffect;
        //        MessageBox.Show(ex.Message);
        //    }
        //}
    }

    //there were a couple of problems with the default editor Visual Studio would have used to let you edit annotations
    //so here we build our own type editor
    public class AttributeCollectionEditor : UITypeEditor
    {
        private AnnotationCollection oldAnnotations;
        private List<Annotation> microsoftAnnotations;
        private List<Annotation> editedAnnotations;
        private Form form;
        private ComboBox combo;
        private TextBox textName;
        private TextBox textValue;
        private Button deleteButton;
        private CheckBox check;

        public AttributeCollectionEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            oldAnnotations = new AnnotationCollection();
            microsoftAnnotations = new List<Annotation>();
            editedAnnotations = new List<Annotation>();
            object returnValue = value;
            try
            {
                if ((provider != null) && (((IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService))) != null))
                {
                    ((AnnotationCollection)value).CopyTo(oldAnnotations);

                    form = new Form();
                    form.Icon = BIDSHelper.Resources.Common.BIDSHelper;
                    form.Text = "BIDS Helper Attributes Editor";
                    form.MaximizeBox = true;
                    form.MinimizeBox = false;
                    form.Width = 550;
                    form.Height = 400;
                    form.SizeGripStyle = SizeGripStyle.Show;
                    form.MinimumSize = new System.Drawing.Size(form.Width,form.Height);

                    check = new CheckBox();
                    check.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    check.Text = "Hide Microsoft Annotations?";
                    check.Click += new EventHandler(check_Click);
                    check.Width = 160;
                    check.Left = 5;
                    check.Checked = true;
                    form.Controls.Add(check);

                    Label labelAnnotation = new Label();
                    labelAnnotation.Text = "Annotation:";
                    labelAnnotation.Top = 25;
                    labelAnnotation.Left = 5;
                    labelAnnotation.Width = 65;
                    labelAnnotation.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    labelAnnotation.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    form.Controls.Add(labelAnnotation);

                    combo = new ComboBox();
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    combo.Width = form.Width - 40 - labelAnnotation.Width;
                    combo.Left = labelAnnotation.Right + 5;
                    combo.Top = 25;
                    combo.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    combo.SelectedIndexChanged += new EventHandler(combo_SelectedIndexChanged);
                    form.Controls.Add(combo);

                    foreach (Annotation a in oldAnnotations)
                    {
                        if (!a.Name.StartsWith("http://schemas.microsoft.com"))
                        {
                            combo.Items.Add(a.Name);
                            editedAnnotations.Add(a);
                        }
                        else
                        {
                            microsoftAnnotations.Add(a);
                        }
                    }
                    combo.Items.Add("<Add New Annotation>");
                    combo.SelectedIndex = -1;

                    check.Left = combo.Left;

                    Label labelName = new Label();
                    labelName.Text = "Name:";
                    labelName.Top = 50;
                    labelName.Left = 5;
                    labelName.Width = 65;
                    labelName.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    labelName.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    form.Controls.Add(labelName);

                    textName = new TextBox();
                    textName.Text = "";
                    textName.Top = labelName.Top;
                    textName.Left = labelName.Right + 5;
                    textName.Width = combo.Width;
                    textName.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    textName.Enabled = false;
                    textName.Leave += new EventHandler(textName_Leave);
                    form.Controls.Add(textName);

                    Label labelValue = new Label();
                    labelValue.Text = "Value:";
                    labelValue.Top = 75;
                    labelValue.Left = 5;
                    labelValue.Width = 65;
                    labelValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    labelValue.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    form.Controls.Add(labelValue);

                    textValue = new TextBox();
                    textValue.Text = "";
                    textValue.Top = labelValue.Top;
                    textValue.Left = labelName.Right + 5;
                    textValue.Width = combo.Width;
                    textValue.ScrollBars = ScrollBars.Vertical;
                    textValue.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                    textValue.Enabled = false;
                    textValue.Multiline = true;
                    textValue.Height = form.Height - 85 - textValue.Top;
                    textValue.Leave += new EventHandler(textValue_Leave);
                    form.Controls.Add(textValue);

                    Button okButton = new Button();
                    okButton.Text = "OK";
                    okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    okButton.Left = form.Right - okButton.Width * 2 - 40;
                    okButton.Top = form.Bottom - okButton.Height*2 - 20;
                    okButton.Click += new EventHandler(okButton_Click);
                    //form.AcceptButton = okButton; //don't want enter to cause this window to close because of multiline value textbox
                    form.Controls.Add(okButton);

                    Button cancelButton = new Button();
                    cancelButton.Text = "Cancel";
                    cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    cancelButton.Left = okButton.Right + 10;
                    cancelButton.Top = okButton.Top;
                    form.CancelButton = cancelButton;
                    form.Controls.Add(cancelButton);

                    deleteButton = new Button();
                    deleteButton.Text = "Delete Annotation";
                    deleteButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                    deleteButton.Left = textValue.Left;
                    deleteButton.Top = okButton.Top;
                    deleteButton.Width += 30;
                    deleteButton.Enabled = false;
                    deleteButton.Click += new EventHandler(deleteButton_Click);
                    form.Controls.Add(deleteButton);

                    DialogResult result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        AnnotationCollection coll = new AnnotationCollection();
                        foreach (Annotation a in this.microsoftAnnotations)
                            coll.Add(a.Clone());
                        foreach (Annotation a in this.editedAnnotations)
                            coll.Add(a.Clone());

                        DesignerTransaction transaction1 = null;
                        try
                        {
                            IDesignerHost host1 = (IDesignerHost)provider.GetService(typeof(IDesignerHost));
                            transaction1 = host1.CreateTransaction("BidsHelperAnnotationCollectionEditorUndoBatchDesc");
                            IComponentChangeService service1 = (IComponentChangeService)context.GetService(typeof(IComponentChangeService));
                            NamedCustomTypeDescriptor instance = (NamedCustomTypeDescriptor)context.Instance;
                            ModelComponent m = (ModelComponent)instance.BrowsableObject; //use ModelComponent from which Cube, Dimension, Measure, etc. are derived... ModelComponent contains the definition for Annotations
                            service1.OnComponentChanging(m, null);
                            m.Annotations.Clear();
                            coll.CopyTo(m.Annotations);
                            service1.OnComponentChanged(m, null, null, null);
                            returnValue = coll;
                            return returnValue;
                        }
                        catch (CheckoutException exception1)
                        {
                            if (transaction1 != null)
                                transaction1.Cancel();
                            if (exception1 != CheckoutException.Canceled)
                            {
                                throw exception1;
                            }
                            return returnValue;
                        }
                        finally
                        {
                            if (transaction1 != null)
                                transaction1.Commit();
                        }
                    }
                    else
                    {
                        returnValue = oldAnnotations;
                    }
                    form.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return returnValue;
        }

        void textValue_Leave(object sender, EventArgs e)
        {
            editedAnnotations[combo.SelectedIndex].Value.Value = textValue.Text;
        }

        void textName_Leave(object sender, EventArgs e)
        {
            for (int i = 0; i < combo.Items.Count; i++) {
                if (i != combo.SelectedIndex)
                {
                    if (combo.Items[i].ToString() == textName.Text)
                    {
                        MessageBox.Show("An annotation with that name already exists.");
                        textName.Focus();
                        return;
                    }
                }
            }
            editedAnnotations[combo.SelectedIndex] = new Annotation(textName.Text, textValue.Text);
            combo.Items[combo.SelectedIndex] = textName.Text;
        }

        void deleteButton_Click(object sender, EventArgs e)
        {
            editedAnnotations.RemoveAt(combo.SelectedIndex);
            combo.Items.RemoveAt(combo.SelectedIndex);
            if (combo.Items.Count == 1)
            {
                combo.SelectedIndex = -1;
                combo_SelectedIndexChanged(sender, e);
            }
            else
            {
                combo.SelectedIndex = 0;
            }
        }

        void okButton_Click(object sender, EventArgs e)
        {
            foreach (Annotation a in editedAnnotations)
            {
                if (String.IsNullOrEmpty(a.Name))
                {
                    MessageBox.Show("No annotation can have a blank name");
                    return;
                }
            }
            form.DialogResult = DialogResult.OK;
            form.Close();
        }

        void combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combo.SelectedIndex + 1 == combo.Items.Count)
            {
                //add a new annotation
                combo.Items.Insert(combo.SelectedIndex, "");
                combo.SelectedIndex--;
                textName.Focus();
            }
            else if (combo.SelectedIndex != -1)
            {
                //edit an existing annotation
                Annotation a;
                if (combo.SelectedIndex < this.editedAnnotations.Count)
                {
                    a = this.editedAnnotations[combo.SelectedIndex];
                }
                else
                {
                    a = new Annotation();
                    editedAnnotations.Add(a);
                }
                if (a.Name == null || !a.Name.StartsWith("http://schemas.microsoft.com"))
                {
                    textName.Enabled = true;
                    textValue.Enabled = true;
                    deleteButton.Enabled = true;
                }
                else
                {
                    textName.Enabled = false;
                    textValue.Enabled = false;
                    deleteButton.Enabled = false;
                }
                System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                textName.Text = a.Name ?? "";
                try
                {
                    textValue.Text = (string)a.GetType().InvokeMember("TextValue", getflags, null, a, null);
                }
                catch
                {
                    textValue.Text = (a.Value != null ? a.Value.OuterXml : "");
                }
            }
            else
            {
                textName.Enabled = false;
                textName.Text = "";
                textValue.Enabled = false;
                textValue.Text = "";
                deleteButton.Enabled = false;
            }
        }

        void check_Click(object sender, EventArgs e)
        {
            combo.SuspendLayout();
            List<Annotation> currentEdited = new List<Annotation>();
            currentEdited.AddRange(microsoftAnnotations);
            currentEdited.AddRange(editedAnnotations);
            editedAnnotations.Clear();
            microsoftAnnotations.Clear();
            combo.Items.Clear();
            foreach (Annotation a in currentEdited)
            {
                if (a.Name == null) continue;
                if (!a.Name.StartsWith("http://schemas.microsoft.com") || !check.Checked)
                {
                    combo.Items.Add(a.Name);
                    editedAnnotations.Add(a);
                }
                else
                {
                    microsoftAnnotations.Add(a);
                }
            }
            combo.Items.Add("<Add New Annotation>");
            combo.SelectedIndex = -1;
            combo_SelectedIndexChanged(sender, e);
            combo.ResumeLayout();
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
