#region

using System.Globalization;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using MK.Booking.PayPal;
using ServiceStack.Logging;

#endregion

namespace apcurium.MK.Booking.Api.Payment
{
    public class ExpressCheckoutServiceFactory
    {
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;

        public ExpressCheckoutServiceFactory(IServerSettings serverSettings, ILogger logger)
        {
            _serverSettings = serverSettings;
            _logger = logger;
        }

        public ExpressCheckoutServiceClient CreateService(PayPalCredentials payPalCredentials, bool useSandbox)
        {
            var regionName = _serverSettings.ServerData.PayPalRegionInfoOverride;

            if (string.IsNullOrWhiteSpace(regionName))
            {
                regionName = _serverSettings.ServerData.PriceFormat;
            }

            return new ExpressCheckoutServiceClient(payPalCredentials, new RegionInfo(regionName), _logger, useSandbox);
        }
    }
}