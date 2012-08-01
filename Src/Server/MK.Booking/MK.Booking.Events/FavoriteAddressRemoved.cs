using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class FavoriteAddressRemoved : VersionedEvent
    {
        public Guid AddressId { get; set; }
    }
}
