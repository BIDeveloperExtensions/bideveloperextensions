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
        ProjectPropertiesAlternate = 131102,
        ProjectProperties = 393246
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

#if DENALI
        public static string SSIS = "{159641d6-6404-4a2a-ae62-294de0fe8301}";
#else
        public static string SSIS = "{d183a3d8-5fd8-494b-b014-37f57b35e655}";
#endif

        public static string SSRS = "{f14b399a-7131-4c87-9e4b-1186c45ef12d}";
    }

    public static class BIDSViewKinds
    {
        public static string Designer = "{7651A702-06E5-11D1-8EBD-00A0C90F26EA}";
    }

    /// <summary>
    /// Feature categories, as assigned to plug-ins, and used in features options dialog.
    /// </summary>
    public enum BIDSFeatureCategories
    {
        General,
        SSAS,
        SSIS,
        SSRS
    }

    public static class SsasCharacters
    {
        public const string Invalid_Name_Characters = ".,;'`:/\\*|?\"&%$!+=()[]{}<>";
    }
}
