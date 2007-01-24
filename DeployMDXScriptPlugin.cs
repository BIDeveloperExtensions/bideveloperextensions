using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Xml.Xsl;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;

// turns off the "missing XML Comment for publicly visible type or member..." compiler warnings
#pragma warning disable 1591


/// <summary>
/// 
/// </summary>
public class DeployMDXScriptPlugin : BIDSHelperPluginBase
{

    public override string ShortName
    {
        get { return "DeployMdxScript"; }
    }

    public override int Bitmap
    {
        get { return 2605; }
    }

    public override string ButtonText
    {
        get { return "Deploy MDX Script"; }
    }

    public override string ToolTip
    {
        get { return "Deploys just the MDX Script for this cube"; }
    }

    /// <summary>
    /// Determines if the command should be displayed or not.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public override bool DisplayCommand(UIHierarchyItem item)
    {
        if (item.Name.EndsWith(".cube")) 
            return true;
        else
            return false;
    }


	public override void Exec()
	{
		UIHierarchyItem hierItem;

		UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
		hierItem = (UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0);
		ProjectItem projItem;
		projItem = (ProjectItem)hierItem.Object;
		//\\ Add the following line to get a strongly typed cube object
		//Dim oCube As Microsoft.AnalysisServices.Cube = CType(projItem.Object, Microsoft.AnalysisServices.Cube)

		try {
			//\\ Save the project and cube
			//TODO - can I check and maybe prompt before saving?
			this.ApplicationObject.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
			ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 1, 5);

			//// Save the cube
			projItem.Save("");
			//dwItm.DTE.Solution.Projects.Item(1).Save()
			//dwitm.DTE.SelectedItems.

			//dwItm.ContainingProject.ProjectItems.Item(1).Document.Save()
			projItem.ContainingProject.ProjectItems.Item(1).Save("");

			//\\ Select the project
			//ctype(dwitm.DTE.Windows.Item(envdte.constants.vsext_wk_SProjectWindow).Object,UIhierarchy).UIHierarchyItems.Item(1).Select(vsUISelectionType.vsUISelectionTypeSelect)
			//DTE.ActiveWindow.Object.GetItem("Adventure Works DW").Select(vsUISelectionType.vsUISelectionTypeSelect)
			//DTE.ExecuteCommand("File.SaveSelectedItems")

			projItem.DTE.ExecuteCommand("File.SaveAll","");
			ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 2, 5);
			//dwItm.DTE.Solution.Projects.Item(1).Save()
			//dwItm.ContainingProject.ParentProjectItem

			//\\ extract deployment information
			DeploymentSettings deploySet = new DeploymentSettings(projItem);

			//\\ use xlst to create xmla alter command
			XslCompiledTransform xslt = new XslCompiledTransform();
			XmlReader xsltRdr;
			XmlReader xrdr;

			//\\ read xslt from embedded resource
            xsltRdr = XmlReader.Create(new StringReader(My.Resources.DeployMdxScript));
			using ((xsltRdr)) {
				//\\ read content from .cube file
				xrdr = XmlReader.Create(projItem.get_FileNames(1));
				using (xrdr) {
					//\\ Build up the Alter MdxScript command using XSLT against the .cube file
					XslCompiledTransform xslta = new XslCompiledTransform();
					StringBuilder sb = new StringBuilder();
					XmlWriterSettings xws = new XmlWriterSettings();
					xws.OmitXmlDeclaration = true;
					XmlWriter xwrtr = XmlWriter.Create(sb, xws);
					xslta.Load(xsltRdr);
					XsltArgumentList xslarg = new XsltArgumentList();
					xslarg.AddParam("TargetDatabase", "", deploySet.TargetDatabase);
					xslta.Transform(xrdr, xslarg, xwrtr);
					System.Diagnostics.Debug.Print(sb.ToString());

					ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 3, 5);
					//\\ Connect to Analysis Services
					Microsoft.AnalysisServices.Server svr = new Microsoft.AnalysisServices.Server();
					svr.Connect(deploySet.TargetServer);
					ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 4, 5);
					//\\ execute the xmla
					svr.Execute(sb.ToString());
					ApplicationObject.StatusBar.Progress(true, "Deploying MdxScript", 5, 5);
					//\\ report any results back (status bar?)

				}
			}
		}
		catch (System.Exception ex) {
			MessageBox.Show(ex.Message);
		}
		finally {
			ApplicationObject.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
			ApplicationObject.StatusBar.Progress(false, "Deploying MdxScript", 5, 5);
			//_addInInstance.DTE.StatusBar.Clear()
		}
	}

	//Public Sub AddCommand(ByVal appObject As DTE2, ByVal addin As EnvDTE.AddIn, ByVal cmdBars As Microsoft.VisualStudio.CommandBars.CommandBars) Implements IBIDSHelperPlugin.AddCommand
	//    Dim cmd As Command = appObject.Commands.AddNamedCommand(addin, ShortName, "Deploy MDX Script" _
	//                                    , "Deploys only the cubes MDX Script", True, 2605, Nothing _
	//                                    , CType(vsCommandStatus.vsCommandStatusSupported, Integer) + CType(vsCommandStatus.vsCommandStatusEnabled, Integer))
	//    'command = _applicationObject.Commands.AddNamedCommand(_addInInstance, "DeployMdxScript", "Deploy MDX Script" _
	//    '                , "Deploys just the MDX Script from the selected cube", True, 59, Nothing _
	//    '                , CType(vsCommandStatus.vsCommandStatusSupported, Integer) + CType(vsCommandStatus.vsCommandStatusEnabled, Integer))

	//    '133	green right arrow
	//    '317	page & blue down arrow
	//    '1591	yellow page and right blue arrow
	//    '1795
	//    '1924
	//    '2605

	//    itemCmdBar = cmdBars.Item("Item")
	//    If itemCmdBar Is Nothing Then
	//        MsgBox("Cannot get the Item menubar")
	//    End If
	//    cmd.AddControl(itemCmdBar)
	//    '_addInInstance.Connected = True
	//End Sub

}
