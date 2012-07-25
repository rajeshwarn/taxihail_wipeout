using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/accounts/{AccountId}/orders/{OrderId}/cancel", "POST")]
    public class CancelOrder : BaseDTO
    {

        public Guid AccountId { get; set; }
        public Guid OrderId { get; set; }
    }
}
