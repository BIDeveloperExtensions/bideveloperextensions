using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace BIDSHelper.SSIS.Biml
{
    public partial class MultipleSelectionConfirmationDialog : Form
    {
        private List<string> itemsSource;

        public IEnumerable<string> SelectedFilePaths
        {
            get; private set;
        }

        public MultipleSelectionConfirmationDialog(List<string> itemsSource, List<bool> highlighted, string projectDirectory, int safeCount)
        {
            this.itemsSource = itemsSource;
            InitializeComponent();
            confirmationTextLabel.Text = string.Format(
                CultureInfo.CurrentCulture,
                "In addition to {0} new items, the template generated the following items that conflict with existing items.  Which of these items would you like to overwrite?",
                safeCount);

            int index = 0;
            foreach (var item in itemsSource)
            {
                selectionList.AddItem(Path.Combine(projectDirectory, Path.GetFileName(item)), true, highlighted[index]);
                index++;
            }

            panelWarning.Visible = (highlighted.Contains(true));
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(BIDSHelper.Resources.Common.BimlOverwriteConfirmationHelpUrl);
        }

        private void commitButton_Click(object sender, EventArgs e)
        {
            // Transalate profile file paths displayed in the selection list, back to temp file paths that we were supplied with
            List<string> paths = new List<string>();
            foreach (int selectedIndex in selectionList.CheckedIndices)
            {
                paths.Add(itemsSource[selectedIndex]);
            }

            this.SelectedFilePaths = paths;
        }
    }
}
