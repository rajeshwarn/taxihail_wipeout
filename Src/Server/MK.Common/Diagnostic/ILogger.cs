#region

using System;

#endregion

namespace apcurium.MK.Common.Diagnostic
{
    public interface ILogger
    {
        void LogError(Exception ex);

        void LogMessage(string message, params object[] args);

        void LogStack();

        string GetStack(int position);

        IDisposable StartStopwatch(string message);
    }
}