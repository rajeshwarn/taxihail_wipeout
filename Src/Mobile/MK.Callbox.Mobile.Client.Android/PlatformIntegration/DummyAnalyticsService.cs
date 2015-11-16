using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Callbox.Mobile.Client.PlatformIntegration
{

	// This is a dummy service for Callbox
	public class DummyAnalyticsService: IAnalyticsService
	{
		public void LogViewModel(string name)
		{
			
		}

		public void LogNavigation(string source, string destination)
		{
			
		}

		public void LogCommand(string commandName, string parameter)
		{
			
		}

		public void LogException(string className, string methodName, Exception e, bool isFatal = false)
		{
			
		}

		public void LogEvent(string @event)
		{
			
		}

		public void ReportConversion()
		{
			
		}
	}
}