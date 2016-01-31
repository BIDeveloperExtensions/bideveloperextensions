using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace BIDSHelper.SSIS.Biml
{
    public partial class MultipleSelectionConfirmationDialog : Form
    {
        private List<string> _itemsSource;

        public IEnumerable<string> SelectedFilePaths
        {
            get
            {
                return this.selectionList.SelectedItems;
            }
        }

        public MultipleSelectionConfirmationDialog(List<string> itemsSource, string projectDirectory, int safeCount)
        {
            _itemsSource = itemsSource;
            InitializeComponent();
            confirmationTextLabel.Text = string.Format(
                CultureInfo.CurrentCulture,
                "In addition to {0} new items, the template generated the following items that conflict with existing items.  Which of these items would you like to overwrite?",
                safeCount);

            foreach (var item in itemsSource)
            {
                selectionList.AddItem(Path.Combine(projectDirectory, Path.GetFileName(item)), true);
            }
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(BIDSHelper.Resources.Common.BimlOverwriteConfirmationHelpUrl);
        }
    }
}
