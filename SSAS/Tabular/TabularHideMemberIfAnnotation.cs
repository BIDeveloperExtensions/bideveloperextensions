using System.Xml.Serialization;
using System.Collections.Generic;

namespace BIDSHelper.SSAS
{
    public class TabularHideMemberIfAnnotation
    {
        [XmlArray("Levels")]
        [XmlArrayItem("Level")]
        public TabularLevelHideMemberIf[] TabularLevels { get; set; }

        public TabularLevelHideMemberIf Find(Microsoft.AnalysisServices.Level level)
        {
            if (TabularLevels == null) return null;
            foreach (TabularLevelHideMemberIf a in TabularLevels)
            {
                if (a.DimensionID == level.ParentDimension.ID && a.HierarchyID == level.Parent.ID && a.LevelID == level.ID)
                {
                    return a;
                }
            }
            return null;
        }

        public void Set(Microsoft.AnalysisServices.Level level)
        {
            TabularLevelHideMemberIf levelAnnotation = Find(level);
            if (levelAnnotation == null)
            {
                List<TabularLevelHideMemberIf> levels = new List<TabularLevelHideMemberIf>(TabularLevels ?? new TabularLevelHideMemberIf[] { });
                levelAnnotation = new TabularLevelHideMemberIf();
                levelAnnotation.DimensionID = level.ParentDimension.ID;
                levelAnnotation.HierarchyID = level.Parent.ID;
                levelAnnotation.LevelID = level.ID;
                levels.Add(levelAnnotation);
                TabularLevels = levels.ToArray();
            }
            levelAnnotation.HideMemberIf = level.HideMemberIf;
        }

    }

    public class TabularLevelHideMemberIf
    {
        public string DimensionID { get; set; }
        public string HierarchyID { get; set; }
        public string LevelID { get; set; }
        public Microsoft.AnalysisServices.HideIfValue HideMemberIf { get; set; }
    }
}



