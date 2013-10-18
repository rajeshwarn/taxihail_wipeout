using System;

using System.Diagnostics;
using log4net;


namespace apcurium.MK.Common.Diagnostic
{
    public class Logger : ILogger
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(Logger));      


        public void LogError(Exception ex)
        {
            Log.Error( ex.Message + " " + ex.StackTrace);
            if (ex.InnerException != null)
            {
                LogError(ex.InnerException);
            }
        }

        public void LogMessage(string message, params object[] args)  
         {
            if ( ( args != null ) && ( args.Length > 0 ))
            {
                message = string.Format(message, args);
            }
            Log.Info( message );        
        }


      

        public void LogStack()
        {
            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            // write call stack method names
            foreach (StackFrame stackFrame in stackFrames)
            {
                if (stackFrame.GetMethod().Name != "LogStack")
                {
                    LogMessage ("Stack : " + stackFrame.GetMethod().Name);   // write method name
                }
            }

        }

        public string GetStack(int position)
        {
            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            return stackFrames[position].GetMethod().Name;
        }


        public IDisposable StartStopwatch(string message)
        {
            
            var w = new Stopwatch();
            w.Start();
            LogMessage("Start: " + message);
            return Disposable.Create(() =>
            {
                w.Stop();
                LogMessage("Stop:  " + message + " Execution time : " + w.ElapsedMilliseconds.ToString() + " ms");
            });
        }

        private class Disposable : IDisposable
        {
            public static IDisposable Create(Action action)
            {
                return new Disposable(action);
            }

            private Action _action;
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
        }


    }
}
