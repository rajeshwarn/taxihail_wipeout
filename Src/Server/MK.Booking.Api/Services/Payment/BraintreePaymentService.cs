using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Braintree;
using BraintreeEncryption.Library;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Booking.Commands;

namespace apcurium.MK.Booking.Api.Services
{
    public class BraintreePaymentService : Service
    {
        readonly ICommandBus _commandBus;
        readonly ICreditCardPaymentDao _dao;
        readonly IOrderDao _orderDao;
        readonly IConfigurationManager _configurationManager;

        public BraintreePaymentService(ICommandBus commandBus, ICreditCardPaymentDao dao, IOrderDao orderDao, IConfigurationManager configurationManager)
        {
            _commandBus = commandBus;
            _dao = dao;
            _orderDao = orderDao;
            _configurationManager = configurationManager;
            
            BraintreeGateway = GetBraintreeGateway(((ServerPaymentSettings)_configurationManager.GetPaymentSettings()).BraintreeServerSettings);
        }

        public static BraintreeGateway GetBraintreeGateway(BraintreeServerSettings settings)
        {
            var env = Braintree.Environment.SANDBOX;
            if (!settings.IsSandbox)
            {
                env = Braintree.Environment.PRODUCTION;
            }

            return new BraintreeGateway
            {
                Environment = env,
                MerchantId = settings.MerchantId,
                PublicKey = settings.PublicKey,
                PrivateKey = settings.PrivateKey,
            };
        }
        public static bool TestClient(BraintreeServerSettings settings, BraintreeClientSettings braintreeClientSettings)
        {
            settings.IsSandbox = true;
            var client = GetBraintreeGateway(settings);

            var dummyCreditCard = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Braintree).Visa;

            var braintree = new BraintreeEncrypter(braintreeClientSettings.ClientKey);
 

            return TokenizedCreditCard(client, new TokenizeCreditCardBraintreeRequest()
                {
                    EncryptedCreditCardNumber = braintree.Encrypt(dummyCreditCard.Number),
                    EncryptedCvv = braintree.Encrypt(dummyCreditCard.AvcCvvCvv2+""),
                    EncryptedExpirationDate = braintree.Encrypt(dummyCreditCard.ExpirationDate.ToString("MM/yyyy")),
                }).IsSuccessfull;

        }


        private BraintreeGateway BraintreeGateway { get; set; }

        /*
         * Braintree Creds:
         * U: apcurium
         * P: apcurium5200!
         */
        
        public TokenizedCreditCardResponse Post(TokenizeCreditCardBraintreeRequest tokenizeRequest)
        {
            return TokenizedCreditCard(BraintreeGateway, tokenizeRequest);
        }

        private static TokenizedCreditCardResponse TokenizedCreditCard(BraintreeGateway client, TokenizeCreditCardBraintreeRequest tokenizeRequest)
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

            var result = client.Customer.Create(request);

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
            BraintreeGateway.CreditCard.Delete(request.CardToken);
            return new DeleteTokenizedCreditcardResponse()
                {
                    IsSuccessfull = true,
                    Message = "Success"
                };
        }

        public PreAuthorizePaymentResponse Post(PreAuthorizePaymentBraintreeRequest preAuthorizeRequest)
        {
            try
            {


                var orderDetail = _orderDao.FindById(preAuthorizeRequest.OrderId);
                if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                var request = new TransactionRequest
                    {
                        Amount = preAuthorizeRequest.Amount,
                        PaymentMethodToken = preAuthorizeRequest.CardToken,
                        OrderId = orderDetail.IBSOrderId.ToString(),

                        Options = new TransactionOptionsRequest
                            {
                                SubmitForSettlement = false
                            }
                    };

                var result = BraintreeGateway.Transaction.Sale(request);
                bool isSuccessful = result.IsSuccess();
                if (isSuccessful)
                {
                    _commandBus.Send(new InitiateCreditCardPayment
                        {
                            PaymentId = Guid.NewGuid(),
                            Amount = preAuthorizeRequest.Amount,
                            TransactionId = result.Target.Id,
                            OrderId = preAuthorizeRequest.OrderId,
                            CardToken = preAuthorizeRequest.CardToken
                        });
                }

                return new PreAuthorizePaymentResponse()
                    {
                        TransactionId = result.Target.Id,
                        IsSuccessfull = isSuccessful,
                        Message = "Success",
                    };
            }
            catch (Exception e)
            {
                return new PreAuthorizePaymentResponse()
                    {
                        IsSuccessfull = false,
                        Message = e.Message,
                        TransactionId = null
                    };
            }

        }


        public CommitPreauthorizedPaymentResponse Post(CommitPreauthorizedPaymentBraintreeRequest request)
        {
            try
            {
                var payment = _dao.FindByTransactionId(request.TransactionId);
                if (payment == null) throw new HttpError(HttpStatusCode.NotFound, "Payment not found");


                var result = BraintreeGateway.Transaction.SubmitForSettlement(request.TransactionId);

                var isSuccessful = result.IsSuccess();
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
            catch (Exception e)
            {
                return new CommitPreauthorizedPaymentResponse()
                    {
                        IsSuccessfull = false,
                        Message = e.Message,
                    };
            }
        }

        
    }
}
