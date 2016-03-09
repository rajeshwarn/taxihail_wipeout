using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/privacypolicy", "GET, POST")]
    public class PrivacyPolicyRequest
    {
        public string Policy { get; set; }
    }
}