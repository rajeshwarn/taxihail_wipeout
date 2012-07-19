using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace apcurium.MK.Common.Diagnostic
{
    public class Logger : ILogger
    {
        public void LogError(Exception ex)
        {
            Trace.TraceError(ex.Message);
        }

        public void LogMessage(string message)
        {
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
