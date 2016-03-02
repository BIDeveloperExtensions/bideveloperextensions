using System.Xml.Serialization;
using Microsoft.AnalysisServices;

namespace BIDSHelper.SSAS
{
    public class TabularTranslationsAnnotation
    {
        [XmlArray("Objects")]
        [XmlArrayItem("Object")]
        public TabularTranslationObjectAnnotation[] TabularTranslations { get; set; }

        public TabularTranslationObjectAnnotation Find(IModelComponent obj)
        {
            if (TabularTranslations == null) return null;
            foreach (TabularTranslationObjectAnnotation a in TabularTranslations)
            {
                if (obj is DimensionAttribute && a.ObjectType == TabularTranslatedItemType.Column)
                {
                    if (((DimensionAttribute)obj).ID == a.ObjectID && ((DimensionAttribute)obj).Parent.ID == a.TableID)
                    {
                        return a;
                    }
                }
                else if (obj is Hierarchy && a.ObjectType == TabularTranslatedItemType.Hierarchy)
                {
                    if (((Hierarchy)obj).ID == a.ObjectID && ((Hierarchy)obj).Parent.ID == a.TableID)
                    {
                        return a;
                    }
                }
                else if (obj is Level && a.ObjectType == TabularTranslatedItemType.Level)
                {
                    if (((Level)obj).ID == a.ObjectID && ((Level)obj).Parent.ID == a.HierarchyID && ((Level)obj).ParentDimension.ID == a.TableID)
                    {
                        return a;
                    }
                }
                else if (obj is Dimension && a.ObjectType == TabularTranslatedItemType.Table)
                {
                    if (((Dimension)obj).ID == a.ObjectID)
                    {
                        return a;
                    }
                }
                else if (obj is Database && a.ObjectType == TabularTranslatedItemType.Database)
                {
                    if (((Database)obj).ID == a.ObjectID)
                    {
                        return a;
                    }
                }
                else if (obj is Cube && a.ObjectType == TabularTranslatedItemType.Cube)
                {
                    if (((Cube)obj).ID == a.ObjectID)
                    {
                        return a;
                    }
                }
                else if (obj is Perspective && a.ObjectType == TabularTranslatedItemType.Perspective)
                {
                    if (((Perspective)obj).ID == a.ObjectID)
                    {
                        return a;
                    }
                }
                else if (obj is Action && a.ObjectType == TabularTranslatedItemType.Action)
                {
                    if (((Action)obj).ID == a.ObjectID)
                    {
                        return a;
                    }
                }
                else if (obj is CalculationProperty && a.ObjectType == TabularTranslatedItemType.Measure)
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

    public class TabularTranslationObjectAnnotation
    {
        public string TableID { get; set; }
        public string HierarchyID { get; set; }
        public string ObjectID { get; set; }
        public TabularTranslatedItemType ObjectType { get; set; }

        [XmlArray("Translations")]
        [XmlArrayItem("Translation")]
        public TabularTranslationAnnotation[] TabularTranslations { get; set; }
    }

    public class TabularTranslationAnnotation
    {
        public int Language { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string DisplayFolder { get; set; }
    }
}



