#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PayPalService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;
        private readonly IOrderPaymentDao _dao;
        private readonly ExpressCheckoutServiceFactory _factory;
        private readonly IIbsOrderService _ibs;
        private readonly IAccountDao _accountDao;
        private readonly ILogger _logger;
        private readonly IOrderDao _orderDao;

        public PayPalService(ICommandBus commandBus, IOrderPaymentDao dao,
            ExpressCheckoutServiceFactory factory, IConfigurationManager configurationManager, 
            IIbsOrderService ibs, IAccountDao accountDao, ILogger logger, IOrderDao orderDao)
        {
            _commandBus = commandBus;
            _dao = dao;
            _factory = factory;
            _configurationManager = configurationManager;
            _ibs = ibs;
            _accountDao = accountDao;
            _logger = logger;
            _orderDao = orderDao;
        }


        public PayPalExpressCheckoutPaymentResponse Post(
            InitiatePayPalExpressCheckoutPaymentRequest request)
        {
            var root = ApplicationPathResolver.GetApplicationPath(RequestContext);
            var successUrl = root + "/api/payment/paypal/success";
            var cancelUrl = root + "/api/payment/paypal/cancel";

            var payPalSettings = GetPayPalSettings();
            var credentials = payPalSettings.IsSandbox
                ? payPalSettings.SandboxCredentials
                : payPalSettings.Credentials;

            var service = _factory.CreateService(credentials, payPalSettings.IsSandbox);

            var token = service.SetExpressCheckout(request.Amount, successUrl, cancelUrl);
            var checkoutUrl = service.GetCheckoutUrl(token);

            _commandBus.Send(new InitiatePayPalExpressCheckoutPayment
            {
                OrderId = request.OrderId,
                PaymentId = Guid.NewGuid(),
                Token = token,
                Amount = request.Amount,
                Meter = request.Meter,
                Tip = request.Tip,
            });

            return new PayPalExpressCheckoutPaymentResponse
            {
                CheckoutUrl = checkoutUrl,
            };
        }

        public HttpResult Get(CancelPayPalExpressCheckoutPaymentRequest request)
        {
            var payment = _dao.FindByPayPalToken(request.Token);
            if (payment == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            _commandBus.Send(new CancelPayPalExpressCheckoutPayment
            {
                PaymentId = payment.PaymentId,
            });
            return new HttpResult
            {
                StatusCode = HttpStatusCode.Redirect,
                Headers =
                {
                    {HttpHeaders.Location, "taxihail://cancel"}
                }
            };
        }

        public HttpResult Get(CompletePayPalExpressCheckoutPaymentRequest request)
        {
            var payment = _dao.FindByPayPalToken(request.Token);
            if (payment == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            var payPalSettings = GetPayPalSettings();
            var credentials = payPalSettings.IsSandbox
                ? payPalSettings.SandboxCredentials
                : payPalSettings.Credentials;
            var service = _factory.CreateService(credentials, payPalSettings.IsSandbox);
            var transactionId = service.DoExpressCheckoutPayment(payment.PayPalToken, request.PayerId, payment.Amount);

            var orderDetail = _orderDao.FindById(payment.OrderId);
            var account = _accountDao.FindById(orderDetail.AccountId);

            //send information to IBS
            try
            {

                _ibs.ConfirmExternalPayment(orderDetail.Id, 
                                                orderDetail.IBSOrderId.Value,
                                                payment.Amount,
                                                payment.Tip,
                                                payment.Meter,
                                                PaymentType.PayPal.ToString(),
                                                PaymentProvider.PayPal.ToString(),
                                                transactionId,
                                                request.PayerId,
                                                payment.CardToken,
                                                account.IBSAccountId,
                                                orderDetail.Settings.Name,
                                                orderDetail.Settings.Phone,
                                                account.Email,
                                                orderDetail.UserAgent.GetOperatingSystem(),
                                                orderDetail.UserAgent);

                _commandBus.Send(new CompletePayPalExpressCheckoutPayment
                {
                    PaymentId = payment.PaymentId,
                    PayPalPayerId = request.PayerId,
                    TransactionId = transactionId,
                });
            }
            catch (Exception e)
            {
                _logger.LogMessage("Can't send Payment Information to IBS");
                _logger.LogError(e);

                //cancel paypal transaction
                try
                {
                    service.RefundTransaction(transactionId);
                }
                catch (Exception exe)
                {
                    _logger.LogMessage("Can't cancel Paypal transaction");
                    _logger.LogError(exe);

                    _commandBus.Send(new LogCancellationFailurePayPalPayment
                    {
                        PaymentId = payment.PaymentId,
                        Reason = exe.Message
                    });
                }

                _commandBus.Send(new CancelPayPalExpressCheckoutPayment
                {
                    PaymentId = payment.PaymentId
                });

                return new HttpResult
                {
                    StatusCode = HttpStatusCode.Redirect,
                    Headers =
                        {
                            {HttpHeaders.Location, "taxihail://failure"}
                        }
                };

            }

            return new HttpResult
            {
                StatusCode = HttpStatusCode.Redirect,
                Headers =
                {
                    {HttpHeaders.Location, "taxihail://success"}
                }
            };
        }

        private PayPalServerSettings GetPayPalSettings()
        {
            var paymentSettings = (ServerPaymentSettings) _configurationManager.GetPaymentSettings();
            if (paymentSettings == null)
                throw new HttpError(HttpStatusCode.InternalServerError, "InternalServerError",
                    "Payment settings not found");

            var payPalSettings = paymentSettings.PayPalServerSettings;
            if (payPalSettings == null)
                throw new HttpError(HttpStatusCode.InternalServerError, "InternalServerError",
                    "PayPal settings not found");

            return payPalSettings;
        }


        public static bool TestClient(IConfigurationManager configurationManager, IRequestContext requestContext,
            PayPalCredentials payPalServerSettings, bool isSandbox)
        {
            try
            {
                var service = new ExpressCheckoutServiceFactory(configurationManager)
                    .CreateService(payPalServerSettings, isSandbox);
                service.SetExpressCheckout(2, "http://example.net/success", "http://example.net/cancel");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}