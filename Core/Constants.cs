using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace BIDSHelper
{

    public enum BIDSToolbarButtonID
    {
        FormView = 12854,
        ScriptView = 12853,
        CalculationProperties = 12875,
        ProjectProperties = 131102
    }

    public enum BIDSViewMenuItemCommandID
    {
        Calculations = 12899,
        CubeStructure = 12897,
        Partitions = 12902,
        Perspectives = 12904
    }

    public static class BIDSProjectKinds
    {
        public static string SSAS = "{d2abab84-bf74-430a-b69e-9dc6d40dda17}";
        public static string SSIS = "{d183a3d8-5fd8-494b-b014-37f57b35e655}";
    }

    public static class BIDSViewKinds
    {
        public static string SsisDesigner = "{7651A702-06E5-11D1-8EBD-00A0C90F26EA}";
    }
}