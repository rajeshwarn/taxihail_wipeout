using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/roaming/market", "GET")]
    public class FindMarketRequest : BaseDto
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
