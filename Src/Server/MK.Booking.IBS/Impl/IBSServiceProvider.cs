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
        private readonly Dictionary<string, IBSSettingContainer> _ibsSettings = new Dictionary<string, IBSSettingContainer>();

        public IBSServiceProvider(IServerSettings serverSettings, ILogger logger, ITaxiHailNetworkServiceClient taxiHailNetworkService)
        {
            _serverSettings = serverSettings;
            _logger = logger;
            _taxiHailNetworkService = taxiHailNetworkService;
        }

        public IAccountWebServiceClient Account(string companyKey)
        {
            return new AccountWebServiceClient(_serverSettings, GetSettingContainer(companyKey), _logger);
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
                var networkFleet = _taxiHailNetworkService.GetNetworkFleet(_serverSettings.ServerData.TaxiHail.ApplicationKey);

                _ibsSettings.Clear();

                foreach (var networkFleetResponse in networkFleet)
                {
                    var settingContainer = new IBSSettingContainer
                    {
                        WebServicesUrl = networkFleetResponse.IbsUrl,
                        WebServicesPassword = networkFleetResponse.IbsPassword,
                        WebServicesUserName = networkFleetResponse.IbsUserName
                    };
                    _ibsSettings.Add(networkFleetResponse.CompanyKey,settingContainer);
                }
            }

            return _ibsSettings[companyKey];
        }
    }
}