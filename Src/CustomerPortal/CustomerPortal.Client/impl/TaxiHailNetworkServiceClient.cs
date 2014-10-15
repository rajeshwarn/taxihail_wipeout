using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using CustomerPortal.Contract.Resources;
using Newtonsoft.Json;

namespace CustomerPortal.Client.Impl
{
    public class TaxiHailNetworkServiceClient : BaseServiceClient, ITaxiHailNetworkServiceClient
    {

        public async Task<List<CompanyPreference>> GetNetworkCompanyPreferences(string companyId)
        {
            var response = await Client.GetAsync(@"customer/"+companyId+"/network");
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
               return JsonConvert.DeserializeObject<List<CompanyPreference>>(json);
            }
            return new List<CompanyPreference>();
        }


        public Task SetNetworkCompanyPreferences(string companyId, CompanyPreference[] preferences)
        {
            var content = new ObjectContent<CompanyPreference[]>(preferences, new JsonMediaTypeFormatter());
            
            return Client.PostAsync(@"customer/" + companyId + "/network", content);
        }
    }
}
