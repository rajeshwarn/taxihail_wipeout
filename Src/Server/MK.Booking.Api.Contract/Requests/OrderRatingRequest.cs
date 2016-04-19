#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/ratings", "POST")]
    [RouteDescription("/ratings/{OrderId}", "GET")]
    public class OrderRatingsRequest : BaseDto
    {
        public Guid OrderId { get; set; }
        public string Note { get; set; }
        public List<RatingScore> RatingScores { get; set; }
    }
}