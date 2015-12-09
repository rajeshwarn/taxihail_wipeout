using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/updateintrip", "POST")]
    public class OrderUpdateRequest : BaseDto
    {
        public OrderUpdateRequest()
        {
            DropOffAddress = new Address();
        }
        public Guid OrderId { get; set; }
        public Address DropOffAddress { get; set; }
    }
}
