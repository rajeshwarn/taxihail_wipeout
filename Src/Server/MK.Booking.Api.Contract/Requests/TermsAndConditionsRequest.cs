using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/termsandconditions", "GET, POST")]
    public class TermsAndConditionsRequest : IReturn<TermsAndConditions>
    {
        public string TermsAndConditions { get; set; }
    }
}