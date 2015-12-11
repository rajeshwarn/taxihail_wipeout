using System;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
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
        private readonly IServerSettings _serverSettings;
        private readonly IPairingService _pairingService;
        private readonly ICreditCardDao _creditCardDao;

        private BraintreeGateway BraintreeGateway { get; set; }

        public BraintreePaymentService(ICommandBus commandBus,
            ILogger logger,
            IOrderPaymentDao paymentDao,
            IServerSettings serverSettings,
            ServerPaymentSettings serverPaymentSettings,
            IPairingService pairingService,
            ICreditCardDao creditCardDao)
        {
            _commandBus = commandBus;
            _logger = logger;
            _paymentDao = paymentDao;
            _serverSettings = serverSettings;
            _pairingService = pairingService;
            _creditCardDao = creditCardDao;

            BraintreeGateway = GetBraintreeGateway(serverPaymentSettings.BraintreeServerSettings);
        }

        public PaymentProvider ProviderType(string companyKey, Guid? orderId = null)
        {
            return PaymentProvider.Braintree;
        }

        public bool IsPayPal(Guid? accountId = null, Guid? orderId = null, bool isForPrepaid = false)
        {
            return false;
        }

        public PairingResponse Pair(string companyKey, Guid orderId, string cardToken, int autoTipPercentage)
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

        public BasePaymentResponse Unpair(string companyKey, Guid orderId)
        {
           _pairingService.Unpair(orderId);

            return new BasePaymentResponse
            {
                IsSuccessful = true,
                Message = "Success"
            };
        }

        public void VoidPreAuthorization(string companyKey, Guid orderId, bool isForPrepaid = false)
        {
            var message = string.Empty;
            try
            {
                var paymentDetail = _paymentDao.FindByOrderId(orderId, companyKey);
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

        public void VoidTransaction(string companyKey, Guid orderId, string transactionId, ref string message)
        {
            Void(transactionId, ref message);
        }

        private void Void(string transactionId, ref string message)
        {
            //see paragraph oops here https://www.braintreepayments.com/docs/dotnet/transactions/submit_for_settlement

            var transaction = BraintreeGateway.Transaction.Find(transactionId);
            var operationDone = string.Empty;
            Result<Transaction> cancellationResult = null;
            if (transaction.Status == TransactionStatus.AUTHORIZING
                || transaction.Status == TransactionStatus.AUTHORIZED
                || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                // can void
                cancellationResult = BraintreeGateway.Transaction.Void(transactionId);
                operationDone = "voided";
            }
            else if (transaction.Status == TransactionStatus.SETTLED
                || transaction.Status == TransactionStatus.SETTLING)
            {
                // will have to refund it
                cancellationResult = BraintreeGateway.Transaction.Refund(transactionId);
                operationDone = "refunded";
            }

            if (cancellationResult == null
                || !cancellationResult.IsSuccess())
            {
                throw new Exception(cancellationResult != null
                    ? cancellationResult.Message
                    : string.Format("transaction {0} status {1}, can't cancel it",
                        transactionId, transaction.Status));
            }

            message = string.Format("{0} The transaction has been {1}.", message, operationDone);
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
        
        public PreAuthorizePaymentResponse PreAuthorize(string companyKey, Guid orderId, AccountDetail account, decimal amountToPreAuthorize, bool isReAuth = false, bool isSettlingOverduePayment = false, bool isForPrepaid = false, string cvv = null)
        {
            var message = string.Empty;
            var transactionId = string.Empty;
            DateTime? transactionDate = null;

            try
            {
                bool isSuccessful;
                var isCardDeclined = false;
                var orderIdentifier = isReAuth ? string.Format("{0}-{1}", orderId, GenerateShortUid()) : orderId.ToString();

                var customerId = account.BraintreeAccountId;
                
                if (!customerId.HasValueTrimmed())
                {
                    var creditCard = _creditCardDao.FindByAccountId(account.Id).First();
                    var braintreeCreditCard = BraintreeGateway.CreditCard.Find(creditCard.Token);
                    customerId = braintreeCreditCard.CustomerId;
                    var name = account.Name.Split(' ');
                    var braintreeCustomerUpdate = new CustomerRequest()
                    {
                        FirstName = name.FirstOrDefault(),
                        LastName = name.LastOrDefault()
                    };

                    BraintreeGateway.Customer.Update(customerId, braintreeCustomerUpdate);
                }

                var customer = BraintreeGateway.Customer.Find(customerId);
                
                if (amountToPreAuthorize > 0)
                {
                    var transactionRequest = new TransactionRequest
                    {
                        Amount = amountToPreAuthorize,
                        PaymentMethodToken = customer.DefaultPaymentMethod.Token,
                        OrderId = orderIdentifier,
                        Channel = "MobileKnowledgeSystems_SP_MEC",
                        Options = new TransactionOptionsRequest
                        {
                            SubmitForSettlement = false
                        }
                    };

                    AddCvvInfo(transactionRequest, cvv);

                    //sale
                    var result = BraintreeGateway.Transaction.Sale(transactionRequest);
                    
                    message = result.Message;
                    isSuccessful = result.IsSuccess();
                    isCardDeclined = IsCardDeclined(result.Transaction);
                    transactionId = isSuccessful ? result.Target.Id : result.Transaction.Id;
                    transactionDate = isSuccessful ? result.Target.CreatedAt : result.Transaction.CreatedAt;
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
                        CardToken = customer.DefaultPaymentMethod.Token,
                        Provider = PaymentProvider.Braintree,
                        IsNoShowFee = false,
                        CompanyKey = companyKey
                    });
                }

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    Message = message,
                    TransactionId = transactionId,
                    ReAuthOrderId = isReAuth ? orderIdentifier : null,
                    IsDeclined = isCardDeclined,
                    TransactionDate = transactionDate
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage(string.Format("Error during preauthorization (validation of the card) for client {0}: {1} - {2}", account.Email, message, e));
                _logger.LogError(e);

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = false,
                    Message = message
                };
            }
        }

        private PreAuthorizePaymentResponse ReAuthorizeIfNecessary(string companyKey, Guid orderId, AccountDetail account, decimal preAuthAmount, decimal amount)
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

            VoidPreAuthorization(companyKey, orderId);

            _logger.LogMessage(string.Format("Re-Authorizing order for amount of {0}", amount));

            return PreAuthorize(companyKey, orderId, account, amount, true);
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(string companyKey, Guid orderId, AccountDetail account, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId, string reAuthOrderId = null, bool isForPrepaid = false)
        {
            var commitTransactionId = transactionId;
            string authorizationCode = null;

            try
            {
                var authResponse = ReAuthorizeIfNecessary(companyKey, orderId, account, preauthAmount, amount);
                if (!authResponse.IsSuccessful)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessful = false, 
                        IsDeclined = authResponse.IsDeclined,
                        TransactionId = commitTransactionId,
                        TransactionDate = authResponse.TransactionDate,
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

                var isCardDeclined = IsCardDeclined(settlementResult.Transaction);
                var transactionDate = isSuccessful ? settlementResult.Target.UpdatedAt : settlementResult.Transaction.UpdatedAt;

                if (isSuccessful)
                {
                    authorizationCode = settlementResult.Target.ProcessorAuthorizationCode;
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    AuthorizationCode = authorizationCode,
                    Message = settlementResult.Message,
                    TransactionId = commitTransactionId,
                    IsDeclined = isCardDeclined,
                    TransactionDate = transactionDate
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

        public BasePaymentResponse RefundPayment(string companyKey, Guid orderId)
        {
            var paymentDetail = _paymentDao.FindByOrderId(orderId, companyKey);
            if (paymentDetail == null)
            {
                // No payment to refund
                var message = string.Format("Cannot refund because no payment was found for order {0}.", orderId);
                _logger.LogMessage(message);

                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = message
                };
            }

            try
            {
                var message = string.Empty;
                Void(paymentDetail.TransactionId, ref message);

                return new BasePaymentResponse
                {
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogMessage(string.Format("Braintree refund for transaction {0} failed. {1}", paymentDetail.TransactionId, ex.Message));

                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
                };
            }
        }

        public BasePaymentResponse UpdateAutoTip(string companyKey, Guid orderId, int autoTipPercentage)
        {
            throw new NotImplementedException("Method only implemented for CMT RideLinQ");
        }

        private void AddCvvInfo(TransactionRequest transactionRequest, string cvv)
        {
            if (_serverSettings.GetPaymentSettings().AskForCVVAtBooking)
            {
                if (!cvv.HasValue())
                {
                    _logger.LogMessage("AskForCVVAtBooking setting is enabled but no cvv found for this order, could be from a reauth");
                }
                else
                {
                    // we pass the cvv to Braintree, but if no cvv rules are configured on the 
                    // Braintree Gateway of the client, the cvv will not cause a preauth failure
                    transactionRequest.CreditCard = new TransactionCreditCardRequest
                    {
                        CVV = cvv
                    };
                }
            }
        }

        private bool IsCardDeclined(Transaction transaction)
        {
            if (transaction == null || transaction.Status == null)
            {
                return false;
            }

            return transaction.Status == TransactionStatus.PROCESSOR_DECLINED
                || transaction.Status == TransactionStatus.GATEWAY_REJECTED;
        }

        private string GenerateShortUid()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", string.Empty)
                .Replace("+", string.Empty)
                .Replace("/", string.Empty);
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
                PrivateKey = settings.PrivateKey
            };
        }
    }
}