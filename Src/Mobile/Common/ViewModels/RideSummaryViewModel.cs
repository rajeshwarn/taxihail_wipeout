using System;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using System.Globalization;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Configuration.Impl;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class RideSummaryViewModel: BaseViewModel
	{
		public void Init(string order, string orderStatus)
		{			
			Order = order.FromJson<Order> ();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

			IsRatingButtonShown = this.Services().Config.GetSetting("Client.RatingEnabled", false);  
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

		public bool IsSendReceiptButtonShown 
		{
			get
			{
                var sendReceiptAvailable = this.Services().Config.GetSetting("Client.SendReceiptAvailable", false);
                return (OrderStatus != null) && OrderStatus.FareAvailable && sendReceiptAvailable;
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
				return GetCommand(() =>
				{
					this.Services().Booking.SendReceipt(Order.Id);
                    ReceiptSent = true;
				});
			}
		}

		public ICommand NavigateToRatingPage
        {
			get {
				return GetCommand(() => 
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
				return GetCommand(() =>
                {
					this.Services().Message.ShowMessage("Confirmation", this.Services().Localize["ConfirmationOfPaymentSent"]);
                    this.Services().Payment.ResendConfirmationToDriver(Order.Id);
                });
            }
        }

		public ICommand PayCommand
        {
			get {
				return GetCommand(() => 
					ShowViewModel<ConfirmCarNumberViewModel>(
					    new 
					    { 
					        order = Order.ToJson(),
					        orderStatus = OrderStatus.ToJson()
					    }));
			}
		}
	}
}

