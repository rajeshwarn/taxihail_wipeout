﻿using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree
{
    [Authenticate]
    [Route("/payments/braintree/tokenize", "POST")]
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