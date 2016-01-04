using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client;
using apcurium.MK.Booking.IBS.ChargeAccounts;

namespace apcurium.MK.Booking.IBS.Impl
{
    public class IBSServiceProvider : IIBSServiceProvider
    {
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkService;
        private readonly Dictionary<string, IBSSettingContainer> _ibsSettings = new Dictionary<string, IBSSettingContainer>();

        public IBSServiceProvider(IServerSettings serverSettings, ILogger logger,
            ITaxiHailNetworkServiceClient taxiHailNetworkService)
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

        public IChargeAccountWebServiceClient ChargeAccount(string companyKey)
        {
            return new ChargeAccountWebServiceClient(_serverSettings, GetSettingContainer(companyKey), _logger);
        }

        public IBSSettingContainer GetSettingContainer(string companyKey)
        {
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