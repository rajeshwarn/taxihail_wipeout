using System;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Disposables;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.ComponentModel;
using apcurium.MK.Booking.Mobile.Data;

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

            this.Observe(_orderWorkflowService.GetAndObserveAddressSelectionMode(), addressSelectionMode => AddressSelectionMode = addressSelectionMode);
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

        private MapBounds _userMapBounds;
        public MapBounds UserMapBounds
        {
            get
            {
                 return _userMapBounds;
            }

            set
            {            
                _userMapBounds = value;
                DeltaLongitude = Math.Abs((UserMapBounds.EastBound - UserMapBounds.WestBound) / 2);
                DeltaLatitude = Math.Abs((UserMapBounds.NorthBound - UserMapBounds.SouthBound) / 2);
                _orderWorkflowService.SetAddressToCoordinate(new Position() { Latitude = UserMapBounds.GetCenter().Latitude, Longitude =  UserMapBounds.GetCenter().Longitude });
            }
        }

        public AddressSelectionMode AddressSelectionMode { get; set;}

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

        [DefaultValue(0.002d)]
        public double DeltaLatitude { get; set; }

        [DefaultValue(0.002d)]
        public double DeltaLongitude { get; set; }

        public double DefaultDelta = 0.002d;

		private void OnPickupAddressChanged()
		{			
            if (PickupAddress.HasValidCoordinate())
			{
				MapBounds = new MapBounds
				{
                    NorthBound = PickupAddress.Latitude + DeltaLatitude,
                    SouthBound = PickupAddress.Latitude - DeltaLatitude,
                    EastBound = PickupAddress.Longitude + DeltaLongitude,
                    WestBound = PickupAddress.Longitude - DeltaLongitude,
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

