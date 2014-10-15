using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Client
{
    public interface ITaxiHailNetworkServiceClient
    {
        Task<List<CompanyPreference>> GetNetworkCompanyPreferences(string companyId);
        Task SetNetworkCompanyPreferences(string companyId,CompanyPreference[] companyPreferences);

    }
}
