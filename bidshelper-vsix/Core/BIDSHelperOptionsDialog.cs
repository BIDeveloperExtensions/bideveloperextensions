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
    [Guid(BIDSHelperOptionsDialog.OptionsGuidString)]
    public class BIDSHelperOptionsDialog : DialogPage
    {
        public const string OptionsGuidString = "9EBCE16B-26C2-4A22-A409-9752750A16AE";
        /*
        private int optionInt = 256;

        [Category("BIDS Helper")]
        [DisplayName("My Integer option")]
        [Description("My integer option")]
        public int OptionInteger
        {
            get { return optionInt; }
            set { optionInt = value; }
        }

        private string optionStr = "";
        [Category("BIDS Helper")]
        [DisplayName("My String option")]
        [Description("My String option desc")]
        public string OptionString
        {
            get { return optionStr; }
            set { optionStr = value; }
        }
        */
        protected override IWin32Window Window
        {
            get
            {
                BIDSHelperOptionsPage page = new BIDSHelperOptionsPage();
                //page.optionsPage = this;
                page.Initialize();
                return page;
            }
        }
    }
}
