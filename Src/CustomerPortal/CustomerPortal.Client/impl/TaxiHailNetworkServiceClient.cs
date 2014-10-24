using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using Newtonsoft.Json;
using apcurium.MK.Common.Configuration;

namespace CustomerPortal.Client.Impl
{
    public class TaxiHailNetworkServiceClient : BaseServiceClient, ITaxiHailNetworkServiceClient
    {
        public TaxiHailNetworkServiceClient(IServerSettings serverSettings):base(serverSettings) 
        { 

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
        public async Task<List<NetworkFleetResponse>> GetNetworkFleet(string companyId,MapCoordinate coordinate)
        {
            var response = await Client.PostAsJsonAsync(@"customer/" + companyId + "/networkfleet",coordinate);
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
