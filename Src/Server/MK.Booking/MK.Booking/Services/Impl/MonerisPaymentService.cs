using System;
using System.Linq;
using System.Threading;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
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
        private readonly IOrderDao _orderDao;
        private readonly ILogger _logger;
        private readonly IServerSettings _serverSettings;
        private readonly IPairingService _pairingService;
        private readonly IIbsOrderService _ibs;
        private readonly IAccountDao _accountDao;
        private readonly IOrderPaymentDao _paymentDao;

        private readonly string CryptType_SSLEnabledMerchant = "7";

        public MonerisPaymentService(ICommandBus commandBus,
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
            _serverSettings = serverSettings;
            _pairingService = pairingService;
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
                // we must do a completion with $0 (see eSELECTplus_DotNet_IG.pdf, Process Flow for PreAuth / Capture Transactions)
                var paymentDetail = _paymentDao.FindNonPayPalByOrderId(orderId);
                if (paymentDetail == null)
                {
                    if (_serverSettings.GetPaymentSettings().IsPreAuthEnabled)
                    {
                        throw new Exception(string.Format("Payment for order {0} not found", orderId));
                    }

                    // PreAuth disabled, no Void to do
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

        private void Void(Guid orderId, string commitTransactionId, ref string message)
        {
            var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;

            var correctionCommand = new PurchaseCorrection(orderId.ToString(), commitTransactionId, CryptType_SSLEnabledMerchant);
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

            try
            {
                var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;

                // PreAuthorize transaction
                var preAuthorizeCommand = new ResPreauthCC(cardToken, orderId.ToString(), amountToPreAuthorize.ToString("F"), CryptType_SSLEnabledMerchant);
                var preAuthRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, preAuthorizeCommand);
                var preAuthReceipt = preAuthRequest.GetReceipt();

                var transactionId = preAuthReceipt.GetTxnNumber();
                var isSuccessful = RequestSuccesful(preAuthReceipt, out message);

                if (isSuccessful)
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
                Receipt commitReceipt = null;

                var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;
                
                // commit transaction
                if (!paymentDetail.IsCompleted)
                {
                    var completionCommand = new Completion(orderId.ToString(), amount.ToString("F"), paymentDetail.TransactionId, CryptType_SSLEnabledMerchant);
                    var commitRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, completionCommand);
                    commitReceipt = commitRequest.GetReceipt();

                    isSuccessful = RequestSuccesful(commitReceipt, out message);
                }

                if (isSuccessful && !isNoShowFee)
                {
                    authorizationCode = commitReceipt.GetAuthCode();
                    var commitTransactionId = commitReceipt.GetTxnNumber();

                    //send information to IBS
                    try
                    {
                        _ibs.ConfirmExternalPayment(orderDetail.Id,
                                                    orderDetail.IBSOrderId.Value,
                                                    Convert.ToDecimal(amount),
                                                    Convert.ToDecimal(tipAmount),
                                                    Convert.ToDecimal(meterAmount),
                                                    PaymentType.CreditCard.ToString(),
                                                    PaymentProvider.Moneris.ToString(),
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

                        //cancel moneris transaction
                        try
                        {
                            Void(orderId, commitTransactionId, ref message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogMessage("Can't cancel Moneris transaction");
                            _logger.LogError(ex);
                            message = message + ex.Message;
                        }
                    }
                }

                if (isSuccessful)
                {
                    //payment completed
                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        PaymentId = paymentDetail.PaymentId,
                        AuthorizationCode = authorizationCode,
                        Provider = PaymentProvider.Moneris,
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