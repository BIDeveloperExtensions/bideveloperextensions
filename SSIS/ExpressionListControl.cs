using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper
{
    public partial class ExpressionListControl : UserControl
    {
        //TODO: Add "Live Update" - automatically filter diplay based on currently selected object.
        //TODO: Expose methods on ExpressionList Control for adding and removing rows, rather than direct access to the grid.
        public ExpressionListControl()
        {
            InitializeComponent();
            StopProgressBar();
            btnRefresh.Image = (Image) BIDSHelper.Properties.Resources.RefreshExpressions.ToBitmap();

            dataGridView1.CellContentClick += new DataGridViewCellEventHandler(dataGridView1_CellContentClick);


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

        public void AddExpression(string objectID, string objectType, string objectPath, string objectName, string propertyName, string expression)
        {
            int lastPart = objectType.LastIndexOf(".") + 1;

            string objectTypeAdjusted = objectType.Substring(lastPart, objectType.Length - lastPart) //TODO: Adjust Length
                + " [" + objectType.Substring(0, objectType.LastIndexOf(".")) + "]";
            string[] newRow = { objectID, objectTypeAdjusted, objectPath, objectName, propertyName, expression };

            dataGridView1.Rows.Add(newRow);
        }

        public void ClearResults()
        {
            dataGridView1.Rows.Clear();
        }

        void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridView1.Columns[e.ColumnIndex].Name == "EditorBtn")
                {
                    OnRaiseEditExpressionSelected(
                        new EditExpressionSelectedEventArgs(dataGridView1.Rows[e.RowIndex].Cells[dataGridView1.Columns["ObjectPath"].Index].Value.ToString(),
                            dataGridView1.Rows[e.RowIndex].Cells[dataGridView1.Columns["Expression"].Index].Value.ToString(),
                            dataGridView1.Rows[e.RowIndex].Cells[dataGridView1.Columns["Property"].Index].Value.ToString(), 
                            dataGridView1.Rows[e.RowIndex].Cells[dataGridView1.Columns["ObjectID"].Index].Value.ToString(),
                            dataGridView1.Rows[e.RowIndex].Cells[dataGridView1.Columns["ObjectType"].Index].Value.ToString()));
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
            toolStripProgressBar1.Enabled = true;
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
        }

        public void StopProgressBar()
        {
            toolStripProgressBar1.Enabled = false;
            toolStripProgressBar1.Visible = false;
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
        }

    }
    
    public class EditExpressionSelectedEventArgs : EventArgs
    {
        public EditExpressionSelectedEventArgs(string objectPath, string expression, string property, string objectID, string objectType)
        {
            this.path = objectPath;
            this.expression = expression;
            this.property = property;
            this.objectID = objectID;
            this.objectType = objectType;
        }
        private string expression;
        private string property;
        private string path;
        private string objectID;
        private string objectType;

        public string TaskPath
        {
            get { return path; }
        }
        public string Expression
        {
            get { return expression; }
        }
        public string Property
        {
            get { return property; }
        }
        public string ObjectID
        {
            get { return objectID; }
        }
        public string ObjectType
        {
            get { return objectType; }
        }
    }

}
