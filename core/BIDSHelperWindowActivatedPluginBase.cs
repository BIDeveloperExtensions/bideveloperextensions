using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using EnvDTE80;
using System.Runtime.InteropServices;
using BIDSHelper.Core;

namespace BIDSHelper
{
    public abstract class BIDSHelperWindowActivatedPluginBase
        : BIDSHelperPluginBase
        , IWindowActivatedPlugin
    {
        [DllImport("user32.dll")]
        static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate,
                          IntPtr hrgnUpdate, uint flags);
        const int RDW_INVALIDATE  = 0x0001;
        const int RDW_ERASE       = 0x0004;
        const int RDW_UPDATENOW   = 0x0100;
        const int RDW_ALLCHILDREN = 0x0080;

        private WindowEvents windowEvents;

                #region "Constructors"
        public BIDSHelperWindowActivatedPluginBase(BIDSHelperPackage package)
            : base(package)
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
            package.Log.Debug("BIDSHelperWindowActivatedPluginBase HookWindowActivation fired (" + this.GetType().Name + ")");
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
            this.package.Log.Debug("BIDSHelperWindowActivatedPlugin " + this.IdeMode + " (" + this.GetType().Name + ")");
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
            package.Log.Debug("BIDSHelperWindowActivatedPluginBase OnEnable fired");
            base.OnEnable();
            this.HookWindowActivation();
            try
            {
                // force OnWindowActivate to fire
                OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            this.UnHookWindowActivation();
            // force the active window to repaint
            RedrawWindow( new IntPtr(this.ApplicationObject.ActiveWindow.HWnd) , IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_ERASE | RDW_UPDATENOW | RDW_ALLCHILDREN);
        }
    }
}
