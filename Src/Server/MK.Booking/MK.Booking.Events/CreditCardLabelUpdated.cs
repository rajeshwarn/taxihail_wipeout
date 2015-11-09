using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardLabelUpdated : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
        public string Label { get; set; }
    }
}
