#region

using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/places/detail", "GET")]
    public class PlaceDetailRequest : IReturn<Address>
    {
        public string ReferenceId { get; set; }
        public string PlaceName { get; set; }
    }
}