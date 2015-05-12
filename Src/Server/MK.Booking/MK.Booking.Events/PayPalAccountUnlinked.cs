using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PayPalAccountUnlinked : VersionedEvent
    {
        public Guid AccountId { get; set; }
    }
}
