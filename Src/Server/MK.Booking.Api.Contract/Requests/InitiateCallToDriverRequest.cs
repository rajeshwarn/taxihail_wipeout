using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/orders/{OrderId}/calldriver/", "GET")]
    public class InitiateCallToDriverRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}