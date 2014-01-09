using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using System.Globalization;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class RideSummaryViewModel: BaseViewModel
	{
		public RideSummaryViewModel (string order, string orderStatus)
		{			
			Order = order.FromJson<Order> ();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

            IsRatingButtonShown = this.Services().Config.GetSetting( "Client.RatingEnabled", false );  

		}
        public override void Start(bool firstStart = false)
        {
            base.Start(firstStart);
			RaisePropertyChanged(() => IsPayButtonShown);
			RaisePropertyChanged(() => IsResendConfirmationButtonShown);
			RaisePropertyChanged(() => IsSendReceiptButtonShown);
        }

		public string ThankYouTitle {
			get{
				return Str.ThankYouTitle;
			}
		}

		public string ThankYouMessage {
			get{
				return Str.ThankYouMessage;
			}
		}

	    private bool _receiptSent;
        public bool ReceiptSent 
        {
            get { return _receiptSent; }
            set {
                _receiptSent = value;
				RaisePropertyChanged();
            }
        }

		private Order Order {get; set;}
		OrderStatusDetail OrderStatus{ get; set;}

		public bool IsPayButtonShown{
			get{
                var setting = this.Services().Config.GetPaymentSettings();
				var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
				return isPayEnabled && setting.PaymentMode != PaymentMethod.RideLinqCmt && !this.Services().Payment.GetPaymentFromCache(Order.Id).HasValue; // TODO not sure about this
			}
		}

	    public bool IsResendConfirmationButtonShown
	    {
	        get
	        {
                var setting = this.Services().Config.GetPaymentSettings();
                var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
                return isPayEnabled && setting.PaymentMode != PaymentMethod.RideLinqCmt && this.Services().Payment.GetPaymentFromCache(Order.Id).HasValue;
	        }
	    }

		public bool IsSendReceiptButtonShown {
			get{
                var sendReceiptAvailable = this.Services().Config.GetSetting("Client.SendReceiptAvailable", false);
                return (OrderStatus != null) && OrderStatus.FareAvailable && sendReceiptAvailable;
             
			}
		}

		bool _isRatingButtonShow;		
		public bool IsRatingButtonShown {
			get { 
				return _isRatingButtonShow;
			}
			set { 
				_isRatingButtonShow = value;
				RaisePropertyChanged ();
			}
		}

        public AsyncCommand SendReceiptCommand
        {
			get {
				return new AsyncCommand (() =>
				{
                    this.Services().Booking.SendReceipt(Order.Id);
                    ReceiptSent = true;
				});
			}
		}

        public AsyncCommand NavigateToRatingPage
        {
			get {
				return new AsyncCommand (() => RequestSubNavigate<BookRatingViewModel, OrderRated> (new 
				{
				    orderId = Order.Id.ToString (), 
				    canRate = true.ToString (CultureInfo.InvariantCulture), 
				    isFromStatus = true.ToString (CultureInfo.InvariantCulture)
				}.ToStringDictionary(),_=>{
				                              IsRatingButtonShown = false;
				}));
			}
		}
        public AsyncCommand ResendConfirmationCommand
        {
            get {
                return new AsyncCommand (() =>
                {
                    this.Services().Message.ShowMessage("Confirmation",
                        this.Services().Resources.GetString("ConfirmationOfPaymentSent"));
                    this.Services().Payment.ResendConfirmationToDriver(Order.Id);

                });
            }
        }

        public AsyncCommand PayCommand
        {
			get {
				return new AsyncCommand (() => RequestNavigate<ConfirmCarNumberViewModel>(
				    new 
				    { 
				        order = Order.ToJson(),
				        orderStatus = OrderStatus.ToJson()
				    }));
			}
		}
	}
}

