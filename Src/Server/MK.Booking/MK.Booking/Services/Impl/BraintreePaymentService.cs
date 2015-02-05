using System;
using apcurium.MK.Booking.Commands;
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
        private readonly ILogger _logger;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly IPairingService _pairingService;

        private BraintreeGateway BraintreeGateway { get; set; }

        public BraintreePaymentService(ICommandBus commandBus,
            ILogger logger,
            IOrderPaymentDao paymentDao,
            IServerSettings serverSettings,
            IPairingService pairingService)
        {
            _commandBus = commandBus;
            _logger = logger;
            _paymentDao = paymentDao;
            _pairingService = pairingService;

            BraintreeGateway = GetBraintreeGateway(serverSettings.GetPaymentSettings().BraintreeServerSettings);
        }

        public PaymentProvider ProviderType
        {
            get
            {
                return PaymentProvider.Braintree;
            }
        }

        public PairingResponse Pair(Guid orderId, string cardToken, int? autoTipPercentage)
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
                var paymentDetail = _paymentDao.FindByOrderId(orderId);
                if (paymentDetail == null)
                {
                    // nothing to void
                    return;
                } 

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

        public void VoidTransaction(Guid orderId, string transactionId, ref string message)
        {
            Void(transactionId, ref message);
        }

        private void Void(string transactionId, ref string message)
        {
            //see paragraph oops here https://www.braintreepayments.com/docs/dotnet/transactions/submit_for_settlement

            var transaction = BraintreeGateway.Transaction.Find(transactionId);
            Result<Transaction> cancellationResult = null;
            if (transaction.Status == TransactionStatus.AUTHORIZING
                || transaction.Status == TransactionStatus.AUTHORIZED
                || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                // can void
                cancellationResult = BraintreeGateway.Transaction.Void(transactionId);
            }
            else if (transaction.Status == TransactionStatus.SETTLED
                || transaction.Status == TransactionStatus.SETTLING)
            {
                // will have to refund it
                cancellationResult = BraintreeGateway.Transaction.Refund(transactionId);
            }

            if (cancellationResult == null
                || !cancellationResult.IsSuccess())
            {
                throw new Exception(cancellationResult != null
                    ? cancellationResult.Message
                    : string.Format("transaction {0} status {1}, can't cancel it",
                        transactionId, transaction.Status));
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

        public PreAuthorizePaymentResponse PreAuthorize(Guid orderId, string email, string cardToken, decimal amountToPreAuthorize, bool isReAuth = false)
        {
            var message = string.Empty;
            var transactionId = string.Empty;

            try
            {
                bool isSuccessful;
                var orderIdentifier = isReAuth ? string.Format("{0}-1", orderId) : orderId.ToString();

                if (amountToPreAuthorize > 0)
                {
                    var transactionRequest = new TransactionRequest
                    {
                        Amount = amountToPreAuthorize,
                        PaymentMethodToken = cardToken,
                        OrderId = orderIdentifier,
                        Channel = "MobileKnowledgeSystems_SP_MEC",
                        Options = new TransactionOptionsRequest
                        {
                            SubmitForSettlement = false
                        }
                    };

                    //sale
                    var result = BraintreeGateway.Transaction.Sale(transactionRequest);

                    transactionId = result.Target.Id;
                    message = result.Message;
                    isSuccessful = result.IsSuccess();
                }
                else
                {
                    // if we're preauthorizing $0, we skip the preauth with payment provider
                    // but we still send the InitiateCreditCardPayment command
                    // this should never happen in the case of a real preauth (hence the minimum of $50)
                    isSuccessful = true;
                }

                if (isSuccessful && !isReAuth)
                {
                    var paymentId = Guid.NewGuid();
                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = paymentId,
                        Amount = amountToPreAuthorize,
                        TransactionId = transactionId,
                        OrderId = orderId,
                        CardToken = cardToken,
                        Provider = PaymentProvider.Braintree,
                        IsNoShowFee = false
                    });
                }

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    Message = message,
                    TransactionId = transactionId,
                    ReAuthOrderId = isReAuth ? orderIdentifier : null
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

        private PreAuthorizePaymentResponse ReAuthorizeIfNecessary(Guid orderId, decimal preAuthAmount, decimal amount)
        {
            if (amount <= preAuthAmount)
            {
                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = true
                };
            }

            _logger.LogMessage(string.Format("Re-Authorizing order {0} because it exceeded the original pre-auth amount ", orderId));
            _logger.LogMessage(string.Format("Voiding original Pre-Auth of {0}", preAuthAmount));

            VoidPreAuthorization(orderId);

            var paymentDetail = _paymentDao.FindByOrderId(orderId);

            _logger.LogMessage(string.Format("Re-Authorizing order for amount of {0}", amount));

            return PreAuthorize(orderId, null, paymentDetail.CardToken, amount, true);
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId)
        {
            var commitTransactionId = transactionId;
            string authorizationCode = null;

            try
            {
                var authResponse = ReAuthorizeIfNecessary(orderId, preauthAmount, amount);
                if (!authResponse.IsSuccessful)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessful = false,
                        TransactionId = commitTransactionId,
                        Message = string.Format("Braintree Re-Auth of amount {0} failed.", amount)
                    };
                }

                if (authResponse.TransactionId.HasValue())
                {
                    commitTransactionId = authResponse.TransactionId;
                }

                var settlementResult = BraintreeGateway.Transaction.SubmitForSettlement(commitTransactionId, amount);
                
                var isSuccessful = settlementResult.IsSuccess()
                    && settlementResult.Target != null
                    && settlementResult.Target.ProcessorAuthorizationCode.HasValue();

                if (isSuccessful)
                {
                    authorizationCode = settlementResult.Target.ProcessorAuthorizationCode;
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    AuthorizationCode = authorizationCode,
                    Message = settlementResult.Message,
                    TransactionId = commitTransactionId
                };
            }
            catch (Exception ex)
            {
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = commitTransactionId,
                    Message = ex.Message
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