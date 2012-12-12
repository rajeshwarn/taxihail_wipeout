using System;
using System.Collections.ObjectModel;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;


namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PaymentDetailsViewModel: BaseSubViewModel<string>,
        IMvxServiceConsumer<IAccountService>
    {
        private IAccountService _accountService;
        public PaymentDetailsViewModel (string messageId): base(messageId)
        {
            _accountService = this.GetService<IAccountService>();
            LoadCreditCards();
        }

        private readonly ObservableCollection<CreditCardDetails> _creditCards = new ObservableCollection<CreditCardDetails>();
        public ObservableCollection<CreditCardDetails> CreditCards {
            get {
                return _creditCards;
            }
        }

        private Task LoadCreditCards ()
        {
            return Task.Factory.StartNew(() => {
                
                var cards = _accountService.GetCreditCards();
                CreditCards.Clear();
                foreach (var card in cards) {
                    CreditCards.Add(card);
                }
            });
        }
    }
}

