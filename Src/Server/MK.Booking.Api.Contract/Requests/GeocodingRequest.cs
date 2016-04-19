#region

using apcurium.MK.Booking.MapDataProvider.Google.Resources;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/geocode", "POST")]
    public class GeocodingRequest : BaseDto
    {
        public string Name { get; set; }

        public double? Lat { get; set; }

        public double? Lng { get; set; }

        public GeoResult GeoResult { get; set; }
    }
}