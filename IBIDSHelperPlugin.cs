namespace BIDSHelper
{
    public interface IBIDSHelperPlugin
    {
        string FeatureName { get; }
        string FeatureDescription { get; }
        BIDSFeatureCategories FeatureCategory { get; }
        string HelpUrl { get; }
        bool Enabled { get; set; }
    }
}