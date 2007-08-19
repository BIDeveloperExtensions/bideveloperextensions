using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using EnvDTE80;

namespace BIDSHelper
{
    public abstract class BIDSHelperWindowActivatedPluginBase
        : BIDSHelperPluginBase
        , IWindowActivatedPlugin
    {
        private WindowEvents windowEvents;

                #region "Constructors"
        public BIDSHelperWindowActivatedPluginBase(DTE2 appObject, AddIn addinInstance)
            : base(appObject, addinInstance)
        {
            //appObj = appObject;
            //addIn = addinInstance;
        }

        public BIDSHelperWindowActivatedPluginBase()
        {

        }
        #endregion

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
        public virtual void OnWindowActivated(Window GotFocus, Window LostFocus)
        { }

        public void OnActiveViewChanged()
        {
            if (this.IdeMode == enumIDEMode.Design)
            {
                OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
            }
        }

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
