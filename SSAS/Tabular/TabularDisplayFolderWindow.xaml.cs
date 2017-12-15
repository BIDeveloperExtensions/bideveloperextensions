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

namespace BIDSHelper.SSAS
{
    public partial class TabularDisplayFolderWindow : Window
    {
        public TabularDisplayFolderWindow()
        {
            //InitializeComponent();
            //overriding the default InitializeComponent() method as it gets hardcoded to BidsHelper2017
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/" + this.GetType().Assembly.GetName().Name + ";component/ssas/tabular/tabulardisplayfolderwindow.xaml", System.UriKind.Relative);

#line 1 "..\..\..\..\SSAS\Tabular\TabularDisplayFolderWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);

#line default
#line hidden


        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
