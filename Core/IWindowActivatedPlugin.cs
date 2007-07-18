using System;
using System.Collections.Generic;
using System.Text;

namespace BIDSHelper.Core
{
    public interface IWindowActivatedPlugin
    {
        void HookWindowActivation();
        void UnHookWindowActivation();
        void WindowActivated(Window GotFocus, Window LostFocus);
    }
}
