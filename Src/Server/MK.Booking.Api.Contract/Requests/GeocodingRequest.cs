using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/geocode", "POST")]
    public class GeocodingRequest : BaseDTO
    {
        public string Name{ get; set; }

        public double? Lat{ get; set; }
        
        public double? Lng { get; set; }
    }
}
