using System;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/orders/{OrderId}/updateintrip", "POST")]
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
