namespace BIDSHelper.SSIS
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class ExpressionListControl : UserControl
    {
        //TODO: Add "Live Update" - automatically filter diplay based on currently selected object.
        //TODO: Expose methods on ExpressionList Control for adding and removing rows, rather than direct access to the grid.
        public ExpressionListControl()
        {
            InitializeComponent();

            // Hide internal columns - Do it here as easy to make visible when debugging
            this.expressionGrid.Columns[0].Visible = false;
            this.expressionGrid.Columns[1].Visible = false;
            this.expressionGrid.Columns[4].Visible = false;

            StopProgressBar();
            btnRefresh.Image = (Image)BIDSHelper.Resources.Common.RefreshExpressions.ToBitmap();

            expressionGrid.CellContentClick += new DataGridViewCellEventHandler(expressionGrid_CellContentClick);
        }

        public event EventHandler RefreshExpressions;
        public event EventHandler<EditExpressionSelectedEventArgs> EditExpressionSelected;

        protected virtual void OnRaiseRefreshExpression()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler handler = RefreshExpressions;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                EventArgs e = new EventArgs();

                handler(this, e);
            }
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

        public void AddExpression(Type type, string containerID, string objectID, string objectType, string objectPath, string objectName, string propertyName, string expression, Icon icon)
        {
            string[] newRow = { containerID, objectID, objectType, objectPath, objectName, propertyName, expression };
            int index = expressionGrid.Rows.Add(newRow);
            expressionGrid.Rows[index].Tag = type;
            expressionGrid.Rows[index].Cells["ObjectType"].Tag = icon;
        }

        public void ClearResults()
        {
            expressionGrid.Rows.Clear();
        }

        void expressionGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (expressionGrid.Columns[e.ColumnIndex] == this.EditorColumn)
                {
                    DataGridViewRow row = expressionGrid.Rows[e.RowIndex];

                    OnRaiseEditExpressionSelected(
                        new EditExpressionSelectedEventArgs(row.Tag as Type,
                            row.Cells[expressionGrid.Columns["ObjectPath"].Index].Value.ToString(),
                            row.Cells[expressionGrid.Columns["Expression"].Index].Value as string,
                            row.Cells[expressionGrid.Columns["Property"].Index].Value.ToString(),
                            row.Cells[expressionGrid.Columns["ContainerID"].Index].Value.ToString(),
                            row.Cells[expressionGrid.Columns["ObjectID"].Index].Value.ToString(),
                            row.Cells[expressionGrid.Columns["ObjectType"].Index].Value.ToString()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while editing the expression: " + ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            OnRaiseRefreshExpression();
        }

        public void StartProgressBar()
        {
            toolStripProgressBar.Enabled = true;
            toolStripProgressBar.Visible = true;
            toolStripProgressBar.Style = ProgressBarStyle.Marquee;
        }

        public void StopProgressBar()
        {
            toolStripProgressBar.Enabled = false;
            toolStripProgressBar.Visible = false;
            toolStripProgressBar.Style = ProgressBarStyle.Blocks;
        }

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
    
    public class EditExpressionSelectedEventArgs : EventArgs
    {
        public EditExpressionSelectedEventArgs(Type type, string objectPath, string expression, string property, string containerID, string objectID, string objectType)
        {
            this.type = type;
            this.containerID = containerID;
            this.path = objectPath;
            this.expression = expression;
            this.property = property;
            this.objectID = objectID;
            this.objectType = objectType;
        }

        private Type type;
        private string expression;
        private string property;
        private string path;
        private string containerID; 
        private string objectID;
        private string objectType;

        public Type Type
        {
            get { return this.type; }
        }

        public string TaskPath
        {
            get { return this.path; }
        }
        public string Expression
        {
            get { return this.expression; }
        }
        public string Property
        {
            get { return this.property; }
        }

        public string ContainerID
        {
            get { return this.containerID; }
        }
        public string ObjectID
        {
            get { return this.objectID; }
        }
        public string ObjectType
        {
            get { return this.objectType; }
        }
    }
}
