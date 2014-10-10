using System.Collections.Generic;
using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Client
{
    public interface ITaxiHailNetworkServiceClient
    {
        List<CompanyPreference> GetOverlapingCompaniesPreferences(
            string companyId);

        void SetOverlapingCompaniesPreferences(string companyId,CompanyPreference[] companyPreferences);

    }
}
