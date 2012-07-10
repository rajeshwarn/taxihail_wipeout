using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/orderstatus/{OrderId}", "GET")]   
    public class OrderStatusRequest
    {
        public Guid OrderId { get; set; }
    }
}