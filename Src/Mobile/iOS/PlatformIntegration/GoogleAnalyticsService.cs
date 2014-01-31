using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using GoogleAnalytics;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class GoogleAnalyticsService: IAnalyticsService
    {

        GAITracker Tracker {
            get;
            set;
        }

        public GoogleAnalyticsService (IAppSettings settings,IPackageInfo packageInfo)
        {
            GAI.SharedInstance.TrackUncaughtExceptions = true;
            GAI.SharedInstance.DispatchInterval = 20;
            GAI.SharedInstance.Debug = true;// Optional: set debug to YES for extra debugging information.

            Tracker = GAI.SharedInstance.GetTracker (  "UA-44714416-1" );
            Tracker.AppName =  settings.Data.ApplicationName.Replace( ' ' , '_' );
            Tracker.AppVersion = packageInfo.Version;


        }

        public void LogViewModel (string viewModelName){
            Tracker.TrackView (viewModelName);
        }

        public void LogCommand(string commandName, string parameter)
        {
            Tracker.TrackEvent ("Interaction","Command Issued",commandName+"("+parameter +")", 0);
        }

        public void LogNavigation(string source, string destination)
        {
            Tracker.TrackEvent ("Interaction","NavigationRequested",source +" to "+ destination, 0);
        }
        public void LogException(string className, string methodName, Exception e, bool isFatal = false)
        {
            Tracker.TrackException (isFatal,className+":"+methodName+": "+e.Message);
        }
    }
}

