using System;
using BraintreeEncryption.Library;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Client.Payments.Braintree
{
    public class BraintreeServiceClient : BaseServiceClient, IPaymentServiceClient
    {
        
        public BraintreeServiceClient(string url, string sessionId):base(url, sessionId)
        {
            //todo client side get
            ClientKey =
                "MIIBCgKCAQEAoj1SlyPOlcbsemg8jNsZkgBjYspWd8goY7Dyf7IAMi68s6lX1QkEZ5iRVDZW8WANT46bbXFSZcerULT9nUx9lMWP8rrcv+i7Qy9LGjj2Zys7D0b98mzcdOoYiAg1GKDWjDW49mEtzlRbTSpgETvzCt3tonqAgZKt5E68P8SkQX+lem7N06KwaW5jFJRYNkYc5cNTyo3pMoCGnWJvBLMuW1CV4dXWxvTQU8dgnug6Y/i0AVJGJtnH2iaqk40+w6mifzpjDI6luTFw9ZXI7wlXitrQDcE0a3Dqx896IdvqP7PNLi6zVGM2DtOojO5f5KIXiFcBkepYnDkzJ33L1iwTKQIDAQAB";

        }

        protected string ClientKey { get; set; }


        public TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            var braintree = new BraintreeEncrypter(ClientKey);
            var encryptedNumber = braintree.Encrypt(creditCardNumber);
            var encryptedExpirationDate = braintree.Encrypt(expiryDate.ToString("MM/yyyy"));
            var encryptedCvv = braintree.Encrypt(cvv);

            return Client.Post(new TokenizeCreditCardBraintreeRequest()
            {
                EncryptedCreditCardNumber = encryptedNumber,
                EncryptedExpirationDate = encryptedExpirationDate,
                EncryptedCvv = encryptedCvv,
            });
        }

        public DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken)
        {
            return Client.Delete(new DeleteTokenizedCreditcardBraintreeRequest()
                {
                    CardToken = cardToken,
                });
        }

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, string orderNumber)
        {
            return Client.Post(new PreAuthorizePaymentBraintreeRequest()
                {
                    Amount = (decimal) amount,
                    CardToken = cardToken,
                    OrderNumber = orderNumber,
                });

        }

        public CommitPreauthorizedPaymentResponse CommitPreAuthorized(string transactionId, string orderNumber)
        {
            return Client.Post(new CommitPreauthorizedPaymentBraintreeRequest()
                {
                    TransactionId = transactionId,
                });
        }
    }
}
