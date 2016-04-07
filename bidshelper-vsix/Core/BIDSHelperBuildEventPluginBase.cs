using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using EnvDTE80;
using System.Runtime.InteropServices;
using BIDSHelper.Core;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

namespace BIDSHelper
{
    public abstract class BIDSHelperBuildEventPluginBase
        : BIDSHelperPluginBase
        , IVsUpdateSolutionEvents2
    {
        private IVsSolutionBuildManager2 sbm;
        private uint updateSolutionEventsCookie;

        #region "Constructors"
        public BIDSHelperBuildEventPluginBase(BIDSHelperPackage package)
            : base(package)
        {
            
        }


        #endregion

        
        public override void OnEnable()
        {
            package.Logger.Info("BIDSHelperWindowActivatedPluginBase OnEnable fired");
            base.OnEnable();
            this.HookBuildEvents();
        }

        private void HookBuildEvents()
        {
            // Get solution build manager
            sbm = ServiceProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            if (sbm != null)
            {
                sbm.AdviseUpdateSolutionEvents(this, out updateSolutionEventsCookie);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            this.UnHookBuildEvents();
        }

        private void UnHookBuildEvents()
        {
            if (sbm != null && updateSolutionEventsCookie != 0)
            {
                sbm.UnadviseUpdateSolutionEvents(updateSolutionEventsCookie);
            }
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
        //    OnBeginBuild(ref pfCancelUpdate);

            return VSConstants.S_OK;
        }

        // internal abstract void OnBeginBuild(ref int pfCancelUpdate);

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            // do nothing
            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            // do nothing
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            // do nothing
            return VSConstants.S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            // do nothing
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            OnUpdateConfigBegin(pHierProj, (Microsoft.VisualStudio.Shell.Interop.VSSOLNBUILDUPDATEFLAGS)dwAction, ref pfCancel);
            return VSConstants.S_OK;
        }

        internal abstract void OnUpdateConfigBegin(IVsHierarchy pHierProj, VSSOLNBUILDUPDATEFLAGS dwAction, ref int pfCancel);

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            // do nothing
            return VSConstants.S_OK;
        }
    }
}
