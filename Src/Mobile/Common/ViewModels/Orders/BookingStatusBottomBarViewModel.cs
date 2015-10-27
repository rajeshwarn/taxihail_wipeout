using System;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class BookingStatusBottomBarViewModel : BaseViewModel
	{
		private readonly IPhoneService _phoneService;
		private readonly IBookingService _bookingService;
		private readonly IPaymentService _paymentService;
		private readonly IAccountService _accountService;

		private bool _orderWasUnpaired;
		private bool _isCmtRideLinq;

		public BookingStatusBottomBarViewModel(IPhoneService phoneService, IBookingService bookingService, IPaymentService paymentService, IAccountService accountService)
		{
			_phoneService = phoneService;
			_bookingService = bookingService;
			_paymentService = paymentService;
			_accountService = accountService;

			GetIsCmtRideLinq();
		}

		private async void GetIsCmtRideLinq()
		{
			try
			{
				var paymentSettings = await _paymentService.GetPaymentSettings();

				_isCmtRideLinq = paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt;

				RaisePropertyChanged(() => ButtonEditTipLabel);
			}
			catch(Exception ex) 
			{
				Logger.LogError(ex);	
			}
		}

		public BookingStatusViewModel ParentViewModel
		{
			get { return (BookingStatusViewModel) Parent; }
		}

		public void PrepareForNewOrder()
		{
			IsCancelButtonVisible = false;
			CanEditAutoTip = false;
			IsUnpairButtonVisible = false;
			_orderWasUnpaired = false;
			_currentTip = null;
		}

		private async void UpdateActionsPossibleOnOrder()
		{
			try
			{

				IsCancelButtonVisible = ParentViewModel.ManualRideLinqDetail == null
					&& ParentViewModel.OrderStatusDetail != null
					&& _bookingService.IsOrderCancellable(ParentViewModel.OrderStatusDetail);

				var arePassengersOnBoard = ParentViewModel.ManualRideLinqDetail != null
					|| ParentViewModel.OrderStatusDetail.SelectOrDefault(orderStatus => orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded));

				var isUnPairPossible = ParentViewModel.ManualRideLinqDetail == null
					&& ParentViewModel.OrderStatusDetail != null
					&& DateTime.UtcNow <= ParentViewModel.OrderStatusDetail.UnpairingTimeOut;

				if (arePassengersOnBoard && IsUsingPaymentMethodOnFile())
				{
					var isPaired = ParentViewModel.ManualRideLinqDetail != null
						|| await _bookingService.IsPaired(ParentViewModel.Order.SelectOrDefault(order => order.Id, Guid.Empty));

					CanEditAutoTip = isPaired;
                    IsUnpairButtonVisible = isPaired && isUnPairPossible && !_orderWasUnpaired;
				}
				else
				{
					IsUnpairButtonVisible = false;
					CanEditAutoTip = false;
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex);
			}
		}

		private bool IsUsingPaymentMethodOnFile()
		{
			if (ParentViewModel.ManualRideLinqDetail == null && ParentViewModel.Order == null)
			{
				return false;
			}

			return ParentViewModel.ManualRideLinqDetail != null
				|| ParentViewModel.Order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
				|| ParentViewModel.Order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id;
		}

		public void NotifyBookingStatusAppbarChanged()
		{
			UpdateActionsPossibleOnOrder();
			RaisePropertyChanged(() => IsCallCompanyHidden);
		}

		public bool IsCallCompanyHidden
		{
			get { return Settings.HideCallDispatchButton || ParentViewModel.ManualRideLinqDetail != null; }
		}

		public ICommand CancelOrder
		{
			get
			{
				return this.GetCommand(() =>
				{
					if ((ParentViewModel.OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Done) || (ParentViewModel.OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded))
					{
						this.Services().Message.ShowMessage(this.Services().Localize["CannotCancelOrderTitle"], this.Services().Localize["CannotCancelOrderMessage"]);
						return;
					}

					var confirmationMessage = Settings.WarnForFeesOnCancel
						&& (VehicleStatuses.CanCancelOrderStatus.Contains(ParentViewModel.OrderStatusDetail.IBSStatusId))
						? string.Format(
							this.Services().Localize["StatusConfirmCancelRideAndWarnForCancellationFees"],
							Settings.TaxiHail.ApplicationName)
						: this.Services().Localize["StatusConfirmCancelRide"];

					this.Services().Message.ShowMessage(
						string.Empty,
						confirmationMessage,
						this.Services().Localize["YesButton"],
						async () =>
						{
							bool isSuccess;
							using (this.Services().Message.ShowProgress())
							{
								isSuccess = await _bookingService.CancelOrder(ParentViewModel.Order.Id);
							}
							if (isSuccess)
							{
								this.Services().Analytics.LogEvent("BookCancelled");
								_bookingService.ClearLastOrder();
								ParentViewModel.ReturnToInitialState();
							}
							else
							{
								this.Services().Message.ShowMessage(this.Services().Localize["StatusConfirmCancelRideErrorTitle"], this.Services().Localize["StatusConfirmCancelRideError"]).FireAndForget();
							}
						},
						this.Services().Localize["NoButton"], () => { });
				});
			}
		}

		public ICommand CallCompany
		{
			get
			{
				var isLuxury = ParentViewModel.Order != null && ParentViewModel.Order.Settings.ServiceType == ServiceType.Luxury;

				return this.GetCommand(() =>
					this.Services().Message.ShowMessage(string.Empty, isLuxury ? Settings.DefaultPhoneNumberForLuxuryDisplay : Settings.DefaultPhoneNumberDisplay,
						this.Services().Localize["CallButton"], () => _phoneService.Call(isLuxury ? Settings.DefaultPhoneNumberForLuxury : Settings.DefaultPhoneNumber),
                        this.Services().Localize["Cancel"], () => { }));
			}
		}

		public ICommand Unpair
		{
			get
			{
				return this.GetCommand(() =>
				{
					var message = ParentViewModel.Order.PromoCode.HasValue()
						? this.Services().Localize["UnpairWarningMessageWithPromo"]
						: this.Services().Localize["UnpairWarningMessage"];


					this.Services().Message.ShowMessage(
						this.Services().Localize["WarningTitle"],
						message,
                        this.Services().Localize["UnpairWarningOkButton"],
						async () =>
						{
							try
							{
								using (this.Services().Message.ShowProgress())
								{
									var unpairingResponse = await _paymentService.Unpair(ParentViewModel.Order.Id);
		
								    if (unpairingResponse.IsSuccessful)
								    {
                                        _orderWasUnpaired = true;
								        IsUnpairButtonVisible = false;

									    var paymentSettings = await _paymentService.GetPaymentSettings();
									    if (paymentSettings.CancelOrderOnUnpair)
									    {
										    // Cancel order
                                            var isSuccess = await _bookingService.CancelOrder(ParentViewModel.Order.Id);
                                            if (isSuccess)
                                            {
                                                this.Services().Analytics.LogEvent("BookCancelled");
                                                _bookingService.ClearLastOrder();
                                                ParentViewModel.ReturnToInitialState();
                                            }
                                            else
                                            {
                                                this.Services().Message.ShowMessage(this.Services().Localize["StatusConfirmCancelRideErrorTitle"], this.Services().Localize["StatusConfirmCancelRideError"]).FireAndForget();
                                            }
									    }
									    else
									    {
										    NotifyBookingStatusAppbarChanged();
									    }
								    }
								    else
								    {
									    this.Services().Message
										    .ShowMessage(this.Services().Localize["CmtRideLinqErrorTitle"], this.Services().Localize["UnpairErrorMessage"])
										    .FireAndForget();
								    }
                                }
							}
							catch (Exception ex)
							{
								Logger.LogError(ex);
							}
						},
                        this.Services().Localize["UnpairWarningCancelButton"], () => { });
				});
			}
		}

		public string ButtonEditTipLabel
		{
			get
			{
				return _isCmtRideLinq ? this.Services().Localize["StatusEditAutoPaymentButton"] : this.Services().Localize["StatusEditAutoTipButton"];
			}
		}

		private bool _isUnpairButtonVisible;
		public bool IsUnpairButtonVisible
		{
			get { return _isUnpairButtonVisible; }
			set
			{
				_isUnpairButtonVisible = value;
				RaisePropertyChanged();
			}
		}

		bool _isCancelButtonVisible;
		public bool IsCancelButtonVisible
		{
			get { return _isCancelButtonVisible; }
			set
			{
				_isCancelButtonVisible = value;
				RaisePropertyChanged();
			}
		}

		private bool _canEditAutoTip;
		public bool CanEditAutoTip
		{
			get { return _canEditAutoTip; }
			set
			{
				if (_canEditAutoTip != value)
				{
					_canEditAutoTip = value;
					RaisePropertyChanged();
				}
			}
		}

		private int? _currentTip;
		private int GetTip()
		{
			return _currentTip ?? _accountService.CurrentAccount.DefaultTipPercent ?? Settings.DefaultTipPercentage;
		}

		public ICommand EditAutoTipCommand
		{
			get
			{
				return this.GetCommand(() =>
				{
					ShowSubViewModel<EditAutoTipViewModel, int>(new { tip = GetTip()}, tip => _currentTip = tip);
				});
			}
		}
	}
}