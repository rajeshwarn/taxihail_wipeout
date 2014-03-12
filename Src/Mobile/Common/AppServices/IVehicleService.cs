using System;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IVehicleService
	{
		void Start();
		void Stop();
		IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles();
	}
}

