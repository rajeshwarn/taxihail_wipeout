using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.AppServices.Orders;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class BottomBarViewModel: ChildViewModel
    {
		readonly IOrderWorkflowService _orderWorkflowService;

		public BottomBarViewModel(IOrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;

			BookLater = AddChild<BookLaterViewModel>();

			this.Observe(_orderWorkflowService.GetAndObserveAddressSelectionMode(),
				m => EstimateSelected = m == AddressSelectionMode.DropoffSelection);
		}

		private BookLaterViewModel _bookLater;
		public BookLaterViewModel BookLater
		{ 
			get { return _bookLater; }
			private set
			{ 
				_bookLater = value;
				RaisePropertyChanged();
			}
		}

		private bool _estimateSelected;
		public bool EstimateSelected
		{
			get
			{
				return _estimateSelected;
			}
			set
			{
				if(value != _estimateSelected)
				{
					_estimateSelected = value;
					RaisePropertyChanged();
				}
			}
		}

		public ICommand ChangeAddressSelectionMode
		{
			get
			{
				return this.GetCommand(() => {
					_orderWorkflowService.ToggleBetweenPickupAndDestinationSelectionMode();
				});
			}
		}

		public ICommand BookNow
		{
			get
			{
				return GetCommand(() =>
					{
						//TODO: _orderWorkflowService.SetPickupTimeToNow();
						try
						{
							_orderWorkflowService.ValidatePickupDestinationAndTime();
						}
						catch(OrderValidationException e)
						{
							// TODO: Display error
						}
                        ChangePresentation(new ShowOrderReviewPresentationHint());
					});
			}
		}	
    }
}

