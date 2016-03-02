using System;
using System.Windows.Forms;

namespace BIDSHelper
{
    ///<summary>
    /// Form to capture Batch Property Update settings from user
    ///</summary>
    public partial class BatchPropertyUpdateForm : Form
    {
        private string _PropertyPath;
        /// <summary>
        /// Property Path to look for
        /// </summary>
        public string PropertyPath
        {
            get { return _PropertyPath; }
            set { _PropertyPath = value; }
        }

        private string _NewValue;
        /// <summary>
        /// New value to apply to property
        /// </summary>
        public string NewValue
        {
            get { return _NewValue; }
            set { _NewValue = value; }
        }

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
