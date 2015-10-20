using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public class RateApplicationServiceSettings
	{
		public static string Name = "RateApplicationServiceSettings";

		static ICacheService _cacheService;

		public RateApplicationState RateApplicationState { get; set; }

		public DateTime StateTime { get; set; }

		public ApplicationVersion ApplicationVersion { get; set; }

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
				RateApplicationState = RateApplicationState.NotRated;
				StateTime = DateTime.Now;
				ApplicationVersion = new ApplicationVersion(null);
			}
		}

		public void SetState(RateApplicationState rateApplicationState, DateTime stateTime, ApplicationVersion applicationVersion)
		{
			RateApplicationState = rateApplicationState;

			StateTime = stateTime;
			ApplicationVersion = applicationVersion;

			_cacheService.Set<RateApplicationServiceSettings>(RateApplicationServiceSettings.Name, this);
		}
	}
}