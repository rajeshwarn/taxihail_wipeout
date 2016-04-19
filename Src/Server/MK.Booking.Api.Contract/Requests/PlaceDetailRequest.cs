#region

using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/places/detail", "GET")]
    public class PlaceDetailRequest : IReturn<Address>
    {
        public string PlaceId { get; set; }
        public string PlaceName { get; set; }
    }
}