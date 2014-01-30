using System;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Disposables;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class MapViewModel: MvxNavigatingObject
    {
		readonly IOrderWorkflowService _orderWorkflowService;
		readonly CompositeDisposable _subscriptions = new CompositeDisposable();
		public MapViewModel(IOrderWorkflowService orderWorkflowService)
        {
			_orderWorkflowService = orderWorkflowService;

			this.Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address);
        }


		private Address _pickupAddress;
		public Address PickupAddress
		{
			get { return _pickupAddress; }
			set
			{
				if (value != _pickupAddress)
				{
					_pickupAddress = value;
					RaisePropertyChanged("PickupAddress");
				}
			}
		}

		private void Observe<T>(IObservable<T> observable, Action<T> onNext)
		{
			observable
				.Subscribe(x => InvokeOnMainThread(() => onNext(x)))
				.DisposeWith(_subscriptions);
		}
    }
}

