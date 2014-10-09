using System.Web.Http;
using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Contract.Requests
{
    [Route("api/customer/{companyId}/network/")]
    public class PostCompanyPreferencesRequest
    {
        public string CompanyId { get; set; }

        public CompanyPreference[] Preferences { get; set; }
    }
}