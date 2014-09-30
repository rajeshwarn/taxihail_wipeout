using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using GoogleAnalytics;
using GoogleAnalytics.iOS;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    // v3.0.3.4
	public class GoogleAnalyticsService: IAnalyticsService
	{
		private IGAITracker Tracker { get; set; }

		public GoogleAnalyticsService (IAppSettings settings,IPackageInfo packageInfo)
		{
			GAI.SharedInstance.TrackUncaughtExceptions = true;
			GAI.SharedInstance.DispatchInterval = 20;

            // Prevent testing/debugging data from being sent to GA
            #if DEBUG
            GAI.SharedInstance.DryRun = true;
            #endif

            Tracker = GAI.SharedInstance.GetTracker ("UA-44714416-1");
			Tracker.Set(GAIConstants.AppName, settings.Data.ApplicationName.Replace(' ' , '_'));
			Tracker.Set(GAIConstants.AppVersion, packageInfo.Version);
		}

		public void LogViewModel (string viewModelName)
        {
			var appView = GAIDictionaryBuilder.CreateAppView ();
			appView.Set (viewModelName, GAIConstants.ScreenName);
			Tracker.Send(appView.Build());
		}

		public void LogEvent(string @event)
		{
			var eventGA = GAIDictionaryBuilder.CreateEvent ("Interaction", "Event", @event, 0);
			Tracker.Send (eventGA.Build());
		}

		public void LogCommand(string commandName, string parameter)
		{
		}

		public void LogNavigation(string source, string destination)
		{

		}

		public void LogException(string className, string methodName, Exception e, bool isFatal = false)
		{   
//            var exception = GAIDictionaryBuilder.CreateException (e.Message, new NSNumber (isFatal));
//            exception.Set(GAIConstants.ex
//            Tracker.Send(exception.Build());
		}
	}
}

