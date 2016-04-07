using BIDSHelper;
using BIDSHelper.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDSHelper.Core.Logger
{
    public class OutputLogger : ILog
    {
        private BIDSHelperPackage _package;

        public OutputLogger() { LogLevel = LogLevels.None; }
        public LogLevels LogLevel { get; set; }

        public OutputLogger(BIDSHelperPackage package)
        {
            _package = package;
        }
        private string FormatMessage(string level, string message)
        {
            return string.Format("{0} {1} {2}\n", DateTime.Now.ToString("HH:mm:ss"), level, message);
        }

        public void Error(string message)
        {
            if (LogLevel >= LogLevels.Error)
                _package.OutputString(FormatMessage("ERROR",message));
        }

        public void Info(string message)
        {
            if (LogLevel >= LogLevels.Info)
                _package.OutputString(FormatMessage("INFO", message));
        }

        public void Verbose(string message)
        {
            if (LogLevel >= LogLevels.Verbose)
                _package.OutputString(FormatMessage("VERBOSE", message));
        }

        public void Warn(string message)
        {
            if (LogLevel >= LogLevels.Warning)
                _package.OutputString(FormatMessage("WARN", message));
        }

        public void Exception(string message, Exception ex)
        {
            Error(string.Format("{0} - {1}\n{2}", message, ex.Message, ex.StackTrace));
        }
    }
}
