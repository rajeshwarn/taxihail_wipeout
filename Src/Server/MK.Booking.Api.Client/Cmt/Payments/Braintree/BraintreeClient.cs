using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braintree;
using BraintreeEncryption.Library;
using MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using Environment = Braintree.Environment;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.BrainTree
{
    public class BraintreeClient : IPaymentServiceClient
    {
        public static BraintreeGateway Client { get; set; }

        /*
         * Braintree Creds:
         * U: apcurium
         * P: apcurium5200!
         */

        private const string CLIENT_KEY = "MIIBCgKCAQEAoj1SlyPOlcbsemg8jNsZkgBjYspWd8goY7Dyf7IAMi68s6lX1QkEZ5iRVDZW8WANT46bbXFSZcerULT9nUx9lMWP8rrcv+i7Qy9LGjj2Zys7D0b98mzcdOoYiAg1GKDWjDW49mEtzlRbTSpgETvzCt3tonqAgZKt5E68P8SkQX+lem7N06KwaW5jFJRYNkYc5cNTyo3pMoCGnWJvBLMuW1CV4dXWxvTQU8dgnug6Y/i0AVJGJtnH2iaqk40+w6mifzpjDI6luTFw9ZXI7wlXitrQDcE0a3Dqx896IdvqP7PNLi6zVGM2DtOojO5f5KIXiFcBkepYnDkzJ33L1iwTKQIDAQAB";
        
        public BraintreeClient(Environment env)
        {
            Client = new BraintreeGateway
            {
                Environment = env,
                MerchantId = "v3kjnzjzhv8z37pq",
                PublicKey = "d268b7by244xnvw9",
                PrivateKey = "92780e4aa457e9269b1910d88ac79d17"
            };
        }


        public TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string encryptedCvv)
        {
            var braintree = new BraintreeEncrypter(CLIENT_KEY);
            
            var request = new CustomerRequest
            {
                CreditCard = new CreditCardRequest()
                {
                    Number = braintree.Encrypt(creditCardNumber),
                    ExpirationDate = braintree.Encrypt(expiryDate.ToString("MM/yyyy")),
                    CVV = braintree.Encrypt(encryptedCvv),
                }
            };

            var result = Client.Customer.Create(request);

            var customer = result.Target;

            var cc = customer.CreditCards.First();
            return new TokenizedCreditCardResponse()
                {
                    CardOnFileToken = cc.Token,
                    CardType = cc.CardType.ToString(),
                    LastFour = cc.LastFour,
                    IsSuccessfull = result.IsSuccess(),
                    Message = result.Message,
                };
            
        }

        public DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken)
        {
            Client.CreditCard.Delete(cardToken);
            return new DeleteTokenizedCreditcardResponse()
                {
                    IsSuccessfull = true,
                    Message = "Success"
                };

        }

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, string orderNumber)
        {
            var request = new TransactionRequest
            {
                Amount = (decimal)amount,
                PaymentMethodToken = cardToken,
       
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = false
                }
            };

            var result = Client.Transaction.Sale(request);

            return new PreAuthorizePaymentResponse()
                {
                    TransactionId = result.Target.Id,
                    IsSuccessfull = result.IsSuccess(),
                    Message = "Success",
                };

        }

        public CommitPreauthoriedPaymentResponse CommitPreAuthorized(string transactionId, string orderNumber)
        {
            var result = Client.Transaction.SubmitForSettlement(transactionId);

            return new CommitPreauthoriedPaymentResponse()
                {
                    IsSuccessfull = result.IsSuccess(),
                    Message = "Success"
                };
        }
    }
}
