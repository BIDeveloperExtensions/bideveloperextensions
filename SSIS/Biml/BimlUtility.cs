namespace BIDSHelper.SSIS.Biml
{
    using System.Collections.Generic;
    using System.Text;
    using EnvDTE;
    using Microsoft.DataWarehouse.Design;
    using Microsoft.Win32;
    using Varigence.Flow.FlowFramework;
    using Varigence.Flow.FlowFramework.Validation;
    using Varigence.Languages.Biml;
    using Varigence.Languages.Biml.Platform;
    
    internal static class BimlUtility
    {
        public static bool CheckRequiredFrameworkVersion()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5");
            if (rk == null || rk.GetValue("Install") == null)
            {
                var dialog = new FrameworkVersionAlertDialog();
                dialog.ShowDialog();
                return false;
            }

            return true;
        }

        private static SsisVersion GetSsisVersion2008Variant()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\100\DTS\Setup");
            if (rk != null)
            {
                var version = rk.GetValue("Version") as string;
                if (version != null && version.StartsWith("10.5"))
                {
                    return SsisVersion.Ssis2008R2;
                }
            }

            return SsisVersion.Ssis2008;
        }

        internal static ValidationReporter GetValidationReporter(List<string> bimlScriptPaths, Project project, string projectDirectory, string tempTargetDirectory)
        {
            // ArgumentNullException - Value cannot be null. Parameter: input - Caused when using the 1.6 BIML engine version but 1.7 code, BidsHelperPhaseWorkflows xml file name mismatched. Biml vs Hadron 
#if KATMAI
            SsisVersion ssisVersion = BimlUtility.GetSsisVersion2008Variant();
            ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Biml.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2008, ssisVersion, SsasVersion.Ssas2008, SsisDeploymentModel.Package);
#elif DENALI
            ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Biml.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2008, SsisVersion.Ssis2012, SsasVersion.Ssas2008, DeployPackagesPlugin.IsLegacyDeploymentMode(project) ? SsisDeploymentModel.Package : SsisDeploymentModel.Project);
#elif SQL2014
            ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Biml.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2008, SsisVersion.Ssis2014, SsasVersion.Ssas2008, DeployPackagesPlugin.IsLegacyDeploymentMode(project) ? SsisDeploymentModel.Package : SsisDeploymentModel.Project);
#else
            ValidationReporter validationReporter = BidsHelper.CompileBiml(typeof(AstNode).Assembly, "Varigence.Biml.BidsHelperPhaseWorkflows.xml", "Compile", bimlScriptPaths, new List<string>(), tempTargetDirectory, projectDirectory, SqlServerVersion.SqlServer2005, SsisVersion.Ssis2005, SsasVersion.Ssas2005, SsisDeploymentModel.Package);
#endif
            return validationReporter;
        }

        internal static void ProcessValidationReport(IOutputWindow outputWindow, ValidationReporter validationReporter, bool showWarnings)
        {
            // Enumerate all validation messages and write to the output window.
            foreach (ValidationItem item in validationReporter.ValidationItemsList)
            {
                if (item.Severity == Severity.Error)
                {
                    outputWindow.ReportStatusError(OutputWindowErrorSeverity.Error, GetErrorId(item.ValidationCode), GetValidationReporterMessage(item), item.FilePath, item.Line, item.Offset);
                }
                else if (item.Severity == Severity.Warning)
                {
                    outputWindow.ReportStatusError(OutputWindowErrorSeverity.Warning, GetErrorId(item.ValidationCode), GetValidationReporterMessage(item), item.FilePath, item.Line, item.Offset);
                }
                else
                {
                    outputWindow.ReportStatusMessage(GetValidationReporterMessage(item));
                }
            }

            if (validationReporter.HasErrors || (showWarnings && validationReporter.HasWarnings))
            {
                // Display the modal form as well
                var form = new BimlValidationListForm(validationReporter, showWarnings);
                form.ShowDialog();
            }
        }

        private static string GetErrorId(ValidationCode validationCode)
        {
            return ((int)validationCode).ToString();
        }

        private static string GetValidationReporterMessage(ValidationItem item)
        {
            // Build a more detailed message for the Output window.
            StringBuilder builder = new StringBuilder();

            // Check we have a value, and it isn't something useless i.e. "BimlEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=dd4a9bc4187e1297"
            if (!string.IsNullOrEmpty(item.PhaseName) && !item.PhaseName.StartsWith("BimlEngine, Version")) 
            {
                builder.AppendFormat("{0}. ", item.PhaseName);
            }

            GetMessageString(item.Message, ref builder);
            GetMessageString(item.Recommendation, ref builder);

            if (!string.IsNullOrEmpty(item.PropertyName))
            {
                builder.AppendFormat("Property {0}. ", item.PropertyName);
            }

            if (!string.IsNullOrEmpty(item.SchemaName))
            {
                builder.AppendFormat("Schema {0}. ", item.SchemaName);
            }

            if (item.Exception != null)
            {
                // Just show the exception type name, which can help indicate the type of issue, e.g. invalid XML vs invalid BIML
                // We don't want the full Exception as it is confusing and makes you think something has gone wrong.
                builder.AppendFormat("Exception type: {0}", item.Exception.GetType().Name);
            }

            return builder.ToString().TrimEnd();
        }

        private static void GetMessageString(string input, ref StringBuilder builder)
        {
            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            input = input.Trim();

            if (input.EndsWith("."))
            {
                builder.AppendFormat("{0} ", input);
            }
            else
            {
                builder.AppendFormat("{0}. ", input);
            }
        }

    }
}
