using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using Moneris;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Services.Impl
{
    public class MonerisPaymentService : IPaymentService
    {
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;
        private readonly ServerPaymentSettings _serverPaymentSettings;
        private readonly IPairingService _pairingService;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly IServerSettings _serverSettings;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IOrderDao _orderDao;

        private readonly string CryptType_SSLEnabledMerchant = "7";

        public MonerisPaymentService(ICommandBus commandBus,
            ILogger logger,
            IOrderPaymentDao paymentDao,
            IServerSettings serverSettings,
            ServerPaymentSettings serverPaymentSettings, 
            IPairingService pairingService,
            ICreditCardDao creditCardDao,
            IOrderDao orderDao)
        {
            _commandBus = commandBus;
            _logger = logger;
            _paymentDao = paymentDao;
            _serverSettings = serverSettings;
            _serverPaymentSettings = serverPaymentSettings;
            _pairingService = pairingService;
            _creditCardDao = creditCardDao;
            _orderDao = orderDao;
            MonerisHttpRequestWrapper.SetLogger(_logger);
        }

        public PaymentProvider ProviderType(string companyKey, Guid? orderId = null)
        {
            return PaymentProvider.Moneris;
        }

        public bool IsPayPal(Guid? accountId = null, Guid? orderId = null, bool isForPrepaid = false)
        {
            return false;
        }

        public Task<PairingResponse> Pair(string companyKey, Guid orderId, string cardToken, int autoTipPercentage)
        {
            return Task.Run(() =>
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
            });
        }

        public Task<BasePaymentResponse> Unpair(string companyKey, Guid orderId)
        {
            return Task.Run(() =>
            {
                _pairingService.Unpair(orderId);

                return new BasePaymentResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            });
        }

        public void VoidPreAuthorization(string companyKey, Guid orderId, bool isForPrepaid = false)
        {
            var message = string.Empty;
            try
            {
                // we must do a completion with $0 (see eSELECTplus_DotNet_IG.pdf, Process Flow for PreAuth / Capture Transactions)
                var paymentDetail = _paymentDao.FindByOrderId(orderId, companyKey);
                if (paymentDetail == null)
                {
                    // nothing to void
                    return;
                }
                
                var monerisSettings = _serverPaymentSettings.MonerisPaymentSettings;

                var completionCommand = new Completion(orderId.ToString(), 0.ToString("F"), paymentDetail.TransactionId, CryptType_SSLEnabledMerchant);
                var commitRequest = MonerisHttpRequestWrapper.NewHttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, completionCommand);
                var commitReceipt = commitRequest.GetAndLogReceipt(_logger);

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

        public void VoidTransaction(string companyKey, Guid orderId, string transactionId, ref string message)
        {
            Void(orderId, transactionId, ref message);
        }

        private void Void(Guid orderId, string transactionId, ref string message)
        {
            var monerisSettings = _serverPaymentSettings.MonerisPaymentSettings;

            var correctionCommand = new PurchaseCorrection(orderId.ToString(), transactionId, CryptType_SSLEnabledMerchant);
            var correctionRequest = MonerisHttpRequestWrapper.NewHttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, correctionCommand);
            var correctionReceipt = correctionRequest.GetAndLogReceipt(_logger);

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
            var monerisSettings = _serverPaymentSettings.MonerisPaymentSettings;

            try
            {
                var deleteCommand = new ResDelete(cardToken);
                var deleteRequest = MonerisHttpRequestWrapper.NewHttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId,
                     monerisSettings.ApiToken, deleteCommand);
                var receipt = deleteRequest.GetAndLogReceipt(_logger);

                string message;
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
                    Message = ex.InnerExceptions.First().Message
                };
            }
            catch (Exception ex)
            {
                return new DeleteTokenizedCreditcardResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
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
                bool isCardDeclined = false;
                var creditCard = _creditCardDao.FindById(account.DefaultCreditCard.GetValueOrDefault());

                var order = _orderDao.FindOrderStatusById(orderId);
				string driverId = order != null ? order.DriverInfos != null ? order.DriverInfos.DriverId : null : null;

                // We cannot re-use the same id has a previously failed payment
                var shouldGenerateNewOrderId = isReAuth || isSettlingOverduePayment;

                var orderIdentifier = shouldGenerateNewOrderId
                    ? string.Format("{0}-{1}", orderId, GenerateShortUid())
                    : orderId.ToString();
                
                if (amountToPreAuthorize > 0)
                {
                    // PreAuthorize transaction
                    var monerisSettings = _serverPaymentSettings.MonerisPaymentSettings;

                    var preAuthorizeCommand = new ResPreauthCC(creditCard.Token, orderIdentifier, driverId, amountToPreAuthorize.ToString("F"), CryptType_SSLEnabledMerchant);
                    AddCvvInfo(preAuthorizeCommand, cvv);

                    var preAuthRequest = MonerisHttpRequestWrapper.NewHttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, preAuthorizeCommand);
                    var preAuthReceipt = preAuthRequest.GetAndLogReceipt(_logger);

                    isSuccessful = RequestSuccesful(preAuthReceipt, out message);
                    isCardDeclined = IsCardDeclined(preAuthReceipt);
                    transactionId = preAuthReceipt.GetTxnNumber();
                    transactionDate = GetTransactionDate(preAuthReceipt);
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
                        IsNoShowFee = false,
                        CompanyKey = companyKey
                    });
                }

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    Message = message,
                    TransactionId = transactionId,
                    ReAuthOrderId = shouldGenerateNewOrderId ? orderIdentifier : null,
                    IsDeclined = isCardDeclined,
                    TransactionDate = transactionDate,
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
            string message;
            string authorizationCode = null;
            string commitTransactionId = transactionId;

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
                        Message = string.Format("Moneris Re-Auth of amount {0} failed.", amount)
                    };
                }

                if (authResponse.TransactionId.HasValue())
                {
                    commitTransactionId = authResponse.TransactionId;
                }

                string orderIdentifier;
                if (reAuthOrderId.HasValue())
                {
                    // Settling overdue payment
                    orderIdentifier = reAuthOrderId;
                }
                else if (authResponse.ReAuthOrderId.HasValue())
                {
                    // Normal re-auth
                    orderIdentifier = authResponse.ReAuthOrderId;
                }
                else
                {
                    // Normal flow
                    orderIdentifier = orderId.ToString();
                }

                var monerisSettings = _serverPaymentSettings.MonerisPaymentSettings;
                var completionCommand = new Completion(orderIdentifier, amount.ToString("F"), commitTransactionId, CryptType_SSLEnabledMerchant);

                var orderStatus = _orderDao.FindOrderStatusById(orderId);
                if (orderStatus != null
                    && orderStatus.DriverInfos != null
                    && orderStatus.DriverInfos.DriverId.HasValue())
                {
                    //add driver id to "memo" field
                    completionCommand.SetDynamicDescriptor(orderStatus.DriverInfos.DriverId); 
                }

                var commitRequest = MonerisHttpRequestWrapper.NewHttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, completionCommand);
                var commitReceipt = commitRequest.GetAndLogReceipt(_logger);

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
                    IsDeclined = isCardDeclined,
                    TransactionDate = GetTransactionDate(commitReceipt)
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

        public Task<RefundPaymentResponse> RefundPayment(string companyKey, Guid orderId)
        {
            throw new NotImplementedException();
        }

        public Task<BasePaymentResponse> UpdateAutoTip(string companyKey, Guid orderId, int autoTipPercentage)
        {
            throw new NotImplementedException("Method only implemented for CMT RideLinQ");
        }

        private void AddCvvInfo(ResPreauthCC preAuthorizeCommand, string cvv)
        {
            if (_serverSettings.GetPaymentSettings().AskForCVVAtBooking)
            {
                if (!cvv.HasValue())
                {
                    _logger.LogMessage("AskForCVVAtBooking setting is enabled but no cvv found for this order, could be from a reauth");
                }
                else
                {
                    // Only supported for Visa/MasterCard/Amex
                    var cvdCheck = new CvdInfo();
                    cvdCheck.SetCvdIndicator("1");
                    cvdCheck.SetCvdValue(cvv);
                    preAuthorizeCommand.SetCvdInfo(cvdCheck);
                }
            }
        }

        private DateTime? GetTransactionDate(Receipt transactionReceipt)
        {
            DateTime localTransactionDate;
            var isValidDate = DateTime.TryParse(
                string.Format("{0} {1}", transactionReceipt.GetTransDate(), transactionReceipt.GetTransTime()),
                CultureInfo.InvariantCulture, DateTimeStyles.None,
                out localTransactionDate);

            return isValidDate ? localTransactionDate.ToUniversalTime() : default(DateTime?);
        }

        private bool RequestSuccesful(Receipt receipt, out string message)
        {
            message = string.Empty;
            if (!bool.Parse(receipt.GetComplete()) || bool.Parse(receipt.GetTimedOut()))
            {
                message = receipt.GetMessage();
                return false;
            }

            var responseCode = int.Parse(receipt.GetResponseCode());
            if (responseCode >= MonerisResponseCodes.DECLINED)
            {
                message = receipt.GetMessage();
                return false;
            }

            return true;
        }

        private bool IsCardDeclined(Receipt receipt)
        {
            int responseCode;
            var isNumber = int.TryParse(receipt.GetResponseCode(), out responseCode);
            if (!isNumber)
            {
                // GetResponseCode will return "null" string when transaction is a success...
                return false;
            }

            return MonerisResponseCodes.GetDeclinedCodes().Contains(responseCode);
        }

        private string GenerateShortUid()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", string.Empty)
                .Replace("+", string.Empty)
                .Replace("/", string.Empty);
        }
    }

    /// <summary>
    /// Completion class from 2.5.5
    /// </summary>
    public class Completion : Transaction
    {
        private static string[] xmlTags = 
        {
          "order_id",
          "comp_amount",
          "txn_number",
          "crypt_type"
        };

        private Hashtable keyHashes = new Hashtable();

        public Completion(Hashtable completion)
            : base(completion, xmlTags)
        {
        }

        public Completion(string orderId, string compAmount, string txnNumber, string cryptType)
            : base(xmlTags)
        {
            transactionParams.Add("order_id", orderId);
            transactionParams.Add("comp_amount", compAmount);
            transactionParams.Add("txn_number", txnNumber);
            transactionParams.Add("crypt_type", cryptType);
        }

        public void SetDynamicDescriptor(string dynamicDescriptor)
        {
            keyHashes.Add("dynamic_descriptor", dynamicDescriptor);
        }

        public override string toXML()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("<completion>");
            stringBuilder.Append(base.toXML());
            var enumerator = keyHashes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                stringBuilder.Append("<" + enumerator.Key + ">" + enumerator.Value + "</" + enumerator.Key + ">");
            }
            stringBuilder.Append("</completion>");
            return stringBuilder.ToString();
        }

    }

    internal class MonerisHttpRequestWrapper
    {
        private static ILogger _logger;
        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        internal static HttpsPostRequest NewHttpsPostRequest(string host, string store, string apiTok, Transaction t)
        {
            LogTransaction(t);
            return new HttpsPostRequest(host, store, apiTok, t);
        }
        internal static HttpsPostRequest NewHttpsPostRequest(string host, string store, string apiTok, string statusCheck, Transaction t)
        {
            LogTransaction(t);
            return new HttpsPostRequest(host, store, apiTok, statusCheck, t);
        }
        internal static HttpsPostRequest NewHttpsPostRequest(string host, string store, string apiTok, Transaction t, System.Net.WebProxy prxy)
        {
            LogTransaction(t);
            return new HttpsPostRequest(host, store, apiTok, t, prxy);
        }
        private static void LogTransaction(Transaction t)
        {
            _logger.Maybe(() => _logger.LogMessage("Moneris Request : " + JsonConvert.SerializeObject(t)));
        }
    }

    public static class MonerisExtensions
    {
        public static Receipt GetAndLogReceipt(this HttpsPostRequest request, ILogger logger)
        {
            Receipt r = request.GetReceipt();
            logger.Maybe(() => logger.LogMessage("Moneris Response : " + JsonConvert.SerializeObject(r)));
            return r;
        }
    }
}