using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.PresentationHints;

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
							ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Review));
						}
						catch(OrderValidationException e)
						{
							// TODO: Display error
						}
					});
			}
		}

		public ICommand ConfirmOrder
		{
			get
			{
				return GetCommand(async () =>
					{
						//TODO: Show Progress
							try
							{
							/*
								var orderInfo = await this.Services().Booking.CreateOrder(Order);

								if (!orderInfo.IbsOrderId.HasValue || !(orderInfo.IbsOrderId > 0))
									return;

								var orderCreated = new Order
								{
									CreatedDate = DateTime.Now, 
									DropOffAddress = Order.DropOffAddress, 
									IbsOrderId = orderInfo.IbsOrderId, 
									Id = Order.Id, PickupAddress = Order.PickupAddress,
									Note = Order.Note, 
									PickupDate = Order.PickupDate.HasValue ? Order.PickupDate.Value : DateTime.Now,
									Settings = Order.Settings
								};
								*/

								ShowViewModel<BookingStatusViewModel>(new
									{
									//order = orderCreated.ToJson(),
									//orderStatus = orderInfo.ToJson()
									});	

							}
							catch
							{
							/*if (CallIsEnabled)
								{
									var err = string.Format(this.Services().Localize["ServiceError_ErrorCreatingOrderMessage"], Settings.ApplicationName, 
										Settings.DefaultPhoneNumberDisplay);
									this.Services().Message.ShowMessage(this.Services().Localize["ErrorCreatingOrderTitle"], err);
								}
								else
								{
									this.Services().Message.ShowMessage(this.Services().Localize["ErrorCreatingOrderTitle"], this.Services().Localize["ServiceError_ErrorCreatingOrderMessage_NoCall"]);
								}
								*/
							}


					}); 
			}
		}

		public ICommand Edit
		{
			get
			{
				return GetCommand(() => {
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Edit));
				});
			}
		}

		public ICommand Save
		{
			get
			{
				return GetCommand(() => {
					// TODO: Actually save changes
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Review));
				});
			}
		}	

		public ICommand CancelReview
        {
            get
			{
				return GetCommand(() => {
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
				});
			}
        }

		public ICommand CancelEdit
		{
			get
			{
				return GetCommand(() => {
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Review));
				});
			}
		}
    }
}

