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
		readonly ISubject<bool> _sessionStatus = new ReplaySubject<bool>(1);

		public abstract void Connect(string permissions);
		
        public abstract Task<FacebookUserInfo> GetUserInfo();

		public IObservable<bool> GetAndObserveSessionStatus()
		{
			return _sessionStatus;
		}

        protected IObserver<bool> SessionStatusObserver
		{
			get
			{
				return _sessionStatus;
			}
		}

    }
}

