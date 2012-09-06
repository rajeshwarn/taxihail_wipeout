using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/orders/{OrderId}", "GET,DELETE")]
    public class OrderRequest : BaseDTO
    {
        public Guid OrderId { get; set; }
    }
}
