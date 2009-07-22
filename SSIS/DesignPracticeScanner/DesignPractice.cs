using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using EnvDTE;

namespace BIDSHelper.SSIS.DesignPracticeScanner
{
    public abstract class DesignPractice
    {
        private string _name;
        private string _description;
        private Results _results = new Results();

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

        public abstract void Check(Package package, ProjectItem projectItem);
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
        Low=0,
        Normal=1,
        High=2
    }

}
