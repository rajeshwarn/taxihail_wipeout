using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Braintree
{
    [Route("/payments/braintree/tokenize","POST")]
    public class TokenizeCreditCardBraintreeRequest : IReturn<TokenizedCreditCardResponse>
    {
        public string EncryptedCreditCardNumber { get; set; }

        public string EncryptedExpirationDate { get; set; }
 
        public string EncryptedCvv { get; set; }
    }
}
