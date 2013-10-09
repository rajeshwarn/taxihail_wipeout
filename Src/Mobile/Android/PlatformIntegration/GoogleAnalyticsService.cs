using Android.Content;
using Com.Google.Analytics.Tracking.Android;
using Java.Lang;

using Exception = System.Exception;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class GoogleAnalyticsService : IAnalyticsService
    {
        #region IAnaliticsService implementation

        public GoogleAnalyticsService(Context c, IPackageInfo packageInfo, IAppSettings settings)
        {

            try{

            GAServiceManager.Instance.SetDispatchPeriod(20);

            var g = Com.Google.Analytics.Tracking.Android.GoogleAnalytics.GetInstance(c);
            g.SetDebug(true);

            Tracker = g.GetTracker("UA-44714416-1");

            Tracker.SetAppName(  settings.ApplicationName.Replace( ' ' , '_' ) );
            Tracker.SetAppVersion(packageInfo.Version);
            }
            catch ( Exception ex )
            {
                ex.ToString();
            }
        }

        protected Tracker Tracker { get; set; }

        public void LogViewModel(string name)
        {
            Tracker.SendView(name);
        }

        public void LogNavigation(string source, string destination)
        {
            Tracker.TrackEvent("Interaction", "NavigationRequested", source + " to " + destination, new Long(0));
        }

        public void LogCommand(string commandName, string parameter)
        {
            Tracker.SendEvent("Interaction", "Command Issued", commandName + "(" + parameter + ")", new Long(0));
        }

        public void LogException(string className, string methodName, Exception e, bool isFatal = false)
        {
            Tracker.TrackException(className + ":" + methodName + ": " + e.Message, isFatal);

        }

        #endregion
    }
}

