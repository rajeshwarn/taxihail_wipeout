#region

using System;
using System.Linq;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Braintree;
using BraintreeEncryption.Library;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using Environment = Braintree.Environment;

#endregion

namespace apcurium.MK.Booking.Api.Services.Payment
{
    /*
    * Braintree Creds:
    * U: apcurium
    * P: apcurium5200!
    */

    public class BraintreePaymentService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly ILogger _logger;
        private readonly IIbsOrderService _ibs;
        private readonly IAccountDao _accountDao;

        public BraintreePaymentService(ICommandBus commandBus,
            IOrderDao orderDao,
            ILogger logger,
            IConfigurationManager configurationManager,
            IIbsOrderService ibs,
            IAccountDao accountDao)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _logger = logger;
            _ibs = ibs;
            _accountDao = accountDao;

            BraintreeGateway =
                GetBraintreeGateway(
                    ((ServerPaymentSettings) configurationManager.GetPaymentSettings()).BraintreeServerSettings);
        }

        private BraintreeGateway BraintreeGateway { get; set; }

        public static bool TestClient(BraintreeServerSettings settings, BraintreeClientSettings braintreeClientSettings)
        {
            var client = GetBraintreeGateway(settings);

            var dummyCreditCard = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Braintree).Visa;

            var braintreeEncrypter = new BraintreeEncrypter(braintreeClientSettings.ClientKey);

            return TokenizedCreditCard(client, new TokenizeCreditCardBraintreeRequest
            {
                EncryptedCreditCardNumber = braintreeEncrypter.Encrypt(dummyCreditCard.Number),
                EncryptedCvv = braintreeEncrypter.Encrypt(dummyCreditCard.AvcCvvCvv2 + ""),
                EncryptedExpirationDate = braintreeEncrypter.Encrypt(dummyCreditCard.ExpirationDate.ToString("MM/yyyy")),
            }).IsSuccessfull;
        }

        public TokenizedCreditCardResponse Post(TokenizeCreditCardBraintreeRequest tokenizeRequest)
        {
            return TokenizedCreditCard(BraintreeGateway, tokenizeRequest);
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardBraintreeRequest request)
        {
            BraintreeGateway.CreditCard.Delete(request.CardToken);
            return new DeleteTokenizedCreditcardResponse
            {
                IsSuccessfull = true,
                Message = "Success"
            };
        }

        

        public CommitPreauthorizedPaymentResponse Post(PreAuthorizeAndCommitPaymentBraintreeRequest request)
        {
            try
            {
                var isSuccessful = false;
                string message;
                var authorizationCode = string.Empty;

                var session = this.GetSession();
                var account = _accountDao.FindById(new Guid(session.UserAuthId));

                var orderDetail = _orderDao.FindById(request.OrderId);
                if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                var transactionRequest = new TransactionRequest
                {
                    Amount = request.Amount,
                    PaymentMethodToken = request.CardToken,
                    OrderId = orderDetail.IBSOrderId.ToString(),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = false
                    }
                };

                //sale
                var result = BraintreeGateway.Transaction.Sale(transactionRequest);
                message = result.Message;
                if (result.IsSuccess())
                {
                    var transactionId = result.Target.Id;
                    var paymentId = Guid.NewGuid();

                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = paymentId,
                        Amount = request.Amount,
                        Meter = request.MeterAmount,
                        Tip = request.TipAmount,
                        TransactionId = transactionId,
                        OrderId = request.OrderId,
                        CardToken = request.CardToken,
                        Provider = PaymentProvider.Braintree,
                    });

                    // wait for OrderPaymentDetail to be created
                    Thread.Sleep(500);

                    // commit transaction
                    var settlementResult = BraintreeGateway.Transaction.SubmitForSettlement(transactionId);
                    message = settlementResult.Message;

                    isSuccessful = settlementResult.IsSuccess() && (settlementResult.Target != null) &&
                                   (settlementResult.Target.ProcessorAuthorizationCode.HasValue());

                    if (isSuccessful)
                    {
                        authorizationCode = settlementResult.Target.ProcessorAuthorizationCode;

                        //send information to IBS
                        try
                        {
                            _ibs.ConfirmExternalPayment(orderDetail.IBSOrderId.Value, 
                                                            request.Amount, 
                                                            request.TipAmount,
                                                            request.MeterAmount,
                                                            PaymentType.CreditCard.ToString(),
                                                            PaymentProvider.Braintree.ToString(), 
                                                            transactionId, 
                                                            authorizationCode, 
                                                            request.CardToken, 
                                                            account.IBSAccountId, 
                                                            orderDetail.Settings.Name, 
                                                            orderDetail.Settings.Phone, 
                                                            account.Email, 
                                                            orderDetail.UserAgent.GetOperatingSystem(), 
                                                            orderDetail.UserAgent);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e);
                            message = e.Message;
                            isSuccessful = false;

                            //cancel braintree transaction
                            //see paragraph oops here https://www.braintreepayments.com/docs/dotnet/transactions/submit_for_settlement
                            try
                            {
                                var transaction = BraintreeGateway.Transaction.Find(transactionId);
                                Result<Transaction> cancellationResult = null;
                                if (transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
                                {
                                    // can void
                                    cancellationResult = BraintreeGateway.Transaction.Void(transactionId);
                                }
                                else if (transaction.Status == TransactionStatus.SETTLED)
                                {
                                    // will have to refund it
                                    cancellationResult = BraintreeGateway.Transaction.Refund(transactionId);
                                }

                                if (cancellationResult == null
                                    || !cancellationResult.IsSuccess())
                                {
                                    throw new Exception(cancellationResult != null ?
                                            cancellationResult.Message
                                            : string.Format("transaction {0} status unkonw, can't cancel it", transactionId));
                                }

                                message = message + cancellationResult;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogMessage("can't cancel braintree transaction");
                                _logger.LogError(ex);
                                message = message + ex.Message;
                                //can't cancel transaction, send a command to log
                                _commandBus.Send(new LogCreditCardPaymentCancellationFailed
                                {
                                    PaymentId = paymentId,
                                    Reason = message
                                });
                            }
                        }
                    }

                    if (isSuccessful)
                    {
                        //payment completed
                        _commandBus.Send(new CaptureCreditCardPayment
                        {
                            PaymentId = paymentId,
                            AuthorizationCode = authorizationCode,
                            Provider = PaymentProvider.Braintree,
                        });
                    }
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    AuthorizationCode = authorizationCode,
                    IsSuccessfull = isSuccessful,
                    Message = isSuccessful ? "Success" : message
                };
            }
            catch (Exception e)
            {
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessfull = false,
                    Message = e.Message,
                };
            }
        }

        

        private static TokenizedCreditCardResponse TokenizedCreditCard(BraintreeGateway client,
            TokenizeCreditCardBraintreeRequest tokenizeRequest)
        {
            var request = new CustomerRequest
            {
                CreditCard = new CreditCardRequest
                {
                    Number = tokenizeRequest.EncryptedCreditCardNumber,
                    ExpirationDate = tokenizeRequest.EncryptedExpirationDate,
                    CVV = tokenizeRequest.EncryptedCvv
                }
            };

            var result = client.Customer.Create(request);

            var customer = result.Target;

            var cc = customer.CreditCards.First();
            return new TokenizedCreditCardResponse
            {
                CardOnFileToken = cc.Token,
                CardType = cc.CardType.ToString(),
                LastFour = cc.LastFour,
                IsSuccessfull = result.IsSuccess(),
                Message = result.Message,
            };
        }

        private static BraintreeGateway GetBraintreeGateway(BraintreeServerSettings settings)
        {
            var env = Environment.SANDBOX;
            if (!settings.IsSandbox)
            {
                env = Environment.PRODUCTION;
            }

            return new BraintreeGateway
            {
                Environment = env,
                MerchantId = settings.MerchantId,
                PublicKey = settings.PublicKey,
                PrivateKey = settings.PrivateKey,
            };
        }
    }
}