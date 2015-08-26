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

		// In seconds
        private int _refreshInterval = 5;

        public ManualRideLinqStatusViewModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public void Init(string orderManualRideLinqDetail)
        {
			var orderManualRideLinq = JsonSerializer.DeserializeFromString<OrderManualRideLinqDetail>(orderManualRideLinqDetail);
        	
			DriverId = orderManualRideLinq.DriverId.ToString();
			PairingCode = orderManualRideLinq.PairingCode;
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

		private string _pairingCode;
		public string PairingCode
		{
			get
			{
				return _pairingCode;
			}
			set
			{
				_pairingCode = value;
				RaisePropertyChanged();
			}
		}

		private string _driverId;
		public string DriverId
		{
			get
			{
				return _driverId;
			}
			set
			{
				_driverId = value;
				RaisePropertyChanged();
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

                                ShowViewModelAndRemoveFromHistory<HomeViewModel>();
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