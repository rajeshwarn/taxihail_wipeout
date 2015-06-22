using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/calldriver/", "GET")]
    public class InitiateCallToDriverRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}