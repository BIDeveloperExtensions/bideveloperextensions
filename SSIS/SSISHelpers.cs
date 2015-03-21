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
            return (Package) container;
        }

        public static bool IsParameterVariable(Variable v)
        {
            return v.Namespace.StartsWith("$");
        }
    }
}