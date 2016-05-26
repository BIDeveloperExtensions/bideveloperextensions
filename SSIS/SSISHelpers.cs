using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Project;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper
{
    public class SSISHelpers
    {

#if SQL2016
        public const string CreationNameIndex = "5";
#elif SQL2014
        public const string CreationNameIndex = "4";
#elif DENALI
        public const string CreationNameIndex = "3";
#endif

        public enum SsisDesignerTabIndex
        {
            ControlFlow = 0,
            DataFlow = 1,
            Parameters = 2,
            EventHandlers = 3,
            PackageExplorer = 4
        }

        // This is only defined in the SQL 2016+ tools, OneDesigner, so it won't be available in older versions
        public enum ProjectTargetVersion
        {
            LatestSQLServerVersion = 12,
            SQLServer2012 = 11,
            SQLServer2014 = 12,
            SQLServer2016 = 12
        }

        public static ProjectTargetVersion? LatestProjectTargetVersion = null;

        public static void MarkPackageDirty(Package package)
        {
            if (package == null) return;

            var trans = Cud.BeginTransaction(package);
            trans.ChangeComponent(package);
            trans.Commit();

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

        internal static DtsContainer FindContainer(DtsContainer component, string objectId)
        {
            if (component == null)
            {
                return null;
            }
            else if (component.ID == objectId)
            {
                return component;
            }

            EventsProvider eventsProvider = component as EventsProvider;
            if (eventsProvider != null)
            {
                foreach (DtsEventHandler eventhandler in eventsProvider.EventHandlers)
                {
                    DtsContainer container = FindContainer(eventhandler, objectId);
                    if (container != null)
                    {
                        return container;
                    }
                }
            }

            IDTSSequence sequence = component as IDTSSequence;
            if (sequence != null)
            {
                foreach (Executable executable in sequence.Executables)
                {
                    DtsContainer container = FindContainer((DtsContainer)executable, objectId);
                    if (container != null)
                    {
                        return container;
                    }
                }
            }

            return null;
        }

        internal static Variable FindVariable(DtsContainer container, string objectID)
        {
            if (container != null)
            {
                if (container.Variables.Contains(objectID))
                {
                    return container.Variables[objectID];
                }
            }

            return null;
        }

        internal static ConnectionManager FindConnectionManager(Package package, string objectID)
        {
            if (package.Connections.Contains(objectID))
            {
                return package.Connections[objectID];
            }

            return null;
        }

        internal static PrecedenceConstraint FindConstraint(DtsContainer container, string objectID)
        {
            IDTSSequence sequence = container as IDTSSequence;

            if (sequence == null)
            {
                System.Diagnostics.Debug.Assert(false, "sequence cannot be found");
                return null;
            }

            if (sequence.PrecedenceConstraints.Contains(objectID))
            {
                return sequence.PrecedenceConstraints[objectID];
            }

            return null;
        }
    }
}