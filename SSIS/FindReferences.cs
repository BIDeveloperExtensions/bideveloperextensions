using BIDSHelper.Core;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BIDSHelper.SSIS
{
    public partial class FindReferences : Form
    {
        private BackgroundWorker processPackage = null;
        private System.Diagnostics.Stopwatch stopwatch;
        private FindVariables finder = new FindVariables();
        private Package package;
        private Variable variable;

        public event EventHandler<EditExpressionSelectedEventArgs> EditExpressionSelected;

        public FindReferences()
        {
            InitializeComponent();

            // Hide internal columns - Do it here as easy to make visible when debugging
            this.expressionGrid.Columns[0].Visible = false;
            this.expressionGrid.Columns[1].Visible = false;
            this.expressionGrid.Columns[4].Visible = false;
            this.expressionGrid.Columns[5].Visible = false;
            this.expressionGrid.Columns[0].Width = 0;
            this.expressionGrid.Columns[1].Width = 0;
            this.expressionGrid.Columns[4].Width = 0;
            this.expressionGrid.Columns[5].Width = 0;

            processPackage = new BackgroundWorker();
            processPackage.WorkerReportsProgress = true;
            processPackage.WorkerSupportsCancellation = true;
            processPackage.DoWork += new DoWorkEventHandler(processPackage_DoWork);
            processPackage.ProgressChanged += new ProgressChangedEventHandler(processPackage_ProgressChanged);
            processPackage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(processPackage_RunWorkerCompleted);

            expressionGrid.CellContentClick += new DataGridViewCellEventHandler(expressionGrid_CellContentClick);

            this.Icon = BIDSHelper.Resources.Versioned.VariableFindReferences;
        }

        public void Show(Package package, Variable variable)
        {

            finder.VariableFound += new EventHandler<VariableFoundEventArgs>(VariableFound);

            this.package = package;
            this.variable = variable;

            stopwatch = new System.Diagnostics.Stopwatch();
            processPackage.RunWorkerAsync();

            this.Show();
        }

        public void Show(Package package, Parameter parameter)
        {

            finder.VariableFound += new EventHandler<VariableFoundEventArgs>(VariableFound);

            this.package = package;

            // Get the Variable object that is the same as the Parameter. A parameter is also an item in the Variables collection.
            this.variable = package.Variables[parameter.ID];

            stopwatch = new System.Diagnostics.Stopwatch();
            processPackage.RunWorkerAsync();

            this.Show();
        }

        public void AddValue(Type type, string containerID, string objectID, string objectType, string objectPath, string objectName, string propertyName, string value, Icon icon, bool isExpression)
        {
            // containerID, objectID, objectName are hidden columns.
            string[] newRow = { containerID, objectID, objectType, objectPath, objectName, propertyName, value };
            int index = expressionGrid.Rows.Add(newRow);
            DataGridViewRow row = expressionGrid.Rows[index];
            row.Tag = type;
            row.Cells["ObjectType"].Tag = icon;

            if (!isExpression)
            {
                DataGridViewButtonDisableCell buttonCell = (DataGridViewButtonDisableCell)row.Cells["EditorColumn"];
                buttonCell.Enabled = false;
            }
        }

        private void VariableFound(object sender, VariableFoundEventArgs e)
        {
            // Report variable found via BackGroundWorker to ensure we are thread safe when accessing the form control later on
            this.processPackage.ReportProgress(0, e);
        }

        protected virtual void OnRaiseEditExpressionSelected(EditExpressionSelectedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<EditExpressionSelectedEventArgs> handler = EditExpressionSelected;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region BackgroundWorker Events

        private void processPackage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //expressionListWindow.StopProgressBar();
            stopwatch.Stop();
            this.Text += (" " + stopwatch.ElapsedMilliseconds.ToString());
        }

        private void processPackage_DoWork(object sender, DoWorkEventArgs e)
        {
            stopwatch.Start();
            finder.FindReferences(this.package, this.variable);
        }

        private void processPackage_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            VariableFoundEventArgs variableFound = (VariableFoundEventArgs)e.UserState;
            AddValue(variableFound.Type, variableFound.ContainerID, variableFound.ObjectID, variableFound.ObjectType, variableFound.ObjectPath, variableFound.ObjectName, variableFound.PropertyName, variableFound.Value, variableFound.Icon, variableFound.IsExpression);
        }
        #endregion

        /// <summary>
        /// Handles the CellPainting event of the expressionGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellPaintingEventArgs"/> instance containing the event data.</param>
        private void expressionGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Skip headers
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }

            // Check for an icon associated with cell, and if 
            // found draw the image as well as the text.
            Icon icon = this.expressionGrid[e.ColumnIndex, e.RowIndex].Tag as Icon;
            if (icon != null)
            {
                // Check if cell is selected, so we can paint the backrgound and text correctly
                bool paintSelected = this.expressionGrid.SelectedRows.Contains(this.expressionGrid.Rows[e.RowIndex]);
                e.PaintBackground(e.CellBounds, paintSelected);

                int padding = e.CellStyle.Padding.Left;
                if (padding < 2)
                {
                    padding = 2;
                }

                e.Graphics.DrawIcon(icon, e.CellBounds.X + padding, e.CellBounds.Y + GetCenterOffset(e.CellBounds.Height, icon.Height));

                if (e.Value != null)
                {
                    // Get text color, checking for selected state
                    Color textColor = e.CellStyle.ForeColor;
                    if (paintSelected)
                    {
                        textColor = e.CellStyle.SelectionForeColor;
                    }

                    using (Brush brush = new SolidBrush(textColor))
                    {
                        // HACK: We assume the cell style alignment is always Middle Left
                        StringFormat format = new StringFormat();
                        format.LineAlignment = StringAlignment.Center;
                        e.Graphics.DrawString(e.Value.ToString(), e.CellStyle.Font, brush, e.CellBounds.X + padding + icon.Width, e.CellBounds.Y + (e.CellBounds.Height / 2), format);
                    }
                }

                e.Handled = true;
            }
        }

        private void expressionGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (expressionGrid.Columns[e.ColumnIndex] == this.EditorColumn)
                {

                    DataGridViewRow row = expressionGrid.Rows[e.RowIndex];
                    DataGridViewButtonDisableCell cell = (DataGridViewButtonDisableCell)row.Cells[this.EditorColumn.Index];

                    if (cell.Enabled)
                    {
                        OnRaiseEditExpressionSelected(
                            new EditExpressionSelectedEventArgs(row.Tag as Type,
                                row.Cells[expressionGrid.Columns["ObjectPath"].Index].Value.ToString(),
                                row.Cells[expressionGrid.Columns["Expression"].Index].Value as string,
                                row.Cells[expressionGrid.Columns["Property"].Index].Value.ToString(),
                                row.Cells[expressionGrid.Columns["ContainerID"].Index].Value.ToString(),
                                row.Cells[expressionGrid.Columns["ObjectID"].Index].Value.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while editing the expression: " + ex.Message);
            }
        }

        private static int GetCenterOffset(int bound, int dimension)
        {
            if (bound > dimension)
            {
                return (bound - dimension) / 2;
            }
            else
            {
                return 0;
            }
        }
    }
}
