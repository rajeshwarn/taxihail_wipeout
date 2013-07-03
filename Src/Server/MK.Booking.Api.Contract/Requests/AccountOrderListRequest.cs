using System.Collections.Generic;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/orders", "GET")]
    public class AccountOrderListRequest { }

    [NoCache]
    public class AccountOrderListRequestResponse: List<Order>
    {
        
    }
}
