namespace BIDSHelper.SSIS.DesignPracticeScanner
{
    using System;
    using EnvDTE;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

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

    class VariableEvaluateAsExpressionPractice : DesignPractice
    {
        public VariableEvaluateAsExpressionPractice(string registryPath) : base(registryPath)
        {
            this.Name = BIDSHelper.Resources.Common.VariableEvaluateAsExpressionPracticeName;
            this.Description = BIDSHelper.Resources.Common.VariableEvaluateAsExpressionPracticeDescription;
        }

        public override void Check(Package package, ProjectItem projectItem)
        {
            ProcessObject(package, string.Empty);
        }

        #region Package Scanning

        private void ProcessObject(object component, string path)
        {
            DtsContainer container = component as DtsContainer;

            // Should only get package as we call GetPackage up front. Could make scope like, but need UI indicator that this is happening
            Package package = component as Package;
            if (package != null)
            {
                path = "\\Package";
            }
            else if (!(component is DtsEventHandler))
            {
                path = path + "\\" + container.Name;
            }

            if (container != null)
            {
                ScanVariables(path, container.Variables); 
            }

            EventsProvider eventsProvider = component as EventsProvider;
            if (eventsProvider != null)
            {
                foreach (DtsEventHandler eventhandler in eventsProvider.EventHandlers)
                {
                    ProcessObject(eventhandler, path + ".EventHandlers[" + eventhandler.Name + "]");
                }
            }

            IDTSSequence sequence = component as IDTSSequence;
            if (sequence != null)
            {
                ProcessSequence(container, sequence, path);
            }
        }

        private void ProcessSequence(DtsContainer container, IDTSSequence sequence, string path)
        {
            foreach (Executable executable in sequence.Executables)
            {
                ProcessObject(executable, path);
            }
        }

        private void ScanVariables(string objectPath, Variables variables)
        {
            foreach (Variable variable in variables)
            {
                if (string.IsNullOrEmpty(variable.Expression))
                {
                    continue;
                }

                if (!variable.EvaluateAsExpression)
                {
                    this.Results.Add(new Result(false, string.Format(BIDSHelper.Resources.Common.VariableEvaluateAsExpressionPracticeResultFormatString, variable.Name, objectPath), ResultSeverity.Normal));
                }
            }
        }

        #endregion

    }
}
