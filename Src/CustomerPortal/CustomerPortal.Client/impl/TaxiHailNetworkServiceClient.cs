using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using apcurium.MK.Common.Diagnostic;
using CustomerPortal.Contract.Resources;
using Newtonsoft.Json;
using apcurium.MK.Common.Configuration;

namespace CustomerPortal.Client.Impl
{
    public class TaxiHailNetworkServiceClient : BaseServiceClient, ITaxiHailNetworkServiceClient
    {
        private readonly ILogger _logger;

        public TaxiHailNetworkServiceClient(IServerSettings serverSettings, ILogger logger)
            : base(serverSettings)
        {
            _logger = logger;
        }

        public async Task<List<CompanyPreference>> GetNetworkCompanyPreferences(string companyId)
        {
            var response = await Client.GetAsync(@"customer/"+companyId+"/network");
            var json = await response.Content.ReadAsStringAsync();
            
            _logger.LogMessage("Request send to TaxiHailNetwork: " + response.RequestMessage);

            if (response.IsSuccessStatusCode)
            {
               return JsonConvert.DeserializeObject<List<CompanyPreference>>(json);
            }

            _logger.LogMessage("Request to TaxiHailNetwork failed: " + response.RequestMessage);

            return new List<CompanyPreference>();
        }


        public Task SetNetworkCompanyPreferences(string companyId, CompanyPreference[] preferences)
        {
            var content = new ObjectContent<CompanyPreference[]>(preferences, new JsonMediaTypeFormatter());
            
            return Client.PostAsync(@"customer/" + companyId + "/network", content);
        }
    }
}
