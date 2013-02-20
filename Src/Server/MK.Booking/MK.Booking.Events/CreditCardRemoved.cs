using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardRemoved : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
    }
}