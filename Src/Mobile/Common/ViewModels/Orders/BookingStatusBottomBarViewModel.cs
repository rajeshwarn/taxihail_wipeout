using System;
using System.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Common;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class BookingStatusBottomBarViewModel : BaseViewModel
	{
		private readonly IPhoneService _phoneService;
		private readonly IBookingService _bookingService;
		private readonly IPaymentService _paymentService;

		public BookingStatusBottomBarViewModel(IPhoneService phoneService, IBookingService bookingService, IPaymentService paymentService)
		{
			_phoneService = phoneService;
			_bookingService = bookingService;
			_paymentService = paymentService;
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

		public BookingStatusViewModel ParentViewModel
		{
			get { return (BookingStatusViewModel) Parent; }
		}

		public async void UpdateActionsPossibleOnOrder(string statusId)
		{
			IsCancelButtonVisible = _bookingService.IsOrderCancellable(statusId);

			var arePassengersOnBoard = ParentViewModel.OrderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded);
			var isUnPairPossible = DateTime.UtcNow <= ParentViewModel.OrderStatusDetail.UnpairingTimeOut;

			if (arePassengersOnBoard
				&& isUnPairPossible
				&& (ParentViewModel.Order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
				|| ParentViewModel.Order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id))
			{
				IsUnpairButtonVisible = await _bookingService.IsPaired(ParentViewModel.Order.Id);
			}
			else
			{
				IsUnpairButtonVisible = false;
			}
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
								ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser = true });
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
				return this.GetCommand(() =>
					this.Services().Message.ShowMessage(string.Empty,
						Settings.DefaultPhoneNumberDisplay,
						this.Services().Localize["CallButton"],
						() => _phoneService.Call(Settings.DefaultPhoneNumber),
						this.Services().Localize["Cancel"],
						() => { }));
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
						this.Services().Localize["UnpairWarningCancelButton"],
						async () =>
						{
							try
							{
								var response = await _paymentService.Unpair(ParentViewModel.Order.Id);

								if (response.IsSuccessful)
								{
									ParentViewModel.RefreshStatus();
								}
								else
								{
									this.Services().Message.ShowMessage(this.Services().Localize["CmtRideLinqErrorTitle"], this.Services().Localize["UnpairErrorMessage"]);
								}
							}
							catch (Exception ex)
							{
								Logger.LogError(ex);
							}

						},
						this.Services().Localize["Cancel"], () => { });
				});
			}
		}

		bool _isUnpairButtonVisible;
		public bool IsUnpairButtonVisible
		{
			get { return _isUnpairButtonVisible; }
			set
			{
				_isUnpairButtonVisible = value;
				RaisePropertyChanged();
			}
		}
	}
}