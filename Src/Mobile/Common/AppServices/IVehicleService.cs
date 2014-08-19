using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IVehicleService
	{
		void Start();
		void Stop();
		IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles();
		IObservable<Direction> GetAndObserveEta();
		Direction GetEtaBetweenCoordinates (double fromLat, double fromLng, double toLat, double toLng);
	}
}

