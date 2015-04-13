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
        private OrderManualRideLinqDetail _orderManualRideLinqDetail;

		// In seconds
        private const int RefreshInterval = 5;

        public ManualRideLinqStatusViewModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public void Init(string orderManualRideLinqDetail)
        {
            OrderManualRideLinqDetail = JsonSerializer.DeserializeFromString<OrderManualRideLinqDetail>(orderManualRideLinqDetail);
        }

		public override void Start()
		{
			base.Start();

			Observable.Timer(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(RefreshInterval))
                .SelectMany((_, cancellationToken) => RefreshDetails(cancellationToken))
				.Where(orderDetails => orderDetails.EndTime.HasValue)
				//TODO: Find out why OnViewUnloaded is not called and remove the Take(1) when that is fixed.
				//.Take(1) //Workaround because OnViewUnloaded is not called.
				.Subscribe(
					ToRideSummary,
					ex => Logger.LogError(ex))
				.DisposeWith (Subscriptions);
		}

		private async Task<OrderManualRideLinqDetail> RefreshDetails(CancellationToken token)
		{
            return await _bookingService.ManualRideGetTripInfo(OrderManualRideLinqDetail.OrderId);
		}


        public OrderManualRideLinqDetail OrderManualRideLinqDetail
        {
            get { return _orderManualRideLinqDetail; }
            set
            {
                if (_orderManualRideLinqDetail != value)
                {
                    _orderManualRideLinqDetail = value;
                    RaisePropertyChanged();
                }
            }
        }

		private void ToRideSummary(OrderManualRideLinqDetail orderManualRideLinqDetail)
		{
            _bookingService.ClearLastOrder();

			var orderSummary = orderManualRideLinqDetail.ToJson();

			ShowViewModelAndRemoveFromHistory<ManualRideLinqSummaryViewModel>(new {orderManualRideLinqDetail = orderSummary});
		}

        public ICommand UnpairFromRideLinq
        {
            get
            {
                return this.GetCommand(() =>
                {
                    try
                    {
                        this.Services().Message.ShowMessage(
                            this.Services().Localize["WarningTitle"],
                            this.Services().Localize["UnpairWarningMessage"],
                            this.Services().Localize["UnpairWarningCancelButton"],
                            async () =>
                            {
                                using (this.Services().Message.ShowProgress())
                                {
                                    await _bookingService.ManualRideLinqUnpair(_orderManualRideLinqDetail.OrderId);

                                    _bookingService.ClearLastOrder();

                                    ShowViewModelAndRemoveFromHistory<HomeViewModel>(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
                                }
                            },
                            this.Services().Localize["Cancel"], () => { });
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