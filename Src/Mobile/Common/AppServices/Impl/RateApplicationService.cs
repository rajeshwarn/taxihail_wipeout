using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public class RateApplicationService:IRateApplicationService
	{
		private IMessageService _messageService;
		private IDeviceRateApplicationService _deviceRateApplicationService;
		private ILocalization _localization;
		private IAppSettings _applicationSettings;
		private RateApplicationServiceSettings _currentRatingState;
		private ApplicationVersion _currentApplicationVersion;

		public RateApplicationService(IMessageService messageService, IDeviceRateApplicationService deviceRateApplicationService, IPackageInfo packageInfo, ILocalization localization, IAppSettings applicationSettings)
		{
			_messageService = messageService;
			_deviceRateApplicationService = deviceRateApplicationService;
			_localization = localization;
			_applicationSettings = applicationSettings;

			_currentRatingState = new RateApplicationServiceSettings();
			_currentRatingState.LoadCurrentSettings();
			_currentApplicationVersion = new ApplicationVersion(packageInfo.Version);
		}

		public bool IsShowRateApplicationDialog(int numberOfTripsToAllowRating)
		{
			bool result = false;

			if ((_currentRatingState.RateApplicationState == RateApplicationState.NotRated
					|| _currentRatingState.RateApplicationState == RateApplicationState.Postponed)
				&& (_applicationSettings.Data.RateMobileApplicationMinimumSuccessfulTrips == 0
					|| (numberOfTripsToAllowRating > 0 && numberOfTripsToAllowRating % _applicationSettings.Data.RateMobileApplicationMinimumSuccessfulTrips == 0)))
			{
				result = true;
			}

			return result;
		}

		public RateApplicationState CurrentRateApplicationState()
		{
			return _currentRatingState.RateApplicationState;
		}

		public void ShowRateApplicationSuggestDialog()
		{
			_messageService.ShowMessage(_localization["RateMobileApplicationTitle"], _localization["RateMobileApplicationText"],
				_localization["RateMobileApplicationNow"], RedirectToRatingPage,
				_localization["RateMobileApplicationNever"], CancelRatingPopup,
				_localization["RateMobileApplicationLater"], PostponeRatingPopup);
		}

		private void RedirectToRatingPage()
		{
			_currentRatingState.SetState(RateApplicationState.Rated, DateTime.Now, _currentApplicationVersion);
			_deviceRateApplicationService.RedirectToRatingPage();
		}

		private void PostponeRatingPopup()
		{
			_currentRatingState.SetState(RateApplicationState.Postponed, DateTime.Now, _currentApplicationVersion);
		}

		private void CancelRatingPopup()
		{
			_currentRatingState.SetState(RateApplicationState.Cancelled, DateTime.Now, _currentApplicationVersion);
		}
	}
}