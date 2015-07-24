using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
//using GoogleAnalytics.iOS;
using apcurium.MK.Common.Extensions;
using GoogleConversionTracking.Unified;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using Foundation;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    // v3.0.10.2
    public class GoogleAnalyticsService: IAnalyticsService
	{
        private IAppSettings _settings;
		//private List<IGAITracker> Trackers { get; set; }

		public GoogleAnalyticsService (IAppSettings settings, IPackageInfo packageInfo)
		{
            _settings = settings;

//            Trackers = new List<IGAITracker>();
//
//			GAI.SharedInstance.TrackUncaughtExceptions = true;
//			GAI.SharedInstance.DispatchInterval = 20;
//
//            // Prevent testing/debugging data from being sent to GA
//            #if DEBUG
//            GAI.SharedInstance.DryRun = true;
//            #endif
//
//            // MK's tracking id
//            Trackers.Add(GAI.SharedInstance.GetTracker ("UA-44714416-1"));
//
//            if (_settings.Data.GoogleAnalyticsTrackingId.HasValue())
//            {
//                // Company's own tracking id
//                Trackers.Add(GAI.SharedInstance.GetTracker (_settings.Data.GoogleAnalyticsTrackingId));
//            }
//
//            var appName = settings.Data.TaxiHail.ApplicationName.Replace(' ', '_');
//            var version = packageInfo.Version;
//            Trackers.ForEach(x => {
//                x.Set(GAIConstants.AppName, appName);
//                x.Set(GAIConstants.AppVersion, version);
//            });
		}

		public void LogViewModel (string viewModelName)
        {
//            var appView = GAIDictionaryBuilder.CreateScreenView ();
//			appView.Set (viewModelName, GAIConstants.ScreenName);
//            Trackers.ForEach(x => x.Send(appView.Build()));
		}

		public void LogEvent(string @event)
		{
			//var eventGA = GAIDictionaryBuilder.CreateEvent ("Interaction", "Event", @event, 0);
            //Trackers.ForEach(x => x.Send (eventGA.Build()));
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

        public void ReportConversion()
        {
            #if !DEBUG
            var conversionId = _settings.Data.GoogleAdWordsConversionId;
            var label = _settings.Data.GoogleAdWordsConversionLabel;
            if(conversionId.HasValue() && label.HasValue())
            {
                try
                {
                    ACTConversionReporter.ReportWithConversionID((NSString)conversionId, (NSString)label, (NSString)"1.000000", false);
                }
                catch (Exception e)
                {
                    Logger.LogError (e);
                }
            }
            #endif
        }
	}
}

