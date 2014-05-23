using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Moneris;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Moneris;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class MonerisPaymentService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly ILogger _logger;
        private readonly IConfigurationManager _configurationManager;
        private readonly IIbsOrderService _ibs;
        private readonly IAccountDao _accountDao;
        private readonly IOrderPaymentDao _paymentDao;

        private readonly string CryptType_SSLEnabledMerchant = "7";

        public MonerisPaymentService(ICommandBus commandBus,
            IOrderDao orderDao,
            ILogger logger,
            IConfigurationManager configurationManager,
            IIbsOrderService ibs,
            IAccountDao accountDao,
            IOrderPaymentDao paymentDao)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _logger = logger;
            _configurationManager = configurationManager;
            _ibs = ibs;
            _accountDao = accountDao;
            _paymentDao = paymentDao;
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardMonerisRequest deleteRequest)
        {
            var monerisSettings = _configurationManager.GetPaymentSettings().MonerisPaymentSettings;

            try
            {
                var deleteCommand = new ResDelete(deleteRequest.CardToken);
                var request = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, deleteCommand);
                var receipt = request.GetReceipt();

                var message = string.Empty;
                var success = RequestSuccesful(receipt, out message);

                return new DeleteTokenizedCreditcardResponse
                {
                    IsSuccessfull = success,
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
                    IsSuccessfull = false,
                    Message = ex.InnerExceptions.First().Message,
                };
            }
        }

        public CommitPreauthorizedPaymentResponse Post(PreAuthorizeAndCommitPaymentMonerisRequest request)
        {
            try
            {
                var isSuccessful = false;
                var message = string.Empty;
                var authorizationCode = string.Empty;

                var session = this.GetSession();
                var account = _accountDao.FindById(new Guid(session.UserAuthId));

                var orderDetail = _orderDao.FindById(request.OrderId);
                if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                //check if already a payment
                if (_paymentDao.FindByOrderId(request.OrderId) != null)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessfull = false,
                        Message = "order already paid or payment currently processing"
                    };
                }

                var monerisSettings = _configurationManager.GetPaymentSettings().MonerisPaymentSettings;

                // PreAuthorize transaction
                var preAuthorizeCommand = new ResPreauthCC(request.CardToken, request.OrderId.ToString(), request.Amount.ToString("F"), CryptType_SSLEnabledMerchant);
                var preAuthRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, preAuthorizeCommand);
                var preAuthReceipt = preAuthRequest.GetReceipt();

                var success = RequestSuccesful(preAuthReceipt, out message);
                if (success)
                {
                    var transactionId = preAuthReceipt.GetTxnNumber();
                    var paymentId = Guid.NewGuid();

                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = paymentId,
                        Amount = Convert.ToDecimal(request.Amount),
                        Meter = Convert.ToDecimal(request.MeterAmount),
                        Tip = Convert.ToDecimal(request.TipAmount),
                        TransactionId = transactionId,
                        OrderId = request.OrderId,
                        CardToken = request.CardToken,
                        Provider = PaymentProvider.Moneris,
                    });

                    // wait for OrderPaymentDetail to be created
                    Thread.Sleep(500);

                    // commit transaction
                    var completionCommand = new Completion(request.OrderId.ToString(), request.Amount.ToString("F"), transactionId, CryptType_SSLEnabledMerchant);
                    var commitRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, completionCommand);
                    var commitReceipt = commitRequest.GetReceipt();

                    isSuccessful = RequestSuccesful(commitReceipt, out message);
                    if (isSuccessful)
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

                            //cancel moneris transaction
                            try
                            {
                                var correctionCommand = new PurchaseCorrection(request.OrderId.ToString(), commitTransactionId, CryptType_SSLEnabledMerchant);
                                var correctionRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, correctionCommand);
                                var correctionReceipt = correctionRequest.GetReceipt();

                                var monerisMessage = string.Empty;
                                var correctionSuccess = RequestSuccesful(correctionReceipt, out monerisMessage);

                                if (!correctionSuccess)
                                {
                                    message = message + monerisMessage;
                                    throw new Exception("Moneris PurchaseCorrection failed");
                                }

                                message = message + " the transaction has been cancelled.";
                            }
                            catch (Exception ex)
                            {
                                _logger.LogMessage("can't cancel moneris transaction");
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
                            Provider = PaymentProvider.Moneris,
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