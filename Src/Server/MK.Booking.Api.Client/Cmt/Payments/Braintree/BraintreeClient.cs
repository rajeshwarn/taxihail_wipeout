using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braintree;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.BrainTree
{
    public class BraintreeClient
    {
        public static BraintreeGateway BraintreeGatewaySandBox
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

        public static TransactionCreditCardRequest DummyCreditCard
        {
            get
            {
                return new TransactionCreditCardRequest
                {
                    Number = "4111111111111111",
                    ExpirationMonth = "05",
                    ExpirationYear = "2012"
                };
            }
        }


        public void CreateTransaction(string number, string cvv, string month, string year, decimal amount)
        {
            var request = new TransactionRequest
            {
                Amount = amount,
                CreditCard = new TransactionCreditCardRequest
                {
                    Number = number,
                    CVV = cvv,
                    ExpirationMonth = month,
                    ExpirationYear = year
                },
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            var result = BraintreeGatewaySandBox.Transaction.Sale(request);

            if (result.IsSuccess())
            {
                var transaction = result.Target;
                var transactionId = transaction.Id;
            }
            else
            {
                var error = result.Message;
            }


        }
    }
}
