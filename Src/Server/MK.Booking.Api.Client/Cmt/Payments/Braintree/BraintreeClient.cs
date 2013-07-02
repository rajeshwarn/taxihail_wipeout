using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braintree;
using BraintreeEncryption.Library;
using MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.BrainTree
{
    public class BraintreeClient : IPaymentServiceClient
    {
        public static BraintreeGateway Client
        {
            get
            {
                return new BraintreeGateway
                {
                    Environment = Braintree.Environment.SANDBOX,
                    MerchantId = "grpc45xgw92ns363",
                    PublicKey = "kw827xn4rby289z7",
                    PrivateKey = "519c90ba4a227a74da354533d38db54b"
                };
            }
        }

        private const string CLIENT_KEY = "MIIBCgKCAQEAvEQ88pEhx4MyX4B/JmW1/iJcvMEkwV3iKPV2ikCt2BldJmFVpPQpzlTXY5MJ0i8uxMjj1lsvVKgugHGQGPa8qUWUTeO/WmyTsYAO/CeK/9yJq+p4u3IZbSslyknkl7voDEJejXQAi+rVITxLoN5y/j+APk1addiPHb7nqK/ldmLgVP9PNO3uuKq0xWppBRT+7h7RHGT+SUhqiFGhFhv3E593Xr+U1iBaJtj5avLcmp4XO5As+F5qpVLwYEVjpsJhYG5AQn2YirBXp2OlUOmxLa3DNOddwal9sYtWvSdoVaGQ570UJ0Vzhf6qiMtcUa82N0rq2nAxxlMYBpRSX8usmwIDAQAB";

        public TokenizeResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            var braintree = new BraintreeEncrypter(CLIENT_KEY);
            
            var request = new CustomerRequest
            {

                CreditCard = new CreditCardRequest()
                {
                    CVV = braintree.Encrypt("4111111111111111"),
                    ExpirationDate = braintree.Encrypt("111"),
                    Number = braintree.Encrypt("01/2013")
                }
            };

            var result = Client.Customer.Create(request);

            var customer = result.Target;
            Console.WriteLine("Success!: " + customer.Id);
            var cc = customer.CreditCards.First();
            return new TokenizeResponse()
                {
                    CardOnFileToken = cc.Token,
                    CardType = cc.CardType.ToString(),
                    LastFour = cc.LastFour,
                    ResponseCode = result.IsSuccess() ? 0 : 1,
                    ResponseMessage = result.Message,
                };
            
        }

        public TokenizeDeleteResponse ForgetTokenizedCard(string cardToken)
        {
            throw new NotImplementedException();
        }

        public string PreAuthorize(string cardToken, string encryptedCvv, double amount, string orderNumber)
        {
            var request = new TransactionRequest
            {
                Amount = (decimal)amount,
                PaymentMethodToken = cardToken,
                
                CreditCard = new TransactionCreditCardRequest()
                {
                     CVV = encryptedCvv,
                },
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = false
                }
            };

            var result = Client.Transaction.Sale(request);

            return result.Target.OrderId;

        }

        public bool CommitPreAuthorized(string transactionId, string orderNumber)
        {
            var result = Client.Transaction.SubmitForSettlement(transactionId);

            return result.IsSuccess();
        }
    }
}
