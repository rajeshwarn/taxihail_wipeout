using System;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Disposables;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class MapViewModel: ChildViewModel
    {
		readonly IOrderWorkflowService _orderWorkflowService;
		readonly IVehicleService _vehicleService;

		public MapViewModel(IOrderWorkflowService orderWorkflowService, IVehicleService vehicleService)
        {
			_orderWorkflowService = orderWorkflowService;
			_vehicleService = vehicleService;

			this.Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address);
			this.Observe(_orderWorkflowService.GetAndObserveDestinationAddress(), address => DestinationAddress = address);
			this.Observe(_vehicleService.GetAndObserveAvailableVehicles(), availableVehicles => AvailableVehicles = availableVehicles);
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
					RaisePropertyChanged();
					OnPickupAddressChanged();
				}
			}
		}

		private Address _destinationAddress;
		public Address DestinationAddress
		{
			get { return _destinationAddress; }
			set
			{
				if (value != _destinationAddress)
				{
					_destinationAddress = value;
					RaisePropertyChanged();
					OnDestinationAddressChanged();
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
					RaisePropertyChanged();
				}
			}
		}

		private IEnumerable<AvailableVehicle> _availableVehicles = new List<AvailableVehicle>();
		public IEnumerable<AvailableVehicle> AvailableVehicles
		{
			get{ return _availableVehicles; }
			set
			{ 
				_availableVehicles = value;
				RaisePropertyChanged();
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
					EastBound = PickupAddress.Longitude + deltaLng,
					WestBound = PickupAddress.Longitude - deltaLng,
				};
			}
		}

		private void OnDestinationAddressChanged()
		{
			var deltaLat = 0.002;
			var deltaLng = 0.002;

			if (DestinationAddress.HasValidCoordinate())
			{
				MapBounds = new MapBounds
				{
					NorthBound = DestinationAddress.Latitude + deltaLat,
					SouthBound = DestinationAddress.Latitude - deltaLat,
					EastBound = DestinationAddress.Longitude + deltaLng,
					WestBound = DestinationAddress.Longitude - deltaLng,
				};
			}
		}
    }
}

