using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/orders/{OrderId}/ignoreDispatchCompanySwitch", "POST")]
    public class IgnoreDispatchCompanySwitchRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}
