#region

using System;
using System.Linq;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Moneris;
using ServiceStack.Common.Web;
using ServiceStack.Messaging;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Payment
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

        public PairingResponse Pair(PairingForPaymentRequest request)
        {
            try
            {
                _pairingService.Pair(request.OrderId, request.CardToken, request.AutoTipPercentage);

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

        public BasePaymentResponse Unpair(UnpairingForPaymentRequest request)
        {
            _pairingService.Unpair(request.OrderId);

            return new BasePaymentResponse
            {
                IsSuccessful = true,
                Message = "Success"
            };
        }

        public void Void(Guid orderId)
        {
            var message = string.Empty;
            try
            {
                // TODO call void preauth (if possible with eselect plus?)
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Can't cancel Moneris transaction");
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

        public DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(DeleteTokenizedCreditcardRequest request)
        {
            var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;

            try
            {
                var deleteCommand = new ResDelete(request.CardToken);
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
                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = orderId,
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

        public CommitPreauthorizedPaymentResponse CommitPayment(CommitPaymentRequest request)
        {
            var orderDetail = _orderDao.FindById(request.OrderId);
            if (orderDetail == null)
                throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
            if (orderDetail.IBSOrderId == null)
                throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

            var paymentDetail = _paymentDao.FindByOrderId(request.OrderId);

            if (paymentDetail == null)
                throw new HttpError(HttpStatusCode.BadRequest, "Payment not found");

            try
            {
                string message = "Order already paid or payment currently processing";
                bool isSuccessful = false;
                string authorizationCode = null;
                Receipt commitReceipt = null;

                var account = _accountDao.FindById(orderDetail.AccountId);
                var monerisSettings = _serverSettings.GetPaymentSettings().MonerisPaymentSettings;
                
                // commit transaction
                if (!paymentDetail.IsCompleted)
                {
                    var completionCommand = new Completion(request.OrderId.ToString(), request.Amount.ToString("F"), paymentDetail.TransactionId, CryptType_SSLEnabledMerchant);
                    var commitRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, completionCommand);
                    commitReceipt = commitRequest.GetReceipt();

                    isSuccessful = RequestSuccesful(commitReceipt, out message);
                }

                if (isSuccessful && !request.IsNoShowFee)
                {
                    authorizationCode = commitReceipt.GetAuthCode();
                    var commitTransactionId = commitReceipt.GetTxnNumber();

                    //send information to IBS
                    try
                    {
                        _ibs.ConfirmExternalPayment(orderDetail.Id,
                                                    orderDetail.IBSOrderId.Value,
                                                    Convert.ToDecimal(request.Amount),
                                                    Convert.ToDecimal(request.TipAmount),
                                                    Convert.ToDecimal(request.MeterAmount),
                                                    PaymentType.CreditCard.ToString(),
                                                    PaymentProvider.Moneris.ToString(),
                                                    paymentDetail.TransactionId,
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

                        //cancel moneris transaction
                        try
                        {
                            Void(request.OrderId, commitTransactionId, ref message);
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
                        Amount = request.Amount,
                        MeterAmount = request.MeterAmount,
                        TipAmount = request.TipAmount,
                        IsNoShowFee = request.IsNoShowFee
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