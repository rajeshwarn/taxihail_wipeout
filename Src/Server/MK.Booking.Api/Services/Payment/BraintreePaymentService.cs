#region

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
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
    public class BraintreePaymentService : Service, IPaymentService
    {
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly ILogger _logger;
        private readonly IIbsOrderService _ibs;
        private readonly IAccountDao _accountDao;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly IConfigurationManager _configManager;

        private BraintreeGateway BraintreeGateway { get; set; }

        public BraintreePaymentService(ICommandBus commandBus,
            IOrderDao orderDao,
            ILogger logger,
            IIbsOrderService ibs,
            IAccountDao accountDao,
            IOrderPaymentDao paymentDao,
            IConfigurationManager configManager)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _logger = logger;
            _ibs = ibs;
            _accountDao = accountDao;
            _paymentDao = paymentDao;
            _configManager = configManager;

            BraintreeGateway =
                    GetBraintreeGateway(
                        ((ServerPaymentSettings)configManager.GetPaymentSettings()).BraintreeServerSettings);
        }
        
        public TokenizedCreditCardResponse Post(TokenizeCreditCardBraintreeRequest tokenizeRequest)
        {
            return TokenizedCreditCard(BraintreeGateway, tokenizeRequest);
        }

        public PairingResponse Pair(PairingForPaymentRequest request)
        {
            try
            {
                var orderStatusDetail = _orderDao.FindOrderStatusById(request.OrderId);
                if (orderStatusDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderStatusDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                // send a message to driver, if it fails we abort the pairing
                _ibs.SendMessageToDriver(
                    new Resources.Resources(_configManager.GetSetting("TaxiHail.ApplicationKey"))
                    .Get("PairingConfirmationToDriver"), orderStatusDetail.VehicleNumber);

                // send a command to save the pairing state for this order
                _commandBus.Send(new PairForPayment
                {
                    OrderId = request.OrderId,
                    TokenOfCardToBeUsedForPayment = request.CardToken,
                    AutoTipAmount = request.AutoTipAmount,
                    AutoTipPercentage = request.AutoTipPercentage
                });

                return new PairingResponse
                {
                    IsSuccessfull = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new PairingResponse
                {
                    IsSuccessfull = false,
                    Message = e.Message
                };
            }
        }

        public BasePaymentResponse Unpair(UnpairingForPaymentRequest request)
        {
            var orderPairingDetail = _orderDao.FindOrderPairingById(request.OrderId);
            if (orderPairingDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");

            var orderStatusDetail = _orderDao.FindOrderStatusById(request.OrderId);
            if (orderStatusDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");

            // send a message to driver, if it fails we abort the unpairing
            _ibs.SendMessageToDriver(
                new Resources.Resources(_configManager.GetSetting("TaxiHail.ApplicationKey"))
                    .Get("UnpairingConfirmationToDriver"), orderStatusDetail.VehicleNumber);

            // send a command to delete the pairing pairing info for this order
            _commandBus.Send(new UnpairForPayment
            {
                OrderId = request.OrderId
            });

            return new BasePaymentResponse
            {
                IsSuccessfull = true,
                Message = "Success"
            };
        }

        public DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(DeleteTokenizedCreditcardRequest request)
        {
            BraintreeGateway.CreditCard.Delete(request.CardToken);
            return new DeleteTokenizedCreditcardResponse
            {
                IsSuccessfull = true,
                Message = "Success"
            };
        }

        public CommitPreauthorizedPaymentResponse PreAuthorizeAndCommitPayment(PreAuthorizeAndCommitPaymentRequest request)
        {
            try
            {
                var isSuccessful = false;
                string message;
                var authorizationCode = string.Empty;

                var orderDetail = _orderDao.FindById(request.OrderId);
                if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                var account = _accountDao.FindById(orderDetail.AccountId);

                //check if already a payment
                if (_paymentDao.FindByOrderId(request.OrderId) != null)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessfull = false,
                        Message = "order already paid or payment currently processing"
                    };
                }

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
                            _ibs.ConfirmExternalPayment(orderDetail.Id,
                                                            orderDetail.IBSOrderId.Value,
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

                                message = message + " The transaction has been cancelled.";
                            }
                            catch (Exception ex)
                            {
                                _logger.LogMessage("Can't cancel Braintree transaction");
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

        public static bool TestClient(BraintreeServerSettings settings, BraintreeClientSettings braintreeClientSettings)
        {
            var client = GetBraintreeGateway(settings);

            var dummyCreditCard = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Braintree).Visa;

            var braintreeEncrypter = new BraintreeEncrypter(braintreeClientSettings.ClientKey);

            return TokenizedCreditCard(client, new TokenizeCreditCardBraintreeRequest
            {
                EncryptedCreditCardNumber = braintreeEncrypter.Encrypt(dummyCreditCard.Number),
                EncryptedCvv = braintreeEncrypter.Encrypt(dummyCreditCard.AvcCvvCvv2 + ""),
                EncryptedExpirationDate = braintreeEncrypter.Encrypt(dummyCreditCard.ExpirationDate.ToString("MM/yyyy", CultureInfo.InvariantCulture)),
            }).IsSuccessfull;
        }

        private static TokenizedCreditCardResponse TokenizedCreditCard(BraintreeGateway client, TokenizeCreditCardBraintreeRequest tokenizeRequest)
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