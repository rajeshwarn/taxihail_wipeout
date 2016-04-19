#region

using System;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/orders/{OrderId}", "GET,DELETE")]
    public class OrderRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}