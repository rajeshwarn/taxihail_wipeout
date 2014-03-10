using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Client;
using System;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Reactive.Subjects;
using apcurium.MK.Common.Entity;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		private IOrderWorkflowService _orderWorkflowService;

		private CompositeDisposable _subscriptions;
		readonly ISubject<AvailableVehicle[]> _availableVehiclesSubject = new BehaviorSubject<AvailableVehicle[]>(new AvailableVehicle[0]);

		private bool IsStarted { get; set; }
		private Address AroundLocation { get; set; }

		protected void Observe<T>(IObservable<T> observable, Action<T> onNext)
		{
			observable
				.Subscribe(x => onNext(x))
				.DisposeWith(_subscriptions);
		}

		public void Start()
		{   
			if(IsStarted)
			{
				return;
			}

			_subscriptions = new CompositeDisposable ();
			_orderWorkflowService = TinyIoCContainer.Current.Resolve<IOrderWorkflowService>();
			this.Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => AroundLocation = address);

			IsStarted = true;

			ObserveAvailableVehicles();
		}

		private async void CheckForAvailableVehicles()
		{
			if (AroundLocation != null && AroundLocation.Latitude != 0 && AroundLocation.Longitude != 0) 
			{
				try 
				{
					var availableVehicles = await UseServiceClientAsync<IVehicleClient, AvailableVehicle[]> (service => 
						service.GetAvailableVehiclesAsync (AroundLocation.Latitude, AroundLocation.Longitude));
					_availableVehiclesSubject.OnNext (availableVehicles);
				} 
				catch 
				{
				}
			}
		}

		private async void ObserveAvailableVehicles()
		{
			Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds (5))
				.Subscribe (_ => CheckForAvailableVehicles())
				.DisposeWith (_subscriptions);
		}
		
		public void Stop ()
		{   
			if(IsStarted)
			{
				_subscriptions.Dispose();
				IsStarted = false;
			}
		}

		public IObservable<AvailableVehicle[]> GetAndObserveAvailableVehicles()
		{
			return _availableVehiclesSubject;
		}
    }
}