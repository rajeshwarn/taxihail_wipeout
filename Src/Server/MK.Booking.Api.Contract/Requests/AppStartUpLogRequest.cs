using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/app/starts/{LastMinutes}")]
    public class AppStartUpLogRequest
    {
        public long LastMinutes { get; set; }
    }
}