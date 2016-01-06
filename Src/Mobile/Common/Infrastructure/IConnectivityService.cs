using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IConnectivityService
	{
        IObservable<bool> GetAndObserveIsConnected();
	}
}