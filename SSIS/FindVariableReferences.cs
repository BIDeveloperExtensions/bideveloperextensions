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
        private System.Diagnostics.Stopwatch stopwatch;
        private FindVariables finder = new FindVariables();
        private Package package;
        private Variable variable;

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
            this.progressBar.Visible = true;

            this.package = package;
            this.variable = variable;

            InitializeTreeView();

            stopwatch = new System.Diagnostics.Stopwatch();
            processPackage.RunWorkerAsync();

            this.Show();
        }

        public void Show(Package package, Parameter parameter)
        {
            // Get the Variable object that is the same as the Parameter. A parameter is also an item in the Variables collection.
            Variable variable = package.Variables[parameter.ID];
            this.Show(package, variable);
        }

        private void InitializeTreeView()
        {
            this.treeView.Nodes.Clear();
            this.treeView.SuspendLayout();
            this.treeView.Enabled = false;
        }

        private void VariableFound(object sender, VariableFoundEventArgs e)
        {
            // Report variable found via BackGroundWorker to ensure we are thread safe when accessing the form control later on
            this.processPackage.ReportProgress(0, e);
        }

        private void PruneNodes(TreeNode parent)
        {
            for (int index = parent.Nodes.Count -1; index >= 0; index--)
            {
                TreeNode node = parent.Nodes[index];

                if (node.IsExpanded || node.Checked)
                {
                    PruneNodes(node);
                }
                else
                {
                    node.Remove();
                }
            }
        }

        #region BackgroundWorker Events

        private void processPackage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TreeNode parent = this.treeView.Nodes[0];
            PruneNodes(parent);

            stopwatch.Stop();
            this.Text += (" " + stopwatch.ElapsedMilliseconds.ToString());

            this.treeView.Enabled = true;            
            this.treeView.ResumeLayout();

            this.progressBar.Visible = false;
        }

        private void processPackage_DoWork(object sender, DoWorkEventArgs e)
        {
            stopwatch.Start();            
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
    }
}
