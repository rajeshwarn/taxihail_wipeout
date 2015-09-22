using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Entity;
using Position = apcurium.MK.Booking.Mobile.Infrastructure.Position;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class MapViewModel: BaseViewModel
    {
		
		private static readonly int TimeToKeepVehiclesOnMapWhenResultNull = 10; // In seconds
		private DateTime? _keepVehiclesWhenResultNullStartTime = null;
       
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IVehicleService _vehicleService;

		public static int ZoomStreetLevel = 14;

		public MapViewModel(IOrderWorkflowService orderWorkflowService, IVehicleService vehicleService)
        {
			_orderWorkflowService = orderWorkflowService;
			_vehicleService = vehicleService;

			Observe(_orderWorkflowService.GetAndObserveAddressSelectionMode(), addressSelectionMode => AddressSelectionMode = addressSelectionMode);
			Observe(_orderWorkflowService.GetAndObserveIsDestinationModeOpened(), isDestinationModeOpened => IsDestinationModeOpened = isDestinationModeOpened);
            Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address);
			Observe(_orderWorkflowService.GetAndObserveDestinationAddress(), address => DestinationAddress = address);
			Observe(_vehicleService.GetAndObserveAvailableVehicles(), availableVehicles => AvailableVehicles = availableVehicles);
        }

		public override void Start()
		{
			base.Start();

			Observe(ObserveCurrentHomeViewModelState(), HomeViewModelStateChanged);
		}

		private IObservable<HomeViewModelState> ObserveCurrentHomeViewModelState()
		{
			return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
					h => Parent.PropertyChanged += h,
					h => Parent.PropertyChanged -= h
				)
				.Where(args => args.EventArgs.PropertyName.Equals("CurrentViewState"))
				.Select(_ => ((HomeViewModel) Parent).CurrentViewState)
				.DistinctUntilChanged();
		}

		private void HomeViewModelStateChanged(HomeViewModelState state)
		{
			if (state == HomeViewModelState.Initial || state == HomeViewModelState.BookingStatus || state == HomeViewModelState.ManualRidelinq)
			{
				IsMapDisabled = false;
			}
			else
			{
				IsMapDisabled = true;
			}
		}

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

		private bool _isMapDisabled;
		public bool IsMapDisabled
		{
			get { return _isMapDisabled; }
			set
			{
				if (_isMapDisabled != value)
				{
					_isMapDisabled = value;
					RaisePropertyChanged();
				}
			}
		}

        private IList<AvailableVehicle> _availableVehicles = new List<AvailableVehicle>();
		public IList<AvailableVehicle> AvailableVehicles
		{
			get { return _availableVehicles; }
		    private set
			{
                if (value != null
                    && value.Count == 0
                    && _availableVehicles != null
                    && _availableVehicles.Count > 0)
				{
					if (_keepVehiclesWhenResultNullStartTime == null)
					{
						_keepVehiclesWhenResultNullStartTime = DateTime.Now;
						return;
					}
					if ((DateTime.Now - _keepVehiclesWhenResultNullStartTime.Value).TotalSeconds <= TimeToKeepVehiclesOnMapWhenResultNull)
					{
						return;
					}
				}

				_keepVehiclesWhenResultNullStartTime = null;

				_availableVehicles = value;
				RaisePropertyChanged();
			}
		}

		private CancellableCommand<MapBounds> _userMovedMap;
		public CancellableCommand<MapBounds> UserMovedMap
        {
            get
            {
				return _userMovedMap ?? (_userMovedMap = new CancellableCommand<MapBounds>(SetAddressToCoordinate, _ => CanExecuteUserMovedMap()));
            }
        }

		private bool CanExecuteUserMovedMap()
		{
			return ((HomeViewModel)Parent).CurrentViewState == HomeViewModelState.Initial;
		}

		private async Task SetAddressToCoordinate(MapBounds bounds, CancellationToken token)
		{
			if (AddressSelectionMode == AddressSelectionMode.None)
			{
				return;
			}

			var position = new Position
			{
				Latitude = bounds.GetCenter().Latitude,
				Longitude = bounds.GetCenter().Longitude
			};

			await _orderWorkflowService.SetAddressToCoordinate(position, token);
		}
    }
}