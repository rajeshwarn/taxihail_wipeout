using System;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using Braintree;
using Infrastructure.Messaging;
using Environment = Braintree.Environment;

namespace apcurium.MK.Booking.Services.Impl
{
    public class BraintreePaymentService : IPaymentService
    {
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly ILogger _logger;
        private readonly IIbsOrderService _ibs;
        private readonly IAccountDao _accountDao;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly IPairingService _pairingService;

        private BraintreeGateway BraintreeGateway { get; set; }

        public BraintreePaymentService(ICommandBus commandBus,
            IOrderDao orderDao,
            ILogger logger,
            IIbsOrderService ibs,
            IAccountDao accountDao,
            IOrderPaymentDao paymentDao,
            IServerSettings serverSettings,
            IPairingService pairingService)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _logger = logger;
            _ibs = ibs;
            _accountDao = accountDao;
            _paymentDao = paymentDao;
            _pairingService = pairingService;

            BraintreeGateway = GetBraintreeGateway(serverSettings.GetPaymentSettings().BraintreeServerSettings);
        }
        
        public PairingResponse Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
        {
            try
            {
                _pairingService.Pair(orderId, cardToken, autoTipPercentage);
                
                return new PairingResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new PairingResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
                };
            }
        }

        public BasePaymentResponse Unpair(Guid orderId)
        {
           _pairingService.Unpair(orderId);

            return new BasePaymentResponse
            {
                IsSuccessful = true,
                Message = "Success"
            };
        }

        public void VoidPreAuthorization(Guid orderId)
        {
            var message = string.Empty;
            try
            {
                var paymentDetail = _paymentDao.FindNonPayPalByOrderId(orderId);

                if (paymentDetail == null)
                    throw new Exception(string.Format("Payment for order {0} not found", orderId));

                Void(paymentDetail.TransactionId, ref message);
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Can't cancel Braintree preauthorization");
                _logger.LogError(ex);
                message = message + ex.Message;
                //can't cancel transaction, send a command to log later
            }
            finally
            {
                _logger.LogMessage(message);
            }
        }

        private void Void(string transactionId, ref string message)
        {
            //see paragraph oops here https://www.braintreepayments.com/docs/dotnet/transactions/submit_for_settlement

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
                throw new Exception(cancellationResult != null
                    ? cancellationResult.Message
                    : string.Format("transaction {0} status unknown, can't cancel it",
                        transactionId));
            }

            message = message + " The transaction has been cancelled.";
        }

        public DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken)
        {
            try
            {
                BraintreeGateway.CreditCard.Delete(cardToken);
                return new DeleteTokenizedCreditcardResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return new DeleteTokenizedCreditcardResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message,
                };
            }
        }

        public PreAuthorizePaymentResponse PreAuthorize(Guid orderId, string email, string cardToken, decimal amountToPreAuthorize)
        {
            string message = string.Empty;

            try
            {
                var transactionRequest = new TransactionRequest
                {
                    Amount = amountToPreAuthorize,
                    PaymentMethodToken = cardToken,
                    OrderId = orderId.ToString(),                    
                    Channel = "MobileKnowledgeSystems_SP_MEC",
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = false
                    }
                };

                //sale
                var result = BraintreeGateway.Transaction.Sale(transactionRequest);

                var transactionId = result.Target.Id;
                message = result.Message;

                if (result.IsSuccess())
                {
                    var paymentId = Guid.NewGuid();
                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = paymentId,
                        Amount = 0,
                        Meter = 0,
                        Tip = 0,
                        TransactionId = transactionId,
                        OrderId = orderId,
                        CardToken = cardToken,
                        Provider = PaymentProvider.Braintree,
                        IsNoShowFee = false
                    });
                }

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = result.IsSuccess(),
                    Message = message,
                    TransactionId = transactionId
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage(string.Format("Error during preauthorization (validation of the card) for client {0}: {1} - {2}", email, message, e));
                _logger.LogError(e);

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = false,
                    Message = message
                };
            }
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(decimal amount, decimal meterAmount, decimal tipAmount, string cardToken, Guid orderId, bool isNoShowFee)
        {
            var orderDetail = _orderDao.FindById(orderId);
            if (orderDetail == null)
            {
                throw new Exception("Order not found");
            }

            if (orderDetail.IBSOrderId == null)
            {
                throw new Exception("Order has no IBSOrderId");
            }

            var account = _accountDao.FindById(orderDetail.AccountId);
            
            var paymentDetail = _paymentDao.FindNonPayPalByOrderId(orderId);
            if (paymentDetail == null)
            {
                throw new Exception("Payment not found");
            }

            try
            {
                string message = "Order already paid or payment currently processing";
                bool isSuccessful = false;
                string authorizationCode = null;
                Result<Transaction> settlementResult = null;

                // commit transaction
                if (!paymentDetail.IsCompleted)
                {
                    settlementResult = BraintreeGateway.Transaction.SubmitForSettlement(paymentDetail.TransactionId, amount);
                    message = settlementResult.Message;

                    isSuccessful = settlementResult.IsSuccess() 
                        && settlementResult.Target != null 
                        && settlementResult.Target.ProcessorAuthorizationCode.HasValue();
                }

                if (isSuccessful && !isNoShowFee)
                {
                    authorizationCode = settlementResult.Target.ProcessorAuthorizationCode;

                    //send information to IBS
                    try
                    {
                        _ibs.ConfirmExternalPayment(orderDetail.Id,
                            orderDetail.IBSOrderId.Value,
                            amount,
                            tipAmount,
                            meterAmount,
                            PaymentType.CreditCard.ToString(),
                            PaymentProvider.Braintree.ToString(),
                            paymentDetail.TransactionId,
                            authorizationCode,
                            cardToken,
                            account.IBSAccountId.Value,
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
                        try
                        {
                            Void(paymentDetail.TransactionId, ref message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogMessage("Can't cancel Braintree transaction");
                            _logger.LogError(ex);
                            message = message + ex.Message;
                            //can't cancel transaction, send a command to log later
                        }
                    }
                }

                if (isSuccessful)
                {
                    //payment completed
                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        PaymentId = paymentDetail.PaymentId,
                        Provider = PaymentProvider.Braintree,
                        Amount = amount,
                        MeterAmount = meterAmount,
                        TipAmount = tipAmount,
                        IsNoShowFee = isNoShowFee
                    });
                }
                else
                {
                    //payment error
                    _commandBus.Send(new LogCreditCardError
                    {
                        PaymentId = paymentDetail.PaymentId,
                        Reason = message
                    });
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    AuthorizationCode = authorizationCode,
                    TransactionId = paymentDetail.TransactionId,
                    IsSuccessful = isSuccessful,
                    Message = isSuccessful ? "Success" : message
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("Error during payment " + e);
                _logger.LogError(e);
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = paymentDetail.TransactionId,
                    Message = e.Message,
                };
            }
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