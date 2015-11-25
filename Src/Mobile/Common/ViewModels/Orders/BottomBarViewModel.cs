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
using System.ComponentModel;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Extensions;
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

			if (!Settings.HideDestination)
			{
				Observe(_orderWorkflowService.GetAndObserveIsDestinationModeOpened(),
					isDestinationModeOpened => EstimateSelected = isDestinationModeOpened);
			}

            if (Settings.PromotionEnabled)
            {
                Observe(ObserveIsPromoCodeApplied(), isPromoCodeApplied => IsPromoCodeActive = isPromoCodeApplied);
            }

			RefreshAppBarViewState(HomeViewModelState.Initial);

            Observe(_orderWorkflowService.GetAndObserveOrderValidationResult(), OrderValidated);

			Observe(_orderWorkflowService.GetAndObserveCanExecuteBookingOperation(), canExecuteBookOperation => CanExecuteBookOperation = canExecuteBookOperation);
        }

		public override void Start()
		{
			base.Start();
			Observe(ObserveHomeViewModelState(), RefreshAppBarViewState);
		}

		private void RefreshAppBarViewState(HomeViewModelState state)
		{
			if (state == HomeViewModelState.PickDate || state == HomeViewModelState.BookATaxi)
			{
				// These states don't affect visibility
				return;
			}

			HideOrderButtons = !(state == HomeViewModelState.Initial && !IsManualRidelinqEnabled);
			HideReviewButtons = state != HomeViewModelState.Review;
			HideEditButtons = state != HomeViewModelState.Edit;
			HideManualRideLinqButtons = !(state == HomeViewModelState.Initial && IsManualRidelinqEnabled);
			HideAirportButtons = state != HomeViewModelState.AirportDetails;
		}

		private IObservable<HomeViewModelState> ObserveHomeViewModelState()
		{
			return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
					h => Parent.PropertyChanged += h,
					h => Parent.PropertyChanged -= h
				)
				.Where(args => args.EventArgs.PropertyName.Equals("CurrentViewState"))
				.Select(_ => ((HomeViewModel) Parent).CurrentViewState)
				.DistinctUntilChanged();
		}

        public async void CheckManualRideLinqEnabledAsync()
        {
            try
            {
                var settings = await _paymentService.GetPaymentSettings();

                IsManualRidelinqEnabled = settings.PaymentMode == PaymentMethod.RideLinqCmt
                                           && settings.CmtPaymentSettings.IsManualRidelinqCheckInEnabled;

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
				if (_estimateSelected != value)
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

				RefreshAppBarViewState(ParentViewModel.CurrentViewState);

                RaisePropertyChanged();
                RaisePropertyChanged(() => BookButtonText);
            }
        }

		bool _bookButtonHidden = true;

		public bool BookButtonHidden
		{
			get { return _bookButtonHidden; }
			set
			{
				_bookButtonHidden = value;
				RaisePropertyChanged(() => BookButtonHidden);
			}
		}

		public HomeViewModel ParentViewModel
		{
			get
			{
				return (HomeViewModel)Parent;
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

		#region iOS Bindings
		private bool _hideManualRideLinqButtons;
		public bool HideManualRideLinqButtons
		{
			get { return _hideManualRideLinqButtons; }
			set
			{
				if (_hideManualRideLinqButtons != value)
				{
					_hideManualRideLinqButtons = value;
					RaisePropertyChanged();
				}
			}
		}

		private bool _hideOrderButtons;
		public bool HideOrderButtons
		{
			get { return _hideOrderButtons; }
			set
			{
				if (_hideOrderButtons != value)
				{
					_hideOrderButtons = value;
					RaisePropertyChanged();
				}
			}
		}

		private bool _hideReviewButtons;
		public bool HideReviewButtons
		{
			get { return _hideReviewButtons; }
			set
			{
				if (_hideReviewButtons != value)
				{
					_hideReviewButtons = value;
					RaisePropertyChanged();
				}
			}
		}

		private bool _hideEditButtons;
		public bool HideEditButtons
		{
			get { return _hideEditButtons; }
			set
			{
				if (_hideEditButtons != value)
				{
					_hideEditButtons = value;
					RaisePropertyChanged();
				}
			}
		}

		private bool _hideAirportButtons;
		public bool HideAirportButtons
		{
			get { return _hideAirportButtons; }
			set
			{
				if (_hideAirportButtons != value)
				{
					_hideAirportButtons = value;
					RaisePropertyChanged();
				}
			}
		}

		#endregion

        private void OrderValidated(OrderValidationResult orderValidationResult)
        {
            _orderValidationResult = orderValidationResult;
            IsFutureBookingDisabled = Settings.DisableFutureBooking 
				|| (orderValidationResult != null && orderValidationResult.DisableFutureBooking) 
                || Settings.UseSingleButtonForNowAndLaterBooking;

			Book.RaiseCanExecuteChangedIfPossible();
			BookLater.RaiseCanExecuteChangedIfPossible();
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
						await Task.WhenAll(
							_orderWorkflowService.ValidatePickupAndDestination(),
							_orderWorkflowService.ValidatePickupTime(),
							_orderWorkflowService.ValidateNumberOfPassengers(null)
						);
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

					await ReviewOrderDetails();
				});
			}
		}

		private bool _canExecuteBookOperation;

		/// <summary>
		/// WARNING: DO NOT BIND THIS PROPERTY TO AN ENABLE, THIS SHOULD BE USED IN A CanExecute.
		/// </summary>
		/// <value><c>true</c> if this instance can execute book operation; otherwise, <c>false</c>.</value>
		public bool CanExecuteBookOperation
		{
			get
			{
				return _canExecuteBookOperation;
			}
			set
			{
				_canExecuteBookOperation = value;
				Book.RaiseCanExecuteChangedIfPossible();
			}
		}

        public ICommand SetPickupDateAndReturnToAirport
        {
            get
            {
                return this.GetCommand<DateTime?>(async date =>
                {
                    // since it can take some time, recalculate estimate for date only if 
                    // last calculated estimate was not for now
                    if (date != null)
                    {
                        await _orderWorkflowService.SetPickupDate(date);
                    }

                    try
                    {
	                    await Task.WhenAll(
		                    _orderWorkflowService.ValidatePickupAndDestination(),
							_orderWorkflowService.ValidatePickupTime(),
							_orderWorkflowService.ValidateNumberOfPassengers(null));
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
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                        ResetToInitialState.ExecuteIfPossible();
                        return;
                    }

					await ValidateOrderDetails();
					ParentViewModel.CurrentViewState = HomeViewModelState.AirportDetails;
                });
            }
        }

        public async Task ReviewOrderDetails()
	    {
			await ValidateOrderDetails();
			ParentViewModel.CurrentViewState = HomeViewModelState.Review;
	    }

		private async Task ValidateOrderDetails()
		{
			using (this.Services().Message.ShowProgress())
			{
				await Task.WhenAll(
					_orderWorkflowService.ResetOrderSettings(),
					ShowFareEstimateAlertDialogIfNecessary(),
					ValidateCardOnFile(),
					PreValidateOrder()
				);
			}
		}

        public ICommand ConfirmOrderCommand
        {
            get
            {
                return this.GetCommand(async () =>
                {
					var tipIncentive = await _orderWorkflowService.GetTipIncentive();

					if(tipIncentive.HasValue && tipIncentive > 0)
					{
						await this.Services().Message.ShowMessage(
							this.Services().Localize["DriverBonusWarningTitle"], 
							this.Services().Localize["DriverBonusWarningMessage"],
							this.Services().Localize["YesButton"], 
							async () => 
							{ 
								await ConfirmOrder(); 
							},
							this.Services().Localize["NoButton"], 
							() => { }); 
					}
					else
					{
						await ConfirmOrder();
					}
				});
            }
        }

		private async Task ConfirmOrder()
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

							if (questions != null
								&& questions.Length > 0
								&& questions[0].Question.HasValue())
					{
						ParentViewModel.CurrentViewState = HomeViewModelState.Initial;								
								// Navigate to Q&A page

						ShowSubViewModel<InitializeOrderForAccountPaymentViewModel, Tuple<Order, OrderStatusDetail>>(
							null, 
							result => ParentViewModel.GotoBookingStatus(result.Item1, result.Item2)
						);
					}
					else
					{
								// Skip Q&A page and confirm order
						await ConfirmOrderAndGoToBookingStatus();
					}
				}
				else
				{
							// Skip Q&A page and confirm order
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

									ParentViewModel.BookingStatus.StartBookingStatus(orderInfos.Item1, orderInfos.Item2);

									ParentViewModel.CurrentViewState = HomeViewModelState.BookingStatus;
									ParentViewModel.AutomaticLocateMeAtPickup.ExecuteIfPossible();
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
		}

		private async Task ConfirmOrderAndGoToBookingStatus()
		{
			using (this.Services().Message.ShowProgress())
			{
				var result = await _orderWorkflowService.ConfirmOrder();
				this.Services().Analytics.LogEvent("Book");
				ParentViewModel.GotoBookingStatus(result.Item1, result.Item2);
			}
		}

	    private ICommand _bookLater;
        public ICommand BookLater
        {
            get
            {
	            if (_bookLater != null)
	            {
		            return _bookLater;
	            }

				return _bookLater = this.GetCommand(async () =>
                {
					Action onValidated = () => ParentViewModel.CurrentViewState = HomeViewModelState.PickDate;
					await PrevalidatePickupAndDestinationRequired(onValidated);
                }, CanProceedToBook);
            }
        }
			
		private bool CanProceedToBook()
		{
			return CanExecuteBookOperation;
		}

        public ICommand BookAirportLater
        {
            get
            {
				return this.GetCommand(() =>
				{
					ParentViewModel.CurrentViewState = HomeViewModelState.AirportPickDate;
				});
            }
        }

		private async Task<bool> PrevalidatePickupAndDestinationRequired(Action onValidated)
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
						return false;
					case OrderValidationError.PickupAddressRequired:
						ResetToInitialState.ExecuteIfPossible();
						this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfo"]);
						return false;
					case OrderValidationError.DestinationAddressRequired:
						ResetToInitialState.ExecuteIfPossible();
						this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfoWhenDestinationIsRequired"]);
						return false;
				}
			}
			catch(Exception e)
			{
				Logger.LogError(e);
				ResetToInitialState.ExecuteIfPossible();
				return false;
			}
			return true;
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
                    _orderWorkflowService.SetNoteToDriver(string.Empty);
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
					ParentViewModel.CurrentViewState = HomeViewModelState.Initial;
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

        public ICommand CancelAirport
        {
            get
            {
                return this.GetCommand(() =>
                {
                    // set pickup date to null to reset the estimate for now and not the possible date set by book later
                    _orderWorkflowService.SetPickupDate(null);
                    _orderWorkflowService.CancelRebookOrder();
                    _orderWorkflowService.SetNoteToDriver(string.Empty);
					ParentViewModel.CurrentViewState = HomeViewModelState.Initial;
                });
            }
        }

        private ICommand _nextAirport;
        public ICommand NextAirport
        {
            get { return _nextAirport; }
            set
            {
                if (value != _nextAirport)
                {
                    _nextAirport = value;
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

	    public ICommand CreateOrder
	    {
		    get
		    {
			    return this.GetCommand<DateTime?>(async dateTime =>
			    {
					var pickupAddress = await _orderWorkflowService.GetAndObservePickupAddress().Take(1);
					// airport mode immediate booking
				    if (!Settings.DisableImmediateBooking
				        && Settings.FlightStats.UseAirportDetails
				        && pickupAddress != null
				        && pickupAddress.AddressLocationType == AddressLocationType.Airport
				        && pickupAddress.PlaceId.HasValue())
				    {
						SetPickupDateAndReturnToAirport.ExecuteIfPossible(dateTime);
				    }
				    // immediate booking
				    else
					{
						SetPickupDateAndReviewOrder.ExecuteIfPossible(dateTime);
					}
			    });
		    }
	    }

	    private ICommand _book;
        public ICommand Book
        {
            get
            {
	            if (_book != null)
	            {
		            return _book;
	            }

                return _book = this.GetCommand(async () =>
                {
					// popup
					if ((Settings.UseSingleButtonForNowAndLaterBooking || IsManualRidelinqEnabled) 
						&& !Settings.DisableFutureBooking && !Settings.DisableImmediateBooking)
                    {
						Action onValidated = () => ParentViewModel.CurrentViewState = HomeViewModelState.BookATaxi;
						var success = await PrevalidatePickupAndDestinationRequired(onValidated);

						if (success)
						{
                        	this.Services().Message.ShowMessage(null, this.Services().Localize["BookATaxi_Message"],
                            this.Services().Localize["Cancel"], () => ResetToInitialState.ExecuteIfPossible(),
                            this.Services().Localize["Now"], () => CreateOrder.ExecuteIfPossible(),
                            this.Services().Localize["BookItLaterButton"], () => BookLater.ExecuteIfPossible());
						}

	                    return;
                    }
					if(!Settings.DisableImmediateBooking)
					{
						CreateOrder.ExecuteIfPossible();
					}
					// future booking
					else if (!Settings.DisableFutureBooking)
					{
						Action onValidated = () => ParentViewModel.CurrentViewState = HomeViewModelState.PickDate;
						await PrevalidatePickupAndDestinationRequired(onValidated);
					}
                }, CanProceedToBook);
            }
        }

        public ICommand ManualPairingRideLinq
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    var localize = this.Services().Localize;

                    if (_accountService.CurrentAccount.DefaultCreditCard == null)
                    {
                        this.Services().Message.ShowMessage(localize["ErrorCreatingOrderTitle"], localize["ManualRideLinqNoCardOnFile"]);
                        return;
                    }

                    if (_accountService.CurrentAccount.DefaultCreditCard.IsDeactivated)
                    {
                        this.Services().Message.ShowMessage(
                            this.Services().Localize["View_Overdue"],
                            this.Services().Localize["Overdue_OutstandingPaymentExists"],
                            this.Services().Localize["OkButtonText"], async () =>
                            {
                                OverduePayment overduePayment;

                                using (this.Services().Message.ShowProgress())
                                {
                                    overduePayment = await _paymentService.GetOverduePayment();
                                }

                                ShowViewModel<OverduePaymentViewModel>(new
                                {
                                    overduePayment = overduePayment.ToJson()
                                });
                            },
                            this.Services().Localize["Cancel"],
                            () => {});

                        return;
                    }

	                var homeViewModel = (HomeViewModel) Parent;
					ShowSubViewModel<ManualPairingForRideLinqViewModel, OrderManualRideLinqDetail>(null, orderManualPairingDetails => homeViewModel.GoToManualRideLinq(orderManualPairingDetails));
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
						ParentViewModel.CurrentViewState = HomeViewModelState.Initial;
						ShowViewModel<CreditCardAddViewModel>(new { showInstructions = true });
                    },
                    this.Services().Localize["Cancel"],
					() => ParentViewModel.CurrentViewState = HomeViewModelState.Initial);
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
						this.Services().Localize["Cancel"], () => ParentViewModel.CurrentViewState = HomeViewModelState.Initial);
                }
                else
                {
                    _orderWorkflowService.ConfirmValidationOrder();
                }
            }
        }

        private async Task<Tuple<Order, OrderStatusDetail>> GetOrderInfos(Guid pendingOrderId)
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

            return Tuple.Create(order, orderStatus);
        }
    }
}

