using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IVehicleService
	{
		void Start();
		void Stop();
		IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles();
		MapBounds GetBoundsForNearestVehicles(Address pickup, IEnumerable<AvailableVehicle> cars);
		IObservable<Direction> GetAndObserveEta();
		Direction GetEtaBetweenCoordinates (double fromLat, double fromLng, double toLat, double toLng);
	}
}

