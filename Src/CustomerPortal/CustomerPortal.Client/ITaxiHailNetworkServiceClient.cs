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
        Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId, MapCoordinate coordinate = null);
        List<NetworkFleetResponse> GetNetworkFleet(string companyId, MapCoordinate coordinate = null);

    }
}
