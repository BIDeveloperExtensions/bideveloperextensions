using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DataWarehouse.Design;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.DataTransformationServices.Design;


namespace BIDSHelper
{
    class SSISHelpers
    {
        public static void MarkPackageDirty(Package package)
        {
            if (package == null) return;
            #if DENALI
            Cud.Transaction trans = Cud.BeginTransaction(package);
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

        public enum SsisDesignerTabIndex
        {
            ControlFlow = 0,
            DataFlow = 1,
#if DENALI
            Parameters = 2,
            EventHandlers = 3,
            PackageExplorer = 4
#else
            EventHandlers = 2,
            PackageExplorer = 3
#endif
        }

    }
}
