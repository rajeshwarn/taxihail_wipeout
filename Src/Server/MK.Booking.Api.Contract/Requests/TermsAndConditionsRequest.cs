using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
#if !CLIENT
    [Authenticate(ApplyTo.Post)]
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
#endif
    [Route("/termsandconditions", "GET, POST")]
    public class TermsAndConditionsRequest : IReturn<TermsAndConditionsResponse>
    {
        public string TermsAndConditions { get; set; }
    }

    public class TermsAndConditionsResponse
    {
        public string Content { get; set; }
    }
}