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
            #if DENALI
            Cud.Transaction trans = Cud.BeginTransaction(package);
            trans.ChangeComponent(package);
            trans.Commit();
            #else
            DesignUtils.MarkPackageDirty(package);
            #endif
        }
    }
}
