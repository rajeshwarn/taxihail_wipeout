#region

using apcurium.MK.Booking.Google.Resources;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/geocode", "POST")]
    public class GeocodingRequest : BaseDto
    {
        public string Name { get; set; }

        public double? Lat { get; set; }

        public double? Lng { get; set; }

        public GeoResult GeoResult { get; set; }
    }
}