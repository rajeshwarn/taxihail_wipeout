using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class BottomBarViewModel: ChildViewModel
    {
		readonly IOrderWorkflowService _orderWorkflowService;
		readonly IMessageService _messageService;
		readonly ILocalization _localize;

		public BottomBarViewModel(IOrderWorkflowService orderWorkflowService, IMessageService messageService, ILocalization localize)
		{
			_localize = localize;
			_messageService = messageService;
			_orderWorkflowService = orderWorkflowService;

			this.Observe(_orderWorkflowService.GetAndObserveAddressSelectionMode(),
				m => EstimateSelected = m == AddressSelectionMode.DropoffSelection);
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

		public ICommand SetPickupDateAndReviewOrder
		{
			get
			{
				return GetCommand<DateTime?>(async date =>
				{
					await _orderWorkflowService.SetPickupDate(date);
					try
					{
						await _orderWorkflowService.ValidatePickupDestinationAndTime();
						ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Review));
					}
					catch (OrderValidationException e)
					{
						switch (e.Error)
						{
							case OrderValidationError.PickupAddressRequired:
								_messageService.ShowMessage(_localize["InvalidBookinInfoTitle"], _localize["InvalidBookinInfo"]);
								break;
							case OrderValidationError.DestinationAddressRequired:
								_messageService.ShowMessage(_localize["InvalidBookinInfoTitle"], _localize["InvalidBookinInfoWhenDestinationIsRequired"]);
								break;
							case OrderValidationError.InvalidPickupDate:
								_messageService.ShowMessage(_localize["InvalidBookinInfoTitle"], _localize["BookViewInvalidDate"]);
								break;
							default:
								Logger.LogError(e);
								break;
						}
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
					try
					{
						var result = await _orderWorkflowService.ConfirmOrder();

						ShowViewModel<BookingStatusViewModel>(new
						{
							order = result.Item1.ToJson(),
							orderStatus = result.Item2.ToJson()
						});

					}
					catch(Exception e)
					{
						Logger.LogError(e);

						var settings = this.Services().Settings;
						var callIsEnabled = !settings.HideCallDispatchButton;
						if (callIsEnabled)
						{
							var errorMessage = string.Format(_localize["ServiceError_ErrorCreatingOrderMessage"],
								settings.ApplicationName, 
								settings.DefaultPhoneNumberDisplay);
							_messageService.ShowMessage(_localize["ErrorCreatingOrderTitle"], errorMessage);
						}
						else
						{
							_messageService.ShowMessage(_localize["ErrorCreatingOrderTitle"], _localize["ServiceError_ErrorCreatingOrderMessage_NoCall"]);
						}
					}


				});
			}
		}

        public ICommand BookLater
        {
            get
            {
                return GetCommand(() => {
                    ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.PickDate));
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

