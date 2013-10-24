using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Braintree
{
    [Authenticate]
    [Route("/payments/braintree/tokenize","POST")]
    public class TokenizeCreditCardBraintreeRequest : IReturn<TokenizedCreditCardResponse>
    {
        public string EncryptedCreditCardNumber { get; set; }

        public string EncryptedExpirationDate { get; set; }
 
        public string EncryptedCvv { get; set; }
    }
}
