using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class UserRemovedFromPromotionWhiteList : VersionedEvent
    {
        public Guid AccountId { get; set; }
    }
}
