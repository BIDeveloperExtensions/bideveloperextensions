using BIDSHelper.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDSHelper.Core.Logger
{
    public class NullLogger : ILog
    {
        public LogLevels LogLevel { get; set; }

        public void Debug(string message)
        {
            // do nothing
        }

        public void Error(string message)
        {
            // do nothing
        }

        public void Exception(string message, Exception ex)
        {
            // do nothing
        }

        public void Info(string message)
        {
            // do nothing
        }

        public void Verbose(string message)
        {
            // do nothing
        }

        public void Warn(string message)
        {
            // do nothing
        }
    }
}
