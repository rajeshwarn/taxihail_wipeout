using System;
using System.Windows.Input;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using Cirrious.MvvmCross.Plugins.PhoneCall;
using System.Threading.Tasks;


namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class BottomBarViewModel: ChildViewModel
    {
		readonly IOrderWorkflowService _orderWorkflowService;
		readonly IMvxPhoneCallTask _phone;

		public BottomBarViewModel(IOrderWorkflowService orderWorkflowService, IMvxPhoneCallTask phone)
		{
			_phone = phone;
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
				return this.GetCommand<DateTime?>(async date =>
				{
					_orderWorkflowService.SetPickupDate(date);
					try
					{
						await _orderWorkflowService.ValidatePickupDestinationAndTime();
					}
					catch (OrderValidationException e)
					{
						switch (e.Error)
						{
							case OrderValidationError.PickupAddressRequired:
								this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfo"]);
								break;
							case OrderValidationError.DestinationAddressRequired:
								this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfoWhenDestinationIsRequired"]);
								break;
							case OrderValidationError.InvalidPickupDate:
								this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["BookViewInvalidDate"]);
								break;
							default:
								Logger.LogError(e);
								break;
						}
					}
					_orderWorkflowService.ResetOrderSettings();
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Review));
					await ShowFareEstimateAlertDialogIfNecessary();
					await PreValidateOrder();
				});
			}
		}

		public ICommand ConfirmOrder
		{
			get
			{
				return this.GetCommand(async () =>
				{
					try
					{
                        using(this.Services().Message.ShowProgress())						
						{
							var result = await _orderWorkflowService.ConfirmOrder();
                            ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Initial, true));
							ShowViewModel<BookingStatusViewModel>(new
							{
								order = result.Item1.ToJson(),
								orderStatus = result.Item2.ToJson()
							});
						}
					}
					catch(OrderCreationException e)
					{
						Logger.LogError(e);

						var settings = this.Services().Settings;
						var callIsEnabled = false;//!settings.HideCallDispatchButton;
						var title = this.Services().Localize["ErrorCreatingOrderTitle"];

						if (callIsEnabled)
						{
							this.Services().Message.ShowMessage(title,
								e.Message,
								"Call",
								() => _phone.MakePhoneCall (settings.ApplicationName, settings.DefaultPhoneNumber),
								"Cancel",
								delegate { });
						}
						else
						{
							this.Services().Message.ShowMessage(title, e.MessageNoCall);
						}
					}
					catch(Exception e)
					{
						Logger.LogError(e);
					}


				});
			}
		}

        public ICommand BookLater
        {
            get
            {
				return this.GetCommand(() => {
                    ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.PickDate));
                });
            }
        }

		public ICommand Edit
		{
			get
			{
				return this.GetCommand(() => {
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Edit));
				});
			}
		}

		private ICommand _save;
		public ICommand Save
		{
			get { return _save; }
			set
			{
				if (value != _save)
				{
					_save = value;
					RaisePropertyChanged();
				}
			}
		}	

		public ICommand CancelReview
        {
            get
			{
				return this.GetCommand(() => {
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
				});
			}
        }

		public ICommand CancelBookLater
		{
			get
			{
				return this.GetCommand(() => {
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
				});
			}
		}

		private ICommand _cancelEdit;
		public ICommand CancelEdit
		{
			get { return _cancelEdit; }
			set
			{
				if (value != _cancelEdit)
				{
					_cancelEdit = value;
					RaisePropertyChanged();
				}
			}
		}

		public bool DisableFutureBooking
		{
			get
			{
				return this.Services().Settings.DisableFutureBooking;
			}
		}

		async Task ShowFareEstimateAlertDialogIfNecessary()
		{
			if (await _orderWorkflowService.ShouldWarnAboutEstimate())
			{
				this.Services().Message.ShowMessage(this.Services().Localize["WarningEstimateTitle"], this.Services().Localize["WarningEstimate"],
					"Ok", delegate{ },
					this.Services().Localize["WarningEstimateDontShow"], () => this.Services().Cache.Set("WarningEstimateDontShow", "yes"));
			}
		}

		private async Task PreValidateOrder()
		{
			var validationInfo = await _orderWorkflowService.ValidateOrder();
			if (validationInfo.HasWarning)
			{
				this.Services().Message.ShowMessage(this.Services().Localize["WarningTitle"], 
					validationInfo.Message, 
					this.Services().Localize["Continue"], 
					delegate{}, 
					this.Services().Localize["Cancel"], 
					() => { ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
				});
			}
		}

    }
}

