using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class RateApplicationService : BaseService, IRateApplicationService
	{
        public const string RateApplicationStateKey = "AppRating.RateApplicationState";

		private readonly IMessageService _messageService;
		private readonly IDeviceRateApplicationService _deviceRateApplicationService;
		private readonly ILocalization _localization;
		private readonly IAppSettings _applicationSettings;
	    private readonly ICacheService _applicationCache;

        private RateApplicationState _ratingState;

		public RateApplicationService(
            IMessageService messageService,
            IDeviceRateApplicationService deviceRateApplicationService,
            ILocalization localization,
            IAppSettings applicationSettings,
            ICacheService cacheService)
		{
			_messageService = messageService;
			_deviceRateApplicationService = deviceRateApplicationService;
			_localization = localization;
			_applicationSettings = applicationSettings;
            _applicationCache = cacheService;

            LoadAppRatingState();
		}

		public void LoadAppRatingState()
		{
            var currentStateText = _applicationCache.Get<string>(RateApplicationStateKey);
			if (currentStateText.HasValue())
			{
                Enum.TryParse(currentStateText, out _ratingState);
			}
		}

        public bool CanShowRateApplicationDialog(int ordersAboveRatingThreshold)
		{
            // Modulo is because we want to show the rating pop-up every 'MinimumTripsForAppRating' trips
            return CanRateApp
                && (_applicationSettings.Data.MinimumTripsForAppRating == 0
                    || (ordersAboveRatingThreshold > 0
                        && ordersAboveRatingThreshold % _applicationSettings.Data.MinimumTripsForAppRating == 0));
		}

		public Task ShowRateApplicationDialog()
		{
			return _messageService.ShowMessage(_localization["RateMobileApplicationTitle"], _localization["RateMobileApplicationText"],
				_localization["RateMobileApplicationNow"], RedirectToRatingPage,
				_localization["RateMobileApplicationNever"], CancelRatingPopup,
				_localization["RateMobileApplicationLater"], PostponeRatingPopup);
		}

        private bool CanRateApp
        {
            get
            {
                return _ratingState == RateApplicationState.NotRated || _ratingState == RateApplicationState.Postponed;
            }
        }

		private void RedirectToRatingPage()
		{
            SaveRatingState(RateApplicationState.Rated);

			if (!_deviceRateApplicationService.RedirectToRatingPage())
			{
                Logger.LogMessage("Tried to show the 'Rate application pop-up' but the store URL was not set.");
                SaveRatingState(RateApplicationState.NotRated);

				_messageService.ShowMessage(_localization["LinkForApplicationStoreAbsentTitle"], _localization["LinkForApplicationStoreAbsentText"]);
			}
		}

		private void PostponeRatingPopup()
		{
            SaveRatingState(RateApplicationState.Postponed);
		}

		private void CancelRatingPopup()
		{
            SaveRatingState(RateApplicationState.NeverPrompt);
		}

	    private void SaveRatingState(RateApplicationState ratingState)
	    {
	        _ratingState = ratingState;
            _applicationCache.Set(RateApplicationStateKey, _ratingState.ToString());
	    }
	}
}