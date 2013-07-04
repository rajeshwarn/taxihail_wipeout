using System.Globalization;
using System.Web.Hosting;
using MK.Booking.PayPal;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Helpers;

namespace apcurium.MK.Booking.Api.Payment
{
    public class ExpressCheckoutServiceFactory
    {
        public ExpressCheckoutServiceClient CreateService(IRequestContext requestContext)
        {
            var root = ApplicationPathResolver.GetApplicationPath(requestContext);
            var successUrl = root + "/PayPalExpressCheckout/mobile-success.html";
            var cancelUrl = root + "/PayPalExpressCheckout/mobile-cancel.html";

            return new ExpressCheckoutServiceClient(new SandboxCredentials(), new RegionInfo("en-US"), successUrl, cancelUrl, useSandbox: true);
        }
        /// <summary>
        /// Temporary, for development purpose only
        /// </summary>
        private class SandboxCredentials: IPayPalCredentials
        {
            public string Username
            {
                get { return "vincent.costel-facilitator_api1.gmail.com"; }
            }
            public string Password
            {
                get { return "1372362468"; }
            }
            public string Signature
            {
                get { return "AFcWxV21C7fd0v3bYYYRCpSSRl31ADYXGX.gPsewqg6pNBBa9JL5zoCL"; }
            }
        }
    }
}
