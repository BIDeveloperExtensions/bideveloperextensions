using System.Xml.Serialization;
using Microsoft.AnalysisServices;

namespace BIDSHelper.SSAS
{
    public class TabularDisplayFoldersAnnotation
    {
        [XmlArray("DisplayFolders")]
        [XmlArrayItem("DisplayFolder")]
        public TabularDisplayFolderAnnotation[] TabularDisplayFolders { get; set; }

        public TabularDisplayFolderAnnotation Find(IModelComponent obj)
        {
            if (TabularDisplayFolders == null) return null;
            foreach (TabularDisplayFolderAnnotation a in TabularDisplayFolders)
            {
                if (obj is DimensionAttribute && a.ObjectType == TabularDisplayFolderType.Column)
                {
                    if (((DimensionAttribute)obj).ID == a.ObjectID && ((DimensionAttribute)obj).Parent.ID == a.TableID)
                    {
                        return a;
                    }
                }
                else if (obj is Hierarchy && a.ObjectType == TabularDisplayFolderType.Hierarchy)
                {
                    if (((Hierarchy)obj).ID == a.ObjectID && ((Hierarchy)obj).Parent.ID == a.TableID)
                    {
                        return a;
                    }
                }
                else if (obj is CalculationProperty && a.ObjectType == TabularDisplayFolderType.Measure)
                {
                    string sObjectName = ((CalculationProperty)obj).CalculationReference;

                    if (sObjectName.StartsWith("[") && sObjectName.EndsWith("]"))
                    {
                        sObjectName = sObjectName.Substring(1, sObjectName.Length - 2);
                    }

                    if (sObjectName == a.ObjectID) //no need to compare tables since measure names must be unique across all tables
                    {
                        return a;
                    }
                }
            }
            return null;
        }
    }

    public class TabularDisplayFolderAnnotation
    {
        public string TableID { get; set; }
        public string ObjectID { get; set; }
        public TabularDisplayFolderType ObjectType { get; set; }
        public string DisplayFolder { get; set; }
    }
}



