using Infrastructure.EventSourcing;
using System;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardDeactivated : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
    }
}
