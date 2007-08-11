using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace BIDSHelper.Core
{
    public interface IWindowActivatedPlugin
    {
        void HookWindowActivation();
        void UnHookWindowActivation();
        void OnWindowActivated(Window GotFocus, Window LostFocus);
    }
}
