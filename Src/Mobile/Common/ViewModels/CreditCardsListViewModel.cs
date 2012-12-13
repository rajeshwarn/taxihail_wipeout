using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Data;

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
				return GetCommand(() => RequestSubNavigate<CreditCardAddViewModel,CreditCardInfos>(null, newCreditCard => {}));
            }
        }
    }
}

