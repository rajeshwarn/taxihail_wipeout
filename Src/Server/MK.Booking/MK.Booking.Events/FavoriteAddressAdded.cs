using System;
using Infrastructure.EventSourcing;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Events
{
    public class FavoriteAddressAdded : VersionedEvent
    {
        public Address Address { get; set; }
    }
}
