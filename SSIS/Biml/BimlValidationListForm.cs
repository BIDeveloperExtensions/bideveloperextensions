using System;
using System.Windows.Forms;
using Varigence.Flow.FlowFramework.Validation;

namespace BIDSHelper.SSIS.Biml
{
    public partial class BimlValidationListForm : Form
    {
        public BimlValidationListForm(ValidationReporter validationReporter)
        {
            InitializeComponent();
            foreach (var validationItem in validationReporter.Errors)
            {
                listViewValidationItems.Items.Add(new ListViewItem(new string[] { validationItem.Severity.ToString(), validationItem.Message, validationItem.Recommendation, validationItem.Line.ToString(), validationItem.Offset.ToString() }));
            }
            listViewValidationItems.Columns[1].Width = -1;
            listViewValidationItems.Columns[2].Width = -1;
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(BIDSHelper.Resources.Common.BimlValidationHelpUrl);
        }

        private void BimlValidationListForm_Load(object sender, EventArgs e)
        {
            if (listViewValidationItems.Columns[1].Width > 225)
            {
                listViewValidationItems.Columns[1].Width = 225;
            }

            if (listViewValidationItems.Columns[2].Width > 225)
            {
                listViewValidationItems.Columns[2].Width = 225;
            }
        }
    }
}
