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
using System.Reactive;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ManualRideLinqStatusViewModel : PageViewModel
    {
        private readonly IBookingService _bookingService;
        private OrderManualRideLinqDetail _orderManualRideLinqDetail;

		// In seconds
		private readonly int _refreshPeriod = 5;

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

			Observable.Timer(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(_refreshPeriod))
                .SelectMany((_, ct) => RefreshDetails(ct))
				.Where(orderDetails => orderDetails.EndTime.HasValue)
				.Subscribe(
					ToRideSummary,
					ex => this.Logger.LogError(ex)
				)
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
                _orderManualRideLinqDetail = value; 
                RaisePropertyChanged();
            }
        }

		private void ToRideSummary(OrderManualRideLinqDetail orderManualRideLinqDetail)
		{
			var orderSummary = orderManualRideLinqDetail.ToJson();

			ShowViewModelAndRemoveFromHistory<ManualRideLinqSummaryViewModel>(new {orderManualRideLinqDetail = orderSummary});

            _bookingService.ClearLastOrder();
		}

        public ICommand UnpairFromRideLinq
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    await _bookingService.ManualRideLinqUnpair(_orderManualRideLinqDetail.OrderId);

                    _bookingService.ClearLastOrder();

                    ShowViewModelAndRemoveFromHistory<HomeViewModel>(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
                });
            }
        }
    }
}