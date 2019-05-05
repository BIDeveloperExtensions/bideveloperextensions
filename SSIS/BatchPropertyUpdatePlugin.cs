using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using System;
using Microsoft.SqlServer.Dts.Runtime;
using wrap = Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using BIDSHelper.Core;

namespace BIDSHelper.SSIS
{
    /// <summary>
    /// SSIS plugin that supports updating a property across multiple packages
    /// </summary>
    [FeatureCategory(BIDSFeatureCategories.SSIS)]
    public class BatchPropertyUpdatePlugin : BIDSHelperPluginBase
    {
        private IComponentChangeService changesvc;

        public BatchPropertyUpdatePlugin(BIDSHelperPackage package)
            : base(package)
        {
            CreateContextMenu(CommandList.BatchPropertyUpdateId);
        }

        public override string ShortName
        {
            get { return "BatchPropertyUpdatePlugin"; }
        }

        //public override int Bitmap
        //{
        //    get { return 313; }
        //}

        public override string FeatureName
        {
            get { return "Batch Property Update"; }
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
            get { return "Find and Replace for properties across multiple packages. Update property values using the /SET syntax."; }
        }

        public override bool ShouldDisplayCommand()
        {
            //return false;

            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            if (((System.Array)solExplorer.SelectedItems).Length == 1)
            {
                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));

