using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Client;
using System;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Reactive.Subjects;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		// TODO use the pickup address in the orderWorkflowService instead
		private ILocationService _locationService;

		readonly ISubject<AvailableVehicle[]> _availableVehiclesSubject = new BehaviorSubject<AvailableVehicle[]>(new AvailableVehicle[0]);

		bool _isStarted;
		public bool IsStarted {
			get {
				return _isStarted;
			}
		}

		public void Start()
		{   
			if(_isStarted)
			{
				return;
			}

			_locationService = TinyIoCContainer.Current.Resolve<ILocationService>();

			_isStarted = true;

			ObserveAvailableVehicles();
		}

		private async void ObserveAvailableVehicles()
		{
			do
			{
				var lastKnownPosition = _locationService.LastKnownPosition;
				if(lastKnownPosition != null)
				{
					try{
					var availableVehicles = await UseServiceClientAsync<IVehicleClient, AvailableVehicle[]>(service => service
						GetAvailableVehiclesAsync(lastKnownPosition.Latitude, lastKnownPosition.Longitude));
						_availableVehiclesSubject.OnNext(availableVehicles);
					}
					catch{}

				}

				await Task.Delay(5000);
			}
			while(_isStarted);
		}
		
		public void Stop ()
		{   
			if(_isStarted)
			{
				_isStarted = false;
			}
		}

		public IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles()
		{
			return _availableVehiclesSubject;
		}

		[Obsolete("Old method for legacy purpose")]
		public Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude)
        {
			return UseServiceClientAsync<IVehicleClient, AvailableVehicle[]>(service => service
				.GetAvailableVehiclesAsync( latitude, longitude));
        }
    }
}