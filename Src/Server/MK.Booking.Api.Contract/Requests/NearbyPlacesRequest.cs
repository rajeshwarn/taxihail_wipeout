using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/places", "GET, OPTIONS")]
    public class NearbyPlacesRequest : BaseDTO
    {
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public int? Radius { get; set; }
        public string Name { get; set; }

        public bool IsLocationEmpty()
        {
            return Lat == null && Lng == null;
        }
    }
}
