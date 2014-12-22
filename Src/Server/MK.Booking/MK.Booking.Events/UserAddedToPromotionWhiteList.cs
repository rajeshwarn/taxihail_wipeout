using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class UserAddedToPromotionWhiteList : VersionedEvent
    {
        public Guid AccountId { get; set; }
        public double? LastTriggeredAmount { get; set; }
    }
}
