using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Com.Google.Analytics.Tracking.Android;
using Java.Lang;
using Exception = System.Exception;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    // V2
    public class GoogleAnalyticsService : IAnalyticsService
    {
        private Tracker Tracker { get; set; }

        public GoogleAnalyticsService(Context c, IPackageInfo packageInfo, IAppSettings settings, ILogger logger)
        {
            try
            {
                GAServiceManager.Instance.SetDispatchPeriod(20);

                var instance = Com.Google.Analytics.Tracking.Android.GoogleAnalytics.GetInstance(c);

                // Prevent testing/debugging data from being sent to GA
                #if DEBUG
                instance.SetDebug(true);
                #endif

                Tracker = instance.GetTracker("UA-44714416-1");
                Tracker.SetAppName(settings.Data.ApplicationName.Replace(' ', '_'));
                Tracker.SetAppVersion(packageInfo.Version);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
            }
        }

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

		public void LogEvent(string @event)
		{
			Tracker.SendEvent("Interaction", "Event", @event, new Long(0));
		}

        public void LogException(string className, string methodName, Exception e, bool isFatal = false)
        {
            Tracker.TrackException(className + ":" + methodName + ": " + e.Message, isFatal);
        }
    }
}