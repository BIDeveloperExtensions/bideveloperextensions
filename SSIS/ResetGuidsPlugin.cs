using EnvDTE;
using EnvDTE80;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using System;
using Microsoft.SqlServer.Dts.Runtime;
using System.Collections.Generic;
using BIDSHelper.Core;

namespace BIDSHelper.SSIS
{
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class ResetGuidsPlugin : BIDSHelperPluginBase
    {
        public ResetGuidsPlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.ResetGUIDsId, ".dtsx");
        }

        public override string ShortName
        {
            get { return "ResetGuidsPlugin"; }
        }

        //public override int Bitmap
        //{
        //    get { return 313; }
        //}

        public override string FeatureName
        {
            get { return "Reset GUIDs"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }


        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSIS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Reset the IDs for all tasks, connection managers, configurations, event handlers, variables, and the package ID itself. Ensures that all IDs in the current package are unique."; }
        }

        //public override bool DisplayCommand(UIHierarchyItem item)
        //{
        //    if (ApplicationObject.Mode == vsIDEMode.vsIDEModeDebug) return false;
        //    UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
        //    if (((System.Array)solExplorer.SelectedItems).Length != 1)
        //        return false;

        //    UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
        //    string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
        //    return (sFileName.EndsWith(".dtsx"));
        //}


        private List<string> _listGuidsToReplace;

        public override void Exec()
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                ProjectItem pi = (ProjectItem)hierItem.Object;

                if (System.Windows.Forms.MessageBox.Show("Are you sure you want to reset the GUIDs for all tasks, connection managers, configurations,\r\nevent handlers, and variables in package " + pi.Name + "?", "BIDS Helper - Reset GUIDs?", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) return;

                Window w = pi.Open(BIDSViewKinds.Designer); //opens the designer
                w.Activate();

                IDesignerHost designer = w.Object as IDesignerHost;
                if (designer == null) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                Package package = win.PropertiesLinkComponent as Package;
                if (package == null) return;


                //capture list of GUIDs to replace
                _listGuidsToReplace = new List<string>();
                AddGuid(package.ID);
                RecurseExecutablesAndCaptureGuids(package);
                foreach (DtsEventHandler h in package.EventHandlers)
                {
                    AddGuid(h.ID);
                    RecurseExecutablesAndCaptureGuids(h);
                }
                foreach (ConnectionManager cm in package.Connections)
                {
                    AddGuid(cm.ID);
                }
                foreach (Microsoft.SqlServer.Dts.Runtime.Configuration conf in package.Configurations)
                {
                    AddGuid(conf.ID);
                }

                //open package XML 
                ApplicationObject.ExecuteCommand("View.ViewCode", String.Empty);

                // Loop through GUIDs captured and replace
                foreach (string sOldGuid in _listGuidsToReplace)
                {

                    ApplicationObject.Find.FindWhat = sOldGuid;
                    ApplicationObject.Find.ReplaceWith = "{" + System.Guid.NewGuid().ToString().ToUpper() + "}";
                    ApplicationObject.Find.Target = vsFindTarget.vsFindTargetCurrentDocument;
                    ApplicationObject.Find.MatchCase = false;
                    ApplicationObject.Find.MatchWholeWord = true;
                    ApplicationObject.Find.MatchInHiddenText = false;
                    ApplicationObject.Find.Action = vsFindAction.vsFindActionReplaceAll;
                    ApplicationObject.Find.PatternSyntax = vsFindPatternSyntax.vsFindPatternSyntaxLiteral;

                    if (ApplicationObject.Find.Execute() == vsFindResult.vsFindResultNotFound)
                    {
                        System.Diagnostics.Debug.WriteLine("couldn't find " + sOldGuid);
                        System.Windows.Forms.MessageBox.Show("Resetting GUIDs did NOT complete successfully as the package has changed.\r\n\r\nThis may have happened if you did not have the latest version from source control before running the Reset GUIDs command.", "BIDS Helper - Problem Resetting GUIDs");
                        return;
                    }
                }

                ApplicationObject.ActiveWindow.Close(vsSaveChanges.vsSaveChangesNo); //close the package XML
                ApplicationObject.ActiveDocument.Save(null);
                w.Close(vsSaveChanges.vsSaveChangesNo); //close the designer

                w = pi.Open(BIDSViewKinds.Designer); //opens the designer
                w.Activate();
                //that was the quick and easy way to get the expression highlighter up to date
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void RecurseExecutablesAndCaptureGuids(IDTSSequence parentExecutable)
        {
            foreach (Variable v in ((DtsContainer)parentExecutable).Variables)
            {
                //don't replace system variables since they're not in the XML
                //don't replace parameters which show as variables since they have a different ID and you won't find the variable's ID in the XML to replace
                if (!v.SystemVariable && !SSISHelpers.IsParameterVariable(v) )
                {
                    AddGuid(v.ID);
                }
            }
            foreach (Executable e in parentExecutable.Executables)
            {
                AddGuid(((DtsContainer)e).ID);
                if (e is IDTSSequence)
                {
                    RecurseExecutablesAndCaptureGuids((IDTSSequence)e);
                }
            }
        }

        private void AddGuid(string guid)
        {
            if (!string.IsNullOrEmpty(guid) && !_listGuidsToReplace.Contains(guid))
                _listGuidsToReplace.Add(guid);
        }
    }
}