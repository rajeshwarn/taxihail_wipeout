using System;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class ProcessPaymentService : Service
    {
        private readonly IPaymentService _paymentService;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly IServerSettings _serverSettings;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly IIbsOrderService _ibs;
        private readonly ICommandBus _commandBus;
        private readonly IPromotionDao _promotionDao;
        private readonly IEventSourcedRepository<Promotion> _promoRepository;
        private readonly ILogger _logger;

        public ProcessPaymentService(
            IPaymentService paymentService, 
            IAccountDao accountDao, 
            IOrderDao orderDao, 
            IServerSettings serverSettings, 
            IOrderPaymentDao paymentDao,
            IIbsOrderService ibs,
            ICommandBus commandBus,
            IPromotionDao promotionDao,
            IEventSourcedRepository<Promotion> promoRepository,
            ILogger logger)
        {
            _paymentService = paymentService;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _serverSettings = serverSettings;
            _paymentDao = paymentDao;
            _ibs = ibs;
            _commandBus = commandBus;
            _promotionDao = promotionDao;
            _promoRepository = promoRepository;
            _logger = logger;
        }

        public CommitPreauthorizedPaymentResponse Post(CommitPaymentRequest request)
        {
            var totalAmount = request.Amount;

            // TODO RedeemPromotion

            var preAuthResponse = PreauthorizePaymentIfNecessary(request.OrderId, request.CardToken, totalAmount);
            if (preAuthResponse.IsSuccessful)
            {
                return CommitPayment(totalAmount, request.MeterAmount, request.TipAmount, request.CardToken, request.OrderId, request.IsNoShowFee);
            }

            return new CommitPreauthorizedPaymentResponse
            {
                IsSuccessful = false,
                Message = string.Format("PreAuthorization Failed: {0}", preAuthResponse.Message)
            };
        }

        private CommitPreauthorizedPaymentResponse CommitPayment(decimal totalOrderAmount, decimal meterAmount, decimal tipAmount, string cardToken, Guid orderId, bool isNoShowFee)
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

            var paymentProviderServiceResponse = new CommitPreauthorizedPaymentResponse
            {
                TransactionId = paymentDetail.TransactionId
            };

            try
            {
                var message = string.Empty;

                if (paymentDetail.IsCompleted)
                {
                    message = "Order already paid or payment currently processing";
                }
                else
                {
                    paymentProviderServiceResponse = _paymentService.CommitPayment(orderId, totalOrderAmount, meterAmount, tipAmount, paymentDetail.TransactionId);
                    message = paymentProviderServiceResponse.Message;
                }

                if (paymentProviderServiceResponse.IsSuccessful && !isNoShowFee)
                {
                    //send information to IBS
                    try
                    {
                        _ibs.ConfirmExternalPayment(orderDetail.Id,
                            orderDetail.IBSOrderId.Value,
                            totalOrderAmount,
                            Convert.ToDecimal(tipAmount),
                            Convert.ToDecimal(meterAmount),
                            PaymentType.CreditCard.ToString(),
                            _paymentService.ProviderType.ToString(),
                            paymentProviderServiceResponse.TransactionId,
                            paymentProviderServiceResponse.AuthorizationCode,
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
                        paymentProviderServiceResponse.IsSuccessful = false;

                        //cancel braintree transaction
                        try
                        {
                            _paymentService.VoidTransaction(orderId, paymentProviderServiceResponse.TransactionId, ref message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogMessage("Can't cancel transaction");
                            _logger.LogError(ex);
                            message = message + ex.Message;
                            //can't cancel transaction, send a command to log later
                        }
                    }
                }

                if (paymentProviderServiceResponse.IsSuccessful)
                {
                    //payment completed
                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType,
                        Amount = totalOrderAmount,
                        MeterAmount = Convert.ToDecimal(meterAmount),
                        TipAmount = Convert.ToDecimal(tipAmount),
                        IsNoShowFee = isNoShowFee,
                        AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode
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
                    AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode,
                    TransactionId = paymentProviderServiceResponse.TransactionId,
                    IsSuccessful = paymentProviderServiceResponse.IsSuccessful,
                    Message = paymentProviderServiceResponse.IsSuccessful ? "Success" : message
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("Error during payment " + e);
                _logger.LogError(e);
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = paymentProviderServiceResponse.TransactionId,
                    Message = e.Message,
                };
            }
        }

        private PreAuthorizePaymentResponse PreauthorizePaymentIfNecessary(Guid orderId, string cardToken, decimal amount)
        {
            if (_serverSettings.GetPaymentSettings().IsPreAuthEnabled)
            {
                // Already preautorized on create order, do nothing
                return new PreAuthorizePaymentResponse { IsSuccessful = true };
            }

            var orderDetail = _orderDao.FindById(orderId);
            if (orderDetail == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Order not found");
            }

            var account = _accountDao.FindById(orderDetail.AccountId);

            var result = _paymentService.PreAuthorize(orderId, account.Email, cardToken, amount);

            if (result.IsSuccessful)
            {
                // Wait for OrderPaymentDetail to be created
                Thread.Sleep(500);
            }

            return result;
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardRequest request)
        {
            return _paymentService.DeleteTokenizedCreditcard(request.CardToken);
        }

        public PairingResponse Post(PairingForPaymentRequest request)
        {
            return _paymentService.Pair(request.OrderId, request.CardToken, request.AutoTipPercentage, request.AutoTipAmount);
        }

        public BasePaymentResponse Post(UnpairingForPaymentRequest request)
        {
            return _paymentService.Unpair(request.OrderId);
        }
    }
}