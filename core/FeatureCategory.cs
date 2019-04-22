using System;

namespace BIDSHelper
{
    /// <summary>
    /// Used in startup of the add-in to determine which features to enable based upon whether SSAS, SSRS, or SSIS extensions are enabled
    /// </summary>
    class FeatureCategory : Attribute
    {
        public FeatureCategory(BIDSFeatureCategories category)
        {
            _category = category;
        }

        protected BIDSFeatureCategories _category;

        public BIDSFeatureCategories Category
        {
            get { return _category; }
            set { _category = value; }
        }
    }
}
