﻿#region

using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/cancel", "POST")]
    public class CancelOrder : BaseDto
    {
        public Guid OrderId { get; set; }
    }
}