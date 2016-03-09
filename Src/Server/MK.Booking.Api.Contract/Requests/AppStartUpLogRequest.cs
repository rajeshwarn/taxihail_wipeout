using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/app/starts/{LastMinutes}")]
    public class AppStartUpLogRequest
    {
        public long LastMinutes { get; set; }
    }
}