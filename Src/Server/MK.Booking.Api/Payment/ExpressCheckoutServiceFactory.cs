#region

using System.Globalization;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using MK.Booking.PayPal;

#endregion

namespace apcurium.MK.Booking.Api.Payment
{
    public class ExpressCheckoutServiceFactory
    {
        private readonly IConfigurationManager _configurationManager;

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