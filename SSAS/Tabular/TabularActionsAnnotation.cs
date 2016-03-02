//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using System.Xml.Serialization;

namespace BIDSHelper.SSAS
{
    public class TabularActionsAnnotation
    {
        [XmlArray("Actions")]
        [XmlArrayItem("Action")]
        public TabularAction[] TabularActions { get; set; }

        public TabularAction Find(string actionID)
        {
            if (TabularActions == null) return null;
            foreach (TabularAction a in TabularActions)
            {
                if (a.ID == actionID)
                {
                    return a;
                }
            }
            return null;
        }
    }

    public class TabularAction
    {
        public string ID { get; set; }
        public string OriginalTarget { get; set; }
        public bool IsMasterClone { get; set; }

        [XmlArray("Perspectives")]
        [XmlArrayItem("PerspectiveID")]
        public string[] Perspectives { get; set; }
    }
}



