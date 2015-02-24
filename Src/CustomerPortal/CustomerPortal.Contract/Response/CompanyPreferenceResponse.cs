using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Contract.Response
{
    public class CompanyPreferenceResponse
    {
        public CompanyPreference CompanyPreference { get; set; }

        public bool CanDispatchTo { get; set; }

        public int FleetId { get; set; }
    }
}
