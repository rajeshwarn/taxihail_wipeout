using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AddressRemoved : VersionedEvent
    {
        public Guid AddressId { get; set; }
    }
}
