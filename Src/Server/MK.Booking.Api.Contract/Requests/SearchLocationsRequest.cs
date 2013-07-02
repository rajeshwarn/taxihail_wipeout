using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Google.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/searchlocation", "POST")]
    public class SearchLocationsRequest : BaseDTO
    {
        public string Name { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public GeoResult GeoResult { get; set; }
    }
}
