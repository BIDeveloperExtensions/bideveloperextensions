using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
    }
}
