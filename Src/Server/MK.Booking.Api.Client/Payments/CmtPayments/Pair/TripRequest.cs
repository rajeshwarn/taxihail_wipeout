using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Pair
{
    [Route("v1/trip/{Token}")]
    public class TripRequest : IReturn<Trip>
    {
        public string Token { get; set; }
    }
}