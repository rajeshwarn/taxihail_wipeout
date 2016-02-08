#region

using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;
using System;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class OrderRated : VersionedEvent
    {
		public Guid? AccountId { get; set; }
        public string Note { get; set; }
        public List<RatingScore> RatingScores { get; set; }
    }
}