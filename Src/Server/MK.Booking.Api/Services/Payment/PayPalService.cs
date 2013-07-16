﻿using System;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
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
        private readonly IPayPalExpressCheckoutPaymentDao _dao;
        private readonly ExpressCheckoutServiceFactory _factory;
        private readonly IConfigurationManager _configurationManager;

        public PayPalService(ICommandBus commandBus, IPayPalExpressCheckoutPaymentDao dao,
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
            var payPalSettings = GetPayPalSettings();
            var credentials = payPalSettings.IsSandbox
                                  ? payPalSettings.SandboxCredentials
                                  : payPalSettings.Credentials;

            var service = _factory
                .CreateService(RequestContext, credentials, payPalSettings.IsSandbox);

            var token = service.SetExpressCheckout(request.Amount);
            var checkoutUrl = service.GetCheckoutUrl(token);

            _commandBus.Send(new InitiatePayPalExpressCheckoutPayment
                                 {
                                     OrderId = request.OrderId,
                                     PaymentId = Guid.NewGuid(),
                                     Token = token,
                                     Amount = request.Amount,
                                 });

            return new PayPalExpressCheckoutPaymentResponse
                       {
                           CheckoutUrl = checkoutUrl,
                       };
        }

        public HttpResult Get(CancelPayPalExpressCheckoutPaymentRequest request)
        {
            var payment = _dao.FindByToken(request.Token);
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
            var payment = _dao.FindByToken(request.Token);
            if (payment == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            _commandBus.Send(new CompletePayPalExpressCheckoutPayment
                                 {
                                     PaymentId = payment.PaymentId,
                                     PayPalPayerId = request.PayerId,
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
                .CreateService(requestContext, payPalServerSettings, isSandbox);
                service.SetExpressCheckout(2);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            

        }
    }
}
