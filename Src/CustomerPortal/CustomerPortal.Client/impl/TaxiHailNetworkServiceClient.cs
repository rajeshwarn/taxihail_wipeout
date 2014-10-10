using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace CustomerPortal.Client.impl
{
    public class TaxiHailNetworkServiceClient :  BaseServiceClient, ITaxiHailNetworkServiceClient
    {
        public TaxiHailNetworkServiceClient()
            : base()
        public List<CompanyPreference> GetOverlapingCompaniesPreferences(string companyId)
            var response = Client.GetAsync(@"customer/"+companyId+"/Network/").Result;
        public void SetOverlapingCompaniesPreferences(string companyId, CompanyPreference[] preferences)
            var content = new ObjectContent<CompanyPreference[]>(preferences, new JsonMediaTypeFormatter());
            Client.PostAsync(@"customer/" + companyId + "/Network/", content);
    }
}
