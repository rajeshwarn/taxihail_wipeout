using System;

using System.Diagnostics;

namespace apcurium.MK.Common.Diagnostic
{
    public class Logger : ILogger
    {
        public void LogError(Exception ex)
        {
            Trace.TraceError(ex.Message + " " + ex.StackTrace);
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
            Trace.WriteLine(message);            
        }

        public void StartStopwatch(string message)
        {
        }


        public void StopStopwatch(string message)
        {
        }

        public void LogStack()
        {
            
        }
    }
}
