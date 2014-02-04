using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Data;
using System.Windows.Input;

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
					RaisePropertyChanged(() => ShowDestination);
				}
			} 
		}

		public bool ShowDestination
		{
			get { 
				var showDestination = AddressSelectionMode == AddressSelectionMode.DropoffSelection || AddressSelectionMode == AddressSelectionMode.None;
				if (!showDestination)
				{
					_orderWorkflowService.ClearDestinationAddress();
				}

				return showDestination;
			}
		}

		private bool _isConfirmationScreen;
		public bool IsConfirmationScreen
		{
			get { return _isConfirmationScreen; }
			set
			{
				if (value != _isConfirmationScreen)
				{
					if (value)
					{
						AddressSelectionMode = AddressSelectionMode.None;
					}
					else
					{
						AddressSelectionMode = AddressSelectionMode.PickupSelection;
					}

					_isConfirmationScreen = value;
					RaisePropertyChanged();
				}
			}
		}
	}
}

