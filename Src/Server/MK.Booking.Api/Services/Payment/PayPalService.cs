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

        public PayPalService(ExpressCheckoutServiceFactory factory)
        {
            var paymentSettings = ((ServerPaymentSettings) configurationManager.GetPaymentSettings()).PayPalSettings;
            var creds = paymentSettings.IsSandBox
                            ? paymentSettings.PayPalSandboxCredentials
                            : paymentSettings.PayPalCredentials;
            _factory = factory;
        }

        public PayPalResponse Post(PayPalRequest request)
        {
            var checkoutUrl = _factory
                .CreateService(RequestContext)
                .SetExpressCheckout(request.Amount);

            return new PayPalResponse
            {
                CheckoutUrl = checkoutUrl,
            };
        }
    }
}
