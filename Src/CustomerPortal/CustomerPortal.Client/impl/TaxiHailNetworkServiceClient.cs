using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using CustomerPortal.Contract.Resources;
using Newtonsoft.Json;

namespace CustomerPortal.Client.impl
{
    public class TaxiHailNetworkServiceClient : BaseServiceClient, ITaxiHailNetworkServiceClient
    {
        public TaxiHailNetworkServiceClient() : base()
        {
            
        }

        public List<CompanyPreference> GetOverlapingCompaniesPreferences(string companyId)
        {
             var response = Client.GetAsync(@"customer/"+companyId+"/network").Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var overlapingCompanies=new List<CompanyPreference>();
            if (response.IsSuccessStatusCode)
            {
                overlapingCompanies = JsonConvert.DeserializeObject<List<CompanyPreference>>(json);
            }
            return overlapingCompanies;
        }


        public void SetOverlapingCompaniesPreferences(string companyId, CompanyPreference[] preferences)
        {
            var content = new ObjectContent<CompanyPreference[]>(preferences, new JsonMediaTypeFormatter());
            Client.PostAsync(@"customer/" + companyId + "/network", content);
        }
    }
}
