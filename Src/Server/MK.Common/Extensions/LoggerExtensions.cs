using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using apcurium.MK.Common.Diagnostic;

namespace MK.Common.Android.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogErrorWithCaller(this ILogger logger, Exception ex, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = -1)
        {
            logger.LogError(ex, memberName, lineNumber);
        }
    }
}