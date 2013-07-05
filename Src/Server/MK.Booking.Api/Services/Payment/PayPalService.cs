using MK.Booking.PayPal;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Payment;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PayPalService: Service
    {
        readonly ExpressCheckoutServiceFactory _factory;
        private readonly IConfigurationManager _configurationManager;

        public PayPalService(ExpressCheckoutServiceFactory factory, IConfigurationManager configurationManager)
        {
            _factory = factory;
            _configurationManager = configurationManager;
        }

        public PayPalResponse Post(PayPalRequest request)
        {
            var checkoutUrl = _factory
                .CreateService(RequestContext, GetPayPalCredentials())
                .SetExpressCheckout(request.Amount);

            return new PayPalResponse
            {
                CheckoutUrl = checkoutUrl,
            };
        }

        private PayPalCredentials GetPayPalCredentials()
        {
            var paymentSettings = ((ServerPaymentSettings)_configurationManager.GetPaymentSettings()).PayPalSettings;
            return paymentSettings.IsSandBox
                            ? paymentSettings.PayPalSandboxCredentials
                            : paymentSettings.PayPalCredentials;
        }
    }
}
