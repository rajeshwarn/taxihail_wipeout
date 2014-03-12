using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Extensions;

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

		public bool IsAddNew { get { return CreditCardDetails.CreditCardId.IsNullOrEmpty(); } }
		public bool IsLast { get; set; } //needed for android
        public string Picture { get; set; }
		public ICommand RemoveCreditCards
        {
            get
            {
                return this.GetCommand(() => this.Services().MessengerHub.Publish(new RemoveCreditCard(this, CreditCardDetails.CreditCardId)));
            }
        }
    }
}