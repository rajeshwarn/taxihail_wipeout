using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using ILogger = apcurium.MK.Common.Diagnostic.ILogger;
using Java.Lang;
using Exception = System.Exception;
using System.Collections.Generic;
using apcurium.MK.Common.Extensions;
using Xamarin.GoogleAnalyticsV2.Tracking;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    // V2
    public class GoogleAnalyticsService : IAnalyticsService
    {
        private List<Tracker> Trackers { get; set; }

        public GoogleAnalyticsService(Context c, IPackageInfo packageInfo, IAppSettings settings, ILogger logger)
        {

            Trackers = new List<Tracker>();

            try
            {
                GAServiceManager.Instance.SetDispatchPeriod(20);

                var instance = GoogleAnalytics.GetInstance(c);

                // Prevent testing/debugging data from being sent to GA
                #if DEBUG
                instance.SetDebug(true);
                #endif

                // MK's tracking id
                Trackers.Add(instance.GetTracker("UA-44714416-1"));

                if (settings.Data.GoogleAnalyticsTrackingId.HasValue())
                {
                    // Company's own tracking id
                    Trackers.Add(instance.GetTracker(settings.Data.GoogleAnalyticsTrackingId));
                }

                var appName = settings.Data.TaxiHail.ApplicationName.Replace(' ', '_');
                var version = packageInfo.Version;
                Trackers.ForEach(x => {
                    x.SetAppName(appName);
                    x.SetAppVersion(version);
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
            }
        }

        public void LogViewModel(string name)
        {
            Trackers.ForEach(x => x.SendView(name));
        }

        public void LogNavigation(string source, string destination)
        {
            Trackers.ForEach(x => x.TrackEvent("Interaction", "NavigationRequested", source + " to " + destination, new Long(0)));
        }

        public void LogCommand(string commandName, string parameter)
        {
            Trackers.ForEach(x => x.SendEvent("Interaction", "Command Issued", commandName + "(" + parameter + ")", new Long(0)));
        }

        public void LogException(string className, string methodName, Exception e, bool isFatal = false)
        {
            Trackers.ForEach(x => x.TrackException(className + ":" + methodName + ": " + e.Message, isFatal));
        }

        public void LogEvent(string @event)
        {
            Trackers.ForEach(x => x.SendEvent("Interaction", "Event", @event, new Long(0)));
        }

        public void ReportConversion()
        {
            // nothing to do, android does it for us
        }
    }
}