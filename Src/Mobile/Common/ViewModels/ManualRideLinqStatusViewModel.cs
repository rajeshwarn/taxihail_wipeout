using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using System.Reactive.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ManualRideLinqStatusViewModel : PageViewModel
    {
        private readonly IBookingService _bookingService;
		private readonly IAccountService _accountService;

		// In seconds
        private int _refreshInterval = 5;

		public ManualRideLinqStatusViewModel(IBookingService bookingService, IAccountService accountService)
        {
            _bookingService = bookingService;
			_accountService = accountService;
        }

        public void Init(string orderManualRideLinqDetail)
        {
			var orderManualRideLinq = JsonSerializer.DeserializeFromString<OrderManualRideLinqDetail>(orderManualRideLinqDetail);

			Medallion = orderManualRideLinq.Medallion;
			OrderId = orderManualRideLinq.OrderId;
		}

		public override void Start()
		{
			base.Start();

            _refreshInterval = Settings.OrderStatus.ClientPollingInterval;

			Observable.Timer(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(_refreshInterval))
                .SelectMany((_, cancellationToken) => RefreshDetails(cancellationToken))
				.Where(orderDetails => orderDetails.EndTime.HasValue)
				.Take(1) // trigger only once
				.Subscribe(
					ToRideSummary,
					Logger.LogError)
				.DisposeWith (Subscriptions);
		}

		private async Task<OrderManualRideLinqDetail> RefreshDetails(CancellationToken token)
		{
            return await _bookingService.GetTripInfoFromManualRideLinq(OrderId);
		}

		private Guid OrderId { get; set; }

        private string _medallion;
        public string Medallion
        {
            get
            {
                return _medallion;
            }
            set
            {
                _medallion = value;
                RaisePropertyChanged();
            }
        }

		public string Email
		{
			get
			{
				return _accountService.CurrentAccount.Email;
			}
		}

		public string PaymentInfo
		{
			get
			{
				return string.Format(this.Services().Localize["ManualRideLinqStatus_Payment"],
					_accountService.CurrentAccount.DefaultCreditCard.CreditCardCompany,
					_accountService.CurrentAccount.DefaultCreditCard.Last4Digits,
                    _accountService.CurrentAccount.DefaultTipPercent ?? Settings.DefaultTipPercentage);
			}
		}
	
		private void ToRideSummary(OrderManualRideLinqDetail orderManualRideLinqDetail)
		{
            _bookingService.ClearLastOrder();

			var orderSummary = orderManualRideLinqDetail.ToJson();

			ShowViewModelAndRemoveFromHistory<ManualRideLinqSummaryViewModel>(new {orderManualRideLinqDetail = orderSummary});
		}

        public ICommand EditAutoTipCommand
        {
            get
            {
                return this.GetCommand(() =>
                {
					ShowViewModel<EditAutoTipViewModel>();
                });
            }
        }

        public ICommand UnpairFromRideLinq
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    try
                    {
                        var shouldUnpair = new TaskCompletionSource<bool>();

                        this.Services().Message.ShowMessage(
                            this.Services().Localize["WarningTitle"],
                            this.Services().Localize["UnpairWarningMessage"],
                            this.Services().Localize["UnpairWarningCancelButton"],
                            () => shouldUnpair.SetResult(true),
                            this.Services().Localize["Cancel"], 
                            () => shouldUnpair.SetResult(false));

                        if (await shouldUnpair.Task)
                        {
                            using (this.Services().Message.ShowProgress())
                            {
                                await _bookingService.UnpairFromManualRideLinq(OrderId);

                                _bookingService.ClearLastOrder();

                                ShowViewModelAndRemoveFromHistory<HomeViewModel>(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
                            } 
                        }
                    }
                    catch (Exception)
                    {
                        this.Services().Message.ShowMessage(
                                        this.Services().Localize["ManualPairingForRideLinQ_Error_Title"],
                                        this.Services().Localize["ManualUnPairingForRideLinQ_Error_Message"]);
                    }
                });
            }
        }
    }
}