using BIDSHelper.Core;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BIDSHelper.Core
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    [Guid("5F8DEE5F-2790-4EE2-B709-A3F2D11E2042")]
    public class BIDSHelperPreferencesDialogPage : DialogPage
    {
        private BIDSHelperPreferencesPage page = null;

        protected override IWin32Window Window
        {
            get
            {
                try
                {
                    page = new BIDSHelperPreferencesPage();
                    page.OptionsPage = this;
                    page.Initialize();
                    return page;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                    return null;
                }
            }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            page.Apply();    
        }
    }
}
