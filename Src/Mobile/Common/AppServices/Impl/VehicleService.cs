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
using System.Reactive.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		readonly IConnectableObservable<AvailableVehicle[]> _availableVehiclesObservable;
        readonly IObservable<AvailableVehicle[]> _availableVehiclesWhenTypeChangesObservable;
		readonly IObservable<Direction> _etaObservable;
		readonly ISubject<IObservable<long>> _timerSubject = new BehaviorSubject<IObservable<long>>(Observable.Never<long>());
		readonly IDirections _directions;
		readonly IAppSettings _settings;

		private IDisposable _marketDisposable;

	    private bool _isStarted;
	    private string _market;

		public VehicleService(IOrderWorkflowService orderWorkflowService,
			IDirections directions,
			IAppSettings settings)
		{
			_directions = directions;
			_settings = settings;

			// having publish and connect fixes the problem that caused the code to be executed 2 times
			// because there was 2 subscriptions
            _availableVehiclesObservable = _timerSubject
                .Switch()
                .CombineLatest(orderWorkflowService.GetAndObservePickupAddress(), (_, address) => address)
                .Where(x => x.HasValidCoordinate())
                .SelectMany(async x => {
                    var vehicleTypeId = await orderWorkflowService.GetAndObserveVehicleType().Take(1).ToTask();
                    return await CheckForAvailableVehicles(x, vehicleTypeId);
                })
				.Publish();
			_availableVehiclesObservable.Connect ();

            _availableVehiclesWhenTypeChangesObservable = orderWorkflowService.GetAndObserveVehicleType()
                .SelectMany(async vehicleTypeId => 
                    { 
                        var address = await orderWorkflowService.GetAndObservePickupAddress().Take(1).ToTask();
                        return new { vehicleTypeId, address };
                    })
                .Where(x => x.address.HasValidCoordinate())
                .SelectMany(x => CheckForAvailableVehicles(x.address, x.vehicleTypeId));

			_etaObservable = _availableVehiclesObservable
				.Where (_ => _settings.Data.ShowEta)
				.CombineLatest(orderWorkflowService.GetAndObservePickupAddress (), (vehicles, address) => new { address, vehicles } )
				.Select (x => new { x.address, vehicle =  GetNearestVehicle(x.address, x.vehicles) })
				.DistinctUntilChanged(x => x.vehicle == null ? double.MaxValue : Position.CalculateDistance (x.vehicle.Latitude, x.vehicle.Longitude, x.address.Latitude, x.address.Longitude))
				.Select(x => CheckForEta(x.address, x.vehicle));

			_marketDisposable = orderWorkflowService.GetAndObserveMarket().Subscribe(market => _market = market);
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

		private async Task<AvailableVehicle[]> CheckForAvailableVehicles(Address address, int? vehicleTypeId)
		{
			try
			{
				return await UseServiceClientAsync<IVehicleClient, AvailableVehicle[]>(service => 
					service.GetAvailableVehiclesAsync(address.Latitude, address.Longitude, vehicleTypeId, _market))
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

			var centerLatitude = pickup.Latitude;
			var centerLongitude = pickup.Longitude;

			var vehicles = OrderVehiclesByDistance (pickup, cars)
				.Where (car => Position.CalculateDistance (car.Latitude, car.Longitude, centerLatitude, centerLongitude) <= radius)
				.Take (vehicleCount)
				.ToArray();

			if (vehicles.Count() == 0) {
				return null;
			}


			var distanceFromLastVehicle = Position.CalculateDistance (vehicles.Last ().Latitude, vehicles.Last ().Longitude, centerLatitude, centerLongitude); 

			var maximumBounds = MapBounds.GetBoundsFromCenterAndRadius(centerLatitude, centerLongitude, distanceFromLastVehicle, distanceFromLastVehicle);

			return maximumBounds;
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
				_marketDisposable.Dispose ();
			}
		}

		public IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles()
		{
            return _availableVehiclesObservable.Merge (_availableVehiclesWhenTypeChangesObservable);
		}

        public IObservable<AvailableVehicle[]> GetAndObserveAvailableVehiclesWhenVehicleTypeChanges()
        {
            return _availableVehiclesWhenTypeChangesObservable;
        }

		public IObservable<Direction> GetAndObserveEta()
		{
			return _etaObservable;
		}
    }
}