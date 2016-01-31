using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace BIDSHelper.SSIS
{
    // Using a partial class to help separate these methods, as easier to not include the filc:\users\darre\documents\visual studio 2015\Projects\CheckedListPlay\CheckedListPlay\SelectionList.cse in < SQL 2012 projects.
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
                dialog.Show(package, variable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
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

                FindUnusedVariables dialog = new FindUnusedVariables();
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
