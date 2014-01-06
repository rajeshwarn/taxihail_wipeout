using System;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Subjects;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public abstract class FacebookServiceBase: IFacebookService
    {
		private ISubject<bool> _sessionStatus = new ReplaySubject<bool>(1);
		public abstract void Connect(string permissions);

		public abstract Task<FacebookUserInfo> GetUserInfo(string accessToken);
		/*public async Task<FacebookUserInfo> GetUserInfo(string accessToken)
		{
			var fb = new FacebookClient(accessToken);
			var me = fb.GetTaskAsync("me");
			return FacebookUserInfo.CreateFrom((IDictionary<string, object>) await me);
		}*/

		public IObservable<bool> GetAndObserveSessionStatus()
		{
			return _sessionStatus;
		}

        protected IObserver<bool> SessionStatusSubject
		{
			get
			{
				return _sessionStatus;
			}
		}

    }
}

