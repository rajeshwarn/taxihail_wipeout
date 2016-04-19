using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/accounts/orders/{OrderId}/pairing/tip", "POST")]
    public class UpdateAutoTipRequest : BaseDto
    {
        public Guid OrderId { get; set; }

        public int AutoTipPercentage { get; set; }
    }
}
