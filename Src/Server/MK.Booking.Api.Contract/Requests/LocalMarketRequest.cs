using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/roaming/localmarket", "GET")]
    public class LocalMarketRequest : BaseDto
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
