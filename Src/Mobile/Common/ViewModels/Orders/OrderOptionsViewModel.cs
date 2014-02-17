using System;
using System.Windows.Input;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderOptionsViewModel : ChildViewModel
	{
		readonly IOrderWorkflowService _orderWorkflowService;
		public OrderOptionsViewModel(IOrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;

			this.Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address);
			this.Observe(_orderWorkflowService.GetAndObserveDestinationAddress(), address => DestinationAddress = address);
			this.Observe(_orderWorkflowService.GetAndObserveAddressSelectionMode(), selectionMode => AddressSelectionMode = selectionMode);
			this.Observe(_orderWorkflowService.GetAndObserveEstimatedFare(), fare => EstimatedFare = fare);
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
				}
			}
		}

		

		private AddressSelectionMode _addressSelectionMode;
		public AddressSelectionMode AddressSelectionMode
		{
			get { return _addressSelectionMode; }
			set 
			{ 
				if (value != _addressSelectionMode)
				{
					_addressSelectionMode = value;
					RaisePropertyChanged();
					OnAddressSelectionModeChanged();
				}
			} 
		}

		private bool _showDestination;
		public bool ShowDestination
		{
			get
			{ 
				return _showDestination;
			}
			private set
			{
                _showDestination = value;
				RaisePropertyChanged();
			}
		}

		private string _estimatedFare;
		public string EstimatedFare
		{
			get { return _estimatedFare; }
			set
			{
				if (value != _estimatedFare)
				{
					_estimatedFare = value;
					RaisePropertyChanged();
				}
			}
		}

        public ICommand SetAddress
        {
            get
            {
                return this.GetCommand<Address>(address => {
                    _orderWorkflowService.SetAddress(address);
                });
            }
        }

		public ICommand ShowSearchAddress
		{
			get
			{
				return this.GetCommand(() => {
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.AddressSearch));
				});
			}
		}

		void OnAddressSelectionModeChanged()
		{
			ShowDestination = AddressSelectionMode == AddressSelectionMode.DropoffSelection;
            if (!ShowDestination)
            {
                _orderWorkflowService.ClearDestinationAddress();
            }
		}
	}
}

