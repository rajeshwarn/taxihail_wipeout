using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		readonly IObservable<AvailableVehicle[]> _availableVehiclesObservable;
		readonly ISubject<IObservable<long>> _timerSubject = new BehaviorSubject<IObservable<long>>(Observable.Never<long>());

		private bool _isStarted { get; set; }

		public VehicleService(IOrderWorkflowService orderWorkflowService)
		{
			_availableVehiclesObservable = _timerSubject
				.Switch()
				.CombineLatest(orderWorkflowService.GetAndObservePickupAddress(), (_, address) => address)
				.Where(a => a.HasValidCoordinate())
				.SelectMany(a => CheckForAvailableVehicles(a));

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

		private async Task<AvailableVehicle[]> CheckForAvailableVehicles(Address address)
		{
			try
			{
				return await UseServiceClientAsync<IVehicleClient, AvailableVehicle[]>(service => 
                    service.GetAvailableVehiclesAsync(address.Latitude, address.Longitude))
						.ConfigureAwait(false);
			}
			catch (Exception e)
			{
				Logger.LogError(e);
				return new AvailableVehicle[0];
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

		public IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles()
		{
			return _availableVehiclesObservable;
		}
    }
}