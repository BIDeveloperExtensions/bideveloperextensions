using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AnalysisServices;

namespace BIDSHelper.SSAS
{
    public class TabularTranslatedItem
    {
        private bool _dirty = false;
        private bool _restoredTranslations = false;
        private IModelComponent _object;
        private string _ObjectName;

        public TabularTranslatedItem(string table, IModelComponent obj, TabularTranslatedItemProperty property, TabularTranslatedItem caption, SSAS.TabularTranslationsAnnotation annotations)
        {
            _Table = table;
            _object = obj;
            _Property = property;

            TranslationCollection translations;
            string sCaption;
            string sDescription;
            string sDisplayFolder = null;
            if (obj is DimensionAttribute)
            {
                _ObjectType = TabularTranslatedItemType.Column;
                DimensionAttribute typedobj = (DimensionAttribute)obj;
                translations = typedobj.Translations;
                sDisplayFolder = typedobj.AttributeHierarchyDisplayFolder;
                sCaption = typedobj.Name;
                sDescription = typedobj.Description;
            }
            else if (obj is Hierarchy)
            {
                _ObjectType = TabularTranslatedItemType.Hierarchy;
                Hierarchy typedobj = (Hierarchy)obj;
                translations = typedobj.Translations;
                sDisplayFolder = typedobj.DisplayFolder;
                sCaption = typedobj.Name;
                sDescription = typedobj.Description;
            }
            else if (obj is Level)
            {
                _ObjectType = TabularTranslatedItemType.Level;
                Level typedobj = (Level)obj;
                translations = typedobj.Translations;
                sCaption = typedobj.Name;
                sDescription = typedobj.Description;
            }
            else if (obj is CalculationProperty)
            {
                _ObjectType = TabularTranslatedItemType.Measure;
                CalculationProperty typedobj = (CalculationProperty)obj;
                translations = typedobj.Translations;
                sDisplayFolder = typedobj.DisplayFolder;
                sCaption = typedobj.CalculationReference;
                sDescription = typedobj.Description;

                if (sCaption.StartsWith("[") && sCaption.EndsWith("]"))
                {
                    sCaption = sCaption.Substring(1, sCaption.Length - 2);
                }
            }
            else if (obj is Database)
            {
                _ObjectType = TabularTranslatedItemType.Database;
                Database typedobj = (Database)obj;
                translations = typedobj.Translations;
                sCaption = typedobj.Name;
                sDescription = typedobj.Description;
            }
            else if (obj is Cube)
            {
                _ObjectType = TabularTranslatedItemType.Cube;
                Cube typedobj = (Cube)obj;
                translations = typedobj.Translations;
                sCaption = typedobj.Name;
                sDescription = typedobj.Description;
            }
            else if (obj is Perspective)
            {
                _ObjectType = TabularTranslatedItemType.Perspective;
                Perspective typedobj = (Perspective)obj;
                translations = typedobj.Translations;
                sCaption = typedobj.Name;
                sDescription = typedobj.Description;
            }
            else if (obj is Dimension)
            {
                _ObjectType = TabularTranslatedItemType.Table;
                Dimension typedobj = (Dimension)obj;
                translations = typedobj.Translations;
                sCaption = typedobj.Name;
                sDescription = typedobj.Description;
            }
            else if (obj is Microsoft.AnalysisServices.Action)
            {
                _ObjectType = TabularTranslatedItemType.Action;
                Microsoft.AnalysisServices.Action typedobj = (Microsoft.AnalysisServices.Action)obj;
                translations = typedobj.Translations;
                sCaption = typedobj.Name;
                sDescription = typedobj.Description;
            }
            else
            {
                throw new Exception("Unexpected object type: " + obj.GetType().Name);
            }

            _ObjectName = sCaption;

            if (property == TabularTranslatedItemProperty.Caption)
            {
                _DefaultLanguage = sCaption;

                SSAS.TabularTranslationObjectAnnotation annotation = annotations.Find(obj);
                if (annotation != null && translations.Count == 0)
                {
                    foreach (SSAS.TabularTranslationAnnotation tranAnnotation in annotation.TabularTranslations)
                    {
                        Translation t = new Translation(tranAnnotation.Language);
                        if (obj is DimensionAttribute)
                            t = new AttributeTranslation(tranAnnotation.Language);
                        t.Caption = tranAnnotation.Caption;
                        t.Description = tranAnnotation.Description;
                        t.DisplayFolder = tranAnnotation.DisplayFolder;
                        translations.Add(t);
                    }
                    _restoredTranslations = true;
                }
            
            }
            else if (property == TabularTranslatedItemProperty.Description)
            {
                _DefaultLanguage = sDescription;
            }
            else if (property == TabularTranslatedItemProperty.DisplayFolder)
            {
                _DefaultLanguage = sDisplayFolder;
            }

            Languages = new Core.DirtyMonitoredDictionary<int, string>();
            foreach (Translation t in translations)
            {
                if (property == TabularTranslatedItemProperty.Caption)
                {
                    Languages.Add(t.Language, t.Caption);
                }
                else if (property == TabularTranslatedItemProperty.Description)
                {
                    Languages.Add(t.Language, t.Description);
                }
                else if (property == TabularTranslatedItemProperty.DisplayFolder)
                {
                    Languages.Add(t.Language, t.DisplayFolder);
                }
            }

            if (caption != null)
            {
                caption.DependentProperties.Add(this);
            }
        }

