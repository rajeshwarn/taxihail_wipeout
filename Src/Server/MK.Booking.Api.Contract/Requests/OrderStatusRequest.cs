using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/orders/{OrderId}/status/", "GET")]
    public class OrderStatusRequest : BaseDTO
    {
        public Guid OrderId { get; set; }
    }
}