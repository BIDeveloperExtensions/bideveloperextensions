using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BIDSHelper.Core
{
    

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    [Guid(BIDSHelperOptionsFeatures.OptionsGuidString)]
    public class BIDSHelperOptionsFeatures: DialogPage
    {
        public const string OptionsGuidString = "9EBCE16B-26C2-4A22-A409-9752750A16AE";
        private BIDSHelperOptionsPage page;
        protected override IWin32Window Window
        {
            get
            {
                page = new BIDSHelperOptionsPage();
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
