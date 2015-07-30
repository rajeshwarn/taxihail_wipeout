using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using ILogger = apcurium.MK.Common.Diagnostic.ILogger;
using Java.Lang;
using Exception = System.Exception;
using System.Collections.Generic;
using apcurium.MK.Common.Extensions;
using Android.Gms.Analytics;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    // V3
    public class GoogleAnalyticsService : IAnalyticsService
    {
        private List<Tracker> Trackers { get; set; }

        public GoogleAnalyticsService(Context c, IPackageInfo packageInfo, IAppSettings settings, ILogger logger)
        {
            Trackers = new List<Tracker>();

            try
            {
                var instance = GoogleAnalytics.GetInstance(c);
                instance.SetLocalDispatchPeriod(20);

                // Prevent testing/debugging data from being sent to GA
                #if DEBUG
                instance.SetDryRun(true);
                #endif

                // MK's tracking id
                Trackers.Add(instance.NewTracker("UA-44714416-1"));

                if (settings.Data.GoogleAnalyticsTrackingId.HasValue())
                {
                    // Company's own tracking id
                    Trackers.Add(instance.NewTracker(settings.Data.GoogleAnalyticsTrackingId));
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
            Trackers.ForEach(x => { 
                x.SetScreenName(name);
                x.Send(new HitBuilders.ScreenViewBuilder().Build()); 
            });
        }

        public void LogEvent(string @event)
        {
            var eventBuilder = new HitBuilders.EventBuilder("Interaction", "Event");
            eventBuilder.SetLabel(@event);

            Trackers.ForEach(x => x.Send(eventBuilder.Build()));
        }

        public void LogException(string className, string methodName, Exception e, bool isFatal = false)
        {
            var exceptionBuilder = new HitBuilders.ExceptionBuilder();
            exceptionBuilder.SetDescription(className + ":" + methodName + ": " + e.Message);
            exceptionBuilder.SetFatal(isFatal);

            Trackers.ForEach(x => x.Send(exceptionBuilder.Build()));
        }

        public void LogNavigation(string source, string destination)
        {
        }

        public void LogCommand(string commandName, string parameter)
        {
        }

        public void ReportConversion()
        {
            // nothing to do, android does it for us
        }
    }
}