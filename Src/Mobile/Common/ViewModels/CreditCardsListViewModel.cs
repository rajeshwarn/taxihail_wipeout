using System;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CreditCardsListViewModel : BaseViewModel
    {
        public CreditCardsListViewModel ()
        {
        }

        public IMvxCommand NavigateToAddCreditCard
        {
            get
            {
                return GetCommand(() => RequestNavigate<CreditCardAddViewModel>());
            }
        }
    }
}

