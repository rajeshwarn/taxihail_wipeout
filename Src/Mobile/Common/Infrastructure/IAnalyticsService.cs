using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IAnalyticsService
    {
        
        void LogViewModel (string name);

        void LogNavigation (string source, string destination);
        void LogCommand (string commandName, string parameter);

        void LogException(string className, string methodName, Exception e, bool isFatal = false);

    }
}

