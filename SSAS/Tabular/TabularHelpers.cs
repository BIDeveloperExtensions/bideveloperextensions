using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;

namespace BIDSHelper
{
    public class TabularHelpers
    {
        public static Microsoft.AnalysisServices.BackEnd.DataModelingSandbox GetTabularSandboxFromBimFile(UIHierarchyItem hierItem, bool openIfNotOpen)
        {
            Microsoft.VisualStudio.Project.Automation.OAFileItem project = (Microsoft.VisualStudio.Project.Automation.OAFileItem)hierItem.Object;
            if (openIfNotOpen && !project.get_IsOpen(EnvDTE.Constants.vsViewKindPrimary))
            {
                Window win = project.Open(EnvDTE.Constants.vsViewKindPrimary);
                if (win == null) throw new Exception("BIDS Helper was unable to open designer window.");
                win.Activate();
            }

            Microsoft.AnalysisServices.VSHost.Integration.BISMFileNode bim = (Microsoft.AnalysisServices.VSHost.Integration.BISMFileNode)project.Object;
            Microsoft.AnalysisServices.VSHost.VSHostManager host = (Microsoft.AnalysisServices.VSHost.VSHostManager)bim.GetType().InvokeMember("VSHostManager", System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, bim, null); //bim.VSHostManager;
            return host.Sandbox;
        }

        public static void SaveXmlAnnotation(MajorObject obj, string annotationName, object annotationValue)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(annotationValue.GetType());
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter writer = new System.IO.StringWriter(sb);
            serializer.Serialize(writer, annotationValue);
            System.Xml.XmlDocument xml = new XmlDocument();
            xml.LoadXml(sb.ToString());
            if (obj.Annotations.Contains(SSAS.TabularActionsEditorForm.ACTION_ANNOTATION)) obj.Annotations.Remove(SSAS.TabularActionsEditorForm.ACTION_ANNOTATION);
            Annotation annotation = new Annotation(SSAS.TabularActionsEditorForm.ACTION_ANNOTATION, xml.DocumentElement);
            obj.Annotations.Add(annotation);
        }
    }
}

