using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client;
using apcurium.MK.Booking.IBS.ChargeAccounts;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.IBS.Impl
{
    public class IBSServiceProvider : IIBSServiceProvider
    {
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkService;
        private readonly IServiceTypeSettingsProvider _serviceTypeSettingsProvider;
        private readonly Dictionary<string, IBSSettingContainer> _ibsSettings = new Dictionary<string, IBSSettingContainer>();

        public IBSServiceProvider(IServerSettings serverSettings, ILogger logger,
            ITaxiHailNetworkServiceClient taxiHailNetworkService, IServiceTypeSettingsProvider serviceTypeSettingsProvider)
        {
            _serverSettings = serverSettings;
            _logger = logger;
            _taxiHailNetworkService = taxiHailNetworkService;
            _serviceTypeSettingsProvider = serviceTypeSettingsProvider;
        }

        public IAccountWebServiceClient Account(string companyKey)
        {
            return new AccountWebServiceClient(_serverSettings, GetSettingContainer(companyKey), _logger);
        }

        public IStaticDataWebServiceClient StaticData(string companyKey = null, ServiceType? serviceType = null)
        {
            return new StaticDataWebServiceClient(GetSettingContainer(companyKey, serviceType), _logger);
        }

        public IBookingWebServiceClient Booking(string companyKey)
        {
            return new BookingWebServiceClient(_serverSettings, GetSettingContainer(companyKey), _logger);
        }

        public IChargeAccountWebServiceClient ChargeAccount(string companyKey)
        {
            return new ChargeAccountWebServiceClient(_serverSettings, GetSettingContainer(companyKey), _logger);
        }

        public IBSSettingContainer GetSettingContainer(string companyKey = null, ServiceType? serviceType = null)
        {
            if (serviceType.HasValue)
            {
                var ibsSettings = _serverSettings.ServerData.IBS;
                
                if (serviceType.Value == ServiceType.Luxury)
                {
                    var luxuryIBSWebServiceUrl = _serviceTypeSettingsProvider.GetIBSWebServicesUrl(serviceType.Value);
                    ibsSettings.WebServicesUrl = luxuryIBSWebServiceUrl;
                    return ibsSettings;
                }
            }

            if (!companyKey.HasValue())
            {
                return _serverSettings.ServerData.IBS;
            }

            // Switched companies or external market
            if (!_ibsSettings.ContainsKey(companyKey))
            {
                // Get all fleets available to dispatch to company
                var networkFleet = _taxiHailNetworkService.GetNetworkFleet(_serverSettings.ServerData.TaxiHail.ApplicationKey);

                _ibsSettings.Clear();

                foreach (var networkFleetResponse in networkFleet)
                {
                    var settingContainer = new IBSSettingContainer
                    {
                        RestApiUrl = networkFleetResponse.RestApiUrl,
                        RestApiUser = networkFleetResponse.RestApiUser,
                        RestApiSecret = networkFleetResponse.RestApiSecret,
                        WebServicesUrl = networkFleetResponse.IbsUrl,
                        WebServicesPassword = networkFleetResponse.IbsPassword,
                        WebServicesUserName = networkFleetResponse.IbsUserName,
                        TimeDifference = networkFleetResponse.IbsTimeDifference
                    };

                    _ibsSettings.Add(networkFleetResponse.CompanyKey, settingContainer);
                }
            }

            return _ibsSettings[companyKey]; 
        }
    }
}