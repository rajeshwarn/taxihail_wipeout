#region

using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Pair
{
    [Route("v1/trip/{Token}")]
    public class TripRequest : IReturn<Trip>
    {
        public string Token { get; set; }
    }
}