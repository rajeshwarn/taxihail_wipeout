using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;

namespace CustomerPortal.Client
{
    public interface ITaxiHailNetworkServiceClient
    {
        Task<List<CompanyPreferenceResponse>> GetNetworkCompanyPreferences(string companyId);

        Task SetNetworkCompanyPreferences(string companyId, CompanyPreference[] companyPreferences);

        Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId, double? latitude = null, double? longitude = null);

        List<NetworkFleetResponse> GetNetworkFleet(string companyId, double? latitude = null, double? longitude = null);

        string GetCompanyMarket(double latitude, double longitude);

        IEnumerable<NetworkFleetResponse> GetMarketFleets(string market, double latitude, double longitude);
    }
}
