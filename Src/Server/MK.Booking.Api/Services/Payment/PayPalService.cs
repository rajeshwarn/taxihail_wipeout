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
        readonly ExpressCheckoutServiceClient _client;

        public PayPalService(ExpressCheckoutServiceFactory factory, IConfigurationManager configurationManager)
        {
            var paymentSettings = ((ServerPaymentSettings) configurationManager.GetPaymentSettings()).PayPalSettings;
            var creds = paymentSettings.IsSandBox
                            ? paymentSettings.PayPalSandboxCredentials
                            : paymentSettings.PayPalCredentials;

            _client = factory.CreateService(creds);
        }

        public PayPalResponse Post(PayPalRequest request)
        {
            var checkoutUrl = _client.SetExpressCheckout(request.Amount);

            return new PayPalResponse
            {
                CheckoutUrl = checkoutUrl,
            };
        }
    }
}
