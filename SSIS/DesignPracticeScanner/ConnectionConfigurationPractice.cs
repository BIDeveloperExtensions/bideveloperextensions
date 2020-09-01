extern alias sharedDataWarehouseInterfaces;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using EnvDTE;

namespace BIDSHelper.SSIS.DesignPracticeScanner
{
    class ConnectionConfigurationsPractice : DesignPractice
    {
        public ConnectionConfigurationsPractice(string registryPath) : base(registryPath)
        {
            base.Name = "Connection Configurations";
            base.Description = "Checks to see if all connection strings have configurations set";
        }

        public override void Check(Package package, EnvDTE.ProjectItem projectItem)
        {
            Results.Clear();
            string sVisualStudioRelativePath = projectItem.DTE.FullName.Substring(0, projectItem.DTE.FullName.LastIndexOf('\\') + 1);

            bool bOfflineMode = projectItem.ContainingProject.GetOfflineMode();



            List<string> configPaths = new List<string>(package.Configurations.Count);
            foreach (Microsoft.SqlServer.Dts.Runtime.Configuration c in package.Configurations)
            {
                foreach (PackageConfigurationSetting setting in PackageConfigurationLoader.GetPackageConfigurationSettings(c, package, sVisualStudioRelativePath, bOfflineMode))
                {
                    configPaths.Add(setting.Path);
                }

            }

            foreach (ConnectionManager cm in package.Connections)
            {
                DtsProperty prop = cm.Properties["ConnectionString"];
                string sPackagePath = prop.GetPackagePath(cm);

                string hasConfig = "does not have ";

                bool result = false;
                if (configPaths.Contains(sPackagePath) || (! string.IsNullOrEmpty(cm.GetExpression(prop.Name))))
                {
                    hasConfig = "has";
                    result = true;
                }
                Results.Add(new Result(result, string.Format("The connection manager {0} {1} a configuration or expression defined for the connection string.", cm.Name,
                                            hasConfig), ResultSeverity.Normal));
            }
        }
    }
}
