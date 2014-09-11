using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Plugins.PhoneCall;
using ServiceStack.Text;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
    public class BottomBarViewModel : BaseViewModel, IRequestPresentationState<HomeViewModelStateRequestedEventArgs>
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IMvxPhoneCallTask _phone;
        private readonly IAccountService _accountService;

        public event EventHandler<HomeViewModelStateRequestedEventArgs> PresentationStateRequested;

        public BottomBarViewModel(IOrderWorkflowService orderWorkflowService, IMvxPhoneCallTask phone, IAccountService accountService)
        {
            _phone = phone;
            _orderWorkflowService = orderWorkflowService;
            _accountService = accountService;

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
                if (value != _estimateSelected)
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
				return this.GetCommand(async () => {
					var mode = await _orderWorkflowService.GetAndObserveAddressSelectionMode().Take(1).ToTask();
					if(mode == AddressSelectionMode.PickupSelection)
					{
						this.Services().Analytics.LogEvent("DestinationButtonTapped");
					}
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
                    await _orderWorkflowService.SetPickupDate(date);
                    try
                    {
                        await _orderWorkflowService.ValidatePickupAndDestination();
                        await _orderWorkflowService.ValidatePickupTime();
                    }
                    catch (OrderValidationException e)
                    {
                        switch (e.Error)
                        {
                            case OrderValidationError.OpenDestinationSelection:
                                // not really an error, but we stop the command from proceeding at this point
                                return;
                            case OrderValidationError.PickupAddressRequired:
                                this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfo"]);
                                return;
                            case OrderValidationError.DestinationAddressRequired:
                                this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfoWhenDestinationIsRequired"]);
                                return;
                            case OrderValidationError.InvalidPickupDate:
                                this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["BookViewInvalidDate"]);
                                return;
                            default:
                                Logger.LogError(e);
                                return;
                        }
                    }

                    ReviewOrderDetails();
				});
			}
		}

        public async void ReviewOrderDetails()
	    {
            await _orderWorkflowService.ResetOrderSettings();
            PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Review));
            await ShowFareEstimateAlertDialogIfNecessary();
            await ValidateCardOnFile();
            await PreValidateOrder();
	    }

        public ICommand ConfirmOrder
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    try
                    {
                        var canBeConfirmed = await _orderWorkflowService.GetAndObserveOrderCanBeConfirmed().Take(1).ToTask();
                        if (!canBeConfirmed)
                        {
                            return;
                        }

                        var cardValidated = await _orderWorkflowService.ValidateCardOnFile();
                        if (!cardValidated)
                        {
                            this.Services().Message.ShowMessage(
                                this.Services().Localize["ErrorCreatingOrderTitle"],
                                this.Services().Localize["NoCardOnFileMessage"]);

                            return;
                        }

                        _orderWorkflowService.BeginCreateOrder();

                        if (await _orderWorkflowService.ShouldGoToAccountNumberFlow())
                        {
                            var hasValidAccountNumber = await _orderWorkflowService.ValidateAccountNumberAndPrepareQuestions();
                            if (!hasValidAccountNumber)
                            {
                                var accountNumber = await this.Services().Message.ShowPromptDialog(
                                    this.Services().Localize["AccountPaymentNumberRequiredTitle"],
                                    this.Services().Localize["AccountPaymentNumberRequiredMessage"],
                                    () => { return; });

                                hasValidAccountNumber = await _orderWorkflowService.ValidateAccountNumberAndPrepareQuestions(accountNumber);
                                if (!hasValidAccountNumber)
                                {
                                    await this.Services().Message.ShowMessage(
                                        this.Services().Localize["Error_AccountPaymentTitle"],
                                        this.Services().Localize["Error_AccountPaymentMessage"]);
                                    return;
                                }

                                await _orderWorkflowService.SetAccountNumber(accountNumber);
                            }

                            var questions = await _orderWorkflowService.GetAccountPaymentQuestions();
                            if ((questions != null) && (questions.Length > 0))
                            {
                                PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial, true));
                                ShowViewModel<InitializeOrderForAccountPaymentViewModel>();
                            }
                            else
                            {
                                using (this.Services().Message.ShowProgress())
                                {
                                    var result = await _orderWorkflowService.ConfirmOrder();

								this.Services().Analytics.LogEvent("Book");
                                    PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial, true));
                                    ShowViewModel<BookingStatusViewModel>(new
                                    {
                                        order = result.Item1.ToJson(),
                                        orderStatus = result.Item2.ToJson()
                                    });
                                }
                            }
                        }
                        else
                        {
                            using (this.Services().Message.ShowProgress())
                            {
                                var result = await _orderWorkflowService.ConfirmOrder();

                                PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial, true));
                                ShowViewModel<BookingStatusViewModel>(new
                                {
                                    order = result.Item1.ToJson(),
                                    orderStatus = result.Item2.ToJson()
                                });
                            }
                        }
                    }
                    catch (OrderCreationException e)
                    {
                        Logger.LogError(e);

                        var title = this.Services().Localize["ErrorCreatingOrderTitle"];

                        switch (e.Message)
                        {
                            case "CreateOrder_PendingOrder":
                                {
                                    Guid pendingOrderId;
                                    Guid.TryParse(e.MessageNoCall, out pendingOrderId);

								this.Services().Message.ShowMessage(title, this.Services().Localize["ServiceError" + e.Message],
									this.Services().Localize["View"], async () =>
									{
										var orderInfos = await GetOrderInfos(pendingOrderId);

										PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial, true));
										ShowViewModel<BookingStatusViewModel>(new {order = orderInfos.Item1, orderStatus = orderInfos.Item2});
									},
                                        this.Services().Localize["Cancel"], () => {});
                                }
                                break;
                            default:
                                {
                                    if (!Settings.HideCallDispatchButton)
                                    {
                                        this.Services().Message.ShowMessage(title, e.Message,
                                            "Call", () => _phone.MakePhoneCall(Settings.ApplicationName, Settings.DefaultPhoneNumber),
                                            "Cancel", () => { });
                                    }
                                    else
                                    {
                                        this.Services().Message.ShowMessage(title, e.MessageNoCall);
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }
 					finally 
					{
						_orderWorkflowService.EndCreateOrder ();
					}
				});
            }
        }

        public ICommand BookLater
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    try
                    {
                        await _orderWorkflowService.ValidatePickupAndDestination();
                        PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.PickDate));
                    }
                    catch (OrderValidationException e)
                    {
                        switch (e.Error)
                        {
                            case OrderValidationError.OpenDestinationSelection:
                                // not really an error, but we stop the command from proceeding at this point
                                return;
                            case OrderValidationError.PickupAddressRequired:
                                this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfo"]);
                                return;
                            case OrderValidationError.DestinationAddressRequired:
                                this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfoWhenDestinationIsRequired"]);
                                return;
                        }
                    }
                });
            }
        }

        public ICommand Edit
        {
            get
            {
                return this.GetCommand(() =>
                {
					this.Services().Analytics.LogEvent("EditOrderSettingsTapped");
                    PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Edit));
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
                return this.GetCommand(() =>
                {
                    // set pickup date to null to reset the estimate for now and not the possible date set by book later
                    _orderWorkflowService.SetPickupDate(null);
                    _orderWorkflowService.CancelRebookOrder();
                    PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial));
                });
            }
        }

        public ICommand CancelBookLater
        {
            get
            {
                return this.GetCommand(() =>
                {
                    PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial));
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

        async Task ShowFareEstimateAlertDialogIfNecessary()
        {
            if (await _orderWorkflowService.ShouldWarnAboutEstimate())
            {
                this.Services().Message.ShowMessage(this.Services().Localize["WarningEstimateTitle"], this.Services().Localize["WarningEstimate"],
					this.Services().Localize["OkButtonText"], () => {},
                    this.Services().Localize["WarningEstimateDontShow"], () => this.Services().Cache.Set("WarningEstimateDontShow", "yes"));
            }
        }

        private async Task ValidateCardOnFile()
        {
            if (!await _orderWorkflowService.ValidateCardOnFile())
            {
                this.Services().Message.ShowMessage(
                    this.Services().Localize["ErrorCreatingOrderTitle"], this.Services().Localize["NoCardOnFileMessage"],
                    this.Services().Localize["AddACardButton"],
                    () => {
                        PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial));
						ShowViewModel<CreditCardAddViewModel>(new { showInstructions = true });
                    },
                    this.Services().Localize["Cancel"],
                    () => PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial)));
            }
        }

        private async Task PreValidateOrder()
        {
            var validationInfo = await _orderWorkflowService.ValidateOrder();
            if (validationInfo.HasError)
            {
                this.Services().Message.ShowMessage(
                    this.Services().Localize["ErrorCreatingOrderTitle"], validationInfo.Message,
                    () => PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial)));
            }
            else
            {
                if (validationInfo.HasWarning)
                {
                    this.Services().Message.ShowMessage(
                        this.Services().Localize["WarningTitle"], validationInfo.Message,
                        this.Services().Localize["Continue"], () => _orderWorkflowService.ConfirmValidationOrder(),
                        this.Services().Localize["Cancel"], () => PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial)));
                }
                else
                {
                    _orderWorkflowService.ConfirmValidationOrder();
                }
            }
        }

        private async Task<Tuple<string, string>> GetOrderInfos(Guid pendingOrderId)
        {
            var order = await _accountService.GetHistoryOrderAsync(pendingOrderId);

            var orderStatus = new OrderStatusDetail
            {
                IBSOrderId = order.IBSOrderId,
                IBSStatusDescription = this.Services().Localize["LoadingMessage"],
                IBSStatusId = "",
                OrderId = pendingOrderId,
                Status = OrderStatus.Unknown,
                VehicleLatitude = null,
                VehicleLongitude = null
            };

            return Tuple.Create(order.ToJson(), orderStatus.ToJson());
        }
    }
}

