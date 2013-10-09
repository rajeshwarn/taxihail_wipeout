using System;

using GoogleAnalytics;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class GoogleAnalyticsService: IAnalyticsService
    {

        GAITracker Tracker {
            get;
            set;
        }

        public GoogleAnalyticsService (IAppSettings settings,IPackageInfo packageInfo)
        {
            GoogleAnalytics.GAI.SharedInstance.TrackUncaughtExceptions = true;
            GoogleAnalytics.GAI.SharedInstance.DispatchInterval = 20;
            GoogleAnalytics.GAI.SharedInstance.Debug = true;// Optional: set debug to YES for extra debugging information.

            Tracker = GoogleAnalytics.GAI.SharedInstance.GetTracker (  "UA-44714416-1" );
            Tracker.AppName =  settings.ApplicationName.Replace( ' ' , '_' );
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

