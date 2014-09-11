using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		readonly IObservable<AvailableVehicle[]> _availableVehiclesObservable;
		readonly IObservable<Direction> _etaObservable;
		readonly ISubject<IObservable<long>> _timerSubject = new BehaviorSubject<IObservable<long>>(Observable.Never<long>());
		readonly IDirections _directions;
		readonly IAppSettings _settings;

		private bool _isStarted { get; set; }

		public VehicleService(IOrderWorkflowService orderWorkflowService,
			IDirections directions,
			IAppSettings settings)
		{
			_directions = directions;
			_settings = settings;

			_availableVehiclesObservable = _timerSubject
				.Switch()
				.CombineLatest(
					orderWorkflowService.GetAndObservePickupAddress(), 
					orderWorkflowService.GetAndObserveVehicleType(), 
					(_, address, vehicleTypeId) => new { address, vehicleTypeId })
				.Where(x => x.address.HasValidCoordinate() && x.vehicleTypeId.HasValue)
				.SelectMany(x => CheckForAvailableVehicles(x.address, x.vehicleTypeId.Value));

			_etaObservable = _availableVehiclesObservable
				.Where (_ => _settings.Data.ShowEta)
				.CombineLatest(orderWorkflowService.GetAndObservePickupAddress (), (vehicles, address) => new { address, vehicles } )
				.Select (x => new { x.address, vehicle =  GetNearestVehicle(x.address, x.vehicles) })
				.DistinctUntilChanged(x => x.vehicle == null ? double.MaxValue : Position.CalculateDistance (x.vehicle.Latitude, x.vehicle.Longitude, x.address.Latitude, x.address.Longitude))
				.Select(x => CheckForEta(x.address, x.vehicle));
		}

		public void Start()
		{   
			if(_isStarted)
			{
				return;
			}

			_isStarted = true;

			_timerSubject.OnNext(Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds (5)));
		}

		private async Task<AvailableVehicle[]> CheckForAvailableVehicles(Address address, int vehicleTypeId)
		{
			try
			{
				return await UseServiceClientAsync<IVehicleClient, AvailableVehicle[]>(service => 
					service.GetAvailableVehiclesAsync(address.Latitude, address.Longitude, vehicleTypeId))
						.ConfigureAwait(false);
			}
			catch (Exception e)
			{
				Logger.LogError(e);
				return new AvailableVehicle[0];
			}
		}

		public AvailableVehicle GetNearestVehicle(Address pickup, AvailableVehicle[] cars)
		{
			if ((cars == null) || (!cars.Any ())) {
				return null;
			}

			return OrderVehiclesByDistance (pickup, cars).First ();
		}

		public MapBounds GetBoundsForNearestVehicles(Address pickup, IEnumerable<AvailableVehicle> cars)
		{
			if ((cars == null) || (!cars.Any ()) || !_settings.Data.ZoomOnNearbyVehicles) 
			{
				return null;
			}

			var radius = _settings.Data.ZoomOnNearbyVehiclesRadius;
			var vehicleCount = _settings.Data.ZoomOnNearbyVehiclesCount;

			var vehicles = OrderVehiclesByDistance (pickup, cars)
				.Where (car => Position.CalculateDistance (car.Latitude, car.Longitude, pickup.Latitude, pickup.Longitude) <= radius)
				.Take (vehicleCount)
				.ToArray();

			var south = vehicles.OrderBy (x => x.Latitude).First ().Latitude;
			var north = vehicles.OrderBy (x => x.Latitude).Last ().Latitude;
			var west = vehicles.OrderBy (x => x.Longitude).First ().Longitude;
			var east = vehicles.OrderBy (x => x.Longitude).Last ().Longitude;

			return new MapBounds () { NorthBound = north, SouthBound = south, EastBound = east, WestBound = west };
		}

		IEnumerable<AvailableVehicle> OrderVehiclesByDistance(Address pickup, IEnumerable<AvailableVehicle> cars)
		{
			return cars
				.OrderBy (car => Position.CalculateDistance (car.Latitude, car.Longitude, pickup.Latitude, pickup.Longitude))
				.ToArray();
		}

		public Direction CheckForEta(Address pickup, AvailableVehicle vehicleLocation)
		{
			if(vehicleLocation == null)
			{
				return null;
			}

			return  GetEtaBetweenCoordinates(vehicleLocation.Latitude, vehicleLocation.Longitude, pickup.Latitude, pickup.Longitude);                    	
		}

		public Direction GetEtaBetweenCoordinates(double fromLat, double fromLng, double toLat, double toLng)
		{
			return  _directions.GetDirection(fromLat, fromLng, toLat, toLng, null, null, true);  
		}

		public void Stop ()
		{   
			if(_isStarted)
			{
				_timerSubject.OnNext(Observable.Never<long>());
				_isStarted = false;
			}
		}

		public IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles()
		{
			return _availableVehiclesObservable;
		}

		public IObservable<Direction> GetAndObserveEta()
		{
			return _etaObservable;
		}
    }
}