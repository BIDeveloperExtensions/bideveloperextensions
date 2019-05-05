using BIDSHelper.Core;
using EnvDTE;
using EnvDTE80;

namespace BIDSHelper.SSIS.Biml
{
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public abstract class BimlFeaturePluginBase : BIDSHelperPluginBase
    {
        public BimlFeaturePluginBase(BIDSHelperPackage package)
            : base(package)
        {
        }

        /// <summary>
        /// Gets the feature name as displayed in the enabled features list, previously known as the friendly name.
        /// </summary>
        /// <value>The feature name.</value>
        /// <remarks>
        ///     If not overridden then the ButtonText will be used instead.
        ///     The feature name is the default page title used for by the HelpUrl.
        ///     Using a friendly name accross multiple plug-ins allows you to group commands (each a plug-in) together. The BIML Package Generator feature includes 4 commandfs/plug-ins, Add New File, Expand, Validate and Help.
        /// </remarks>
        public override string FeatureName
        {
            get { return "Biml Package Generator"; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "The Biml Package Generator feature allows you to use the XML-based Biml language to describe your BI solution and generate packages."; }
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }
    }
}