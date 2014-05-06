#region


using apcurium.MK.Booking.MapDataProvider.Resources;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/searchlocation", "POST")]
    public class SearchLocationsRequest : BaseDto
    {
        public string Name { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public GeoResult GeoResult { get; set; }
    }
}