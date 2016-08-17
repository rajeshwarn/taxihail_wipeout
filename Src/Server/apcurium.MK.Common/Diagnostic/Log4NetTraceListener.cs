using System.Diagnostics;
using log4net;

namespace apcurium.MK.Common.Diagnostic
{
    public class Log4netTraceListener : TraceListener
    {
        private readonly ILog _log;

        public Log4netTraceListener()
        {
            _log = LogManager.GetLogger("System.Diagnostics.Redirection");
        }

        public Log4netTraceListener(ILog log)
        {
            _log = log;
        }

        public override void Write(string message)
        {
            if (_log != null)
            {
                _log.Debug(message);
            }
        }

        public override void WriteLine(string message)
        {
            if (_log != null)
            {
                _log.Debug(message);
            }
        }
    }
}