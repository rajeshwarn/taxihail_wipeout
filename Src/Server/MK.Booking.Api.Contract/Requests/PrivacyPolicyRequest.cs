using ServiceStack.ServiceInterface;
#if !CLIENT
using apcurium.MK.Booking.Security;
using apcurium.MK.Booking.Api.Contract.Security;
#endif
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
#if !CLIENT
    [Authenticate(ApplyTo.Post)]
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
#endif
    [Route("/privacypolicy", "GET, POST")]
    public class PrivacyPolicyRequest
    {
        public string Policy { get; set; }
    }
}