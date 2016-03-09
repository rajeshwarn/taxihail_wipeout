using System;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/orders/{OrderId}/updateintrip", "POST")]
    public class OrderUpdateRequest : IReturn<bool>
    {
        public OrderUpdateRequest()
        {
            DropOffAddress = new Address();
        }
        public Guid OrderId { get; set; }
        public Address DropOffAddress { get; set; }
    }
}
