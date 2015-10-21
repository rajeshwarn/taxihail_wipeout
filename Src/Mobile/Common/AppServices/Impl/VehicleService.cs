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
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.ServiceClient.Web;
using System.Net;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		private readonly IConnectableObservable<AvailableVehicle[]> _availableVehiclesObservable;
        private readonly IObservable<AvailableVehicle[]> _availableVehiclesWhenTypeChangesObservable;
		private readonly IObservable<Direction> _etaObservable;
		private readonly ISubject<IObservable<long>> _timerSubject = new BehaviorSubject<IObservable<long>>(Observable.Never<long>());
	    private readonly IObservable<bool> _isUsingGeoServicesObservable; 
		private readonly ISubject<bool> _availableVehicleEnabled = new BehaviorSubject<bool>(true);
		

		private readonly IDirections _directions;
		private readonly IAppSettings _settings;

	    private bool _isStarted;

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
                .CombineLatest(
					orderWorkflowService.GetAndObservePickupAddress(),
					_availableVehicleEnabled.DistinctUntilChanged(), 
					(_, address, enableAvailableVehicle) => new { address, enableAvailableVehicle}
				)
                .Where(x => x.enableAvailableVehicle && x.address.HasValidCoordinate())
				.Select(x => x.address)
                .SelectMany(async x => 
				{
                    var vehicleTypeId = await orderWorkflowService.GetAndObserveVehicleType()
						.Take(1)
						.ToTask()
						.ConfigureAwait(false);

                    return await CheckForAvailableVehicles(x, vehicleTypeId).ConfigureAwait(false);
                })
				.Publish();
			_availableVehiclesObservable.Connect ();

            _availableVehiclesWhenTypeChangesObservable = orderWorkflowService.GetAndObserveVehicleType()
                .SelectMany(async vehicleTypeId => 
                    { 
                        var address = await orderWorkflowService.GetAndObservePickupAddress()
							.Take(1)
							.ToTask()
							.ConfigureAwait(false);

                        return new { vehicleTypeId, address };
                    })
                .Where(x => x.address.HasValidCoordinate())
                .SelectMany(x => CheckForAvailableVehicles(x.address, x.vehicleTypeId));

			_isUsingGeoServicesObservable = orderWorkflowService.GetAndObserveIsUsingGeo();

            _etaObservable = _availableVehiclesObservable
				.Where (_ => _settings.Data.ShowEta)
				.CombineLatest(
                    _isUsingGeoServicesObservable,
                    orderWorkflowService.GetAndObservePickupAddress (), 
                    (vehicles, isUsingGeoServices, address) => new { address, isUsingGeoServices, vehicles } 
                )
				.Select (x => new { x.address, x.isUsingGeoServices, vehicle =  GetNearestVehicle(x.isUsingGeoServices, x.address, x.vehicles) })
				.DistinctUntilChanged(x =>
				{
				    if (x.isUsingGeoServices && x.vehicle != null)
				    {
				        return x.vehicle.Eta ?? double.MaxValue;
				    }

				    return x.vehicle == null
				        ? double.MaxValue
				        : Position.CalculateDistance(x.vehicle.Latitude, x.vehicle.Longitude, x.address.Latitude, x.address.Longitude);
				})
				.SelectMany(x => CheckForEta(x.isUsingGeoServices, x.address, x.vehicle));
		}

		public void SetAvailableVehicle(bool enable)
		{
			_availableVehicleEnabled.OnNext(enable);
		}


		public void Start()
		{   
			if(_isStarted)
			{
				return;
			}

			_isStarted = true;

		    var refreshRate = _settings.Data.AvailableVehicleRefreshRate > 0
		        ? _settings.Data.AvailableVehicleRefreshRate
		        : 1;

            _timerSubject.OnNext(Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(refreshRate)));
		}

		private async Task<AvailableVehicle[]> CheckForAvailableVehicles(Address address, int? vehicleTypeId)
		{
			try
			{
				return await UseServiceClientAsync<IVehicleClient, AvailableVehicle[]>(service => 
					service.GetAvailableVehiclesAsync(address.Latitude, address.Longitude, vehicleTypeId),
					ex =>
					{
						// Do not use the default event handler because we do not want to show the
						// connection error message for GAV requests
						Logger.LogMessage("Error while trying to get available vehicles");
						Logger.LogError(ex);
					})
					.ConfigureAwait(false);
			}
			catch (Exception e)
			{
				Logger.LogError(e);
				return new AvailableVehicle[0];
			}
		}

		public AvailableVehicle GetNearestVehicle(bool isUsingGeoService, Address pickup, AvailableVehicle[] cars)
		{
			if (cars == null || !cars.Any ())
            {
				return null;
			}

			return OrderVehiclesByDistanceIfNeeded (isUsingGeoService, pickup, cars).First();
		}

		public MapBounds GetBoundsForNearestVehicles(bool isUsingGeoServices, Address pickup, IEnumerable<AvailableVehicle> cars)
		{
			if (cars == null || !cars.Any() || !_settings.Data.ZoomOnNearbyVehicles) 
			{
				return null;
			}

			var radius = _settings.Data.ZoomOnNearbyVehiclesRadius;
			var vehicleCount = _settings.Data.ZoomOnNearbyVehiclesCount;

			var centerLatitude = pickup.Latitude;
			var centerLongitude = pickup.Longitude;

			var vehicles = OrderVehiclesByDistanceIfNeeded (isUsingGeoServices, pickup, cars)
				.Where (car => Position.CalculateDistance (car.Latitude, car.Longitude, centerLatitude, centerLongitude) <= radius)
				.Take (vehicleCount)
				.ToArray();

			if (!vehicles.Any())
			{
				return null;
			}

			var distanceFromLastVehicle = Position.CalculateDistance (vehicles.Last ().Latitude, vehicles.Last ().Longitude, centerLatitude, centerLongitude); 

			var maximumBounds = MapBounds.GetBoundsFromCenterAndRadius(centerLatitude, centerLongitude, distanceFromLastVehicle, distanceFromLastVehicle);

			return maximumBounds;
		}

		private IEnumerable<AvailableVehicle> OrderVehiclesByDistanceIfNeeded(bool isUsingGeoServices, Address pickup, IEnumerable<AvailableVehicle> cars)
		{
		    return isUsingGeoServices
                // Ensure that the cars are ordered correctly.
                ? cars.OrderBy(car => car.Eta.HasValue ? 0 : 1).ThenBy(car => car.Eta).ThenBy(car  => car.VehicleName)
                : cars.OrderBy (car => Position.CalculateDistance (car.Latitude, car.Longitude, pickup.Latitude, pickup.Longitude));
		}

	    private async Task<Direction> CheckForEta(bool isUsingCmtGeo, Address pickup, AvailableVehicle vehicleLocation)
		{
			if(vehicleLocation == null)
			{
                return new Direction();
			}

		    var etaBetweenCoordinates = await GetEtaBetweenCoordinates(vehicleLocation.Latitude, vehicleLocation.Longitude, pickup.Latitude, pickup.Longitude);

		    if (isUsingCmtGeo)
		    {
                // Needs value in minutes not in seconds.
                etaBetweenCoordinates.Duration = vehicleLocation.Eta / 60;
		    }

		    return etaBetweenCoordinates;
		}

		
		public async Task<GeoDataEta> GetVehiclePositionInfoFromGeo(double fromLat, double fromLng, string vehicleRegistration, Guid orderId)
	    {
			try
			{
				var etaFromGeo = await UseServiceClientAsync<IVehicleClient, EtaForPickupResponse>(service => service.GetEtaFromGeo(fromLat, fromLng, vehicleRegistration, orderId));

				return new GeoDataEta
				{
					Eta = etaFromGeo.Eta / 60,
					Latitude = etaFromGeo.Latitude,
					Longitude = etaFromGeo.Longitude,
					CompassCourse = etaFromGeo.CompassCourse,
                Market = etaFromGeo.Market
				};
			}
			catch(WebServiceException ex)
			{
				Logger.LogMessage("An error occurred while obtaining vehicle: {0} from Geo in order {1}",vehicleRegistration??"Unknown vehicle", orderId);
				Logger.LogError(ex);

				return null;
			}
            
	    }

		private async Task<AvailableVehicle> GetVehiclePositionFromGeo(Guid orderId, string medallion)
		{
			try
			{
				return await UseServiceClientAsync<IVehicleClient, AvailableVehicle>(service => service.GetTaxiLocation(orderId, medallion));
			}
			catch (Exception ex)
			{
				Logger.LogError(ex);
				return null;
			}
		}


		public Task<Direction> GetEtaBetweenCoordinates(double fromLat, double fromLng, double toLat, double toLng)
		{
			return _directions.GetDirectionAsync(fromLat, fromLng, toLat, toLng, null, null, true);  
		}

	    public async Task<bool> SendMessageToDriver(string message, string vehicleNumber, Guid orderId)
	    {
            try
            {
                await UseServiceClientAsync<IVehicleClient>(service => service.SendMessageToDriver(message, vehicleNumber, orderId))
                    .ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                Logger.LogMessage("Error when sending message to driver");
                Logger.LogError(e);
                return false;
            }
	    }

		public void Stop ()
		{   
			if(_isStarted)
			{
				_timerSubject.OnNext(Observable.Never<long>());
				_isStarted = false;
			}
		}

		public IObservable<AvailableVehicle> GetAndObserveCurrentTaxiLocation(string medallion, Guid orderId)
		{
			return _timerSubject.Switch()
				.SelectMany(_ => _availableVehicleEnabled.DistinctUntilChanged())
				.Where(enabled => enabled)
				.SelectMany(_ => GetVehiclePositionFromGeo(orderId, medallion))
				.Where(vehicle => vehicle != null);
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

    public class GeoDataEta
    {
        public long? Eta { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public double? CompassCourse { get; set; }
        
        public string Market { get; set; }

		public bool IsPositionValid
		{
			get
			{
				return Latitude.HasValue
					&& Longitude.HasValue
					&& Latitude.Value != 0
					&& Longitude.Value != 0;
			}
		}
    }
}