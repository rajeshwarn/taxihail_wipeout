using apcurium.MK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public enum RateApplicationState
	{
		NotRated,
		Postponed,
		Rated,
		Cancelled
	}

	public class RateApplicationServiceSettings
	{
		public static string Name = "RateApplicationServiceSettings";

		static ICacheService _cacheService;

		public RateApplicationServiceSettings()
		{
			if (_cacheService == null)
			{
				_cacheService = (ICacheService)TinyIoC.TinyIoCContainer.Current.Resolve(typeof(ICacheService), "UserAppCache");
			}
		}

		public void LoadCurrentSettings()
		{
			RateApplicationServiceSettings currentState = _cacheService.Get<RateApplicationServiceSettings>(RateApplicationServiceSettings.Name);

			if (currentState != null)
			{
				RateApplicationState = currentState.RateApplicationState;
				StateTime = currentState.StateTime;
				ApplicationVersion = currentState.ApplicationVersion;
			}
			else
			{
				RateApplicationState = Infrastructure.RateApplicationState.NotRated;
				StateTime = DateTime.Now;
				ApplicationVersion = new ApplicationVersion(null);
			}
		}

		public RateApplicationState RateApplicationState { get; set; }

		public DateTime StateTime { get; set; }

		public ApplicationVersion ApplicationVersion { get; set; }

		public void SetState(RateApplicationState rateApplicationState, DateTime stateTime, ApplicationVersion applicationVersion)
		{
			RateApplicationState = rateApplicationState;
			StateTime = stateTime;
			ApplicationVersion = applicationVersion;

			_cacheService.Set<RateApplicationServiceSettings>(RateApplicationServiceSettings.Name, this);
		}
	}

	public class RateApplicationService:IRateApplicationService
	{
		private IMessageService _messageService;
		private IDeviceRateApplicationService _deviceRateApplicationService;
		private RateApplicationServiceSettings _currentRatingState;
		private ApplicationVersion _currentApplicationVersion;

		public RateApplicationService(IMessageService messageService, IDeviceRateApplicationService deviceRateApplicationService, IPackageInfo packageInfo)
		{
			_messageService = messageService;
			_deviceRateApplicationService = deviceRateApplicationService;
			_currentRatingState = new RateApplicationServiceSettings();
			_currentRatingState.LoadCurrentSettings();
			_currentApplicationVersion = new ApplicationVersion(packageInfo.Version);
		}

		public bool IsShowRateApplicationDialog()
		{
			bool result = false;

			if (_currentRatingState.RateApplicationState == RateApplicationState.NotRated
				|| (_currentRatingState.RateApplicationState == RateApplicationState.Postponed && _currentRatingState.StateTime >= DateTime.Now.AddDays(1))
				|| _currentRatingState.ApplicationVersion < _currentApplicationVersion)
			{
				result = true;
			}

			return result;
		}

		public void ShowRateApplicationSuggestDialog()
		{
			_messageService.ShowMessage("Rate this application",
				"If you enjoy using this application would you mind taking a moment to rate it? Thanks for your support!", "Rate It Now", RedirectToRatingPage, "Remind Me Later", PostponeRatingPopup, "No, Thanks", CancelRatingPopup);
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