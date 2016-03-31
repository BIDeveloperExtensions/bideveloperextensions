using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDSHelper.Core.VsIntegration
{
    public class StatusBar
    {
        private IServiceProvider _serviceProvider;
        private uint pdwCookie;
        public StatusBar(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected IVsStatusbar StatusBarService
        { get { return (IVsStatusbar)_serviceProvider.GetService(typeof(SVsStatusbar)); } }

        public void Animate(bool fOnOff, VsIntegration.VsStatusBarAnimations animation ) {
            StatusBarService.Animation(Convert.ToInt32(fOnOff), animation );
        }

        public void Progress(bool inProgress, string label, int complete, int total) {
            StatusBarService.Progress(ref pdwCookie, Convert.ToInt32(inProgress), label, Convert.ToUInt32( complete), Convert.ToUInt32(total));
        }

        public void Clear()
        {
            StatusBarService.Clear();
        }
    }
}
