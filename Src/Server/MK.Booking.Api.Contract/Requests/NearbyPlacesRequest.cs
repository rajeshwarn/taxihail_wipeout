

using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/places", "GET")]
    public class NearbyPlacesRequest : BaseDto
    {
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string Name { get; set; }

        public bool IsLocationEmpty()
        {
            return Lat == null && Lng == null;
        }
    }
}