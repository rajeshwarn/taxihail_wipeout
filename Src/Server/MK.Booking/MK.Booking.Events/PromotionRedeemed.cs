using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PromotionRedeemed : VersionedEvent
    {
        public Guid OrderId { get; set; }

        public decimal AmountSaved { get; set; }
    }
}