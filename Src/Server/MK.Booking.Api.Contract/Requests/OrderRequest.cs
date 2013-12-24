﻿#region

using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}", "GET,DELETE")]
    public class OrderRequest : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}