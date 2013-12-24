#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/places", "GET")]
    public class NearbyPlacesRequest : BaseDto
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