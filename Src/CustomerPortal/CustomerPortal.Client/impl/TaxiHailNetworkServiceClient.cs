using System.Collections.Generic;
using System.Net.Http;
using CustomerPortal.Contract.Requests;
using CustomerPortal.Contract.Resources;
using Newtonsoft.Json;

namespace CustomerPortal.Client.impl
{
    class TaxiHailNetworkServiceClient :  BaseServiceClient, ITaxiHailNetworkServiceClient
    {
        public TaxiHailNetworkServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }
        public List<CompanyPreference> GetOverlapingCompaniesPreferences(PostCompanyPreferencesRequest postCompanyPreferencesRequest)
        {

            var response = Client.Get<HttpResponseMessage>(postCompanyPreferencesRequest);
            var json = response.Content.ReadAsStringAsync().Result;
            var overlapingCompanies = JsonConvert.DeserializeObject<List<CompanyPreference>>(json);
            return overlapingCompanies;
        }

        public void SetOverlapingCompaniesPreferences(PostCompanyPreferencesRequest postCompanyPreferencesRequest)
        {

            Client.Post(postCompanyPreferencesRequest);
        }
    }
}
