using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;

namespace apcurium.MK.Common.Diagnostic
{
    public abstract class BaseLogger : ILogger
    {
        private static bool _flushNextWrite;

        public void LogError(Exception ex)
        {
            if (ex is AggregateException) 
            {
                ((AggregateException)ex).Handle (x => 
                {
                    LogError (x, 0);
                    return true;
                });
            } 
            else 
            {            
                LogError (ex, 0);
            }
        }

        public void LogMessage(string message, params object[] args)
        {
            if ((args != null) && (args.Length > 0))
            {
                message = string.Format(message, args);
            }

            Write(string.Format ("Message on {0} : {1}", DateTime.Now, message));
        }

        public void LogStack ()
        {
            var stackTrace = new StackTrace();           // get call stack
            var stackFrames = stackTrace.GetFrames();    // get method calls (frames)

            // write call stack method names
            if (stackFrames != null)
            {
                foreach (var stackFrame in stackFrames)
                {
                    if (stackFrame.GetMethod().Name != "LogStack")
                    {
                        Write(string.Format ("Stack: {0}", stackFrame.GetMethod().Name)); // write method name
                    }
                }
            }
        }

        public IDisposable StartStopwatch (string message)
        {
            var w = new Stopwatch();
            w.Start();
            LogMessage(string.Format("Start: {0}", message));
            return Disposable.Create (() => 
                {
                    w.Stop();
                    LogMessage(string.Format("Stop: {0} Execution time : {1} ms", message, w.ElapsedMilliseconds));
                });
        }

        private void LogError(Exception ex, int indent)
        {
            string indentStr = "";
            for (int i = 0; i < indent; i++)
            {
                indentStr += "   ";
            }
            if (indent == 0)
            {
                Write(string.Format("{0}Error on {1}", indentStr, DateTime.Now));
            }

            Write(string.Format ("{0}Message : {1}", indentStr, ex.Message));
            Write(string.Format ("{0}Stack : {1}", indentStr, ex.StackTrace));

            if (ex.InnerException != null)
            {
                LogError(ex.InnerException, ++indent);
            }
        }

        public void FlushNextWrite ()
        {
            _flushNextWrite = true;
        }

        protected void DeleteLogIfNecessary()
        {
            var filePath = GetErrorLogPath ();
            if (File.Exists(filePath) && _flushNextWrite)
            {
                File.Delete(filePath);
            }

            _flushNextWrite = false;
        }

        public abstract string GetErrorLogPath();

        protected abstract void Write(string message);
    }
}