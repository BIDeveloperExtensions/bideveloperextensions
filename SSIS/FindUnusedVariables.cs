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
            processPackage.ProgressChanged += new ProgressChangedEventHandler(processPackage_ProgressChanged);
            processPackage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(processPackage_RunWorkerCompleted);
        }

        public void Show(Package package)
        {
            this.checkedListBox.Items.Clear();

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

            this.Show();
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

            if (!unusedVariablesList.Remove(match))
            {
                match = "::" + match;
                match = unusedVariablesList.Find(v => v.EndsWith(match));
                unusedVariablesList.Remove(match);
            }
            
        }

        #region BackgroundWorker Events

        private void processPackage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();
            this.Text = stopwatch.ElapsedMilliseconds.ToString();

            this.checkedListBox.Items.Clear();
            this.checkedListBox.Items.AddRange(this.unusedVariablesList.ToArray());
        }

        private void processPackage_DoWork(object sender, DoWorkEventArgs e)
        {
            stopwatch.Start();
            Variable[] variables = e.Argument as Variable[];
            finder.FindReferences(this.package, variables);
        }

        private void processPackage_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            VariableFoundEventArgs info = (VariableFoundEventArgs)e.UserState;
            //AddValue(info.Type, info.ContainerID, info.ObjectID, info.ObjectType, info.ObjectPath, info.ObjectName, info.PropertyName, info.Value, info.Icon, info.IsExpression);
        }
        #endregion

    }
}
