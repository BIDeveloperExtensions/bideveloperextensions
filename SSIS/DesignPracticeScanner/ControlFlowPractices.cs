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
            
        }

        public override void Check(Package package, ProjectItem projectItem)
        {

        }
    }
}
