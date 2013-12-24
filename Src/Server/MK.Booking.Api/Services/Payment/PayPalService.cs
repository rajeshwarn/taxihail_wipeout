using System;
using System.Net;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PayPalService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IOrderPaymentDao _dao;
        private readonly ExpressCheckoutServiceFactory _factory;
        private readonly IConfigurationManager _configurationManager;

        public PayPalService(ICommandBus commandBus, IOrderPaymentDao dao,
                             ExpressCheckoutServiceFactory factory, IConfigurationManager configurationManager)
        {
            _commandBus = commandBus;
            _dao = dao;
            _factory = factory;
            _configurationManager = configurationManager;
        }



        public PayPalExpressCheckoutPaymentResponse Post(
            InitiatePayPalExpressCheckoutPaymentRequest request)
        {
            var root = ApplicationPathResolver.GetApplicationPath(base.RequestContext);
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
                                     Tip = request.Tip ,
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
            return new HttpResult() {
                StatusCode = HttpStatusCode.Redirect,
                Headers = {
                    { HttpHeaders.Location, "taxihail://cancel" }
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

            
            _commandBus.Send(new CompletePayPalExpressCheckoutPayment
                                 {
                                     PaymentId = payment.PaymentId,
                                     PayPalPayerId = request.PayerId,
                                     TransactionId = transactionId,
                                 });

            return new HttpResult()
            {
                StatusCode = HttpStatusCode.Redirect,
                Headers = {
                    { HttpHeaders.Location, "taxihail://success" }
                }
            };
        }

        private PayPalServerSettings GetPayPalSettings()
        {
            var paymentSettings = (ServerPaymentSettings) _configurationManager.GetPaymentSettings();
            if (paymentSettings == null) throw new HttpError(HttpStatusCode.InternalServerError, "InternalServerError", "Payment settings not found");
            
            var payPalSettings = paymentSettings.PayPalServerSettings;
            if (payPalSettings == null) throw new HttpError(HttpStatusCode.InternalServerError, "InternalServerError", "PayPal settings not found");

            return payPalSettings;
        }


        public static bool TestClient(IConfigurationManager configurationManager, IRequestContext requestContext, PayPalCredentials payPalServerSettings, bool isSandbox)
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
