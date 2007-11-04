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
        public virtual bool ShouldHookWindowClosing
        {
            get { return false; }
        }

        public void HookWindowActivation()
        {
            windowEvents = base.GetWindowEvents();
            if (ShouldHookWindowActivated)
                windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            if (ShouldHookWindowCreated)
                windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
            if (ShouldHookWindowClosing)
                windowEvents.WindowClosing += new _dispWindowEvents_WindowClosingEventHandler(windowEvents_WindowClosing);
        }

        public void UnHookWindowActivation()
        {
            if (windowEvents != null)
            {
                if (ShouldHookWindowActivated)
                    windowEvents.WindowActivated -= windowEvents_WindowActivated;
                if (ShouldHookWindowCreated)
                    windowEvents.WindowCreated -= windowEvents_WindowCreated;
                if (ShouldHookWindowClosing)
                    windowEvents.WindowClosing -= windowEvents_WindowClosing;
            }
        }
        public virtual void OnWindowActivated(Window GotFocus, Window LostFocus)
        { }
        public virtual void OnWindowClosing(Window ClosingWindow)
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

        private void windowEvents_WindowClosing(Window windowClosing)
        {
            OnWindowClosing(windowClosing);
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
