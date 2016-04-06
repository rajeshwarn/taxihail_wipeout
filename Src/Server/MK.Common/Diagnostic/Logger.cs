#region

using System;
using System.Diagnostics;
using apcurium.MK.Common.Extensions;
using log4net;

#endregion

namespace apcurium.MK.Common.Diagnostic
{
	public class Logger : ILogger
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Logger));
        
        public void LogError(Exception ex)
        {
            LogError(ex, null, -1);
        }

	    public void LogError(Exception ex, string method, int lineNumber, bool minimalLogging = false)
        {
            var errorLocation = method.HasValueTrimmed() && lineNumber > -1
                ? " at {0}:{1}".InvariantCultureFormat(method, lineNumber)
                : string.Empty;

	        var errorFormat = string.Format("{0}{1}{2}", ex.Message, errorLocation,
	            minimalLogging ? string.Empty : " " + ex.StackTrace);
            Log.Error(errorFormat);
            if (ex.InnerException != null)
            {
                LogError(ex.InnerException, null, -1, minimalLogging);
            }
        }

        public void LogMessage(string message, params object[] args)
        {
            if ((args != null) && (args.Length > 0))
            {
                message = string.Format(message, args);
            }
            Log.Info(message);
        }

        public void LogStack()
        {
            var stackTrace = new StackTrace(); // get call stack
            var stackFrames = stackTrace.GetFrames(); // get method calls (frames)

            // write call stack method names
            if (stackFrames != null)
            {
                foreach (var stackFrame in stackFrames)
                {
                    if (stackFrame.GetMethod().Name != "LogStack")
                    {
                        LogMessage("Stack : " + stackFrame.GetMethod().Name); // write method name
                    }
                }
            }
        }

        public IDisposable StartStopwatch(string message)
        {
            var w = new Stopwatch();
            w.Start();
            LogMessage("Start: " + message);
            return Disposable.Create(() =>
            {
                w.Stop();
                LogMessage("Stop:  " + message + " Execution time : " + w.ElapsedMilliseconds + " ms");
            });
        }

        public string[] GetLogFilesFullName()
        {
            throw new NotSupportedException("This call is only supported on mobile.");
        }

        private class Disposable : IDisposable
        {
            private readonly Action _action;

            public Disposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                if (_action != null)
                {
                    _action();
                }
            }

            public static IDisposable Create(Action action)
            {
                return new Disposable(action);
            }
        }

		public string GetLogFileName()
		{
			throw new NotSupportedException("This is supported only on mobile.");
		}
	}
}