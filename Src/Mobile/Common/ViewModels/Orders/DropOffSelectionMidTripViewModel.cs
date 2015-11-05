using System;
using apcurium.MK.Common.Entity;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class DropOffSelectionMidTripViewModel : BaseViewModel
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private DropOffSelectionBottomBarViewModel _bottomBar;

		public DropOffSelectionMidTripViewModel(IOrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;

			BottomBar = AddChild<DropOffSelectionBottomBarViewModel>();

			Observe (_orderWorkflowService.GetAndObserveDestinationAddress (), address => DestinationAddress = address);
			Observe (_orderWorkflowService.GetAndObserveLoadingAddress (), loading => IsLoadingAddress = loading);
		}

		public DropOffSelectionBottomBarViewModel BottomBar
		{
			get { return _bottomBar; }
			set
			{
				_bottomBar = value; 
				RaisePropertyChanged();
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

		private bool _isLoadingAddress;
		public bool IsLoadingAddress
		{
			get
			{ 
				return _isLoadingAddress;
			}
			private set
			{
				if (_isLoadingAddress != value)
				{
					_isLoadingAddress = value;
					RaisePropertyChanged();
				}
			}
		}

		public ICommand SetAddress
		{
			get
			{
				return this.GetCommand<Address>(address => _orderWorkflowService.SetAddress(address));
			}
		}

		public ICommand ShowDestinationSearchAddress
		{
			get
			{
				return this.GetCommand(() =>
					{
						((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.AddressSearch;
					});
			}
		}
	}
}

