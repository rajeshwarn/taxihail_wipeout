using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.MapDataProvider.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IVehicleService
	{
		void Start();

		void Stop();

		IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles();

        IObservable<AvailableVehicle[]> GetAndObserveAvailableVehiclesWhenVehicleTypeChanges();

		MapBounds GetBoundsForNearestVehicles(bool isUsingGeoServices, Address pickup, IEnumerable<AvailableVehicle> cars);

		IObservable<Direction> GetAndObserveEta();

		IObservable<AvailableVehicle> GetAndObserveCurrentTaxiLocation(string medallion, Guid orderId);
		
		Task<GeoDataEta> GetVehiclePositionInfoFromGeo(double fromLat, double fromLng, string vehicleRegistration, Guid orderId);

		Task<Direction> GetEtaBetweenCoordinates (double fromLat, double fromLng, double toLat, double toLng);

		Task<bool> SendMessageToDriver(string message, string vehicleNumber, Guid orderId);

		void SetAvailableVehicle(bool enable);
	}
}
