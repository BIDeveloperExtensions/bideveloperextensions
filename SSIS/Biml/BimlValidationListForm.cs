using System;
using System.Windows.Forms;
using Varigence.Flow.FlowFramework.Validation;
using System.Text;

namespace BIDSHelper.SSIS.Biml
{
    public partial class BimlValidationListForm : Form
    {
        public BimlValidationListForm(ValidationReporter validationReporter, bool ShowWarnings)
        {
            InitializeComponent();
            StringBuilder sb = new StringBuilder();
            foreach (var item in validationReporter.ValidationItems)
            {
                if (item.Severity == Varigence.Flow.FlowFramework.Severity.Error ||
                    (item.Severity == Varigence.Flow.FlowFramework.Severity.Warning && ShowWarnings)
                    )
                sb.AppendLine(item.ToString(true));
            }
            textBox1.Text = sb.ToString();
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(BIDSHelper.Resources.Common.BimlValidationHelpUrl);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {

        }
    }
}
