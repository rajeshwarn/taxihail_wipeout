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

		private AddressSelectionMode AddressSelectionMode = AddressSelectionMode.PickupSelection;
		public bool ShowDestination
		{
			get { return AddressSelectionMode == AddressSelectionMode.DropoffSelection; }
			set
			{
				if (value != ShowDestination)
				{
					if (value)
					{
						AddressSelectionMode = AddressSelectionMode.DropoffSelection;
					}
					else
					{
						AddressSelectionMode = AddressSelectionMode.PickupSelection;
					}
					RaisePropertyChanged();
				}
			}
		}

		public ICommand ChangeSelectionMode
		{
			get {
				return GetCommand(() =>
					{
						ShowDestination = !ShowDestination;
					});
			}
		}
	}
}

