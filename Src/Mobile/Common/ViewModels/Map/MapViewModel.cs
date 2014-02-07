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

        public ICommand UserMovedMap
        {
            get
            {
                return new CancellableCommand<MapBounds>(async (bounds, token) =>
                {

                    DeltaLongitude = Math.Abs((bounds.EastBound - bounds.WestBound) / 2);
                    DeltaLatitude = Math.Abs((bounds.NorthBound - bounds.SouthBound) / 2);
                        await _orderWorkflowService.SetAddressToCoordinate(new Position() { Latitude = bounds.GetCenter().Latitude, Longitude = bounds.GetCenter().Longitude },
                            token);

                }, _ => true);
            }
        }

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
                    catch(Exception e)
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

