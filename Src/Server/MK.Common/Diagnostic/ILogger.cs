using System;

namespace apcurium.MK.Common.Diagnostic
{
    public interface ILogger
    {
        void LogError(Exception ex);

        void LogMessage(string message, params object[] args);

        void LogStack();

        IDisposable StartStopwatch(string message);

		string[] GetLogFilesFullName();

		string MergeLogFiles();

		void RemoveMergedFile();
    }
}