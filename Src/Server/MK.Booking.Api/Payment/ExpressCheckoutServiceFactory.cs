using System.Globalization;
using MK.Booking.PayPal;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Payment
{
    public class ExpressCheckoutServiceFactory
    {
        readonly IConfigurationManager _configurationManager;

        public ExpressCheckoutServiceFactory(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public ExpressCheckoutServiceClient CreateService(IRequestContext requestContext, PayPalCredentials payPalCredentials, bool useSandbox)
        {
            var root = ApplicationPathResolver.GetApplicationPath(requestContext);
            var successUrl = root + "/api/payment/paypal/success";
            var cancelUrl = root + "/api/payment/paypal/cancel";
            var cultureName = _configurationManager.GetSetting("PriceFormat");

            return new ExpressCheckoutServiceClient(payPalCredentials, new RegionInfo(cultureName), successUrl, cancelUrl, useSandbox);
        }

    }
}
