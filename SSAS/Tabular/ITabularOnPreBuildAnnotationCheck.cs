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
    }
}
