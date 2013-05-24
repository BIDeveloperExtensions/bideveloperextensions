using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using Microsoft.DataWarehouse;
using System.Globalization;

namespace BIDSHelper.SSAS
{
    public partial class TabularTranslationsEditorWindow : Window
    {
        private List<int> _languages;
        private int _defaultLanguage;
        private List<DataGridTextColumn> _languageColumns;

        public TabularTranslationsEditorWindow(int defaultLanguage, List<int> languages)
        {
            _iLanguageColumnIndexForContextMenu = -1;
            _defaultLanguage = defaultLanguage;
            _languages = languages;
            InitializeComponent();
            RefreshFields();
        }

        private void RefreshFields()
        {
            this.dataGrid1.Columns.Clear();

            Color color = Colors.DimGray;

            System.Windows.Data.Binding bindingTable = new System.Windows.Data.Binding("Table");
            bindingTable.Mode = BindingMode.OneWay;
            this.dataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "Table",
                Binding = bindingTable,
                CellStyle = (Style)dataGrid1.Resources["CellStyleGray"]
            });

            System.Windows.Data.Binding bindingObjectType = new System.Windows.Data.Binding("ObjectType");
            bindingObjectType.Mode = BindingMode.OneWay;
            this.dataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "Object Type",
                Binding = bindingObjectType,
                CellStyle = (Style)dataGrid1.Resources["CellStyleGray"]
            });

            System.Windows.Data.Binding bindingProperty = new System.Windows.Data.Binding("PropertyForDisplay");
            bindingProperty.Mode = BindingMode.OneWay;
            this.dataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "Property",
                Binding = bindingProperty,
                CellStyle = (Style)dataGrid1.Resources["CellStyleGray"]
            });

            int iIndexDefaultLang = CultureInfoCollection.Instance.IndexOfLCID(_defaultLanguage);
            if (iIndexDefaultLang == -1) throw new Exception("Unexpected LCID " + _defaultLanguage);
            string sDefaultLanguageName = CultureInfoCollection.Instance[iIndexDefaultLang].DisplayName;
            System.Windows.Data.Binding bindingDefaultLanguage = new System.Windows.Data.Binding("DefaultLanguage");
            bindingDefaultLanguage.Mode = BindingMode.OneWay;
            Style styleHeader = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            styleHeader.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "LocaleIdentifier=" + _defaultLanguage));
            this.dataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = sDefaultLanguageName,
                Binding = bindingDefaultLanguage,
                CellStyle = (Style)dataGrid1.Resources["CellStyleGray"],
                HeaderStyle = styleHeader
            });

            _languageColumns = new List<DataGridTextColumn>();
            foreach (int iLang in _languages)
            {
                int iIndex = CultureInfoCollection.Instance.IndexOfLCID(iLang);
                if (iIndex == -1) throw new Exception("Unexpected LCID " + iLang);
                string sLanguageName = CultureInfoCollection.Instance[iIndex].DisplayName;

                styleHeader = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
                styleHeader.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "LocaleIdentifier=" + iLang)); 

                DataGridTextColumn col = new DataGridTextColumn
                {
                    Header = sLanguageName,
                    Binding = new System.Windows.Data.Binding(string.Format("Languages[{0}]", iLang)),
                    CellStyle = (Style)dataGrid1.Resources["CellStyleTransparent"],
                    HeaderStyle = styleHeader
                };
                _languageColumns.Add(col);
                this.dataGrid1.Columns.Add(col);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            //null out deleted translations now they they have clicked OK
            foreach (SSAS.TabularTranslatedItem row in dataGrid1.ItemsSource)
            {
                for (int i = 0; i < row.Languages.Keys.Count; i++)
                {
                    int iLang = row.Languages.Keys.ElementAt(i);
                    if (!_languages.Contains(iLang))
                    {
                        row.Languages[iLang] = null; //null out the translation so it will mark it as dirty and so we won't save this translation they've deleted
                    }
                }
            }

            this.Close();
        }

        private void btnAddLanguage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hashtable hashtable = new Hashtable();
                SelectionListBoxDialog form = new SelectionListBoxDialog();
                form.Text = "Select Language";
                form.TopLabel.Text = "Select a language for the new translation:";
                form.Icon = BIDSHelper.Resources.Common.BIDSHelper;
                form.HelpID = "sql11.asvs.cubeeditor.languageselection.f1";
                foreach (CultureInfo info in CultureInfoCollection.Instance)
                {
                    if (!info.IsNeutralCulture)
                    {
                        int lCID = info.LCID;
                        if (!this._languages.Contains(lCID) && lCID != _defaultLanguage && !hashtable.ContainsKey(info.DisplayName))
                        {
                            form.ListBox.Items.Add(info.DisplayName);
                            hashtable.Add(info.DisplayName, info.LCID);
                        }
                    }
                }
                form.ListBox.Sorted = true;
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK && form.ListBox.SelectedItem != null)
                {
                    int iLang = (int)hashtable[form.ListBox.SelectedItem.ToString()];
                    this._languages.Add(iLang);
                    this._languages.Sort(); //make sure the languages will appear in the same order that they do the next time you open this dialog
                    this.RefreshFields();
                }
                this.dataGrid1.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_iLanguageColumnIndexForContextMenu < 0) return;

                _languages.RemoveAt(_iLanguageColumnIndexForContextMenu);
                _languageColumns.RemoveAt(_iLanguageColumnIndexForContextMenu);
                RefreshFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        private int _iLanguageColumnIndexForContextMenu;
        private void dataGrid1_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                object oDataContext = null;
                if (e.OriginalSource is FrameworkElement)
                {
                    oDataContext = ((FrameworkElement)e.OriginalSource).DataContext;
                }
                else if (e.OriginalSource is FrameworkContentElement)
                {
                    oDataContext = ((FrameworkContentElement)e.OriginalSource).DataContext;
                }

                if (oDataContext is string)
                {
                    for (int iIndex = 0; iIndex < _languageColumns.Count; iIndex++)
                    {
                        DataGridTextColumn col = _languageColumns[iIndex];
                        if ((string)col.Header == (string)oDataContext)
                        {
                            e.Handled = false; //show the context menu
                            _iLanguageColumnIndexForContextMenu = iIndex;
                            return;
                        }
                    }
                }

                e.Handled = true; //don't show the context menu
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
                e.Handled = true; //don't show the context menu
            }
        }
    }
}
