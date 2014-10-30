using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using Newtonsoft.Json;
using apcurium.MK.Common.Configuration;

namespace CustomerPortal.Client.Impl
{
    public class TaxiHailNetworkServiceClient : BaseServiceClient, ITaxiHailNetworkServiceClient
    {
        private readonly IServerSettings _serverSettings;

        public TaxiHailNetworkServiceClient(IServerSettings serverSettings):base(serverSettings)
        {
            _serverSettings = serverSettings;
        }

        public async Task<List<CompanyPreferenceResponse>> GetNetworkCompanyPreferences(string companyId)
        {
            var response = await Client.GetAsync(string.Format(@"customer/{0}/network", companyId));
            var json = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode 
                ? JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json) 
                : new List<CompanyPreferenceResponse>();
        }

        public List<NetworkFleetResponse> GetNetworkFleet(string companyId, MapCoordinate coordinate = null)
        {
            var companyKey = companyId ?? _serverSettings.ServerData.TaxiHail.ApplicationKey;

            var response = Client.PostAsJsonAsync(string.Format(@"customer/{0}/networkfleet", companyKey), coordinate).Result;
            var json =  response.Content.ReadAsStringAsync().Result;

            return response.IsSuccessStatusCode 
                ? JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json) 
                : new List<NetworkFleetResponse>();
        }

        public async Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId, MapCoordinate coordinate=null)
        {
            var companyKey = companyId ?? _serverSettings.ServerData.TaxiHail.ApplicationKey;

            var response = await Client.PostAsJsonAsync(string.Format(@"customer/{0}/networkfleet", companyKey), coordinate);
            var json = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode 
                ? JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json) 
                : new List<NetworkFleetResponse>();
        }
        
        public Task SetNetworkCompanyPreferences(string companyId, CompanyPreference[] preferences)
        {
            return Client.PostAsJsonAsync(string.Format(@"customer/{0}/network", companyId), preferences);
        }
    }
}
