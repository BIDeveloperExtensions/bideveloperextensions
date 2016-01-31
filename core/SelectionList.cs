using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BIDSHelper.Core
{
    /// <summary>
    /// Alternatve to the CheckedListBox control. Uses a checkbox to indicate selection.
    /// Click anywhere on the row, or space bar to toggle the checkbox.
    /// </summary>
    public partial class SelectionList : UserControl
    {
        private int totalItems;
        private int checkedItems;
        private bool checkChangedEnabled = true;

        public event EventHandler<SelectionListSelectionChangedEventArgs> SelectionChanged;

        public SelectionList()
        {
            InitializeComponent();
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = sender as DataGridView;

            // Get current cell state
            bool currentState = Convert.ToBoolean(grid.Rows[e.RowIndex].Cells[0].Value);

            foreach (DataGridViewRow row in grid.SelectedRows)
            {
                // Invert current state to check/uncheck all selected rows
                row.Cells[0].Value = !currentState;
            }

            UpateSelectAll();
        }

        private void dataGridView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != ' ')
            {
                return;
            }

            DataGridView grid = sender as DataGridView;
            bool currentState = Convert.ToBoolean(grid.CurrentRow.Cells[0].Value);

            foreach (DataGridViewRow row in grid.SelectedRows)
            {
                row.Cells[0].Value = !currentState;
            }

            UpateSelectAll();
        }

        private void checkBoxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkChangedEnabled)
                return;

            bool checkAll = checkBoxSelectAll.Checked;
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                row.Cells[0].Value = checkAll;
            }

            checkedItems = (checkAll ? totalItems : 0);

            OnRaiseSelectionChanged(new SelectionListSelectionChangedEventArgs(this.checkedItems, this.totalItems));
        }

        private void UpateSelectAll()
        {
            checkChangedEnabled = false;

            int counter = 0;
            // Enumerate rows to get reliable current state. Too many issues when trying to keep track via counters.
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value))
                {
                    counter++;
                }
            }

            checkedItems = counter;

            if (totalItems == checkedItems)
            {
                checkBoxSelectAll.CheckState = CheckState.Checked;
            }
            else if (checkedItems == 0)
            {
                checkBoxSelectAll.CheckState = CheckState.Unchecked;
            }
            else
            {
                checkBoxSelectAll.CheckState = CheckState.Indeterminate;
            }

            checkChangedEnabled = true;

            OnRaiseSelectionChanged(new SelectionListSelectionChangedEventArgs(this.checkedItems, this.totalItems));
        }

        public void AddItem(string item)
        {
            AddItem(item, false);
        }

        public void AddItem(string item, bool selected)
        {
            dataGridView.Rows.Add(selected, item);
            totalItems++;
            checkedItems += (selected ? 1 : 0);
        }

        public void AddRange(string[] items)
        {
            foreach (string item in items)
            {
                AddItem(item, false);
            }
        }

        public void ClearItems()
        {
            this.dataGridView.Rows.Clear();
        }

        public IEnumerable<string> SelectedItems
        {
            get
            {
                List<string> selectedItems = new List<string>();
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                {
                    selectedItems.Add(row.Cells[1].Value.ToString());
                }

                return selectedItems;
            }
        }

        protected virtual void OnRaiseSelectionChanged(SelectionListSelectionChangedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<SelectionListSelectionChangedEventArgs> handler = SelectionChanged;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    public class SelectionListSelectionChangedEventArgs : EventArgs
    {
        public SelectionListSelectionChangedEventArgs(int selectedItems, int totalItems)
        {
            this.SelectedItems = selectedItems;
            this.TotalItems = totalItems;
        }

        public int SelectedItems { get; private set; }

        public int TotalItems { get; private set; }
        
    }
}
