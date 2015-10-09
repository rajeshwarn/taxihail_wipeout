using Infrastructure.EventSourcing;
using System;

namespace apcurium.MK.Booking.Events
{
    public class DefaultCreditCardUpdated : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
    }
}
