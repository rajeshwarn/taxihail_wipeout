using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Web.Areas.Customer.Models.RequestResponse
{
    public class CompanyPreferencesRequest
    {
        public string CompanyId { get; set; }

        public CompanyPreference[] Preferences { get; set; }
    }
}