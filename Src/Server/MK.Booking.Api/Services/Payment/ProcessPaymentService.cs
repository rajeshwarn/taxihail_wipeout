using System;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
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
        private readonly IPaymentServiceFactory _paymentServiceFactory;
        private readonly IAccountDao _accountDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IOrderDao _orderDao;
        private readonly IServerSettings _serverSettings;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly IIbsOrderService _ibs;
        private readonly ICommandBus _commandBus;
        private readonly IPromotionDao _promotionDao;
        private readonly IEventSourcedRepository<Promotion> _promoRepository;
        private readonly ILogger _logger;

        public ProcessPaymentService(
            IPaymentServiceFactory paymentServiceFactory, 
            IAccountDao accountDao, 
            IOrderDao orderDao,
            IIBSServiceProvider ibsServiceProvider,
            IServerSettings serverSettings, 
            IOrderPaymentDao paymentDao,
            IIbsOrderService ibs,
            ICommandBus commandBus,
            IPromotionDao promotionDao,
            IEventSourcedRepository<Promotion> promoRepository,
            ILogger logger)
        {
            _paymentServiceFactory = paymentServiceFactory;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _ibsServiceProvider = ibsServiceProvider;
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
            var amountSaved = 0m;

            var promoUsed = _promotionDao.FindByOrderId(request.OrderId);
            if (promoUsed != null)
            {
                var promoDomainObject = _promoRepository.Get(promoUsed.PromoId);
                amountSaved = promoDomainObject.GetAmountSaved(totalAmount);
                totalAmount = totalAmount - amountSaved;
            }

            var preAuthResponse = PreauthorizePaymentIfNecessary(request.OrderId, request.CardToken, totalAmount);
            if (preAuthResponse.IsSuccessful)
            {
                return CommitPayment(
                    totalAmount, 
                    request.MeterAmount, 
                    request.TipAmount, 
                    request.CardToken, 
                    request.OrderId, 
                    request.IsNoShowFee, 
                    promoUsed != null 
                        ? promoUsed.PromoId 
                        : (Guid?) null, 
                    amountSaved);
            }

            return new CommitPreauthorizedPaymentResponse
            {
                IsSuccessful = false,
                Message = string.Format("PreAuthorization Failed: {0}", preAuthResponse.Message)
            };
        }

        private CommitPreauthorizedPaymentResponse CommitPayment(decimal totalOrderAmount, decimal meterAmount, decimal tipAmount, string cardToken, Guid orderId, bool isNoShowFee, Guid? promoUsedId = null, decimal amountSaved = 0)
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
                    if (totalOrderAmount > 0)
                    {
                        paymentProviderServiceResponse = _paymentServiceFactory.GetInstance().CommitPayment(orderId, totalOrderAmount, meterAmount, tipAmount, paymentDetail.TransactionId);
                        message = paymentProviderServiceResponse.Message;
                    }
                    else
                    {
                        // promotion made the ride free to the user
                        // void preauth if it exists
                        _paymentServiceFactory.GetInstance().VoidPreAuthorization(orderId);

                        paymentProviderServiceResponse.IsSuccessful = true;
                        paymentProviderServiceResponse.AuthorizationCode = "AUTH_PROMO_FREE";
                    }
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
                            _paymentServiceFactory.GetInstance().ProviderType.ToString(),
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
                            _paymentServiceFactory.GetInstance().VoidTransaction(orderId, paymentProviderServiceResponse.TransactionId, ref message);
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

                    var fareObject = Fare.FromAmountInclTax(Convert.ToDouble(meterAmount), _serverSettings.ServerData.VATIsEnabled ? _serverSettings.ServerData.VATPercentage : 0);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentServiceFactory.GetInstance().ProviderType,
                        Amount = totalOrderAmount,
                        MeterAmount = Convert.ToDecimal(fareObject.AmountExclTax),
                        TipAmount = Convert.ToDecimal(tipAmount),
                        TaxAmount = Convert.ToDecimal(fareObject.TaxAmount),
                        IsNoShowFee = isNoShowFee,
                        AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode,
                        PromotionUsed = promoUsedId,
                        AmountSavedByPromotion = amountSaved
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

            var result = _paymentServiceFactory.GetInstance().PreAuthorize(orderId, account.Email, cardToken, amount);

            if (result.IsSuccessful)
            {
                // Wait for OrderPaymentDetail to be created
                Thread.Sleep(500);
            }

            return result;
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardRequest request)
        {
            return _paymentServiceFactory.GetInstance().DeleteTokenizedCreditcard(request.CardToken);
        }

        public PairingResponse Post(PairingForPaymentRequest request)
        {
            var response =  _paymentServiceFactory.GetInstance().Pair(request.OrderId, request.CardToken, request.AutoTipPercentage, request.AutoTipAmount);
            if ( response.IsSuccessful )
            {
                var o = _orderDao.FindById( request.OrderId );
                var a = _accountDao.FindById( o.AccountId );
                if (!UpdateOrderPaymentType(a.IBSAccountId.Value, o.IBSOrderId.Value, 7))
                {
                    response.IsSuccessful = false;
                    _paymentServiceFactory.GetInstance().VoidPreAuthorization(request.OrderId);
                }
            }
            return response;

        }

        private bool UpdateOrderPaymentType(int ibsAccountId, int ibsOrderId, int chargeTypeId, string companyKey = null)
        {
            var result = _ibsServiceProvider.Booking(companyKey).UpdateOrderPaymentType( ibsAccountId, ibsOrderId, chargeTypeId );
            return result;
        }

        public BasePaymentResponse Post(UnpairingForPaymentRequest request)
        {
            return _paymentServiceFactory.GetInstance().Unpair(request.OrderId);
        }
    }
}