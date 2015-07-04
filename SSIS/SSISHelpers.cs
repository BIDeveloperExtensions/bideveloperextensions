using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Project;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper
{
    public class SSISHelpers
    {
        public enum SsisDesignerTabIndex
        {
            ControlFlow = 0,
            DataFlow = 1,
#if DENALI || SQL2014
            Parameters = 2,
            EventHandlers = 3,
            PackageExplorer = 4
#else
            EventHandlers = 2,
            PackageExplorer = 3
#endif
        }

        public static void MarkPackageDirty(Package package)
        {
            if (package == null) return;
#if DENALI || SQL2014
            var trans = Cud.BeginTransaction(package);
            trans.ChangeComponent(package);
            trans.Commit();
#else
            DesignUtils.MarkPackageDirty(package);
#endif
        }

        public static Package GetPackageFromContainer(DtsContainer container)
        {
            while (!(container is Package))
            {
                container = container.Parent;
            }
            return (Package)container;
        }

        public static bool IsParameterVariable(Variable v)
        {
            return v.Namespace.StartsWith("$");
        }

        //this is only defined in the latest VS2013 OneDesigner, so it won't be available in older versions
        public enum ProjectTargetVersion
        {
            LatestSQLServerVersion = 12,
            SQLServer2012 = 11,
            SQLServer2014 = 12
        }

        public static ProjectTargetVersion? LatestProjectTargetVersion = null;

        /// <summary>
        /// Will return null if the TargetDeploymentVersion property isn't defined (i.e. we're using an older version of SSDTBI before that was introduced)
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static ProjectTargetVersion? GetProjectTargetVersion(EnvDTE.Project project)
        {
            try
            {
                Microsoft.DataWarehouse.Interfaces.IConfigurationSettings settings = (Microsoft.DataWarehouse.Interfaces.IConfigurationSettings)((System.IServiceProvider)project).GetService(typeof(Microsoft.DataWarehouse.Interfaces.IConfigurationSettings));
                Microsoft.DataWarehouse.Project.DataWarehouseProjectManager projectManager = (Microsoft.DataWarehouse.Project.DataWarehouseProjectManager)settings.GetType().InvokeMember("ProjectManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy, null, settings, null);
                Microsoft.DataTransformationServices.Project.DataTransformationsProjectConfigurationOptions options = (Microsoft.DataTransformationServices.Project.DataTransformationsProjectConfigurationOptions)projectManager.ConfigurationManager.CurrentConfiguration.Options;
                object oVersion = options.GetType().InvokeMember("TargetDeploymentVersion", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy, null, options, null);
                LatestProjectTargetVersion = (ProjectTargetVersion)System.Enum.Parse(typeof(ProjectTargetVersion), oVersion.ToString());
                return LatestProjectTargetVersion;
            }
            catch
            {
                LatestProjectTargetVersion = null;
                return null;
            }
        }
    }
}