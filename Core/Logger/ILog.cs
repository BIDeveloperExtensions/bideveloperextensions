using System;

namespace BIDSHelper.Core.Logger
{
    public interface ILog
    {
        LogLevels LogLevel { get; set; }
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Verbose(string message);
        void Exception(string message, Exception ex);
    }
}