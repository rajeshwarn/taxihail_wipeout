using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/account/orders/{OrderId}/calldriver/", "GET")]
    public class InitiateCallToDriverRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}