using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree
{
    [RouteDescription("/payments/braintree/tokenize", "POST")]
    public class TokenizeCreditCardBraintreeRequest : IReturn<TokenizedCreditCardResponse>
    {
        public string EncryptedCreditCardNumber { get; set; }

        public string EncryptedExpirationDate { get; set; }

        public string EncryptedCvv { get; set; }

        /// <summary>
        ///  Used for tokenization from javascript API
        /// </summary>

        public string PaymentMethodNonce { get; set; }
    }
}