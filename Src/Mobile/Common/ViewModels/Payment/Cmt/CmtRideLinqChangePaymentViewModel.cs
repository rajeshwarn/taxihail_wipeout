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
    public class CmtRideLinqChangePaymentViewModel : BaseSubViewModel<PaymentDetailsViewModel>, IMvxServiceConsumer<IAccountService>
	{
        private readonly IAccountService _accountService;
        public CmtRideLinqChangePaymentViewModel(string messageId, string order, string orderStatus): base(messageId)
		{
            Order = order.FromJson<Order>();
            _accountService  = this.GetService<IAccountService>();
            _paymentPreferences = PaymentPreferences; // Workaround

            // TODO: Currently, no saved manual tip amount in the profile. So, always 0.            
            PlaceholderAmount = "0";
		}

        public string _placeholderAmount { get; set; }
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
                    var account = _accountService.CurrentAccount;
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
                return tipAmount.ToString() + "%";                
            }
        }        

        public IMvxCommand SomeCommand
        {
            get
            {
                return GetCommand(() =>
                {
                    
                });
            }
        }

        public IMvxCommand SomeCancel
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

