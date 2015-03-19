using System;
using System.Collections.Generic;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class UserAddedToPromotionWhiteList : VersionedEvent
    {
        public IEnumerable<Guid> AccountIds { get; set; }

        public double? LastTriggeredAmount { get; set; }
    }
}