        public void OverrideCaption(string caption)
        {
            _DefaultLanguage = caption;
        }

        public void Save(List<SSAS.TabularTranslationObjectAnnotation> annotationsList)
        {
            if (_Property != TabularTranslatedItemProperty.Caption) return; //every object will have a caption, so only perform the save on the caption

            Dictionary<int, string> DisplayFolderLanguages = new Dictionary<int, string>();
            Dictionary<int, string> DescriptionLanguages = new Dictionary<int, string>();
            bool bDependentPropertyDirty = false;
            foreach (TabularTranslatedItem dependent in DependentProperties)
            {
                bDependentPropertyDirty = bDependentPropertyDirty || dependent.Dirty;
                if (dependent.Property == TabularTranslatedItemProperty.DisplayFolder)
                    DisplayFolderLanguages = dependent.Languages;
                else if (dependent.Property == TabularTranslatedItemProperty.Description)
                    DescriptionLanguages = dependent.Languages;
            }

            //if (!Dirty && !bDependentPropertyDirty) return; //would be nice if we could short-circuit here, but we need to build the annotation

            SSAS.TabularTranslationObjectAnnotation annotation = new TabularTranslationObjectAnnotation();
            annotation.ObjectType = _ObjectType;

            if (_object is DimensionAttribute)
            {
                DimensionAttribute da = (DimensionAttribute)_object;
                annotation.ObjectID = da.ID;
                annotation.TableID = da.Parent.ID;
                SaveInternal(da.Translations, DisplayFolderLanguages, DescriptionLanguages, annotation);
            }
            else if (_object is Hierarchy)
            {
                Hierarchy h = (Hierarchy)_object;
                annotation.ObjectID = h.ID;
                annotation.TableID = h.Parent.ID;
                SaveInternal(h.Translations, DisplayFolderLanguages, DescriptionLanguages, annotation);
            }
            else if (_object is Level)
            {
                Level l = (Level)_object;
                annotation.ObjectID = l.ID;
                annotation.HierarchyID = l.Parent.ID;
                annotation.TableID = l.ParentDimension.ID;
                SaveInternal(l.Translations, DisplayFolderLanguages, DescriptionLanguages, annotation);
            }
            else if (_object is CalculationProperty)
            {
                CalculationProperty calc = (CalculationProperty)_object;
                annotation.ObjectID = _ObjectName;
                //no need to save table
                SaveInternal(calc.Translations, DisplayFolderLanguages, DescriptionLanguages, annotation);
            }
            else if (_object is Database)
            {
                Database db = (Database)_object;
                annotation.ObjectID = db.ID;
                SaveInternal(db.Translations, DisplayFolderLanguages, DescriptionLanguages, annotation);
            }
            else if (_object is Cube)
            {
                Cube cube = (Cube)_object;
                annotation.ObjectID = cube.ID;
                SaveInternal(cube.Translations, DisplayFolderLanguages, DescriptionLanguages, annotation);
            }
            else if (_object is Perspective)
            {
                Perspective p = (Perspective)_object;
                annotation.ObjectID = p.ID;
                SaveInternal(p.Translations, DisplayFolderLanguages, DescriptionLanguages, annotation);
            }
            else if (_object is Dimension)
            {
                Dimension dim = (Dimension)_object;
                annotation.ObjectID = dim.ID;
                SaveInternal(dim.Translations, DisplayFolderLanguages, DescriptionLanguages, annotation);
                foreach (Cube cube in dim.Parent.Cubes)
                {
                    foreach (CubeDimension cd in cube.Dimensions)
                    {
                        if (cd.DimensionID == dim.ID)
                        {
                            SaveInternal(cd.Translations, DisplayFolderLanguages, DescriptionLanguages, null);
                        }
                    }
                    foreach (MeasureGroup mg in cube.MeasureGroups)
                    {
                        if (mg.ID == dim.ID)
                        {
                            SaveInternal(mg.Translations, DisplayFolderLanguages, DescriptionLanguages, null);
                        }
                    }
                }
            }
            else if (_object is Microsoft.AnalysisServices.Action)
            {
                Microsoft.AnalysisServices.Action actionMaster = (Microsoft.AnalysisServices.Action)_object;
                foreach (Microsoft.AnalysisServices.Action action in actionMaster.Parent.Actions)
                {
                    if (action.ID.StartsWith(actionMaster.ID))
                    {
                        annotation.ObjectID = action.ID;
                        SaveInternal(action.Translations, DisplayFolderLanguages, DescriptionLanguages, annotation);
                    }
                }
            }
            else
            {
                throw new Exception("Unexpected object type: " + _object.GetType().Name);
            }

            if (annotation.TabularTranslations.Length > 0)
            {
                annotationsList.Add(annotation);
            }
        }

