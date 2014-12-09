#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
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
        private readonly IServerSettings _serverSettings;
        private readonly IOrderPaymentDao _dao;        
        private readonly ExpressCheckoutServiceFactory _factory;
        private readonly IIbsOrderService _ibs;
        private readonly IAccountDao _accountDao;
        private static ILogger _logger;
        private readonly IOrderDao _orderDao;
        private readonly Resources.Resources _resources;

        public PayPalService(ICommandBus commandBus, IOrderPaymentDao dao,
            ExpressCheckoutServiceFactory factory, IServerSettings serverSettings, 
            IIbsOrderService ibs, IAccountDao accountDao, ILogger logger, IOrderDao orderDao)
        {
            _commandBus = commandBus;
            _dao = dao;
            _factory = factory;
            _serverSettings = serverSettings;
            _ibs = ibs;
            _accountDao = accountDao;
            _logger = logger;
            _orderDao = orderDao;

            _resources = new Resources.Resources(serverSettings);
        }

        public PayPalExpressCheckoutPaymentResponse Post(InitiatePayPalExpressCheckoutPaymentRequest request)
        {
            var root = ApplicationPathResolver.GetApplicationPath(RequestContext);
            var successUrl = root + "/api/payment/paypal/success";
            var cancelUrl = root + "/api/payment/paypal/cancel";

            var payPalSettings = GetPayPalSettings();
            var credentials = payPalSettings.IsSandbox
                ? payPalSettings.SandboxCredentials
                : payPalSettings.Credentials;

            var service = _factory.CreateService(credentials, payPalSettings.IsSandbox);

            var conversionRate = _serverSettings.ServerData.PayPalConversionRate;
            
            var regionName = _serverSettings.ServerData.PayPalRegionInfoOverride;
            var description  =  "";
            if (!string.IsNullOrWhiteSpace(regionName))
            {
                description = string.Format(_resources.Get("PaymentItemDescription", request.LanguageCode), request.IbsOrderId, request.TotalAmount);
            }                        
            
            _logger.LogMessage("Paypal Conversion Rate : " + conversionRate);
            var amount = Math.Round(request.Amount * conversionRate, 2);
            
            var token = service.SetExpressCheckout(amount , successUrl, cancelUrl, description);
            var checkoutUrl = service.GetCheckoutUrl(token);
            
            var fareObject = Fare.FromAmountInclTax(Convert.ToDouble(request.Meter), _serverSettings.ServerData.VATIsEnabled ? _serverSettings.ServerData.VATPercentage : 0);

            _commandBus.Send(new InitiatePayPalExpressCheckoutPayment
            {
                OrderId = request.OrderId,
                PaymentId = Guid.NewGuid(),
                Token = token,
                Amount = request.Amount,
                Meter = Convert.ToDecimal(fareObject.AmountExclTax),
                Tip = request.Tip,
                Tax = Convert.ToDecimal(fareObject.TaxAmount)
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
            try
            {
                var payment = _dao.FindByPayPalToken(request.Token);
                if (payment == null)
                {
                    throw new HttpError(HttpStatusCode.NotFound, "Payment Not Found");
                }

                var payPalSettings = GetPayPalSettings();
                var credentials = payPalSettings.IsSandbox
                    ? payPalSettings.SandboxCredentials
                    : payPalSettings.Credentials;
                var service = _factory.CreateService(credentials, payPalSettings.IsSandbox);

                var conversionRate = _serverSettings.ServerData.PayPalConversionRate;
                _logger.LogMessage("Paypal Converstion Rate : " + conversionRate);

                var amount = Math.Round(payment.Amount * conversionRate, 2);

                var regionName = _serverSettings.ServerData.PayPalRegionInfoOverride;

                string description = "";

                var order = _orderDao.FindById(payment.OrderId);
                if (!string.IsNullOrWhiteSpace(regionName))
                {
                    description = string.Format(_resources.Get("PaymentItemDescription", order.ClientLanguageCode), order.IBSOrderId, payment.Amount);
                }

                var status = _orderDao.FindOrderStatusById(payment.OrderId);

                string note = string.Format("Order #{0}, Vehicle #{1}, Driver id {2}, Driver Name {3} {4}, Registration {5}", order.IBSOrderId, status.VehicleNumber, status.DriverInfos.DriverId, status.DriverInfos.FirstName, status.DriverInfos.LastName, status.DriverInfos.VehicleRegistration);

                var transactionId = service.DoExpressCheckoutPayment(payment.PayPalToken, request.PayerId, amount, description, note);

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
                                                    account.IBSAccountId.Value,
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

                    throw e;
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
            catch (Exception ex)
            {
                _logger.LogMessage("Error occured in PayPalService CompletePayPalExpressCheckoutPaymentRequest handling");
                _logger.LogError(ex);

                return new HttpResult
                {
                    StatusCode = HttpStatusCode.Redirect,
                    Headers =
                    {
                        {HttpHeaders.Location, "taxihail://failure"}
                    }
                };
            }
        }

        private PayPalServerSettings GetPayPalSettings()
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();
            if (paymentSettings == null)
                throw new HttpError(HttpStatusCode.InternalServerError, "InternalServerError",
                    "Payment settings not found");

            var payPalSettings = paymentSettings.PayPalServerSettings;
            if (payPalSettings == null)
                throw new HttpError(HttpStatusCode.InternalServerError, "InternalServerError",
                    "PayPal settings not found");

            return payPalSettings;
        }


        public static bool TestClient(IServerSettings serverSettings, IRequestContext requestContext,
            PayPalCredentials payPalServerSettings, bool isSandbox)
        {
            try
            {
                var service = new ExpressCheckoutServiceFactory(serverSettings, _logger)
                    .CreateService(payPalServerSettings, isSandbox);
                service.SetExpressCheckout(2, "http://example.net/success", "http://example.net/cancel", string.Empty);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}