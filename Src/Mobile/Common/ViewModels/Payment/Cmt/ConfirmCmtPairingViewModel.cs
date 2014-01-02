using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile
{
    public class ConfirmCmtPairingViewModel : BaseViewModel
	{
        private readonly IAccountService _accountService;
        private readonly IBookingService _bookingService;
        public ConfirmCmtPairingViewModel(string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();                        
		}

        private PaymentDetailsViewModel _paymentPreferences;
        public PaymentDetailsViewModel PaymentPreferences
        {
            get
            {
                if (_paymentPreferences == null)
                {
                    var account = _accountService.CurrentAccount;
                    var paymentInformation = new PaymentInformation
                    {
                        CreditCardId = account.DefaultCreditCard,
                        TipPercent = account.DefaultTipPercent,
                    };

                    _paymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), paymentInformation);
                }
                return _paymentPreferences;
            }
        }

		Order Order { get; set; }
		OrderStatusDetail OrderStatus { get; set; }
        
        public string CarNumber{
			get{
				return OrderStatus.VehicleNumber;
			}
		}

        public string CardNumber
        {
            get
            {                
                //var selectedCard = PaymentPreferences.SelectedCreditCard;                
                //return selectedCard.CreditCardCompany + " " + selectedCard.Last4Digits;
                return "Visa 4556";
            }
        }

        public string TipAmountInPercent
        {
            get
            {
                //var tipAmount = PaymentPreferences.Tip;
                //return tipAmount.ToString() + "%";
                return "3%";
            }
        }        

		public IMvxCommand ConfirmPayment
		{
			get {
				return GetCommand (() =>
				{                      
                    // MKTAXI-1161 - Change the Pair status with account service
                    //var booking = _bookingService.
				});
			}
		}

        public IMvxCommand ChangePaymentInfo
        {
            get
            {
                return GetCommand(() =>
                {
                    //RequestSubNavigate<PaymentCmtViewModel, object>(
                    //new
                    //{
                    //    order = Order.ToJson(),
                    //    orderStatus = OrderStatus.ToJson(),
                    //}.ToStringDictionary(),
                    //_ =>
                    //{
                    //});

                    //RequestClose(this);
                });
            }
        }

        public IMvxCommand CancelPayment
        {
            get
            {
                return GetCommand(() =>
                {
                    //RequestNavigate<PaymentViewModel, object>( ...............                    
                });
            }
        }


	}
}

