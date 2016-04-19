using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/privacypolicy", "GET, POST")]
    public class PrivacyPolicyRequest
    {
        public string Policy { get; set; }
    }
}