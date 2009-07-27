using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper.SSIS.DesignPracticeScanner
{
    class ProtectionLevelPractice:DesignPractice
    {
        public ProtectionLevelPractice(string registryPath):base(registryPath)
        {
            Name = "Package ProtectionLevel";
            Description = "Validates that the ProtectionLevel property is set to DontSaveSensitive or ServerStorage";
        }

        public override void Check(Package package, ProjectItem projectItem)
        {
            switch (package.ProtectionLevel)
            {
                case DTSProtectionLevel.DontSaveSensitive:
                case DTSProtectionLevel.ServerStorage:
                    break;
                case DTSProtectionLevel.EncryptSensitiveWithUserKey:
                case DTSProtectionLevel.EncryptSensitiveWithPassword:
                case DTSProtectionLevel.EncryptAllWithPassword:
                case DTSProtectionLevel.EncryptAllWithUserKey:
                    Results.Add(new Result(false, String.Format ("Consider using ServerStorage for packages stored in SQL Server, or DontSaveSensitive with appropriately secured configurations, as it makes packages easier to deploy and share with other developers."), ResultSeverity.Normal));
                break;
            }
        }
    }
}
