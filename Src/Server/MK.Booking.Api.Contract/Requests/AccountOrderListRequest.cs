using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/{AccountId}/orders", "GET")]
    public class AccountOrderListRequest
    {
        public Guid AccountId { get; set; }
    }
}
