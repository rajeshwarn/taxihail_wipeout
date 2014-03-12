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
    [Route("/ratings", "POST")]
    [Route("/ratings/{OrderId}", "GET")]
    public class OrderRatingsRequest : BaseDto
    {
        public Guid OrderId { get; set; }
        public string Note { get; set; }
        public List<RatingScore> RatingScores { get; set; }
    }
}