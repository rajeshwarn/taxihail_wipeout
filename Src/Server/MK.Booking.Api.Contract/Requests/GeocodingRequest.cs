#region


using apcurium.MK.Booking.MapDataProvider.Resources;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;

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