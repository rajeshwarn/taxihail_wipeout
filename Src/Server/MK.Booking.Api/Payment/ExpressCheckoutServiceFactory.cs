using System.Globalization;
using System.Web.Hosting;
using MK.Booking.PayPal;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Payment
{
    public class ExpressCheckoutServiceFactory
    {
        public ExpressCheckoutServiceClient CreateService(PayPalCredentials payPalCredentials)
        {
            var successUrl = HostingEnvironment.MapPath("~/PayPalExpressCheckout/mobile-success.html");
            var cancelUrl = HostingEnvironment.MapPath("~/PayPalExpressCheckout/mobile-cancel.html");

            return new ExpressCheckoutServiceClient(payPalCredentials, new RegionInfo("en-US"), successUrl, cancelUrl, useSandbox: true);
        }

    }
}
