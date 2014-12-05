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

        public IAccountWebServiceClient Account(string companyKey, string market)
        {
            return new AccountWebServiceClient(_serverSettings, GetSettingContainer(companyKey, market), _logger);
        }

        public IStaticDataWebServiceClient StaticData(string companyKey, string market)
        {
            return new StaticDataWebServiceClient(GetSettingContainer(companyKey, market), _logger);
        }

        public IBookingWebServiceClient Booking(string companyKey, string market)
        {
            return new BookingWebServiceClient(_serverSettings, GetSettingContainer(companyKey, market), _logger);
        }

        public IChargeAccountWebServiceClient ChargeAccount(string companyKey, string market)
        {
            return new ChargeAccountWebServiceClient(_serverSettings, GetSettingContainer(companyKey, market), _logger);
        }

        public IBSSettingContainer GetSettingContainer(string companyKey, string market)
        {
            if (!companyKey.HasValue())
            {
                // In home market
                return _serverSettings.ServerData.IBS;
            }

            // In local market but switched companies
            if (!_ibsSettings.ContainsKey(companyKey) && !market.HasValue())
            {
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
                        WebServicesUserName = networkFleetResponse.IbsUserName
                    };
                    _ibsSettings.Add(networkFleetResponse.CompanyKey, settingContainer);
                }
            }

            // In external market
            if (!_ibsSettings.ContainsKey(companyKey) && market.HasValue())
            {
                var marketFleets = _taxiHailNetworkService.GetMarketFleets(companyKey, market);

                _ibsSettings.Clear();

                foreach (var networkFleetResponse in marketFleets)
                {
                    var settingContainer = new IBSSettingContainer
                    {
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