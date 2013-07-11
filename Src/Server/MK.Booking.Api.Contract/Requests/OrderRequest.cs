using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}", "GET,DELETE")]
    public class OrderRequest : BaseDTO
    {
        public Guid OrderId { get; set; }
    }
}
