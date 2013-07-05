using System;
using Infrastructure.Messaging;
using MK.Booking.PayPal;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PayPalService: Service
    {
        readonly ICommandBus _commandBus;
        readonly ExpressCheckoutServiceFactory _factory;
        readonly IConfigurationManager _configurationManager;

        public PayPalService(ICommandBus commandBus, ExpressCheckoutServiceFactory factory, IConfigurationManager configurationManager)
        {
            _commandBus = commandBus;
            _factory = factory;
            _configurationManager = configurationManager;
        }

        public PayPalExpressCheckoutPaymentResponse Post(InitiatePayPalExpressCheckoutPaymentRequest expressCheckoutPaymentRequest)
        {
            var service = _factory
                .CreateService(RequestContext, GetPayPalCredentials());

            var token = service.SetExpressCheckout(expressCheckoutPaymentRequest.Amount);
            var checkoutUrl = service.GetCheckoutUrl(token);

            _commandBus.Send(new InitiatePayPalExpressCheckoutPayment
            {
                OrderId = expressCheckoutPaymentRequest.OrderId,
                PaymentId = Guid.NewGuid(),
                Token = token
            });

            return new PayPalExpressCheckoutPaymentResponse
            {
                CheckoutUrl = checkoutUrl,
            };
        }

        public void Get(CancelPayPalExpressCheckoutPaymentRequest request)
        {
            throw new NotImplementedException("Work in progress");
            _commandBus.Send(new CancelPayPalExpressCheckoutPayment
            {
            });
        }

        public void Get(CompletePayPalExpressCheckoutPaymentRequest request)
        {
            throw new NotImplementedException("Work in progress");
            _commandBus.Send(new CompletePayPalExpressCheckoutPayment
            {
            });
        }

        private PayPalCredentials GetPayPalCredentials()
        {
            var paymentSettings = ((ServerPaymentSettings)_configurationManager.GetPaymentSettings()).PayPalServerSettings;
            return paymentSettings.IsSandbox
                            ? paymentSettings.SandboxCredentials
                            : paymentSettings.Credentials;
        }
    }
}
