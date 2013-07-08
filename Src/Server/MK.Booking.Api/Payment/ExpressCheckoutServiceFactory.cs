using System.Globalization;
using MK.Booking.PayPal;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Payment
{
    public class ExpressCheckoutServiceFactory
    {
        public ExpressCheckoutServiceClient CreateService(IRequestContext requestContext, PayPalCredentials payPalCredentials)
        {
            var root = ApplicationPathResolver.GetApplicationPath(requestContext);
            var successUrl = root + "/payment/paypal/success";
            var cancelUrl = root + "/payment/paypal/cancel";


            return new ExpressCheckoutServiceClient(payPalCredentials, new RegionInfo("en-US"), successUrl, cancelUrl, useSandbox: true);
        }

    }
}
