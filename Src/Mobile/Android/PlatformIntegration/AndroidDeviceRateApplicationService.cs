using Android.App;
using Android.Content;
using Android.Net;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class AndroidDeviceRateApplicationService:IDeviceRateApplicationService
	{
		/// <summary>
		/// http://developer.android.com/distribute/tools/promote/linking.html
		/// </summary>

		readonly private ILogger _logger;
		readonly private IAppSettings _settings;

		public AndroidDeviceRateApplicationService(ILogger logger, IAppSettings settings)
		{
			_logger = logger;
			_settings = settings;
		}

		public bool RedirectToRatingPage()
		{
			if (!_settings.Data.Store.PlayLink.HasValue())
			{
				return false;
			}

			var androidMarketLink = Uri.Parse(_settings.Data.Store.PlayLink);
			var androidMarketLinkIntent = new Intent(Intent.ActionView, androidMarketLink);
			androidMarketLinkIntent.SetFlags(ActivityFlags.NewTask);

			try
			{
				Application.Context.StartActivity(androidMarketLinkIntent);
			}
			catch (ActivityNotFoundException exception)
			{
				_logger.LogError(exception);
			}

			return true;
		}
	}
}