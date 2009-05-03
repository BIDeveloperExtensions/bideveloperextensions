using System;
using System.Windows.Forms;

namespace BIDSHelper
{
    ///<summary>
    /// Form to capture Batch Property Update settings from user
    ///</summary>
    public partial class BatchPropertyUpdateForm : Form
    {
        /// <summary>
        /// Property Path to look for
        /// </summary>
        public string PropertyPath { get; set; }

        /// <summary>
        /// New value to apply to property
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// Constructor for form to capture PropertyPath and NewValue from user
        /// </summary>
        public BatchPropertyUpdateForm()
        {
            InitializeComponent();
            txtPropertyPath.Text = PropertyPath;
            txtNewValue.Text = NewValue;
        }

        private void txtPropertyPath_TextChanged(object sender, EventArgs e)
        {
            PropertyPath = txtPropertyPath.Text;
        }

        private void txtNewValue_TextChanged(object sender, EventArgs e)
        {
            NewValue = txtNewValue.Text;
        }
    }
}
