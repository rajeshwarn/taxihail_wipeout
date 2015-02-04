using System;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
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

        private readonly string CryptType_SSLEnabledMerchant = "7";

        public MonerisPaymentService(ICommandBus commandBus,
            ILogger logger,
            IOrderPaymentDao paymentDao,
            IServerSettings serverSettings, 
            IPairingService pairingService)
        {
            _commandBus = commandBus;
            _logger = logger;
            _paymentDao = paymentDao;
            _serverSettings = serverSettings;
            _pairingService = pairingService;
        }

        public PaymentProvider ProviderType
        {
            get
            {
                return PaymentProvider.Moneris;
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

        public PreAuthorizePaymentResponse PreAuthorize(Guid orderId, string email, string cardToken, decimal amountToPreAuthorize)
        {
            var message = string.Empty;
            var transactionId = string.Empty;

            try
            {
                bool isSuccessful;

                if (amountToPreAuthorize > 0)
                {
                    // PreAuthorize transaction
                    var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;

                    var preAuthorizeCommand = new ResPreauthCC(cardToken, orderId.ToString(), amountToPreAuthorize.ToString("F"), CryptType_SSLEnabledMerchant);
                    var preAuthRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, preAuthorizeCommand);
                    var preAuthReceipt = preAuthRequest.GetReceipt();

                    transactionId = preAuthReceipt.GetTxnNumber();
                    isSuccessful = RequestSuccesful(preAuthReceipt, out message);
                }
                else
                {
                    // if we're preauthorizing $0, we skip the preauth with payment provider
                    // but we still send the InitiateCreditCardPayment command
                    // this should never happen in the case of a real preauth (hence the minimum of $50)
                    isSuccessful = true;
                }
                
                if (isSuccessful)
                {
                    var paymentId = Guid.NewGuid();
                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = paymentId,
                        Amount = amountToPreAuthorize,
                        TransactionId = transactionId,
                        OrderId = orderId,
                        CardToken = cardToken,
                        Provider = PaymentProvider.Moneris,
                        IsNoShowFee = false
                    });
                }

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = isSuccessful,
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

        public CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId)
        {
            try
            {
                string message = null;
                string authorizationCode = null;
                string commitTransactionId = transactionId;

                var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;
                var completionCommand = new Completion(orderId.ToString(), amount.ToString("F"), transactionId,
                    CryptType_SSLEnabledMerchant);
                var commitRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId,
                    monerisSettings.ApiToken, completionCommand);
                var commitReceipt = commitRequest.GetReceipt();

                var isSuccessful = RequestSuccesful(commitReceipt, out message);
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
                    TransactionId = commitTransactionId 
                };
            }
            catch (Exception ex)
            {
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = transactionId,
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

            if (int.Parse(receipt.GetResponseCode()) >= 50)
            {
                message = receipt.GetMessage();
                return false;
            }

            return true;
        }
    }
}