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

        public FindReferences()
        {
            InitializeComponent();

            // Hide internal columns - Do it here as easy to make visible when debugging
            this.expressionGrid.Columns[0].Visible = false;
            this.expressionGrid.Columns[1].Visible = false;
            this.expressionGrid.Columns[4].Visible = false;

            processPackage = new BackgroundWorker();
            processPackage.WorkerReportsProgress = true;
            processPackage.WorkerSupportsCancellation = true;
            processPackage.DoWork += new DoWorkEventHandler(processPackage_DoWork);
            processPackage.ProgressChanged += new ProgressChangedEventHandler(processPackage_ProgressChanged);
            processPackage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(processPackage_RunWorkerCompleted);
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

        public void AddValue(Type type, string containerID, string objectID, string objectType, string objectPath, string objectName, string propertyName, string value, Icon icon, bool isExpression)
        {
            string[] newRow = { containerID, objectID, objectType, objectPath, objectName, propertyName, value };
            int index = expressionGrid.Rows.Add(newRow);
            DataGridViewRow row = expressionGrid.Rows[index];
            row.Tag = type;
            row.Cells["ObjectType"].Tag = icon;

            if (!isExpression)
            {
                row.Cells["EditorColumn"].DetachEditingControl();
            }
        }

        private void VariableFound(object sender, VariableFoundEventArgs e)
        {
            // Report variable found via BackGroundWorker to ensure we are thread safe when accessing the form control later on
            this.processPackage.ReportProgress(0, e);
        }

        #region BackgroundWorker Events

        private void processPackage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //expressionListWindow.StopProgressBar();
            stopwatch.Stop();
            this.Text = stopwatch.ElapsedMilliseconds.ToString();
        }

        private void processPackage_DoWork(object sender, DoWorkEventArgs e)
        {
            stopwatch.Start();
            finder.FindReferences(this.package, this.variable);
        }

        private void processPackage_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            VariableFoundEventArgs info = (VariableFoundEventArgs)e.UserState;
            AddValue(info.Type, info.ContainerID, info.ObjectID, info.ObjectType, info.ObjectPath, info.ObjectName, info.PropertyName, info.Value, info.Icon, info.IsExpression);
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
