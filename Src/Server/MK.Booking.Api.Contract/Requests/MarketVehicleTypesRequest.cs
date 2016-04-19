using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/roaming/externalMarketVehicleTypes", "GET")]
    public class MarketVehicleTypesRequest
    {
        public double Longitude { get; set; }

        public double Latitude { get; set; }
    }
}
