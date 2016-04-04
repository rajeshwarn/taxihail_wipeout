﻿#region

using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/accounts/orders", "GET")]
    public class AccountOrderListRequest
    {
    }

    [NoCache]
    public class AccountOrderListRequestResponse : List<Order>
    {
    }
}