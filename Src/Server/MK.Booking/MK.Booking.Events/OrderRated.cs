using System;
using System.Collections.Generic;
using Infrastructure.EventSourcing;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Events
{
    public class OrderRated : VersionedEvent
    {
        public string Note { get; set; }
        public List<RatingScore> RatingScores { get; set; }
    }
}