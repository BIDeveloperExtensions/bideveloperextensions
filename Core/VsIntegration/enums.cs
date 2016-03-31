using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDSHelper.Core.VsIntegration
{
    //FROM: https://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.interop.ivsstatusbar.animation.aspx
    public enum VsStatusBarAnimations
    {
        SBAI_General = 0,   //Standard animation icon.
        SBAI_Print = 1,     //Animation when printing.
        SBAI_Save = 2,      //Animation when saving files.
        SBAI_Deploy = 3,    //Animation when deploying the solution.
        SBAI_Synch = 4,     //Animation when synchronizing files over the network.
        SBAI_Build = 5,     //Animation when building the solution.
        SBAI_Find = 6       //Animation when searching.
    }
}
