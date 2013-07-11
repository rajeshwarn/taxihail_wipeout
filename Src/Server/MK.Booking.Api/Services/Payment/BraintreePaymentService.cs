using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Braintree;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Booking.Commands;

namespace apcurium.MK.Booking.Api.Services
{
    public class BraintreePaymentService : Service
    {
        readonly ICommandBus _commandBus;
        readonly ICreditCardPaymentDao _dao;
        readonly IConfigurationManager _configurationManager;
        public BraintreePaymentService(ICommandBus commandBus, ICreditCardPaymentDao dao, IConfigurationManager configurationManager)
        {
            _commandBus = commandBus;
            _dao = dao;
            _configurationManager = configurationManager;

            var settings = ((ServerPaymentSettings)_configurationManager.GetPaymentSettings()).BraintreeServerSettings;

            var env = Braintree.Environment.SANDBOX;
            if (!settings.IsSandbox)
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
            bool isSuccessful = result.IsSuccess();
            if (isSuccessful)
            {
                _commandBus.Send(new InitiateCreditCardPayment
                {
                    PaymentId = Guid.NewGuid(),
                    Amount = preAuthorizeRequest.Amount,
                    TransactionId = result.Target.Id,
                });
            }

            return new PreAuthorizePaymentResponse()
            {
                TransactionId = result.Target.Id,
                IsSuccessfull = isSuccessful,
                Message = "Success",
            };

        }


        public CommitPreauthorizedPaymentResponse Post(CommitPreauthorizedPaymentBraintreeRequest request)
        {
            var payment = _dao.FindByTransactionId(request.TransactionId);
            if (payment == null) throw new HttpError(HttpStatusCode.NotFound, "Payment not found");


            var result = Client.Transaction.SubmitForSettlement(request.TransactionId);

            bool isSuccessful = result.IsSuccess();
            if (isSuccessful)
            {
                _commandBus.Send(new CaptureCreditCardPayment
                {
                    PaymentId = payment.PaymentId,
                });
            }

            return new CommitPreauthorizedPaymentResponse()
            {
                IsSuccessfull = isSuccessful,
                Message = "Success"
            };
        }

    }
}
