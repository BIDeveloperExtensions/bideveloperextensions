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
    //[Guid(BIDSHelperOptionsVersion.OptionsGuidString)]
    public class BIDSHelperOptionsPreferences : DialogPage
    {
        //    public const string OptionsGuidString = "9EBCE16B-26C2-4A22-A409-9752750A16AE";
        BIDSHelperPreferencesPage page = null;

        protected override IWin32Window Window
        {
            get
            {
                page = new BIDSHelperPreferencesPage();
                //page.optionsPage = this;
                page.Initialize();
                return page;
            }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            page.Apply();    
        }
    }
}
