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
        }
        public event EventHandler RefreshExpressions;

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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Enabled=true;
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            OnRaiseRefreshExpression();
        }

        public void StopProgressBar()
        {
            toolStripProgressBar1.Enabled = false;
            toolStripProgressBar1.Style= ProgressBarStyle.Blocks;
        }

    }
}
