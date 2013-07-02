using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braintree;
using BraintreeEncryption.Library;
using MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using apcurium.MK.Booking.Api.Client.Responses;
using apcurium.MK.Common.Configuration.Impl;
using Environment = Braintree.Environment;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.BrainTree
{
    public class BraintreeServiceClient : IPaymentServiceClient
    {
        public static BraintreeGateway Client { get; set; }

        /*
         * Braintree Creds:
         * U: apcurium
         * P: apcurium5200!
         */
        
        public BraintreeServiceClient(BraintreeSettings settings)
        {
            var env = Environment.SANDBOX;
            if (!settings.IsSandBox)
            {
                env = Environment.PRODUCTION;
            }

            Client = new BraintreeGateway
            {
                Environment = env,
                MerchantId = settings.MerchantId,
                PublicKey = settings.PublicKey,
                PrivateKey = settings.PrivateKey,
            };

            ClientKey = settings.ClientKey;

        }

        protected string ClientKey { get; set; }


        public TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string encryptedCvv)
        {
            var braintree = new BraintreeEncrypter(ClientKey);
            
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
