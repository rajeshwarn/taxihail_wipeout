using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client;

namespace apcurium.MK.Booking.IBS.Impl
{
    public class IBSServiceProvider : IIBSServiceProvider
    {
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkService;
        private IDictionary<string, IBSSettingContainer> _ibsSettings = new Dictionary<string, IBSSettingContainer>();

        public IBSServiceProvider(IServerSettings serverSettings, ILogger logger,ITaxiHailNetworkServiceClient taxiHailNetworkService)
        {
            _serverSettings = serverSettings;
            _logger = logger;
            _taxiHailNetworkService = taxiHailNetworkService;
        }

        public IAccountWebServiceClient Account(string companyKey)
        {
            return new AccountWebServiceClient(GetSettingContainer(companyKey), _logger);
        }

        public IStaticDataWebServiceClient StaticData(string companyKey)
        {
            return new StaticDataWebServiceClient(GetSettingContainer(companyKey), _logger);
        }

        public IBookingWebServiceClient Booking(string companyKey)
        {
            return new BookingWebServiceClient(_serverSettings, GetSettingContainer(companyKey), _logger);
        }

        private IBSSettingContainer GetSettingContainer(string companyKey)
        {
            if (!companyKey.HasValue())
            {
                return _serverSettings.ServerData.IBS;
            }

            if (!_ibsSettings.ContainsKey(companyKey))
            {
                var networkFleet = _taxiHailNetworkService.GetNetworkFleet(companyKey);
                // call customer portal to get info and feed _ibsSettings dictionary
            }

            return _ibsSettings[companyKey];
        }
    }
}