        private void SaveInternal(TranslationCollection translations, Dictionary<int, string> DisplayFolderLanguages, Dictionary<int, string> DescriptionLanguages, SSAS.TabularTranslationObjectAnnotation annotation)
        {
            List<int> listAllLanguages = new List<int>(Languages.Keys);
            foreach (int iLang in DisplayFolderLanguages.Keys)
            {
                if (!listAllLanguages.Contains(iLang))
                    listAllLanguages.Add(iLang);
            }
            foreach (int iLang in DescriptionLanguages.Keys)
            {
                if (!listAllLanguages.Contains(iLang))
                    listAllLanguages.Add(iLang);
            }

            List<SSAS.TabularTranslationAnnotation> listAnnotations = new List<TabularTranslationAnnotation>();
            translations.Clear();
            foreach (int iLang in listAllLanguages)
            {
                Translation t = new Translation(iLang);
                if (translations is AttributeTranslationCollection)
                    t = new AttributeTranslation(iLang);
                if (DisplayFolderLanguages.ContainsKey(iLang))
                    t.DisplayFolder = DisplayFolderLanguages[iLang];
                if (DescriptionLanguages.ContainsKey(iLang))
                    t.Description = DescriptionLanguages[iLang];

                if (Languages.ContainsKey(iLang) && !string.IsNullOrEmpty(Languages[iLang]))
                    t.Caption = Languages[iLang];
                else if (!string.IsNullOrEmpty(t.DisplayFolder) || !string.IsNullOrEmpty(t.Description))
                    t.Caption = this.DefaultLanguage; //this works around a problem where if they translate the display folder or the description but not the caption, then the caption ends up blank
                
                if (!string.IsNullOrEmpty(t.Caption) || !string.IsNullOrEmpty(t.Description) || !string.IsNullOrEmpty(t.DisplayFolder))
                {
                    translations.Add(t);
                    SSAS.TabularTranslationAnnotation tranAnnotation = new TabularTranslationAnnotation();
                    tranAnnotation.Caption = t.Caption;
                    tranAnnotation.Description = t.Description;
                    tranAnnotation.DisplayFolder = t.DisplayFolder;
                    tranAnnotation.Language = t.Language;
                    listAnnotations.Add(tranAnnotation);
                }
            }

            if (annotation != null)
                annotation.TabularTranslations = listAnnotations.ToArray();
        }

        private string _Table;
        public string Table
        {
            get { return _Table; }
        }

        private TabularTranslatedItemType _ObjectType;
        public TabularTranslatedItemType ObjectType
        {
            get
            {
                return _ObjectType;
            }
        }

        private TabularTranslatedItemProperty _Property;
        public TabularTranslatedItemProperty Property
        {
            get
            {
                return _Property;
            }
        }

        public string PropertyForDisplay
        {
            get
            {
                if (_Property == TabularTranslatedItemProperty.Caption 
                    && _object is Microsoft.AnalysisServices.Action
                    && ((Microsoft.AnalysisServices.Action)_object).CaptionIsMdx)
                {
                        return "Caption (MDX)";
                }
                else
                {
                    return _Property.ToString();
                }
            }
        }

        private string _DefaultLanguage;
        public string DefaultLanguage
        {
            get
            {
                return _DefaultLanguage;
            }
            //don't let the default language be editable for now... I'm afraid that renaming a measure might have negative effects or would be tricky to implement; TODO
            //set
            //{
            //    if (string.IsNullOrWhiteSpace(value))
            //        value = null;

            //    if (_DefaultLanguage != value)
            //    {
            //        _dirty = true;
            //    }

            //    _DefaultLanguage = value;
            //}
        }

        internal string ObjectID
        {
            get
            {
                if (_object is NamedComponent)
                {
                    return ((NamedComponent)_object).ID;
                }
                else
                {
                    return _ObjectName;
                }
            }
        }


        public Core.DirtyMonitoredDictionary<int, string> Languages { get; set; }

        public List<TabularTranslatedItem> DependentProperties = new List<TabularTranslatedItem>();

        internal bool Dirty
        {
            get { return Languages.Dirty || _dirty || _restoredTranslations; }
        }

        internal bool RestoredTranslations
        {
            get { return _restoredTranslations; }
        }

    }


    public enum TabularTranslatedItemType
    {
        Table,
        Column,
        Measure,
        Hierarchy,
        Level,
        Action,
        Cube,
        Database,
        Perspective
    }

    public enum TabularTranslatedItemProperty
    {
        Caption,
        Description,
        DisplayFolder
    }
}
