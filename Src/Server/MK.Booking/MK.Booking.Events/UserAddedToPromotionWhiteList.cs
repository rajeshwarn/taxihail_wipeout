using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    [Obsolete("Replaced by UserAddedToPromotionWhiteList_V2", false)]
    public class UserAddedToPromotionWhiteList : VersionedEvent
    {
        [Obsolete("This field is obsolete. Use AccountIds from UserAddedToPromotionWhiteList_V2 instead")]
        public Guid AccountId { get; set; }

        public double? LastTriggeredAmount { get; set; }
    }
}
