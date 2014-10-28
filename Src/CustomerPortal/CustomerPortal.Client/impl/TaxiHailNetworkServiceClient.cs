﻿using System.Collections.Generic;
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
        public List<NetworkFleetResponse> GetNetworkFleet(string companyId, MapCoordinate coordinate = null)
        {

            var response = Client.GetAsync(string.Format(@"customer/{0}/networkfleet?Latitude={1}&Longitude={2}",companyId,coordinate.Latitude, coordinate.Longitude)).Result;
            var json =  response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json);
            }
            return new List<NetworkFleetResponse>();
        }
        public async Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId,MapCoordinate coordinate=null)
        {
            var response = await Client.GetAsync(string.Format(@"customer/{0}/networkfleet?Latitude={1}&Longitude={2}", companyId, coordinate.Latitude, coordinate.Longitude));
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
