using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Common.Entity;
using System.Linq;
using MapBounds = apcurium.MK.Booking.Maps.Geo.MapBounds;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class MapViewModel: BaseViewModel
    {
		// seconds
		static readonly int TimeToKeepVehiclesOnMapWhenResultNull = 10;
		DateTime? KeepVehiclesWhenResultNullStartTime = null;
       
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IVehicleService _vehicleService;

		public MapViewModel(IOrderWorkflowService orderWorkflowService, IVehicleService vehicleService)
        {
			_orderWorkflowService = orderWorkflowService;
			_vehicleService = vehicleService;

			Observe(_orderWorkflowService.GetAndObserveIsDestinationModeOpened(), isDestinationModeOpened => IsDestinationModeOpened = isDestinationModeOpened);
            Observe(_orderWorkflowService.GetAndObserveAddressSelectionMode(), addressSelectionMode => AddressSelectionMode = addressSelectionMode);
            Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address);
			Observe(_orderWorkflowService.GetAndObserveDestinationAddress(), address => DestinationAddress = address);
			Observe(_vehicleService.GetAndObserveAvailableVehicles(), availableVehicles => AvailableVehicles = availableVehicles);
        }

        public static int ZoomStreetLevel = 14;

        private Address _pickupAddress;
		public Address PickupAddress
        {
            get { return _pickupAddress; }
            set
            {			
				_pickupAddress = value;					
				RaisePropertyChanged();	
            }
        }

		private Address _destinationAddress;
		public Address DestinationAddress
		{
			get { return _destinationAddress; }
			set
			{
				_destinationAddress = value;
				RaisePropertyChanged();
			}
		}

		private AddressSelectionMode _addressSelectionMode; 
		public AddressSelectionMode AddressSelectionMode
		{ 
			get { return _addressSelectionMode; }
			set
			{
				_addressSelectionMode = value;

				if (AddressSelectionMode == AddressSelectionMode.PickupSelection && PickupAddress.HasValidCoordinate())
				{					
					ChangePresentation(new CenterMapPresentationHint(PickupAddress.Latitude, PickupAddress.Longitude));
				}
				else if (AddressSelectionMode == AddressSelectionMode.DropoffSelection && DestinationAddress.HasValidCoordinate ())
				{
					ChangePresentation(new CenterMapPresentationHint(DestinationAddress.Latitude, DestinationAddress.Longitude));
				}

				RaisePropertyChanged();
			}
		}

		private bool _isDestinationModeOpened;
		public bool IsDestinationModeOpened
		{
			get { return _isDestinationModeOpened; }
			set
			{
				_isDestinationModeOpened = value;
				RaisePropertyChanged();
			}
		}

        private IList<AvailableVehicle> _availableVehicles = new List<AvailableVehicle>();
		public IList<AvailableVehicle> AvailableVehicles
		{
			get { return _availableVehicles; }
			set
			{
				if (value.Count == 0 && _availableVehicles.Count > 0)
				{
					if (KeepVehiclesWhenResultNullStartTime == null)
					{
						KeepVehiclesWhenResultNullStartTime = DateTime.Now;
						return;
					}
					else if ((DateTime.Now - KeepVehiclesWhenResultNullStartTime.Value).TotalSeconds <= TimeToKeepVehiclesOnMapWhenResultNull)
					{
						return;
					}
				}

				KeepVehiclesWhenResultNullStartTime = null;

				_availableVehicles = value;
				RaisePropertyChanged();
			}
		}

		private CancellableCommand<MapBounds> _userMovedMap;
		public CancellableCommand<MapBounds> UserMovedMap
        {
            get
            {
				return _userMovedMap ?? (_userMovedMap = new CancellableCommand<MapBounds>(async (bounds, token) =>
                {
                	await _orderWorkflowService.SetAddressToCoordinate(
						new Position 
							{ 
								Latitude = bounds.GetCenter().Latitude, 
								Longitude = bounds.GetCenter().Longitude 
							},
                        token);
				}, _ => true));
            }
        }
    }
}