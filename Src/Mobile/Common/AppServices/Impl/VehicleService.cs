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

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		readonly IObservable<AvailableVehicle[]> _availableVehiclesObservable;
		readonly IObservable<Direction> _eta;
		readonly ISubject<IObservable<long>> _timerSubject = new BehaviorSubject<IObservable<long>>(Observable.Never<long>());
		readonly IDirections _directions;

		private bool _isStarted { get; set; }

		public VehicleService(IOrderWorkflowService orderWorkflowService,
			IDirections directions)
		{
			_directions = directions;
			_availableVehiclesObservable = _timerSubject
				.Switch()
				.CombineLatest(
					orderWorkflowService.GetAndObservePickupAddress(), 
					orderWorkflowService.GetAndObserveVehicleType(), 
					(_, address, vehicleTypeId) => new { address, vehicleTypeId })
				.Where(x => x.address.HasValidCoordinate() && x.vehicleTypeId.HasValue)
				.SelectMany(x => CheckForAvailableVehicles(x.address, x.vehicleTypeId.Value));

			_eta = _timerSubject
				.Switch ()
				.CombineLatest (
				orderWorkflowService.GetAndObservePickupAddress (), 
				_availableVehiclesObservable, 
				(_, address, availableVehicles) => new { address, availableVehicles })
				.Where (x => x.address.HasValidCoordinate () )
				.Select (x => CheckForEta (x.address, x.availableVehicles));
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

		public Direction CheckForEta(Address pickup, AvailableVehicle[] availableVehicles) // Using pickupDate here to be consistant with date I18n
		{
			var cars = availableVehicles.ToList();	
			var vehicleLocation = cars.OrderByDescending (car => Position.CalculateDistance (car.Latitude, car.Longitude, pickup.Latitude, pickup.Longitude))
				.First();
			
			return  _directions.GetDirection(vehicleLocation.Latitude, vehicleLocation.Longitude, pickup.Latitude, pickup.Longitude, null, true);                    	
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
			return _eta;
		}
    }
}