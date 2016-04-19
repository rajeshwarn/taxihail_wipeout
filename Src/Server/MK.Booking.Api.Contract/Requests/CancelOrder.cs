#region

using System;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/orders/{OrderId}/cancel", "POST")]
    public class CancelOrder : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}