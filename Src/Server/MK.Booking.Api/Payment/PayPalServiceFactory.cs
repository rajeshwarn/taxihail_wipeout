#region

using System.Globalization;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using MK.Booking.PayPal;

#endregion

namespace apcurium.MK.Booking.Api.Payment
{
    public class PayPalServiceFactory
    {
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;

        public PayPalServiceFactory(IServerSettings serverSettings, ILogger logger)
        {
            _serverSettings = serverSettings;
            _logger = logger;
        }

        public PayPalServiceClient CreateService(PayPalServerCredentials payPalServerCredentials, bool useSandbox)
        {
            var regionName = _serverSettings.ServerData.PayPalRegionInfoOverride;

            if (string.IsNullOrWhiteSpace(regionName))
            {
                regionName = _serverSettings.ServerData.PriceFormat;
            }

            return new PayPalServiceClient(/*payPalServerCredentials, new RegionInfo(regionName), _logger, useSandbox*/);
        }
    }
}