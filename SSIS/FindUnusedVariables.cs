using BIDSHelper.Core;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace BIDSHelper.SSIS
{
    public partial class FindUnusedVariables : Form
    {
        private BackgroundWorker processPackage = null;
        private System.Diagnostics.Stopwatch stopwatch;
        private FindVariables finder = new FindVariables();
        private Package package;
        private List<string> unusedVariablesList;
        
        public FindUnusedVariables(VariablesDisplayMode displayMode)
        {
            this.DisplayMode = displayMode;

            InitializeComponent();
            processPackage = new BackgroundWorker();
            processPackage.WorkerReportsProgress = true;
            processPackage.WorkerSupportsCancellation = true;
            processPackage.DoWork += new DoWorkEventHandler(processPackage_DoWork);
            processPackage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(processPackage_RunWorkerCompleted);

            if (displayMode == VariablesDisplayMode.Variables)
            {
                selectionList.SelectionChanged += new EventHandler<SelectionListSelectionChangedEventArgs>(selectionList_SelectionChanged);                
            }
            else
            {
                // Hide delete buttton, we don't support it for parameters (yet)
                this.buttonDelete.Enabled = false;
                this.buttonDelete.Visible = false;

                selectionList.SelectionEnabled = false;
                
                // Change title
                this.Text = "Find unused parameters";
            }

            this.Icon = BIDSHelper.Resources.Versioned.VariableFindUnused;
        }

        public VariablesDisplayMode DisplayMode { get; private set; }

        public DialogResult Show(Package package)
        {
            this.selectionList.ClearItems();
            this.progressBar.Visible = true;

            finder.VariableFound += new EventHandler<VariableFoundEventArgs>(VariableFound);

            this.package = package;

            // Add all variables to a list, they will get removed later if found.
            unusedVariablesList = new List<string>();
            List<Variable> variables = new List<Variable>();
            foreach (Variable variable in package.Variables)
            {
                if (DisplayMode == VariablesDisplayMode.Variables)
                {
                    // Exclude system variables, they cannot be removed so no point reporting them as unused
                    // Exclude Project and Package parameters when in Variables mode
                    if (!variable.SystemVariable && variable.Namespace != "$Project" && variable.Namespace != "$Package")
                    {
                        unusedVariablesList.Add(variable.QualifiedName);
                        variables.Add(variable);
                    }
                }
                else
                {
                    // Restrict to parameters, for now both types
                    if (variable.Namespace == "$Project" || variable.Namespace == "$Package")
                    {
                        unusedVariablesList.Add(variable.QualifiedName);
                        variables.Add(variable);
                    }
                }
            }

            stopwatch = new System.Diagnostics.Stopwatch();
            processPackage.RunWorkerAsync(variables.ToArray());

            return this.ShowDialog();
        }
        private void VariableFound(object sender, VariableFoundEventArgs e)
        {
            string match = e.Match;
            if (match.StartsWith("@"))
            {
                match = match.Substring(1);
            }
            if (match.StartsWith("["))
            {
                match = match.Substring(1, match.Length - 2);
            }

            // Remove it from our simpel list, for qualified names
            if (unusedVariablesList.Remove(match))
                return;

            // Try unqualified matches, just check for variables ending in "::VariableName", skipping the namespace
            match = "::" + match;
            match = unusedVariablesList.Find(v => v.EndsWith(match));
            unusedVariablesList.Remove(match);
        }

        #region BackgroundWorker Events

        private void processPackage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.progressBar.Visible = false;
            stopwatch.Stop();
#if DEBUG
            this.Text = string.Format("{0} ({1})", this.Text, stopwatch.ElapsedMilliseconds);
#endif
            this.selectionList.ClearItems();
            this.selectionList.AddRange(this.unusedVariablesList.ToArray());
        }

        private void processPackage_DoWork(object sender, DoWorkEventArgs e)
        {
            stopwatch.Start();

            Variable[] variables = e.Argument as Variable[];
            finder.FindReferences(this.package, variables);
        }
        #endregion

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            foreach (string variable in this.selectionList.CheckedItems)
            {
                package.Variables.Remove(variable);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void selectionList_SelectionChanged(object sender, SelectionListSelectionChangedEventArgs e)
        {
            this.buttonDelete.Enabled = (e.SelectedItems > 0);
        }
    }
}
