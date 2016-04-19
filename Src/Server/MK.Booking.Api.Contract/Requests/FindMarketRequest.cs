using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/roaming/market", "GET")]
    public class FindMarketRequest : BaseDto
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}