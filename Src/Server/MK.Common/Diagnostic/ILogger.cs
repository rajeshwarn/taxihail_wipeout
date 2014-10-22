using System;

namespace apcurium.MK.Common.Diagnostic
{
    public interface ILogger
    {
        string GetErrorLogPath();

        void LogError(Exception ex);

        void LogMessage(string message, params object[] args);

        void LogStack();

        void FlushNextWrite();

        IDisposable StartStopwatch(string message);
    }
}