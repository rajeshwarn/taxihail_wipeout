using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace CMTPayment.Pair
{
    [RouteDescription("trip/{Token}")]
    public class TripRequest : IReturn<Trip>
    {
        public string Token { get; set; }
    }
}