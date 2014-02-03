using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using System;


namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IVehicleService
	{
		Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude);

		void Start();
		void Stop();
		IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles();
	}
}

