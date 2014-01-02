using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using System.Linq;

namespace apcurium.MK.Booking.Mobile
{
    public class ConfirmCmtPairingViewModel : BaseViewModel, IMvxServiceConsumer<IAccountService>, IMvxServiceConsumer<IPaymentService>
	{
        private readonly IAccountService _accountService;
        private readonly IPaymentService _paymentService;
        public ConfirmCmtPairingViewModel(string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();                        
            _accountService  = this.GetService<IAccountService>();
            _paymentService = this.GetService<IPaymentService>();

            if (PaymentPreferences.SelectedCreditCard == null)
            {
                RequestSubNavigate<CreditCardsListViewModel, Guid>(null, result =>
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

        public bool _isConfirmEnabled = false;
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

        public string _cardNumber = "";
        public string CardNumber
        {
            get
            {
                if (_cardNumber != "")
                {
                    return _cardNumber;
                }
                else return "None";
            }
        }

        public string TipAmountInPercent
        {
            get
            {
                var tipAmount = PaymentPreferences.Tip;
                return tipAmount.ToString() + "%";                
            }
        }        

		public IMvxCommand ConfirmPayment
		{
			get {
				return GetCommand (() =>
				{                      
                    // MKTAXI-1161 - Change the Pair status with account service
                    // TODO: Get tip in percent
                    var payment = _paymentService;                    
                    
                    PairingResponse pairingResponse = payment.Pair(Order.Id, PaymentPreferences.SelectedCreditCard.Token, PaymentPreferences.Tip, 0d);                    
                    
                    // TODO: Should we handle [pairingResponse.IsSuccessfull = false] scenario?                         
                    RequestNavigate<BookingStatusViewModel>(new
                    {
                        order = Order.ToJson(),
                        orderStatus = OrderStatus.ToJson()
                    });                   
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

