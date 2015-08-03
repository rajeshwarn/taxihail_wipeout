using ServiceStack.ServiceHost;

namespace CMTPayment.Pair
{
    [Route("trip/{Token}")]
    public class TripRequest : IReturn<Trip>
    {
        public string Token { get; set; }
    }
}