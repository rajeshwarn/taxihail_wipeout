using System;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/ignoreDispatchCompanySwitch", "POST")]
    public class IgnoreDispatchCompanySwitchRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}
