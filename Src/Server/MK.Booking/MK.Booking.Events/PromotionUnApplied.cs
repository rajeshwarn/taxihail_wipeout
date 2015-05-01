using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PromotionUnApplied : VersionedEvent
    {
        public Guid OrderId { get; set; }

        public Guid AccountId { get; set; }
    }
}
