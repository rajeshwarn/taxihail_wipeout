using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/accounts/{AccountId}/orders/{OrderId}/status/", "GET")]
    public class OrderStatusRequest : BaseDTO
    {
        public Guid AccountId { get; set; }
        public Guid OrderId { get; set; }



    }
}