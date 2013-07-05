using System.Globalization;
using System.Web.Hosting;
using MK.Booking.PayPal;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Helpers;

namespace apcurium.MK.Booking.Api.Payment
{
    public class ExpressCheckoutServiceFactory
    {
        public ExpressCheckoutServiceClient CreateService(IRequestContext requestContext, PayPalCredentials payPalCredentials)
        {
            var root = ApplicationPathResolver.GetApplicationPath(requestContext);
            var successUrl = root + "/PayPalExpressCheckout/mobile-success.html";
            var cancelUrl = root + "/PayPalExpressCheckout/mobile-cancel.html";


            return new ExpressCheckoutServiceClient(payPalCredentials, new RegionInfo("en-US"), successUrl, cancelUrl, useSandbox: true);
        }

    }
}
