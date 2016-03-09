using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Client
{
    [Route("/client/hail/confirm", "POST")]
    public class ConfirmHailRequest
    {
        public OrderKey OrderKey { get; set; }

        public VehicleCandidate VehicleCandidate { get; set; }
    }
}
