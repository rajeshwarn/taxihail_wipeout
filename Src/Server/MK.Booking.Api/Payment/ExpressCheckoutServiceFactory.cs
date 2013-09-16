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

        public ExpressCheckoutServiceClient CreateService(PayPalCredentials payPalCredentials, bool useSandbox)
        {
            var cultureName = _configurationManager.GetSetting("PriceFormat");

            return new ExpressCheckoutServiceClient(payPalCredentials, new RegionInfo(cultureName), useSandbox);
        }

    }
}
