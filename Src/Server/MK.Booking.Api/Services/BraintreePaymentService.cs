using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braintree;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Services
{
    public class BraintreePaymentService : Service
    {
        readonly IConfigurationManager _configurationManager;
        public BraintreePaymentService(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;

            var settings = ((PaymentSetting)_configurationManager.GetPaymentSettings()).BraintreeSettings;

            var env = Braintree.Environment.SANDBOX;
            if (!settings.IsSandBox)
            {
                env = Braintree.Environment.PRODUCTION;
            }

            Client = new BraintreeGateway
            {
                Environment = env,
                MerchantId = settings.MerchantId,
                PublicKey = settings.PublicKey,
                PrivateKey = settings.PrivateKey,
            };


        }
        
        public static BraintreeGateway Client { get; set; }

        /*
         * Braintree Creds:
         * U: apcurium
         * P: apcurium5200!
         */
        
        public TokenizedCreditCardResponse Post(TokenizeCreditCardBraintreeRequest tokenizeRequest)
        {
            var request = new CustomerRequest
            {
                CreditCard = new CreditCardRequest()
                {
                    Number = tokenizeRequest.EncryptedCreditCardNumber,
                    ExpirationDate = tokenizeRequest.EncryptedExpirationDate,
                    CVV = tokenizeRequest.EncryptedCvv,
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

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardBraintreeRequest request)
        {
            Client.CreditCard.Delete(request.CardToken);
            return new DeleteTokenizedCreditcardResponse()
                {
                    IsSuccessfull = true,
                    Message = "Success"
                };
        }

        public PreAuthorizePaymentResponse Post(PreAuthorizePaymentBraintreeRequest preAuthorizeRequest)
        {
            var request = new TransactionRequest
            {
                Amount = preAuthorizeRequest.Amount,
                PaymentMethodToken = preAuthorizeRequest.CardToken,
                OrderId = preAuthorizeRequest.OrderNumber,

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


        public CommitPreauthorizedPaymentResponse Post(CommitPreauthorizedPaymentBraintreeRequest request)
        {
            var result = Client.Transaction.SubmitForSettlement(request.TransactionId);

            return new CommitPreauthorizedPaymentResponse()
            {
                IsSuccessfull = result.IsSuccess(),
                Message = "Success"
            };
        }

    }
}
