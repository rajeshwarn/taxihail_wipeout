using System;
using System.Collections.Generic;
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

        public TaxiHailNetworkServiceClient(IServerSettings serverSettings) : base(serverSettings)
        {
            _serverSettings = serverSettings;
        }

        public async Task<List<CompanyPreferenceResponse>> GetNetworkCompanyPreferences(string companyId)
        {
            return await Client.Get(string.Format(@"customer/{0}/network", companyId))
                               .Deserialize<List<CompanyPreferenceResponse>>();
        }

        public Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId, double? latitude = null, double? longitude = null)
        {
            var companyKey = companyId ?? _serverSettings.ServerData.TaxiHail.ApplicationKey;

            var @params = new Dictionary<string, string>
                {
                    { "latitude", latitude.ToString() },
                    { "longitude", longitude.ToString() }
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
            var homeCompanyKey = _serverSettings.ServerData.TaxiHail.ApplicationKey;

            var @params = new Dictionary<string, string>
                {
                    { "companyId", homeCompanyKey },
                    { "latitude", latitude.ToString() },
                    { "longitude", longitude.ToString() }
                };

            var queryString = BuildQueryString(@params);

            return Client.Get("customer/roaming/market" + queryString)
                        .Deserialize<string>()
                        .Result;
        }

        public IEnumerable<NetworkFleetResponse> GetMarketFleets(string market, double latitude, double longitude)
        {
            var @params = new Dictionary<string, string>
                {
                    { "market", market },
                    { "latitude", latitude.ToString() },
                    { "longitude", longitude.ToString() }
                };

            var queryString = BuildQueryString(@params);

            return Client.Get("customer/roaming/marketfleets" + queryString)
                         .Deserialize<IEnumerable<NetworkFleetResponse>>()
                         .Result;
        }
    }
}
