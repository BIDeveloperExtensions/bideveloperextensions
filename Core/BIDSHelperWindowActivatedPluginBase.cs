using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace BIDSHelper.Core
{
    abstract class BIDSHelperWindowActivatedPluginBase
        : BIDSHelperPluginBase
        , IWindowActivatedPlugin
    {
        private WindowEvents windowEvents;

        public void HookWindowActivation()
        {
            windowEvents = base.GetWindowEvents();
            windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
        }

        public void UnHookWindowActivation()
        {
            windowEvents.WindowActivated -= windowEvents_WindowActivated;
            windowEvents.WindowCreated += windowEvents_WindowCreated;
        }
        public void OnWindowActivated(Window GotFocus, Window LostFocus)
        { }

        private void windowEvents_WindowCreated(Window Window)
        {
            OnWindowActivated(Window, null);
        }

        private void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            OnWindowActivated(GotFocus, LostFocus);
        }

    }
}
