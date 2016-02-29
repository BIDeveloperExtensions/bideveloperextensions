using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows.Forms;

namespace BIDSHelper.SSIS
{
    // Using a partial class to help separate these methods, as easier to not include the file in SQL 2012+ projects.
    public partial class VariablesWindowPlugin
    {
        private void FindReferencesButtonClick()
        {
            try
            {
#if DENALI || SQL2014
                packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().GetProperty("PackageDesigner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance).GetValue(variablesToolWindowControl, null);
#else
                packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().InvokeMember("PackageDesigner", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, null);
#endif

                if (packageDesigner == null) return;

                Package package = packageDesigner.Component as Package;
                if (package == null) return;

                int selectedRow;
                int selectedCol;
                grid.GetSelectedCell(out selectedRow, out selectedCol);

                if (selectedRow < 0) return;

                Variable variable = GetVariableForRow(selectedRow);

                if (variable == null) return;

                FindReferences dialog = new FindReferences();
                dialog.EditExpressionSelected += new EventHandler<EditExpressionSelectedEventArgs>(findReferences_EditExpressionSelected);

                dialog.Show(package, variable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
        }

        private void findReferences_EditExpressionSelected(object sender, EditExpressionSelectedEventArgs e)
        {
            try
            {
                Package package = null;
                DtsContainer container = null;

                //if (win == null)
                //{
                //    return;
                //}

                try
                {
                    package = GetCurrentPackage();
                    if (package == null)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Assert(false, ex.ToString());
                    return;
                }

                // Parameters for Expression Editor
                Variables variables = null;
                VariableDispenser variableDispenser = null;
                string propertyName = string.Empty;
                Type propertyType = null;

                // Target objects
                IDTSPropertiesProvider propertiesProvider = null;
                Variable variable = null;
                PrecedenceConstraint constraint = null;

                // Get the container
                container = SSISHelpers.FindContainer(package, e.ContainerID);

                // Get the property details and variable objects for the editor
                if (e.Type == typeof(Variable))
                {
                    variable = SSISHelpers.FindVariable(container, e.ObjectID);

                    propertyName = "Value";
                    propertyType = System.Type.GetType("System." + variable.DataType.ToString());

                    variables = container.Variables;
                    variableDispenser = container.VariableDispenser;
                }
                else if (e.Type == typeof(PrecedenceConstraint))
                {
                    constraint = SSISHelpers.FindConstraint(container, e.ObjectID);

                    propertyName = "Expression";
                    propertyType = typeof(bool);

                    variables = container.Variables;
                    variableDispenser = container.VariableDispenser;
                }
                else
                {
                    if (e.Type == typeof(ConnectionManager))
                    {
                        propertiesProvider = FindConnectionManager(package, e.ObjectID) as IDTSPropertiesProvider;
                    }
                    else if (e.Type == typeof(ForEachEnumerator))
                    {
                        ForEachLoop forEachLoop = container as ForEachLoop;
                        propertiesProvider = forEachLoop.ForEachEnumerator as IDTSPropertiesProvider;
                    }
                    else
                    {
                        propertiesProvider = container as IDTSPropertiesProvider;
                    }

                    if (propertiesProvider != null)
                    {
                        DtsProperty property = propertiesProvider.Properties[e.Property];
                        propertyName = property.Name;
                        propertyType = PackageHelper.GetTypeFromTypeCode(property.Type);
                        variables = container.Variables;
                        variableDispenser = container.VariableDispenser;
                    }
                    else
                    {
                        throw new Exception(string.Format(CultureInfo.InvariantCulture, "Expression editing not supported on this object ({0}).", e.ObjectID));
                    }
                }

                // Show the editor
                Konesans.Dts.ExpressionEditor.ExpressionEditorPublic editor = new Konesans.Dts.ExpressionEditor.ExpressionEditorPublic(variables, variableDispenser, propertyType, propertyName, e.Expression);
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    // Get expression
                    string expression = editor.Expression;
                    if (expression == null || string.IsNullOrEmpty(expression.Trim()))
                    {
                        expression = null;
                    }

                    // Set the new expression on the target object
                    object objectChanged = null;
                    if (variable != null)
                    {
                        if (expression == null)
                        {
                            variable.EvaluateAsExpression = false;
                        }

                        variable.Expression = expression;
                        objectChanged = variable;
                    }
                    else if (constraint != null)
                    {
                        if (expression == null)
                        {
                            constraint.EvalOp = DTSPrecedenceEvalOp.Constraint;
                        }

                        constraint.Expression = expression;
                        objectChanged = constraint;
                    }
                    else if (propertiesProvider != null)
                    {
                        // TaskHost, Sequence, ForLoop, ForEachLoop and ConnectionManager
                        propertiesProvider.SetExpression(e.Property, expression);
                        objectChanged = propertiesProvider;
                    }

                    //TODO ! -- expressionListWindow_RefreshExpressions(null, null);

                    // Finish displaying expressions list before you mark the package 
                    // as dirty (which runs the expression highlighter)
                    System.Windows.Forms.Application.DoEvents();

                    SetPackageAsDirty(package, expression, objectChanged);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetPackageAsDirty(IDTSSequence container, string expression, object objectChanged)
        {
            //try
            //{
                //shouldSkipExpressionHighlighting = true; //this flag is used by the expression highlighter to skip re-highlighting if all that's changed is the string of an existing expression... if one has been removed, then re-highlight

                //PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(objectChanged);
                //System.ComponentModel.PropertyDescriptor expressionsProperty = properties.Find("Expressions", false);

                //// Mark package object as dirty
                //IComponentChangeService changeService = (IComponentChangeService)designer.GetService(typeof(IComponentChangeService));
                //if (objectChanged == null)
                //{
                //    changeService.OnComponentChanging(container, null);
                //    changeService.OnComponentChanged(container, null, null, null); //marks the package designer as dirty
                //}
                //else
                //{
                //    changeService.OnComponentChanging(objectChanged, expressionsProperty);
                //    changeService.OnComponentChanged(objectChanged, expressionsProperty, null, null); //marks the package designer as dirty
                //}
                //if (container is Package)
                //{
                    SSISHelpers.MarkPackageDirty((Package)container);
                //}
            //}
            //finally
            //{
            //    shouldSkipExpressionHighlighting = false;
            //}
        }


        private ConnectionManager FindConnectionManager(Package package, string objectID)
        {
            // Copied from ExpressionListPlugin
            if (package.Connections.Contains(objectID))
            {
                return package.Connections[objectID];
            }

            return null;
        }

        private void FindUnusedButtonClick()
        {
            try
            {
#if DENALI || SQL2014
                packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().GetProperty("PackageDesigner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance).GetValue(variablesToolWindowControl, null);
#else
                packageDesigner = (ComponentDesigner)variablesToolWindowControl.GetType().InvokeMember("PackageDesigner", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, null);
#endif

                if (packageDesigner == null) return;

                Package package = packageDesigner.Component as Package;
                if (package == null) return;

                FindUnusedVariables dialog = new FindUnusedVariables(VariablesDisplayMode.Variables);
                if (dialog.Show(package) == DialogResult.OK)
                {
                    // Dialog result OK indicates we have deleted one or more variables
                    // Flag package as dirty
                    SSISHelpers.MarkPackageDirty(package);

                    // Refresh the grid
                    variablesToolWindowControl.GetType().InvokeMember("FillGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance, null, variablesToolWindowControl, new object[] { });
                    SetButtonEnabled();
                    RefreshHighlights();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
        }
    }
}
