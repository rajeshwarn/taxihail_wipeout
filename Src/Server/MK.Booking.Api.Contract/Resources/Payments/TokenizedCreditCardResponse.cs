using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    public class TokenizedCreditCardResponse : BasePaymentResponse
    {
        public string LastFour { get; set; }
        public string CardType { get; set; }

        public string CardOnFileToken { get; set; }

        public string BraintreeAccountId { get; set; }
    }
}