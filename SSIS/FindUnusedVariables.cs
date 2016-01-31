using BIDSHelper.Core;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public FindUnusedVariables()
        {
            InitializeComponent();
            processPackage = new BackgroundWorker();
            processPackage.WorkerReportsProgress = true;
            processPackage.WorkerSupportsCancellation = true;
            processPackage.DoWork += new DoWorkEventHandler(processPackage_DoWork);
            processPackage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(processPackage_RunWorkerCompleted);

            selectionList.SelectionChanged += new EventHandler<SelectionListSelectionChangedEventArgs>(selectionList_SelectionChanged);
        }

        public DialogResult Show(Package package)
        {
            this.selectionList.ClearItems();

            finder.VariableFound += new EventHandler<VariableFoundEventArgs>(VariableFound);

            this.package = package;

            // Add all variables to a list, they will get removed later if found.
            unusedVariablesList = new List<string>();
            List<Variable> variables = new List<Variable>();
            foreach (Variable variable in package.Variables)
            {
                if (!variable.SystemVariable)
                {
                    unusedVariablesList.Add(variable.QualifiedName);
                    variables.Add(variable);
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
            stopwatch.Stop();
            this.Text += (" " + stopwatch.ElapsedMilliseconds.ToString());

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
            foreach (string variable in this.selectionList.SelectedItems)
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
