using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using System.Globalization;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class RideSummaryViewModel: BaseViewModel
	{
		public RideSummaryViewModel (string order, string orderStatus)
		{			
			Order = order.FromJson<Order> ();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

            IsRatingButtonShown = Config.GetSetting( "Client.RatingEnabled", false );  

		}
        public override void Start(bool firstStart = false)
        {
            base.Start(firstStart);
            FirePropertyChanged(() => IsPayButtonShown);
            FirePropertyChanged(() => IsResendConfirmationButtonShown);
            FirePropertyChanged(() => IsSendReceiptButtonShown);
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
                FirePropertyChanged(() => ReceiptSent);
            }
        }

		private Order Order {get; set;}
		OrderStatusDetail OrderStatus{ get; set;}

		public bool IsPayButtonShown{
			get{
				var setting = ConfigurationManager.GetPaymentSettings ();
				var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
                return isPayEnabled && !PaymentService.GetPaymentFromCache(Order.Id).HasValue;
			}
		}

	    public bool IsResendConfirmationButtonShown
	    {
	        get
	        {
                var setting = ConfigurationManager.GetPaymentSettings();
                var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
                return isPayEnabled && PaymentService.GetPaymentFromCache(Order.Id).HasValue;
	        }
	    }

		public bool IsSendReceiptButtonShown {
			get{
                var sendReceiptAvailable =  ConfigurationManager.GetSetting("Client.SendReceiptAvailable",false);
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
				FirePropertyChanged (() => IsRatingButtonShown);
			}
		}

		public IMvxCommand SendReceiptCommand {
			get {
				return new AsyncCommand (() =>
				{
					BookingService.SendReceipt (Order.Id);
                    ReceiptSent = true;
				});
			}
		}

		public IMvxCommand NavigateToRatingPage {
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
        public IMvxCommand ResendConfirmationCommand {
            get {
                return new AsyncCommand (() =>
                {
                    MessageService.ShowMessage( "Confirmation",
                        Resources.GetString("ConfirmationOfPaymentSent"));
                    PaymentService.ResendConfirmationToDriver( Order.Id );

                });
            }
        }

		public IMvxCommand PayCommand {
			get {
				return new AsyncCommand (() => RequestNavigate<ConfirmCarNumberViewModel>(
				    new 
				    { 
				        order = Order.ToJson(),
				        orderStatus = OrderStatus.ToJson()
				    }, false, MvxRequestedBy.UserAction));
			}
		}
	}
}

