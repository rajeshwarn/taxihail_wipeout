using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class RateApplicationService:BaseService, IRateApplicationService
	{
		public static string RateApplicationServiceSettingsName = "RateApplicationServiceSettings";

		private readonly IMessageService _messageService;
		private readonly IDeviceRateApplicationService _deviceRateApplicationService;
		private readonly ILocalization _localization;
		private readonly IAppSettings _applicationSettings;
		private RateApplicationState _currentState;

		public RateApplicationService(IMessageService messageService, IDeviceRateApplicationService deviceRateApplicationService, ILocalization localization, IAppSettings applicationSettings)
		{
			_messageService = messageService;
			_deviceRateApplicationService = deviceRateApplicationService;
			_localization = localization;
			_applicationSettings = applicationSettings;

			LoadCurrentSettings();
		}

		public void LoadCurrentSettings()
		{
			var currentStateText = UserCache.Get<string>(RateApplicationServiceSettingsName);
			var currentState = RateApplicationState.NotRated;

			if (currentStateText.HasValue())
			{
				Enum.TryParse<RateApplicationState>(currentStateText, out currentState);
			}
		}

		public void SetState(RateApplicationState rateApplicationState, DateTime stateTime, ApplicationVersion applicationVersion)
		{
			_currentState = rateApplicationState;
			UserCache.Set<string>(RateApplicationServiceSettingsName, _currentState.ToString());
		}

		public bool CanShowRateApplicationDialog(int successfulTripsNumber)
		{
			bool result = false;

			if ((_currentState == RateApplicationState.NotRated || _currentState == RateApplicationState.Postponed)
				&& (_applicationSettings.Data.MinimumTripsForAppRating == 0
					|| (successfulTripsNumber > 0 && successfulTripsNumber % _applicationSettings.Data.MinimumTripsForAppRating == 0)))
			{
				result = true;
			}

			return result;
		}

		public RateApplicationState CurrentRateApplicationState()
		{
			return _currentState;
		}

		public void ShowRateApplicationDialog()
		{
			_messageService.ShowMessage(_localization["RateMobileApplicationTitle"], _localization["RateMobileApplicationText"],
				_localization["RateMobileApplicationNow"], RedirectToRatingPage,
				_localization["RateMobileApplicationNever"], CancelRatingPopup,
				_localization["RateMobileApplicationLater"], PostponeRatingPopup);
		}

		private void RedirectToRatingPage()
		{
			UserCache.Set<string>(RateApplicationServiceSettingsName, RateApplicationState.Rated.ToString());
			if (!_deviceRateApplicationService.RedirectToRatingPage())
			{
				_messageService.ShowMessage(_localization["LinkForApplicationStoreAbsentTitle"], _localization["LinkForApplicationStoreAbsentText"]);
			}
		}

		private void PostponeRatingPopup()
		{
			UserCache.Set<string>(RateApplicationServiceSettingsName, RateApplicationState.Postponed.ToString());
		}

		private void CancelRatingPopup()
		{
			UserCache.Set<string>(RateApplicationServiceSettingsName, RateApplicationState.Ignored.ToString());
		}
	}
}