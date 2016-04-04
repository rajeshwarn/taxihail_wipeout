﻿#region

using System;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/accounts/orders/{OrderId}", "GET,DELETE")]
    public class OrderRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}