using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Client
{
    [Authenticate]
    [Route("/client/hail/confirm", "POST")]
    public class ConfirmHailRequest
    {
        public OrderKey OrderKey { get; set; }

        public VehicleCandidate VehicleCandidate { get; set; }
    }
}
