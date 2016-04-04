using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/accounts/orders/{OrderId}/ignoreDispatchCompanySwitch", "POST")]
    public class IgnoreDispatchCompanySwitchRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}
