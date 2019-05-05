using System;
using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using System.Data;
using System.Windows.Forms;
using BIDSHelper.Core;
using BIDSHelper.SSAS;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASTabular)]
    public class TabularTranslationsEditorPlugin : BIDSHelperPluginBase, ITabularOnPreBuildAnnotationCheck
    {
        public const string TRANSLATIONS_ANNOTATION = "BIDS_Helper_Tabular_Translations_Backups";
        //private Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox;
        private DataModelingSandboxWrapper sandboxWrapper;
        private Cube cube;

        #region Standard Plugin Overrides
        public TabularTranslationsEditorPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.TabularTranslationsEditorId);
        }

        public override string ShortName
        {
            get { return "TabularTranslationsEditor"; }
        }

        //public override int Bitmap
        //{
        //    get { return 3621; }
        //}



        public override string FeatureName
        {
            get { return "Tabular Translations Editor"; }
        }

        //public override string MenuName
        //{
        //    get { return "Item"; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
        //}

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASTabular; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Provides a UI for editing translations of metadata (not data) in Tabular models."; }
        }

        public override bool ShouldDisplayCommand()
        {
            var selectedFile = GetSelectedFile();
            if (selectedFile != null && selectedFile.Extension == ".bim")
            {
#if !DENALI && !SQL2014
                var sb = TabularHelpers.GetTabularSandboxFromBimFile(this, false);
                if (sb == null) return false;
                return !sb.IsTabularMetadata;
#else
                return true;
#endif
            }
            return false;
        }

        #endregion


        public override void Exec()
        {
            try
            {
                sandboxWrapper = new DataModelingSandboxWrapper(this);
                //sandbox = TabularHelpers.GetTabularSandboxFromBimFile(this, true);
                if (sandboxWrapper.GetSandbox() == null) throw new Exception("Can't get Sandbox!");

                ExecInternal(false);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        private void ExecInternal(bool bPreBuildCheckOnly) {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
#if !(YUKON || KATMAI) //TODO
                cube = sandboxWrapper.GetSandboxAmo().Cube;
#else
                cube = sandboxWrapper.GetSandboxAmo().Cube;
#endif
                if (cube == null) throw new Exception("The workspace database cube doesn't exist.");

                SSAS.TabularDisplayFoldersAnnotation annotationSavedFolders = TabularDisplayFolderPlugin.GetAnnotation(sandboxWrapper.GetSandbox());
                SSAS.TabularTranslationsAnnotation annotationSaved = GetAnnotation(sandboxWrapper.GetSandbox()); 

                string sTargetDatabase = null;
                string sTargetCubeName = null;
                try
                {
                    //get the deployment settings to display in the translations dialog (rather than the workspace database and cube name)
                    Microsoft.VisualStudio.Project.Automation.OAFileItem fileitem = hierItem.Object as Microsoft.VisualStudio.Project.Automation.OAFileItem;
                    DeploymentSettings deploySet = new DeploymentSettings(fileitem.ContainingProject);
                    sTargetDatabase = deploySet.TargetDatabase;
                    sTargetCubeName = deploySet.TargetCubeName;
                }
                catch { } //this seems to blow up in debug mode on my laptop... oh well


                bool bRestoredDisplayFolders = false;
                List<SSAS.TabularTranslatedItem> translationRows = new List<BIDSHelper.SSAS.TabularTranslatedItem>();

                var restrictions = new Dictionary<string,string>();
                restrictions.Add("CUBE_NAME", cube.Name);
                DataSet datasetMeasures = sandboxWrapper.GetSchemaDataSet("MDSCHEMA_MEASURES", restrictions);
                DataRowCollection rowsMeasures = datasetMeasures.Tables[0].Rows;

                Database db = cube.Parent;
                SSAS.TabularTranslatedItem captionTranslationDatabase = new SSAS.TabularTranslatedItem(null, db, SSAS.TabularTranslatedItemProperty.Caption, null, annotationSaved);
                if (!string.IsNullOrEmpty(sTargetDatabase)) captionTranslationDatabase.OverrideCaption(sTargetDatabase);
                translationRows.Add(captionTranslationDatabase);
                if (!string.IsNullOrEmpty(db.Description))
                {
                    translationRows.Add(new SSAS.TabularTranslatedItem(null, db, SSAS.TabularTranslatedItemProperty.Description, captionTranslationDatabase, null));
                }

                List<CalculationProperty> calcProperties = new List<CalculationProperty>();
                foreach (Cube c in db.Cubes)
                {
                    SSAS.TabularTranslatedItem captionTranslationCube = new SSAS.TabularTranslatedItem(null, c, SSAS.TabularTranslatedItemProperty.Caption, null, annotationSaved);
                    if (!string.IsNullOrEmpty(sTargetCubeName)) captionTranslationCube.OverrideCaption(sTargetCubeName);
                    translationRows.Add(captionTranslationCube);
                    if (!string.IsNullOrEmpty(c.Description))
                    {
                        translationRows.Add(new SSAS.TabularTranslatedItem(null, c, SSAS.TabularTranslatedItemProperty.Description, captionTranslationCube, null));
                    }

                    foreach (Perspective p in c.Perspectives)
                    {
                        SSAS.TabularTranslatedItem captionTranslationPerspective = new SSAS.TabularTranslatedItem(null, p, SSAS.TabularTranslatedItemProperty.Caption, null, annotationSaved);
                        translationRows.Add(captionTranslationPerspective);
                        if (!string.IsNullOrEmpty(p.Description))
                        {
                            translationRows.Add(new SSAS.TabularTranslatedItem(null, p, SSAS.TabularTranslatedItemProperty.Description, captionTranslationPerspective, null));
                        }
                    }

                    foreach (MdxScript mdx in c.MdxScripts)
                    {
                        foreach (CalculationProperty calc in mdx.CalculationProperties)
                        {
                            if (calc.Visible && !calc.CalculationReference.StartsWith("KPIs."))
                            {
                                calcProperties.Add(calc);
                            }
                        }
                    }
                }

                foreach (Dimension d in db.Dimensions)
                {
                    SSAS.TabularTranslatedItem captionTranslationDimension = new SSAS.TabularTranslatedItem(d.Name, d, SSAS.TabularTranslatedItemProperty.Caption, null, annotationSaved);
                    translationRows.Add(captionTranslationDimension);
                    if (!string.IsNullOrEmpty(d.Description))
                    {
                        translationRows.Add(new SSAS.TabularTranslatedItem(d.Name, d, SSAS.TabularTranslatedItemProperty.Description, captionTranslationDimension, null));
                    }

                    foreach (DimensionAttribute da in d.Attributes)
                    {
                        if (da.Type != AttributeType.RowNumber && da.AttributeHierarchyVisible)
                        {
                            if (string.IsNullOrEmpty(da.AttributeHierarchyDisplayFolder))
                            {
                                SSAS.TabularDisplayFolderAnnotation a = annotationSavedFolders.Find(da);
                                if (a != null)
                                {
                                    da.AttributeHierarchyDisplayFolder = a.DisplayFolder;
                                    bRestoredDisplayFolders = true; //we have to restore the display folder or the translations on the display folder won't even be visible in the translations dialog
                                }
                            }
                            SSAS.TabularTranslatedItem captionTranslation = new SSAS.TabularTranslatedItem(d.Name, da, SSAS.TabularTranslatedItemProperty.Caption, null, annotationSaved);
                            translationRows.Add(captionTranslation);
                            if (!string.IsNullOrEmpty(da.AttributeHierarchyDisplayFolder))
                                translationRows.Add(new SSAS.TabularTranslatedItem(d.Name, da, SSAS.TabularTranslatedItemProperty.DisplayFolder, captionTranslation, null));
                            if (!string.IsNullOrEmpty(da.Description))
                            {
                                translationRows.Add(new SSAS.TabularTranslatedItem(d.Name, da, SSAS.TabularTranslatedItemProperty.Description, captionTranslation, null));
                            }
                        }
                    }
                    foreach (Hierarchy h in d.Hierarchies)
                    {
                        if (string.IsNullOrEmpty(h.DisplayFolder))
                        {
                            SSAS.TabularDisplayFolderAnnotation a = annotationSavedFolders.Find(h);
                            if (a != null)
                            {
                                h.DisplayFolder = a.DisplayFolder;
                                bRestoredDisplayFolders = true; //we have to restore the display folder or the translations on the display folder won't even be visible in the translations dialog
                            }
                        }

                        SSAS.TabularTranslatedItem captionTranslation = new SSAS.TabularTranslatedItem(d.Name, h, SSAS.TabularTranslatedItemProperty.Caption, null, annotationSaved);
                        translationRows.Add(captionTranslation);

                        if (!string.IsNullOrEmpty(h.DisplayFolder))
                            translationRows.Add(new SSAS.TabularTranslatedItem(d.Name, h, SSAS.TabularTranslatedItemProperty.DisplayFolder, captionTranslation, null));
                        if (!string.IsNullOrEmpty(h.Description))
                            translationRows.Add(new SSAS.TabularTranslatedItem(d.Name, h, SSAS.TabularTranslatedItemProperty.Description, captionTranslation, null));

                        foreach (Level level in h.Levels)
                        {
                            SSAS.TabularTranslatedItem captionTranslationLevel = new SSAS.TabularTranslatedItem(d.Name, level, SSAS.TabularTranslatedItemProperty.Caption, null, annotationSaved);
                            translationRows.Add(captionTranslationLevel);

                            if (!string.IsNullOrEmpty(level.Description))
                                translationRows.Add(new SSAS.TabularTranslatedItem(d.Name, level, SSAS.TabularTranslatedItemProperty.Description, captionTranslationLevel, null));
                        }
                    }

                    for (int i = 0; i < rowsMeasures.Count; i++)
                    {
                        DataRow r = rowsMeasures[i];
                        if (Convert.ToString(r["MEASUREGROUP_NAME"]) == d.Name)
                        {
                            foreach (CalculationProperty calc in calcProperties)
                            {
                                if (Convert.ToString(r["MEASURE_UNIQUE_NAME"]) == "[Measures]." + calc.CalculationReference)
                                {
                                    if (string.IsNullOrEmpty(calc.DisplayFolder))
                                    {
                                        SSAS.TabularDisplayFolderAnnotation a = annotationSavedFolders.Find(calc);
                                        if (a != null)
                                        {
                                            calc.DisplayFolder = a.DisplayFolder;
                                            bRestoredDisplayFolders = true; //we have to restore the display folder or the translations on the display folder won't even be visible in the translations dialog
                                        }
                                    }

                                    SSAS.TabularTranslatedItem captionTranslation = new SSAS.TabularTranslatedItem(d.Name, calc, SSAS.TabularTranslatedItemProperty.Caption, null, annotationSaved);
                                    translationRows.Add(captionTranslation);
                                    if (!string.IsNullOrEmpty(calc.DisplayFolder))
                                        translationRows.Add(new SSAS.TabularTranslatedItem(d.Name, calc, SSAS.TabularTranslatedItemProperty.DisplayFolder, captionTranslation, null));
                                    if (!string.IsNullOrEmpty(calc.Description))
                                        translationRows.Add(new SSAS.TabularTranslatedItem(d.Name, calc, SSAS.TabularTranslatedItemProperty.Description, captionTranslation, null));

                                    rowsMeasures.Remove(r);
                                    i--;
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (Cube c in db.Cubes)
                {
                    SSAS.TabularActionsAnnotation actionAnnotations = TabularActionsEditorPlugin.GetAnnotation(c);
                    foreach (Microsoft.AnalysisServices.Action a in c.Actions)
                    {
                        SSAS.TabularAction actionAnnotation = actionAnnotations.Find(a.ID);
                        if (actionAnnotation == null) actionAnnotation = new SSAS.TabularAction();

                        if (!string.IsNullOrEmpty(actionAnnotation.OriginalTarget)
                        && !actionAnnotation.IsMasterClone)
                        {
                            continue;
                        }

                        SSAS.TabularTranslatedItem captionTranslationAction = new SSAS.TabularTranslatedItem(null, a, SSAS.TabularTranslatedItemProperty.Caption, null, annotationSaved);
                        translationRows.Add(captionTranslationAction);
                        if (!string.IsNullOrEmpty(a.Description))
                        {
                            translationRows.Add(new SSAS.TabularTranslatedItem(null, a, SSAS.TabularTranslatedItemProperty.Description, captionTranslationAction, null));
                        }
                    }
                }

                bool bRestoredTranslations = false;

                //get a list of all the distinct languages
                List<int> listLanguages = new List<int>();
                foreach (SSAS.TabularTranslatedItem item in translationRows)
                {
                    foreach (int iLang in item.Languages.Keys)
                    {
                        if (!listLanguages.Contains(iLang))
                            listLanguages.Add(iLang);
                    }
                    if (item.RestoredTranslations) bRestoredTranslations = true;
                }
                listLanguages.Sort();

                if (bRestoredTranslations)
                {
                    MessageBox.Show("Some translations have been wiped out by other editing operations. Restoring translations may be possible except when an object like a measure or a hierarchy has been renamed.\r\n\r\nBIDS Helper will attempt to restore the translations now. Be sure to click OK after you finish your edits in the Tabular Translations window.", "BIDS Helper Tabular Translations");
                }
                else if (bRestoredDisplayFolders)
                {
                    MessageBox.Show("Some display folders have been wiped out by other editing operations. Restoring display folders may be possible except when an object like a measure or a hierarchy has been renamed.\r\n\r\nBIDS Helper will attempt to restore the display folders now. Properly restored display folders are necessary for this Tabular Translations dialog to function properly. Be sure to click OK after you finish your edits in the Tabular Translations window.", "BIDS Helper Tabular Translations");
                }
                else if (bPreBuildCheckOnly)
                {
                    return; //if this is just a pre-build check and if there's nothing that needs to be restored, then just return without popping up the dialog
                }

                SSAS.TabularTranslationsEditorWindow form = new SSAS.TabularTranslationsEditorWindow(db.Language, listLanguages);
                form.WindowState = System.Windows.WindowState.Maximized;
                form.dataGrid1.ItemsSource = translationRows;


                if (form.ShowDialog() == true)
                {
                    //count dirty changes and create annotation
                    int iNumberOfChanges = 0;
                    foreach (SSAS.TabularTranslatedItem item in translationRows)
                    {
                        if (item.Dirty)
                        {
                            iNumberOfChanges++;
                        }
                    }

                    if (iNumberOfChanges > 0 || bRestoredDisplayFolders || bRestoredTranslations)
                    {
                        AlterDatabase(translationRows);
                    }
                }


            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        private void AlterDatabase(List<SSAS.TabularTranslatedItem> translatedItems)
        {
#if DENALI || SQL2014
            Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.AMOCode code;
#else
            Microsoft.AnalysisServices.BackEnd.AMOCode code;
#endif
            code = delegate
            {
                using (Microsoft.AnalysisServices.BackEnd.SandboxTransaction tran = sandboxWrapper.GetSandbox().CreateTransaction())
                {
                    SSAS.TabularTranslationsAnnotation annotation = new SSAS.TabularTranslationsAnnotation();
                    List<SSAS.TabularTranslationObjectAnnotation> annotationsList = new List<SSAS.TabularTranslationObjectAnnotation>();
                    foreach (SSAS.TabularTranslatedItem item in translatedItems)
                    {
                        item.Save(annotationsList);
                    }
                    annotation.TabularTranslations = annotationsList.ToArray();
                    TabularHelpers.SaveXmlAnnotation(cube.Parent, TRANSLATIONS_ANNOTATION, annotation);

                    TabularHelpers.EnsureDataSourceCredentials(sandboxWrapper.GetSandbox());
                    cube.Parent.Update(UpdateOptions.ExpandFull);

                    tran.GetType().InvokeMember("Commit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, tran, null); //The .Commit() function used to return a list of strings, but in the latest set of code it is a void method which leads to "method not found" errors
                    //tran.Commit();
                }
            };
#if DENALI || SQL2014
            sandboxWrapper.GetSandbox().ExecuteAMOCode(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationType.Update, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
#else
            sandboxWrapper.GetSandbox().ExecuteEngineCode(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationType.Update, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
#endif
        }

        private SSAS.TabularTranslationsAnnotation GetAnnotation(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            SSAS.TabularTranslationsAnnotation annotation = new SSAS.TabularTranslationsAnnotation();
#if DENALI || SQL2014
            var db = sandbox.Database;
#else
            var db = ((Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)sandbox.Impl).Database;
#endif
            if (db.Annotations.Contains(TRANSLATIONS_ANNOTATION))
            {
                string xml = TabularHelpers.GetAnnotationXml(db, TRANSLATIONS_ANNOTATION);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SSAS.TabularTranslationsAnnotation));
                annotation = (SSAS.TabularTranslationsAnnotation)serializer.Deserialize(new System.IO.StringReader(xml));
            }
            return annotation;
        }

#region ITabularOnPreBuildAnnotationCheck
        public TabularOnPreBuildAnnotationCheckPriority TabularOnPreBuildAnnotationCheckPriority
        {
            get
            {
                return TabularOnPreBuildAnnotationCheckPriority.RegularPriority;
            }
        }

        public string GetPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            sandboxWrapper = new DataModelingSandboxWrapper(this);
            //sandbox = TabularHelpers.GetTabularSandboxFromBimFile(this, true);
            if (sandboxWrapper.GetSandbox() == null) throw new Exception("Can't get Sandbox!");

            ExecInternal(true);
            return null; //always return null since ExecInternal just handled it
        }

        public void FixPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            //never gets called
        }
#endregion

    }
}
