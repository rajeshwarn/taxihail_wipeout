using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;

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
        public bool IsAddNew { get; set; }
        public string Picture { get; set; }
        public AsyncCommand RemoveCreditCards
        {
            get
            {
                return GetCommand(() => this.Services().MessengerHub.Publish(new RemoveCreditCard(this, CreditCardDetails.CreditCardId)));
            }
        }
    }
}