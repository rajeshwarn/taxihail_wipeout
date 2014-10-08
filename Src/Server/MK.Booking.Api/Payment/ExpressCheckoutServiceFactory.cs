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
        private readonly IServerSettings _serverSettings;

        public ExpressCheckoutServiceFactory(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
        }

        public ExpressCheckoutServiceClient CreateService(PayPalCredentials payPalCredentials, bool useSandbox)
        {
            var regionName = _serverSettings.ServerData.PayPalRegionInfoOverride;

            if (string.IsNullOrWhiteSpace(regionName))
            {
                regionName = _serverSettings.ServerData.PriceFormat;
            }

            return new ExpressCheckoutServiceClient(payPalCredentials, new RegionInfo(regionName) , useSandbox);
        }
    }
}