                string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                return (sFileName.EndsWith(".dtsx"));
            }
            else
            {
                foreach (object selected in ((System.Array)solExplorer.SelectedItems))
                {
                    UIHierarchyItem hierItem = (UIHierarchyItem)selected;
                    string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
                    if (!sFileName.EndsWith(".dtsx")) return false;
                }
                return (((System.Array)solExplorer.SelectedItems).Length > 0);
            }

        }

        public override void Exec()
        {

            try
            {
                string propertyPath;
                string newValue;

                //Get PropertyPath values
                BatchPropertyUpdateForm frm = new BatchPropertyUpdateForm();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    propertyPath = frm.PropertyPath;
                    newValue = frm.NewValue;
                }
                else
                {
                    return;
                }

                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;

                foreach (UIHierarchyItem hi in ((System.Array)solExplorer.SelectedItems))
                {
                    ProjectItem pi = (ProjectItem)hi.Object;

                    Window w = pi.Open(BIDSViewKinds.Designer); //opens the designer
                    w.Activate();

                    IDesignerHost designer = w.Object as IDesignerHost;
                    if (designer == null) continue;
                    changesvc = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));


                    EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                    Package package = win.PropertiesLinkComponent as Package;
                    if (package == null) continue;
                    SetPropertyValue(package, propertyPath, newValue);

                    SSISHelpers.MarkPackageDirty(package); //for now always mark it as dirty

                    //ApplicationObject.ActiveDocument.Save(null);
                    //w.Close(vsSaveChanges.vsSaveChangesYes);
                    //w.Close(vsSaveChanges.vsSaveChangesNo); //close the designer
                    //w = pi.Open(BIDSViewKinds.Designer); //opens the designer
                    w.Activate(); //that was the quick and easy way to get the expression highlighter up to date
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private object SetPropertyValue(DtsObject dtsObject, string propertyPath, object value)
        {
            propertyPath = propertyPath.Replace("\\", ".");
            object returnValue = null;
            string firstPart = propertyPath;
            string restOfString = string.Empty;

            if (propertyPath.Contains("."))
            {
                //Can have periods in object names (like connection manager names)
                //Need to verify that period is not between an index marker
                int delimiterIndex = propertyPath.IndexOf(".");
                //while (delimiterIndex > propertyPath.IndexOf("[") &&
                //    delimiterIndex < propertyPath.IndexOf("]"))
                //{
                //    delimiterIndex = propertyPath.IndexOf(".", delimiterIndex + 1 );
                //}
                if (delimiterIndex > propertyPath.IndexOf("[") &&
                    delimiterIndex < propertyPath.IndexOf("]"))
                {
                    delimiterIndex = propertyPath.IndexOf(".", propertyPath.IndexOf("]"));
                }

                if (delimiterIndex > -1)
                {
                    firstPart = propertyPath.Substring(0, delimiterIndex);
                    restOfString = propertyPath.Substring(delimiterIndex + 1, (propertyPath.Length - (delimiterIndex + 1)));
                    if (firstPart.Length == 0)
                    {
                        return SetPropertyValue(dtsObject, restOfString, value);
                    }
                }
            }


            if (firstPart.ToUpper().StartsWith("PACKAGE"))
            {
                if (!(dtsObject is Package))
                {
                    throw new ArgumentException("The initial object must be of type Package.", "dtsObject");
                }
                return SetPropertyValue(dtsObject, restOfString, value);
            }

            //    \Package.Variables[User::TestVar].Properties[Value]
            if (firstPart.ToUpper().StartsWith("VARIABLES"))
            {
                if (!(dtsObject is DtsContainer))
                {
                    throw new ArgumentException("Object must be of type DtsContainer to reference variables.", "dtsObject");
                }
                Variables vars = null;
                string varName = GetSubStringBetween(firstPart, "[", "]");

                DtsContainer cont = (DtsContainer)dtsObject;
                cont.VariableDispenser.LockOneForRead(varName, ref vars);
                returnValue = SetPropertyValue(vars[varName], restOfString, value);
                vars.Unlock();
                return returnValue;
            }

            //    \Package.Properties[CreationDate]
            if (firstPart.ToUpper().StartsWith("PROPERTIES"))
            {
                if (!(dtsObject is IDTSPropertiesProvider))
                {
                    throw new ArgumentException("Object must be of type IDTSPropertiesProvider to reference properties.", "dtsObject");
                }
                IDTSPropertiesProvider propProv = (IDTSPropertiesProvider)dtsObject;
                string propIndex = GetSubStringBetween(firstPart, "[", "]");

                DtsProperty prop = propProv.Properties[propIndex];
                if (dtsObject is Variable && prop.Name == "Value")
                {
                    Variable var = (Variable)dtsObject;
                    prop.SetValue(dtsObject, Convert.ChangeType(value, var.DataType));
                }
                else
                {
                    prop.SetValue(dtsObject, Convert.ChangeType(value, propProv.Properties[propIndex].Type));
                }

                //Flag value as changing
                changesvc.OnComponentChanging(prop, null);
                changesvc.OnComponentChanged(prop, null, null, null); //marks the package designer as dirty
                
                return prop.GetValue(dtsObject);
            }

            //    \Package.Connections[localhost.AdventureWorksDW2008].Properties[Description]
            if (firstPart.ToUpper().StartsWith("CONNECTIONS"))
            {
                if (!(dtsObject is Package))
                {
                    throw new ArgumentException("Object must be of type Package to reference Connections.", "dtsObject");
                }
                string connIndex = GetSubStringBetween(firstPart, "[", "]");
                Package pkg = (Package)dtsObject;
                return SetPropertyValue(pkg.Connections[connIndex], restOfString, value);
            }

            //    \Package.EventHandlers[OnError].Properties[Description]
            if (firstPart.ToUpper().StartsWith("EVENTHANDLERS"))
            {
                if (!(dtsObject is EventsProvider))
                {
                    throw new ArgumentException("Object must be of type EventsProvider to reference events.", "dtsObject");
                }
                EventsProvider eventProvider = (EventsProvider)dtsObject;
                string eventIndex = GetSubStringBetween(firstPart, "[", "]");
                return SetPropertyValue(eventProvider.EventHandlers[eventIndex], restOfString, value);
            }

            //First Part of string is not one of the hard-coded values - it's either a task or container
            if (!(dtsObject is IDTSSequence))
            {
                throw new ArgumentException("Object must be of type IDTSSequence to reference other tasks or containers.", "dtsObject");
            }

            IDTSSequence seq = (IDTSSequence)dtsObject;
            if (seq.Executables.Contains(firstPart))
            {
                return SetPropertyValue(seq.Executables[firstPart], restOfString, value);
            }


            //            \Package\Sequence Container\Script Task.Properties[Description]
            //    \Package\Sequence Container.Properties[Description]
            //    \Package\Execute SQL Task.Properties[Description]

            //\Package.EventHandlers[OnError].Variables[System::Cancel].Properties[Value]
            //    \Package.EventHandlers[OnError]\Script Task.Properties[Description]


            if (restOfString.Length > 0)
            {
                returnValue = SetPropertyValue(dtsObject, restOfString, value);
            }

            return returnValue;
        }

        private static string GetSubStringBetween(string stringToParse, string startString, string endString)
        {
            string subString;
            int startPosition = stringToParse.IndexOf(startString) + 1;
            int endPosition = stringToParse.IndexOf(endString);
            subString = stringToParse.Substring(startPosition, endPosition - startPosition);
            return subString;
        }
    }
}