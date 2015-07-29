using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;

using apcurium.MK.Common.Extensions;

using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using Foundation;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    // v3.0.10.2
    public class GoogleAnalyticsService: IAnalyticsService
	{
        private IAppSettings _settings;


		public GoogleAnalyticsService (IAppSettings settings, IPackageInfo packageInfo)
		{
            _settings = settings;

        
		}

		public void LogViewModel (string viewModelName)
        {
        }

		public void LogEvent(string @event)
		{
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
                   // ACTConversionReporter.ReportWithConversionID((NSString)conversionId, (NSString)label, (NSString)"1.000000", false);
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

