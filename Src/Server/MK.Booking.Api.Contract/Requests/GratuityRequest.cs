#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/gratuity", "POST")]
    [Route("/gratuity/{OrderId}", "GET")]
    public class GratuityRequest : BaseDto
    {
        public Guid OrderId { get; set; }
        public int Percentage { get; set; }
    }
}