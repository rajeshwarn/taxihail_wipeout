using ServiceStack.ServiceHost;

namespace CMTPayment.Pair
{
    [Route("v1/trip/{Token}")]
    public class TripRequest : IReturn<Trip>
    {
        public string Token { get; set; }
    }
}