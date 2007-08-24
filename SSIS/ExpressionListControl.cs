using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper
{
    public partial class ExpressionListControl : UserControl
    {
        public ExpressionListControl()
        {
            InitializeComponent();
            toolStripProgressBar1.Enabled = false;
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
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

        void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridView1.Columns[e.ColumnIndex].Name == "EditorBtn")
                {
                    OnRaiseEditExpressionSelected(new EditExpressionSelectedEventArgs(dataGridView1.Rows[e.RowIndex].Cells[dataGridView1.Columns["ObjectPath"].Index].Value.ToString()));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Enabled = true;
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            OnRaiseRefreshExpression();
        }

        public void StopProgressBar()
        {
            toolStripProgressBar1.Enabled = false;
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
        }

    }

    public class EditExpressionSelectedEventArgs : EventArgs
    {
        public EditExpressionSelectedEventArgs(string objectPath)
        {
            this.objectPath = objectPath;
        }
        private string objectPath;
        public string ObjectPath
        {
            get { return objectPath; }
        }
    }

}
