using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/account/manualridelinq/{OrderId}/pairing/tip", "PUT")]
    public class ManualRideLinqUpdateAutoTipRequest : BaseDto
    {
        public Guid OrderId { get; set; }

        public int AutoTipPercentage { get; set; }
    }
}
