namespace BIDSHelper.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Windows.Forms;
    using EnvDTE;
    
    /// <summary>
    /// Features page dialog for BIDS Helper, as seen under the Visual Studio menu Tools -> Options.
    /// </summary>
    public partial class BIDSHelperOptionsPage : UserControl, EnvDTE.IDTToolsOptionsPage
    {
        /// <summary>
        /// Standard caption for message boxes shown by this options page.
        /// </summary>
        private static string DefaultMessageBoxCaption = "BIDS Helper Options";

        /// <summary>
        /// Initializes a new instance of the <see cref="BIDSHelperOptionsPage"/> class.
        /// </summary>
        public BIDSHelperOptionsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="PropertiesObject">The properties object.</param>
        /// <remarks>
        /// Property can be accessed through the object model with code such as the following macro:
        /// Sub ToolsOptionsPageProperties()
        ///     MsgBox(DTE.Properties("My Category", "My Subcategory - Visual C#").Item("MyProperty").Value)
        ///     DTE.Properties("My Category", "My Subcategory - Visual C#").Item("MyProperty").Value = False
        ///     MsgBox(DTE.Properties("My Category", "My Subcategory - Visual C#").Item("MyProperty").Value)
        /// End Sub
        /// </remarks>
        void IDTToolsOptionsPage.GetProperties(ref object PropertiesObject)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <summary>
        /// Called after the dialog has been created.
        /// </summary>
        /// <param name="DTEObject">The DTE object.</param>
        void IDTToolsOptionsPage.OnAfterCreated(DTE DTEObject)
        {
            // Enumerate plug-ins and create a dynamic properties
            // Must also check for unique features
            CustomClass featuresHost = new CustomClass();
            foreach (BIDSHelperPluginBase plugin in Connect.Plugins.Values)
            {
                CustomProperty parent = featuresHost.Find(plugin.FeatureName);
                if (parent == null)
                {
                    // New feature
                    featuresHost.Add(new CustomProperty(plugin));
                }
                else
                {
                    parent.Children.Add(new CustomProperty(plugin));
                }
            }

            // Assign our dynamic properties collection to the property grid
            this.propertyGridFeatures.SelectedObject = featuresHost;

            // Hide property grid when we have nothing to show, and display 
            // "add-in is not currently enabled" message instead
            propertyGridFeatures.Visible = (featuresHost.Count > 0);
            lblCurrentlyDisabled.Visible = !propertyGridFeatures.Visible;

            // Skip the rest if the add-in is disabled
            if (lblCurrentlyDisabled.Visible)
            {
                return;
            }

            // Set width of property grid columns, make the description larger than default 50%
            FieldInfo fieldInfo = typeof(PropertyGrid).GetField("gridView", BindingFlags.NonPublic | BindingFlags.Instance);
            object gridViewRef = fieldInfo.GetValue(this.propertyGridFeatures);
            Type gridViewType = gridViewRef.GetType();
            MethodInfo methodInfo = gridViewType.GetMethod("MoveSplitterTo", BindingFlags.NonPublic | BindingFlags.Instance);
            int width = (int)(this.propertyGridFeatures.Width * 0.7);
            methodInfo.Invoke(gridViewRef, new object[]{width});

            // Description area size is tool small for 3 lines of text, but fine for 2. 
            // The DocComment Lines or Height approach is too unreliable, need to get 
            // accurate border sizes, DrawString etc. Only a few descriptions that are 
            // too long. Most are only 1 line, so live with it as is.

            // Set default focus to General category, the top of the grid
            // Walk up to the top of the grid tree first
            GridItem item = this.propertyGridFeatures.SelectedGridItem;
            if (item == null)
            {
                return;
            }

            while (item.Parent != null)
            {
                item = item.Parent;
            }

            foreach (GridItem category in item.GridItems)
            {
                if (category.Label == CustomProperty.GetFeatureCategoryLabel(BIDSFeatureCategories.General))
                {
                    if (category.GridItems.Count > 0)
                    {
                        category.GridItems[0].Select();
                    }
                    else
                    {
                        category.Select();
                    }
                }
            }
        }

        /// <summary>
        /// Called when form is canceled.
        /// </summary>
        void IDTToolsOptionsPage.OnCancel()
        {
            // Nothing required
        }

        /// <summary>
        /// Called when help is clicked.
        /// </summary>
        void IDTToolsOptionsPage.OnHelp()
        {
            // First check we are disabled, show info on how to enable 
            if (this.lblCurrentlyDisabled.Visible)
            {
                MessageBox.Show("Add-Ins can be enabled or disabled using the Add-In Manager option on the Tools menu.", DefaultMessageBoxCaption);
            }
            else
            {
                // Check we have a selected item
                GridItem item = this.propertyGridFeatures.SelectedGridItem;
                if (item != null)
                {
                    // Check we have a property
                    if (item.GridItemType == GridItemType.Property)
                    {
                        // Show property specific help...
                        // Get property host/collection, to get the selected property, to access the plugin, to get the url
                        CustomClass featuresHost = propertyGridFeatures.SelectedObject as CustomClass;
                        foreach (CustomProperty property in featuresHost)
                        {
                            if (property.Name == item.Label)
                            {
                                BIDSHelperPluginBase plugin = property.Plugin;
                                string helpUrl = plugin.HelpUrl;
                                if (!string.IsNullOrEmpty(helpUrl))
                                {
                                    OpenUrl(plugin.HelpUrl);
                                }
                                else
                                {
                                    MessageBox.Show("Sorry, no help page is available for this plug-in.", DefaultMessageBoxCaption);
                                }

                                return;
                            }
                        }
                    }
                }
            }

            // Default trap
            MessageBox.Show("Please select an individual feature to access detailed help.", DefaultMessageBoxCaption);
        }

        /// <summary>
        /// Called when the form is closed with OK.
        /// </summary>
        void IDTToolsOptionsPage.OnOK()
        {
            // Get dynamic properties collection
            CustomClass featuresHost = propertyGridFeatures.SelectedObject as CustomClass;
            if (featuresHost == null)
            {
                return;
            }

            EnablePlugins(featuresHost);
        }

        /// <summary>
        /// Enables or disables the plug-ins within a features.
        /// </summary>
        /// <param name="properties">The properties collection hosting the plug-ins.</param>
        private static void EnablePlugins(Collection<CustomProperty> properties)
        {
            if (properties == null)
            {
                return;
            }

            // Commit the changes, enable or disable plug-ins as required
            foreach (CustomProperty featureProperty in properties)
            {
                if (featureProperty.Plugin.Enabled != (bool)featureProperty.Value)
                {
                    featureProperty.Plugin.Enabled = (bool)featureProperty.Value;
                }

                // Enable any child plug-ins, as found when we have features that cover multiple plug-ins
                EnablePlugins(featureProperty.Children);
            }
        }

        /// <summary>
        /// Opens a URL.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = "iexplore.exe";
                process.StartInfo.Arguments = url;
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, DefaultMessageBoxCaption);
            }
        }
    }
}
