#region

using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/orders", "GET")]
    public class AccountOrderListRequest
    {
    }

    public class AccountOrderListRequestResponse : List<Order>
    {
    }
}