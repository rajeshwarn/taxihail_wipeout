using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt
{
    public class CmtRideLinqChangePaymentViewModel : BaseSubViewModel<PaymentDetailsViewModel>
    {
        public CmtRideLinqChangePaymentViewModel(string messageId, string order, string orderStatus): base(messageId)
		{
            Order = order.FromJson<Order>();
            _paymentPreferences = PaymentPreferences; // Workaround
            // TODO: Currently, no saved manual tip amount in the profile. So, always 0.            
            PlaceholderAmount = "0";
		}

        private string _placeholderAmount { get; set; }
        public string PlaceholderAmount
        {
            get
            {                
                return _placeholderAmount;
            }

            set
            {
                _placeholderAmount = value;
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

                    _paymentPreferences.Tip = (int)account.DefaultTipPercent;                    
                    _paymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), paymentInformation);
                    _paymentPreferences.SelectedCreditCardId = (Guid)account.DefaultCreditCard;
                    _paymentPreferences.LoadCreditCards();

                }
                return _paymentPreferences;
            }
        }

		Order Order { get; set; }
		OrderStatusDetail OrderStatus { get; set; }

        public string TipAmountInPercent
        {
            get
            {
                var tipAmount = PaymentPreferences.Tip;
                return tipAmount + "%";                
            }
        }        

        public AsyncCommand SomeCommand
        {
            get
            {
                return GetCommand(() =>
                {
                    
                });
            }
        }

        public AsyncCommand SomeCancel
        {
            get
            {
                return GetCommand(() =>
                {
                    // Return original values
                });
            }
        }
	}
}

