namespace BIDSHelper.SSIS.DesignPracticeScanner
{
    using System.Collections.Generic;
    using EnvDTE;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.Win32;
    
    public abstract class DesignPractice
    {
        private string _name;
        private string _description;
        private Results _results = new Results();
        private bool _enabled = true;
        private bool _isEnabledCached = false;
        private string _practiceRegistryBasePath;

        public DesignPractice(string registryBasePath)
        {
            _practiceRegistryBasePath = registryBasePath;
        }

        public string Name
        {
            get { return _name; }
            protected set { _name = value; }
        }

        public string Description
        {
            get { return _description; }
            protected set { _description = value; }
        }

        public Results Results
        {
            get { return _results; }
        }

        public string PracticeRegistryPath
        {
            get { return _practiceRegistryBasePath + "\\" + Name; }
        }

        public bool Enabled
        {
            get
            {
                if (!_isEnabledCached)
                {
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PracticeRegistryPath);
                    _enabled = ((int)regKey.GetValue("Enabled", 1) == 1) ? true : false;
                    regKey.Close();
                    _isEnabledCached = true;
                }
                return _enabled;
            }

            set
            {
                // if the setting is being changed
                if (value != Enabled)
                {

                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(PracticeRegistryPath);
                    _enabled = value;

                    if (_enabled)
                    {
                        // the default state is enabled so we can remove the Enabled key
                        regKey.DeleteValue("Enabled");
                        regKey.Close();
                    }
                    else
                    {
                        // set the enabled property to 0
                        regKey.SetValue("Enabled", _enabled, RegistryValueKind.DWord);
                        regKey.Close();
                    }

                }
            }
        }

        public abstract void Check(Package package, ProjectItem projectItem);

        public override string ToString()
        {
            return Name;
        }
    }

    public class Results : List<Result>
    {

    }

    public class Result
    {
        private string _resultExplanation;
        private ResultSeverity _severity = ResultSeverity.Low;
        private bool _passed;

        public Result(bool passed, string resultExplanation, ResultSeverity severity)
        {
            _passed = passed;
            _severity = severity;
            _resultExplanation = resultExplanation;
        }

        public bool Passed
        {
            get { return _passed; }
        }

        public string ResultExplanation
        {
            get { return _resultExplanation; }
        }

        public ResultSeverity Severity
        {
            get { return _severity; }
        }
    }

    public enum ResultSeverity
    {
        Low = 0,
        Normal = 1,
        High = 2
    }
}
