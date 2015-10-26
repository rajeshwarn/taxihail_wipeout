using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class AndroidDeviceRateApplicationService:IDeviceRateApplicationService
	{
		/// <summary>
		/// http://developer.android.com/distribute/tools/promote/linking.html
		/// </summary>

		private ILogger _logger;
		private IAppSettings _settings;

		public AndroidDeviceRateApplicationService(ILogger logger, IAppSettings settings)
		{
			_logger = logger;
			_settings = settings;
		}

		public void RedirectToRatingPage()
		{
			Uri androidMarketLink = Uri.Parse(_settings.Data.PlayLink);
			Intent androidMarketLinkIntent = new Intent(Intent.ActionView, androidMarketLink);
			androidMarketLinkIntent.SetFlags(ActivityFlags.NewTask);

			try
			{
				Application.Context.StartActivity(androidMarketLinkIntent);
			}
			catch (ActivityNotFoundException exception)
			{
				_logger.LogError(exception);
			}
		}
	}
}