namespace BIDSHelper.Core
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Custom dynamic property, for use with <see cref="BIDSHelper.Core.CustomClass"/>
    /// </summary>
    public class CustomProperty
    {
        private string name = string.Empty;
        private string description = string.Empty;
        private string category = string.Empty;
        private bool readOnly = false;
        private bool visible = true;
        private object objValue = null;
        private Type type;
        private Collection<CustomProperty> childern;

        internal BIDSHelperPluginBase Plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomProperty"/> class, using a <see cref="BIDSHelperPluginBase"/> instance.
        /// </summary>
        /// <param name="plugin">The BIDS Helper plugin to represent as a feature enabled property.</param>
        public CustomProperty(BIDSHelperPluginBase plugin)
        {
            this.Plugin = plugin;
            this.name = plugin.FeatureName;
            this.description = plugin.FeatureDescription;
            this.objValue = plugin.Enabled;
            this.type = typeof(bool);
            this.readOnly = false;
            this.visible = true;
            this.category = GetFeatureCategoryLabel(plugin.FeatureCategory);
            this.childern = new Collection<CustomProperty>();

#if DEBUG
            // Write out list of all plugins as we create the property collection, 
            // used for easy spell checking
            System.Diagnostics.Debug.WriteLine(this.name);
            System.Diagnostics.Debug.WriteLine(this.description);
            System.Diagnostics.Debug.WriteLine(string.Empty);
#endif
        }

        internal static string GetFeatureCategoryLabel(BIDSFeatureCategories featureCategory)
        {
            // Decode enum into catgeory, just in one place, so easy to localise 
            // if we want to in the future
            switch (featureCategory)
            {
                case BIDSFeatureCategories.General:
                    return "General";
                case BIDSFeatureCategories.SSAS:
                    return "SQL Server Analysis Services (SSAS)";
                case BIDSFeatureCategories.SSIS:
                    return "SQL Server Integration Services (SSIS)";
                case BIDSFeatureCategories.SSRS:
                    return "SQL Server Reporting Services (SSRS)";
            }

            throw new ArgumentOutOfRangeException();            
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets a value indicating whether property is read only.
        /// </summary>
        /// <value><c>true</c> if property is read only; otherwise, <c>false</c>.</value>
        public bool ReadOnly
        {
            get
            {
                return readOnly;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                return description;
            }
        }

        /// <summary>
        /// Gets the catgeory.
        /// </summary>
        /// <value>The category.</value>
        public string Category
        {
            get
            {
                return category;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the property is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public bool Visible
        {
            get
            {
                return this.visible;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get
            {
                return objValue;
            }
            set
            {
                objValue = value;

                // Cascade setting to children
                foreach (CustomProperty child in this.childern)
                {
                    child.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the children collection.
        /// </summary>
        /// <value>The children.</value>
        public Collection<CustomProperty> Children
        {
            get
            {
                return this.childern;
            }
            set
            {
                this.childern = value;
            }
        }
    }
}
