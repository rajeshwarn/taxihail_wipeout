using System;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using Moneris;

namespace apcurium.MK.Booking.Services.Impl
{
    public class MonerisPaymentService : IPaymentService
    {
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;
        private readonly IServerSettings _serverSettings;
        private readonly IPairingService _pairingService;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly ICreditCardDao _creditCardDao;

        private readonly string CryptType_SSLEnabledMerchant = "7";

        public MonerisPaymentService(ICommandBus commandBus,
            ILogger logger,
            IOrderPaymentDao paymentDao,
            IServerSettings serverSettings, 
            IPairingService pairingService,
            ICreditCardDao creditCardDao)
        {
            _commandBus = commandBus;
            _logger = logger;
            _paymentDao = paymentDao;
            _serverSettings = serverSettings;
            _pairingService = pairingService;
            _creditCardDao = creditCardDao;
        }

        public PaymentProvider ProviderType(Guid? orderId = null)
        {
            return PaymentProvider.Moneris;
        }

        public bool IsPayPal(Guid? accountId = null, Guid? orderId = null)
        {
            return false;
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
                // we must do a completion with $0 (see eSELECTplus_DotNet_IG.pdf, Process Flow for PreAuth / Capture Transactions)
                var paymentDetail = _paymentDao.FindByOrderId(orderId);
                if (paymentDetail == null)
                {
                    // nothing to void
                    return;
                }
                
                var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;

                var completionCommand = new Completion(orderId.ToString(), 0.ToString("F"), paymentDetail.TransactionId, CryptType_SSLEnabledMerchant);
                var commitRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, completionCommand);
                var commitReceipt = commitRequest.GetReceipt();

                RequestSuccesful(commitReceipt, out message);
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Can't cancel Moneris preauthorization");
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
            Void(orderId, transactionId, ref message);
        }

        private void Void(Guid orderId, string transactionId, ref string message)
        {
            var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;

            var correctionCommand = new PurchaseCorrection(orderId.ToString(), transactionId, CryptType_SSLEnabledMerchant);
            var correctionRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, correctionCommand);
            var correctionReceipt = correctionRequest.GetReceipt();

            string monerisMessage;
            var correctionSuccess = RequestSuccesful(correctionReceipt, out monerisMessage);

            if (!correctionSuccess)
            {
                message = string.Format("{0} and {1}", message, monerisMessage);
                throw new Exception("Moneris Purchase Correction failed");
            }

            message = message + " The transaction has been cancelled.";
        }

        public DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken)
        {
            var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;

            try
            {
                var deleteCommand = new ResDelete(cardToken);
                var deleteRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, deleteCommand);
                var receipt = deleteRequest.GetReceipt();

                var message = string.Empty;
                var success = RequestSuccesful(receipt, out message);

                return new DeleteTokenizedCreditcardResponse
                {
                    IsSuccessful = success,
                    Message = success 
                                ? "Success" 
                                : message
                };
            }
            catch (AggregateException ex)
            {
                ex.Handle(x =>
                {
                    _logger.LogError(x);
                    return true;
                });
                return new DeleteTokenizedCreditcardResponse
                {
                    IsSuccessful = false,
                    Message = ex.InnerExceptions.First().Message,
                };
            }
        }

        public PreAuthorizePaymentResponse PreAuthorize(Guid orderId, AccountDetail account, decimal amountToPreAuthorize, bool isReAuth = false)
        {
            var message = string.Empty;
            var transactionId = string.Empty;

            try
            {
                bool isSuccessful;
                bool isCardDeclined = false;
                var orderIdentifier = isReAuth ? string.Format("{0}-1", orderId) : orderId.ToString();
                var creditCard = _creditCardDao.FindByAccountId(account.Id).First();

                if (creditCard.IsDeactivated)
                {
                    // If card was deactivated, do not accept further payment with this card
                    return new PreAuthorizePaymentResponse
                    {
                        IsDeclined = true, 
                        Message = "Credit card was deactivated"
                    };
                }

                if (amountToPreAuthorize > 0)
                {
                    // PreAuthorize transaction
                    var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;

                    var preAuthorizeCommand = new ResPreauthCC(creditCard.Token, orderIdentifier, amountToPreAuthorize.ToString("F"), CryptType_SSLEnabledMerchant);
                    var preAuthRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, preAuthorizeCommand);
                    var preAuthReceipt = preAuthRequest.GetReceipt();

                    
                    isSuccessful = RequestSuccesful(preAuthReceipt, out message);
                    isCardDeclined = IsCardDeclined(preAuthReceipt);

                    if (isSuccessful)
                    {
                        transactionId = preAuthReceipt.GetTxnNumber();
                    }
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
                        CardToken = creditCard.Token,
                        Provider = PaymentProvider.Moneris,
                        IsNoShowFee = false
                    });
                }

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    Message = message,
                    TransactionId = transactionId,
                    ReAuthOrderId = isReAuth ? orderIdentifier : null,
                    IsDeclined = isCardDeclined
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

        private PreAuthorizePaymentResponse ReAuthorizeIfNecessary(Guid orderId, AccountDetail account, decimal preAuthAmount, decimal amount)
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

            _logger.LogMessage(string.Format("Re-Authorizing order for amount of {0}", amount));

            return PreAuthorize(orderId, account, amount, true);
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, AccountDetail account, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId)
        {
            string message;
            string authorizationCode = null;
            string commitTransactionId = transactionId;

            try
            {
                var authResponse = ReAuthorizeIfNecessary(orderId, account, preauthAmount, amount);
                if (!authResponse.IsSuccessful)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessful = false,
                        TransactionId = commitTransactionId,
                        Message = string.Format("Moneris Re-Auth of amount {0} failed.", amount),
                        IsDeclined = true
                    };
                }

                if (authResponse.TransactionId.HasValue())
                {
                    commitTransactionId = authResponse.TransactionId;
                }

                var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;
                var completionCommand = new Completion(authResponse.ReAuthOrderId ?? orderId.ToString(), amount.ToString("F"), commitTransactionId,
                    CryptType_SSLEnabledMerchant);
                var commitRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId,
                    monerisSettings.ApiToken, completionCommand);
                var commitReceipt = commitRequest.GetReceipt();

                var isSuccessful = RequestSuccesful(commitReceipt, out message);
                var isCardDeclined = IsCardDeclined(commitReceipt);

                if (isSuccessful)
                {
                    authorizationCode = commitReceipt.GetAuthCode();
                    commitTransactionId = commitReceipt.GetTxnNumber();
                }

                // we must return the latest transaction id in case of a successful commit 
                // since this is the one we need to void if there's a problem while contacting driver
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    AuthorizationCode = authorizationCode,
                    Message = message,
                    TransactionId = commitTransactionId,
                    IsDeclined = isCardDeclined
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

        private bool RequestSuccesful(Receipt receipt, out string message)
        {
            message = string.Empty;
            if (!bool.Parse(receipt.GetComplete()) || bool.Parse(receipt.GetTimedOut()))
            {
                message = receipt.GetMessage();
                return false;
            }

            if (int.Parse(receipt.GetResponseCode()) >= MonerisResponseCodes.DECLINED)
            {
                message = receipt.GetMessage();
                return false;
            }

            return true;
        }

        private bool IsCardDeclined(Receipt receipt)
        {
            var responseCode = int.Parse(receipt.GetResponseCode());

            return MonerisResponseCodes.GetDeclinedCodes().Contains(responseCode);
        }
    }
}