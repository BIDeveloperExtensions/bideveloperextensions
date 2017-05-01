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
    [Guid(BIDSHelperOptionsVersion.VersionGuidString)]
    public class BIDSHelperOptionsVersion: DialogPage
    {
        public const string VersionGuidString = "9128d0ed-abbe-3002-9aee-4c06babd03ae";

        protected override IWin32Window Window
        {
            get
            {
                BIDSHelperOptionsVersionCheckPage page = new BIDSHelperOptionsVersionCheckPage();
                //page.optionsPage = this;
                page.Initialize();
                return page;
            }
        }
    }
}
