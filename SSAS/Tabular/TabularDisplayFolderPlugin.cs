using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using System.Data;
using BIDSHelper.Core;
using Microsoft.AnalysisServices.BackEnd;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASTabular)]
    public class TabularDisplayFolderPlugin : BIDSHelperPluginBase, ITabularOnPreBuildAnnotationCheck
    {
        public const string DISPLAY_FOLDER_ANNOTATION = "BIDS_Helper_Tabular_Display_Folder_Backups";
//#if DENALI || SQL2014
        private Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox;
//#else
//        private Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo sandbox;
//#endif
        private Cube cube;

#region Standard Plugin Overrides
        public TabularDisplayFolderPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.TabularDisplayFoldersId);
        }

        public override string ShortName
        {
            get { return "TabularDisplayFolders"; }
        }

        //public override int Bitmap
        //{
        //    get { return 2116; }
        //}

        public override string FeatureName
        {
            get { return "Tabular Display Folders"; }
        }

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
            get { return "Provides a UI for editing display folders for columns and measures in Tabular models."; }
        }
        
#endregion

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                //#if DENALI || SQL2014
                //Microsoft.AnalysisServices.BackEnd.DataModelingSandbox _sandbox = TabularHelpers.GetTabularSandboxFromBimFile(this, true);
                var _sandbox = new DataModelingSandboxWrapper(this);
                //#else
                //                var _sandbox = TabularHelpers.GetTabularSandboxAmoFromBimFile(this, true);
                //#endif
                var conn = _sandbox.GetSandbox().AdomdConnection;

                ExecSandbox(_sandbox);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        private void ExecSandbox(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandboxParam)
        {
            var wrap = new DataModelingSandboxWrapper(sandboxParam);
            ExecSandbox(wrap);
        }
        private void ExecSandbox(DataModelingSandboxWrapper sandboxParam)
        {
            try
            {
//#if DENALI || SQL2014
                sandbox = sandboxParam.GetSandbox();
//#else
//                sandbox = sandboxParam.GetSandboxAmo();
//#endif
                if (sandbox == null) throw new Exception("Can't get Sandbox!");
                cube = sandboxParam.GetSandboxAmo().Cube;
                if (cube == null) throw new Exception("The workspace database cube doesn't exist.");

                bool bRestoreDisplayFolders = false;
                SSAS.TabularDisplayFoldersAnnotation annotationSaved = GetAnnotation(sandboxParam.GetSandbox());
                if (GetPreBuildWarning(sandboxParam.GetSandbox()) != null)
                {
                    if (MessageBox.Show("Some display folders have been blanked out by other editing operations. Restoring display folders may be possible except when a measures or columns have been renamed.\r\n\r\nWould you like BIDS Helper to attempt restore the display folders now?", "BIDS Helper Tabular Display Folders", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        bRestoreDisplayFolders = true;
                    }
                }

                SSAS.TabularDisplayFolderWindow form = new SSAS.TabularDisplayFolderWindow();

                List<BIDSHelper.SSAS.TabularDisplayFolder> displayFolders = new List<BIDSHelper.SSAS.TabularDisplayFolder>();

                Dictionary<string,string> restrictions = new Dictionary<string, string>();
                restrictions.Add("CUBE_NAME", cube.Name);
                
                DataSet datasetMeasures = sandboxParam.GetSchemaDataSet("MDSCHEMA_MEASURES", restrictions);

                Database db = cube.Parent;
                foreach (Dimension d in db.Dimensions)
                {
                    foreach (DimensionAttribute da in d.Attributes)
                    {
                        if (da.Type != AttributeType.RowNumber && da.AttributeHierarchyVisible)
                        {
                            if (bRestoreDisplayFolders && string.IsNullOrEmpty(da.AttributeHierarchyDisplayFolder))
                            {
                                SSAS.TabularDisplayFolderAnnotation a = annotationSaved.Find(da);
                                if (a != null) da.AttributeHierarchyDisplayFolder = a.DisplayFolder;
                            }
                            displayFolders.Add(new BIDSHelper.SSAS.TabularDisplayFolder(d.Name, da));
                        }
                    }
                    foreach (Hierarchy h in d.Hierarchies)
                    {
                        if (bRestoreDisplayFolders && string.IsNullOrEmpty(h.DisplayFolder))
                        {
                            SSAS.TabularDisplayFolderAnnotation a = annotationSaved.Find(h);
                            if (a != null) h.DisplayFolder = a.DisplayFolder;
                        }
                        displayFolders.Add(new BIDSHelper.SSAS.TabularDisplayFolder(d.Name, h));
                    }
                }
                foreach (Cube c in db.Cubes)
                {
                    foreach (MdxScript mdx in c.MdxScripts)
                    {
                        foreach (CalculationProperty calc in mdx.CalculationProperties)
                        {
                            if (calc.Visible && !calc.CalculationReference.StartsWith("KPIs.")) //TODO: display folder for KPIs have to be set inline in MDX script... do this in the future
                            {
                                string sTableName = null;
                                foreach (DataRow r in datasetMeasures.Tables[0].Rows)
                                {
                                    if (Convert.ToString(r["MEASURE_UNIQUE_NAME"]) == "[Measures]." + calc.CalculationReference)
                                    {
                                        sTableName = Convert.ToString(r["MEASUREGROUP_NAME"]);
                                        break;
                                    }
                                }

                                if (bRestoreDisplayFolders && string.IsNullOrEmpty(calc.DisplayFolder))
                                {
                                    SSAS.TabularDisplayFolderAnnotation a = annotationSaved.Find(calc);
                                    if (a != null) calc.DisplayFolder = a.DisplayFolder;
                                }

                                displayFolders.Add(new BIDSHelper.SSAS.TabularDisplayFolder(sTableName, calc));
                            }
                        }
                    }
                }

                displayFolders.Sort();
                form.dataGrid1.ItemsSource = displayFolders;



                if (form.ShowDialog() == true)
                {
                    //count dirty changes and create annotation
                    SSAS.TabularDisplayFoldersAnnotation annotation = new SSAS.TabularDisplayFoldersAnnotation();
                    List<SSAS.TabularDisplayFolderAnnotation> folderAnnotationsList = new List<SSAS.TabularDisplayFolderAnnotation>();
                    int iNumberOfChanges = 0;
                    foreach (BIDSHelper.SSAS.TabularDisplayFolder folder in displayFolders)
                    {
                        if (folder.Dirty)
                        {
                            iNumberOfChanges++;
                        }
                        if (!string.IsNullOrEmpty(folder.DisplayFolder))
                        {
                            SSAS.TabularDisplayFolderAnnotation folderAnnotation = new SSAS.TabularDisplayFolderAnnotation();
                            folderAnnotation.TableID = cube.Parent.Dimensions.GetByName(folder.Table).ID;
                            folderAnnotation.ObjectID = folder.ObjectID;
                            folderAnnotation.ObjectType = folder.ObjectType;
                            folderAnnotation.DisplayFolder = folder.DisplayFolder;
                            folderAnnotationsList.Add(folderAnnotation);
                        }
                    }
                    annotation.TabularDisplayFolders = folderAnnotationsList.ToArray();

                    if (iNumberOfChanges > 0 || bRestoreDisplayFolders)
                    {
#if DENALI || SQL2014
                        Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.AMOCode code;
#else
                        Microsoft.AnalysisServices.BackEnd.AMOCode code;
#endif
                        code = delegate
                            {
                                using (Microsoft.AnalysisServices.BackEnd.SandboxTransaction tran = sandboxParam.GetSandbox().CreateTransaction())
                                {
                                    foreach (BIDSHelper.SSAS.TabularDisplayFolder folder in displayFolders)
                                    {
                                        folder.SaveDisplayFolder();
                                    }
                                    TabularHelpers.SaveXmlAnnotation(cube.Parent, DISPLAY_FOLDER_ANNOTATION, annotation);

                                    TabularHelpers.EnsureDataSourceCredentials(sandboxParam.GetSandbox());
                                    cube.Parent.Update(UpdateOptions.ExpandFull);

                                    tran.GetType().InvokeMember("Commit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, tran, null); //The .Commit() function used to return a list of strings, but in the latest set of code it is a void method which leads to "method not found" errors
                                    //tran.Commit();
                                }
                            };
#if DENALI || SQL2014
                        sandbox.ExecuteAMOCode(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationType.Update, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
#else
                        sandboxParam.GetSandbox().ExecuteEngineCode(DataModelingSandbox.OperationType.Update, DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
#endif
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }
#if DENALI || SQL2014
        public static SSAS.TabularDisplayFoldersAnnotation GetAnnotation(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            SSAS.TabularDisplayFoldersAnnotation annotation = new SSAS.TabularDisplayFoldersAnnotation();
            if (sandbox.Database.Annotations.Contains(DISPLAY_FOLDER_ANNOTATION))
            {
                string xml = TabularHelpers.GetAnnotationXml(sandbox.Database, DISPLAY_FOLDER_ANNOTATION);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SSAS.TabularDisplayFoldersAnnotation));
                annotation = (SSAS.TabularDisplayFoldersAnnotation)serializer.Deserialize(new System.IO.StringReader(xml));
            }
            return annotation;
        }
#else
        public static SSAS.TabularDisplayFoldersAnnotation GetAnnotation(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
#if DENALI || SQL2014
            var sb = sandbox
#else
            var sb = (DataModelingSandboxAmo)sandbox.Impl;
#endif

            SSAS.TabularDisplayFoldersAnnotation annotation = new SSAS.TabularDisplayFoldersAnnotation();
            if (sb.Database.Annotations.Contains(DISPLAY_FOLDER_ANNOTATION))
            {
                string xml = TabularHelpers.GetAnnotationXml(sb.Database, DISPLAY_FOLDER_ANNOTATION);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SSAS.TabularDisplayFoldersAnnotation));
                annotation = (SSAS.TabularDisplayFoldersAnnotation)serializer.Deserialize(new System.IO.StringReader(xml));
            }
            return annotation;
        }
#endif

            #region ITabularOnPreBuildAnnotationCheck
        public TabularOnPreBuildAnnotationCheckPriority TabularOnPreBuildAnnotationCheckPriority
        {
            get
            {
                return TabularOnPreBuildAnnotationCheckPriority.HighPriority;
            }
        }
#if DENALI || SQL2014
        public string GetPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            string strWarning = "Click OK for BIDS Helper to restore display folders. The Tabular Display Folders form will open. Then click Yes then OK.";
            SSAS.TabularDisplayFoldersAnnotation annotation = GetAnnotation(sandbox);


            Database db = sandbox.Database;
            return GetPreBuildWarningInternal(strWarning, annotation, db);
        }
#else
        public string GetPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            string strWarning = "Click OK for BIDS Helper to restore display folders. The Tabular Display Folders form will open. Then click Yes then OK.";
            SSAS.TabularDisplayFoldersAnnotation annotation = GetAnnotation(sandbox);

#if DENALI || SQL2014
            Database db = sandbox.Database;
#else
            Database db = ((DataModelingSandboxAmo)sandbox.Impl).Database;
#endif
            return GetPreBuildWarningInternal(strWarning, annotation, db);
        }
#endif
        private static string GetPreBuildWarningInternal(string strWarning, SSAS.TabularDisplayFoldersAnnotation annotation, Database db)
        {
            foreach (Dimension d in db.Dimensions)
            {
                foreach (DimensionAttribute da in d.Attributes)
                {
                    if (da.Type != AttributeType.RowNumber && da.AttributeHierarchyVisible)
                    {
                        if (string.IsNullOrEmpty(da.AttributeHierarchyDisplayFolder) && annotation.Find(da) != null)
                        {
                            return strWarning;
                        }
                    }
                }
                foreach (Hierarchy h in d.Hierarchies)
                {
                    if (string.IsNullOrEmpty(h.DisplayFolder) && annotation.Find(h) != null)
                    {
                        return strWarning;
                    }
                }
            }
            foreach (Cube c in db.Cubes)
            {
                foreach (MdxScript mdx in c.MdxScripts)
                {
                    foreach (CalculationProperty calc in mdx.CalculationProperties)
                    {
                        if (calc.Visible && !calc.CalculationReference.StartsWith("KPIs.")) //TODO: display folder for KPIs have to be set inline in MDX script... do this in the future
                        {
                            if (string.IsNullOrEmpty(calc.DisplayFolder) && annotation.Find(calc) != null)
                            {
                                return strWarning;
                            }
                        }
                    }
                }
            }

            return null;
        }
#if DENALI || SQL2014
        public void FixPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            //open the actions form and let it fix actions
            ExecSandbox(sandbox);
        }
#else
        public void FixPreBuildWarning(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox)
        {
            //TODO - fix this method
            //open the actions form and let it fix actions
            //ExecSandbox(sandbox);
        }
#endif
        #endregion

        public override bool ShouldDisplayCommand()
        {
            var f = GetSelectedFile();
            if (f == null) return false;
            if (f.Extension.ToLower() != ".bim") return false;
#if DENALI || SQL2014
            return true;
#else
            var sb = TabularHelpers.GetTabularSandboxFromBimFile(this, false);
            if (sb == null) return false;
            if (sb.IsTabularMetadata) return false;
            return true;
#endif
        }
    }
}
