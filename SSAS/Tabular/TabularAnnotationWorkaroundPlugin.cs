using System;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.IO;
using BIDSHelper.Core;
using BIDSHelper.SSAS;

namespace BIDSHelper
{
    [FeatureCategory(BIDSFeatureCategories.SSASTabular)]
    public class TabularAnnotationWorkaroundPlugin : BIDSHelperPluginBase
    {
        
        private const string BACKUP_FILE_SUFFIX = ".BeforeBidsHelperAnnotationFix.bim";

        #region Standard Plugin Overrides
        public TabularAnnotationWorkaroundPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.TabularAnnotationsWorkaroundId);
        }

        public override string ShortName
        {
            get { return "TabularAnnotationWorkaroundPlugin"; }
        }

        //public override int Bitmap
        //{
        //    get { return 2116; }
        //}


        public override string FeatureName
        {
            get { return "Tabular Annotation Workaround"; }
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
            get { return "Provides an automated workaround for the ReadElementContentAs() bug related to BIDS Helper Annotations in SSDT."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ShouldDisplayCommand()
        {
#if DENALI
            try
            {
                if (TabularHelpers.GetTabularSandboxFromBimFile(this, false) == null)
                {
                    return true; //.bim file is closed so show this menu item
                }   
            }
            catch
            {
            }
#endif
            return false;
        }
        #endregion

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                ProjectItem projItem = (ProjectItem)hierItem.Object;
                string sFilePath = projItem.get_FileNames(0);
                string sFileName = projItem.Name + BACKUP_FILE_SUFFIX;

                SSAS.Tabular.TabularAnnotationWorkaroundForm form = new SSAS.Tabular.TabularAnnotationWorkaroundForm(sFileName);
                DialogResult res = form.ShowDialog();

                if (res == DialogResult.OK)
                {
                    FixAnnotations(sFilePath);
                    MessageBox.Show("BIDS Helper annotation format changed successfully!", "BIDS Helper Annotation Workaround");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        

        public static void FixAnnotations(string sBimFilePath)
        {
            //edit the .bim file as an XmlDocument since SSDT can't open it due to the bug https://connect.microsoft.com/SQLServer/feedback/details/776444/tabular-model-error-during-opening-bim-after-sp1-readelementcontentas-methods-cannot-be-called-on-an-element-that-has-child-elements
            XmlDocument doc = new XmlDocument();
            doc.Load(sBimFilePath);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("SSAS", doc.DocumentElement.NamespaceURI);

            if (doc.SelectSingleNode("//SSAS:ObjectDefinition/SSAS:Database/SSAS:Annotations/SSAS:Annotation/SSAS:Name[text()='" + TabularHelpers.ANNOTATION_STYLE_ANNOTATION + "']/../SSAS:Value[text()='" + TabularHelpers.ANNOTATION_STYLE_STRING + "']", nsmgr) != null)
            {
                throw new Exception("BIDS Helper has already switched annotations to the new format!!!");
            }

            bool bChangesMade = false;
            foreach (XmlNode nodeAnnotation in doc.SelectNodes("//SSAS:Annotations/SSAS:Annotation/SSAS:Name[text()='" + TabularDisplayFolderPlugin.DISPLAY_FOLDER_ANNOTATION + "' or text()='" + SSAS.TabularActionsEditorForm.ACTION_ANNOTATION + "' or text()='" + TabularHideMemberIfPlugin.HIDEMEMBERIF_ANNOTATION + "' or text()='" + TabularTranslationsEditorPlugin.TRANSLATIONS_ANNOTATION + "']/../SSAS:Value", nsmgr))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                StringBuilder sb = new StringBuilder();
                XmlWriter wr = XmlWriter.Create(sb, settings);
                XmlDocument annotationDoc = new XmlDocument();
                annotationDoc.LoadXml(nodeAnnotation.InnerXml);
                annotationDoc.Save(wr);
                wr.Close();

                nodeAnnotation.InnerText = sb.ToString();
                bChangesMade = true;
            }

            if (bChangesMade)
            {
                //insert new annotation saying we've switched to the CDATA annotation style
                XmlElement annotations = (XmlElement)doc.SelectSingleNode("//SSAS:ObjectDefinition/SSAS:Database/SSAS:Annotations", nsmgr);
                if (annotations == null) throw new Exception("No Database annotations exist yet!");
                XmlElement newAnnotation = doc.CreateElement("Annotation", doc.DocumentElement.NamespaceURI);
                newAnnotation.AppendChild(doc.CreateElement("Name", doc.DocumentElement.NamespaceURI)).InnerText = TabularHelpers.ANNOTATION_STYLE_ANNOTATION;
                newAnnotation.AppendChild(doc.CreateElement("Value", doc.DocumentElement.NamespaceURI)).InnerText = TabularHelpers.ANNOTATION_STYLE_STRING;
                annotations.PrependChild(newAnnotation);

                System.IO.File.Copy(sBimFilePath, sBimFilePath + BACKUP_FILE_SUFFIX, true);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                settings.Encoding = GetEncodingForFile(sBimFilePath);
                XmlWriter wr = XmlWriter.Create(sBimFilePath, settings);
                doc.Save(wr);
                wr.Close();
            }
            else
                throw new Exception("No changes made. No BIDS Helper features using annotations have been used yet.");
        }

        public static Encoding GetEncodingForFile(string filePath)
        {
            System.IO.FileStream file = new System.IO.FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read);
            if (file.CanSeek)
            {
                byte[] bom = new byte[4]; // Get the byte-order mark, if there is one
                file.Read(bom, 0, 4);

                foreach (EncodingInfo possibleEncInfo in Encoding.GetEncodings())
                {
                    Encoding possibleEnc = possibleEncInfo.GetEncoding();
                    int iPos = 0;
                    if (possibleEnc.GetPreamble().Length > 0)
                    {
                        bool bCorrect = true;
                        foreach (byte b in possibleEnc.GetPreamble())
                        {
                            if (b != bom[iPos++])
                            {
                                bCorrect = false;
                                break;
                            }
                        }
                        if (bCorrect)
                        {
                            file.Close();
                            return possibleEnc;
                        }
                    }
                }

                file.Close();
                return System.Text.Encoding.ASCII; //fallback to ANSI as the default
            }
            else
            {
                throw new Exception("Can't randomly access file " + filePath);
            }
        }

    }
}
