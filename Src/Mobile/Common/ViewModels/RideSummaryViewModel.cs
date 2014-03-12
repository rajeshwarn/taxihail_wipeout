using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class RideSummaryViewModel: PageViewModel
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IPaymentService _paymentService;
		private readonly IBookingService _bookingService;

		public RideSummaryViewModel(IOrderWorkflowService orderWorkflowService,
			IPaymentService paymentService,
			IBookingService bookingService)
		{
			_orderWorkflowService = orderWorkflowService;
			_paymentService = paymentService;
			_bookingService = bookingService;
		}

		public void Init(string order, string orderStatus)
		{			
			Order = order.FromJson<Order> ();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

			IsRatingButtonShown = Settings.RatingEnabled;  
		}

        public override void OnViewStarted(bool firstStart = false)
        {
            base.OnViewStarted(firstStart);
			RaisePropertyChanged(() => IsPayButtonShown);
			RaisePropertyChanged(() => IsResendConfirmationButtonShown);
			RaisePropertyChanged(() => IsSendReceiptButtonShown);
        }

	    private bool _receiptSent;
        public bool ReceiptSent 
        {
            get { return _receiptSent; }
            set 
			{
                _receiptSent = value;
				RaisePropertyChanged();
            }
        }

		private Order Order { get; set; }
		private OrderStatusDetail OrderStatus { get; set;}

		public bool IsPayButtonShown
		{
			get
			{
				var setting = _paymentService.GetPaymentSettings();
				var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
				return isPayEnabled && setting.PaymentMode != PaymentMethod.RideLinqCmt && !_paymentService.GetPaymentFromCache(Order.Id).HasValue; // TODO not sure about this
			}
		}

	    public bool IsResendConfirmationButtonShown
	    {
	        get
	        {
				var setting = _paymentService.GetPaymentSettings();
                var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
				return isPayEnabled && setting.PaymentMode != PaymentMethod.RideLinqCmt && _paymentService.GetPaymentFromCache(Order.Id).HasValue;
	        }
	    }

		public bool IsSendReceiptButtonShown 
		{
			get
			{
				var fareIsAvailable = OrderStatus.FareAvailable || _paymentService.GetPaymentFromCache(Order.Id).HasValue;
				return (OrderStatus != null) && fareIsAvailable && Settings.SendReceiptAvailable;
			}
		}

		bool _isRatingButtonShow;		
		public bool IsRatingButtonShown 
		{
			get 
			{ 
				return _isRatingButtonShow;
			}
			set 
			{ 
				_isRatingButtonShow = value;
				RaisePropertyChanged ();
			}
		}

		public ICommand SendReceiptCommand
        {
			get {
				return this.GetCommand(() =>
				{
					_bookingService.SendReceipt(Order.Id);
                    ReceiptSent = true;
				});
			}
		}

		public ICommand NavigateToRatingPage
        {
			get {
				return this.GetCommand(() => 
					ShowSubViewModel<BookRatingViewModel, OrderRated>(
						new 
						{
							orderId = Order.Id, 
							canRate = true, 
						}, _ => IsRatingButtonShown = false));
			}
		}

		public ICommand ResendConfirmationCommand
        {
            get {
				return this.GetCommand(() =>
                {
					this.Services().Message.ShowMessage("Confirmation", this.Services().Localize["ConfirmationOfPaymentSent"]);
					_paymentService.ResendConfirmationToDriver(Order.Id);
                });
            }
        }

		public ICommand PayCommand
        {
			get {
				return this.GetCommand(() => 
					ShowViewModel<ConfirmCarNumberViewModel>(
					    new 
					    { 
					        order = Order.ToJson(),
					        orderStatus = OrderStatus.ToJson()
					    }));
			}
		}

		public ICommand PrepareNewOrder
		{
			get
			{
				return this.GetCommand(async () =>{
					var address = await _orderWorkflowService.SetAddressToUserLocation();
					if(address.HasValidCoordinate())
					{
						ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude));
					}
				});
			}
		}
	}
}

