using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class OrderRatings : BaseDTO
    {
        public Guid OrderId { get; set; }
        public string Note { get; set; }
        public List<RatingScore> RatingScores { get; set; }
    }
}