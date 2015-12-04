using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using apcurium.MK.Common.Configuration;
using CustomerPortal.Client.Http.Extensions;

namespace CustomerPortal.Client.Impl
{
    public class TaxiHailNetworkServiceClient : BaseServiceClient, ITaxiHailNetworkServiceClient
    {
        private readonly IServerSettings _serverSettings;

        public TaxiHailNetworkServiceClient(IServerSettings serverSettings)
            : base(serverSettings)
        {
            _serverSettings = serverSettings;
        }

        public async Task<List<CompanyPreferenceResponse>> GetNetworkCompanyPreferences(string companyId)
        {
            return await Client.Get(string.Format(@"customer/{0}/network", companyId))
                               .Deserialize<List<CompanyPreferenceResponse>>();
        }

        public async Task<Dictionary<string, List<CompanyPreferenceResponse>>> GetRoamingCompanyPreferences(string companyId)
        {
            return await Client.Get(string.Format(@"customer/{0}/roaming/networkfleets", companyId))
                               .Deserialize<Dictionary<string, List<CompanyPreferenceResponse>>>();
        }

        public Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId, double? latitude = null, double? longitude = null)
        {
            var companyKey = companyId ?? _serverSettings.ServerData.TaxiHail.ApplicationKey;

            var @params = new Dictionary<string, string>
                {
                    { "latitude", latitude.HasValue ? latitude.Value.ToString(CultureInfo.InvariantCulture) : null},
                    { "longitude", longitude.HasValue ? longitude.Value.ToString(CultureInfo.InvariantCulture) : null }
                };

            var queryString = BuildQueryString(@params);

            return Client.Get(string.Format("customer/{0}/networkfleet/", companyKey) + queryString)
                         .Deserialize<List<NetworkFleetResponse>>();
        }

        public List<NetworkFleetResponse> GetNetworkFleet(string companyId, double? latitude = null, double? longitude = null)
        {
            return GetNetworkFleetAsync(companyId, latitude, longitude).Result;
        }

        public Task SetNetworkCompanyPreferences(string companyId, CompanyPreference[] preferences)
        {
            return Client.Post(string.Format(@"customer/{0}/network", companyId), preferences);
        }

        public string GetCompanyMarket(double latitude, double longitude)
        {
            if (!_serverSettings.ServerData.Network.Enabled)
            {
                return null;
            }

            var homeCompanyKey = _serverSettings.ServerData.TaxiHail.ApplicationKey;

            var @params = new Dictionary<string, string>
            {
                { "companyId", homeCompanyKey },
                { "latitude", latitude.ToString(CultureInfo.InvariantCulture) },
                { "longitude", longitude.ToString(CultureInfo.InvariantCulture) }
            };

            var queryString = BuildQueryString(@params);

            return Client.Get("customer/roaming/market" + queryString)
                         .Deserialize<string>()
                         .Result;
        }

        public CompanyMarketSettingsResponse GetCompanyMarketSettings(double latitude, double longitude)
        {
            var homeCompanyKey = _serverSettings.ServerData.TaxiHail.ApplicationKey;

            var @params = new Dictionary<string, string>
            {
                { "companyId", homeCompanyKey },
                { "latitude", latitude.ToString(CultureInfo.InvariantCulture) },
                { "longitude", longitude.ToString(CultureInfo.InvariantCulture) }
            };

            var queryString = BuildQueryString(@params);

            return Client.Get("customer/roaming/marketsettings" + queryString)
                         .Deserialize<CompanyMarketSettingsResponse>()
                         .Result;
        }

        public IEnumerable<NetworkFleetResponse> GetMarketFleets(string companyId, string market)
        {
            var companyKey = companyId ?? _serverSettings.ServerData.TaxiHail.ApplicationKey;

            return Client.Get(string.Format("customer/{0}/roaming/marketfleets?market={1}", companyKey, market))
                         .Deserialize<IEnumerable<NetworkFleetResponse>>()
                         .Result;
        }

        public NetworkFleetResponse GetMarketFleet(string market, int fleetId)
        {
            return Client.Get(string.Format("customer/roaming/marketfleet?market={0}&fleetId={1}", market, fleetId))
                         .Deserialize<NetworkFleetResponse>()
                         .Result;
        }

        public IEnumerable<NetworkVehicleResponse> GetMarketVehicleTypes(string companyId = null, string market = null)
        {
            if (companyId == null && market == null)
            {
                throw new ArgumentNullException("You must specify at least either the Market or the CompanyId.");
            } 

           var @params = new Dictionary<string, string>
                {
                    { "companyId", companyId },
                    { "market", market }
                };

            return Client.Get("customer/marketVehicleTypes" + BuildQueryString(@params))
                         .Deserialize<IEnumerable<NetworkVehicleResponse>>()
                         .Result;
        }

        public NetworkVehicleResponse GetAssociatedMarketVehicleType(string companyId, int networkVehicleId)
        {
            return Client.Get(string.Format("customer/{0}/associatedMarketVehicleType?networkVehicleId={1}", companyId, networkVehicleId))
                         .Deserialize<NetworkVehicleResponse>()
                         .Result;
        }

        public Task UpdateMarketVehicleType(string companyId, CompanyVehicleType vehicleType)
        {
            return Client.Post(string.Format("customer/{0}/companyVehicles", companyId), vehicleType);
        }

        public Task DeleteMarketVehicleMapping(string companyId, Guid id)
        {
            return Client.DeleteAsync(string.Format("customer/{0}/companyVehicles?vehicleTypeId={1}", companyId, id));
        }

        public CompanyPaymentSettings GetPaymentSettings(string companyId)
        {
            return Client.Get(string.Format("customer/{0}/paymentSettings", companyId))
                         .Deserialize<CompanyPaymentSettings>()
                         .Result;
        }

        public Task UpdatePaymentSettings(string companyId, CompanyPaymentSettings paymentSettings)
        {
            return Client.Post(string.Format("customer/{0}/paymentSettings", companyId), paymentSettings);
        }
    }
}
