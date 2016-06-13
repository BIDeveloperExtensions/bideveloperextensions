using BIDSHelper.Core;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BIDSHelper.SSIS
{
    public partial class FindVariableReferences : Form
    {
        private BackgroundWorker processPackage = null;
#if DEBUG
        private System.Diagnostics.Stopwatch stopwatch;
#endif
        private FindVariables finder = new FindVariables();
        private Package package;
        private Variable variable;
        private bool parameterMode;

        public FindVariableReferences()
        {
            InitializeComponent();

            processPackage = new BackgroundWorker();
            processPackage.WorkerReportsProgress = true;
            processPackage.WorkerSupportsCancellation = true;
            processPackage.DoWork += new DoWorkEventHandler(processPackage_DoWork);
            processPackage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(processPackage_RunWorkerCompleted);

            this.Icon = BIDSHelper.Resources.Versioned.VariableFindReferences;
        }

        public void Show(Package package, Variable variable)
        {
            if (parameterMode)
                this.Text = string.Format("Find parameter references - {0}", variable.QualifiedName);
            else
                this.Text = string.Format("Find variable references - {0}", variable.QualifiedName);

            this.package = package;
            this.variable = variable;

            this.progressBar.MarqueeAnimationSpeed = 20;
            this.progressBar.Visible = true;

            InitializeTreeView();
#if DEBUG
            stopwatch = new System.Diagnostics.Stopwatch();
#endif
            processPackage.RunWorkerAsync();

            this.Show();
        }

        public void Show(Package package, Parameter parameter)
        {
            parameterMode = true;

            // Get the Variable object that is the same as the Parameter. A parameter is also an item in the Variables collection.
            Variable variable = package.Variables[parameter.ID];
           
            this.Show(package, variable);
        }

        private void InitializeTreeView()
        {
            this.treeView.Nodes.Clear();
            this.treeView.Enabled = false;
        }

        private void VariableFound(object sender, VariableFoundEventArgs e)
        {
            // Report variable found via BackGroundWorker to ensure we are thread safe when accessing the form control later on
            this.processPackage.ReportProgress(0, e);
        }

        #region BackgroundWorker Events

        private void processPackage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //TreeNode parent = this.treeView.Nodes[0];

#if DEBUG
            stopwatch.Stop();
            this.Text += (" " + stopwatch.ElapsedMilliseconds.ToString());
#endif
            this.treeView.Enabled = true;            

            this.progressBar.Visible = false;
        }

        private void processPackage_DoWork(object sender, DoWorkEventArgs e)
        {
#if DEBUG
            stopwatch.Start();
#endif
            finder.FindReferences(this.package, this.variable, this.treeView);
        }
        #endregion

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            SetPropertyGrid(e.Node);
        }

        private void SetPropertyGrid(TreeNode node)
        {
            propertyGrid.SelectedObject = node.Tag;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
