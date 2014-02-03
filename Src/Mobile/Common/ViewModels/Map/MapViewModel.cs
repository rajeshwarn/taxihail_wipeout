using System;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Disposables;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class MapViewModel: ChildViewModel
    {
		readonly IOrderWorkflowService _orderWorkflowService;
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
					OnPickupAddressChanged();
				}
			}
		}

		private MapBounds _mapBounds;
		public MapBounds MapBounds
		{
			get { return _mapBounds; }
			set
			{
				if (value != _mapBounds)
				{
					_mapBounds = value;
					RaisePropertyChanged("MapBounds");
				}
			}
		}

		private void OnPickupAddressChanged()
		{
			var deltaLat = 0.002;
			var deltaLng = 0.002;

			if (PickupAddress.HasValidCoordinate())
			{
				MapBounds = new MapBounds
				{
					NorthBound = PickupAddress.Latitude + deltaLat,
					SouthBound = PickupAddress.Latitude - deltaLat,
					EastBound = PickupAddress.Longitude - deltaLng,
					WestBound = PickupAddress.Longitude + deltaLng,
				};
			}
		}
    }
}

