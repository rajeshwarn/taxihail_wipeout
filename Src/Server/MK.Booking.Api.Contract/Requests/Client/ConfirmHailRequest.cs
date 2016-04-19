using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests.Client
{
    [RouteDescription("/client/hail/confirm", "POST")]
    public class ConfirmHailRequest
    {
        public OrderKey OrderKey { get; set; }

        public VehicleCandidate VehicleCandidate { get; set; }
    }
}
