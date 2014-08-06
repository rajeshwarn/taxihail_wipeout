using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using GoogleAnalytics;
using GoogleAnalytics.iOS;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class GoogleAnalyticsService: IAnalyticsService
	{

		IGAITracker Tracker {
			get;
			set;
		}

		public GoogleAnalyticsService (IAppSettings settings,IPackageInfo packageInfo)
		{
			GAI.SharedInstance.TrackUncaughtExceptions = true;
			GAI.SharedInstance.DispatchInterval = 20;


			Tracker = GAI.SharedInstance.GetTracker (  "UA-44714416-1" );
			Tracker.Set( GAIConstants.AppName,   settings.Data.ApplicationName.Replace( ' ' , '_' ));
			Tracker.Set(GAIConstants.AppVersion, packageInfo.Version);


		}

		public void LogViewModel (string viewModelName){
			var appView = GAIDictionaryBuilder.CreateAppView ();
			appView.Set (viewModelName, GAIConstants.ScreenName);
			Tracker.Send(  appView.Build() );
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

		}
	}
}

