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
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Moneris;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class MonerisPaymentService : IPaymentService
    {
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly ILogger _logger;
        private readonly IConfigurationManager _configManager;
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
            IConfigurationManager configManager)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _logger = logger;
            
            _ibs = ibs;
            _accountDao = accountDao;
            _paymentDao = paymentDao;
            _configManager = configManager;
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
            var monerisSettings = _configManager.GetPaymentSettings().MonerisPaymentSettings;

            try
            {
                var deleteCommand = new ResDelete(request.CardToken);
                var deleteRequest = new HttpsPostRequest(monerisSettings.Host, monerisSettings.StoreId, monerisSettings.ApiToken, deleteCommand);
                var receipt = deleteRequest.GetReceipt();

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

        public CommitPreauthorizedPaymentResponse PreAuthorizeAndCommitPayment(PreAuthorizeAndCommitPaymentRequest request)
        {
            try
            {
                var isSuccessful = false;
                var message = string.Empty;
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

                var monerisSettings = _configManager.GetPaymentSettings().MonerisPaymentSettings;

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
                                    message = string.Format("{0} {1}", message, monerisMessage);
                                    throw new Exception("Moneris PurchaseCorrection failed");
                                }

                                message = message + " The transaction has been cancelled.";
                            }
                            catch (Exception ex)
                            {
                                _logger.LogMessage("Can't cancel moneris transaction");
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
                            PaymentId = paymentId,
                            AuthorizationCode = authorizationCode,
                            Provider = PaymentProvider.Moneris,
                        });
                    }
                    else
                    {
                        //payment error
                        _commandBus.Send(new LogCreditCardError
                        {
                            PaymentId = paymentId,
                            Reason = message
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