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

		private Position _mapCenter;
		public Position MapCenter
		{
			get { return _mapCenter; }
			set
			{
				if (value != _mapCenter)
				{
					_mapCenter = value;
					RaisePropertyChanged();
				}
			}
		}


		private AddressSelectionMode _addressSelectionMode; 
		public AddressSelectionMode AddressSelectionMode
		{ 
			get
			{
				return _addressSelectionMode;
			}
			set
			{
				_addressSelectionMode = value;

				if (PickupAddress != null && AddressSelectionMode == AddressSelectionMode.PickupSelection)
				{
					DeltaLatitude = DeltaLongitude = DefaultDelta;
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

        [DefaultValue(0.002d)]
        public double DeltaLatitude { get; set; }

        [DefaultValue(0.002d)]
        public double DeltaLongitude { get; set; }

        public double DefaultDelta = 0.002d;

        public ICommand UserMovedMap
        {
            get
            {
                return new CancellableCommand<MapBounds>(async (bounds, token) =>
                {
                        await _orderWorkflowService.SetAddressToCoordinate(new Position() { Latitude = bounds.GetCenter().Latitude, Longitude = bounds.GetCenter().Longitude },
                            token);

                }, _ => true);
            }
        }

		private void OnPickupAddressChanged()
		{			
            if (PickupAddress.HasValidCoordinate())
			{
				var coordinate = new Position() { Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };
				MapCenter = coordinate;
			}
		}

		private void OnDestinationAddressChanged()
		{
			if (DestinationAddress.HasValidCoordinate())
			{
				var coordinate = new Position() { Latitude = DestinationAddress.Latitude, Longitude = DestinationAddress.Longitude };
				MapCenter = coordinate;
			}
		}

		private MapBounds GetMapBoundsFromCoordinateAndDelta(Position coordinate, double deltaLatitude, double deltaLongitude)
		{
			return new MapBounds()
			{
				NorthBound = coordinate.Latitude + deltaLatitude / 2,
				SouthBound = coordinate.Latitude - deltaLatitude / 2,
				EastBound = coordinate.Longitude + deltaLongitude / 2,
				WestBound = coordinate.Longitude - deltaLongitude / 2
			};
		}	

		public AsyncCommand ZoomToAddress
		{
			get
			{
				return GetCommand(() =>
				{
					Position pos = new Position();
					if (AddressSelectionMode == AddressSelectionMode.PickupSelection)
					{
						pos = new Position() { Latitude = PickupAddress.Latitude, Longitude = PickupAddress.Longitude };
					}
					else if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
					{
						pos = new Position() { Latitude = DestinationAddress.Latitude, Longitude = DestinationAddress.Longitude };
					}
					
					MapBounds = GetMapBoundsFromCoordinateAndDelta(pos, 0.002d, 0.002d);
				});
			}
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

