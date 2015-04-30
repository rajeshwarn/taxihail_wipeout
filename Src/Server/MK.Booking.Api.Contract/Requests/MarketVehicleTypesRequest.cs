using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/roaming/externalMarketVehicleTypes", "GET")]
    public class MarketVehicleTypesRequest
    {
        public double Longitude { get; set; }

        public double Latitude { get; set; }
    }
}
