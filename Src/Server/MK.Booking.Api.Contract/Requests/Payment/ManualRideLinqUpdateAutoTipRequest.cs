using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/accounts/manualridelinq/{OrderId}/pairing/tip", "PUT")]
    public class ManualRideLinqUpdateAutoTipRequest : BaseDto
    {
        public Guid OrderId { get; set; }

        public int AutoTipPercentage { get; set; }
    }
}
