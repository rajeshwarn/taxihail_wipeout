using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using Newtonsoft.Json;
using apcurium.MK.Common.Configuration;
using System.Net.Http.Headers;

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
            var response = await Client.GetAsync(@"customer/"+companyId+"/network");
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json);
            }
            return new List<CompanyPreferenceResponse>();
        }
        public List<NetworkFleetResponse> GetNetworkFleet(string companyId, MapCoordinate coordinate = null)
        {
            var companyKey = companyId ?? _serverSettings.ServerData.TaxiHail.ApplicationKey;

            HttpContent content = new ObjectContent<MapCoordinate>(coordinate, new JsonMediaTypeFormatter());

            HttpResponseMessage response = Client.PostAsync(string.Format(@"customer/{0}/networkfleet", companyKey), content).Result;

            var json =  response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json);
            }
            return new List<NetworkFleetResponse>();
        }
        public async Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId,MapCoordinate coordinate=null)
        {
            var companyKey = companyId ?? _serverSettings.ServerData.TaxiHail.ApplicationKey;

            HttpContent content = new ObjectContent<MapCoordinate>(coordinate, new JsonMediaTypeFormatter());

            HttpResponseMessage response = await Client.PostAsync(string.Format(@"customer/{0}/networkfleet", companyKey), content);

            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json);
            }
            return new List<NetworkFleetResponse>();
        }


        public Task SetNetworkCompanyPreferences(string companyId, CompanyPreference[] preferences)
        {
            return Client.PostAsJsonAsync(@"customer/" + companyId + "/network", preferences);
        }
    }
}
