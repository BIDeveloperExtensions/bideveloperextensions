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
        private static Microsoft.AnalysisServices.VSHost.VSHostManager GetVSHostManager(UIHierarchyItem hierItem, bool openIfNotOpen)
        {
            Microsoft.VisualStudio.Project.Automation.OAFileItem project = hierItem.Object as Microsoft.VisualStudio.Project.Automation.OAFileItem;
            if (project == null) return null;
            if (openIfNotOpen && !project.get_IsOpen(EnvDTE.Constants.vsViewKindPrimary))
            {
                Window win = project.Open(EnvDTE.Constants.vsViewKindPrimary);
                if (win == null) throw new Exception("BIDS Helper was unable to open designer window.");
                win.Activate();
            }

            Microsoft.AnalysisServices.VSHost.Integration.BISMFileNode bim = (Microsoft.AnalysisServices.VSHost.Integration.BISMFileNode)project.Object;
            Microsoft.AnalysisServices.VSHost.VSHostManager host = (Microsoft.AnalysisServices.VSHost.VSHostManager)bim.GetType().InvokeMember("VSHostManager", System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, bim, null); //bim.VSHostManager;
            return host;
        }

        public static Microsoft.AnalysisServices.BackEnd.DataModelingSandbox GetTabularSandboxFromBimFile(UIHierarchyItem hierItem, bool openIfNotOpen)
        {
            Microsoft.AnalysisServices.VSHost.VSHostManager host = GetVSHostManager(hierItem, openIfNotOpen);
            if (host == null) return null;
            return host.Sandbox;
        }

        public static Microsoft.AnalysisServices.Common.SandboxEditor GetTabularSandboxEditorFromBimFile(UIHierarchyItem hierItem, bool openIfNotOpen)
        {
            Microsoft.AnalysisServices.VSHost.VSHostManager host = GetVSHostManager(hierItem, openIfNotOpen);
            if (host == null) return null;
            return host.Editor;
        }

        public static void SaveXmlAnnotation(MajorObject obj, string annotationName, object annotationValue)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(annotationValue.GetType());
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter writer = new System.IO.StringWriter(sb);
            serializer.Serialize(writer, annotationValue);
            System.Xml.XmlDocument xml = new XmlDocument();
            xml.LoadXml(sb.ToString());
            if (obj.Annotations.Contains(annotationName)) obj.Annotations.Remove(annotationName);
            Annotation annotation = new Annotation(annotationName, xml.DocumentElement);
            obj.Annotations.Add(annotation);
        }
    }
}

