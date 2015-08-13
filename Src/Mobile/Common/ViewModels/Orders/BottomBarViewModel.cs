using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Plugins.PhoneCall;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
    public class BottomBarViewModel : BaseViewModel
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IMvxPhoneCallTask _phone;
        private readonly IAccountService _accountService;
        private readonly IPaymentService _paymentService;

        private OrderValidationResult _orderValidationResult;

        public BottomBarViewModel(IOrderWorkflowService orderWorkflowService, IMvxPhoneCallTask phone, IAccountService accountService, IPaymentService paymentService)
        {
            _phone = phone;
            _orderWorkflowService = orderWorkflowService;
            _accountService = accountService;
            _paymentService = paymentService;

            if (Settings.DestinationIsRequired)
			{
				Observe(_orderWorkflowService.GetAndObserveIsDestinationModeOpened(),
					isDestinationModeOpened => EstimateSelected = isDestinationModeOpened);
			}

            if (Settings.PromotionEnabled)
            {
                Observe(ObserveIsPromoCodeApplied(), isPromoCodeApplied => IsPromoCodeActive = isPromoCodeApplied);
            }

            Observe(_orderWorkflowService.GetAndObserveOrderValidationResult(), OrderValidated);
        }

        public async void CheckManualRideLinqEnabledAsync(bool isInMarket)
        {
            try
            {
                var settings = await _paymentService.GetPaymentSettings();

                IsManualRidelinqEnabled = settings.PaymentMode == PaymentMethod.RideLinqCmt
                                           && settings.CmtPaymentSettings.IsManualRidelinqCheckInEnabled
										   && !isInMarket;

                BookButtonHidden = Settings.DisableImmediateBooking && !Settings.UseSingleButtonForNowAndLaterBooking;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private IObservable<bool> ObserveIsPromoCodeApplied()
        {
            return _orderWorkflowService.GetAndObservePromoCode()
				.Select(promoCode => !string.IsNullOrEmpty(promoCode));
        }

        private bool _isPromoCodeActive;
        public bool IsPromoCodeActive {
            get 
			{ 
				return _isPromoCodeActive; 
			}
            set
            {
                _isPromoCodeActive = value;
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
                if (value != _estimateSelected)
                {
                    _estimateSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isManualRidelinqEnabled;
        public bool IsManualRidelinqEnabled
        {
            get { return _isManualRidelinqEnabled; }
            set
            {
                _isManualRidelinqEnabled = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => BookButtonText);
            }
        }

		bool _bookButtonHidden = true;

		public bool BookButtonHidden
		{
			get
			{
				return _bookButtonHidden;
			}

			set
			{
				_bookButtonHidden = value;
				RaisePropertyChanged(() => BookButtonHidden);
			}
		}

        private bool _isFutureBookingDisabled = true;
        public bool IsFutureBookingDisabled
        {
            get { return _isFutureBookingDisabled; }
            set
            {
                if (_isFutureBookingDisabled != value)
                {
                    _isFutureBookingDisabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void OrderValidated(OrderValidationResult orderValidationResult)
        {
            _orderValidationResult = orderValidationResult;
            IsFutureBookingDisabled = Settings.DisableFutureBooking 
				|| (orderValidationResult != null && orderValidationResult.DisableFutureBooking) 
                || Settings.UseSingleButtonForNowAndLaterBooking;
        }

        public ICommand ChangeAddressSelectionMode
        {
            get
            {
				return this.GetCommand(async () => {
					EstimateSelected = !EstimateSelected;

					var mode = await _orderWorkflowService.GetAndObserveAddressSelectionMode().Take(1).ToTask();
					if(mode == AddressSelectionMode.PickupSelection)
					{
						this.Services().Analytics.LogEvent("DestinationButtonTapped");
					}
						
                    _orderWorkflowService.ToggleBetweenPickupAndDestinationSelectionMode().FireAndForget();
					_orderWorkflowService.ToggleIsDestinationModeOpened().FireAndForget();
                });
            }
        }

        public ICommand SetPickupDateAndReviewOrder
        {
            get
            {
                return this.GetCommand<DateTime?>(async date =>
                {
                    if (_orderValidationResult.HasError
                        && _orderValidationResult.AppliesToCurrentBooking)
                    {
                        this.Services().Message.ShowMessage(this.Services().Localize["CurrentBookingDisabledTitle"], _orderValidationResult.Message);
						ResetToInitialState.ExecuteIfPossible();
                        return;
                    }

					// since it can take some time, recalculate estimate for date only if 
					// last calculated estimate was not for now
					if(date != null)
					{
						await _orderWorkflowService.SetPickupDate(date);
					}
                    
                    try
                    {
                        await _orderWorkflowService.ValidatePickupAndDestination();
                        await _orderWorkflowService.ValidatePickupTime();
						await _orderWorkflowService.ValidateNumberOfPassengers(null);
                    }
                    catch (OrderValidationException e)
                    {
                        switch (e.Error)
                        {
                            case OrderValidationError.OpenDestinationSelection:
                                // not really an error, but we stop the command from proceeding at this point
                                return;
                            case OrderValidationError.PickupAddressRequired:
								ResetToInitialState.ExecuteIfPossible();
                                this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfo"]);
                                return;
                            case OrderValidationError.DestinationAddressRequired:
								ResetToInitialState.ExecuteIfPossible();
                                this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfoWhenDestinationIsRequired"]);
                                return;
                            case OrderValidationError.InvalidPickupDate:
								ResetToInitialState.ExecuteIfPossible();
                                this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["BookViewInvalidDate"]);
                                return;
							case OrderValidationError.InvalidPassengersNumber:
								this.Services().Message.ShowMessage(this.Services().Localize["InvalidPassengersNumberTitle"], this.Services().Localize["InvalidPassengersNumber"]);
								return;
                            default:
                                Logger.LogError(e);
                                return;
                        }
                    }
					catch(Exception ex)
					{
						Logger.LogError(ex);
						ResetToInitialState.ExecuteIfPossible();
						return;
					}

                    ReviewOrderDetails();
				});
			}
		}

        public async void ReviewOrderDetails()
	    {
            await _orderWorkflowService.ResetOrderSettings();
			((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Review;
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
                        var chargeTypeValidated = await _orderWorkflowService.ValidateChargeType();
                        if (!chargeTypeValidated)
                        {
                            this.Services().Message.ShowMessage(
                                this.Services().Localize["InvalidChargeTypeTitle"],
                                this.Services().Localize["InvalidChargeType"]);

                            return;
                        }

                        var canBeConfirmed = await _orderWorkflowService.GetAndObserveOrderCanBeConfirmed().Take(1).ToTask();
                        if (!canBeConfirmed)
                        {
                            return;
                        }

						var promoConditionsValidated = await _orderWorkflowService.ValidatePromotionUseConditions();
						if(!promoConditionsValidated)
						{
							this.Services().Message.ShowMessage(
								this.Services().Localize["ErrorCreatingOrderTitle"],
								this.Services().Localize["PromoMustUseCardOnFileMessage"]);

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

						var cardExpirationValidated = await _orderWorkflowService.ValidateCardExpiration();
						if (!cardExpirationValidated)
						{
							this.Services().Message.ShowMessage(
								this.Services().Localize["ErrorCreatingOrderTitle"],
								this.Services().Localize["CardExpiredMessage"]);

							return;
						}

						if (await _orderWorkflowService.ShouldWarnAboutPromoCode())
						{
							var acceptedConditions = false;
							await this.Services().Message.ShowMessage(
								this.Services().Localize["WarningTitle"], 
								this.Services().Localize["PromoMustUseCardOnFileWarningMessage"],
								this.Services().Localize["OkButtonText"], 
								() => { 
									acceptedConditions = true; 
								},
								this.Services().Localize["Cancel"], 
								() => { },
								this.Services().Localize["WarningPromoCodeDontShow"], 
								() => { 
									this.Services().Cache.Set("WarningPromoCodeDontShow", "yes"); 
									acceptedConditions = true; 
								});

							if(!acceptedConditions)
							{
								return;
							}
						}

						_orderWorkflowService.BeginCreateOrder();

						if (await _orderWorkflowService.ShouldPromptForCvv())
						{
							var cvv = await this.Services().Message.ShowPromptDialog(
								this.Services().Localize["CvvRequiredTitle"],
                                string.Format(this.Services().Localize["CvvRequiredMessage"], _accountService.CurrentAccount.DefaultCreditCard.Last4Digits),
								() => { return; },
                                true );

							// validate that it's a numeric value with 3 or 4 digits
							var cvvSetCorrectly = _orderWorkflowService.ValidateAndSetCvv(cvv);
							if(!cvvSetCorrectly)
							{
								await this.Services().Message.ShowMessage(
									this.Services().Localize["Error_InvalidCvvTitle"],
									this.Services().Localize["Error_InvalidCvvMessage"]);
								return;
							}
						}

						if (await _orderWorkflowService.ShouldGoToAccountNumberFlow())
						{
							var hasValidAccountNumber = await _orderWorkflowService.ValidateAccountNumberAndPrepareQuestions();
							if (!hasValidAccountNumber)
							{
								var accountNumber = await this.Services().Message.ShowPromptDialog(
									this.Services().Localize["AccountPaymentNumberRequiredTitle"],
									this.Services().Localize["AccountPaymentNumberRequiredMessage"],
									() => { return; });

                                var customerNumber = await this.Services().Message.ShowPromptDialog(
                                    this.Services().Localize["AccountPaymentCustomerNumberRequiredTitle"],
                                    this.Services().Localize["AccountPaymentCustomerNumberRequiredMessage"],
                                    () => { return; });

                                hasValidAccountNumber = await _orderWorkflowService.ValidateAccountNumberAndPrepareQuestions(accountNumber, customerNumber);
								if (!hasValidAccountNumber)
								{
									await this.Services().Message.ShowMessage(
										this.Services().Localize["Error_AccountPaymentTitle"],
										this.Services().Localize["Error_AccountPaymentMessage"]);
									return;
								}

								await _orderWorkflowService.SetAccountNumber(accountNumber, customerNumber);
							}

							var questions = await _orderWorkflowService.GetAccountPaymentQuestions();
							if ((questions != null) && (questions.Length > 0))
							{
								((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Initial;
								ShowViewModel<InitializeOrderForAccountPaymentViewModel>();
							}
							else
							{
								await ConfirmOrderAndGoToBookingStatus();
							}
						}
						else
						{
							await ConfirmOrderAndGoToBookingStatus();
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
                                    Guid.TryParse(e.Parameter, out pendingOrderId);

								this.Services().Message.ShowMessage(title, this.Services().Localize["Error" + e.Message],
									this.Services().Localize["View"], async () =>
									{
										var orderInfos = await GetOrderInfos(pendingOrderId);

										var parent = ((HomeViewModel)Parent);
										parent.CurrentViewState = HomeViewModelState.Initial;
										parent.AutomaticLocateMeAtPickup.ExecuteIfPossible();

										ShowViewModelAndRemoveFromHistory<BookingStatusViewModel>(new {order = orderInfos.Item1, orderStatus = orderInfos.Item2});
									},
                                        this.Services().Localize["Cancel"], () => {});
                                }
                                break;
                            default:
                                {
                                    if (!Settings.HideCallDispatchButton)
                                    {
                                        this.Services().Message.ShowMessage(title, e.Message,
                                            "Call", () => _phone.MakePhoneCall(Settings.TaxiHail.ApplicationName, Settings.DefaultPhoneNumber),
                                            "Cancel", () => { });
                                    }
                                    else
                                    {
                                        this.Services().Message.ShowMessage(title, e.Message);
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

		private async Task ConfirmOrderAndGoToBookingStatus()
		{
			using (this.Services().Message.ShowProgress())
			{
				var result = await _orderWorkflowService.ConfirmOrder();
				this.Services().Analytics.LogEvent("Book");
				GotoBookingStatus(result);
			}
		}


        public ICommand BookLater
        {
            get
            {
                return this.GetCommand(async () =>
                {
					Action onValidated = () => ((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.PickDate;
					await PrevalidatePickupAndDestinationRequired(onValidated);
                });
            }
        }

		private async Task PrevalidatePickupAndDestinationRequired(Action onValidated)
		{
			try
			{
				await _orderWorkflowService.ValidatePickupAndDestination();
				onValidated.Invoke();
			}
			catch (OrderValidationException e)
			{
				switch (e.Error)
				{
					case OrderValidationError.OpenDestinationSelection:
						// not really an error, but we stop the command from proceeding at this point
						return;
					case OrderValidationError.PickupAddressRequired:
						ResetToInitialState.ExecuteIfPossible();
						this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfo"]);
						return;
					case OrderValidationError.DestinationAddressRequired:
						ResetToInitialState.ExecuteIfPossible();
						this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfoWhenDestinationIsRequired"]);
						return;
				}
			}
			catch(Exception e)
			{
				Logger.LogError(e);
				ResetToInitialState.ExecuteIfPossible();
			}
		}

        public ICommand Edit
        {
            get
            {
                return this.GetCommand(() =>
                {
					this.Services().Analytics.LogEvent("EditOrderSettingsTapped");
	                ((HomeViewModel) Parent).CurrentViewState = HomeViewModelState.Edit;
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
	                ((HomeViewModel) Parent).CurrentViewState = HomeViewModelState.Initial;
                });
            }
        }

        public ICommand ResetToInitialState
        {
            get
            {
                return this.GetCommand(() =>
                {
					((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Initial;
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

        public string BookButtonText
        {
            get
            {
                return Settings.UseSingleButtonForNowAndLaterBooking || IsManualRidelinqEnabled
                    ? this.Services().Localize["HomeView_BookTaxi"]
                    : this.Services().Localize["BookItButton"];
            }
        }

        public ICommand Book
        {
            get
            {
                return this.GetCommand(async () =>
                {
					// popup
					if ((Settings.UseSingleButtonForNowAndLaterBooking || IsManualRidelinqEnabled) 
						&& !Settings.DisableFutureBooking && !Settings.DisableImmediateBooking)
                    {
						//We need to show the Book A Taxi popup.
						Action onValidated = () => ((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.BookATaxi;
						await PrevalidatePickupAndDestinationRequired(onValidated);
                    }
					// immediate booking
					else if (!Settings.DisableImmediateBooking)
					{
						SetPickupDateAndReviewOrder.ExecuteIfPossible();
					}
					// future booking
					else if (!Settings.DisableFutureBooking)
					{
						Action onValidated = () => ((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.PickDate;
						await PrevalidatePickupAndDestinationRequired(onValidated);
					}
                });
            }
        }

        public ICommand ManualPairingRideLinq
        {
            get
            {
                return this.GetCommand(() =>
                {
                    var localize = this.Services().Localize;

                    if (_accountService.CurrentAccount.DefaultCreditCard == null
						|| _accountService.CurrentAccount.DefaultCreditCard.IsDeactivated)
                    {
                        this.Services().Message.ShowMessage(
                            localize["ErrorCreatingOrderTitle"],
                            localize["ManualRideLinqNoCardOnFile"]);
                        return;
                    }

                    ShowViewModel<ManualPairingForRideLinqViewModel>();
                });
            }
        }

	    private async Task ShowFareEstimateAlertDialogIfNecessary()
        {
            if (await _orderWorkflowService.ShouldWarnAboutEstimate())
            {
                this.Services().Message.ShowMessage(
					this.Services().Localize["WarningEstimateTitle"], 
					this.Services().Localize["WarningEstimate"],
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
						((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Initial;
						ShowViewModel<CreditCardAddViewModel>(new { showInstructions = true });
                    },
                    this.Services().Localize["Cancel"],
                    () => ((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Initial);
            }
        }

        private async Task PreValidateOrder()
        {
            var validationInfo = await _orderWorkflowService.ValidateOrder();
            if (validationInfo.HasError)
            {
	            this.Services().Message.ShowMessage(
		            this.Services().Localize["ErrorCreatingOrderTitle"], validationInfo.Message,
		            () => ((HomeViewModel) Parent).CurrentViewState = HomeViewModelState.Initial);
            }
            else
            {
                if (validationInfo.HasWarning)
                {
                    this.Services().Message.ShowMessage(
                        this.Services().Localize["WarningTitle"], validationInfo.Message,
                        this.Services().Localize["Continue"], () => _orderWorkflowService.ConfirmValidationOrder(),
                        this.Services().Localize["Cancel"], () => ((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Initial);
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
                IBSStatusId = string.Empty,
                OrderId = pendingOrderId,
                Status = OrderStatus.Unknown,
                VehicleLatitude = null,
                VehicleLongitude = null
            };

            return Tuple.Create(order.ToJson(), orderStatus.ToJson());
        }
    }
}

