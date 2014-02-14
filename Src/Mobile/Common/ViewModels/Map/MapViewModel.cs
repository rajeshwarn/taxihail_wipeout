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
using System.Threading.Tasks;
using System.Threading;
using MK.Common.Configuration;

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
                    OnAddressChanged(PickupAddress);
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
                    OnAddressChanged(DestinationAddress);
				}
			}
		}

		private MapBounds _mapBounds;
		public MapBounds MapBounds
		{
			get 
			{ 
				return (_mapBounds == null)
					? GetMapBoundsFromCoordinateAndDelta(new Position
						{ 
							Latitude = this.Services().Settings.DefaultLatitude, 
							Longitude = this.Services().Settings.DefaultLongitude
						}, 0.04, 0.04)
					: _mapBounds;
			}
			set
			{
				if (value != _mapBounds)
				{
					_mapBounds = value;
					RaisePropertyChanged();
				}
			}
		}

		private Position _mapCenter;
		public Position MapCenter
		{
			get { return _mapCenter; }
			{ 
			set
			{
				if (value != _mapCenter)
				{
					_mapCenter = value;
					RaisePropertyChanged();
				}
			}
		}

		private bool _isZooming;
        public bool IsZooming
        {
			get { return _isZooming; }
            set
            {
				if (value != _isZooming)
                {
					_isZooming = value;
                    RaisePropertyChanged();
                }
            }
        }

		private AddressSelectionMode _addressSelectionMode; 
		public AddressSelectionMode AddressSelectionMode
		{ 
			get { return _addressSelectionMode; }
			set
			{
				_addressSelectionMode = value;

				if (PickupAddress != null && AddressSelectionMode == AddressSelectionMode.PickupSelection)
				{					
                    var coordinate = new Position() { Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };                   
					MapCenter = coordinate;
				}

				RaisePropertyChanged();
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

        public ICommand UserMovedMap
        {
            get
            {
                return new CancellableCommand<MapBounds>(async (bounds, token) =>
                {
                	await _orderWorkflowService.SetAddressToCoordinate(
						new Position 
							{ 
								Latitude = bounds.GetCenter().Latitude, 
								Longitude = bounds.GetCenter().Longitude 
							},
                        token);

                }, _ => true);
            }
        }

		private void OnPickupAddressChanged()
		{			
            if (PickupAddress.HasValidCoordinate() && !IsZooming)
			{
				var coordinate = new Position() { Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };
				MapCenter = coordinate;
			}
		}

		private void OnDestinationAddressChanged()
		{
            if (DestinationAddress.HasValidCoordinate() && !IsZooming)
			{
				var coordinate = new Position() { Latitude = DestinationAddress.Latitude, Longitude = DestinationAddress.Longitude };
				MapCenter = coordinate;
			}
		}

        private void OnAddressChanged(Address address)
        {
            if (!address.HasValidCoordinate())
            {
                return;
            }

            if (IsZooming)
            {
                MapBounds = GetMapBoundsFromCoordinateAndDelta(PositionFromAddress(address), 0.002d, 0.002d);
                return;
            }
        }

		public static MapBounds GetMapBoundsFromCoordinateAndDelta(Position coordinate, double deltaLatitude, double deltaLongitude)
		{
			return new MapBounds()
			{
				NorthBound = coordinate.Latitude + deltaLatitude / 2,
				SouthBound = coordinate.Latitude - deltaLatitude / 2,
				EastBound = coordinate.Longitude + deltaLongitude / 2,
				WestBound = coordinate.Longitude - deltaLongitude / 2
			};
		}	

		public static Position PositionFromAddress(Address address)
		{
			return new Position() { Latitude = address.Latitude, Longitude = address.Longitude };
		}

		public class CancellableCommand<TParam>: ICommand
        {
            private Func<TParam,bool> _canExecute;
            private Func<TParam, CancellationToken, Task> _execute;
            private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

            public CancellableCommand(Func<TParam, CancellationToken, Task> execute, Func<TParam, bool> canExecute)
            {
                _execute = execute;
                _canExecute = canExecute;
                new CancellationTokenSource().Dispose();
            }

            public bool CanExecute(object parameter)
            {
                if (_canExecute == null)
                {
                    return true;
                }

                return _canExecute((TParam)parameter);
            }

            public bool CanExecute()
            {
                return CanExecute(null);
            }

            public async void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    var token = GetNewCancellationToken();
                    try
                    {
                        var source = new CancellationTokenSource();
                        await _execute((TParam)parameter, token);
                    }
                    catch(Exception)
                    {
                    }
                }
            }

            public void Execute()
            {
                Execute(null);
            }

            public void Cancel()
            {
                _cancellationTokenSource.Cancel();
            }

            public event EventHandler CanExecuteChanged;

            protected virtual void OnCanExecuteChanged()
            {
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, new EventArgs());
                }
            }

            private CancellationToken GetNewCancellationToken()
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                return _cancellationTokenSource.Token;
            }
        }
    }
}