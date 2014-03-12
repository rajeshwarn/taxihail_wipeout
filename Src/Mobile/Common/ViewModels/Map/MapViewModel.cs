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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class MapViewModel: BaseViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IVehicleService _vehicleService;

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

				if (PickupAddress.HasValidCoordinate() && AddressSelectionMode == AddressSelectionMode.PickupSelection)
				{					
					ChangePresentation(new CenterMapPresentationHint(PickupAddress.Latitude, PickupAddress.Longitude));
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