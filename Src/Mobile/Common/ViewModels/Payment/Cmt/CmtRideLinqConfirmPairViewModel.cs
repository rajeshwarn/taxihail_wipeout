using System;
using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt
{
    public class CmtRideLinqConfirmPairViewModel : BaseViewModel
	{
        public CmtRideLinqConfirmPairViewModel(string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

            if (PaymentPreferences.SelectedCreditCard == null)
            {
                RequestSubNavigate<CreditCardsListViewModel, Guid>(
                    null, result =>
                {
                    if (result != default(Guid))
                    {
                        PaymentPreferences.SelectedCreditCardId = result;
                        PaymentPreferences.LoadCreditCards();
                        RefreshCreditCards();
                    }
                });
            }
            else
            {
                RefreshCreditCards();
            }
		}

        private PaymentDetailsViewModel _paymentPreferences;
        public PaymentDetailsViewModel PaymentPreferences
        {
            get
            {
                if (_paymentPreferences == null)
                {
                    var account = this.Services().Account.CurrentAccount;
                    var paymentInformation = new PaymentInformation
                    {
                        CreditCardId = account.DefaultCreditCard,
                        TipPercent = account.DefaultTipPercent,
                    };

                    _paymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), paymentInformation);
                }
                return _paymentPreferences;
            }

            set
            {
                _paymentPreferences = value;
            }
        }

		Order Order { get; set; }
		OrderStatusDetail OrderStatus { get; set; }

        public void RefreshCreditCards()
        {
            var selectedCard = PaymentPreferences.SelectedCreditCard;
            
            if (selectedCard != null)
            {
                IsConfirmEnabled = true;
                _cardNumber = selectedCard.CreditCardCompany + " " + selectedCard.Last4Digits;
            }
            else
            {
                IsConfirmEnabled = false;
                _cardNumber = "";
            }

            FirePropertyChanged(() => IsConfirmEnabled);
            FirePropertyChanged(() => CardNumber);
        }

        private bool _isConfirmEnabled;
        public bool IsConfirmEnabled
        {
            get
            {
                return _isConfirmEnabled;
            }   
         
            set
            {
                _isConfirmEnabled = value;
            }
        }

        public string CarNumber{
			get{
				return OrderStatus.VehicleNumber;
			}
		}

        private string _cardNumber = "";
        public string CardNumber
        {
            get
            {
                if (_cardNumber != "")
                {
                    return _cardNumber;
                }
                return "None";
            }
        }

        public string TipAmountInPercent
        {
            get
            {
                var tipAmount = PaymentPreferences.Tip;
                return tipAmount.ToString(CultureInfo.InvariantCulture) + "%";                
            }
        }        

		public AsyncCommand ConfirmPayment
		{
			get {
				return GetCommand (() =>
				{                      
                    // MKTAXI-1161 - Change the Pair status with account service
                    // TODO: Get tip in percent
                    var payment = this.Services().Payment;                    
                    
                    PairingResponse pairingResponse = payment.Pair(Order.Id, PaymentPreferences.SelectedCreditCard.Token, PaymentPreferences.Tip, 0d);                    
                    
                    this.Services().Cache.Set("CmtRideLinqPairState" + Order.Id.ToString(), pairingResponse.IsSuccessfull ? "success" : "failed");
                        
                    RequestNavigate<BookingStatusViewModel>(new
                    {
                        order = Order.ToJson(),
                        orderStatus = OrderStatus.ToJson()
                    });                   
				});
			}
		}

// ReSharper disable once UnusedMember.Global
        public AsyncCommand ChangePaymentInfo
        {
            get
            {
                return GetCommand(() =>
                {
                    RequestSubNavigate<CmtRideLinqChangePaymentViewModel, PaymentDetailsViewModel>(
                        new
                        {
                            order = Order.ToJson(),
                            orderStatus = OrderStatus.ToJson(),
                        }.ToStringDictionary(), result =>
                    {                                                
                            PaymentPreferences = result;
                            PaymentPreferences.LoadCreditCards();
                            RefreshCreditCards();                        
                    });
                });
            }
        }

        public AsyncCommand CancelPayment
        {
            get
            {
                return GetCommand(() =>
                {
                    this.Services().Cache.Set("CmtRideLinqPairState" + Order.Id.ToString(), "canceled");

                    RequestNavigate<BookingStatusViewModel>(new
                    {
                        order = Order.ToJson(),
                        orderStatus = OrderStatus.ToJson()
                    });                   
                });
            }
        }
	}
}

