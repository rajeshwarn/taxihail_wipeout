using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/accounts/orders/{OrderId}/pairing/tip", "POST")]
    public class UpdateAutoTipRequest : BaseDto
    {
        public Guid OrderId { get; set; }

        public int AutoTipPercentage { get; set; }
    }
}
