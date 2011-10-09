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

        public List<string> SelectedFilePaths { get; set; }

        private bool _isInItemCheck;

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
                selectionCheckedListBox.Items.Add(Path.Combine(projectDirectory, Path.GetFileName(item)), CheckState.Checked);
            }
        }

        private void selectAllCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInItemCheck)
            {
                CheckState checkState = selectAllCheckBox.CheckState == CheckState.Indeterminate ? CheckState.Checked : selectAllCheckBox.CheckState;
                for (int i = 0; i < selectionCheckedListBox.Items.Count; i++)
                {
                    selectionCheckedListBox.SetItemCheckState(i, checkState);
                }
            }
        }

        private void selectionCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _isInItemCheck = true;
            selectAllCheckBox.CheckState = GetSelectAllCheckState(e.NewValue == CheckState.Checked ? 1 : -1);
            _isInItemCheck = false;
        }

        private CheckState GetSelectAllCheckState(int adjustment)
        {
            int count = selectionCheckedListBox.CheckedIndices.Count + adjustment;
            if (count == 0)
            {
                return CheckState.Unchecked;
            }

            if (count == selectionCheckedListBox.Items.Count)
            {
                return CheckState.Checked;
            }

            return CheckState.Indeterminate;
        }

        private void commitButton_Click(object sender, EventArgs e)
        {
            SelectedFilePaths = new List<string>();
            foreach (int selectedIndex in selectionCheckedListBox.CheckedIndices)
            {
                SelectedFilePaths.Add(_itemsSource[selectedIndex]);
            }
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(BIDSHelper.Resources.Common.BimlOverwriteConfirmationHelpUrl);
        }
    }
}
