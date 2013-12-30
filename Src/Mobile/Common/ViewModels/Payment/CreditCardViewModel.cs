using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class CreditCardViewModel : BaseViewModel
    {
        public CreditCardDetails CreditCardDetails { get; set; }
        public string Last4DigitsWithStars
        {
            get
            {
                if(!IsAddNew)
                {
                    return "\u2022\u2022\u2022\u2022 " + CreditCardDetails.Last4Digits;
                }
                return "";
            }
        }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
        public bool ShowPlusSign { get; set; }
        public bool IsAddNew { get; set; }
        public string Picture { get; set; }
        public IMvxCommand RemoveCreditCards
        {
            get
            {
                return GetCommand(() => this.Services().MessengerHub.Publish(new RemoveCreditCard(this, CreditCardDetails.CreditCardId)));
            }
        }
    }
}