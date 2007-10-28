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
        public BIDSHelperWindowActivatedPluginBase(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
            //appObj = appObject;
            //addIn = addinInstance;
        }

        public BIDSHelperWindowActivatedPluginBase()
        {

        }
        #endregion

        public virtual bool ShouldHookWindowCreated
        {
            get { return false; }
        }
        public virtual bool ShouldHookWindowActivated
        {
            get { return true; }
        }

        public void HookWindowActivation()
        {
            windowEvents = base.GetWindowEvents();
            if (ShouldHookWindowActivated)
                windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            if (ShouldHookWindowCreated)
                windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
        }

        public void UnHookWindowActivation()
        {
            if (windowEvents != null)
            {
                if (ShouldHookWindowActivated)
                    windowEvents.WindowActivated -= windowEvents_WindowActivated;
                if (ShouldHookWindowCreated)
                    windowEvents.WindowCreated -= windowEvents_WindowCreated;
            }
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

        public override void OnEnable()
        {
            base.OnEnable();
            this.HookWindowActivation();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            this.UnHookWindowActivation();

        }
    }
}
