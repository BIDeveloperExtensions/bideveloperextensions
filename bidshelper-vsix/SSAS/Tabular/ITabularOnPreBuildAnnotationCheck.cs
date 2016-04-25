using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BIDSHelper
{
    interface ITabularOnPreBuildAnnotationCheck
    {

        string GetPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox);
        void FixPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox);

        /// <summary>
        /// Represents the sequence in which this check should be run. HighPriority checks get run first. If one check must succeed before another check is run, this property helps ensure that.
        /// </summary>
        TabularOnPreBuildAnnotationCheckPriority TabularOnPreBuildAnnotationCheckPriority { get; }
    }

    public enum TabularOnPreBuildAnnotationCheckPriority
    {
        RegularPriority,

        /// <summary>
        /// HighPriority means that this check should run before other RegularPriority checks
        /// </summary>
        HighPriority
    }
}
